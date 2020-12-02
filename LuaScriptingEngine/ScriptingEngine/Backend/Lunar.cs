//#define REFLECTION_STYLA
using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using LuaCSharp;
using LoggerNamespace;

namespace ScriptingEngine
{
    public struct RegType<T>
    {
        public delegate int mfp(Lua.lua_State L, T ptr);
        public String name;
        public mfp mfunc;
    };

    public class Lunar<T>
    {
        private static Dictionary<Int32, T> m_objectReferences = new Dictionary<Int32, T>();
        private static MersenneTwister Rand = new MersenneTwister((Int32)DateTime.Now.Ticks);
        private Lunar()
        {
            
        }

        public static String RemoveChar(String from, Char w)
        {
            Char[] arr = from.ToCharArray();
            StringBuilder sb = new StringBuilder();
            foreach (Char c in arr)
            {
                if (c != w)
                    sb.Append(c);
            }
            return sb.ToString();
        }

        public static void Register(Lua.lua_State L, bool loadAll)
	    {
#if REFLECTION_STYLA
            String name = RemoveChar(typeof(T).FullName, '.');
#else
#if !NETFX_CORE
            Object[] attrsClass = typeof(T).GetCustomAttributes(typeof(LuaClass), true);
#else
            Object[] attrsClass = typeof(T).GetTypeInfo().GetCustomAttributes(typeof(LuaClass), true).ToArray();
#endif
            if (attrsClass.Length == 0)
                return;

            String name = ((LuaClass)attrsClass[0]).GetName();
#endif
            //

		    Lua.lua_newtable(L);
		    int methods = Lua.lua_gettop(L);

            Lua.luaL_newmetatable(L, name);
		    int metatable = Lua.lua_gettop(L);
    		
		    Lua.luaL_newmetatable(L, "DO NOT TRASH");
		    Lua.lua_pop(L, 1);

		    // store method table in globals so that
		    // scripts can add functions written in Lua.
		    Lua.lua_pushvalue(L, methods);
            Lua.lua_setfield(L, Lua.LUA_GLOBALSINDEX, name);

		    // hide metatable from Lua getmetatable()
		    Lua.lua_pushvalue(L, methods);
		    Lua.lua_setfield(L, metatable, "__metatable");

		    Lua.lua_pushvalue(L, methods);
		    Lua.lua_setfield(L, metatable, "__index");

		    Lua.lua_pushcfunction(L, tostring_T);
		    Lua.lua_setfield(L, metatable, "__tostring");

		    Lua.lua_pushcfunction(L, gc_T);
		    Lua.lua_setfield(L, metatable, "__gc");

		    Lua.lua_newtable(L);                // mt for method table
		    Lua.lua_setmetatable(L, methods);

#if !NETFX_CORE
            MethodInfo[] methodInfos = typeof(T).GetMethods(BindingFlags.Public | /*BindingFlags.DeclaredOnly | */BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy);
#else
            MethodInfo[] methodInfos = typeof(T).GetMethods(BindingFlags.Public, "LGView");
#endif

            Lua.lua_CFunction Fthunk = new Lua.lua_CFunction(thunk);
            Lua.lua_CFunction SFthunk = new Lua.lua_CFunction(Sthunk);

            //List of LuaGlobalManual
            Dictionary<String, Lua.lua_CFunction> dictGlobal = new Dictionary<String, Lua.lua_CFunction>();
            Dictionary<String, String> dictAddedFunc = new Dictionary<String, String>();

            foreach (MethodInfo mi in methodInfos)
            {
                if (mi.IsStatic && mi.Name.Contains("__tostring"))
#if NETFX_CORE
                    dictGlobal.Add("__tostring", (Lua.lua_CFunction)mi.CreateDelegate(typeof(Lua.lua_CFunction)));
#else
                    dictGlobal.Add("__tostring", (Lua.lua_CFunction)Delegate.CreateDelegate(typeof(Lua.lua_CFunction), mi));
#endif
                else if(mi.IsStatic && mi.Name.Contains("__gc"))
#if NETFX_CORE
                    dictGlobal.Add("__gc", (Lua.lua_CFunction)mi.CreateDelegate(typeof(Lua.lua_CFunction)));
#else
                    dictGlobal.Add("__gc", (Lua.lua_CFunction)Delegate.CreateDelegate(typeof(Lua.lua_CFunction), mi));
#endif
                else if (mi.IsStatic && mi.Name.Contains("__index"))
#if NETFX_CORE
                    dictGlobal.Add("__index", (Lua.lua_CFunction)mi.CreateDelegate(typeof(Lua.lua_CFunction)));
#else
                    dictGlobal.Add("__index", (Lua.lua_CFunction)Delegate.CreateDelegate(typeof(Lua.lua_CFunction), mi));
#endif
                else if (mi.IsStatic && mi.Name.Contains("__newindex"))
#if NETFX_CORE
                    dictGlobal.Add("__newindex", (Lua.lua_CFunction)mi.CreateDelegate(typeof(Lua.lua_CFunction)));
#else
                    dictGlobal.Add("__newindex", (Lua.lua_CFunction)Delegate.CreateDelegate(typeof(Lua.lua_CFunction), mi));
#endif
                if (loadAll)
                {
                    Lua.lua_pushstring(L, mi.Name);
                    //Lua.lua_pushlightuserdata(L, mi);
                    ParameterInfo[] piArr = mi.GetParameters();
                    List<Type> pArr = new List<Type>();
                    foreach(ParameterInfo pi in piArr)
                    {
                        pArr.Add(pi.ParameterType);
                    }
                    LuaFunction a = new LuaFunction(pArr.ToArray());
                    ((LuaFunction)a).SetMethodInfo(mi);
                    Lua.lua_pushlightuserdata(L, a);
                    //Lua.lua_pushlightuserdata(L, (void*)l);
                    if(mi.IsStatic)
                        Lua.lua_pushcclosure(L, SFthunk, 1);
                    else
                        Lua.lua_pushcclosure(L, Fthunk, 1);
                    Lua.lua_settable(L, methods);
                }
                else
                {
                    Object[] attrs = mi.GetCustomAttributes(true)
#if NETFX_CORE
                    .ToArray()
#endif
                    ;
                    foreach (Object a in attrs)
                    {
                        //Do we have a access
                        if (a is LuaFunction)
                        {
                            if (dictAddedFunc.ContainsKey(mi.Name))
                                continue;
                            dictAddedFunc.Add(mi.Name, "");
                            Lua.lua_pushstring(L, mi.Name);
                            //Lua.lua_pushlightuserdata(L, mi);
                            ((LuaFunction)a).SetMethodInfo(mi);
                            Lua.lua_pushlightuserdata(L, a);
                            //Lua.lua_pushlightuserdata(L, (void*)l);
                            if (mi.IsStatic)
                                Lua.lua_pushcclosure(L, SFthunk, 1);
                            else
                                Lua.lua_pushcclosure(L, Fthunk, 1);
                            Lua.lua_settable(L, methods);
                        }
                    }
                }
            }

		    // fill method table with methods from class T
		    /*for (RegType *l = ((RegType*)GetMethodTable<T>()); l->name; l++) {
			    lua_pushstring(L, l->name);
			    lua_pushlightuserdata(L, (void*)l);
			    lua_pushcclosure(L, thunk, 1);
			    lua_settable(L, methods);
		    }*/

            attrsClass = typeof(T).GetCustomAttributes(typeof(LuaGlobalString), true);
            if (attrsClass.Length != 0)
            {
                LuaGlobalString lgs = (LuaGlobalString)attrsClass[0];
                String[] keys = lgs.GetKeys();
                String[] vals = lgs.GetVals();
                for (int i = 0; i < keys.Length; i++)
                {
                    Lua.lua_pushliteral(L, vals[i]);
                    Lua.lua_setglobal(L, keys[i]);
                }
            }

            attrsClass = typeof(T).GetCustomAttributes(typeof(LuaGlobalInt), true);
            if (attrsClass.Length != 0)
            {
                LuaGlobalInt lgi = (LuaGlobalInt)attrsClass[0];
                String[] keys = lgi.GetKeys();
                Int32[] vals = lgi.GetVals();
                for (int i = 0; i < keys.Length; i++)
                {
                    Lua.lua_pushinteger(L, vals[i]);
                    Lua.lua_setglobal(L, keys[i]);
                }
            }

            attrsClass = typeof(T).GetCustomAttributes(typeof(LuaGlobalUInt), true);
            if (attrsClass.Length != 0)
            {
                LuaGlobalUInt lgi = (LuaGlobalUInt)attrsClass[0];
                String[] keys = lgi.GetKeys();
                UInt32[] vals = lgi.GetVals();
                for (int i = 0; i < keys.Length; i++)
                {
                    Lua.lua_pushinteger(L, (Int32)vals[i]);
                    Lua.lua_setglobal(L, keys[i]);
                }
            }

            attrsClass = typeof(T).GetCustomAttributes(typeof(LuaGlobalNumber), true);
            if (attrsClass.Length != 0)
            {
                LuaGlobalNumber lgn = (LuaGlobalNumber)attrsClass[0];
                String[] keys = lgn.GetKeys();
                Double[] vals = lgn.GetVals();
                for (int i = 0; i < keys.Length; i++)
                {
                    Lua.lua_pushnumber(L, vals[i]);
                    Lua.lua_setglobal(L, keys[i]);
                }
            }

		    Lua.lua_pop(L, 2);  // drop metatable and method table

            attrsClass = typeof(T).GetCustomAttributes(typeof(LuaGlobalManual), true);
            if (attrsClass.Length != 0)
            {
                LuaGlobalManual lgm = (LuaGlobalManual)attrsClass[0];
                String namea = lgm.GetName();
                Lua.lua_newtable(L);
                int methodsa = Lua.lua_gettop(L);

                Lua.luaL_newmetatable(L, namea);
                int metatablea = Lua.lua_gettop(L);

                Lua.luaL_newmetatable(L, "DO NOT TRASH");
                Lua.lua_pop(L, 1);

                // store method table in globals so that
                // scripts can add functions written in Lua.
                Lua.lua_pushvalue(L, methodsa);
                Lua.lua_setfield(L, Lua.LUA_GLOBALSINDEX, namea);

                // hide metatable from Lua getmetatable()
                Lua.lua_pushvalue(L, methodsa);
                Lua.lua_setfield(L, metatablea, "__metatable");

                Lua.lua_pushvalue(L, methodsa);
                Lua.lua_setfield(L, metatablea, "__index");

                Lua.lua_pushcfunction(L, dictGlobal["__tostring"]);
                Lua.lua_setfield(L, metatablea, "__tostring");

                Lua.lua_pushcfunction(L, dictGlobal["__gc"]);
                Lua.lua_setfield(L, metatablea, "__gc");

                Lua.lua_newtable(L);                // mt for method table
                int mt = Lua.lua_gettop(L);

                Lua.lua_pushcfunction(L, dictGlobal["__index"]);
                Lua.lua_setfield(L, mt, "__index");

                Lua.lua_pushcfunction(L, dictGlobal["__newindex"]);
                Lua.lua_setfield(L, mt, "__newindex");

                Lua.lua_setmetatable(L, methodsa);
                Lua.lua_pop(L, 2);
            }
	    }

