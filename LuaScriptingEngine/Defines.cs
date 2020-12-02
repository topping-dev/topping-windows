using System;
using System.Net;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Input;
using System.IO;
using System.Runtime.Serialization;
using ScriptingEngine;
#if !NETFX_CORE
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO.IsolatedStorage;
using System.Windows.Resources;
#else
using Windows.UI.Xaml;
using Windows.ApplicationModel;
using Windows.Storage;
using System.Threading.Tasks;
#endif

namespace LuaScriptingEngine
{
    public class Defines
    {
        private static ResourceDictionary genericResourceDictionary;
        public static ResourceDictionary GetGenericResourceDictionary()
        {
            if (genericResourceDictionary == null)
            {
#if !NETFX_CORE
                Uri resourceLocater = new Uri("/LuaScriptingEngine;component/Themes/Generic.xaml", System.UriKind.Relative);
#else
                Uri resourceLocater = new Uri("ms-appx:///LuaScriptingEngineWindowsStore/Themes/GenericWinRT.xaml");
#endif
                
                genericResourceDictionary = new ResourceDictionary();
                Application.LoadComponent(genericResourceDictionary, resourceLocater);
            }

            return genericResourceDictionary;
        }

        private static MersenneTwister mt;
        public static MersenneTwister GetRandomGenerator()
        {
            if (mt == null)
                mt = new MersenneTwister(Convert.ToInt32(DateTime.Now.Ticks % Int32.MaxValue));

            return mt;
        }

        public static Stream GetResourceAssetSd(string path, string resName)
        {
#if WINDOWS_PHONE
            StreamResourceInfo sri = Application.GetResourceStream(new Uri(path + resName, UriKind.Relative));
            if (sri != null && sri.Stream != null)
            {
                return sri.Stream;
            }
            else
            {
                IsolatedStorageFile store = GetIsolatedStorageFile();
                if (store.FileExists(path + resName))
                {
                    return store.OpenFile(path + resName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                }
            }
#elif NETFX_CORE
            String filePath = Path.Combine(Package.Current.InstalledLocation.Path, path, resName);
            String dirPath = Path.GetDirectoryName(filePath);
            StorageFolder cFolder = StorageFolder.GetFolderFromPathAsync(dirPath).AsTask().Synchronize();
            Stream s = cFolder.OpenStreamForReadAsync(Path.GetFileName(filePath)).Synchronize();
            if(s == null)
            {
                Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, path, resName);
                dirPath = Path.GetDirectoryName(filePath);
                cFolder = StorageFolder.GetFolderFromPathAsync(dirPath).AsTask().Synchronize();
                s = cFolder.OpenStreamForReadAsync(Path.GetFileName(filePath)).Synchronize();
                return s;
            }
#endif
            return null;
        }

        public static Stream  GetResourceSdAsset(string path, string resName)
        {
#if WINDOWS_PHONE
            IsolatedStorageFile store = GetIsolatedStorageFile();
            if (store.FileExists(path + resName))
            {
                return store.OpenFile(path + resName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            }
            else
            {
                StreamResourceInfo sri = Application.GetResourceStream(new Uri(path + resName, UriKind.Relative));
                if (sri != null && sri.Stream != null)
                {
                    return sri.Stream;
                }
            }
            return null;
#elif NETFX_CORE
            Stream s = null;
            try
            {
                String filePath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, path, resName);
                String dirPath = Path.GetDirectoryName(filePath);
                StorageFolder cFolder = StorageFolder.GetFolderFromPathAsync(dirPath).AsTask().Synchronize();
                s = cFolder.OpenStreamForReadAsync(Path.GetFileName(filePath)).Synchronize();
            }
            catch(Exception ex)
            {
            }
            finally
            {
                if (s == null)
                {
                    String filePath = Path.Combine(Package.Current.InstalledLocation.Path, path, resName);
                    String dirPath = Path.GetDirectoryName(filePath);
                    StorageFolder cFolder = StorageFolder.GetFolderFromPathAsync(dirPath).AsTask().Synchronize();
                    s = cFolder.OpenStreamForReadAsync(Path.GetFileName(filePath)).Synchronize();
                }                    
            }
            return s;
#endif
        }

