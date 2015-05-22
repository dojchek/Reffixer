using System;
using System.IO;

namespace Reffixer.Log
{
	internal class ConsoleLogger : ILogger
	{
		private readonly string _logPath;

		public ConsoleLogger(string logPath)
		{
			_logPath = logPath;
			using (File.Create(_logPath)) { }
		}

		public void Info(string message)
		{
			Console.ResetColor();
			Console.WriteLine(message);
			Log(string.Format("INFO: {0}", message));
		}

		public void Warning(string message)
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.Write("WARNING:\n");
			Console.ResetColor();
			Console.WriteLine(message);
			Log(string.Format("WARNING: {0}", message));
		}

		public void Warning(string message, Exception exception)
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.Write("WARNING:\n");
			Console.ResetColor();
			Console.WriteLine(message);
			Log(string.Format("WARNING: {0}\nException:{1}", message, exception));
		}

		public void Error(string message)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("ERROR:\n");
			Console.ResetColor();
			Console.WriteLine(message);
			Log(string.Format("ERROR: {0}", message));
		}

		public void Error(Exception exception)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("ERROR:\n");
			Console.ResetColor();
			Log(string.Format("ERROR: {0}\nStacktrace:\n{1}", exception.Message, exception.StackTrace));
		}

		public void Error(string message, Exception exception)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("ERROR:\n");
			Console.ResetColor();
			Console.WriteLine(exception.Message);
			Log(string.Format("ERROR: {0}\nException:\n{1}\nStacktrace:\n{2}", message, exception.Message, exception.StackTrace));
		}

		private void Log(string logMessage)
		{
			using (var w = File.AppendText(_logPath))
			{
				Log(logMessage, w);
			}
		}

		private static void Log(string logMessage, TextWriter w)
		{
			w.Write("\r\nLog Entry : ");
			w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString());
			w.WriteLine("  :");
			w.WriteLine("  :{0}", logMessage);
			w.WriteLine("-------------------------------");
		}
	}
}