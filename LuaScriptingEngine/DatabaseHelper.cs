using System;
using System.Net;
using System.Windows;
using System.IO;
using System.Windows.Input;
#if !NETFX_CORE
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Resources;
using SQLiteClient;
using Community.CsharpSqlite;
#else
using SQLite;
using SQLiteClient;
#endif
#if WINDOWS_PHONE
using System.IO.IsolatedStorage;
#endif

namespace LuaScriptingEngine
{
    public class DatabaseHelper
    {
        public static String DB_FILE = "mydb.sqlite";

        public void CreateDatabase(bool force)
        {
#if WINDOWS_PHONE
            StreamResourceInfo sr = Application.GetResourceStream(new Uri(DB_FILE, UriKind.Relative));

            IsolatedStorageFile iStorage = IsolatedStorageFile.GetUserStoreForApplication();

            bool delete = false;
            if ((delete = (force && iStorage.FileExists(DB_FILE))) || !iStorage.FileExists(DB_FILE))
            {
                if (delete)
                    iStorage.DeleteFile(DB_FILE);
                using (var outputStream = iStorage.OpenFile(DB_FILE, FileMode.CreateNew))
                {
                    byte[] buffer = new byte[10000];

                    for (; ; )
                    {
                        int read = sr.Stream.Read(buffer, 0, buffer.Length);

                        if (read <= 0)
                            break;

                        outputStream.Write(buffer, 0, read);
                    }
                }
            }
#endif
        }

#if WINDOWS_PHONE
        Sqlite3 db;
        SQLiteConnection conn;
#elif NETFX_CORE
        SQLiteClient.SQLiteConnection conn;
#endif
        
        public void Open()
        {
            conn = new
#if !WINDOWS_PHONE
            SQLiteClient.
#endif
            SQLiteConnection("asd");
            conn.Open();
             

        }

        public SQLiteDataReader Query(String query, bool insert)
        {
#if NETFX_CORE
            SQLiteClient.
#endif
            SQLiteCommand cmd = conn.CreateCommand(query);
            if (insert)
            {
                cmd.ExecuteNonQuery();
                return new SQLiteDataReader(cmd);
            }
            else
            {
                return cmd.ExecuteReader();
            }
        }

        public bool Read(SQLiteDataReader reader)
        {
            return reader.Read();
        }

        public void Close()
        {
            conn.Dispose();
        }

        public Int32 GetInt(SQLiteDataReader reader, Int32 column)
        {
            return reader.GetInt(column);
        }

        public Int64 GetLong(SQLiteDataReader reader, Int32 column)
        {
            return reader.GetLong(column);
        }

        public String GetString(SQLiteDataReader reader, Int32 column)
        {
            return reader.GetString(column);
        }

        public Double GetDouble(SQLiteDataReader reader, Int32 column)
        {
            return reader.GetDouble(column);
        }
    }
}
