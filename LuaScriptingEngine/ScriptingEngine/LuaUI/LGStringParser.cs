using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml.Linq;

namespace ScriptingEngine.LuaUI
{
    public class LGStringParser : Singleton<LGStringParser>
    {
        Dictionary<String, Dictionary<Int32, String>> stringMap;

        public void ParseXML(String filename)
        {
            Stream sr = (Stream)LuaResource.GetResource(LuaEngine.Instance.GetUIRoot(), filename).GetStream();
            if (sr != null)
            {
                XDocument doc = XDocument.Load(sr);
            }
        }

        public void ParseXML(int orientation, XElement element)
        {
            if (stringMap == null)
            {
                stringMap = new Dictionary<String, Dictionary<Int32, String>>();
            }

            foreach (XAttribute attr in element.Attributes())
            {
                if (attr.Name.LocalName == "name")
                {
                    Dictionary<Int32, String> oldValue = null;
                    if (stringMap.ContainsKey(attr.Value))
                        oldValue = stringMap[attr.Value];
                    Dictionary<Int32, String> valueDict = new Dictionary<Int32, String>();
                    if ((orientation & DynamicResource.PORTRAIT) > 0)
                    {
                        valueDict.Add(DynamicResource.PORTRAIT, element.Value);
                    }
                    else
                    {
                        if (oldValue != null && oldValue.ContainsKey(DynamicResource.PORTRAIT))
                            valueDict.Add(DynamicResource.PORTRAIT, oldValue[DynamicResource.PORTRAIT]);
                    }
                    if ((orientation & DynamicResource.LANDSCAPE) > 0)
                    {
                        valueDict.Add(DynamicResource.LANDSCAPE, element.Value);
                    }
                    else
                    {
                        if (oldValue != null && oldValue.ContainsKey(DynamicResource.LANDSCAPE))
                            valueDict.Add(DynamicResource.LANDSCAPE, oldValue[DynamicResource.LANDSCAPE]);
                    }

                    if (oldValue != null)
                        stringMap.Remove(attr.Value);
                    stringMap.Add(attr.Value, valueDict);
                    break;
                }
            }
        }

        public String GetString(String key)
        {
            if (stringMap.ContainsKey(key))
            {
                Dictionary<Int32, String> result = stringMap[key];
                if (ResolutionHelper.CurrentOrientation == PageOrientation.Portrait)
                {
                    if (result.ContainsKey(DynamicResource.PORTRAIT))
                        return result[DynamicResource.PORTRAIT];
                }
                else//(ResolutionHelper.CurrentOrientation == PageOrientation.Landscape)
                {
                    if (result.ContainsKey(DynamicResource.LANDSCAPE))
                        return result[DynamicResource.LANDSCAPE];
                }
            }
            return key;
        }
    }
}