    // push onto the Lua stack a userdata containing a pointer to T object
	    public static int push(Lua.lua_State L, T obj, bool gc)
        {
            if (obj == null)
            {
                Lua.lua_pushnil(L);
                return Lua.lua_gettop(L);
            }

#if REFLECTION_STYLA
            String name = RemoveChar(typeof(T).FullName, '.');
#else
            Object[] attrsClass = typeof(T).GetCustomAttributes(typeof(LuaClass), true);
            if (attrsClass.Length == 0)
            {
                Lua.lua_pushnil(L);
                return Lua.lua_gettop(L);
            }

            String name = ((LuaClass)attrsClass[0]).GetName();
#endif

            Lua.luaL_getmetatable(L, name);  // lookup metatable in Lua registry
            if (Lua.lua_isnil(L, -1))
                Lua.luaL_error(L, "%s missing metatable", name);

            int mt = Lua.lua_gettop(L);
            //Lua.lua_pushlightuserdata(L, obj);
            Object ptr = Lua.lua_newuserdata(L, typeof(LuaObject<T>));
            ((LuaObject<T>)ptr).PushObject(obj);
            int ud = Lua.lua_gettop(L);
            {
                Lua.lua_pushvalue(L, mt);
                Lua.lua_setmetatable(L, -2);
                Lua.lua_getfield(L, Lua.LUA_REGISTRYINDEX, "DO NOT TRASH");
                if (Lua.lua_isnil(L, -1))
                {
                    Lua.luaL_newmetatable(L, "DO NOT TRASH");
                    Lua.lua_pop(L, 1);
                }
                Lua.lua_getfield(L, Lua.LUA_REGISTRYINDEX, "DO NOT TRASH");
                if (gc == false)
                {
                    Lua.lua_pushboolean(L, 1);
                    Lua.lua_setfield(L, -2, name);
                }
                Lua.lua_pop(L, 1);
            }

            Lua.lua_settop(L, ud);
            Lua.lua_replace(L, mt);
            Lua.lua_settop(L, mt);
            return mt;  // index of userdata containing pointer to T object
        }

