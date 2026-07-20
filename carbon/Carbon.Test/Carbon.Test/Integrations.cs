using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using API.Logger;
using Facepunch;
using UnityEngine;

namespace Carbon.Test;

public static class Integrations
{
	public enum ExitCodes
	{
		Ok = 0,
		FatalFailure = -1
	}

	public class TestBank(int channel, string context) : List<Test>
	{
		public int Channel = channel;

		public string Context = context;

		public void AddTest(object target, Type type, MethodInfo method, Test test)
		{
			test.Setup(target, type, method);
			Add(test);
		}

		public bool AnyTestsFailedFatally()
		{
			for (int i = 0; i < base.Count; i++)
			{
				Test test = base[i];
				if (test.HasFailedFatally())
				{
					return true;
				}
			}
			return false;
		}
	}

	public interface ITestable
	{
		void CollectTests(int channel);
	}

	[AttributeUsage(AttributeTargets.Method)]
	public class Test : Attribute
	{
		public enum StatusTypes
		{
			None,
			Running,
			Complete,
			Canceled,
			Failed,
			Fatal,
			Timeout
		}

		[AttributeUsage(AttributeTargets.Method)]
		public class Assert : Test
		{
			public override string ToPrettyString()
			{
				return base.ToPrettyString() + "assert|";
			}

			public bool IsTrue(bool condition, string info = null)
			{
				if (condition)
				{
					Warn("IsTrue passed    - " + (string.IsNullOrEmpty(info) ? "[bool condition]" : info));
					return true;
				}
				Fail("IsTrue failed    - " + (string.IsNullOrEmpty(info) ? "[bool condition]" : info));
				return false;
			}

			public bool IsFalse(bool condition, string info = null)
			{
				if (!condition)
				{
					Warn("IsFalse passed   - " + (string.IsNullOrEmpty(info) ? "[bool condition]" : info));
					return true;
				}
				Fail("IsFalse failed   - " + (string.IsNullOrEmpty(info) ? "[bool condition]" : info));
				return false;
			}

			public bool IsNull(object value, string info = null)
			{
				if (value == null)
				{
					Warn("IsNull passed    - " + (string.IsNullOrEmpty(info) ? "[object value]" : info) + " == null");
					return true;
				}
				Fail(string.Format("IsNull failed    - {0} == {1}", string.IsNullOrEmpty(info) ? "[object value]" : info, value));
				return false;
			}

			public bool IsNotNull(object value, string info = null)
			{
				if (value != null)
				{
					Warn(string.Format("IsNotNull passed - {0} == {1}", string.IsNullOrEmpty(info) ? "[object value]" : info, value));
					return true;
				}
				Fail("IsNotNull failed - " + (string.IsNullOrEmpty(info) ? "[object value]" : info) + " = null");
				return false;
			}
		}

		public int Channel = 1;

		public float Timeout = 1000f;

		public bool CancelOnFail = true;

		private List<Exception> _exceptions = new List<Exception>();

		private Type _type;

		private MethodInfo _method;

		private object _target;

		private StatusTypes _statusType;

		private static int _prefixScale;

		private double _duration;

		private bool _isAsync;

		private static object[] _args = new object[1];

		public bool IsRunning => Status == StatusTypes.Running;

		public StatusTypes Status => _statusType;

		public IEnumerable<Exception> Exceptions => _exceptions.AsEnumerable();

		public void Setup(object target, Type type, MethodInfo info)
		{
			_type = type;
			_method = info;
			_target = target;
			_isAsync = _method.ReturnType?.GetMethod("GetAwaiter") != null || _method.GetCustomAttribute<AsyncStateMachineAttribute>() != null;
		}

		public void SetDuration(TimeSpan span)
		{
			_duration = span.TotalMilliseconds;
		}

		public void SetStatus(StatusTypes status)
		{
			_statusType = status;
		}

		public void Run()
		{
			SetStatus(StatusTypes.Running);
			_args[0] = this;
			try
			{
				_method.Invoke(_target, (_method.GetParameters().Length == 1) ? _args : Array.Empty<object>());
				if (!_isAsync)
				{
					Complete();
				}
			}
			catch (Exception ex)
			{
				_exceptions.Add(ex);
				Fatal("Runtime method failure", ex);
			}
		}

		public void RunCheck()
		{
			if (!(Timeout <= 0f) && _duration >= (double)Timeout)
			{
				TimeOut();
			}
		}

		public void Reset()
		{
			SetStatus(StatusTypes.None);
			_exceptions.Clear();
			SetDuration(default(TimeSpan));
		}

		public bool HasFailedFatally()
		{
			if (Status != StatusTypes.Fatal)
			{
				if (CancelOnFail)
				{
					return Status != StatusTypes.Complete;
				}
				return false;
			}
			return true;
		}

		public void Complete()
		{
			if (IsRunning)
			{
				SetStatus(StatusTypes.Complete);
				Log($"Complete - {_exceptions.Count:n0} excp.");
			}
		}

		public void TimeOut()
		{
			if (IsRunning)
			{
				SetStatus(StatusTypes.Timeout);
				Warn($"Timeout >= {Timeout:0}ms");
			}
		}

		public void Fail(string message, Exception exception = null)
		{
			if (IsRunning)
			{
				SetStatus(StatusTypes.Failed);
				Error("Fail - " + message, exception);
			}
		}

		public void Fatal(string message, Exception exception = null)
		{
			if (IsRunning)
			{
				SetStatus(StatusTypes.Fatal);
				Error("Fatal - " + message, exception);
			}
		}

