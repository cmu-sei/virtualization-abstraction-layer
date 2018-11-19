using System;
using System.IO;

namespace AppUtil
{

	/// <summary>
	/// Logger to file if possible or console.
	/// </summary>
	public class Log {

      private StreamWriter _logger;
      private bool _toConsole;

		public Log() {
		}

      public void Init(string logfilepath, bool appendlog, bool toConsole) {
         try {
            string finlog = logfilepath;
            if (logfilepath.LastIndexOf(".") < 0) {
               finlog = logfilepath + ".txt";
            }
            _logger = new StreamWriter(finlog, appendlog);
            _logger.AutoFlush = true;

            _toConsole = toConsole;
            InternalLogLine(DateTime.Now, "Begin Log.");
         } catch (Exception e) {
            Console.WriteLine("Exception initializing log to : " + 
                              logfilepath + ". Using console. ");

            _logger = null;
            _toConsole = true;
         }
      }

      public void Close() {
         lock (this) {
            if (_logger != null) {
               DateTime dt = DateTime.Now;
               InternalLogLine(dt, "End Log.");
               InternalLogLine(dt, "");
               _logger.Flush();
               _logger.Close();
               _logger = null;
            }
         }
      }

      public void LogLine(string strmsg) {
         lock (this) {
            if (_logger != null) {
               _logger.WriteLine(strmsg);
            }
            if (_toConsole || _logger == null) {
               Console.WriteLine(strmsg);
            }
         }
      }

      public void InternalLogLine(DateTime dt, string msg) {
         lock (this) {
            string strmsg = msg;
            if (msg.Length > 0) {
               strmsg = "[ " + dt.ToString() + " ] " + msg;
            }
            if (_logger != null) {
               _logger.WriteLine(strmsg);
            }
            if (_toConsole || _logger == null) {
               Console.WriteLine(strmsg);
            }
         }
      }
   }
}