    // get userdata from Lua stack and return pointer to T object
	    private static T check(Lua.lua_State L, int narg, out Int32 ptr) 
        {
            T obj = (T)((LuaObject<T>)Lua.lua_touserdata(L, narg)).obj;
            //T obj = (T)Lua.lua_touserdata(L, narg);
            ptr = 0;
            if (obj == null)
			    return default(T);
            /*if (!m_objectReferences.ContainsKey(ptr))
                return default(T);
            T obj = m_objectReferences[ptr];*/
		    return obj;
	    }

        public static Object ParseTable(Lua.lua_TValue valP)
        {
    	    Dictionary<Object, Object> map = new Dictionary<Object, Object>();
    	    Lua.Table ot = Lua.hvalue(valP);
    	    int size = Lua.sizenode(ot);
		    for(int i = 0; i < size; i++)
		    {
			    Lua.Node node = Lua.gnode(ot, i);
			    Lua.lua_TValue key = Lua.key2tval(node);
			    Object keyObject = null;
			    switch(Lua.ttype(key))
			    {
				    case Lua.LUA_TNIL:
					    break;
				    case Lua.LUA_TSTRING:
				    {
					    Lua.TString str = Lua.rawtsvalue(key);
					    keyObject = str.ToString();
				    }break;
				    case Lua.LUA_TNUMBER:
					    keyObject = Lua.nvalue(key);
					    break;
				    default:
					    break;
			    }
			
			    Lua.lua_TValue val = Lua.luaH_get(ot, key);
			    Object valObject = null;
			    switch(Lua.ttype(val))
			    {
				    case Lua.LUA_TNIL:
					    break;
				    case Lua.LUA_TSTRING:
				    {
					    Lua.TString str = Lua.rawtsvalue(val);
					    valObject = str.ToString();
				    }break;
				    case Lua.LUA_TNUMBER:
					    valObject = Lua.nvalue(val);
					    break;
				    case Lua.LUA_TTABLE:
					    valObject = ParseTable(val);
					    break;
				    case Lua.LUA_TUSERDATA:
				    {
					    valObject = (Lua.rawuvalue(val).user_data);
					    if(valObject != null)
					    {
                            if (valObject.GetType().FullName.StartsWith("ScriptingEngine.LuaObject"))
	                        {
                                PropertyInfo pi = valObject.GetType().GetProperty("obj");
                                if (pi == null)
                                {
                                    Logger.Log(LogType.CONSOLE, LogLevel.ERROR, "Cannot get lua object property static thunk");
                                    return 0;
                                }
                                Object objA = pi.GetValue(valObject, null);
	
	                            valObject = objA;
	                        }
					    }
				    }break;
				    case Lua.LUA_TLIGHTUSERDATA:
				    {
					    valObject = Lua.pvalue(val);
					    if(valObject != null)
					    {
                            if (valObject.GetType().FullName.StartsWith("ScriptingEngine.LuaObject"))
                            {
                                PropertyInfo pi = valObject.GetType().GetProperty("obj");
                                if (pi == null)
                                {
                                    Logger.Log(LogType.CONSOLE, LogLevel.ERROR, "Cannot get lua object property static thunk");
                                    return 0;
                                }
                                Object objA = pi.GetValue(valObject, null);

                                valObject = objA;
                            }
					    }
				    }break;					
					
			    }
			    map.Add(keyObject, valObject);
		    }
		    return map;
        }

