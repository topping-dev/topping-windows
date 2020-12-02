using System;
using System.Net;
using System.Windows;
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
using System.Collections.Generic;
using System.IO;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.ApplicationModel;
#endif

namespace ScriptingEngine
{
    [LuaClass("LuaResource")]
    public class LuaResource : LuaInterface
    {
        /**
	     * This function gets resource from package, if can not it gets from other data location.
	     * @param path root path to search.
	     * @param resName resource name to search
	     * @return LuaStream of resource
	     */
	    [LuaFunction(typeof(String), typeof(String))]
	    public static Object GetResourceAssetSd(String path, String resName)
	    {
		    LuaStream ls = new LuaStream();
		    ls.SetStream(Defines.GetResourceAssetSd(path, resName));
		    return ls;
	    }
	
	    /**
	     * This function gets resource from other data location, if can not it gets from package.
	     * @param path root path to search.
	     * @param resName resource name to search
	     * @return LuaStream of resource
	     */
	    [LuaFunction(typeof(String), typeof(String))]
	    public static Object GetResourceSdAsset(String path, String resName)
	    {
		    LuaStream ls = new LuaStream();
		    ls.SetStream(Defines.GetResourceSdAsset(path, resName));
		    return ls;
	    }
	
	    /**
	     * This function gets resource from package.
	     * @param path root path to search.
	     * @param resName resource name to search
	     * @return LuaStream of resource
	     */
	    [LuaFunction(typeof(String), typeof(String))]
	    public static LuaStream GetResourceAsset(String path, String resName)
	    {
		    LuaStream ls = new LuaStream();
		    ls.SetStream(Defines.GetResourceAsset(path, resName));
		    return ls;
	    }
	
	    /**
	     * This function gets resource from other data location.
	     * @param path root path to search.
	     * @param resName resource name to search
	     * @return LuaStream of resource
	     */
	    [LuaFunction(typeof(String), typeof(String))]
	    public static Object GetResourceSd(String path, String resName)
	    {
		    LuaStream ls = new LuaStream();
		    ls.SetStream(Defines.GetResourceSd(path, resName));
		    return ls;
	    }
	
