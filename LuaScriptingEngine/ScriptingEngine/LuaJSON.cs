using System;
using System.Net;
using System.Windows;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
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
    [LuaClass("LuaJSONArray")]
    public class LuaJSONArray : LuaInterface
    {
        public JArray jsa;

        /**
	     * Get array count.
	     * @return Int32 count of array.
	     */
	    [LuaFunction(false)]
	    public int Count()
	    {
		    return jsa.Count;
	    }
	
	    /**
	     * Get object value at index.
	     * @param index value.
	     * @return LuaJSONObject
	     */
	    [LuaFunction(typeof(Int32))]
	    public LuaJSONObject GetJSONObject(Int32 index)
	    {
		    LuaJSONObject lso = new LuaJSONObject();
            lso.jso = (JObject)jsa[index];
		    return lso;
	    }
	
	    /**
	     * Get array value at index.
	     * @param index value.
	     * @return LuaJSONArray
	     */
	    [LuaFunction(typeof(Int32))]
	    public LuaJSONArray GetJSONArray(Int32 index)
	    {
		    LuaJSONArray lsa = new LuaJSONArray();
		    lsa.jsa = (JArray)jsa[index];
		    return lsa;
	    }
	
	    /**
	     * Get string value at name.
	     * @param index value.
	     * @return String value.
	     */
	   [LuaFunction(typeof(Int32))]
	    public String GetString(Int32 index)
	    {
            return (String)jsa[index];
	    }
	
	    /**
	     * Get int value at name.
	     * @param index value.
	     * @return Int32 value.
	     */
	    [LuaFunction(typeof(Int32))]
	    public Int32 GetInt(Int32 index)
	    {
            return (Int32)jsa[index]; 
	    }
	
	    /**
	     * Get double value at name.
	     * @param index value.
	     * @return Double value.
	     */
	    [LuaFunction(typeof(Int32))]
	    public Double GetDouble(Int32 index)
	    {
            return (Double)jsa[index];
	    }
	
	    /**
	     * Get Single value at name.
	     * @param index value.
	     * @return Single value.
	     */
	    [LuaFunction(typeof(Int32))]
	    public Single GetSingle(Int32 index)
	    {
            return (Single)jsa[index];
	    }
	
	    /**
	     * Get boolean value at name.
	     * @param index value.
	     * @return Boolean value.
	     */
        [LuaFunction(typeof(Int32))]
	    public Boolean GetBool(Int32 index)
	    {
            return (Boolean)jsa[index];
	    }
	
	    /**
	     * Frees the object.
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
            return "LuaJSONArray";
        }

        #endregion
    }

    [LuaClass("LuaJSONObject")]
    public class LuaJSONObject : LuaInterface
    {
        public JObject jso;
        /**
	     * Creates LuaJSON from json string.
	     * @param str
	     * return LuaJSONObject
	     */
	    [LuaFunction(typeof(String))]
	    public static LuaJSONObject CreateJSOFromString(String str)
	    {
		    LuaJSONObject lso = new LuaJSONObject();
            lso.jso = (JObject)JsonConvert.DeserializeObject(str);
		    return lso;
	    }
	
	    /**
	     * Get object value at name.
	     * @param name Name value.
	     * @return LuaJSONObject
	     */
	    [LuaFunction(typeof(String))]
	    public LuaJSONObject GetJSONObject(String name)
	    {
		    LuaJSONObject lso = new LuaJSONObject();
			lso.jso = (JObject)jso[name];
		    return lso;
	    }
	
	    /**
	     * Get array value at name.
	     * @param name Name value.
	     * @return LuaJSONArray
	     */
	    [LuaFunction(typeof(String))]
	    public LuaJSONArray GetJSONArray(String name)
	    {
		    LuaJSONArray lsa = new LuaJSONArray();
            lsa.jsa = (JArray)jso[name];
		    return lsa;
	    }
	
	    /**
	     * Get string value at name.
	     * @param name Name value.
	     * @return String value.
	     */
	    [LuaFunction(typeof(String))]
	    public String GetString(String name)
	    {
            return (String)jso[name];
	    }
	
	    /**
	     * Get int value at name.
	     * @param name Name value.
	     * @return Int32 value.
	     */
	    [LuaFunction(typeof(String))]
	    public Int32 GetInt(String name)
	    {
            return (Int32)jso[name];
	    }
	
	    /**
	     * Get double value at name.
	     * @param name Name value.
	     * @return Double value.
	     */
	    [LuaFunction(typeof(String))]
	    public Double GetDouble(String name)
	    {
            return (Double)jso[name];
	    }
	
	    /**
	     * Get Single value at name.
	     * @param name Name value.
	     * @return Single value.
	     */
	    [LuaFunction(typeof(String))]
	    public Single GetFloat(String name)
	    {
            return (Single)jso[name];
	    }
	
	    /**
	     * Get boolean value at name.
	     * @param name Name value.
	     * @return Boolean value.
	     */
	    [LuaFunction(typeof(String))]
	    public Boolean GetBool(String name)
	    {
            return (Boolean)jso[name];
	    }
	
	    /**
	     * Frees the object.
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
            return "LuaJSONObject";
        }

        #endregion
    }
}
