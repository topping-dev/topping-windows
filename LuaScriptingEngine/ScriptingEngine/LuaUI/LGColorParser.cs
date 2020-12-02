using System;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Microsoft.Phone.Controls;
#if !NETFX_CORE
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
#else
using Windows.UI;
#endif

namespace ScriptingEngine.LuaUI
{
    [LuaClass("LGColorParser")]
    public class LGColorParser : Singleton<LGColorParser>, LuaInterface
    {
        public Dictionary<String, Dictionary<Int32, Color>> colorMap;

        public LGColorParser()
        {
            colorMap = new Dictionary<String, Dictionary<Int32, Color>>();
            Dictionary<Int32, Color> transparent = new Dictionary<Int32, Color>();
            transparent.Add(DynamicResource.PORTRAIT, Colors.Transparent);
            transparent.Add(DynamicResource.LANDSCAPE, Colors.Transparent);
            colorMap.Add("transparent", transparent);
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
                    Dictionary<Int32, Color> oldValue = null;
                    if (colorMap.ContainsKey(attr.Value))
                        oldValue = colorMap[attr.Value];
                    Dictionary<Int32, Color> valueDict = new Dictionary<Int32, Color>();
                    if ((orientation & DynamicResource.PORTRAIT) > 0)
                    {
                        valueDict.Add(DynamicResource.PORTRAIT, ParseColor(element.Value));
                    }
                    else
                    {
                        if (oldValue != null && oldValue.ContainsKey(DynamicResource.PORTRAIT))
                            valueDict.Add(DynamicResource.PORTRAIT, oldValue[DynamicResource.PORTRAIT]);
                    }
                    if ((orientation & DynamicResource.LANDSCAPE) > 0)
                    {
                        valueDict.Add(DynamicResource.LANDSCAPE, ParseColor(element.Value));
                    }
                    else
                    {
                        if (oldValue != null && oldValue.ContainsKey(DynamicResource.LANDSCAPE))
                            valueDict.Add(DynamicResource.LANDSCAPE, oldValue[DynamicResource.LANDSCAPE]);
                    }

                    if (oldValue != null)
                        colorMap.Remove(attr.Value);
                    colorMap.Add(attr.Value, valueDict);
                    break;
                }
            }
        }

        [LuaFunction(typeof(String))]
        public Color ParseColor(String color)
        {
            String[] arr = color.Split('/');
	        Color? retVal = null;

            if (arr.Length > 1 && colorMap.ContainsKey(arr[1]))
            {
                if (colorMap.ContainsKey(arr[1]))
                {
                    Dictionary<Int32, Color> result = colorMap[arr[1]];
                    if (ResolutionHelper.CurrentOrientation == PageOrientation.Portrait)
                    {
                        if (result.ContainsKey(DynamicResource.PORTRAIT))
                            retVal = result[DynamicResource.PORTRAIT];
                    }
                    else//(ResolutionHelper.CurrentOrientation == PageOrientation.Landscape)
                    {
                        if (result.ContainsKey(DynamicResource.LANDSCAPE))
                            retVal = result[DynamicResource.LANDSCAPE];
                    }
                }
            }
	        if(retVal == null && color.StartsWith("#"))
		        retVal = ParseColorInternal(color);
            //if (retVal == null);// retVal = Colors.Transparent;
	        return (Color)retVal;
        }

        private Color ParseColorInternal(String color)
        {
            byte aI = 255;
            byte rI = 0;
            byte gI = 0;
            byte bI = 0;
	        if(color.Length == 7)
	        {
                rI = (byte)Int32.Parse(color.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
                gI = (byte)Int32.Parse(color.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
                bI = (byte)Int32.Parse(color.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
	        }
	        else if(color.Length == 9)
	        {
                aI = (byte)Int32.Parse(color.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
                rI = (byte)Int32.Parse(color.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
                gI = (byte)Int32.Parse(color.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
                bI = (byte)Int32.Parse(color.Substring(7, 2), System.Globalization.NumberStyles.HexNumber);	
	        }
            return Color.FromArgb(aI, rI, gI, bI);
        }

        #region LuaInterface Members

        public void RegisterEventFunction(string var, LuaTranslator lt)
        {
            
        }

        public string GetId()
        {
            return "LGColorParser";
        }

        #endregion
    }
}
