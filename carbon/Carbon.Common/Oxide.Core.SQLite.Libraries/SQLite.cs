using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Threading;
using Carbon;
using Mono.Data.Sqlite;
using Oxide.Core.Database;
using Oxide.Core.Libraries;
using Oxide.Core.Plugins;

namespace Oxide.Core.SQLite.Libraries;

public class SQLite : Library, IDatabaseProvider
{
	public class SQLiteQuery
	{
		private SqliteCommand _cmd;

		private SqliteConnection _connection;

		public Action<List<Dictionary<string, object>>> Callback { get; internal set; }

		public Action<int> CallbackNonQuery { get; internal set; }

		public Sql Sql { get; internal set; }

		public Connection Connection { get; internal set; }

		public bool NonQuery { get; internal set; }

		private void Cleanup()
		{
			if (_cmd != null)
			{
				((Component)(object)_cmd).Dispose();
				_cmd = null;
			}
			_connection = null;
		}

		public void Handle()
		{
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Expected O, but got Unknown
			List<Dictionary<string, object>> list = null;
			int nonQueryResult = 0;
			long lastInsertRowId = 0L;
			try
			{
				if (Connection == null)
				{
					throw new Exception("Connection is null");
				}
				_connection = (SqliteConnection)Connection.Con;
				if (((DbConnection)(object)_connection).State == ConnectionState.Closed)
				{
					((DbConnection)(object)_connection).Open();
				}
				_cmd = _connection.CreateCommand();
				((DbCommand)(object)_cmd).CommandText = Sql.SQL;
				Sql.AddParams((IDbCommand)_cmd, Sql.Arguments, "@");
				if (NonQuery)
				{
					nonQueryResult = ((DbCommand)(object)_cmd).ExecuteNonQuery();
				}
				else
				{
					SqliteDataReader val = _cmd.ExecuteReader();
					try
					{
						list = new List<Dictionary<string, object>>();
						while (((DbDataReader)(object)val).Read())
						{
							Dictionary<string, object> dictionary = new Dictionary<string, object>();
							for (int i = 0; i < ((DbDataReader)(object)val).FieldCount; i++)
							{
								dictionary.Add(((DbDataReader)(object)val).GetName(i), ((DbDataReader)(object)val).GetValue(i));
							}
							list.Add(dictionary);
						}
					}
					finally
					{
						((IDisposable)val)?.Dispose();
					}
				}
				lastInsertRowId = GetLastInsertRowId(_connection);
				Cleanup();
			}
			catch (Exception ex)
			{
				string text = "Sqlite handle raised an exception";
				if (Connection?.Plugin != null)
				{
					text += $" in '{Connection.Plugin.Name} v{Connection.Plugin.Version}' plugin";
				}
				Logger.Error(text, ex);
				Cleanup();
			}
			Interface.Oxide.NextTick(delegate
			{
				Connection?.Plugin?.TrackStart();
				try
				{
					if (Connection != null)
					{
						Connection.LastInsertRowId = lastInsertRowId;
					}
					if (!NonQuery)
					{
						Callback(list);
					}
					else
					{
						CallbackNonQuery?.Invoke(nonQueryResult);
					}
				}
				catch (Exception ex2)
				{
					string text2 = "Sqlite command callback raised an exception";
					if (Connection?.Plugin != null)
					{
						text2 += $" in '{Connection.Plugin.Name} v{Connection.Plugin.Version}' plugin";
					}
					Logger.Error(text2, ex2);
				}
				Connection?.Plugin?.TrackEnd();
			});
		}

