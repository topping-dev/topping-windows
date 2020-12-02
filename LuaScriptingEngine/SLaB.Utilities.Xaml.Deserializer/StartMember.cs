﻿using System;
using System.Net;
using System.Windows;
#if !NETFX_CORE
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
#else
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Controls;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Shapes;
using WinRTXamlToolkit.Imaging;
#endif
using System.Linq;

namespace SLaB.Utilities.Xaml.Deserializer
{
    internal class StartMember : XamlNode
    {
        private string _Name;
        internal string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
                var values = _Name.Split('.');
                FullyQualified = values.Length == 2;
                TypeName = values.First();
                MemberName = values.Last();
            }
        }
        internal string Prefix { get; set; }
        internal string TypeName { get; private set; }
        internal string MemberName { get; private set; }
        internal bool FullyQualified { get; private set; }
        internal override NodeType NodeType
        {
            get { return NodeType.StartMember; }
        }
        public override string ToString()
        {
            return "StartMember: " + Prefix + ":" + Name;
        }
    }
}
