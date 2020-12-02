using System;

using System.Collections.Generic;
using System.Text;
using System.IO;
using LoggerNamespace;
using System.Reflection;
using LuaCSharp;
using System.Windows;
#if !NETFX_CORE
using System.Windows.Resources;
#else
using Windows.UI.Xaml;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage;
using Windows.ApplicationModel;
using System.Threading.Tasks;
#endif
using System.Net;
using LuaScriptingEngine;
using ScriptingEngine.LuaUI;
#if WINDOWS_PHONE
using System.IO.IsolatedStorage;
#endif

namespace ScriptingEngine
{
    public class LuaEngine : Singleton<LuaEngine>
    {
        public const String LUA_RESOURCE_FOLDER = "LuaEngine";
        public const String LUA_DRAWABLE_FOLDER = "drawable";
        public const String LUA_VALUES_FOLDER = "values";

	    //reg type defines
	    public const int REGTYPE_GUI = (1 << 0);

        public enum GuiEvents
        {
            GUI_EVENT_ZERO,
            GUI_EVENT_CREATE,
            GUI_EVENT_RESUME,
            GUI_EVENT_PAUSE,
            GUI_EVENT_DESTROY,
            GUI_EVENT_UPDATE,
            GUI_EVENT_PAINT,
            GUI_EVENT_MOUSEDOWN,
            GUI_EVENT_MOUSEUP,
            GUI_EVENT_MOUSEMOVE,
            GUI_EVENT_ADAPTERVIEW,
            GUI_EVENT_EVENT,
            GUI_EVENT_KEYDOWN,
            GUI_EVENT_KEYUP,
            GUI_EVENT_NFC,
            GUI_EVENT_COUNT
        }

        struct LuaGuiBinding { public Dictionary<GuiEvents, String> Functions; }
    	
	    Dictionary<String, LuaGuiBinding> guiBinding = new Dictionary<String, LuaGuiBinding>();
    	
	    public Lua.lua_State L;
        List<Lua.lua_State> pendingThreads;

        String scriptsRoot = LUA_RESOURCE_FOLDER;
        int primaryLoad = RESOURCE_DATA;
        double forceLoad = 0;
        String uiRoot = LUA_RESOURCE_FOLDER;
        String mainUI;
        String mainForm;
        public const int EXTERNAL_DATA = 1;
        public const int INTERNAL_DATA = 2;
        public const int RESOURCE_DATA = 3;

