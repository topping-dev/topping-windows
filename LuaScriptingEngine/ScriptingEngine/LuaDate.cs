using System;
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
#else
using Windows.UI.Xaml.Controls;
#endif

namespace ScriptingEngine
{
    [LuaClass("LuaDate")]
    public class LuaDate : LuaInterface
    {
        DateTime dateStore;
	
	    /**
	     * Returns the current LuaDate
	     * @return
	     */
	    [LuaFunction(false)]
	    public static LuaDate Now()
	    {
		    LuaDate ret = new LuaDate();
		    ret.dateStore = DateTime.Now;
		    return ret;
	    }
	
	    /**
	     * Creates LuaDate with given parameters
	     * @param day
	     * @param month
	     * @param year
	     * @return
	     */
	    [LuaFunction(typeof(Int32), typeof(Int32), typeof(Int32))]
	    public static LuaDate CreateDate(int day, int month, int year)
	    {
		    LuaDate ret = new LuaDate();
		    ret.dateStore = new DateTime(year, month, day);
		    return ret;
	    }
	
	    /**
	     * Creates LuaDate with given parameters
	     * @param day
	     * @param month
	     * @param year
	     * @param hour
	     * @param minute
	     * @param second
	     * @return
	     */
	    [LuaFunction(typeof(Int32), typeof(Int32), typeof(Int32), typeof(Int32), typeof(Int32), typeof(Int32))]
	    public static LuaDate CreateDateWithTime(int day, int month, int year, int hour, int minute, int second)
	    {
		    LuaDate ret = new LuaDate();
		    ret.dateStore = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Local);
		    return ret;
	    }
	
	    /**
	     * Gets the day of month
	     * @return
	     */
	    [LuaFunction(false)]
	    public Int32 GetDay()
	    {
		    return dateStore.Day;
	    }
	
	    /**
	     * Sets the day of month
	     * @param val
	     */
	    [LuaFunction(typeof(Int32))]
	    public void SetDay(Int32 val)
	    {
		    dateStore = new DateTime(dateStore.Year, dateStore.Year, val, dateStore.Hour, dateStore.Minute, dateStore.Second, dateStore.Millisecond);
	    }
	
	    /**
	     * Gets the month
	     * @return
	     */
	    [LuaFunction(false)]
	    public Int32 GetMonth()
	    {
		    return dateStore.Month;
	    }
	
	    /**
	     * Sets the month
	     * @param val
	     */
	    [LuaFunction(typeof(Int32))]
	    public void SetMonth(Int32 val)
	    {
		    dateStore = new DateTime(dateStore.Year, val, dateStore.Day, dateStore.Hour, dateStore.Minute, dateStore.Second, dateStore.Millisecond);
	    }
	
	    /**
	     * Gets the year
	     * @return
	     */
	    [LuaFunction(false)]
	    public Int32 GetYear()
	    {
		    return dateStore.Year;
	    }
	
	    /**
	     * Sets the year
	     * @param val
	     */
	    [LuaFunction(typeof(Int32))]
	    public void SetYear(Int32 val)
	    {
		    dateStore = new DateTime(val, dateStore.Month, dateStore.Day, dateStore.Hour, dateStore.Minute, dateStore.Second, dateStore.Millisecond);
	    }
	
	    /**
	     * Gets the hour of the day (24)
	     * @return
	     */
	    [LuaFunction(false)]
	    public Int32 GetHour()
	    {
		    return dateStore.Hour;
	    }
	
	    /**
	     * Sets the hour of the day (24)
	     * @param val
	     */
	    [LuaFunction(typeof(Int32))]
	    public void SetHour(Int32 val)
	    {
		    dateStore = new DateTime(dateStore.Year, dateStore.Month, dateStore.Day, val, dateStore.Minute, dateStore.Second, dateStore.Millisecond);
	    }	
	
	    /**
	     * Gets the minute
	     * @return
	     */
	    [LuaFunction(false)]
	    public Int32 GetMinute()
	    {
		    return dateStore.Minute;
	    }
	
	    /**
	     * Set the minute
	     * @param val
	     */
	    [LuaFunction(typeof(Int32))]
	    public void SetMinute(Int32 val)
	    {
		    dateStore = new DateTime(dateStore.Year, dateStore.Month, dateStore.Day, dateStore.Hour, val, dateStore.Second, dateStore.Millisecond);
	    }	
	
	    /**
	     * Gets the Second
	     * @return
	     */
	    [LuaFunction(false)]
	    public Int32 GetSecond()
	    {
		    return dateStore.Second;
	    }
	
	    /**
	     * Sets the second
	     * @param val
	     */
	    [LuaFunction(typeof(Int32))]
	    public void SetSecond(Int32 val)
	    {
		    dateStore = new DateTime(dateStore.Year, dateStore.Month, dateStore.Day, dateStore.Hour, dateStore.Minute, val, dateStore.Millisecond);
	    }
	
	    /**
	     * Gets the millisecond
	     * @return
	     */
	    [LuaFunction(false)]
	    public Int32 GetMilliSecond()
	    {
		    return dateStore.Millisecond;
	    }
	
	    /**
	     * Sets the millisecond
	     * @param val
	     */
	    [LuaFunction(typeof(Int32))]
	    public void SetMilliSecond(Int32 val)
	    {
		    dateStore = new DateTime(dateStore.Year, dateStore.Month, dateStore.Day, dateStore.Hour, dateStore.Minute, dateStore.Second, val);
	    }
	
	    /**
	     * Gets the string representation of date
	     * 
	     * D 	day in year 	(Number) 	189
	     * E 	day of week 	(Text) 	Tuesday
	     * F 	day of week in month 	(Number) 	2 (2nd Wed in July)
	     * G 	era designator 	(Text) 	AD
	     * H 	hour in day (0-23) 	(Number) 	0
	     * K 	hour in am/pm (0-11) 	(Number) 	0
	     * L 	stand-alone month 	(Text/Number) 	July / 07
	     * M 	month in year 	(Text/Number) 	July / 07
	     * S 	fractional seconds 	(Number) 	978
	     * W 	week in month 	(Number) 	2
	     * Z 	time zone (RFC 822) 	(Timezone) 	-0800
	     * a 	am/pm marker 	(Text) 	PM
	     * c 	stand-alone day of week 	(Text/Number) 	Tuesday / 2
	     * d 	day in month 	(Number) 	10
	     * h 	hour in am/pm (1-12) 	(Number) 	12
	     * k 	hour in day (1-24) 	(Number) 	24
	     * m 	minute in hour 	(Number) 	30
	     * s 	second in minute 	(Number) 	55
	     * w 	week in year 	(Number) 	27
	     * y 	year 	(Number) 	2010
	     * z 	time zone 	(Timezone) 	Pacific Standard Time
	     * ' 	escape for text 	(Delimiter) 	'Date='
	     * '' 	single quote 	(Literal) 	'o''clock'
	     * @param frmt
	     * @return
	     */
        [LuaFunction(typeof(String))]
	    public String ToString(String frmt)
	    {
            return dateStore.ToString(frmt);
	    }
	
	    /**
	     * Frees LuaDate.
	     */
	    [LuaFunction(false)]
	    public void Free()
	    {
		
	    }
        #region LuaInterface Members

        public void RegisterEventFunction(string var, LuaTranslator lt)
        {
        }

        public string GetId()
        {
            return "LuaDate";
        }

        #endregion
    }
}