	    // member function dispatcher
        private static int thunk(Lua.lua_State L)
        {
		    // stack has userdata, followed by method args
            Int32 ptr = 0;
		    T obj = check(L, 1, out ptr);  // get 'self', or if you prefer, 'this'
		    Lua.lua_remove(L, 1);  // remove self so member function args start at index 1
		    // get member function from upvalue
            //MethodInfo mi = (MethodInfo)Lua.lua_touserdata(L, Lua.lua_upvalueindex(1));
            LuaFunction la = (LuaFunction)Lua.lua_touserdata(L, Lua.lua_upvalueindex(1));
            MethodInfo mi = la.GetMethodInfo();
            if (la.GetManual())
                mi.Invoke(obj, new object[] { L });
            else
            {
                int count = 1;
                List<Object> argList = new List<Object>();
                List<Type> argTypeList = la.GetArguments();
                foreach (Type t in argTypeList)
                {
                    switch (t.FullName)
                    {
                        case "System.Boolean":
                            argList.Add(Convert.ToBoolean(Lua.luaL_checkinteger(L, count)));
                            break;
                        case "System.Byte":
                            argList.Add(Convert.ToByte(Lua.luaL_checkinteger(L, count)));
                            break;
                        case "System.Int16":
                            argList.Add(Convert.ToInt16(Lua.luaL_checkinteger(L, count)));
                            break;
                        case "System.UInt16":
                            argList.Add(Convert.ToUInt16(Lua.luaL_checkinteger(L, count)));
                            break;
                        case "System.Int32":
                            argList.Add(Lua.luaL_checkinteger(L, count));
                            break;
                        case "System.UInt32":
                            argList.Add(Convert.ToUInt32(Lua.luaL_checkinteger(L, count)));
                            break;
                        case "System.Int64":
                            argList.Add(Lua.luaL_checklong(L, count));
                            break;
                        case "System.UInt64":
                            argList.Add(Convert.ToUInt64(Lua.luaL_checklong(L, count)));
                            break;                        case "System.Single":
                            argList.Add(Convert.ToSingle(Lua.luaL_checknumber(L, count)));
                            break;
                        case "System.Double":
                            argList.Add(Lua.luaL_checknumber(L, count));
                            break;
                        case "System.Char":
                            argList.Add(Convert.ToChar(Lua.luaL_checkstring(L, count).ToString()));
                            break;
                        case "System.String":
                            argList.Add(Lua.luaL_checkstring(L, count).ToString());
                            break;
                        case "KopiLua.Lua.lua_State":
                            argList.Add(L);
                            break;
                        default:
                            Object o = Lua.lua_touserdata(L, count);
                            if (o != null)
                            {
                                if (o.GetType().FullName.StartsWith("ScriptingEngine.LuaObject"))
                                {
                                    PropertyInfo pi = o.GetType().GetProperty("obj");
                                    if (pi == null)
                                    {
                                        Logger.Log(LogType.CONSOLE, LogLevel.ERROR, "Cannot get lua object property static thunk");
                                        return 0;
                                    }
                                    Object objc = pi.GetValue(o, null);

                                    argList.Add(objc);
                                }
                                else if(o.GetType().FullName.StartsWith("ScriptingEngine.LuaNativeObject"))
	            		        {
	            			        Object objNative = ((LuaNativeObject)o).obj;
	            			        if(objNative == null)
	            			        {
	            				        Log.e("Lunar Push", "Cannot get lua native object property thunk");
	            				        return 0;
	            			        }
	            			
	            			        argList.Add(objNative);
	            		        }
                                else
                                    argList.Add(o);
                            }
                            else
                            {
                                o = Lua.lua_topointer(L, count);
            			        if(o == null)
            			        {
            				        if(Lua.lua_isboolean(L, count))
            					        argList.Add(Lua.lua_toboolean(L, count));
            				        else if(Lua.lua_isnumber(L, count) > 0)
            					        argList.Add(Lua.lua_tonumber(L, count));
            				        else if(Lua.lua_isstring(L, count) > 0)
            					        argList.Add(Lua.lua_tostring(L, count).toString());
            				        else if(Lua.lua_isnoneornil(L, count))
            					        argList.Add(null);
            			        }
            			        else
            			        {
	            			        Lua.Table ot = (Lua.Table)o;
	            			        Dictionary<Object, Object> map = new Dictionary<Object, Object>();
	            			        int size = Lua.sizenode(ot);
	            			        for(int i = 0; i < size; i++)
	            			        {
	            				        Lua.Node node = Lua.gnode(ot, i);
	            				        Lua.lua_TValue key = Lua.key2tval(node);
	            				        Object keyObject = null;
	            				        switch(Lua.ttype(key))
								        {
									        case Lua.LUA_TNIL:
										        break;
									        case Lua.LUA_TSTRING:
									        {
										        Lua.TString str = Lua.rawtsvalue(key);
										        keyObject = str.ToString();
									        }break;
									        case Lua.LUA_TNUMBER:
										        keyObject = Lua.nvalue(key);
										        break;
									        default:
										        break;
								        }
	            				
	            				        Lua.lua_TValue val = Lua.luaH_get(ot, key);
	            				        Object valObject = null;
	            				        switch(Lua.ttype(val))
	            				        {
	            					        case Lua.LUA_TNIL:
	            						        break;
	            					        case Lua.LUA_TSTRING:
	            					        {
	            						        Lua.TString str = Lua.rawtsvalue(val);
	            						        valObject = str.ToString();
	            					        }break;
	            					        case Lua.LUA_TNUMBER:
	            						        valObject = Lua.nvalue(val);
	            						        break;
	            					        case Lua.LUA_TTABLE:
	            						        valObject = ParseTable(val);
	            						        break;
	            					        case Lua.LUA_TUSERDATA:
	            					        {
	            						        valObject = (Lua.rawuvalue(val).user_data);
	            						        if(valObject != null)
	            						        {
		            						        if(valObject.GetType().FullName.StartsWith("ScriptingEngine.LuaObject"))
		            	                            {
                                                        PropertyInfo pi = valObject.GetType().GetProperty("obj");
                                                        if (pi == null)
                                                        {
                                                            Logger.Log(LogType.CONSOLE, LogLevel.ERROR, "Cannot get lua object property static thunk");
                                                            return 0;
                                                        }
                                                        Object objA = pi.GetValue(valObject, null);
		            	
		            	                                valObject = objA;
		            	                            }
	            						        }
	            					        }break;
	            					        case Lua.LUA_TLIGHTUSERDATA:
	            					        {
	            						        valObject = Lua.pvalue(val);
	            						        if(valObject != null)
	            						        {
		            						        if(valObject.GetType().FullName.StartsWith("ScriptingEngine.LuaObject"))
		            	                            {
                                                        PropertyInfo pi = valObject.GetType().GetProperty("obj");
                                                        if (pi == null)
                                                        {
                                                            Logger.Log(LogType.CONSOLE, LogLevel.ERROR, "Cannot get lua object property static thunk");
                                                            return 0;
                                                        }
                                                        Object objA = pi.GetValue(valObject, null);
		            	
		            	                                valObject = objA;
		            	                            }
	            						        }
	            					        }break;
	            				        }
                                        if (keyObject == null && valObject == null)
                                        {
                                            for(int j = 0; j < ot.array.Length; j++)
                                            {
                                                Lua.lua_TValue valO = ot.array[j];
                                                switch (Lua.ttype(valO))
                                                {
                                                    case Lua.LUA_TNIL:
                                                        break;
                                                    case Lua.LUA_TSTRING:
                                                        {
                                                            Lua.TString str = Lua.rawtsvalue(valO);
                                                            valObject = str.ToString();
                                                        } break;
                                                    case Lua.LUA_TNUMBER:
                                                        valObject = Lua.nvalue(valO);
                                                        break;
                                                    case Lua.LUA_TTABLE:
                                                        valObject = ParseTable(valO);
                                                        break;
                                                    case Lua.LUA_TUSERDATA:
                                                        {
                                                            valObject = (Lua.rawuvalue(valO).user_data);
                                                            if (valObject != null)
                                                            {
                                                                if (valObject.GetType().FullName.StartsWith("ScriptingEngine.LuaObject"))
                                                                {
                                                                    PropertyInfo pi = valObject.GetType().GetProperty("obj");
                                                                    if (pi == null)
                                                                    {
                                                                        Logger.Log(LogType.CONSOLE, LogLevel.ERROR, "Cannot get lua object property static thunk");
                                                                        return 0;
                                                                    }
                                                                    Object objA = pi.GetValue(valObject, null);

                                                                    valObject = objA;
                                                                }
                                                            }
                                                        } break;
                                                    case Lua.LUA_TLIGHTUSERDATA:
                                                        {
                                                            valObject = Lua.pvalue(valO);
                                                            if (valObject != null)
                                                            {
                                                                if (valObject.GetType().FullName.StartsWith("ScriptingEngine.LuaObject"))
                                                                {
                                                                    PropertyInfo pi = valObject.GetType().GetProperty("obj");
                                                                    if (pi == null)
                                                                    {
                                                                        Logger.Log(LogType.CONSOLE, LogLevel.ERROR, "Cannot get lua object property static thunk");
                                                                        return 0;
                                                                    }
                                                                    Object objA = pi.GetValue(valObject, null);

                                                                    valObject = objA;
                                                                }
                                                            }
                                                        } break;
                                                }

                                                map.Add(j, valObject);
                                            }
                                        }
                                        else
                                            map.Add(keyObject, valObject);
	            			        }
	            			        argList.Add(map);
            			        }
                            }
                            break;
                    };
                    ++count;
                }


                Object retval = mi.Invoke(obj, argList.ToArray());
                Type rettype = null;

                if (retval == null)
                    LuaEngine.Instance.PushNIL();
                else
                {
                    rettype = retval.GetType();
                    switch (rettype.FullName)
                    {
                        case "System.Boolean":
                            LuaEngine.Instance.PushBool(Convert.ToBoolean(retval));
                            break;
                        case "System.Byte":
                        case "System.Int16":
                        case "System.Int32":
                        case "System.UInt16":
                        case "System.UInt32":
                            LuaEngine.Instance.PushInt(Convert.ToInt32(retval));
                            break;
                        case "System.Int64":
                        case "System.UInt64":
                            LuaEngine.Instance.PushInt(Convert.ToInt32(retval));
                            break;
                        case "System.Single":
                            LuaEngine.Instance.PushFloat(Convert.ToSingle(retval));
                            break;
                        case "System.Double":
                            LuaEngine.Instance.PushDouble(Convert.ToDouble(retval));
                            break;
                        case "System.Char":
                        case "System.String":
                            LuaEngine.Instance.PushString(Convert.ToString(retval));
                            break;
                        case "System.Void":
                            LuaEngine.Instance.PushInt(0);
                            break;
                        default:
                            typeof(Lunar<>).MakeGenericType(rettype).GetMethod("push").Invoke(null, new object[] { LuaEngine.Instance.GetState(), retval, true });
                            break;
                    };
                }
            }
            
            
		    /*RegType *l = static_cast<RegType*>(lua_touserdata(L, lua_upvalueindex(1)));
		    //return (obj->*(l->mfunc))(L);  // call member function
		    return l->mfunc(L,obj);*/
            return 1;
	    }

