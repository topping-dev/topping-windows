using System;
using System.Net;
using System.Windows;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Input;
using SQLite;

namespace SQLiteClient
{     
    public class SQLiteException : Exception
    {
        private SQLite3.Result _errorCode;
        public SQLite3.Result ErrorCode
        {
            get { return _errorCode; }
            set { _errorCode = value; }
        }
        public SQLiteException(SQLite3.Result errorCode, string message)
            : base(message)
        {
            _errorCode = errorCode;
        }
    }

    public class SQLiteDataReader
    {
        private SQLiteCommand cmd;
        private SQLiteCommand sQLiteCommand;
        private IntPtr stmt;

        public SQLiteDataReader(SQLiteCommand cmd)
        {
            this.cmd = cmd;
        }

        public SQLiteDataReader(SQLiteCommand sQLiteCommand, IntPtr stmt)
        {
            // TODO: Complete member initialization
            this.cmd = sQLiteCommand;
            this.stmt = stmt;
        }

        public bool Read()
        {
            if (SQLite3.Step(stmt) == SQLite3.Result.Row)
                return true;
            else
                return false;
        }

        public Int32 GetInt(Int32 column)
        {
            return (Int32)cmd.ReadCol(stmt, column, typeof(Int32));
        }

        public Int64 GetLong(Int32 column)
        {
            return (Int64)cmd.ReadCol(stmt, column, typeof(Int64));
        }

        public String GetString(Int32 column)
        {
            return (String)cmd.ReadCol(stmt, column, typeof(String));
        }

        public Double GetDouble(Int32 column)
        {
            return (Double)cmd.ReadCol(stmt, column, typeof(Double));
        }

        public void Close()
        {
            if (stmt != null)
                SQLite3.Finalize(stmt);
        }
    }

    public class SQLiteConnection : IDisposable
    {
        private IntPtr _db;
        private bool _open;
        public string callbackError;
      
        public string Database { get; set; }

        public bool TransactionOpened=false;

        public SQLiteConnection(string database)
        {
            Database = database;
        }

        string SQLiteLastError()
        {
            return SQLite3.GetErrmsg(_db);
        }

        int callback(object pArg, System.Int64 nArg, object azArgs, object azCols)
        {
            int i;
            string[] azArg = (string[])azArgs;
            string[] azCol = (string[])azCols;
            String sb = "";// = new String();
            for (i = 0; i < nArg; i++)
                sb += azCol[i] + " = " + azArg[i] + "\n";
            callbackError += sb.ToString();            
            return 0;
        }

        public void Open()
        {
            SQLite3.Result n = SQLite3.Open(Database, out _db);
            if (n != SQLite3.Result.OK)
                throw new SQLiteException(SQLite3.Result.OK, "Could not open database file: " + Database);            
            string errMsg = string.Empty;
            /*n = SQLite3.(_db, "PRAGMA journal_mode=PERSIST", (SQLite3.dx)this.callback, null, ref errMsg);
            if (n != SQLite3.SQLITE_OK)
            {
                SQLite3.SQLite3_close(_db);
                _db = null;
                _open = false;
                throw new SQLiteException(n, "Cannot set journal mode to PERSIST: " + Database);
            }       */     
            _open = true;
        }
        public SQLiteCommand CreateCommand(string cmdText, params object[] ps)
        {
            if (!_open)
            {
                throw new SQLiteException( SQLite3.Result.Error, "Cannot create commands from unopened database");
            }
            else
            {
                var cmd = new SQLiteCommand(_db);
                cmd.CommandText = cmdText;
                foreach (var o in ps)
                {
                    cmd.Bind(o);
                }
                return cmd;
            }
        }
        public SQLiteCommand CreateCommand(string cmdText)
        {
            if (!_open)
            {
                throw new SQLiteException( SQLite3.Result.Error,"Cannot create commands from unopened database");
            }
            else
            {
                var cmd = new SQLiteCommand(_db);
                cmd.CommandText = cmdText;
                return cmd;
            }
        }

        public int Execute(string query, params object[] ps)
        {
            var cmd = CreateCommand(query, ps);
            return cmd.ExecuteNonQuery();
        }

