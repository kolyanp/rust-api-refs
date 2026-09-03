using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Facepunch;
using UnityEngine;
using UnityEngine.Assertions;

namespace Network;

public abstract class BaseNetwork
{
	private struct DelayedRead
	{
		public NetRead Read;

		public long DeliverAtTimestamp;
	}

	public enum StatTypeLong
	{
		BytesSent,
		BytesSent_LastSecond,
		BytesReceived,
		BytesReceived_LastSecond,
		MessagesInSendBuffer,
		BytesInSendBuffer,
		MessagesInResendBuffer,
		BytesInResendBuffer,
		PacketLossAverage,
		PacketLossLastSecond,
		ThrottleBytes
	}

	public static ArrayPool<byte> ArrayPool = new ArrayPool<byte>(4195328);

	public static bool Multithreading = true;

	protected readonly object readLock = new object();

	protected readonly object writeLock = new object();

	protected readonly object decryptLock = new object();

	private Stopwatch stopwatch = new Stopwatch();

	private ConcurrentQueue<NetRead> readQueue;

	private ConcurrentQueue<NetWrite> writeQueue;

	private ConcurrentQueue<NetRead> decryptQueue;

	private int readQueueSizeInBytes;

	private int writeQueueSizeInBytes;

	private int decryptQueueSizeInBytes;

	private int readQueueCount;

	private int writeQueueCount;

	private int decryptQueueCount;

	private AutoResetEvent mainThreadReset;

	private AutoResetEvent readThreadReset;

	private AutoResetEvent writeThreadReset;

	private AutoResetEvent decryptThreadReset;

	private Thread readThread;

	private Thread writeThread;

	private Thread decryptThread;

	private long _timeCacheTicks;

	public INetworkCryptography cryptography;

	public static float SimulatedReadLatencyMs = 0f;

	private const int SimulatedLatencyQueueCapacity = 4096;

	private DelayedRead[] delayedReads;

	private int delayedReadHead;

	private int delayedReadTail;

	private int delayedReadCount;

	protected abstract int MaxReceiveTimeValue { get; }

	protected abstract int MaxReadQueueLengthValue { get; }

	protected abstract int MaxWriteQueueLengthValue { get; }

	protected abstract int MaxDecryptQueueLengthValue { get; }

	protected abstract int MaxReadQueueBytesValue { get; }

	protected abstract int MaxWriteQueueBytesValue { get; }

	protected abstract int MaxDecryptQueueBytesValue { get; }

	protected abstract int MaxMainThreadWaitValue { get; }

	protected abstract int MaxReadThreadWaitValue { get; }

	protected abstract int MaxWriteThreadWaitValue { get; }

	protected abstract int MaxDecryptThreadWaitValue { get; }

	public int ReadQueueLength => Volatile.Read(in readQueueCount);

	public int WriteQueueLength => Volatile.Read(in writeQueueCount);

	public int DecryptQueueLength => Volatile.Read(in decryptQueueCount);

	public int ReadQueueBytes => readQueueSizeInBytes;

	public int WriteQueueBytes => writeQueueSizeInBytes;

	public int DecryptQueueBytes => decryptQueueSizeInBytes;

	protected void MultithreadingInit(IServerCallback callbacks)
	{
		if (readThread != null)
		{
			readThread.Abort();
			readThread = null;
		}
		if (writeThread != null)
		{
			writeThread.Abort();
			writeThread = null;
		}
		if (decryptThread != null)
		{
			decryptThread.Abort();
			decryptThread = null;
		}
		if (Multithreading)
		{
			readQueue = new ConcurrentQueue<NetRead>();
			writeQueue = new ConcurrentQueue<NetWrite>();
			decryptQueue = new ConcurrentQueue<NetRead>();
			readQueueSizeInBytes = 0;
			writeQueueSizeInBytes = 0;
			decryptQueueSizeInBytes = 0;
			mainThreadReset = new AutoResetEvent(initialState: false);
			readThreadReset = new AutoResetEvent(initialState: false);
			writeThreadReset = new AutoResetEvent(initialState: false);
			decryptThreadReset = new AutoResetEvent(initialState: false);
			readThread = new Thread(ReadThread);
			readThread.IsBackground = true;
			readThread.Start();
			writeThread = new Thread(WriteThread);
			writeThread.IsBackground = true;
			writeThread.Start();
			decryptThread = new Thread(DecryptThread);
			decryptThread.IsBackground = true;
			decryptThread.Start();
		}
	}

