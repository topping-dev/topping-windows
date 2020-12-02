using System;
using System.Net;
using System.Windows;
using ScriptingEngine.LuaUI;
using SQLiteClient;
using LuaScriptingEngine;
using System.Windows.Input;
#if !NETFX_CORE
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
#else
using Windows.UI.Xaml.Controls;
#endif

namespace ScriptingEngine
{
    [LuaClass("LuaDatabase")]
    public class LuaDatabase : LuaInterface
    {
        DatabaseHelper db;

         /**
	     * Creates LuaDatabase Object From Lua.
	     * @return LuaDatabase
	     */
	    [LuaFunction(typeof(LuaContext))]
        public static LuaDatabase Create(LuaContext context)
        {
            LuaDatabase db = new LuaDatabase();
            db.db = new DatabaseHelper();
            return db;
        }
	
	    /**
	     * Checks and Creates Database File on Storage.
	     */
        [LuaFunction(false)]
	    public void CheckAndCreateDatabase()
	    {
		    try
		    {
			    db.CreateDatabase(false);
		    } 
		    catch (Exception e)
		    {
		    }
	    }
	
	    /**
	     * Opens connection to database.
	     * @return LuaObjectStore of connection.
	     */
	    [LuaFunction(false)]
	    public LuaObjectStore Open()
	    {
		    db.Open();
		    return new LuaObjectStore();
	    }
	
	    /**
	     * Send sql query to connection.
	     * @param conn object store of connection
	     * @param str sql statement string
	     * @return LuaObjectStore of statement.
	     */
	    [LuaFunction(typeof(LuaObjectStore), typeof(String))]
	    public LuaObjectStore Query(LuaObjectStore conn, String str)
	    {
		    LuaObjectStore los = new LuaObjectStore();
		    los.obj = db.Query(str, false);
		    return los;
	    }
	
	    /**
	     * Send sql query to connection for insert,update operations.
	     * @param conn object store of connection
	     * @param str sql statement string
	     * @return LuaObjectStore of statement.
	     */
	    [LuaFunction(typeof(LuaObjectStore), typeof(String))]
	    public LuaObjectStore Insert(LuaObjectStore conn, String str)
	    {
		    LuaObjectStore los = new LuaObjectStore();
		    los.obj = db.Query(str, true);
		    return los;
	    }
	
	    /**
	     * Finalize statement.
	     * @param LuaObjectStore of statement.
	     */
        [LuaFunction(typeof(LuaObjectStore))]
	    public void Finalize(LuaObjectStore stmt)
	    {
		    SQLiteDataReader c = (SQLiteDataReader)stmt.obj;
		    c.Close();
	    }
	
	    /**
	     * Finalize statement.
	     * @param LuaObjectStore of connection.
	     */
	    [LuaFunction(typeof(LuaObjectStore))]
	    public void Close(LuaObjectStore conn)
	    {
		    db.Close();
	    }
	
	    /**
	     * Get Int32 value at column
	     * @param stmt statement object
	     * @param column column
	     * @return Int32 value
	     */
	    [LuaFunction(typeof(LuaObjectStore), typeof(Int32))]
	    public Int32 GetInt(LuaObjectStore stmt, Int32 column)
	    {
		    return db.GetInt((SQLiteDataReader)stmt.obj, column);
	    }
	
	    /**
	     * Get Float value at column
	     * @param stmt statement object
	     * @param column column
	     * @return Float value
	     */
	    [LuaFunction(typeof(LuaObjectStore), typeof(Int32))]
	    public Single GetFloat(LuaObjectStore stmt, Int32 column)
	    {
		    return (Single)db.GetDouble((SQLiteDataReader)stmt.obj, column);
	    }
	
	    /**
	     * Get String value at column
	     * @param stmt statement object
	     * @param column column
	     * @return String value
	     */
	    [LuaFunction(typeof(LuaObjectStore), typeof(Int32))]
	    public String GetString(LuaObjectStore stmt, Int32 column)
	    {
		    return db.GetString((SQLiteDataReader)stmt.obj, column);
	    }
	
	    /**
	     * Get Double value at column
	     * @param stmt statement object
	     * @param column column
	     * @return Double value
	     */
	    [LuaFunction(typeof(LuaObjectStore), typeof(Int32))]
	    public Double GetDouble(LuaObjectStore stmt, Int32 column)
	    {
		    return db.GetDouble((SQLiteDataReader)stmt.obj, column);
	    }
	
	    /**
	     * Get Long value at column
	     * @param stmt statement object
	     * @param column column
	     * @return Long value
	     */
	    [LuaFunction(typeof(LuaObjectStore), typeof(Int32))]
	    public Int64 GetLong(LuaObjectStore stmt, Int32 column)
	    {
		    return db.GetLong((SQLiteDataReader)stmt.obj, column);
	    }
	
	    /**
	     * Frees LuaDatabase.
	     */
	    [LuaFunction(false)]
	    public void Free()
	    {
		
	    }

        #region LuaInterface Members

        public void RegisterEventFunction(string var, LuaTranslator lt)
        {
        }

        public string GetId()
        {
            return "LuaDatabase";
        }

        #endregion
    }
}
