using LuaCSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows;
using System.Xml.Linq;
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
using Windows.Storage;
using Windows.ApplicationModel;
using System.Threading.Tasks;
using LuaScriptingEngine;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Globalization;
using Windows.UI.Xaml;
#endif

namespace ScriptingEngine.LuaUI
{
    public struct DynamicResource
    {
        public static int PORTRAIT = 0x1;
        public static int LANDSCAPE = 0x2;

        public int orientation;
        public Object data;
    }

    [LuaClass("LGParser")]
    public class LGParser : Singleton<LGParser>, LuaInterface
    {
        public LGDrawableParser DrawableParser { get; set; }
        public LGDimensionParser DimensionParser { get; set; }
        public LGColorParser ColorParser { get; set; }
        public LGStringParser StringParser { get; set; }

        private List<List<String>> MatchStringsStart = new List<List<String>>();
        private List<List<String>> MatchStringsEnd = new List<List<String>>();

        public enum MATCH_ID
        {
            LANGUAGE = -1,
            LAYOUT_DIRECTION,
            SMALLEST_WIDTH,
            AVAILABLE_WIDTH,
            AVAILABLE_HEIGHT,
            SCREEN_SIZE,
            SCREEN_ORIENTATION,
            SCREEN_PIXEL_DENSITY,
            VERSION,
            MATCH_ID_COUNT = 9
        }