	public virtual bool IsConnected()
	{
		return false;
	}

	protected virtual bool Receive()
	{
		return false;
	}

	public void EnqueueWrite(NetWrite write)
	{
		Assert.IsNotNull<NetWrite>(write, "write != null");
		Assert.IsNotNull<List<Connection>>(write.connections, "write.connections != null");
		foreach (Connection connection in write.connections)
		{
			Assert.IsNotNull<Connection>(connection, "connection != null");
		}
		if (WriteQueueLength >= MaxWriteQueueLengthValue || writeQueueSizeInBytes >= MaxWriteQueueBytesValue)
		{
			Debug.LogWarning((object)"Main thread stalling: Write queue at capacity, waiting for write thread...");
			mainThreadReset.WaitOne(MaxMainThreadWaitValue);
		}
		int value = (int)write.Length;
		writeQueue.Enqueue(write);
		Interlocked.Add(ref writeQueueSizeInBytes, value);
		Interlocked.Increment(ref writeQueueCount);
		writeThreadReset.Set();
	}

	public void EnqueueRead(NetRead read)
	{
		readQueue.Enqueue(read);
		Interlocked.Add(ref readQueueSizeInBytes, (int)read.Length);
		Interlocked.Increment(ref readQueueCount);
	}

	public void EnqueueDecrypt(NetRead read)
	{
		decryptQueue.Enqueue(read);
		Interlocked.Add(ref decryptQueueSizeInBytes, (int)read.Length);
		Interlocked.Increment(ref decryptQueueCount);
		decryptThreadReset.Set();
	}

	public virtual void ProcessWrite(NetWrite write)
	{
	}

	public virtual void ProcessRead(NetRead read)
	{
	}

	public void ProcessDecrypt(NetRead read)
	{
		Decrypt(read.connection, read);
		if (Multithreading)
		{
			EnqueueRead(read);
		}
		else
		{
			ProcessRead(read);
		}
	}