	    /**
	     * This function gets resource based on defines.lua config
	     * @param path root path to search.
	     * @param resName resource name to search
	     * @return LuaStream of resource
	     */
	    public static LuaStream GetResource(String path, String resName)
	    {
		    //String scriptsRoot = LuaEngine.getInstance().GetScriptsRoot();
		    int primaryLoad = LuaEngine.Instance.GetPrimaryLoad();
		    switch(primaryLoad)
		    {
			    case LuaEngine.EXTERNAL_DATA:
			    {
				    LuaStream ls = new LuaStream();
				    ls.SetStream(Defines.GetResourceSdAsset(path + "/", resName));
				    return ls;
			    }
			    case LuaEngine.INTERNAL_DATA:
                case LuaEngine.RESOURCE_DATA:
			    {
				    LuaStream ls = new LuaStream();
				    ls.SetStream(Defines.GetResourceAsset(path + "/", resName));
				    return ls;
			    }
			    default:
			    {
				    LuaStream ls = new LuaStream();
				    ls.SetStream(Defines.GetResourceAsset(path + "/", resName));
				    return ls;
			    }
		    }
	    }

#if NETFX_CORE
        public static String[] GetResourceDirectories(String startsWith)
        {
            List<String> lst = new List<String>();
            int primaryLoad = LuaEngine.Instance.GetPrimaryLoad();
            switch (primaryLoad)
            {
                case LuaEngine.EXTERNAL_DATA:
                    {
                        bool safe = true;
                        try
                        {
                            String dirPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, LuaEngine.LUA_RESOURCE_FOLDER, LuaEngine.Instance.GetUIRoot());
                            StorageFolder cFolder = StorageFolder.GetFolderFromPathAsync(dirPath).AsTask().Synchronize();
                            IReadOnlyList<StorageFolder> lstFolders = cFolder.GetFoldersAsync().AsTask().Synchronize();
                            foreach(StorageFolder sf in lstFolders)
                            {
                                if(startsWith == null || (startsWith != null && sf.Name.StartsWith(startsWith)))
                                {
                                    lst.Add(sf.Name);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            safe = false;
                        }
                        finally
                        {
                            if (!safe)
                            {
                                String dirPath = Path.Combine(Package.Current.InstalledLocation.Path, LuaEngine.LUA_RESOURCE_FOLDER, LuaEngine.Instance.GetUIRoot());
                                StorageFolder cFolder = StorageFolder.GetFolderFromPathAsync(dirPath).AsTask().Synchronize();
                                IReadOnlyList<StorageFolder> lstFolders = cFolder.GetFoldersAsync().AsTask().Synchronize();
                                foreach (StorageFolder sf in lstFolders)
                                {
                                    if (startsWith == null || (startsWith != null && sf.Name.StartsWith(startsWith)))
                                    {
                                        lst.Add(sf.Name);
                                    }
                                }
                            }
                        }

                        return lst.ToArray();
                    }
                case LuaEngine.INTERNAL_DATA:
                case LuaEngine.RESOURCE_DATA:
                    {
                        String dirPath = Path.Combine(Package.Current.InstalledLocation.Path, LuaEngine.LUA_RESOURCE_FOLDER, LuaEngine.Instance.GetUIRoot());
                        StorageFolder cFolder = StorageFolder.GetFolderFromPathAsync(dirPath).AsTask().Synchronize();
                        IReadOnlyList<StorageFolder> lstFolders = cFolder.GetFoldersAsync().AsTask().Synchronize();
                        foreach (StorageFolder sf in lstFolders)
                        {
                            if (startsWith == null || (startsWith != null && sf.Name.StartsWith(startsWith)))
                            {
                                lst.Add(sf.Name);
                            }
                        }
                        return lst.ToArray();
                    }
            }
            return new String[] { };
        }

        public static String[] GetResourceFiles(String path)
        {
            int primaryLoad = LuaEngine.Instance.GetPrimaryLoad();
            switch (primaryLoad)
            {
                case LuaEngine.EXTERNAL_DATA:
                    {
                        List<String> lst = new List<String>();

                        bool safe = true;
                        try
                        {
                            String dirPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, LuaEngine.LUA_RESOURCE_FOLDER, LuaEngine.Instance.GetUIRoot(), path);
                            StorageFolder cFolder = StorageFolder.GetFolderFromPathAsync(dirPath).AsTask().Synchronize();
                            IReadOnlyList<StorageFile> lstfiles = cFolder.GetFilesAsync().AsTask().Synchronize();
                            foreach(StorageFile file in lstfiles)
                            {
                                lst.Add(file.Name);
                            }
                        }
                        catch (Exception ex)
                        {
                            safe = false;
                        }
                        finally
                        {
                            if (!safe)
                            {
                                String dirPath = Path.Combine(Package.Current.InstalledLocation.Path, LuaEngine.LUA_RESOURCE_FOLDER, LuaEngine.Instance.GetUIRoot(), path);
                                StorageFolder cFolder = StorageFolder.GetFolderFromPathAsync(dirPath).AsTask().Synchronize();
                                IReadOnlyList<StorageFile> lstfiles = cFolder.GetFilesAsync().AsTask().Synchronize();
                                foreach (StorageFile file in lstfiles)
                                {
                                    lst.Add(file.Name);
                                }
                            }
                        }

                        return lst.ToArray();
                    }
                case LuaEngine.INTERNAL_DATA:
                case LuaEngine.RESOURCE_DATA:
                    {
                        List<String> lst = new List<String>();
                        String dirPath = Path.Combine(Package.Current.InstalledLocation.Path, LuaEngine.LUA_RESOURCE_FOLDER, LuaEngine.Instance.GetUIRoot(), path);
                        StorageFolder cFolder = StorageFolder.GetFolderFromPathAsync(dirPath).AsTask().Synchronize();
                        IReadOnlyList<StorageFile> lstfiles = cFolder.GetFilesAsync().AsTask().Synchronize();
                        foreach (StorageFile file in lstfiles)
                        {
                            lst.Add(file.Name);
                        }
                        return lst.ToArray();
                    }
            }
            return new String[] {};
        }
#endif

        #region LuaInterface Members

        public void RegisterEventFunction(string var, LuaTranslator lt)
        {
            
        }

        public string GetId()
        {
            return "LuaResource";
        }

        #endregion
    }
}
