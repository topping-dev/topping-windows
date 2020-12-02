using System;
using System.Net;
using System.Windows;
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
using WinRTDatePicker;
#endif

namespace ScriptingEngine.LuaUI
{
    [LuaClass("LGDatePicker")]
    public class LGDatePicker : LGView
    {
        public int startDay = -1;
        public int startMonth = -1;
        public int startYear = -1;

        /**
	     * Creates LGDatePicker Object From Lua.
	     * @param lc
	     * @return LGDatePicker
	     */
	    [LuaFunction(typeof(LuaContext))]
	    public static LGDatePicker Create(LuaContext lc)
	    {
		    return new LGDatePicker(lc);
	    }

        /**
	     * Creates LGDatePicker Object From Lua.
	     * @param lc
	     * @param day
	     * @param month
	     * @param year
	     * @return LGDatePicker
	     */
	    [LuaFunction(typeof(LuaContext))]
	    public static LGDatePicker Create(LuaContext lc, int day, int month, int year)
	    {
		    LGDatePicker dp =  new LGDatePicker(lc);
		    dp.startDay = day;
		    dp.startMonth = month;
		    dp.startYear = year;
		    return dp;
	    }

        public LGDatePicker(LuaContext context)
            : base(context)
        {
        }

        public LGDatePicker(LuaContext context, String luaId)
            : base(context, luaId)
        {
        }

        public override void Setup()
        {
#if NETFX_CORE
            view = new WinRTDatePicker.DatePicker();
#else
            view = new DatePicker();
#endif
        }

        public override void AfterSetup()
        {
            base.AfterSetup();
            DateTime dt = DateTime.Now;
            if (startDay == -1)
            {
                dt = new DateTime(startYear, startMonth, startDay);
            }
#if NETFX_CORE
            ((WinRTDatePicker.DatePicker)view).SelectedDate = dt;
#else
            ((DatePicker)view).Value = dt;
#endif
        }

        public override void RegisterEventFunction(string var, LuaTranslator lt)
        {
            if (var == "Changed")
            {
#if NETFX_CORE
                ((WinRTDatePicker.DatePicker)view).SelectedDateChanged += new EventHandler<SelectedDateChangedEventArgs>(delegate(object sender, SelectedDateChangedEventArgs e)
#else
                ((DatePicker)view).ValueChanged += new EventHandler<DateTimeValueChangedEventArgs>(delegate(object sender, DateTimeValueChangedEventArgs e)
#endif
                {
                    lt.CallIn(sender);
                });
            }
            else
                base.RegisterEventFunction(var, lt);
        }
    }
}