		public void Log(object message)
		{
			CalculatePrettyString(out var mainString, out var spacing);
			Logger.Console(spacing + mainString + "  " + ((message == null) ? "no message" : message.ToString()));
		}

		public void Warn(object message)
		{
			CalculatePrettyString(out var mainString, out var spacing);
			Logger.Console(spacing + mainString + "  " + ((message == null) ? "no message" : message.ToString()), Severity.Warning);
		}

		public void Error(object message, Exception exception)
		{
			CalculatePrettyString(out var mainString, out var spacing);
			Logger.Console(spacing + mainString + "  " + ((message == null) ? "no message" : message.ToString()), Severity.Error, exception);
			SetStatus(StatusTypes.Failed);
		}

		public void Fatal(object message, Exception exception)
		{
			CalculatePrettyString(out var mainString, out var spacing);
			Logger.Console(spacing + mainString + "  " + ((message == null) ? "no message" : message.ToString()), Severity.Error, exception);
			SetStatus(StatusTypes.Fatal);
		}

		public virtual string ToPrettyString()
		{
			return $"{_type.Name}.{_method.Name}|{_duration:0}ms|".ToLower();
		}

		public void CalculatePrettyString(out string mainString, out string spacing)
		{
			mainString = ToPrettyString();
			int length = mainString.Length;
			if (length > _prefixScale)
			{
				_prefixScale = length;
			}
			spacing = new string(' ', _prefixScale - length);
		}
	}

	public const int DEFAULT_CHANNEL = 1;

	public static ILogger Logger;

	public static readonly Stopwatch Stopwatch = new Stopwatch();

	public static readonly Dictionary<int, Queue<TestBank>> Banks = new Dictionary<int, Queue<TestBank>>();

	public static Action OnFatalTestFailure;

	public static ExitCodes ExitCode;

	private static bool _isRunning;

	public static bool IsRunning()
	{
		return _isRunning;
	}

	public static TestBank Get(string context, Type type, object target = null, int channel = 1)
	{
		TestBank testBank = null;
		MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (MethodInfo methodInfo in methods)
		{
			Test customAttribute = methodInfo.GetCustomAttribute<Test>();
			if (customAttribute != null && (channel == -1 || customAttribute.Channel == channel))
			{
				(testBank ?? (testBank = new TestBank(channel, context))).AddTest(target ?? (target = Activator.CreateInstance(type)), type, methodInfo, customAttribute);
			}
		}
		return testBank;
	}

	public static void EnqueueBed(TestBank bank)
	{
		if (!Banks.TryGetValue(bank.Channel, out var value))
		{
			value = (Banks[bank.Channel] = new Queue<TestBank>());
		}
		value.Enqueue(bank);
	}

	public static void Run(float delay, int channel)
	{
		((MonoBehaviour)SingletonComponent<ServerMgr>.Instance).StartCoroutine(RunRoutine(delay, channel));
	}

	public static IEnumerator RunRoutine(float delay, int channel)
	{
		if (_isRunning)
		{
			yield break;
		}
		_isRunning = true;
		List<TestBank> banks = Pool.Get<List<TestBank>>();
		Queue<TestBank> value;
		if (channel == -1)
		{
			foreach (Queue<TestBank> value2 in Banks.Values)
			{
				while (value2.Count != 0)
				{
					banks.Add(value2.Dequeue());
				}
			}
		}
		else if (Banks.TryGetValue(channel, out value))
		{
			while (value.Count != 0)
			{
				banks.Add(value.Dequeue());
			}
		}
		bool anyTestsFailedFatally = false;
		for (int i = 0; i < banks.Count; i++)
		{
			TestBank bank = banks[i];
			yield return RunBankRoutine(delay, bank);
			if (bank.AnyTestsFailedFatally())
			{
				anyTestsFailedFatally = true;
				break;
			}
		}
		Pool.FreeUnmanaged<TestBank>(ref banks);
		_isRunning = false;
		try
		{
			if (anyTestsFailedFatally)
			{
				ExitCode = ExitCodes.FatalFailure;
				OnFatalTestFailure?.Invoke();
			}
		}
		catch (Exception exception)
		{
			Logger.Console("Fatal test failure callback error", Severity.Error, exception);
		}
	}

	public static IEnumerator RunBankRoutine(float delay, TestBank bank)
	{
		int completed = 0;
		Logger.Console("initialized testbed - context: " + bank.Context);
		for (int i = 0; i < bank.Count; i++)
		{
			Stopwatch.Restart();
			Test test = bank[i];
			test.Run();
			while (test.IsRunning)
			{
				test.SetDuration(Stopwatch.Elapsed);
				test.RunCheck();
				yield return null;
			}
			if (test.HasFailedFatally())
			{
				Logger.Console("cancelled due to fatal status - context: " + bank.Context, Severity.Error);
				break;
			}
			completed++;
			ExitCode = ExitCodes.Ok;
			if (delay > 0f)
			{
				yield return CoroutineEx.waitForSecondsRealtime(delay);
			}
			else
			{
				yield return null;
			}
		}
		Logger.Console(string.Format("completed {0:n0} out of {1:n0} {2} - context: {3}", new object[4]
		{
			completed,
			bank.Count,
			(bank.Count == 1) ? "test" : "tests",
			bank.Context
		}));
		yield return null;
	}

	public static void Clear(int channel)
	{
		if (channel == -1)
		{
			Banks.Clear();
		}
		else
		{
			Banks.Remove(channel);
		}
	}
}
