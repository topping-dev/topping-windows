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
#if !NETFX_CORE
using System.Windows.Threading;
#else
using Windows.UI.Xaml;
#endif

namespace LoggerNamespace
{
    /// <summary>
    /// Log Type enum
    /// </summary>
    public enum LogType
    {
        /// <summary>
        /// Console log type
        /// </summary>
        CONSOLE,
        /// <summary>
        /// EventLog log type
        /// </summary>
        EVENTLOG,
        /// <summary>
        /// File log type
        /// <see cref="Logger.SetLogFile"/>
        /// <see cref="Logger.SetLogFileSize"/>
        /// </summary>
        FILE
    }

    /// <summary>
    /// Log Level enum
    /// </summary>
    public enum LogLevel : int
    {
        /// <summary>
        /// Log Level None
        /// </summary>
        NONE,
        /// <summary>
        /// Log Level Inform
        /// </summary>
        INFORM = 0x1,
        /// <summary>
        /// Log Level Warning
        /// </summary>
        WARN = 0x2,
        /// <summary>
        /// Log Level Error
        /// </summary>
        ERROR = 0x4,
        /// <summary>
        /// Log Level Debug
        /// </summary>
        DEBUG = 0x8
    }

    /// <summary>
    /// Internal Struct
    /// </summary>
    public struct FileLogData
    {
        /// <summary>
        /// 
        /// </summary>
        public LogLevel level;
        /// <summary>
        /// 
        /// </summary>
        public DateTime time;
        /// <summary>
        /// 
        /// </summary>
        public String data;
    }

    /// <summary>
    /// This class is Logger. I logs to EventLog,Console or File based on your choice.
    /// Before starting to log, dont forget to set loglevel <see cref="Logger.SetLogLevel"/>.
    /// If you are going to log to a file dont forget to set file to log <see cref="Logger.SetLogFile"/>
    /// and filesize of log file <see cref="Logger.SetLogFileSize"/>
    /// 
    /// Logger divides into two partions, a static class and abstract class.
    /// You can use both of them in your needs <see cref="ALogger"/>
    /// </summary>
    public class Logger
    {
        private static Int32 logLevel;
        private static Dictionary<LogLevel, Dictionary<Int32, StringBuilder>> timedEventLog = new Dictionary<LogLevel, Dictionary<Int32, StringBuilder>>();
        private static Dictionary<LogLevel, StringBuilder> timedConsoleLog = new Dictionary<LogLevel, StringBuilder>();
        private static Dictionary<LogLevel, List<FileLogData>> timedFileLog = new Dictionary<LogLevel, List<FileLogData>>();
        private static DispatcherTimer timerConsole = new DispatcherTimer();
        private static bool timerConsoleAdded = false;
        private static DispatcherTimer timerEventLog = new DispatcherTimer();
        private static bool timerEventLogAdded = false;
        private static DispatcherTimer timerFileLog = new DispatcherTimer();
        private static bool timerFileLogAdded = false;
        private static UInt32 FileSize = 1000000;

        private static string FileName;

        /// <summary>
        /// This function is use to set log level.
        /// Logs that higher level than your set will not be logged.
        /// </summary>
        /// <param name="level">Log Level</param>
        public static void SetLogLevel(Int32 level)
        {
            logLevel = level;
        }

        /// <summary>
        /// This function sets file to log
        /// </summary>
        /// <param name="filename">File Name</param>
        public static void SetLogFile(string filename)
        {
            FileName = filename;
        }

        /// <summary>
        /// This function sets file size of the log file
        /// </summary>
        /// <param name="filesize">Size in bytes</param>
        public static void SetLogFileSize(UInt32 filesize)
        {
            FileSize = filesize;
        }

        /// <summary>
        /// This static function logs event based on type.
        /// </summary>
        /// <param name="type">Type of the log</param>
        /// <param name="level">Log level of the log</param>
        /// <param name="text">Text</param>
        /// <param name="p">This parameter is only used on EventLog type, it gives an id to event</param>
        public static void Log(LogType type, LogLevel level, string text, params int[] p)
        {
            if ((logLevel & (Int32)level) > 0)
            {
                switch (type)
                {
                    case LogType.CONSOLE:
                        {
                            Debug.WriteLine(level.ToString() + " : " + text);
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

                            sw.WriteLine(DateTime.Now.ToString() + ":" + "Resto" + ":"
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
        /// This static function logs a line, only works on Console Type
        /// </summary>
        /// <param name="level">Log level of the log</param>
        /// <param name="text">Text</param>
        public static void LogLine(LogLevel level, string text)
        {
            if ((logLevel & (Int32)level) > 0)
            {
                Console.WriteLine("Resto:"
                    + level + " " + text);
            }
        }

        /// <summary>
        /// This static function logs a line, only works on Console Type
        /// </summary>
        public static void LogLine()
        {
            Console.WriteLine();
        }

        /// <summary>
        /// This static function logs after some time, till log all texts that send will be saved as lines.
        /// You can set interval to log.
        /// </summary>
        /// <param name="type">Type of the log</param>
        /// <param name="level">Log level of the log</param>
        /// <param name="text">Text</param>
        /// <param name="p">This parameter is only used on EventLog type, it gives an id to event</param>
        public static void LogTimed(LogType type, LogLevel level, string text, params int[] p)
        {
            if ((logLevel & (Int32)level) == 0)
                return;

            switch (type)
            {
                case LogType.CONSOLE:
                    {
#if !NETFX_CORE
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

                        timedConsoleLog[level].Append(DateTime.Now.ToString() + ":" + level + ":" + "Resto" + ":" + text + "\r\n");
#endif
                    } break;
                case LogType.EVENTLOG:
                    {
                    } break;
                case LogType.FILE:
                    {
#if !NETFX_CORE
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
                        fld.data = "Resto" + ":" + text;
                        timedFileLog[level].Add(fld);
                        //timedFleLog[level].AppendLine(DateTime.Now.ToString() + ":" + level + ":" + "Resto" + ":" + text);
#endif
                    } break;
            };

        }

        /// <summary>
        /// This static function sets timed log interval
        /// </summary>
        /// <param name="type">Type of the log</param>
        /// <param name="interval">Interval in miliseconds</param>
        public static void SetTimerInterval(LogType type, Int32 interval)
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

        private static void ConsoleTimerLog(object sender, EventArgs e)
        {
            for (LogLevel i = LogLevel.INFORM; i <= LogLevel.DEBUG; i++)
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
        }

        private static void EventTimerLog(object sender, EventArgs e)
        {
        }

        private static void FileTimerLog(object sender, EventArgs e)
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

            List<FileLogData> temp = new List<FileLogData>();

            for (LogLevel i = LogLevel.INFORM; i <= LogLevel.DEBUG; i++)
            {
                if (timedFileLog.ContainsKey(i))
                {
                    if (timedFileLog[i].Count != 0)
                    {
                        foreach(FileLogData fld in timedFileLog[i])
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
#endif
        }

        /// <summary>
        /// Comparer
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static int ListSortCompare(FileLogData x, FileLogData y)
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