        public int Execute(IntPtr db, String val, int a, int b, int c)
        {
            var r = SQLite3.Result.OK;
            var stmt = SQLite3.Prepare2(_db, val);
            r = SQLite3.Step(stmt);
            SQLite3.Finalize(stmt);
            if (r == SQLite3.Result.Done)
            {
                int rowsAffected = SQLite3.Changes(_db);
                return rowsAffected;
            }
            else if (r == SQLite3.Result.Error)
            {
                string msg = SQLite3.GetErrmsg(_db);
                throw new SQLiteException(r, msg);
            }
            else
            {
                throw new SQLiteException(r, r.ToString());
            }
        }

        public bool BeginTransaction()
        {     
            int n = Execute(_db, "BEGIN", 0, 0, 0) ;
            if (((SQLite3.Result)n) != SQLite3.Result.OK)
                throw new SQLiteException((SQLite3.Result)n, SQLiteLastError());
            TransactionOpened = true;
            return true;        
        }

        public bool RollbackTransaction()
        {
            int n = Execute(_db, "ROLLBACK", 0, 0, 0);
            if (((SQLite3.Result)n) != SQLite3.Result.OK)
                throw new SQLiteException((SQLite3.Result)n, SQLiteLastError());
            TransactionOpened = false;
            return true;
        }

        public bool CommitTransaction()
        {
            int n =Execute(_db, "COMMIT", 0, 0, 0);
            if (((SQLite3.Result)n) != SQLite3.Result.OK)
                throw new SQLiteException((SQLite3.Result)n, SQLiteLastError());
            TransactionOpened = false;
            return true;
        }


        public IEnumerable<T> Query<T>(string query, params object[] ps) where T : new()
        {
            var cmd = CreateCommand(query, ps);
            return cmd.ExecuteQuery<T>();
        }

        public void Dispose()
        {
            if (_open)
            {
                SQLite3.Close(_db);
                _open = false;
            }
        }
    }

    public class SQLiteCommand
    {
        private IntPtr _db;
        private List<Binding> _bindings;
        public string CommandText { get; set; }

        private static string[] _datetimeFormats = new string[] {
      "THHmmss",
      "THHmm",
      "HH:mm:ss",
      "HH:mm",
      "HH:mm:ss.FFFFFFF",
      "yy-MM-dd",
      "yyyy-MM-dd",
      "yyyy-MM-dd HH:mm:ss.FFFFFFF",
      "yyyy-MM-dd HH:mm:ss",
      "yyyy-MM-dd HH:mm",                               
      "yyyy-MM-ddTHH:mm:ss.FFFFFFF",
      "yyyy-MM-ddTHH:mm",
      "yyyy-MM-ddTHH:mm:ss",
      "yyyyMMddHHmmss",
      "yyyyMMddHHmm",
      "yyyyMMddTHHmmssFFFFFFF",
      "yyyyMMdd"
    };

        internal SQLiteCommand(IntPtr db)
        {
            _db = db;
            _bindings = new List<Binding>();
            CommandText = "";
        }

        public int Execute(IntPtr db, String val, int a, int b, int c)
        {
            var r = SQLite3.Result.OK;
            var stmt = SQLite3.Prepare2(_db, val);
            r = SQLite3.Step(stmt);
            SQLite3.Finalize(stmt);
            if (r == SQLite3.Result.Done)
            {
                int rowsAffected = SQLite3.Changes(_db);
                return rowsAffected;
            }
            else if (r == SQLite3.Result.Error)
            {
                string msg = SQLite3.GetErrmsg(_db);
                throw new SQLiteException(r, msg);
            }
            else
            {
                throw new SQLiteException(r, r.ToString());
            }
        }

        public int ExecuteNonQueryEx()
        {
            int n=Execute(_db, CommandText, 0, 0, 0);
            if (((SQLite3.Result)n) != SQLite3.Result.OK)
                throw new SQLiteException((SQLite3.Result)n, SQLiteLastError());
            return 1;
        }