        public void Initialize(Lua.lua_State L)
        {
            DrawableParser = LGDrawableParser.Instance;
            DimensionParser = LGDimensionParser.Instance;
            ColorParser = LGColorParser.Instance;
            StringParser = LGStringParser.Instance;

            List<String> lst = new List<String>();
            lst.Add("ld");
            MatchStringsStart.Add(lst);
            lst = new List<String>();
            lst.Add("sw");
            MatchStringsStart.Add(lst);
            lst = new List<String>();
            lst.Add("w");
            MatchStringsStart.Add(lst);
            lst = new List<String>();
            lst.Add("h");
            MatchStringsStart.Add(lst);
            lst = new List<String>();
            lst.Add("small");
            lst.Add("normal");
            lst.Add("large");
            lst.Add("xlarge");
            MatchStringsStart.Add(lst);
            lst = new List<String>();
            lst.Add("port");
            lst.Add("land");
            MatchStringsStart.Add(lst);
            lst = new List<String>();
            lst.Add("ldpi");
            lst.Add("mdpi");
            lst.Add("hdpi");
            lst.Add("xhdpi");
            lst.Add("nodpi");
            MatchStringsStart.Add(lst);
            lst = new List<String>();
            lst.Add("v");
            MatchStringsStart.Add(lst);

            lst = new List<String>();
            MatchStringsEnd.Add(lst);
            lst = new List<String>();
            lst.Add("dp");
            MatchStringsEnd.Add(lst);
            lst = new List<String>();
            lst.Add("dp");
            MatchStringsEnd.Add(lst);
            lst = new List<String>();
            lst.Add("dp");
            MatchStringsEnd.Add(lst);
            lst = new List<String>();
            MatchStringsEnd.Add(lst);
            lst = new List<String>();
            MatchStringsEnd.Add(lst);
            lst = new List<String>();
            MatchStringsEnd.Add(lst);
            lst = new List<String>();

            switch (LuaEngine.Instance.GetPrimaryLoad())
            {
                case LuaEngine.EXTERNAL_DATA:
                    {
#if NETFX_CORE
                        StorageFolder internalScriptsFolder = null;
                        /*String[] scriptRootArr = scriptsRoot.Split('/');
                        foreach (String scriptRootPart in scriptRootArr)*/
                        String uiRoot = LuaEngine.Instance.GetUIRoot().Replace("/", "\\");
                        internalScriptsFolder = Package.Current.InstalledLocation.GetFolderAsync(uiRoot).AsTask().Synchronize();
                        IReadOnlyList<StorageFile> files = internalScriptsFolder.GetFilesAsync().AsTask().Synchronize();
                        String rootPath = ApplicationData.Current.LocalFolder.Path;
                        StorageFolder rootFolder = ApplicationData.Current.LocalFolder;
                        StorageFolder scriptsFolder = null;
                        if (LuaEngine.Instance.GetForceLoad() > 0)
                        {
                            try
                            {
                                StorageFolder sf = rootFolder.GetFolderAsync(uiRoot).AsTask().Synchronize();
                                sf.DeleteAsync().AsTask().Synchronize();
                            }
                            catch
                            {

                            }
                            scriptsFolder = rootFolder.CreateFolderAsync(uiRoot, CreationCollisionOption.ReplaceExisting).AsTask().Synchronize();
                        }
                        else
                            scriptsFolder = rootFolder.CreateFolderAsync(uiRoot, CreationCollisionOption.OpenIfExists).AsTask().Synchronize();

                        foreach (StorageFile fileSF in files)
                        {
                            String file = fileSF.Name;
                            Stream sr = Defines.GetResourceAsset(uiRoot + "/", file);
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
#endif
                    } break;
                case LuaEngine.INTERNAL_DATA:
                case LuaEngine.RESOURCE_DATA:
                    {
                    } break;
            }

            ParseValues(L);
            DrawableParser.Initialize(L);                             
        }

        private void ParseValues(Lua.lua_State L)
        {
#if WINDOWS_PHONE
            String valueDirectoriesString = "";
            Lua.lua_getglobal(L, "WP7ValueDirectories");
            if (Lua.lua_isstring(L, -1) != 0)
                valueDirectoriesString = Lua.lua_tostring(L, -1).toString();
            Lua.lua_pop(L, 1);

            String[] valueDirectories = valueDirectoriesString.Split(new char[] { ',' });
#else
            String[] valueDirectories = LuaResource.GetResourceDirectories(LuaEngine.LUA_VALUES_FOLDER);
#endif

            List<DynamicResource> clearedDirectoryList = new List<DynamicResource>();
            //Eliminate non version type values
            Tester(valueDirectories, LuaEngine.LUA_VALUES_FOLDER, ref clearedDirectoryList);

#if WINDOWS_PHONE
            String valueFilesString = "";
            Lua.lua_getglobal(L, "WP7ValueFiles");
            if (Lua.lua_isstring(L, -1) != 0)
                valueFilesString = Lua.lua_tostring(L, -1).toString();
            Lua.lua_pop(L, 1);

            String[] valueFiles = valueFilesString.Split(new char[] { ',' });

            //System.Threading.Thread.CurrentThread.CurrentCulture
            //((PhoneApplicationFrame)Application.Current.RootVisual).Orientation

            foreach (String valueFile in valueFiles)
            {
                String[] fileArr = valueFile.Split(new char[] { '|' });
                String directory = valueDirectories[Convert.ToInt32(fileArr[0])];
                bool found = false;
                int orientation = 0;
                foreach (DynamicResource dr in clearedDirectoryList)
                {
                    if (((String)dr.data) == directory)
                    {
                        found = true;
                        orientation = dr.orientation;
                        break;
                    }
                }
                if(!found)
                    continue;
                LuaStream dat = LuaResource.GetResource(LuaEngine.LUA_RESOURCE_FOLDER + "/" + LuaEngine.Instance.GetUIRoot() + "/" + directory + "/", fileArr[1]);
                if (dat.GetStream() != null)
                {
                    XDocument parse;
                    try
                    {

                        parse = XDocument.Load((Stream)dat.GetStream());
                        String name = parse.Root.Name.LocalName;
                        if (name == "resources")
                        {
                            foreach (XElement child in parse.Root.Elements())
                            {
                                String childName = child.Name.LocalName;
                                if (childName == "color")
                                {
                                    ColorParser.ParseXML(orientation, child);
                                }
                                else if (childName == "dimen")
                                {
                                    DimensionParser.ParseXML(orientation, child);
                                }
                                else if (childName == "string")
                                {
                                    StringParser.ParseXML(orientation, child);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }   
#else
            foreach (DynamicResource dr in clearedDirectoryList)
            {
                String[] filesOfClearFolder = LuaResource.GetResourceFiles((String)dr.data);
                foreach (String file in filesOfClearFolder)
                {
                    LuaStream dat = LuaResource.GetResource(LuaEngine.LUA_RESOURCE_FOLDER + "/" + LuaEngine.Instance.GetUIRoot() + "/" + dr.data + "/", file);
                    if (dat.GetStream() != null)
                    {
                        XDocument parse;
                        try
                        {

                            parse = XDocument.Load((Stream)dat.GetStream());
                            String name = parse.Root.Name.LocalName;
                            if (name == "resources")
                            {
                                foreach (XElement child in parse.Root.Elements())
                                {
                                    String childName = child.Name.LocalName;
                                    if (childName == "color")
                                    {
                                        ColorParser.ParseXML(dr.orientation, child);
                                    }
                                    else if (childName == "dimen")
                                    {
                                        DimensionParser.ParseXML(dr.orientation, child);
                                    }
                                    else if (childName == "string")
                                    {
                                        StringParser.ParseXML(dr.orientation, child);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
            }
#endif
        }

        public void Tester(String[] directories, String directoryType, ref List<DynamicResource> clearedDirectoryList)
        {
            foreach (String dirName in directories)
            {
                if (dirName == directoryType) //Lets always add this
                {
                    DynamicResource dr = new DynamicResource();
                    dr.orientation = DynamicResource.PORTRAIT | DynamicResource.LANDSCAPE;
                    dr.data = dirName;
                    clearedDirectoryList.Add(dr);
                }
                else
                {
                    String[] dirResourceTypes = dirName.Split(new char[] { '-' });

                    MATCH_ID count = 0;
                    bool result = false;
                    int orientation = DynamicResource.PORTRAIT | DynamicResource.LANDSCAPE;
                    foreach (String toMatch in dirResourceTypes)
                    {
                        if (toMatch == directoryType)
                            continue;
                        result = false; //Lets check other guys
                        count = Matcher(count, toMatch, out result);
                        if (result == false)
                        {
#if !NETFX_CORE
                            String replaced = System.Threading.Thread.CurrentThread.CurrentCulture.Name.Replace("-", "-r");
                            if (toMatch == System.Threading.Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName
#else
                            String replaced = CultureInfo.CurrentCulture.Name.Replace("-", "-r");
                            if (toMatch == CultureInfo.CurrentCulture.TwoLetterISOLanguageName
#endif
                            || dirName.Contains(replaced))
                            {
                                result = true;
                                count = MATCH_ID.LANGUAGE;
                            }
                        }
                        else
                        {
                            switch (count)
                            {
                                case MATCH_ID.LAYOUT_DIRECTION:
                                    {
                                    } break;
                                case MATCH_ID.SMALLEST_WIDTH:
                                    {
#if !NETFX_CORE
                                        int height = (int)Application.Current.Host.Content.ActualHeight;
                                        int width = (int)Application.Current.Host.Content.ActualWidth;
#else
                                        int height = (int)Window.Current.Bounds.Height;
                                        int width = (int)Window.Current.Bounds.Width;
#endif
                                        int sw = width;
                                        if (height < width)
                                            sw = height;
                                        String swWidthS = toMatch.Substring(2, toMatch.Length - 2);
                                        int swWidth = Convert.ToInt32(swWidthS);
                                        if (sw >= swWidth)
                                            result = true;
                                    } break;
                                case MATCH_ID.AVAILABLE_WIDTH:
                                    {
#if !NETFX_CORE
                                        int sw = (int)Application.Current.Host.Content.ActualWidth;
#else
                                        int sw = (int)Window.Current.Bounds.Width;
#endif
                                        String swWidthS = toMatch.Substring(1, toMatch.Length - 2);
                                        int swWidth = Convert.ToInt32(swWidthS);
                                        if (sw >= swWidth)
                                            result = true;
                                    } break;
                                case MATCH_ID.AVAILABLE_HEIGHT:
                                    {
#if !NETFX_CORE
                                        int sh = (int)Application.Current.Host.Content.ActualHeight;
#else
                                        int sh = (int)Window.Current.Bounds.Height;
#endif
                                        String swWidthS = toMatch.Substring(1, toMatch.Length - 2);
                                        int swWidth = Convert.ToInt32(swWidthS);
                                        if (sh >= swWidth)
                                            result = true;
                                    } break;
                                case MATCH_ID.SCREEN_SIZE:
                                    {
#if WINDOWS_PHONE
                                        if (toMatch == "large")
                                        {
                                            if (System.Environment.OSVersion.Version.Major == 7)
                                                result = true;
                                        }
                                        else if (toMatch == "xlarge")
                                        {
                                            if (System.Environment.OSVersion.Version.Major >= 8)
                                                result = true;
                                        }
#elif NETFX_CORE
                                        if (toMatch == "xlarge")
                                        {
                                            result = true;
                                        }
#else
                                        //Add other platforms here
#endif
                                    } break;
                                case MATCH_ID.SCREEN_ORIENTATION:
                                    {
                                        if (toMatch == "port")
                                            orientation = DynamicResource.PORTRAIT;
                                        else
                                            orientation = DynamicResource.LANDSCAPE;
                                        result = true;
                                    } break;
                                case MATCH_ID.SCREEN_PIXEL_DENSITY:
                                    {
#if NETFX_CORE
                                        if(toMatch == "xhdpi")
                                        {
                                            result = true;
                                        }
#else
                                        if (toMatch == "mdpi")
                                        {
                                            if (ResolutionHelper.CurrentResolution == Resolution.WVGA)
                                                result = true;
                                        }
                                        else if (toMatch == "hdpi")
                                        {
                                            if (ResolutionHelper.CurrentResolution == Resolution.WXGA || ResolutionHelper.CurrentResolution == Resolution.HD720p)
                                                result = true;
                                        }
#endif
                                    } break;
                                case MATCH_ID.VERSION:
                                    {
                                        String versionS = toMatch.Substring(1);
                                        Int32 version = Convert.ToInt32(versionS);
#if WINDOWS_PHONE
                                        int major = System.Environment.OSVersion.Version.Major;
                                        int minor = System.Environment.OSVersion.Version.Minor;

                                        int tempVer = 0;
                                        if (major == 7 && minor > 0 && minor < 10)
                                            tempVer = 9;
                                        else if (major == 7 && minor >= 10)
                                            tempVer = 10;
                                        else if (major == 8)
                                            tempVer = 11;

                                        if (tempVer >= version)
                                            result = true;
#elif NETFX_CORE
                                        if (11 >= version) //WinRt has one version for now
                                            result = true;
#else
#endif
                                    } break;

                            };
                        }
                    }
                    if (result) //We found a match
                    {
                        DynamicResource dr = new DynamicResource();
                        dr.orientation = orientation;
                        dr.data = dirName;
                        clearedDirectoryList.Add(dr);
                    }
                }
            }
        }

        public MATCH_ID Matcher(MATCH_ID count, String toMatch, out bool result)
        {
            bool found = false;
            int lastCount = 0;
            for (int i = (int)count; i < MatchStringsStart.Count; i++)
            {
                lastCount = i;
                List<String> matchList = MatchStringsStart[i];
                for(int j = 0; j < matchList.Count; j++)
                {
                    String s = matchList[j];
                    if (toMatch.StartsWith(s))
                    {
                        List<String> matchListEnd = MatchStringsEnd[i];
                        if (matchListEnd.Count == 0)
                        {
                            found = true;
                            break;
                        }
                        else
                        {
                            String es = matchListEnd[j];
                            if (toMatch.EndsWith(es))
                            {
                                found = true;
                                break;
                            }
                        }
                    }
                }
                if (found)
                    break;
            }
            result = found;
            return (MATCH_ID)lastCount;
        }

        #region LuaInterface Members

        public void RegisterEventFunction(string var, LuaTranslator lt)
        {
            
        }

        public string GetId()
        {
            return "LGParser";
        }

        #endregion
    }
}