        public static Stream GetResourceAsset(string path, string resName)
        {
#if WINDOWS_PHONE
            StreamResourceInfo sri = Application.GetResourceStream(new Uri(path + resName, UriKind.Relative));
            if (sri != null && sri.Stream != null)
            {
                return sri.Stream;
            }
#elif NETFX_CORE
            String filePath = Path.Combine(Package.Current.InstalledLocation.Path, path, resName);
            String dirPath = Path.GetDirectoryName(filePath);
            StorageFolder cFolder = StorageFolder.GetFolderFromPathAsync(dirPath).AsTask().Synchronize();
            try
            {
                Stream s = cFolder.OpenStreamForReadAsync(Path.GetFileName(filePath)).Synchronize();
                if (s != null)
                    return s;
            }
            catch
            {

            }
#else
#endif
            return null;
        }

        public static Stream GetResourceSd(string path, string resName)
        {
#if WINDOWS_PHONE
            IsolatedStorageFile store = GetIsolatedStorageFile();
            if (store.FileExists(path + resName))
            {
                return store.OpenFile(path + resName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            }
#elif NETFX_CORE
            String filePath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, path, resName);
            String dirPath = Path.GetDirectoryName(filePath);
            StorageFolder cFolder = StorageFolder.GetFolderFromPathAsync(dirPath).AsTask().Synchronize();
            Stream s = cFolder.OpenStreamForReadAsync(Path.GetFileName(filePath)).Synchronize();
            if (s != null)
                return s;
#else
#endif
            return null;
        }

#if WINDOWS_PHONE
        public static IsolatedStorageFile GetIsolatedStorageFile()
        {
            IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
            return store;
        }
#endif

#if NETFX_CORE
        private static StorageSettings settings = null;
        public static StorageSettings GetStorageSettings()
        {
            if (settings == null)
                settings = new StorageSettings();

            return settings;
        }
#endif

#if WINDOWS_PHONE
        public static String[] GetAllDirectories(String pattern, IsolatedStorageFile store)
        {
            // obtain all the subfolders under the specified folder
            string root = System.IO.Path.GetDirectoryName(pattern);
            if (root != "") root += "/";
            //get all folders
            string[] directories;
            directories = store.GetDirectoryNames(pattern);
            List<string> directoryList = new List<string>(directories);
            //get matched folders
            for (int i = 0, max = directories.Length; i < max; i++)
            {
                string directory = directoryList[i] + "/";
                string[] more = GetAllDirectories(root + directory + "*", store);
                //iterate through each folder and add current path
                for (int j = 0; j < more.Length; j++)
                    more[j] = directory + more[j];
                // add the found folders into the array
                foreach (string sub in more)
                    directoryList.Insert(i + 1, sub);
                i += more.Length;
                max += more.Length;
            }
            return (string[])directoryList.ToArray();
        }

        public static String[] GetAllFiles(String pattern, IsolatedStorageFile store)
        {
            //get all files under specified folder
            string fileString = System.IO.Path.GetFileName(pattern);
            string[] files;
            files = store.GetFileNames(pattern);
            List<string> fileList = new List<string>(files);
            //iterate over the folder and add contained files into array
            foreach (string directory in GetAllDirectories("*", store))
                foreach (string file in store.GetFileNames(directory + "/" + fileString))
                    fileList.Add((directory + "/" + file));
            return (string[])fileList.ToArray();
        }
#endif

        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[16 * 1024];
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, read);
            }
        }

        public static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType
#if NETFX_CORE
                    ()
#endif
                    ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }
                toCheck = toCheck.
#if WINDOWS_PHONE
                BaseType;
#else
                GetBaseType();
#endif
            }
            return false;
        }

        public static T DeepClone<T>(T obj)
        {
            T cloned = default(T);

            var serializer = new DataContractSerializer(typeof(T));
            using (var ms = new MemoryStream())
            {
                serializer.WriteObject(ms, obj);
                ms.Position = 0;
                cloned = (T)serializer.ReadObject(ms);
            }
            return cloned;
        }
    }
}
