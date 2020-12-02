using System;
using System.Net;
using System.Windows;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Microsoft.Phone.Controls;
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

namespace ScriptingEngine.LuaUI
{
    [LuaClass("LGDimensionParser")]
    public class LGDimensionParser : Singleton<LGDimensionParser>, LuaInterface
    {
        Dictionary<String, Dictionary<Int32, Int32>> dimensionMap;

        public LGDimensionParser()
        {
            dimensionMap = new Dictionary<String, Dictionary<Int32, Int32>>();
        }

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
            foreach (XAttribute attr in element.Attributes())
            {
                if (attr.Name.LocalName == "name")
                {
                    Dictionary<Int32, Int32> oldValue = null;
                    if (dimensionMap.ContainsKey(attr.Value))
                        oldValue = dimensionMap[attr.Value];
                    Dictionary<Int32, Int32> valueDict = new Dictionary<Int32, Int32>();
                    if ((orientation & DynamicResource.PORTRAIT) > 0)
                    {
                        valueDict.Add(DynamicResource.PORTRAIT, DisplayMetrics.readSize(element.Value));
                    }
                    else
                    {
                        if (oldValue != null && oldValue.ContainsKey(DynamicResource.PORTRAIT))
                            valueDict.Add(DynamicResource.PORTRAIT, oldValue[DynamicResource.PORTRAIT]);
                    }
                    if ((orientation & DynamicResource.LANDSCAPE) > 0)
                    {
                        valueDict.Add(DynamicResource.LANDSCAPE, DisplayMetrics.readSize(element.Value));
                    }
                    else
                    {
                        if (oldValue != null && oldValue.ContainsKey(DynamicResource.LANDSCAPE))
                            valueDict.Add(DynamicResource.LANDSCAPE, oldValue[DynamicResource.LANDSCAPE]);
                    }

                    if (oldValue != null)
                        dimensionMap.Remove(attr.Value);
                    dimensionMap.Add(attr.Value, valueDict);
                    break;
                }
            }
        }

        [LuaFunction(typeof(String))]
        public Int32 GetDimension(String key)
        {
            if (key != null && dimensionMap.ContainsKey(key))
            {
                Dictionary<Int32, Int32> result = dimensionMap[key];
                if (ResolutionHelper.CurrentOrientation == PageOrientation.Portrait)
                {
                    if(result.ContainsKey(DynamicResource.PORTRAIT))
                        return result[DynamicResource.PORTRAIT];
                }
                else//(ResolutionHelper.CurrentOrientation == PageOrientation.Landscape)
                {
                    if (result.ContainsKey(DynamicResource.LANDSCAPE))
                        return result[DynamicResource.LANDSCAPE];
                }
            }
            return DisplayMetrics.readSize(key);
        }

        #region LuaInterface Members

        public void RegisterEventFunction(string var, LuaTranslator lt)
        {
            
        }

        public string GetId()
        {
            return "LGDimensionParser";
        }

        #endregion
    }
}