	private void ReadThread()
	{
		while (IsConnected())
		{
			try
			{
				ReadThreadCycle();
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
			if (DecryptQueueLength >= MaxDecryptQueueLengthValue || decryptQueueSizeInBytes >= MaxDecryptQueueBytesValue)
			{
				readThreadReset.WaitOne(MaxReadThreadWaitValue);
			}
			else
			{
				readThreadReset.WaitOne(1);
			}
		}
	}

	private void WriteThread()
	{
		while (IsConnected())
		{
			try
			{
				WriteThreadCycle();
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
			if (WriteQueueLength <= 0)
			{
				writeThreadReset.WaitOne(MaxWriteThreadWaitValue);
			}
		}
	}

	private void DecryptThread()
	{
		while (IsConnected())
		{
			try
			{
				DecryptThreadCycle();
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
			if (ReadQueueLength >= MaxReadQueueLengthValue || readQueueSizeInBytes >= MaxReadQueueBytesValue || DecryptQueueLength <= 0)
			{
				decryptThreadReset.WaitOne(MaxDecryptThreadWaitValue);
			}
		}
	}

	private void ReadThreadCycle()
	{
		while (DecryptQueueLength < MaxDecryptQueueLengthValue && decryptQueueSizeInBytes < MaxDecryptQueueBytesValue)
		{
			lock (readLock)
			{
				if (!IsConnected() || !Receive())
				{
					break;
				}
			}
		}
	}

	private void WriteThreadCycle()
	{
		NetWrite result;
		while (writeQueue.TryDequeue(out result))
		{
			Interlocked.Add(ref writeQueueSizeInBytes, -(int)result.Length);
			Interlocked.Decrement(ref writeQueueCount);
			mainThreadReset.Set();
			lock (writeLock)
			{
				if (!IsConnected())
				{
					break;
				}
				ProcessWrite(result);
			}
		}
	}

	private void DecryptThreadCycle()
	{
		NetRead result;
		while (ReadQueueLength < MaxReadQueueLengthValue && readQueueSizeInBytes < MaxReadQueueBytesValue && decryptQueue.TryDequeue(out result))
		{
			Interlocked.Add(ref decryptQueueSizeInBytes, -(int)result.Length);
			Interlocked.Decrement(ref decryptQueueCount);
			readThreadReset.Set();
			lock (decryptLock)
			{
				if (!IsConnected())
				{
					break;
				}
				ProcessDecrypt(result);
			}
		}
	}

	public void Cycle()
	{
		Interlocked.Exchange(ref _timeCacheTicks, DateTime.UtcNow.Ticks);
		if (!IsConnected())
		{
			return;
		}
		NetProfileCapture.Tick(this);
		if (Multithreading)
		{
			stopwatch.Restart();
			if (SimulatedReadLatencyMs <= 0f)
			{
				NetRead result;
				while (readQueue.TryDequeue(out result))
				{
					Interlocked.Add(ref readQueueSizeInBytes, -(int)result.Length);
					Interlocked.Decrement(ref readQueueCount);
					decryptThreadReset.Set();
					if (IsConnected())
					{
						ProcessRead(result);
						if (stopwatch.Elapsed.TotalMilliseconds > (double)MaxReceiveTimeValue)
						{
							break;
						}
						continue;
					}
					break;
				}
			}
			else
			{
				RunSimulatedLatencyRead();
			}
		}
		else
		{
			stopwatch.Restart();
			while (IsConnected() && Receive() && !(stopwatch.Elapsed.TotalMilliseconds > (double)MaxReceiveTimeValue))
			{
			}
		}
	}

	private void EnsureDelayedReadQueue()
	{
		if (delayedReads == null)
		{
			delayedReads = new DelayedRead[4096];
			delayedReadHead = 0;
			delayedReadTail = 0;
			delayedReadCount = 0;
		}
	}

	private bool TryEnqueueDelayed(NetRead read, long deliverAtTimestamp)
	{
		if (delayedReadCount >= 4096)
		{
			return false;
		}
		delayedReads[delayedReadTail].Read = read;
		delayedReads[delayedReadTail].DeliverAtTimestamp = deliverAtTimestamp;
		delayedReadTail++;
		if (delayedReadTail >= 4096)
		{
			delayedReadTail = 0;
		}
		delayedReadCount++;
		return true;
	}

	private bool TryPeekDelayed(out DelayedRead item)
	{
		if (delayedReadCount <= 0)
		{
			item = default(DelayedRead);
			return false;
		}
		item = delayedReads[delayedReadHead];
		return true;
	}

	private void DequeueDelayed()
	{
		delayedReads[delayedReadHead].Read = null;
		delayedReadHead++;
		if (delayedReadHead >= 4096)
		{
			delayedReadHead = 0;
		}
		delayedReadCount--;
	}

	public NetWrite StartWrite()
	{
		NetWrite netWrite = Pool.Get<NetWrite>();
		netWrite.Start(this);
		return netWrite;
	}

	private void RunSimulatedLatencyRead()
	{
		float simulatedReadLatencyMs = SimulatedReadLatencyMs;
		EnsureDelayedReadQueue();
		long timestamp = Stopwatch.GetTimestamp();
		long num = (long)((double)simulatedReadLatencyMs * ((double)Stopwatch.Frequency / 1000.0));
		NetRead result;
		while (readQueue.TryDequeue(out result))
		{
			Interlocked.Add(ref readQueueSizeInBytes, -(int)result.Length);
			Interlocked.Decrement(ref readQueueCount);
			decryptThreadReset.Set();
			if (!IsConnected())
			{
				break;
			}
			long deliverAtTimestamp = timestamp + num;
			if (!TryEnqueueDelayed(result, deliverAtTimestamp))
			{
				ProcessRead(result);
			}
			if (stopwatch.Elapsed.TotalMilliseconds > (double)MaxReceiveTimeValue)
			{
				break;
			}
		}
		timestamp = Stopwatch.GetTimestamp();
		DelayedRead item;
		while (TryPeekDelayed(out item) && item.DeliverAtTimestamp <= timestamp && IsConnected())
		{
			NetRead read = item.Read;
			DequeueDelayed();
			ProcessRead(read);
			if (!(stopwatch.Elapsed.TotalMilliseconds > (double)MaxReceiveTimeValue))
			{
				timestamp = Stopwatch.GetTimestamp();
				continue;
			}
			break;
		}
	}

	internal long GetServerTimestampTicks()
	{
		return Interlocked.Read(in _timeCacheTicks);
	}

	protected Message StartMessage(Message.Type type, NetRead read)
	{
		Message message = Pool.Get<Message>();
		message.peer = this;
		message.type = type;
		message.read = read;
		NetProfileCapture.OnReceive(this, type, read);
		return message;
	}

	public void Decrypt(Connection connection, NetRead read)
	{
		if (cryptography == null || connection == null)
		{
			return;
		}
		if (connection.encryptionLevel == 0)
		{
			connection.trusted = connection.isAuthenticated;
		}
		else
		{
			if (read.Length <= 1)
			{
				return;
			}
			int num = read.PeekPacketID() - 140;
			if (num <= 0 || num >= 28 || !Message.EncryptionPerType[num])
			{
				return;
			}
			var (array, num2) = read.GetBuffer();
			if (connection.encryptionLevel > 1)
			{
				if (read.Length >= 23)
				{
					connection.trusted = (array[num2 - 17] & 1) != 0;
				}
			}
			else
			{
				connection.trusted = connection.isAuthenticated;
			}
			ArraySegment<byte> data = new ArraySegment<byte>(array, 1, num2 - 1);
			cryptography.Decrypt(connection, ref data);
			read.SetLength(data.Offset + data.Count);
		}
	}

	public ArraySegment<byte> Encrypt(Connection connection, NetWrite write)
	{
		(byte[] Buffer, int Length) buffer = write.GetBuffer();
		byte[] item = buffer.Buffer;
		int item2 = buffer.Length;
		ArraySegment<byte> arraySegment = new ArraySegment<byte>(item, 1, item2 - 1);
		if (cryptography == null)
		{
			return arraySegment;
		}
		if (connection == null)
		{
			return arraySegment;
		}
		if (connection.encryptionLevel == 0)
		{
			return arraySegment;
		}
		if (write.Length <= 1)
		{
			return arraySegment;
		}
		int num = write.PeekPacketID() - 140;
		if (num <= 0)
		{
			return arraySegment;
		}
		if (num >= 28)
		{
			return arraySegment;
		}
		if (!Message.EncryptionPerType[num])
		{
			return arraySegment;
		}
		return cryptography.EncryptCopy(connection, arraySegment);
	}

	public void RecordReadForConnection(Connection connection, NetRead read)
	{
		connection?.RecordPacket(read);
	}

	public void RecordWriteForConnection(Connection connection, NetWrite write)
	{
		connection?.RecordPacket(write);
	}

	public virtual string GetDebug(Connection connection)
	{
		return null;
	}

	public virtual ulong GetStat(Connection connection, StatTypeLong type)
	{
		return 0uL;
	}
}
