using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Threading;
using Carbon;
using MySqlConnector;
using Oxide.Core.Database;
using Oxide.Core.Libraries;
using Oxide.Core.Plugins;

namespace Oxide.Core.MySql.Libraries;

public class MySql : Library, IDatabaseProvider
{
	public class MySqlQuery
	{
		internal object _cmdSource;

		internal object _connectionSource;

		public Action<List<Dictionary<string, object>>> Callback { get; internal set; }

		public Action<int> CallbackNonQuery { get; internal set; }

		public Sql Sql { get; internal set; }

		public Connection Connection { get; internal set; }

		public bool NonQuery { get; internal set; }

		internal MySqlCommand _cmd()
		{
			object cmdSource = _cmdSource;
			return (MySqlCommand)((cmdSource is MySqlCommand) ? cmdSource : null);
		}

		internal MySqlConnection _connection()
		{
			object connectionSource = _connectionSource;
			return (MySqlConnection)((connectionSource is MySqlConnection) ? connectionSource : null);
		}

		private void Cleanup()
		{
			if (_cmd() != null)
			{
				((Component)(object)_cmd()).Dispose();
				_cmdSource = null;
			}
			_connectionSource = null;
		}

		public bool Handle()
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
				_connectionSource = (object)(MySqlConnection)Connection.Con;
				if (((DbConnection)(object)_connection()).State == ConnectionState.Closed)
				{
					((DbConnection)(object)_connection()).Open();
				}
				_cmdSource = _connection().CreateCommand();
				((DbCommand)(object)_cmd()).CommandTimeout = 120;
				((DbCommand)(object)_cmd()).CommandText = Sql.SQL;
				Sql.AddParams((IDbCommand)_cmd(), Sql.Arguments, "@");
				if (NonQuery)
				{
					nonQueryResult = ((DbCommand)(object)_cmd()).ExecuteNonQuery();
				}
				else
				{
					MySqlDataReader val = _cmd().ExecuteReader();
					try
					{
						list = new List<Dictionary<string, object>>();
						while (((DbDataReader)(object)val).Read() && (!Connection.ConnectionPersistent || (Connection.Con.State != ConnectionState.Closed && Connection.Con.State != ConnectionState.Broken)))
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
				lastInsertRowId = _cmd().LastInsertedId;
				Cleanup();
			}
			catch (Exception ex)
			{
				string text = "MySql handle raised an exception";
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
					string text2 = "MySql command callback raised an exception";
					if (Connection?.Plugin != null)
					{
						text2 += $" in '{Connection.Plugin.Name} v{Connection.Plugin.Version}' plugin";
					}
					Logger.Error(text2, ex2);
				}
				if (Connection != null && Connection.Plugin != null)
				{
					Connection.Plugin.TrackEnd();
				}
			});
			return true;
		}
	}

	private readonly Queue<MySqlQuery> _queue = new Queue<MySqlQuery>();

	private readonly object _syncroot = new object();

	private readonly AutoResetEvent _workevent = new AutoResetEvent(initialState: false);

	private readonly HashSet<Connection> _runningConnections = new HashSet<Connection>();

	private bool _running = true;

	private readonly Dictionary<string, Dictionary<string, Connection>> _connections = new Dictionary<string, Dictionary<string, Connection>>();

	private readonly Thread _worker;

	public MySql()
	{
		_worker = new Thread(Worker);
		_worker.Start();
	}

	private void Worker()
	{
		while (_running || _queue.Count > 0)
		{
			MySqlQuery mySqlQuery = null;
			object syncroot = _syncroot;
			lock (syncroot)
			{
				if (_queue.Count > 0)
				{
					mySqlQuery = _queue.Dequeue();
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
			if (mySqlQuery != null)
			{
				mySqlQuery.Handle();
				if (mySqlQuery.Connection != null)
				{
					_runningConnections.Add(mySqlQuery.Connection);
				}
			}
			else if (_running)
			{
				_workevent.WaitOne();
			}
		}
	}

	public Connection OpenDb(string host, int port, string database, string user, string password, Plugin plugin, bool persistent = false)
	{
		return OpenDb(string.Format("Server={0};Port={1};Database={2};User={3};Password={4};", new object[5] { host, port, database, user, password }) + "Pooling=false;default command timeout=120;Allow Zero Datetime=true;SslMode=Disabled;AllowPublicKeyRetrieval=true;CharSet=utf8mb4;", plugin, persistent);
	}

	public Connection OpenDb(string conStr, Plugin plugin, bool persistent = false)
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Expected O, but got Unknown
		if (!_connections.TryGetValue(plugin?.Name ?? "null", out var value))
		{
			Dictionary<string, Connection> dictionary = (_connections[plugin?.Name ?? "null"] = new Dictionary<string, Connection>());
			value = dictionary;
		}
		if (value.TryGetValue(conStr, out var value2))
		{
			DbConnection con = value2.Con;
			Logger.Warn("Already open connection for MySQL, using existing instead...");
		}
		else
		{
			Connection obj = new Connection(conStr, persistent)
			{
				Plugin = plugin,
				Con = (DbConnection)new MySqlConnection(conStr)
			};
			value2 = (value[conStr] = obj);
		}
		return value2;
	}

	public void CloseDb(Connection db)
	{
		if (db == null)
		{
			return;
		}
		Dictionary<string, Dictionary<string, Connection>> connections = _connections;
		if (connections.TryGetValue(db.Plugin?.Name ?? "null", out var value))
		{
			value.Remove(db.ConnectionString);
			if (value.Count == 0)
			{
				Dictionary<string, Dictionary<string, Connection>> connections2 = _connections;
				connections2.Remove(db.Plugin?.Name ?? "null");
			}
		}
		db.Con?.Close();
		db.Plugin = null;
	}

	public Sql NewSql()
	{
		return Sql.Builder;
	}

	public void Query(Sql sql, Connection db, Action<List<Dictionary<string, object>>> callback)
	{
		MySqlQuery item = new MySqlQuery
		{
			Sql = sql,
			Connection = db,
			Callback = callback
		};
		object syncroot = _syncroot;
		lock (syncroot)
		{
			_queue.Enqueue(item);
		}
		_workevent.Set();
	}

	public void ExecuteNonQuery(Sql sql, Connection db, Action<int> callback = null)
	{
		MySqlQuery item = new MySqlQuery
		{
			Sql = sql,
			Connection = db,
			CallbackNonQuery = callback,
			NonQuery = true
		};
		object syncroot = _syncroot;
		lock (syncroot)
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

	public override void Dispose()
	{
		_running = false;
		_workevent.Set();
		_worker.Join();
	}
}