        public Object ExecuteScalar()
        {
            Object retObj = null;
            var stmt = Prepare();
            var cols = new System.Reflection.PropertyInfo[SQLite3.ColumnCount(stmt)];
            if (SQLite3.Step(stmt) == SQLite3.Result.Row)
            {
                if (cols.Length > 0)
                {
                    var type = SQLite3.ColumnType(stmt, 0);
                    switch(type)
                    {
                        case SQLite3.ColType.Integer:
                            retObj = SQLite3.ColumnInt64(stmt, 0);
                            break;
                        case SQLite3.ColType.Float:
                            retObj = SQLite3.ColumnDouble(stmt, 0);
                            break;
                        case SQLite3.ColType.Blob:
                            retObj = SQLite3.ColumnByteArray(stmt, 0);
                            break;
                        case SQLite3.ColType.Text:
                            retObj=SQLite3.ColumnString(stmt, 0);
                            break;
                        case SQLite3.ColType.Null:
                            break;
                    }
                }
            }
            SQLite3.Finalize(stmt);
            return retObj;
        }

        public int ExecuteNonQuery()
        {
            IntPtr stmt = Prepare();            
            var r = SQLite3.Step(stmt);
            switch (r)
            {
                case SQLite3.Result.Error:
                    string msg = SQLite3.GetErrmsg(_db);
                    SQLite3.Finalize(stmt);                
                    throw new SQLiteException(r,msg);                    
                case SQLite3.Result.Done:
                    int rowsAffected = SQLite3.Changes(_db);
                    SQLite3.Finalize(stmt);  
                    return rowsAffected;                    
                case SQLite3.Result.CannotOpen:                   
                    SQLite3.Finalize(stmt);                
                    throw new SQLiteException(r,"Cannot open database file");                    
                case SQLite3.Result.Constraint:
                    string msgC = SQLite3.GetErrmsg(_db);
                    SQLite3.Finalize(stmt);
                    throw new SQLiteException(r,msgC);                    
                default:
                     SQLite3.Finalize(stmt);  
                     throw new SQLiteException(r,"Unknown error");                     
            }           
        }

        public int ExecuteNonQuery<T>(T toInsert)
        {
            IntPtr stmt;
            SQLite3.Result n = SQLite3.Prepare2(_db, CommandText, CommandText.Length, out stmt, IntPtr.Zero);
            if (n != SQLite3.Result.OK)
                throw new SQLiteException(n, SQLiteLastError());

            var props = GetProps(typeof(T));
            var ncols=SQLite3.ColumnCount(stmt);
            var cols = new System.Reflection.PropertyInfo[ncols];

            _bindings.Clear();
            for (int i = 0; i < props.Length; i++)
                Bind("@"+props[i].Name, props[i].GetValue(toInsert, null));
            BindAll(stmt);
            var r = SQLite3.Step(stmt);
            switch (r)
            {
                case SQLite3.Result.Error:
                    string msg = SQLite3.GetErrmsg(_db);
                    SQLite3.Finalize(stmt);
                    throw new SQLiteException(r, msg);
                case SQLite3.Result.Done:
                    int rowsAffected = SQLite3.Changes(_db);
                    SQLite3.Finalize(stmt);
                    return rowsAffected;
                case SQLite3.Result.CannotOpen:
                    SQLite3.Finalize(stmt);
                    throw new SQLiteException(r, "Cannot open database file");
                case SQLite3.Result.Constraint:
                    string msgC = SQLite3.GetErrmsg(_db);
                    SQLite3.Finalize(stmt);
                    throw new SQLiteException(r, msgC);
                default:
                    SQLite3.Finalize(stmt);
                    throw new SQLiteException(r, "Unknown error");
            }        
        }

        public IEnumerable<T> ExecuteQuery<T>() where T : new()
        {
            var stmt = Prepare();

            var props = GetProps(typeof(T));
            var cols = new System.Reflection.PropertyInfo[SQLite3.ColumnCount(stmt)];
            for (int i = 0; i < cols.Length; i++)
                cols[i] = MatchColProp( SQLite3.ColumnName16(stmt, i), props);         
            while (SQLite3.Step(stmt) == SQLite3.Result.Row)
            {
                var obj = new T();
                for (int i = 0; i < cols.Length; i++)
                {
                    if (cols[i] == null)
                        continue;
                    var val = ReadCol(stmt, i, cols[i].PropertyType);
                    cols[i].SetValue(obj, val, null);
                }
                yield return obj;
            }
            SQLite3.Finalize(stmt);
        }