        public void Startup()
        {
            L = Lua.lua_open();
            pendingThreads = new List<Lua.lua_State>();

            Lua.luaL_openlibs(L);
            RegisterCoreFunctions();
            RegisterGlobals();

#if WINDOWS_PHONE
            LoadGenericTheme();
#endif

            String defines_lua = "";
            using (Stream myFileStream = (Stream) Defines.GetResourceAsset("LuaEngine/", "defines.lua")) 
            {
                if (myFileStream.CanRead)
                {
                    StreamReader myStreamReader = new StreamReader(myFileStream);
                    defines_lua = myStreamReader.ReadToEnd();
                }
            }

            if(Lua.luaL_loadstring(L, defines_lua) != 0)
            {
                Report(L);
            }
            else
            {
                if(Lua.lua_pcall(L, 0, 0, 0) != 0)
                    Report(L);
                else
                    Logger.Log(LogType.CONSOLE, LogLevel.INFORM, "LuaEngine: Script defines.lua loaded.");
            }
        
		    Lua.lua_getglobal(L, "ScriptsRoot");
		    if(Lua.lua_isstring(L, -1) == 0)
			    Log.e("LuaEngine.java", "ScriptsRoot must be string");
		    else
			    scriptsRoot = scriptsRoot + "/" + Lua.lua_tostring(L, -1).toString();
		    Lua.lua_pop(L, 1);
		
		    primaryLoad = 1;
		    Lua.lua_getglobal(L, "PrimaryLoad");
		    if(Lua.lua_isnumber(L, -1) == 0)
			    Log.e("LuaEngine.java", "PrimaryLoad must be number");
		    else
			    primaryLoad = Lua.lua_tointeger(L, -1);
		    Lua.lua_pop(L, 1);
		
		    forceLoad = 0;
		    Lua.lua_getglobal(L, "ForceLoad");
		    if(Lua.lua_isnumber(L, -1) == 0)
			    Log.e("LuaEngine.java", "ForceLoad must be number");
		    else
			    forceLoad = Lua.lua_tointeger(L, -1);
		    Lua.lua_pop(L, 1);
		
		    Lua.lua_getglobal(L, "UIRoot");
		    if(Lua.lua_isstring(L, -1) == 0)
			    Log.e("LuaEngine.java", "UIRoot must be string");
		    else
                uiRoot = /*uiRoot + "/" +*/ Lua.lua_tostring(L, -1).toString();
		    Lua.lua_pop(L, 1);
		
		    mainUI = "main.xml";
		    Lua.lua_getglobal(L, "MainUI");
		    if(Lua.lua_isstring(L, -1) == 0)
			    Log.e("LuaEngine.java", "MainUI must be string");
		    else
			    mainUI = Lua.lua_tostring(L, -1).toString();
		    Lua.lua_pop(L, 1);
		
		    mainForm = "";
		    Lua.lua_getglobal(L, "MainForm");
		    if(Lua.lua_isstring(L, -1) == 0)
			    Log.e("LuaEngine.java", "MainForm must be string");
		    else
			    mainForm = Lua.lua_tostring(L, -1).toString();
		    Lua.lua_pop(L, 1);
		
		    Lua.lua_getglobal(L, "SocketBufferSize");
		    if(Lua.lua_isnumber(L, -1) == 0)
			    Log.i("LuaEngine.java", "SocketBufferSize not set using default");
		    else
			    Lua.STEPSIZE = Lua.lua_tointeger(L, -1);
		    Lua.lua_pop(L, 1);
		
		    Lua.lua_getglobal(L, "LuaBufferSize");
		    if(Lua.lua_isnumber(L, -1) == 0)
			    Log.i("LuaEngine.java", "LuaBufferSize not set using default");
		    else
			    Lua.LUAL_BUFFERSIZE = Lua.lua_tointeger(L, -1);
		    Lua.lua_pop(L, 1);
		
		    Lua.lua_getglobal(L, "PBufferSize");
		    if(Lua.lua_isnumber(L, -1) == 0)
			    Log.i("LuaEngine.java", "PBufferSize not set using default");
		    else
			    pBuffer.BUF_SIZE = Lua.lua_tointeger(L, -1);
		    Lua.lua_pop(L, 1);

#if WINDOWS_PHONE
            String scriptfiles_lua = "";
            using (Stream myFileStream = (Stream)Defines.GetResourceAsset(LUA_RESOURCE_FOLDER + "/", "scriptfiles.lua"))
            {
                if (myFileStream.CanRead)
                {
                    StreamReader myStreamReader = new StreamReader(myFileStream);
                    scriptfiles_lua = myStreamReader.ReadToEnd();
                }
            }

            if (Lua.luaL_loadstring(L, scriptfiles_lua) != 0)
            {
                Report(L);
            }
            else
            {
                if (Lua.lua_pcall(L, 0, 0, 0) != 0)
                    Report(L);
                else
                    Logger.Log(LogType.CONSOLE, LogLevel.INFORM, "LuaEngine: Script scriptfiles.lua loaded.");
            }

            String filesString = "";
            Lua.lua_getglobal(L, "WP7Files");
		    if(Lua.lua_isstring(L, -1) == 0)
			    Log.i("LuaEngine.java", "WindowsFiles must be string");
		    else
			    filesString = Lua.lua_tostring(L, -1).toString();
		    Lua.lua_pop(L, 1);
            String[] files = filesString.Split(',');
            StartupDefines(files);

            String resfiles_lua = "";
            using (Stream myFileStream = (Stream)Defines.GetResourceAsset(LUA_RESOURCE_FOLDER + "/", "resfiles.lua"))
            {
                if (myFileStream.CanRead)
                {
                    StreamReader myStreamReader = new StreamReader(myFileStream);
                    resfiles_lua = myStreamReader.ReadToEnd();
                }
            }

            if (Lua.luaL_loadstring(L, resfiles_lua) != 0)
            {
                Report(L);
            }
            else
            {
                if (Lua.lua_pcall(L, 0, 0, 0) != 0)
                    Report(L);
                else
                    Logger.Log(LogType.CONSOLE, LogLevel.INFORM, "LuaEngine: Script resfiles.lua loaded.");
            }
#else
            StartupDefines();
#endif
            LGParser.Instance.Initialize(L);
        }

#if WINDOWS_PHONE
        private void StartupDefines(String[] files)
        {
            switch (primaryLoad)
            {
                case EXTERNAL_DATA:
                    {
                        IsolatedStorageFile isf = Defines.GetIsolatedStorageFile();
                        if (isf.DirectoryExists(scriptsRoot))
                        {
                            String[] fnames = isf.GetFileNames(scriptsRoot + "/*");
                            foreach (String fname in fnames)
                            {
                                isf.DeleteFile(scriptsRoot + "/" + fname);
                            }
                            isf.DeleteDirectory(scriptsRoot);
                        }

                        isf.CreateDirectory(scriptsRoot);
                        //byte[] eof = new byte[] { Convert.ToByte(Lua.EOF) };
                        foreach (String file in files)
                        {
                            Stream sr = Defines.GetResourceAsset(scriptsRoot + "/", file);
                            if (sr != null)
                            {
                                if (!isf.FileExists(scriptsRoot + "/" + file) || forceLoad > 0)
                                {
                                    if (isf.FileExists(scriptsRoot + "/" + file))
                                        isf.DeleteFile(scriptsRoot + "/" + file);

                                    IsolatedStorageFileStream isfs = isf.CreateFile(scriptsRoot + "/" + file);

                                    byte[] buffer = new byte[1024];
                                    int length = 0;
                                    while ((length = sr.Read(buffer, 0, buffer.Length)) > 0)
                                    {
                                        isfs.Write(buffer, 0, length);
                                    }
                                    //isfs.Write(eof, 0, eof.Length);
                                    isfs.Flush();
                                    isfs.Close();
                                }
                                sr.Close();
                            }
                        }
                    } break;
                case INTERNAL_DATA:
                case RESOURCE_DATA:
                    {
                    }break;
            }

            foreach (String s in files)
            {
                if (Lua.luaL_loadfile(L, s) != 0)
                {
                    Report(L);
                }
                else
                {
                    if (Lua.lua_pcall(L, 0, 0, 0) != 0)
                    {
                        Report(L);
                    }
                    else
                    {
                        Log.i("LuaEngine", "Script " + s + " loaded.");
                    }
                }
            }
        }
#elif NETFX_CORE
        private void StartupDefines()
        {
            StorageFolder internalScriptsFolder = null;
            /*String[] scriptRootArr = scriptsRoot.Split('/');
            foreach (String scriptRootPart in scriptRootArr)*/
            scriptsRoot = scriptsRoot.Replace("/", "\\");
                internalScriptsFolder = Package.Current.InstalledLocation.GetFolderAsync(scriptsRoot).AsTask().Synchronize();

            IReadOnlyList<StorageFile> files = internalScriptsFolder.GetFilesAsync().AsTask().Synchronize();
            switch (primaryLoad)
            {
                case EXTERNAL_DATA:
                    {
                        String rootPath = ApplicationData.Current.LocalFolder.Path;
                        StorageFolder rootFolder = ApplicationData.Current.LocalFolder;
                        StorageFolder scriptsFolder = null;
                        if (forceLoad > 0)
                            scriptsFolder = rootFolder.CreateFolderAsync(scriptsRoot, CreationCollisionOption.ReplaceExisting).AsTask().Synchronize();
                        else
                            scriptsFolder = rootFolder.CreateFolderAsync(scriptsRoot, CreationCollisionOption.OpenIfExists).AsTask().Synchronize();

                        foreach (StorageFile fileSF in files)
                        {
                            String file = fileSF.Name;
                            Stream sr = Defines.GetResourceAsset(scriptsRoot + "/", file);
                            StorageFile toWrite = scriptsFolder.CreateFileAsync(file, CreationCollisionOption.ReplaceExisting).AsTask().Synchronize();
                            var toWriteStream = toWrite.OpenTransactedWriteAsync().AsTask().Synchronize();
                            byte[] buffer = new byte[1024];
                            int length = 0;
                            while ((length = sr.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                toWriteStream.Stream.WriteAsync(buffer.AsBuffer()).AsTask().Synchronize();
                            }
                            toWriteStream.CommitAsync().AsTask().Synchronize();
                            toWriteStream.Dispose();
                        }
                    } break;
                case INTERNAL_DATA:
                case RESOURCE_DATA:
                    {
                    } break;
            }

            foreach (StorageFile sf  in files)
            {
                String s = sf.Name;
                if (Lua.luaL_loadfile(L, s) != 0)
                {
                    Report(L);
                }
                else
                {
                    if (Lua.lua_pcall(L, 0, 0, 0) != 0)
                    {
                        Report(L);
                    }
                    else
                    {
                        Log.i("LuaEngine", "Script " + s + " loaded.");
                    }
                }
            }
        }
#endif

#if WINDOWS_PHONE || NETFX_CORE
        public void LoadGenericTheme()
        {
            Application.Current.Resources.Add("EditableComboBoxListBoxItemBorderColor", Theme.Current.AccentColor);
            Application.Current.Resources.Add("EditableComboBoxListBoxItemBorderThickness", "0,0,0,4");
            Application.Current.Resources.Add("EditableComboBoxListBoxItemEllipseWidth",
#if WINDOWS_PHONE
                Application.Current.Host.Content.ActualWidth.ToString());
#elif NETFX_CORE
                Window.Current.Bounds.Width.ToString());
#endif
        }
#endif

