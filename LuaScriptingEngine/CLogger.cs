/*
 * Logger Component
 * Copyright (C) 2008 Erdoðan Kalemci <olligan@gmail.com>
 * You have no rights to distrubute, modify and use this code unless writer gives permission
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.IO;
using System.Threading;
#if WINDOWS_PHONE
using System.Windows.Threading;
#elif NETFX_CORE
using Windows.UI.Xaml;
#endif

namespace LoggerNamespace
{
    /// <summary>
    /// Class implementation of logger
    /// </summary>
    public class CLogger
    {
        private LogLevel logLevel;
        private Dictionary<LogLevel, Dictionary<Int32, StringBuilder>> timedEventLog;
        private Dictionary<LogLevel, StringBuilder> timedConsoleLog;
        private Dictionary<LogLevel, List<FileLogData>> timedFileLog;
        private Mutex fileM;
        private Mutex eventM;
        private Mutex consoleM;
        private DispatcherTimer timerConsole;
        private bool timerConsoleAdded;
        private DispatcherTimer timerEventLog;
        private bool timerEventLogAdded;
        private DispatcherTimer timerFileLog;
        private bool timerFileLogAdded;
        private UInt32 FileSize;

        private string FileName;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public CLogger()
        {
            timedEventLog = new Dictionary<LogLevel, Dictionary<Int32, StringBuilder>>();
            timedConsoleLog = new Dictionary<LogLevel, StringBuilder>();
            timedFileLog = new Dictionary<LogLevel, List<FileLogData>>();
            timerConsole = new DispatcherTimer();
            timerConsoleAdded = false;
            timerEventLog = new DispatcherTimer();
            timerEventLogAdded = false;
            timerFileLog = new DispatcherTimer();
            timerFileLogAdded = false;
            FileSize = 1000000;
            fileM = new Mutex(false, "FileMutex");
            eventM = new Mutex(false, "EventLogMutex");
            consoleM = new Mutex(false, "ConsoleMutex");
        }

        /// <summary>
        /// Constructor with Log Level
        /// </summary>
        /// <param name="loglevelP">Log Level</param>
        public CLogger(LogLevel loglevelP)
        {
            timedEventLog = new Dictionary<LogLevel, Dictionary<Int32, StringBuilder>>();
            timedConsoleLog = new Dictionary<LogLevel, StringBuilder>();
            timedFileLog = new Dictionary<LogLevel, List<FileLogData>>();
            timerConsole = new DispatcherTimer();
            timerConsoleAdded = false;
            timerEventLog = new DispatcherTimer();
            timerEventLogAdded = false;
            timerFileLog = new DispatcherTimer();
            timerFileLogAdded = false;
            FileSize = 1000000;
            logLevel = loglevelP;
            fileM = new Mutex(false, "FileMutex");
            eventM = new Mutex(false, "EventLogMutex");
            consoleM = new Mutex(false, "ConsoleMutex");
        }

        /// <summary>
        /// File Constructor
        /// </summary>
        /// <param name="FileNameP">File Name</param>
        /// <param name="FileSizeP">File Size</param>
        public CLogger(String FileNameP, UInt32 FileSizeP)
        {
            timedEventLog = new Dictionary<LogLevel, Dictionary<Int32, StringBuilder>>();
            timedConsoleLog = new Dictionary<LogLevel, StringBuilder>();
            timedFileLog = new Dictionary<LogLevel, List<FileLogData>>();
            timerConsole = new DispatcherTimer();
            timerConsoleAdded = false;
            timerEventLog = new DispatcherTimer();
            timerEventLogAdded = false;
            timerFileLog = new DispatcherTimer();
            timerFileLogAdded = false;
            FileName = FileNameP;
            FileSize = FileSizeP;
            fileM = new Mutex(false, "FileMutex");
            eventM = new Mutex(false, "EventLogMutex");
            consoleM = new Mutex(false, "ConsoleMutex");
        }

        /// <summary>
        /// File Constructor with Log Level
        /// </summary>
        /// <param name="loglevelP">Log Level</param>
        /// <param name="FileNameP">File Name</param>
        /// <param name="FileSizeP">File Size</param>
        public CLogger(LogLevel loglevelP, String FileNameP, UInt32 FileSizeP)
        {
            timedEventLog = new Dictionary<LogLevel, Dictionary<Int32, StringBuilder>>();
            timedConsoleLog = new Dictionary<LogLevel, StringBuilder>();
            timedFileLog = new Dictionary<LogLevel, List<FileLogData>>();
            timerConsole = new DispatcherTimer();
            timerConsoleAdded = false;
            timerEventLog = new DispatcherTimer();
            timerEventLogAdded = false;
            timerFileLog = new DispatcherTimer();
            timerFileLogAdded = false;
            logLevel = loglevelP;
            FileName = FileNameP;
            FileSize = FileSizeP;
            fileM = new Mutex(false, "FileMutex");
            eventM = new Mutex(false, "EventLogMutex");
            consoleM = new Mutex(false, "ConsoleMutex");
        }

        /// <summary>
        /// This function is use to set log level.
        /// Logs that higher level than your set will not be logged.
        /// </summary>
        /// <param name="level">Log Level</param>
        public void SetLogLevel(LogLevel level)
        {
            logLevel = level;
        }

        /// <summary>
        /// This function sets file to log
        /// </summary>
        /// <param name="filename">File Name</param>
        public void SetLogFile(string filename)
        {
            FileName = filename;
        }

        /// <summary>
        /// This function sets file size of the log file
        /// </summary>
        /// <param name="filesize">Size in bytes</param>
        public void SetLogFileSize(UInt32 filesize)
        {
            FileSize = filesize;
        }

        /// <summary>
        /// This function logs event based on type.
        /// </summary>
        /// <param name="type">Type of the log</param>
        /// <param name="level">Log level of the log</param>
        /// <param name="text">Text</param>
        /// <param name="p">This parameter is only used on EventLog type, it gives an id to event</param>
        public void Log(LogType type, LogLevel level, string text, params int[] p)
        {
            if ((logLevel & level) > 0)
            {
                switch (type)
                {
                    case LogType.CONSOLE:
                        {
#if !NETFX_CORE
                            Console.Write(Assembly.GetCallingAssembly().GetName().Name + ":"
                                + level + " " + text);
#else
                            System.Diagnostics.Debug.WriteLine(level + " " + text);
#endif
                        } break;
                    case LogType.EVENTLOG:
                        {
                            
                        } break;
                    case LogType.FILE:
                        {
#if !NETFX_CORE
                            FileStream f = new FileStream(FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                            StreamWriter sw = new StreamWriter(f);

                            if (f.Length >= 1000000)
                            {
                                f.SetLength(0);
                                sw.BaseStream.Seek(0, SeekOrigin.Begin);
                            }
                            else
                            {
                                sw.BaseStream.Seek(0, SeekOrigin.End);
                            }

                            sw.WriteLine(DateTime.Now.ToString() + ":Resto:"
                                + level + ":" + text);
                            sw.Flush();

                            sw.Close();
                            f.Close();
#endif
                        } break;
                };
            }
        }

        /// <summary>
        /// This function logs a line, only works on Console Type
        /// </summary>
        /// <param name="level">Log level of the log</param>
        /// <param name="text">Text</param>
        public void LogLine(LogLevel level, string text)
        {
            if ((logLevel & level) > 0)
            {
#if !NETFX_CORE
                Console.WriteLine(Assembly.GetCallingAssembly().GetName().Name + ":"
                    + level + " " + text);
#else
                Console.WriteLine(level + " " + text);
#endif  
            }
        }

        /// <summary>
        /// This function logs a line, only works on Console Type
        /// </summary>
        public void LogLine()
        {
            Console.WriteLine();
        }

        /// <summary>
        /// This function logs after some time, till log all texts that send will be saved as lines.
        /// You can set interval to log.
        /// </summary>
        /// <param name="type">Type of the log</param>
        /// <param name="level">Log level of the log</param>
        /// <param name="text">Text</param>
        /// <param name="p">This parameter is only used on EventLog type, it gives an id to event</param>
        public void LogTimed(LogType type, LogLevel level, string text, params int[] p)
        {
            if ((logLevel & level) == 0)
                return;

            switch (type)
            {
                case LogType.CONSOLE:
                    {
#if !NETFX_CORE
                        consoleM.WaitOne(60000);
                        if (!timedConsoleLog.ContainsKey(level))
                            timedConsoleLog.Add(level, new StringBuilder());

                        if (timedConsoleLog[level].ToString() == "")
                        {
                            timerConsole.Start();
                            if (!timerConsoleAdded)
                            {
                                timerConsole.Tick += new EventHandler(ConsoleTimerLog);
                                timerConsoleAdded = true;
                            }
                        }

                        timedConsoleLog[level].AppendLine(DateTime.Now.ToString() + ":" + level + ":" + Assembly.GetExecutingAssembly().GetName().Name + ":" + text);
                        consoleM.ReleaseMutex();
#endif
                    } break;
                case LogType.EVENTLOG:
                    {
                    } break;
                case LogType.FILE:
                    {
#if !NETFX_CORE
                        fileM.WaitOne(60000);
                        if (!timedFileLog.ContainsKey(level))
                            timedFileLog.Add(level, new List<FileLogData>());

                        if (timedFileLog[level].Count == 0)
                        {
                            timerFileLog.Start();
                            if (!timerFileLogAdded)
                            {
                                timerFileLog.Tick += new EventHandler(FileTimerLog);
                                timerFileLogAdded = true;
                            }
                        }

                        FileLogData fld = new FileLogData();
                        fld.level = level;
                        fld.time = DateTime.Now;
                        fld.data = Assembly.GetExecutingAssembly().GetName().Name + ":" + text;
                        timedFileLog[level].Add(fld);
                        //timedFileLog[level].AppendLine(DateTime.Now.ToString() + ":" + level + ":" + Assembly.GetExecutingAssembly().GetName().Name + ":" + text);
                        fileM.ReleaseMutex();
#endif
                    } break;
            };

        }

        /// <summary>
        /// This function sets timed log interval
        /// </summary>
        /// <param name="type">Type of the log</param>
        /// <param name="interval">Interval in miliseconds</param>
        public void SetTimerInterval(LogType type, UInt32 interval)
        {
            switch (type)
            {
                case LogType.CONSOLE:
                    {
                        timerConsole.Interval = new TimeSpan(interval);
                    } break;
                case LogType.EVENTLOG:
                    {
                        timerEventLog.Interval = new TimeSpan(interval);
                    } break;
                case LogType.FILE:
                    {
                        timerFileLog.Interval = new TimeSpan(interval);
                    } break;
            };
        }

        private void ConsoleTimerLog(object sender, EventArgs e)
        {
            consoleM.WaitOne(60000);
            for (LogLevel i = LogLevel.INFORM; i <= logLevel; i++)
            {
                if (timedConsoleLog.ContainsKey(i))
                {
                    if (timedConsoleLog[i].ToString() != "")
                    {
                        Console.Write(timedConsoleLog[i]);
                        timedConsoleLog[i].Remove(0, timedConsoleLog[i].Length);
                    }
                }
            }
            timerConsole.Stop();
            consoleM.ReleaseMutex();
        }

        private void EventTimerLog(object sender, EventArgs e)
        {
            
        }

        private void FileTimerLog(object sender, EventArgs e)
        {
#if !NETFX_CORE
            fileM.WaitOne(60000);
            FileStream f = new FileStream(FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            StreamWriter sw = new StreamWriter(f);

            if (f.Length >= 1000000)
            {
                f.SetLength(0);
                sw.BaseStream.Seek(0, SeekOrigin.Begin);
            }
            else
            {
                sw.BaseStream.Seek(0, SeekOrigin.End);
            }

            List<FileLogData> temp = new List<FileLogData>();

            for (LogLevel i = LogLevel.INFORM; i <= logLevel; i++)
            {
                if (timedFileLog.ContainsKey(i))
                {
                    if (timedFileLog[i].Count != 0)
                    {
                        foreach (FileLogData fld in timedFileLog[i])
                            temp.Add(fld);

                        timedFileLog[i].Clear();
                    }
                }
            }

            temp.Sort(ListSortCompare);

            foreach (FileLogData fld in temp)
            {
                sw.WriteLine(fld.time + ":" + fld.level + ":" + fld.data);
            }

            temp.Clear();

            sw.Flush();
            sw.Close();
            f.Close();

            timerFileLog.Stop();
            fileM.ReleaseMutex();
#endif
        }

        /// <summary>
        /// Comparer
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int ListSortCompare(FileLogData x, FileLogData y)
        {
            /*if (x == null)
            {
                if (y == null)
                    return 0;
                else
                    return -1;
            }
            else
            {
                if (y == null)
                    return 1;
                else
                {*/
            if (x.time < y.time)
                return -1;
            else if (x.time > y.time)
                return 1;
            else
                return 0;
            /*        }
                }*/
        }
    }
}
