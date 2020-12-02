using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace System
{
    public class StorageSettings
    {
        ApplicationDataContainer settingsContainer = null;

        public StorageSettings()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            if (!localSettings.Containers.ContainsKey("AppSettings"))
                settingsContainer = localSettings.CreateContainer("AppSettings", ApplicationDataCreateDisposition.Always);
            else
                settingsContainer = localSettings.Containers["AppSettings"];

        }

        public Object this[String key]
        {
            get
            {
                return settingsContainer.Values[key];
            }
            set
            {
                settingsContainer.Values[key] = value;
            }
        }

        public bool Contains(String key)
        {
            return settingsContainer.Values.ContainsKey(key);
        }

        public void Save()
        {
        }

        public bool TryGetValue<T1>(string key, out T1 value)
        {
            if (settingsContainer.Values.ContainsKey(key))
            {
                value = (T1)settingsContainer.Values[key];
                return true;
            }
            value = default(T1);
            return false;
        }
    }
}
