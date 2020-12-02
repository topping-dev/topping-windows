using System;
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

namespace ScriptingEngine.LuaUI
{
    [LuaClass("LGRadioGroup")]
    public class LGRadioGroup : LGView
    {
        String GroupName;
        List<LGRadioButton> buttonList;
        //Buttonları listeye ekleyecez
        //Eklerken but.groupname = this.groupname
        public LGRadioGroup(LuaContext context)
            : base(context)
        {
        }

        public LGRadioGroup(LuaContext context, String luaId)
            : base(context, luaId)
        {
        }

        public override void Setup()
        {
        }
    }
}
