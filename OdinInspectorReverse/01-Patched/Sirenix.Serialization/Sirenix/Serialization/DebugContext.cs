using System;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Defines a context for debugging and logging during serialization and deserialization. This class is thread-safe.
	/// </summary>
	public sealed class DebugContext
	{
		private readonly object LOCK = new object();

		private volatile ILogger logger;

		private volatile LoggingPolicy loggingPolicy;

		private volatile ErrorHandlingPolicy errorHandlingPolicy;

		/// <summary>
		/// The logger to use for logging messages.
		/// </summary>
		public ILogger Logger
		{
			get
			{
				if (logger == null)
				{
					lock (LOCK)
					{
						if (logger == null)
						{
							logger = DefaultLoggers.UnityLogger;
						}
					}
				}
				return logger;
			}
			set
			{
				lock (LOCK)
				{
					logger = value;
				}
			}
		}

		/// <summary>
		/// The logging policy to use.
		/// </summary>
		public LoggingPolicy LoggingPolicy
		{
			get
			{
				return loggingPolicy;
			}
			set
			{
				loggingPolicy = value;
			}
		}

		/// <summary>
		/// The error handling policy to use.
		/// </summary>
		public ErrorHandlingPolicy ErrorHandlingPolicy
		{
			get
			{
				return errorHandlingPolicy;
			}
			set
			{
				errorHandlingPolicy = value;
			}
		}

		/// <summary>
		/// Log a warning. Depending on the logging policy and error handling policy, this message may be suppressed or result in an exception being thrown.
		/// </summary>
		public void LogWarning(string message)
		{
			if (errorHandlingPolicy == ErrorHandlingPolicy.ThrowOnWarningsAndErrors)
			{
				throw new SerializationAbortException("The following warning was logged during serialization or deserialization: " + (message ?? "EMPTY EXCEPTION MESSAGE"));
			}
			if (loggingPolicy == LoggingPolicy.LogWarningsAndErrors)
			{
				Logger.LogWarning(message);
			}
		}

		/// <summary>
		/// Log an error. Depending on the logging policy and error handling policy, this message may be suppressed or result in an exception being thrown.
		/// </summary>
		public void LogError(string message)
		{
			if (errorHandlingPolicy != ErrorHandlingPolicy.Resilient)
			{
				throw new SerializationAbortException("The following error was logged during serialization or deserialization: " + (message ?? "EMPTY EXCEPTION MESSAGE"));
			}
			if (loggingPolicy != LoggingPolicy.Silent)
			{
				Logger.LogError(message);
			}
		}

		/// <summary>
		/// Log an exception. Depending on the logging policy and error handling policy, this message may be suppressed or result in an exception being thrown.
		/// </summary>
		public void LogException(Exception exception)
		{
			if (exception == null)
			{
				throw new ArgumentNullException("exception");
			}
			if (exception is SerializationAbortException)
			{
				throw exception;
			}
			if (errorHandlingPolicy != ErrorHandlingPolicy.Resilient)
			{
				throw new SerializationAbortException("An exception of type " + exception.GetType().Name + " occurred during serialization or deserialization.", exception);
			}
			if (loggingPolicy != LoggingPolicy.Silent)
			{
				Logger.LogException(exception);
			}
		}

		public void ResetToDefault()
		{
			lock (LOCK)
			{
				logger = null;
				loggingPolicy = LoggingPolicy.LogErrors;
				errorHandlingPolicy = ErrorHandlingPolicy.Resilient;
			}
		}
	}
}