		private long GetLastInsertRowId(SqliteConnection connection)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			SqliteCommand val = new SqliteCommand("SELECT last_insert_rowid()", connection);
			try
			{
				return (long)((DbCommand)(object)val).ExecuteScalar();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	private readonly string _dataDirectory;

	private readonly Queue<SQLiteQuery> _queue = new Queue<SQLiteQuery>();

	private readonly object _syncroot = new object();

	private readonly AutoResetEvent _workevent = new AutoResetEvent(initialState: false);

	private readonly HashSet<Connection> _runningConnections = new HashSet<Connection>();

	private bool _running = true;

	private readonly Dictionary<string, Connection> _connections = new Dictionary<string, Connection>();

	private readonly Thread _worker;

	private void Worker()
	{
		while (_running || _queue.Count > 0)
		{
			SQLiteQuery sQLiteQuery = null;
			lock (_syncroot)
			{
				if (_queue.Count > 0)
				{
					sQLiteQuery = _queue.Dequeue();
				}
				else
				{
					foreach (Connection runningConnection in _runningConnections)
					{
						if (runningConnection != null && !runningConnection.ConnectionPersistent)
						{
							CloseDb(runningConnection);
						}
					}
					_runningConnections.Clear();
				}
			}
			if (sQLiteQuery != null)
			{
				sQLiteQuery.Handle();
				if (sQLiteQuery.Connection != null)
				{
					_runningConnections.Add(sQLiteQuery.Connection);
				}
			}
			else if (_running)
			{
				_workevent.WaitOne();
			}
		}
	}

	public SQLite()
	{
		_dataDirectory = Interface.Oxide.DataDirectory;
		_worker = new Thread(Worker);
		_worker.Start();
	}

	public Connection OpenDb(string file, Plugin plugin, bool persistent = false)
	{
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Expected O, but got Unknown
		if (string.IsNullOrEmpty(file))
		{
			return null;
		}
		string text = Path.Combine(_dataDirectory, file);
		if (!text.StartsWith(_dataDirectory, StringComparison.Ordinal))
		{
			throw new Exception("Only access to Carbon directory!");
		}
		string text2 = "Data Source=" + text + ";Version=3;";
		if (_connections.TryGetValue(text2, out var value))
		{
			if (plugin != value.Plugin)
			{
				Logger.Warn($"Already open connection ({text2}), by plugin '{value.Plugin}'...");
				return null;
			}
			Logger.Warn("Already open connection (" + text2 + "), using existing instead...");
		}
		else
		{
			value = new Connection(text2, persistent)
			{
				Plugin = plugin,
				Con = (DbConnection)new SqliteConnection(text2)
			};
			_connections[text2] = value;
		}
		return value;
	}

	private void OnRemovedFromManager(Plugin sender, PluginManager manager)
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, Connection> connection in _connections)
		{
			if (connection.Value.Plugin == sender)
			{
				DbConnection con = connection.Value.Con;
				if (con == null || con.State != ConnectionState.Closed)
				{
					Logger.Warn("Unclosed sqlite connection (" + connection.Value.ConnectionString + "), by plugin '" + (connection.Value.Plugin?.Name ?? "null") + "', closing...");
				}
				connection.Value.Con?.Close();
				connection.Value.Plugin = null;
				list.Add(connection.Key);
			}
		}
		foreach (string item in list)
		{
			_connections.Remove(item);
		}
	}

	public void CloseDb(Connection db)
	{
		if (db != null)
		{
			_connections.Remove(db.ConnectionString);
			db.Con?.Close();
			db.Plugin = null;
		}
	}

	public Sql NewSql()
	{
		return Sql.Builder;
	}

	public void Query(Sql sql, Connection db, Action<List<Dictionary<string, object>>> callback)
	{
		SQLiteQuery item = new SQLiteQuery
		{
			Sql = sql,
			Connection = db,
			Callback = callback
		};
		lock (_syncroot)
		{
			_queue.Enqueue(item);
		}
		_workevent.Set();
	}

	public void ExecuteNonQuery(Sql sql, Connection db, Action<int> callback = null)
	{
		SQLiteQuery item = new SQLiteQuery
		{
			Sql = sql,
			Connection = db,
			CallbackNonQuery = callback,
			NonQuery = true
		};
		lock (_syncroot)
		{
			_queue.Enqueue(item);
		}
		_workevent.Set();
	}

	public void Insert(Sql sql, Connection db, Action<int> callback = null)
	{
		ExecuteNonQuery(sql, db, callback);
	}

	public void Update(Sql sql, Connection db, Action<int> callback = null)
	{
		ExecuteNonQuery(sql, db, callback);
	}

	public void Delete(Sql sql, Connection db, Action<int> callback = null)
	{
		ExecuteNonQuery(sql, db, callback);
	}

	public override void Shutdown()
	{
		_running = false;
		_workevent.Set();
		_worker.Join();
	}
}
