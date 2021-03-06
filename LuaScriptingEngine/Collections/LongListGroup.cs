﻿using System;
using System.Net;
using System.Windows;
using System.Collections.Generic;
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

namespace LuaScriptingEngine.Collections.Generic
{
    public class LongListGroup<TKey, TElement> : List<TElement>
    {
        public TKey Key { get; private set; }

        public LongListGroup(TKey key)
            : base()
        {
            Key = key;
        }
    }
}