        void Report(Lua.lua_State L)
	    {
		    int count = 20;
            Lua.CharPtr ptr = Lua.lua_tostring(L, -1);
            if (ptr == null)
            {
                Logger.Log(LogType.CONSOLE, LogLevel.ERROR, "Possible Error on non-lua function");
                return;
            }
            String msg = ptr.ToString();
            String lastMsg = "";
		    while(msg != null && count > 0)
		    {
                if (lastMsg == msg)
                    break;
                lastMsg = msg;
			    Lua.lua_pop(L, -1);
			    Logger.Log(LogType.CONSOLE, LogLevel.ERROR, "LuaEngine: " + msg);
                ptr = Lua.lua_tostring(L, -1);
                if (ptr == null)
                    break;
			    msg = Lua.lua_tostring(L, -1).ToString();
			    count--;
		    }
	    }
    	
	    void ScriptLoadDir(String root, List<String> rtn)
	    {
#if !NETFX_CORE
            //TODO:?
            FileInfo fi = new FileInfo(root);
            if(Directory.Exists(root))
            {
                foreach (String file in Directory.GetFiles(root))
                {
                    if(Directory.Exists(file))
                        ScriptLoadDir(root + "\\" + file, rtn);
                    else
                    {
			            String[] arr = file.Split('.');
			            if(arr[arr.Length - 1] == "lua")
			            {
				            rtn.Add(file);
			            }
                    }
                }
            }
#endif
	    }
        	
	    void LoadScripts(String root)
	    {
		    List<String> rtn = new List<String>();

#if NETFX_CORE
            String path = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
#else
            String path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
#endif


            ScriptLoadDir(path + "\\" + root, rtn);
    		
		    int cnt_uncomp=0;

		    Lua.luaL_openlibs(L);

            RegisterCoreFunctions();
    		
		    foreach(String s in rtn)
		    {
			    if(Lua.luaL_loadfile(L, s) != 0)
			    {
				    Report(L);
			    }
			    else
			    {
				    if(Lua.lua_pcall(L, 0, 0, 0) != 0)
				    {
					    Report(L);
				    }
				    else
				    {
					    Logger.Log(LogType.CONSOLE, LogLevel.INFORM, "LuaEngine: Script " + s + " loaded.");
				    }				
			    }
			    cnt_uncomp++;
		    }
		    Logger.Log(LogType.CONSOLE, LogLevel.INFORM, "LuaEngine: Loaded " + cnt_uncomp + " Lua scripts.");
	    }
    	
	    public bool BeginCall(String func)
	    {
		    String sFuncName = func;
		    String copy = func;
            String [] st = copy.Split('.', ':');
		    bool colon = false;
            if(copy.IndexOfAny(new char[]{'.', ':'}) == -1)
			    Lua.lua_getglobal(L, func);
		    else
		    {
			    Lua.lua_getglobal(L, "_G"); //start out with the global table.
			    int top = 1;
                
                for(Int32 i = 0; i < st.Length;)
			    {
                    String token = st[i];
				    Lua.lua_getfield(L, -1, token); //get the (hopefully) table/func
                    Int32 charPos = sFuncName.IndexOf(token);
				    if(charPos != -1) //if it isn't the first token
				    {
					    if(sFuncName[charPos] == '.') //if it was a .
						    colon = false;
					    else if(sFuncName[charPos] == ':')
						    colon = true;
				    }
				    else //if it IS the first token, we're OK to remove the "_G" from the stack
					    colon = false;
    				
				    if(Lua.lua_isfunction(L, -1) && !Lua.lua_iscfunction(L, -1)) //if it's a Lua function
				    {
					    Lua.lua_replace(L, top);
					    if(colon)
					    {
						    Lua.lua_pushvalue(L, -1); //make the table the first arg
						    Lua.lua_replace(L, top + 1);
						    Lua.lua_settop(L, top + 1);
					    }
					    else
						    Lua.lua_settop(L, top);
					    break;
				    }
				    else if(Lua.lua_istable(L, -1))
					    i++;
			    }

    			
		    }

		    return colon;
	    }
    	
	    public bool ExecuteCall(int parameters, int res)
	    {
		    if(Lua.lua_pcall(L, parameters, res, 0) > 0)
		    {
			    Report(L);
			    return false;
		    }
		    return true;
	    }
    	
	    public void EndCall(int res)
	    {
		    for(int i = res; i > 0; i--)
		    {
			    if(!Lua.lua_isnone(L, res))
				    Lua.lua_remove(L, res);
		    }
	    }
    	
