using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Input;
#if !NETFX_CORE
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Resources;
using System.Windows.Media.Imaging;
#else
using Windows.Storage.Streams;
using Windows.ApplicationModel;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
#endif
#if WINDOWS_PHONE
using System.IO.IsolatedStorage;
#endif

namespace LuaScriptingEngine
{
    public class ResourceHelper
    {
#if !NETFX_CORE
        public static string ExecutingAssemblyName
        {
            get
            {
                string name = System.Reflection.Assembly.GetExecutingAssembly().FullName;
                return name.Substring(0, name.IndexOf(','));
            }
        }
#endif

        public static 
#if !NETFX_CORE
            Stream 
#else
            async Task<Stream>
#endif
            GetStream(string relativeUri, string assemblyName)
        {
#if !NETFX_CORE
            StreamResourceInfo res = Application.GetResourceStream(new Uri(assemblyName + ";component/" + relativeUri, UriKind.Relative));
            if (res == null)
            {
                res = Application.GetResourceStream(new Uri(relativeUri, UriKind.Relative));
            }
            if (res != null)
            {
                return res.Stream;
            }
            else
            {
                return null;
            }
#else
            String filePath = Path.Combine(Package.Current.InstalledLocation.Path, relativeUri);
            String dirPath = Path.GetDirectoryName(filePath);
            StorageFolder cFolder = StorageFolder.GetFolderFromPathAsync(dirPath).AsTask().Synchronize();
            Stream s = cFolder.OpenStreamForReadAsync(Path.GetFileName(filePath)).Synchronize();
            if (s != null)
                return s;
            else
                return null;
#endif
        }

        public static 
#if !NETFX_CORE
            Stream
#else
            Task<Stream>
#endif
            GetStream(string relativeUri)
        {
#if !NETFX_CORE
            return GetStream(relativeUri, ExecutingAssemblyName);
#else
            return GetStream(relativeUri, "");
#endif
        }

        public static BitmapImage GetBitmap(string relativeUri, string assemblyName)
        {
#if !NETFX_CORE
            Stream s = GetStream(relativeUri, assemblyName);
            if (s == null) return null;
            using (s)
            {
                BitmapImage bmp = new BitmapImage();
                bmp.SetSource(s);
                return bmp;
            }
#else
            Task<Stream> s = GetStream(relativeUri, "");
            if (s != null)
            {
                BitmapImage bmp = new BitmapImage();
                s.ContinueWith(task =>
                {
                    bmp.SetSourceAsync(task.Result.AsRandomAccessStream());
                });
                return bmp;
            }
            return null;
#endif
        }

        public static BitmapImage GetBitmap(string relativeUri)
        {
            String resourcePath = "Themes/" + relativeUri;
#if !NETFX_CORE
            BitmapImage bi = GetBitmap(resourcePath, ExecutingAssemblyName);
            if (bi == null)
            {
                try
                {
                    using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        using (IsolatedStorageFileStream fileStream = myIsolatedStorage.OpenFile(relativeUri, FileMode.Open, FileAccess.Read))
                        {
                            if (fileStream != null)
                            {
                                bi = new BitmapImage();
                                bi.SetSource(fileStream);
                            }
                        }
                    }
                }
                catch
                {
                    bi = GetBitmap(relativeUri, ExecutingAssemblyName);
                }
            }
            return bi;
#else
            return null;
#endif
        }

        public static string GetString(string relativeUri, string assemblyName)
        {
#if !NETFX_CORE
            Stream s = GetStream(relativeUri, assemblyName);
            if (s == null) return null;
            using (StreamReader reader = new StreamReader(s))
            {
                return reader.ReadToEnd();
            }
#else
            return null;
#endif
        }

        public static string GetString(string relativeUri)
        {
#if !NETFX_CORE
            return GetString(relativeUri, ExecutingAssemblyName);
#else 
            return null;
#endif
        }

#if !NETFX_CORE
        public static FontSource GetFontSource(string relativeUri, string assemblyName)
        {
            Stream s = GetStream(relativeUri, assemblyName);
            if (s == null) return null;
            using (s)
            {
                return new FontSource(s);
            }
        }

        public static FontSource GetFontSource(string relativeUri)
        {
            return GetFontSource(relativeUri, ExecutingAssemblyName);
        }

        public static object GetXamlObject(string relativeUri, string assemblyName)
        {
            string str = GetString(relativeUri, assemblyName);
            if (str == null) return null;
            object obj = System.Windows.Markup.XamlReader.Load(str);
            return obj;
        }

        public static object GetXamlObject(string relativeUri)
        {
            return GetXamlObject(relativeUri, ExecutingAssemblyName);
        }
#endif

    }
}