        // static member function dispatcher
        private static int Sthunk(Lua.lua_State L)
        {
            // stack has userdata, followed by method args
            /*Int32 ptr = 0;
            T obj = check(L, 1, out ptr);  // get 'self', or if you prefer, 'this'*/
            //Lua.lua_remove(L, 1);  // remove self so member function args start at index 1
            // get member function from upvalue
            //MethodInfo mi = (MethodInfo)Lua.lua_touserdata(L, Lua.lua_upvalueindex(1));
            LuaFunction la = (LuaFunction)Lua.lua_touserdata(L, Lua.lua_upvalueindex(1));
            MethodInfo mi = la.GetMethodInfo();
            if (la.GetManual())
                mi.Invoke(null, new object[] { L });
            else
            {
                int count = 1;
                List<Object> argList = new List<Object>();
                List<Type> argTypeList = la.GetArguments();
                foreach (Type t in argTypeList)
                {
                    switch (t.FullName)
                    {
                        case "System.Boolean":
                            argList.Add(Convert.ToBoolean(Lua.luaL_checkinteger(L, count)));
                            break;
                        case "System.Byte":
                            argList.Add(Convert.ToByte(Lua.luaL_checkinteger(L, count)));
                            break;
                        case "System.Int16":
                            argList.Add(Convert.ToInt16(Lua.luaL_checkinteger(L, count)));
                            break;
                        case "System.UInt16":
                            argList.Add(Convert.ToUInt16(Lua.luaL_checkinteger(L, count)));
                            break;
                        case "System.Int32":
                            argList.Add(Lua.luaL_checkinteger(L, count));
                            break;
                        case "System.UInt32":
                            argList.Add(Convert.ToUInt32(Lua.luaL_checkinteger(L, count)));
                            break;
                        case "System.Int64":
                            argList.Add(Lua.luaL_checklong(L, count));
                            break;
                        case "System.UInt64":
                            argList.Add(Convert.ToUInt64(Lua.luaL_checklong(L, count)));
                            break;
                        case "System.Single":
                            argList.Add(Convert.ToSingle(Lua.luaL_checknumber(L, count)));
                            break;
                        case "System.Double":
                            argList.Add(Lua.luaL_checknumber(L, count));
                            break;
                        case "System.Char":
                            argList.Add(Convert.ToChar(Lua.luaL_checkstring(L, count).ToString()));
                            break;
                        case "System.String":
                            argList.Add(Lua.luaL_checkstring(L, count).ToString());
                            break;
                        case "KopiLua.Lua.lua_State":
                            argList.Add(L);
                            break;
                        default:
                            Object o = Lua.lua_touserdata(L, count);
                            if (o != null)
                            {
                                if (o.GetType().FullName.StartsWith("ScriptingEngine.LuaObject"))
                                {
                                    PropertyInfo pi = o.GetType().GetProperty("obj");
                                    if (pi == null)
                                    {
                                        Logger.Log(LogType.CONSOLE, LogLevel.ERROR, "Cannot get lua object property static thunk");
                                        return 0;
                                    }
                                    Object obj = pi.GetValue(o, null);

                                    argList.Add(obj);
                                }
                                else if(o.GetType().FullName.StartsWith("ScriptingEngine.LuaNativeObject"))
	            		        {
	            			        Object objNative = ((LuaNativeObject)o).obj;
	            			        if(objNative == null)
	            			        {
	            				        Log.e("Lunar Push", "Cannot get lua native object property thunk");
	            				        return 0;
	            			        }
	            			
	            			        argList.Add(objNative);
	            		        }
                                else
                                    argList.Add(o);
                            }
                            else
                            {
                                o = Lua.lua_topointer(L, count);
            			        if(o == null)
            			        {
            				        if(Lua.lua_isboolean(L, count))
            					        argList.Add(Lua.lua_toboolean(L, count));
            				        else if(Lua.lua_isnumber(L, count) > 0)
            					        argList.Add(Lua.lua_tonumber(L, count));
            				        else if(Lua.lua_isstring(L, count) > 0)
            					        argList.Add(Lua.lua_tostring(L, count).toString());
            				        else if(Lua.lua_isnoneornil(L, count))
            					        argList.Add(null);
            			        }
            			        else
            			        {
	            			        Lua.Table ot = (Lua.Table)o;
	            			        Dictionary<Object, Object> map = new Dictionary<Object, Object>();
	            			        int size = Lua.sizenode(ot);
	            			        for(int i = 0; i < size; i++)
	            			        {
	            				        Lua.Node node = Lua.gnode(ot, i);
	            				        Lua.lua_TValue key = Lua.key2tval(node);
	            				        Object keyObject = null;
	            				        switch(Lua.ttype(key))
								        {
									        case Lua.LUA_TNIL:
										        break;
									        case Lua.LUA_TSTRING:
									        {
										        Lua.TString str = Lua.rawtsvalue(key);
										        keyObject = str.ToString();
									        }break;
									        case Lua.LUA_TNUMBER:
										        keyObject = Lua.nvalue(key);
										        break;
									        default:
										        break;
								        }
	            				
	            				        Lua.lua_TValue val = Lua.luaH_get(ot, key);
	            				        Object valObject = null;
	            				        switch(Lua.ttype(val))
	            				        {
	            					        case Lua.LUA_TNIL:
	            						        break;
	            					        case Lua.LUA_TSTRING:
	            					        {
	            						        Lua.TString str = Lua.rawtsvalue(val);
	            						        valObject = str.ToString();
	            					        }break;
	            					        case Lua.LUA_TNUMBER:
	            						        valObject = Lua.nvalue(val);
	            						        break;
	            					        case Lua.LUA_TTABLE:
	            						        valObject = ParseTable(val);
	            						        break;
	            					        case Lua.LUA_TUSERDATA:
	            					        {
	            						        valObject = (Lua.rawuvalue(val).user_data);
	            						        if(valObject != null)
	            						        {
		            						        if(valObject.GetType().FullName.StartsWith("ScriptingEngine.LuaObject"))
		            	                            {
                                                        PropertyInfo pi = valObject.GetType().GetProperty("obj");
                                                        if (pi == null)
                                                        {
                                                            Logger.Log(LogType.CONSOLE, LogLevel.ERROR, "Cannot get lua object property static thunk");
                                                            return 0;
                                                        }
                                                        Object objA = pi.GetValue(valObject, null);
		            	
		            	                                valObject = objA;
		            	                            }
	            						        }
	            					        }break;
	            					        case Lua.LUA_TLIGHTUSERDATA:
	            					        {
	            						        valObject = Lua.pvalue(val);
	            						        if(valObject != null)
	            						        {
		            						        if(valObject.GetType().FullName.StartsWith("ScriptingEngine.LuaObject"))
		            	                            {
                                                        PropertyInfo pi = valObject.GetType().GetProperty("obj");
                                                        if (pi == null)
                                                        {
                                                            Logger.Log(LogType.CONSOLE, LogLevel.ERROR, "Cannot get lua object property static thunk");
                                                            return 0;
                                                        }
                                                        Object objA = pi.GetValue(valObject, null);
		            	
		            	                                valObject = objA;
		            	                            }
	            						        }
	            					        }break;
	            				        }
	            				        map.Add(keyObject, valObject);
	            			        }
	            			        argList.Add(map);
            			        }
                            }
                            break;
                    };
                    ++count;
                }


                Object retval = mi.Invoke(null, argList.ToArray());
                Type rettype = null;

                if (retval == null)
                    LuaEngine.Instance.PushNIL();
                else
                {
                    rettype = retval.GetType();
                    switch (rettype.FullName)
                    {
                        case "System.Boolean":
                            LuaEngine.Instance.PushBool(Convert.ToBoolean(retval));
                            break;
                        case "System.Byte":
                        case "System.Int16":
                        case "System.Int32":
                        case "System.UInt16":
                        case "System.UInt32":
                            LuaEngine.Instance.PushInt(Convert.ToInt32(retval));
                            break;
                        case "System.Int64":
                        case "System.UInt64":
                            LuaEngine.Instance.PushInt(Convert.ToInt32(retval));
                            break;
                        case "System.Single":
                            LuaEngine.Instance.PushFloat(Convert.ToSingle(retval));
                            break;
                        case "System.Double":
                            LuaEngine.Instance.PushDouble(Convert.ToDouble(retval));
                            break;
                        case "System.Char":
                        case "System.String":
                            LuaEngine.Instance.PushString(Convert.ToString(retval));
                            break;
                        case "System.Void":
                            LuaEngine.Instance.PushInt(0);
                            break;
                        default:
                            typeof(Lunar<>).MakeGenericType(rettype).GetMethod("push").Invoke(null, new object[] { LuaEngine.Instance.GetState(), retval, true });
                            break;
                    };
                }
            }


            /*RegType *l = static_cast<RegType*>(lua_touserdata(L, lua_upvalueindex(1)));
            //return (obj->*(l->mfunc))(L);  // call member function
            return l->mfunc(L,obj);*/
            return 1;
        }

	    // garbage collection metamethod
        private static int gc_T(Lua.lua_State L) 
	    {
            Int32 ptr = 0;
		    T obj = check(L, 1, out ptr);
		    if(obj == null)
			    return 0;
		    Lua.lua_getfield(L, Lua.LUA_REGISTRYINDEX, "DO NOT TRASH");
		    if(Lua.lua_istable(L, -1))
		    {
                //m_objectReferences.Remove(ptr);
                Lua.lua_getfield(L, -1, typeof(T).FullName.Trim('.'));
			    if(Lua.lua_isnil(L,-1))
			    {
				    obj = default(T);
			    }
		    }
		    Lua.lua_pop(L, 3);
		    return 0;
	    }

        private static int tostring_T(Lua.lua_State L) 
        {
            Lua.lua_pushstring(L, RemoveChar(typeof(T).FullName, '.'));
		    return 1;
	    }
    }
}