	    public void CallFunction(String FuncName, int refe)
	    {
		    int top = Lua.lua_gettop(L);
		    int args = 0;
		    String sFuncName = FuncName;
		    String copy = FuncName;
            String [] st = copy.Split('.', ':');
		    bool colon = false;
            if(copy.IndexOfAny(new char[]{'.', ':'}) == -1)
			    Lua.lua_getglobal(L, FuncName);
		    else
		    {
			    Lua.lua_getglobal(L, "_G"); //start out with the global table.
                for(Int32 i = 0; i < st.Length;)
			    {
                    String token = st[i];
				    Lua.lua_getfield(L, -1, token); //get the (hopefully) table/func
                    Int32 charPos = sFuncName.IndexOf(token);
				    if(charPos != -1) //if it isn't the first token
				    {
					    if(sFuncName[charPos] == '.') //if it was a .
						    colon = false;
					    else if(sFuncName[charPos] == ':')
						    colon = true;
				    }
				    else //if it IS the first token, we're OK to remove the "_G" from the stack
					    colon = false;
    				
				    if(Lua.lua_isfunction(L, -1) && !Lua.lua_iscfunction(L, -1)) //if it's a Lua function
				    {
					    if(colon)
					    {
						    Lua.lua_pushvalue(L, -2); //make the table the first arg
						    Lua.lua_remove(L, -3);
						    ++args;
					    }
					    else
						    Lua.lua_remove(L, -2);
					    break;
				    }
				    else if(Lua.lua_istable(L, -1))
					    i++;
			    }			
		    }
		    Lua.lua_rawgeti(L, Lua.LUA_REGISTRYINDEX, refe);
		    Lua.lua_State M = Lua.lua_tothread(L, -1);
		    int thread = Lua.lua_gettop(L);
		    int repeats = Lua.luaL_checkinteger(M, 1); //repeats, args
		    int nargs = Lua.lua_gettop(M) - 1;
		    if(nargs != 0) //if we HAVE args...
		    {
			    for(int i = 2; i <= nargs + 1; i++)
			    {
				    Lua.lua_pushvalue(M, i);
			    }
			    Lua.lua_xmove(M, L, nargs);
		    }
		    if(--repeats == 0) //free stuff, then
		    {
			    Lua.luaL_unref(L, Lua.LUA_REGISTRYINDEX, refe);
		    }
		    else
		    {
			    Lua.lua_remove(M, 1); //args
			    Lua.lua_pushinteger(M, repeats); //args, repeats
			    Lua.lua_insert(M, 1); //repeats, args
		    }
		    Lua.lua_remove(L, thread); //now we can remove the thread object
		    int r = Lua.lua_pcall(L, nargs + args, 0, 0);
		    if(r != 0)
			    Report(L);
    		
		    Lua.lua_settop(L, top);
	    }
    	
	    void FillVariable(Object val)
	    {
            if (val == null)
            {
                PushNIL();
                return;
            }
		    switch(val.GetType().FullName)
		    {
                case "System.Boolean":
                    PushBool(Convert.ToBoolean(val));
                    break;
                case "System.Byte":
                case "System.Int16":
                case "System.Int32":
                case "System.UInt16":
                case "System.UInt32":
                    PushInt(Convert.ToInt32(val));
                    break;
                case "System.Int64":
                case "System.UInt64":
                    PushInt(Convert.ToInt32(val));
                    break;
                case "System.Single":
                    PushFloat(Convert.ToSingle(val));
                    break;
                case "System.Double":
                    PushDouble(Convert.ToDouble(val));
                    break;
                case "System.Char":
                case "System.String":
                    PushString(Convert.ToString(val));
                    break;
                case "System.Void":
                    PushInt(0);
                    break;
                default:
                    if (val is System.Collections.IDictionary)
                        PushTable((System.Collections.IDictionary)val);
                    else
                        typeof(Lunar<>).MakeGenericType(val.GetType()).GetMethod("push").Invoke(null, new object[] { L, val, true });
                    break;
            };
	    }
    	
