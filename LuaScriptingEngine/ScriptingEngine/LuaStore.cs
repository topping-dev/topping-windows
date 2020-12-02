using System;
using System.Net;
using System.Windows;
using LuaScriptingEngine;
using LuaCSharp;
using System.Windows.Input;
#if !NETFX_CORE
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.IO.IsolatedStorage;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
#else
using Windows.UI.Xaml.Controls;
#endif

namespace ScriptingEngine
{
    [LuaClass("LuaStore")]
    [LuaGlobalManual("STORE")]
    public class LuaStore : LuaInterface
    {
        /**
	     * Sets the string value to store
	     * @param key 
	     * @param value
	    */
        public static void SetString(String key, String value)
        {
#if NETFX_CORE
            StorageSettings iss = Defines.GetStorageSettings();
#else
            IsolatedStorageSettings iss = IsolatedStorageSettings.ApplicationSettings;
#endif
            iss[key] = value;
            iss.Save();
        }

        /**
         * Sets the number value to store
         * @param key
         * @param value
         */
        public static void SetNumber(String key, double value)
        {
#if NETFX_CORE
            StorageSettings iss = Defines.GetStorageSettings();
#else
            IsolatedStorageSettings iss = IsolatedStorageSettings.ApplicationSettings;
#endif
            iss[key] = value;
            iss.Save();
        }

        /**
         * Gets value stored at key
         * @param key
         * @return
         */
        public static Object Get(String key)
        {
#if NETFX_CORE
            StorageSettings iss = Defines.GetStorageSettings();
#else
            IsolatedStorageSettings iss = IsolatedStorageSettings.ApplicationSettings;
#endif
            String value = "";
            if (iss.TryGetValue<String>(key, out value))
            {
                return value;
            }
            else
            {
                if (iss.Contains(key))
                    return Convert.ToDouble(iss[key]);
                else
                    return null;
            }
        }

        /**
         * Gets string value stored at key
         * @param key
         * @return
         */
        public static String GetString(String key)
        {
#if NETFX_CORE
            StorageSettings iss = Defines.GetStorageSettings();
#else
            IsolatedStorageSettings iss = IsolatedStorageSettings.ApplicationSettings;
#endif
            String value = "";
            if (iss.TryGetValue<String>(key, out value))
            {
                return value;
            }
            return null;
        }

        /**
         * Gets number value stored at key
         * @param key
         * @return
         */
        public static double GetNumber(String key)
        {
#if NETFX_CORE
            StorageSettings iss = Defines.GetStorageSettings();
#else
            IsolatedStorageSettings iss = IsolatedStorageSettings.ApplicationSettings;
#endif
            if (iss.Contains(key))
            {
                return Convert.ToDouble(iss[key]);
            }
            return 0;
        }

        public static int __tostring(Lua.lua_State L)
        {
            Lua.lua_pushstring(L, "STORE");
            return 0;
        }

        public static int __gc(Lua.lua_State L)
        {
            return 0;
        }

        public static int __index(Lua.lua_State L)
        {
            String key = Lua.lua_tostring(L, 2).toString();
		    Object val = LuaStore.Get(key);
		    if(val == null)
			    LuaEngine.Instance.PushNIL();
		    else if(val.GetType() == typeof(String))
                LuaEngine.Instance.PushString((String)val);
		    else
                LuaEngine.Instance.PushDouble((Double)val);
		    return 1;
        }

        public static int __newindex(Lua.lua_State L)
        {
            String key = Lua.lua_tostring(L, 2).toString();
            if (Lua.lua_isstring(L, 3) != 0)
            {
                String val = Lua.lua_tostring(L, 3).toString();
                LuaStore.SetString(key, val);
            }
            else if (Lua.lua_isnumber(L, 3) != 0)
            {
                Double val = Lua.lua_tonumber(L, 3);
                LuaStore.SetNumber(key, val);
            }
            return 1;
        }

        #region LuaInterface Members

        public void RegisterEventFunction(string var, LuaTranslator lt)
        {
        }

        public string GetId()
        {
            return "LuaStore";
        }

        #endregion
    }
}
