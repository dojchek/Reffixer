using System;

namespace Reffixer.Log
{
	internal interface ILogger
	{
		void Info(string message);
		void Warning(string message);
		void Warning(string message, Exception exception);
		void Error(string message);
		void Error(Exception exception);
		void Error(string message, Exception exception);
	}
}