	    public Object OnGuiEvent(Object gui, String FunctionName, params Object[] arguments)
	    {
		    if(FunctionName == null || FunctionName == "")
			    return null;
    		
		    Lua.lua_pushstring(L, FunctionName);
		    Lua.lua_gettable(L, Lua.LUA_GLOBALSINDEX);
		    if(Lua.lua_isnil(L, -1))
		    {
                Log.e("LuaEngine: ", "Tried to call invalid LUA function '" + FunctionName + "' from Player!\n");
			    return null;
		    }
    		
		    //Lunar<Object>.push(L, gui, false);
            typeof(Lunar<>).MakeGenericType(gui.GetType()).GetMethod("push").Invoke(null, new object[] { L, gui, false });

		    int i = 0;
		    foreach(Object type in arguments)
		    {
			    FillVariable(type);
			    i++;
		    }
    		
		    int r = Lua.lua_pcall(L, i + 1, Lua.LUA_MULTRET, 0);
            if (r != 0)
            {
                Report(L);
                return null;
            }

            Object retVal = null;
            Lua.lua_TValue valTest = new Lua.lua_TValue(L.top);
            valTest.set_index(0);
            if (L.top <= valTest || Lua.lua_isnoneornil(L, -1))
                retVal = null;
            else if (Lua.lua_isboolean(L, -1))
                retVal = (Lua.lua_toboolean(L, -1) == 1) ? true : false;
            else if (Lua.lua_isnumber(L, -1) > 0)
                retVal = Lua.lua_tonumber(L, -1);
            else if (Lua.lua_isstring(L, -1) > 0)
                retVal = Lua.lua_tostring(L, -1).toString();
            else
            {
                //argList.add(Lua.luaL_checkudata(L, count, c.getName()));
                Object o = Lua.lua_touserdata(L, -1);
                if (o != null)
                {
                    if (o.GetType() == typeof(LuaObject<>))
                    {
                        PropertyInfo pi = o.GetType().GetProperty("obj");
                        if (pi == null)
                        {
                            Logger.Log(LogType.CONSOLE, LogLevel.ERROR, "Cannot get lua object property static thunk");
                            return 0;
                        }
                        Object obj = pi.GetValue(o, null);
                        retVal = obj;
                    }
                    else
                    {
                        retVal = o;
                    }
                }
                else
                {
                    o = Lua.lua_topointer(L, -1);
                    if (!o.GetType().FullName.StartsWith(typeof(Lua.Table).FullName))
                        retVal = o;
                    else
                    {
                        Lua.Table ot = (Lua.Table)o;
                        Dictionary<Object, Object> map = new Dictionary<Object, Object>();
                        int size = Lua.sizenode(ot);
                        for (int j = 0; j < size; j++)
                        {
                            Lua.Node node = Lua.gnode(ot, j);
                            Lua.lua_TValue key = Lua.key2tval(node);
                            Object keyObject = null;
                            switch (Lua.ttype(key))
                            {
                                case Lua.LUA_TNIL:
                                    break;
                                case Lua.LUA_TSTRING:
                                    {
                                        Lua.TString str = Lua.rawtsvalue(key);
                                        keyObject = str.ToString();
                                    } break;
                                case Lua.LUA_TNUMBER:
                                    keyObject = Lua.nvalue(key);
                                    break;
                                default:
                                    break;
                            }

                            Lua.lua_TValue val = Lua.luaH_get(ot, key);
                            Object valObject = null;
                            switch (Lua.ttype(val))
                            {
                                case Lua.LUA_TNIL:
                                    break;
                                case Lua.LUA_TSTRING:
                                    {
                                        Lua.TString str = Lua.rawtsvalue(val);
                                        valObject = str.ToString();
                                    } break;
                                case Lua.LUA_TNUMBER:
                                    valObject = Lua.nvalue(val);
                                    break;
                                case Lua.LUA_TTABLE:
                                    valObject = Lunar<Object>.ParseTable(val);
                                    break;
                                case Lua.LUA_TUSERDATA:
                                    {
                                        valObject = (Lua.rawuvalue(val).user_data);
                                        if (valObject != null)
                                        {
                                            if (valObject.GetType() == typeof(LuaObject<>))
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
                                        valObject = Lua.pvalue(val);
                                        if (valObject != null)
                                        {
                                            if (valObject.GetType() == typeof(LuaObject<>))
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
                            map.Add(keyObject, valObject);
                        }
                        retVal = map;
                    }
                }
            }

            return retVal;
	    }

        public Object OnGuiEvent(Object gui, GuiEvents eventType, params Object[] arguments)
	    {
		    //LuaGuiBinding binding = getGuiBinding(gui.Entry);
            LuaInterface li = (LuaInterface)gui;
            if (li == null)
                return null;
            if(!guiBinding.ContainsKey(li.GetId()))
                return null;
            LuaGuiBinding binding = guiBinding[li.GetId()];
		    if(!binding.Functions.ContainsKey(eventType))
			    return null;
    		
		    String FunctionName = binding.Functions[eventType];

		    if(FunctionName == null || FunctionName == "")
			    return null;

		    //m_Lock.Acquire();
		    Lua.lua_pushstring(L, FunctionName);
		    Lua.lua_gettable(L, Lua.LUA_GLOBALSINDEX);
		    if(Lua.lua_isnil(L, -1))
		    {
                Log.e("LuaEngine: ", "Tried to call invalid LUA function '" + FunctionName + "' from Gui!");
			    //m_Lock.Release();
			    return null;
		    }

		    //Lunar<Player>::push(lu, pUnit);
		    //L.pushJavaObject(gui);
            //Lunar<Main>.push(L, (Main)gui, false);
            //Lunar<Object>.push(L, gui, false);
            typeof(Lunar<>).MakeGenericType(gui.GetType()).GetMethod("push").Invoke(null, new object[] { L, gui, false });
            //Lua.lua_pushlightuserdata(L, gui);
		    int i = 0;
		    foreach(Object type in arguments)
		    {
			    FillVariable(type);
			    i++;
		    }
    		
		    int r = Lua.lua_pcall(L, i + 1, Lua.LUA_MULTRET, 0);
		    if(r != 0)
			    Report(L);

            Object retVal = null;
            Lua.lua_TValue valTest = new Lua.lua_TValue(L.top);
            valTest.set_index(0);
            if (L.top <= valTest || Lua.lua_isnoneornil(L, -1))
                retVal = null;
            else if (Lua.lua_isboolean(L, -1))
                retVal = (Lua.lua_toboolean(L, -1) == 1) ? true : false;
            else if (Lua.lua_isnumber(L, -1) > 0)
                retVal = Lua.lua_tonumber(L, -1);
            else if (Lua.lua_isstring(L, -1) > 0)
                retVal = Lua.lua_tostring(L, -1).toString();
            else
            {
                //argList.add(Lua.luaL_checkudata(L, count, c.getName()));
                Object o = Lua.lua_touserdata(L, -1);
                if (o != null)
                {
                    if (o.GetType().FullName.StartsWith("ScriptingEngine.LuaObject"))
                    {
                        PropertyInfo pi = o.GetType().GetProperty("obj");
                        if (pi == null)
                        {
                            Logger.Log(LogType.CONSOLE, LogLevel.ERROR, "Cannot get lua object property static thunk");
                        }
                        Object obj = pi.GetValue(o, null);
                        retVal = obj;
                    }
                    else
                    {
                        retVal = o;
                    }
                }
                else
                {
                    o = Lua.lua_topointer(L, -1);
                    if (!o.GetType().FullName.StartsWith(typeof(Lua.Table).FullName))
                        retVal = o;
                    else
                    {
                        Lua.Table ot = (Lua.Table)o;
                        Dictionary<Object, Object> map = new Dictionary<Object, Object>();
                        int size = Lua.sizenode(ot);
                        for (int j = 0; j < size; j++)
                        {
                            Lua.Node node = Lua.gnode(ot, j);
                            Lua.lua_TValue key = Lua.key2tval(node);
                            Object keyObject = null;
                            switch (Lua.ttype(key))
                            {
                                case Lua.LUA_TNIL:
                                    break;
                                case Lua.LUA_TSTRING:
                                    {
                                        Lua.TString str = Lua.rawtsvalue(key);
                                        keyObject = str.ToString();
                                    } break;
                                case Lua.LUA_TNUMBER:
                                    keyObject = Lua.nvalue(key);
                                    break;
                                default:
                                    break;
                            }

                            Lua.lua_TValue val = Lua.luaH_get(ot, key);
                            Object valObject = null;
                            switch (Lua.ttype(val))
                            {
                                case Lua.LUA_TNIL:
                                    break;
                                case Lua.LUA_TSTRING:
                                    {
                                        Lua.TString str = Lua.rawtsvalue(val);
                                        valObject = str.ToString();
                                    } break;
                                case Lua.LUA_TNUMBER:
                                    valObject = Lua.nvalue(val);
                                    break;
                                case Lua.LUA_TTABLE:
                                    valObject = Lunar<Object>.ParseTable(val);
                                    break;
                                case Lua.LUA_TUSERDATA:
                                    {
                                        valObject = (Lua.rawuvalue(val).user_data);
                                        if (valObject != null)
                                        {
                                            if (valObject.GetType().FullName.StartsWith("ScriptingEngine.LuaObject"))
                                            {
                                                PropertyInfo pi = valObject.GetType().GetProperty("obj");
                                                if (pi == null)
                                                {
                                                    Logger.Log(LogType.CONSOLE, LogLevel.ERROR, "Cannot get lua object property static thunk");
                                                    retVal = null;
                                                }
                                                Object objA = pi.GetValue(valObject, null);

                                                valObject = objA;
                                            }
                                        }
                                    } break;
                                case Lua.LUA_TLIGHTUSERDATA:
                                    {
                                        valObject = Lua.pvalue(val);
                                        if (valObject != null)
                                        {
                                            if (valObject.GetType().FullName.StartsWith("ScriptingEngine.LuaObject"))
                                            {
                                                PropertyInfo pi = valObject.GetType().GetProperty("obj");
                                                if (pi == null)
                                                {
                                                    Logger.Log(LogType.CONSOLE, LogLevel.ERROR, "Cannot get lua object property static thunk");
                                                    retVal = null;
                                                }
                                                Object objA = pi.GetValue(valObject, null);

                                                valObject = objA;
                                            }
                                        }
                                    } break;
                            }
                            map.Add(keyObject, valObject);
                        }
                        retVal = map;
                    }
                }
            }

            if (!(L.top <= valTest))
                Lua.lua_pop(L, 1);
            return retVal;
	    }

#region Functions
        Lua.lua_CFunction FRegisterGuiEvent;
        int RegisterGuiEvent(Lua.lua_State L)
        {
            String entry = Lua.luaL_checkstring(L, 1).ToString();
            int ev = Lua.luaL_checkinteger(L, 2);
            String str = Lua.luaL_checkstring(L, 3).ToString();

            if (entry == "" || ev == 0 || str == null || str == "")
                return 0;

            int top = Lua.lua_gettop(L);
            String sFuncName = str;
            String copy = str;
            String[] st = copy.Split('.', ':');
            bool colon = false;
            if (copy.IndexOfAny(new char[] { '.', ':' }) == -1)
            {
                Lua.lua_getglobal(L, str);
                if (Lua.lua_isfunction(L, -1) && !Lua.lua_iscfunction(L, -1))
                    RegisterEvent(REGTYPE_GUI, entry, ev, str);
                else
                {
                    Logger.Log(LogType.CONSOLE, LogLevel.ERROR, "LuaEngine : RegisterPlayerEvent failed! " + str + " is not a valid Lua function.");
                }
            }
            else
            {
                Lua.lua_getglobal(L, "_G"); //start out with the global table.
                for (Int32 i = 0; i < st.Length; )
                {
                    String token = st[i];
                    Lua.lua_getfield(L, -1, token);
                    if (Lua.lua_isfunction(L, - 1) && !Lua.lua_iscfunction(L, -1))
                    {
                        RegisterEvent(REGTYPE_GUI, entry, ev, str);
                        break;
                    }
                    else if (Lua.lua_istable(L, -1))
                    {
                        i++;
                        continue;
                    }
                    else
                    {
                        Logger.Log(LogType.CONSOLE, LogLevel.ERROR, "LuaEngine : RegisterGuiEvent failed! " + str + " is not a valid Lua function.");
                        break;
                    }
                }
            }
            Lua.lua_settop(L, top);
            return 0;
        }

        /*Lua.lua_CFunction FLog;
        int Log(Lua.lua_State ptr)
        {
            int logType = Lua.luaL_checkinteger(L, 1);
            String where = Lua.luaL_checkstring(L, 2).ToString();
            String msg = Lua.luaL_checkstring(L, 3).ToString();

            switch (logType)
            {
                case 2: //2 verbose
                    {
                        //Logger.Log(LogType.CONSOLE, LogLevel..v(where, msg);
                    } break;
                case 3: //3 debug
                    {
                        Logger.Log(LogType.CONSOLE, LogLevel.DEBUG, where + ":" + msg);
                    } break;
                case 4: //4
                    {
                        Logger.Log(LogType.CONSOLE, LogLevel.INFORM, where + ":" + msg);
                    } break;
                case 5: //5
                    {
                        Logger.Log(LogType.CONSOLE, LogLevel.WARN, where + ":" + msg);
                    } break;
                case 6: //6
                    {
                        Logger.Log(LogType.CONSOLE, LogLevel.ERROR, where + ":" + msg);
                    } break;
                case 7: //7
                    {
                        //Log.(where, msg);
                    } break;
                default:
                    break;
            };

            return 0;
        }*/
#endregion

	    void RegisterCoreFunctions()
	    {
		    //lua_register(lu,"RegisterPlayerEvent",RegisterPlayerEvent); c++ equivalent
		    //in lua.h it is defined as
		    //#define lua_register(L,n,f) (lua_pushcfunction(L, (f)), lua_setglobal(L, (n)))
		    //so we will use pushJavaFunction which is implemented as it in luajava
            FRegisterGuiEvent = new Lua.lua_CFunction(RegisterGuiEvent);
            Lua.lua_register(L, "RegisterGuiEvent", FRegisterGuiEvent);
            /*FLog = new Lua.lua_CFunction(Log);
            Lua.lua_register(L, "Log", FLog);*/

            /*Type[] types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (Type t in types)
            {
                Object[] attrs = t.GetCustomAttributes(true);
                foreach (Object o in attrs)
                {
                    if (o is LuaClass)
                    {
                        typeof(Lunar<>).MakeGenericType(t).GetMethod("Register").Invoke(null, new object[] { L, ((LuaClass)o).LoadAll() });
                    }
                }
                
            }*/
           
		    Lunar<LuaTranslator>.Register(L, false);
		    Lunar<LuaContext>.Register(L, false);
		    Lunar<LuaViewInflator>.Register(L, false);

		    Lunar<LGAdapterView>.Register(L, false);
		    Lunar<LGButton>.Register(L, false);
		    Lunar<LGCheckBox>.Register(L, false);
		    Lunar<LGComboBox>.Register(L, false);
		    Lunar<LGDatePicker>.Register(L, false);
		    Lunar<LGEditText>.Register(L, false);
		    Lunar<LGLinearLayout>.Register(L, false);
		    Lunar<LGListView>.Register(L, false);
		    Lunar<LGProgressBar>.Register(L, false);
		    Lunar<LGRadioButton>.Register(L, false);
		    Lunar<LGRadioGroup>.Register(L, false);
		    Lunar<LGScrollView>.Register(L, false);
		    Lunar<LGTextView>.Register(L, false);
		    Lunar<LGView>.Register(L, false);
		
		    Lunar<LuaDefines>.Register(L, false);
		    Lunar<LuaNativeObject>.Register(L, false);
		    Lunar<LuaObjectStore>.Register(L, false);
		    Lunar<LuaDatabase>.Register(L, false);
		    Lunar<LuaHttpClient>.Register(L, false);
		    Lunar<LuaJSONObject>.Register(L, false);
		    Lunar<LuaJSONArray>.Register(L, false);
		    Lunar<LuaResource>.Register(L, false);
		    Lunar<LuaDialog>.Register(L, false);
		    Lunar<LuaToast>.Register(L, false);
		    Lunar<LuaStore>.Register(L, false);
		
		    Lunar<LuaForm>.Register(L, false);
		    Lunar<LuaTabForm>.Register(L, false);
    		
		    //set the suspendluathread a coroutine function
		    /*lua_getglobal(lu,"coroutine");
		    if(lua_istable(lu,-1) )
		    {
			    lua_pushcfunction(lu,SuspendLuaThread);
			    lua_setfield(lu,-2,"wait");
			    lua_pushcfunction(lu,SuspendLuaThread);
			    lua_setfield(lu,-2,"WAIT");
		    }
		    lua_pop(lu,1);*/
	    }

        void RegisterGlobals()
        {
            //GUIEvents
            int count = 0;
            Lua.lua_pushinteger(L, count++);
            Lua.lua_setglobal(L, "GUIEVENT_ZERO");
            Lua.lua_pushinteger(L, count++);
            Lua.lua_setglobal(L, "GUIEVENT_CREATE");
            Lua.lua_pushinteger(L, count++);
            Lua.lua_setglobal(L, "GUIEVENT_RESUME");
            Lua.lua_pushinteger(L, count++);
            Lua.lua_setglobal(L, "GUIEVENT_PAUSE");
            Lua.lua_pushinteger(L, count++);
            Lua.lua_setglobal(L, "GUIEVENT_DESTROY");
            Lua.lua_pushinteger(L, count++);
            Lua.lua_setglobal(L, "GUIEVENT_UPDATE");
            Lua.lua_pushinteger(L, count++);
            Lua.lua_setglobal(L, "GUIEVENT_PAINT");
            Lua.lua_pushinteger(L, count++);
            Lua.lua_setglobal(L, "GUIEVENT_MOUSEDOWN");
            Lua.lua_pushinteger(L, count++);
            Lua.lua_setglobal(L, "GUIEVENT_MOUSEUP");
            Lua.lua_pushinteger(L, count++);
            Lua.lua_setglobal(L, "GUIEVENT_MOUSEMOVE");
            Lua.lua_pushinteger(L, count++);
            Lua.lua_setglobal(L, "GUIEVENT_ADAPTERVIEW");
            Lua.lua_pushinteger(L, count++);
            Lua.lua_setglobal(L, "GUIEVENT_EVENT");
            Lua.lua_pushinteger(L, count++);
            Lua.lua_setglobal(L, "GUIEVENT_KEYDOWN");
            Lua.lua_pushinteger(L, count++);
            Lua.lua_setglobal(L, "GUIEVENT_KEYUP");
            Lua.lua_pushinteger(L, count++);
            Lua.lua_setglobal(L, "GUIEVENT_NFC");
            Lua.lua_pushinteger(L, count++);
            Lua.lua_setglobal(L, "GUIEVENT_COUNT");

#if WINDOWS_PHONE
            Lua.lua_pushstring(L, "Windows Phone");
#else
            Lua.lua_pushstring(L, "Windows Runtime");
#endif
            Lua.lua_setglobal(L, "OS_TYPE");
#if WINDOWS_PHONE
            Lua.lua_pushinteger(L, Environment.OSVersion.Version.Build);
#else
            Lua.lua_pushinteger(L, 0);
#endif
            Lua.lua_setglobal(L, "OS_VERSION");
        }
    	
	    /*int SuspendLuaThread(LuaState L) 
	    {
		    LuaState thread = (L.isThread(-1)) ? L.toThread(1) : null;
		    if(thread == null)
		    {
			    return Log.e("LuaEngine", "SuspendLuaThread expected Lua coroutine, got NULL.");
		    }
		    int waittime = L.LcheckInteger(2);
		    if(waittime <= 0)
		    {
			    return Log.e("LuaEngine","SuspendLuaThread expected timer > 0 instead got " + waittime);
		    }
    		
		    L.pushValue(1);
		    int ref = L.Lref(L.LUA_REGISTRYINDEX);*/
		    /*
		     * #define LUA_NOREF       (-2)
		     * #define LUA_REFNIL      (-1)
		     */
		    /*if(ref == -1 || ref == -2)
			    return Log.e("LuaEngine", "Error in SuspendLuaThread! Failed to create a valid reference.");
    		
		    L.remove(1); // remove thread object
		    L.remove(1); // remove timer.
		    //All that remains now are the extra arguments passed to this function
		    L.xmove(thread, L.getTop());
		    pendingThreads.add(L);
		    return L.yield(L.getTop());
	    }*/
    	
	    public void Startup(String root)
	    {
		    L = Lua.lua_open();
		    pendingThreads = new List<Lua.lua_State>();
    		
		    LoadScripts(root);
	    }
    	
	    void RegisterEvent(int regtype, String id, int evt, String func) 
	    {
		    if(func != null && func != "" && evt > 0) 
		    {
			    switch(regtype) 
			    {
			    case REGTYPE_GUI:
				    {
					    if(id != "" && evt < ((Int32)GuiEvents.GUI_EVENT_COUNT)) 
					    {
						    if(!guiBinding.ContainsKey(id))
						    {
							    LuaGuiBinding bind = new LuaGuiBinding();
                                bind.Functions = new Dictionary<GuiEvents, String>();
							    bind.Functions.Add((GuiEvents)evt, func);
							    guiBinding.Add(id, bind);
						    }
						    else
						    {
							    LuaGuiBinding bind = guiBinding[id];
							    GuiEvents evtid = (GuiEvents)evt;
							    if(bind.Functions.ContainsKey(evtid))
								    bind.Functions.Remove(evtid);
							    bind.Functions.Add(evtid, func);
						    }
					    }
				    }break;
			    default:
				    break;
			    };
		    }
	    }
    	
	    public void Unload()
	    {
		    Lua.lua_close(L);
            if(guiBinding != null)
		        guiBinding.Clear();
	    }
    	
	    public void Restart(String root)
	    {
		    Unload();
		    Startup(root);
	    }

        public Lua.lua_State GetState() { return L; }

        public void PushBool(bool value) { Lua.lua_pushboolean(L, ((value == true) ? 1 : 0)); }
        public void PushNIL() { Lua.lua_pushnil(L); }
        public void PushInt(Int32 val) { Lua.lua_pushinteger(L, val); }
        public void PushFloat(Single val) { Lua.lua_pushnumber(L, val); }
        public void PushDouble(Double val) { Lua.lua_pushnumber(L, val); }
        public void PushString(String val) { Lua.lua_pushstring(L, val); }

        public void PushTable(System.Collections.IDictionary retVal)
	    {
		    Lua.lua_lock(L);
		    bool created = false;
		    if(!created)
    	    {
    		    created = true;
    		    Lua.lua_createtable(L, 0, retVal.Count);
    	    }

            var enumerator = retVal.GetEnumerator();

            while (enumerator.MoveNext())
            {
			    Object val = enumerator.Value;
                if (val == null)
                {
                    PushNIL();
                    return;
                }
                switch (val.GetType().FullName)
                {
                    case "System.Boolean":
                        PushBool(Convert.ToBoolean(val));
                        break;
                    case "System.Byte":
                    case "System.Int16":
                    case "System.Int32":
                    case "System.UInt16":
                    case "System.UInt32":
                        PushInt(Convert.ToInt32(val));
                        break;
                    case "System.Int64":
                    case "System.UInt64":
                        PushInt(Convert.ToInt32(val));
                        break;
                    case "System.Single":
                        PushFloat(Convert.ToSingle(val));
                        break;
                    case "System.Double":
                        PushDouble(Convert.ToDouble(val));
                        break;
                    case "System.Char":
                    case "System.String":
                        PushString(Convert.ToString(val));
                        break;
                    case "System.Void":
                        PushInt(0);
                        break;
                    default:
                        if (val is System.Collections.IDictionary)
                            PushTable((System.Collections.IDictionary)val);
                        else
                            typeof(Lunar<>).MakeGenericType(val.GetType()).GetMethod("push").Invoke(null, new object[] { L, val, true });
                        break;
                };
        	
			    Lua.lua_setfield(L, -2, Convert.ToString(enumerator.Key));
        	    //Lua.lua_pushstring(L, String.valueOf(entry.getKey()));
        	     /*
                 * To put values into the table, we first push the index, then the
                 * value, and then call lua_rawset() with the index of the table in the
                 * stack. Let's see why it's -3: In Lua, the value -1 always refers to
                 * the top of the stack. When you create the table with lua_newtable(),
                 * the table gets pushed into the top of the stack. When you push the
                 * index and then the cell value, the stack looks like:
                 *
                 * <- [stack bottom] -- table, index, value [top]
                 *
                 * So the -1 will refer to the cell value, thus -3 is used to refer to
                 * the table itself. Note that lua_rawset() pops the two last elements
                 * of the stack, so that after it has been called, the table is at the
                 * top of the stack.
                 */
        	    //Lua.lua_rawset(L, -3);
		    }
		    //Lua.sethvalue(L, obj, x)
		    //Lua.luaH_new(L, narray, nhash)
		    //Lua.setbvalue(L.top, (b != 0) ? 1 : 0); // ensure that true is 1
		    //Lua.api_incr_top(L);
		    Lua.lua_unlock(L);
		
	    }

        Dictionary<String, HttpWebRequest> httpClientMap = new Dictionary<string, HttpWebRequest>();
        public HttpWebRequest GetHttpClient(String id, String url)
        {
            CookieContainer cc = null;
            if (httpClientMap.ContainsKey(id))
            {
                cc = httpClientMap[id].CookieContainer;
            }
            HttpWebRequest client = WebRequest.CreateHttp(url);
            if (cc == null)
                cc = new CookieContainer();
            client.CookieContainer = cc;
            httpClientMap[id] = client;
            return client;
        }

        public void DestroyHttpClient(String id)
	    {
		    if(httpClientMap.ContainsKey(id))
		    {
                HttpWebRequest client = httpClientMap[id];
			    httpClientMap.Remove(id);
			    client = null;
		    }
	    }

        public Lua.lua_State GetLuaState()
        {
            return L;
        }

        public Int32 GetPrimaryLoad()
        {
            return primaryLoad;
        }

        public String GetScriptsRoot()
        {
            return scriptsRoot;
        }

        public String GetUIRoot()
        {
            return uiRoot;
        }

        public String GetMainUI()
        {
            return mainUI;
        }

        public String GetMainForm()
        {
            return mainForm;
        }

        public double GetForceLoad()
        {
            return forceLoad;
        }
    }
}