        public void Bind(string name, object val)
        {
            _bindings.Add(new Binding
            {
                Name = name,
                Value = val
            });
        }
        public void Bind(object val)
        {
            Bind(null, val);
        }
        public override string ToString()
        {
            return CommandText;
        }

        public IntPtr Prepare()
        {
            IntPtr ppStmt = IntPtr.Zero;
            SQLite3.Result n=SQLite3.Prepare2(_db, CommandText, CommandText.Length, out ppStmt, IntPtr.Zero);
            if ( n!= SQLite3.Result.OK)
                throw new SQLiteException(n,SQLiteLastError());
            BindAll(ppStmt);
            return ppStmt;
        }

        SQLiteDateFormats dateFormat = (SQLiteDateFormats)Enum.Parse(typeof(SQLiteDateFormats),"ISO8601", true);

        public enum SQLiteDateFormats
        {
            /// <summary>
            /// Using ticks is not recommended and is not well supported with LINQ.
            /// </summary>
            Ticks = 0,
            /// <summary>
            /// The default format for this provider.
            /// </summary>
            ISO8601 = 1,
            /// <summary>
            /// JulianDay format, which is what SQLite uses internally
            /// </summary>
            JulianDay = 2
        }
        public string DateToString(DateTime dateValue)
        {
            switch (dateFormat)
            {
                case SQLiteDateFormats.Ticks:
                    return dateValue.Ticks.ToString(CultureInfo.InvariantCulture);
                /*case SQLiteDateFormats.JulianDay:
                    return ToJulianDay(dateValue).ToString(CultureInfo.InvariantCulture);*/
                default:
                    return dateValue.ToString(_datetimeFormats[7], CultureInfo.InvariantCulture);
            }
        }
        /*public double ToJulianDay(DateTime value)
        {
            return value.ToOADate() + 2415018.5;
        }*/
        public DateTime ToDateTime(string dateText)
        {
            switch (dateFormat)
            {
                case SQLiteDateFormats.Ticks:
                    return new DateTime(Convert.ToInt64(dateText, CultureInfo.InvariantCulture));
                /*case SQLiteDateFormats.JulianDay:
                    return ToDateTime(Convert.ToDouble(dateText, CultureInfo.InvariantCulture));*/
                default:
                    return DateTime.ParseExact(dateText, _datetimeFormats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
            }
        }
        /*public DateTime ToDateTime(double julianDay)
        {
            return DateTime.FromOADate(julianDay - 2415018.5);
        }*/

        public static bool ToBoolean(object source)
        {
            if (source is bool) return (bool)source;

            return ToBoolean(source.ToString());
        }

        /// <summary>
        /// Convert a string to true or false.
        /// </summary>
        /// <param name="source">A string representing true or false</param>
        /// <returns></returns>
        /// <remarks>
        /// "yes", "no", "y", "n", "0", "1", "on", "off" as well as Boolean.FalseString and Boolean.TrueString will all be
        /// converted to a proper boolean value.
        /// </remarks>
        public static bool ToBoolean(string source)
        {
            if (String.Compare(source, bool.TrueString, StringComparison.OrdinalIgnoreCase) == 0) return true;
            else if (String.Compare(source, bool.FalseString, StringComparison.OrdinalIgnoreCase) == 0) return false;

            switch (source.ToLowerInvariant())
            {
                case "yes":
                case "y":
                case "1":
                case "on":
                    return true;
                case "no":
                case "n":
                case "0":
                case "off":
                    return false;
                default:
                    throw new ArgumentException("source");
            }
        }


        void BindAll(IntPtr stmt)
        {
            int nextIdx = 1;
            foreach (var b in _bindings)
            {
                if ((b.Name != null)&&(CommandText.IndexOf(b.Name)!=-1))
                    b.Index =  SQLite3.BindParameterIndex(stmt, b.Name);
                else
                    b.Index = nextIdx++;
            }
            for(int c=0;c<_bindings.Count;c++)
            {
                var b = _bindings[c];
                if (b.Value == null)
                {
                    int n = SQLite3.BindNull(stmt, b.Index);
                    if (n > 0) throw new SQLiteException((SQLite3.Result)n, SQLiteLastError());
                    continue;
                }
                if (b.Value is Byte || b.Value is UInt16 || b.Value is SByte || b.Value is Int16 || b.Value is Int32 || b.Value is Boolean)
                {
                    int n = SQLite3.BindInt(stmt, b.Index, Convert.ToInt32(b.Value,CultureInfo.InvariantCulture));
                    if (n > 0) throw new SQLiteException((SQLite3.Result)n, SQLiteLastError());
                    continue;
                }
                if (b.Value is UInt32 || b.Value is Int64)
                {
                    int n = SQLite3.BindInt64(stmt, b.Index, Convert.ToInt64(b.Value, CultureInfo.InvariantCulture));
                    if (n > 0) throw new SQLiteException((SQLite3.Result)n, SQLiteLastError());
                    continue;
                }
                if (b.Value is Single || b.Value is Double || b.Value is Decimal)
                {
                    int n = SQLite3.BindDouble(stmt, b.Index, Convert.ToDouble(b.Value, CultureInfo.InvariantCulture));
                    if (n > 0) throw new SQLiteException((SQLite3.Result)n, SQLiteLastError());
                    continue;
                }
                if (b.Value is String)
                {
                    int n = SQLite3.BindText(stmt, b.Index, b.Value.ToString(), -1, IntPtr.Zero);
                    if (n > 0) throw new SQLiteException((SQLite3.Result)n, SQLiteLastError());
                    continue;
                }
                if (b.Value is byte[])
                {
                    int n = SQLite3.BindBlob(stmt, b.Index, (byte[])b.Value, ((byte[])b.Value).Length, IntPtr.Zero);
                    if (n > 0) throw new SQLiteException((SQLite3.Result)n, SQLiteLastError());
                    continue;
                }
                if (b.Value is DateTime)
                {
                    int n = SQLite3.BindText(stmt, b.Index, DateToString((DateTime)b.Value), -1, IntPtr.Zero);
                    if (n > 0) throw new SQLiteException((SQLite3.Result)n, SQLiteLastError());
                    continue;
                }
            }
        }

        class Binding
        {
            public string Name { get; set; }
            public object Value { get; set; }
            public int Index { get; set; }
        }

        public object ReadCol(IntPtr stmt, int index, Type clrType)
        {
            var type = SQLite3.ColumnType(stmt, index);
            if (type == SQLite3.ColType.Null)
                return null;
            if (clrType == typeof(Byte) || clrType == typeof(UInt16) || clrType == typeof(SByte) || clrType == typeof(Int16) || clrType == typeof(Int32))
                return Convert.ChangeType(SQLite3.ColumnInt(stmt, index), clrType);
            if (clrType == typeof(UInt32) || clrType == typeof(Int64))
                return Convert.ChangeType(SQLite3.ColumnInt64(stmt, index), clrType);
            if (clrType == typeof(Single) || clrType == typeof(Double) || clrType == typeof(Decimal))
                return Convert.ChangeType(SQLite3.ColumnDouble(stmt, index), clrType);
            if (clrType == typeof(String))
                return Convert.ChangeType(SQLite3.ColumnString(stmt, index), clrType);
            if (clrType == typeof(byte[]))
                return SQLite3.ColumnByteArray(stmt, index);
            if (clrType == typeof(DateTime))
                return ToDateTime(SQLite3.ColumnString(stmt, index));
            if (clrType == typeof(Boolean))
                return ToBoolean(SQLite3.ColumnString(stmt, index));
            throw new NotSupportedException("Don't know how to read " + clrType);
               
        }
        static System.Reflection.PropertyInfo[] GetProps(Type t)
        {
            return t.GetProperties();
        }

        static System.Reflection.PropertyInfo MatchColProp(string colName, System.Reflection.PropertyInfo[] props)
        {
            foreach (var p in props)
            {
                if (p.Name == colName)
                {
                    return p;
                }
            }
            return null;
        }

        string SQLiteLastError()
        {
            return SQLite3.GetErrmsg(_db);
        }

        public SQLiteDataReader ExecuteReader()
        {
            IntPtr stmt = Prepare();
            SQLiteDataReader sdr = new SQLiteDataReader(this, stmt);
            return sdr;
        }
    }
}
