using System;
using System.Net;
using System.Windows;
using System.Windows.Input;
using ScriptingEngine;
using System.Collections.Generic;
using System.Xml.Linq;
using System.IO;
using LoggerNamespace;
using LuaScriptingEngine.CustomControls;
using LuaScriptingEngine;
#if !NETFX_CORE
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
#else
using Windows.UI.Xaml;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.Foundation;
#endif


namespace ScriptingEngine.LuaUI
{
    public struct LayoutParams
    {
        public VerticalAlignment valign;
        public HorizontalAlignment halign;
        public Double Width;
        public Double Height;

        public Int32 mleftInt, mtopInt, mrightInt, mbottomInt;
        public Int32 pleftInt, ptopInt, prightInt, pbottomInt;
        public Int32 Gravity;
        public float Weight;

        public LayoutParams(bool val)
        {
            valign = VerticalAlignment.Top;
            halign = HorizontalAlignment.Left;
            Width = Double.NaN;
            Height = Double.NaN;
            mleftInt = mtopInt = mrightInt = mbottomInt = 0;
            pleftInt = ptopInt = prightInt = pbottomInt = 0;
            Gravity = 0;
            Weight = FrameworkElementExtension.DefaultWeight;
        }

        public void setMargins(Int32 leftInt, Int32 topInt, Int32 rightInt, Int32 bottomInt)
        {
            this.mleftInt = leftInt;
            this.mtopInt = topInt;
            this.mrightInt = rightInt;
            this.mbottomInt = bottomInt;
        }       

        public void setPadding(Int32 leftInt, Int32 topInt, Int32 rightInt, Int32 bottomInt)
        {
            this.pleftInt = leftInt;
            this.ptopInt = topInt;
            this.prightInt = rightInt;
            this.pbottomInt = bottomInt;
        }
    }

    [LuaClass("LuaViewInflator")]
    public class LuaViewInflator : LuaInterface
    {
        Stack<FrameworkElement> layoutStack;
	    Stack<LGView> lgStack;
	    Dictionary<String, Int32> ids;
	    LuaContext lc;
        Dictionary<String, String> namespaces;
	
	    /**
	     * (Ignore)
	     */
	    public LuaViewInflator(LuaContext lc) 
	    {
		    this.layoutStack = new Stack<FrameworkElement>();
		    this.lgStack = new Stack<LGView>();
		    this.ids = new Dictionary<String, Int32>();
		    this.lc = lc;
            this.namespaces = new Dictionary<String, String>();
		    /*DisplayMetrics metrics = DisplayMetrics();
		    ((Activity)lc.GetContext()).getWindowManager().getDefaultDisplay().getMetrics(metrics);
		    DisplayMetrics.density = metrics.density;
		    DisplayMetrics.xdpi = metrics.xdpi;
		    DisplayMetrics.ydpi = metrics.ydpi;
		    DisplayMetrics.scaledDensity = metrics.scaledDensity;*/
	    }
	
	    /**
	     * (Ignore)
	     */
	    /*public String convertStreamToString(java.io.InputStream is) 
	    {
	        try {
	            return new java.util.Scanner(is).useDelimiter("\\A").next();
	        } catch (java.util.NoSuchElementException e) {
	            return "";
	        }
	    }*/
	
	    /**
	     * (Ignore)
	     */
	    public Color parseColor(String val)
	    {
		    if(val == null)
			    return Colors.Transparent;
            return LGParser.Instance.ColorParser.ParseColor(val);
	    }
	
	    /**
	     * Creates LuaViewInflater Object From Lua.
	     * @param lc
	     * @return LuaViewInflater
	     */
        [LuaFunction(typeof(LuaContext))]
        public static LuaViewInflator Create(LuaContext lc)
        {
    	    LuaViewInflator lvi = new LuaViewInflator(lc);
    	    return lvi;
        }
    
        /**
         * Parses xml file
         * @param filename
         * @param parent
         * @return View
         */
        [LuaFunction(typeof(String), typeof(LGView))]
        public LGView ParseFile(String filename, LGView parent)
        {
            XDocument parse;
		    try {
			    Stream stream = (Stream) LuaResource.GetResource(System.IO.Path.Combine(LuaEngine.LUA_RESOURCE_FOLDER, LuaEngine.Instance.GetUIRoot()), filename).GetStream();
                parse = XDocument.Load(stream);
                foreach (XAttribute attr in parse.Root.Attributes())
                {
                    if (attr.IsNamespaceDeclaration)
                    {
                        namespaces.Add(attr.Name.LocalName, attr.Value);
                    }
                }
			    List<LGView> lgRoot = new List<LGView>();
			    FrameworkElement v = inflate(parse, lgRoot);
			    if(v is LGView)
				    return (LGView)v;
			    else
			    {
				    LGLinearLayout lgll = new LGLinearLayout(lc, "");
				    //lgll.addView(v);
				    lgll.SetView(v);
				    foreach(LGView w in lgRoot)
					    lgll.subviews.Add(w);
				    return lgll;
			    }
		    }
		    catch (Exception ex) 
            {
                Log.e("LuaViewInflator.cs", ex.ToString());
            }
		    return null;
        }
    
        /**
         * Frees the native object
         */
        [LuaFunction(false)]
        public void Free()
        {
    	
        }
	
	    /**
	     * (Ignore)
	     */
	    public FrameworkElement inflate(String text) {
		    /*XDocument parse;		
		    try {
			    XmlPullParserFactory factory = XmlPullParserFactory.newInstance();
			    parse = factory.newPullParser();
			    parse.setInput(new StringReader(text));
			    ArrayList<LGView> lgRoot = new ArrayList<LGView>();
			    return inflate(parse, lgRoot);
		    }
		    catch (XmlPullParserException ex) { return null; }
		    catch (IOException ex) { return null; }*/
            throw new NotImplementedException();
	    }
	
        /**
         * (Ignore)
         */
        public FrameworkElement parseInner(XElement parse, List<LGView> lgroot, ref FrameworkElement root)
        {
            FrameworkElement v = createView(parse);
			if(v is LGView)
			{
				if(lgStack.Count > 0)
				{
					LGView vParent = (LGView) lgStack.Peek();
					if(vParent != null)
						vParent.subviews.Add((LGView) v);
					else
						lgroot.Add((LGView) v);
				}
				else
					lgroot.Add((LGView)v);
			}
			if (v == null)
			{
				return null;
			}					
			if (root == null) 
            {
				root = v;
			}
            else
            {
                //TODO:check this?
			    //((LinearLayout)layoutStack.Peek()).Children.Add(v);
			}
			if(v is LGView)
			{
				if (((LGView)v).GetView() is LinearLayout 
                    || ((LGView)v).GetView() is ScrollViewer)
				{
					layoutStack.Push(((LGView)v).GetView());
					lgStack.Push((LGView) v);
				}
			}
			else
			{
                if (v is LinearLayout || v is ScrollViewer)
				{
					layoutStack.Push(v);
				}
			}
            foreach(XElement element in parse.Elements())
            {
                parseInner(element, lgroot, ref root);
            }
            if (isLayout(parse.Name.LocalName))
				layoutStack.Pop();
			if(isLGLayout(parse.Name.LocalName))
				lgStack.Pop();
            return root;
        }

	    /**
	     * (Ignore)
	     */
	    public FrameworkElement inflate(XDocument parse, List<LGView> lgroot)
        {
		    layoutStack.Clear();
		    ids.Clear();

		    FrameworkElement root = null;
            foreach(XElement element in parse.Elements())
            {
                parseInner(element, lgroot, ref root);
            }
            if (root != null)
            {
                if (root is LGLinearLayout)
                    ((LGLinearLayout)root).Populate(null);
            }
		    return root;
	    }
	
        /**
	     * (Ignore)
	     */
	    protected FrameworkElement createView(XElement parse) 
	    {
		    String name = parse.Name.LocalName;
		    FrameworkElement lgresult = null;
		    FrameworkElement result = null;
            IEnumerable<XAttribute> atts = parse.Attributes();
		    String luaId = findAttribute(atts, "lua:id");
            LuaContext context = lc;
		    if(name == "LinearLayout" ||
               name == "LGLinearLayout"){
			    lgresult = new LGLinearLayout(context, luaId);
		    }
		    else if (name == "RadioGroup") {
			    lgresult = new LGRadioGroup(context, luaId);
		    }
		    else if (name == "LGRadioGroup") {
			    lgresult = new LGRadioGroup(context, luaId);
		    }
		    /*else if (name == "TableRow") {
			    lgresult = new LGTableRow(context, luaId);
		    }
		    else if (name == "LGTableRow") {
			    lgresult = new LGTableRow(context, luaId);
		    }
		    else if (name == "TableLayout") {
			    lgresult = new LGTableLayout(context, luaId);
		    }
		    else if (name == "LGTableLayout") {
			    lgresult = new LGTableLayout(context, luaId);
		    }*/
		    /*else if (name.equals("AbsoluteLayout")) {
			    result = new LGAbsoluteLayout(context, luaId);
		    }*/
		    /*else if (name == "RelativeLayout") {
			    lgresult = new LGRelativeLayout(context, luaId);
		    }
		    else if (name == "LGRelativeLayout") {
			    lgresult = new LGRelativeLayout(context, luaId);
		    }*/
		    else if (name == "ScrollView") {
			    lgresult = new LGScrollView(context, luaId);
		    }
		    else if (name == "LGScrollView") {
			    lgresult = new LGScrollView(context, luaId);
		    }
		    /*else if (name == "FrameLayout") {
			    lgresult = new LGFrameLayout(context, luaId);
		    }
		    else if (name == "LGFrameLayout") {
			    lgresult = new LGFrameLayout(context, luaId);
		    }*/
		    else if (name == "TextView") {
			    lgresult = new LGTextView(context, luaId);
		    }
		    else if (name == "LGTextView") {
			    lgresult = new LGTextView(context, luaId);
		    }
		    /*else if (name == "AutoCompleteTextView") {
			    lgresult = new LGAutoCompleteTextView(context, luaId);
		    }
		    else if (name == "LGAutoCompleteTextView") {
			    lgresult = new LGAutoCompleteTextView(context, luaId);
		    }*/
		    /*else if (name.equals("AnalogClock") {
			    result = new LGAnalogClock(context, luaId);
		    }*/
		    else if (name == "Button") {
			    lgresult = new LGButton(context, luaId);
		    }
		    else if (name == "LGButton") {
			    lgresult = new LGButton(context, luaId);
		    }
		    else if (name == "CheckBox") {
			    lgresult = new LGCheckBox(context, luaId);
		    }
		    else if (name == "LGCheckBox") {
			    lgresult = new LGCheckBox(context, luaId);
		    }
		    else if (name == "ComboBox") {
			    lgresult = new LGComboBox(context, luaId);
		    }
		    else if (name == "LGComboBox") {
			    lgresult = new LGComboBox(context, luaId);
		    }
		    else if (name == "DatePicker") {
			    lgresult = new LGDatePicker(context, luaId);
		    }
		    else if (name == "LGDatePicker") {
			    lgresult = new LGDatePicker(context, luaId);
		    }
		    /*else if (name.equals("DigitalClock")) {
			    result = new LGDigitalClock(context, luaId);
		    }*/
		    else if (name == "EditText") {
			    lgresult = new LGEditText(context, luaId);
		    }
		    else if (name == "LGEditText") {
			    lgresult = new LGEditText(context, luaId);
		    }
		    else if (name == "ProgressBar") {
			    lgresult = new LGProgressBar(context, luaId);
		    }
		    else if (name == "LGProgressBar") {
			    lgresult = new LGProgressBar(context, luaId);
		    }
		    else if (name == "RadioButton") {
			    lgresult = new LGRadioButton(context, luaId);
		    }
		    else if (name == "LGRadioButton") {
			    lgresult = new LGRadioButton(context, luaId);
		    }
		    else if (name == "ListView") {
			    lgresult = new LGListView(context, luaId);
		    }
		    else if (name == "LGListView") {
			    lgresult = new LGListView(context, luaId);
		    }
		    else if (name == "ImageView")
		    {
			    lgresult = new LGImageView(context, luaId);
		    }
		    else if (name == "LGImageView")
		    {
			    lgresult = new LGImageView(context, luaId);
		    }
		    else {
                Log.e("LuaViewInflator", "Unhandled tag:"+name);
		    }
		
		    if (lgresult == null)
			    return null;
		
		    if(lgresult is LGView)
			    result = ((LGView)lgresult).GetView();
		    else
			    result = lgresult;
		
		    String id = findAttribute(atts, "android:id");

		    /*if (id != null) {
			    int idNumber = lookupId(id);
			    if (idNumber > -1) {
				    result.setId(idNumber);
			    }
		    }*/
		
		    if (result is TextBox) 
            {
			    TextBox tv = (TextBox)result;
			    String text = findAttribute(atts, "android:text");
			    if (text != null) {
				    text = text.Replace("\\n", "\n");
                    tv.Text = LGStringParser.Instance.GetString(text);
			    }
			    String textColor = findAttribute(atts, "android:textColor");
			    if(textColor != null)
			    {
				    tv.Foreground = new SolidColorBrush(parseColor(textColor));
			    }
			    /*else
				    tv.Foreground = new SolidColorBrush(Colors.Black);*/
			
			    String textSize = findAttribute(atts, "android:textSize");
			    if(textSize != null)
			    {
                    tv.FontSize = LGDimensionParser.Instance.GetDimension(textSize);
			    }
		    }

            if (result is TextBox)
            {
                TextBox tb = (TextBox)result;
                tb.Background = new SolidColorBrush(Colors.Transparent);
            }

            if (result is /*TextBlock*/Border)
            {
                TextBlock tv = (TextBlock)((Border)result).Child;
                String text = findAttribute(atts, "android:text");
                if (text != null)
                {
                    text = text.Replace("\\n", "\n");
                    tv.Text = text;
                }
                String textColor = findAttribute(atts, "android:textColor");
                if (textColor != null)
                {
                    tv.Foreground = new SolidColorBrush(parseColor(textColor));
                }
                /*else
                    tv.Foreground = new SolidColorBrush(Colors.White);*/

                String textSize = findAttribute(atts, "android:textSize");
                if (textSize != null)
                {
                    tv.FontSize = LGDimensionParser.Instance.GetDimension(textSize);
                }

                tv.TextWrapping = TextWrapping.Wrap;
            }

            if (result is Button)
            {
                Button tv = (Button)result;
                String text = findAttribute(atts, "android:text");
                if (text != null)
                {
                    text = text.Replace("\\n", "\n");
                    tv.Content = text;
                }
                String textColor = findAttribute(atts, "android:textColor");
                if (textColor != null)
                {
                    tv.Foreground = new SolidColorBrush(parseColor(textColor));
                }
                /*else
                    tv.Foreground = new SolidColorBrush(Colors.Black);*/

                String textSize = findAttribute(atts, "android:textSize");
                if (textSize != null)
                {
                    tv.FontSize = LGDimensionParser.Instance.GetDimension(textSize);
                }

                /*String minLines = findAttribute(atts, "android:minLines");
                if(minLines != null)
                    tv.setMinLines(Int32.valueOf(minLines));*/
            }
		
		    if(result is Image)
		    {
			    Image iv = (Image)result;
			    String image = findAttribute(atts, "android:src");
                ImageSource isrc = LGParser.Instance.DrawableParser.ParseDrawable(image).img;
                if(isrc != null)
                    iv.Source = isrc;

                iv.Stretch = Stretch.None;
                String stretch = findAttribute(atts, "android:scaleType");
                if (stretch != null)
                {
                    if (stretch == "fitXY")
                        iv.Stretch = Stretch.Fill;
                    else if (stretch == "fitStart"
                        || stretch == "fitEnd"
                        || stretch == "fitCenter")
                        iv.Stretch = Stretch.Uniform;
                    else if (stretch == "centerInside"
                        || stretch == "centerCrop")
                        iv.Stretch = Stretch.UniformToFill;
                }
		    }
		
		    if (result is CheckBox) 
            {
			    CheckBox cb = (CheckBox)result;
			    String check = findAttribute(atts, "android:checked");
                cb.IsChecked = "true" == check;
		    }
		
		    if (result is ProgressBar) {
			    ProgressBar pb = (ProgressBar)result;
			    String indet = findAttribute(atts, "android:indeterminate");
			    if (indet != null) 
                {
                    pb.IsIndeterminate = "true" == indet;
			    }
			    /*String orientation = findAttribute(atts, "android:orientation");
			    if (orientation != null) {
				    if ("horizontal".equals(orientation)) {
					    pb.setOrientation(ProgressBar.HORIZONTAL);
				    }
				    else if ("vertical".equals(orientation)) {
					    pb.setOrientation(ProgressBar.VERTICAL);
				    }
			    }*/
			    String max = findAttribute(atts, "android:max");
			    if (max != null) {
                    pb.Maximum = Convert.ToInt32(max);
			    }
		    }
		
		    if (result is LinearLayout) {
                LinearLayout ll = (LinearLayout)result;
			    String orient = findAttribute(atts, "android:orientation");
			    if (orient != null) {
                    if (orient == "horizontal")
                        ll.Orientation = LayoutSystemOrientation.HORIZONTAL;
                    else if (orient == "vertical")
                        ll.Orientation = LayoutSystemOrientation.VERTICAL;
			    }
		    }
		
		    /*if (result is ) {
			    RadioGroup rg = (RadioGroup)result;
			    String cid = findAttribute(atts, "android:checkedButton");
			    if (cid != null) {
				    rg.check(Int32.parseInt(cid));
			    }
		    }*/
		
		    if (result is FrameworkElement) {
			    FrameworkElement v = (FrameworkElement)result;
			    /* API 11 
			     String alpha = findAttribute(atts, "android:alpha");
			     if (alpha != null) {
				    v.setAlpha(Float.parseFloat(alpha));
			     }
			    */
			    maybeSetBoolean(v, "setClickable", atts, "android:clickable");
			    maybeSetBoolean(v, "setFocusable", atts, "android:focusable");
			    maybeSetBoolean(v, "setHapticFeedbackEnabled", atts, "android:hapticFeedbackEnabled");
			
			    String background = findAttribute(atts, "android:background");
			    if (background != null)
			    {
                    Brush bb = null;
                    
                    ImageSource isrc = LGParser.Instance.DrawableParser.ParseDrawable(background).img;
                    if (isrc != null)
                    {
                        ImageBrush ib = new ImageBrush();
                        ib.ImageSource = isrc;
                        bb = ib;
                    }
                    else
                    {
                        Color c = parseColor(background);
                        bb = new SolidColorBrush(parseColor(background));
                    }
                    if (bb != null)
                    {
                        if (v is Control)
                        {
                            ((Control)v).Background = bb;
                            if (v is Button)
                                ((Button)v).BorderThickness = new Thickness(0);
                        }
                        else if (v is Border)
                            ((Border)v).Background = bb;
                        else if (v is LinearLayout)
                            ((LinearLayout)v).Background = bb;
                    }
			    }
                else
                {
#if NETFX_CORE
                    if (v is ListBox)
                    {
                        //Listbox is not transparent by default on winrt lets make it transparent
                        ((ListBox)result).Background = new SolidColorBrush(Colors.Transparent);
                    }
#endif
                }
			
			    String visibility = findAttribute(atts, "android:visibility");
		        if (visibility != null){
		    	    Visibility code = Visibility.Visible;
		    	    if ("visible" == visibility) {
		    		    code = Visibility.Visible;
		    	    } else if ("invisible" == visibility
                        || "gone" == visibility)
                    {
		    		    code = Visibility.Collapsed;
		    	    }
		    		v.Visibility = code;
		        }
		    }
		
		    if (layoutStack.Count > 0) 
		    {
                FrameworkElement parent = layoutStack.Peek();
                LayoutParams lps = loadLayoutParams(atts, (LGView)lgresult, parent);
			    /*if(lgresult.GetType() == typeof(Panel))
                {*/
                if(lgresult is LGView)
                {
                    LGView w = (LGView)lgresult;
                    w.GetView().VerticalAlignment = lps.valign;
                    w.GetView().HorizontalAlignment = lps.halign;
                    w.GetView().Width = lps.Width;
                    w.GetView().Height = lps.Height;
                    w.GetView().Margin = new Thickness(lps.mleftInt, lps.mtopInt, lps.mrightInt, lps.mbottomInt);
                    if (lps.Weight != FrameworkElementExtension.DefaultWeight)
                    {
                        w.GetView().SetWeight(lps.Weight);
                        parent.IncreaseTotalWeight(lps.Weight);
                    }
                }   

                if (result is ListBox)
                {
                    ((ListBox)result).VerticalContentAlignment = lps.valign;
                    ((ListBox)result).HorizontalContentAlignment = lps.halign;
                    ResourceDictionary rd = Defines.GetGenericResourceDictionary();
                    if(lps.valign == VerticalAlignment.Stretch && lps.halign == HorizontalAlignment.Stretch)
                    {
                        ((ListBox)result).ItemContainerStyle = (Style)rd["ListBoxHorizontalVerticalStretchStyle"];
                    }
                    else if(lps.valign == VerticalAlignment.Stretch)
                    {
                        ((ListBox)result).ItemContainerStyle = (Style)rd["ListBoxVerticalStretchStyle"];
                    }
                    else if(lps.halign == HorizontalAlignment.Stretch)
                    {
                        ((ListBox)result).ItemContainerStyle = (Style)rd["ListBoxHorizontalStretchStyle"];
                    }
                }
                if(result is TextBox)
                {
                    TextBox tv = (TextBox)result;
                    String minLines = findAttribute(atts, "android:minLines");
                    if (minLines != null)
                    {
                        String a = "A";
                        Size aSize = a.Measure(tv.FontSize, tv.FontFamily, tv.FontWeight);
                        tv.Height = aSize.Height * Convert.ToInt32(minLines) * DisplayMetrics.density;
                    }
                }
                if (result is /*TextBlock*/Border)
                {
                    TextBlock tv = (TextBlock)((Border)result).Child;
                    String minLines = findAttribute(atts, "android:minLines");
                    if (minLines != null)
                    {
                        String a = "A";
                        Size aSize = a.Measure(tv.FontSize, tv.FontFamily, tv.FontWeight);
                        tv.Height = aSize.Height * Convert.ToInt32(minLines) * DisplayMetrics.density;
                    }
                }
		    }
		    else
            {
                LayoutParams lps = loadLayoutParams(atts, lgresult);
                if (lgresult is LGView)
                {
                    LGView w = (LGView)lgresult;
                    w.GetView().VerticalAlignment = lps.valign;
                    w.GetView().HorizontalAlignment = lps.halign;
                    w.GetView().Width = lps.Width;
                    w.GetView().Height = lps.Height;
                    w.GetView().Margin = new Thickness(lps.mleftInt, lps.mtopInt, lps.mrightInt, lps.mbottomInt);
                    w.GetView().SetWeight(lps.Weight);
                }

                if (result is ListBox)
                {
                    ((ListBox)result).VerticalContentAlignment = lps.valign;
                    ((ListBox)result).HorizontalContentAlignment = lps.halign;
                    ResourceDictionary rd = Defines.GetGenericResourceDictionary();
                    if (lps.valign == VerticalAlignment.Stretch && lps.halign == HorizontalAlignment.Stretch)
                    {
                        ((ListBox)result).ItemContainerStyle = (Style)rd["ListBoxHorizontalVerticalStretchStyle"];
                    }
                    else if (lps.valign == VerticalAlignment.Stretch)
                    {
                        ((ListBox)result).ItemContainerStyle = (Style)rd["ListBoxVerticalStretchStyle"];
                    }
                    else if (lps.halign == HorizontalAlignment.Stretch)
                    {
                        ((ListBox)result).ItemContainerStyle = (Style)rd["ListBoxHorizontalStretchStyle"];
                    }
                }
                if (result is TextBox)
                {
                    TextBox tv = (TextBox)result;
                    String minLines = findAttribute(atts, "android:minLines");
                    if (minLines != null)
                    {
                        String a = "A";
                        Size aSize = a.Measure(tv.FontSize, tv.FontFamily, tv.FontWeight);
                        tv.Height = aSize.Height * Convert.ToInt32(minLines) * DisplayMetrics.density;
                    }
                }
                if (result is /*TextBlock*/Border)
                {
                    TextBlock tv = (TextBlock)((Border)result).Child;
                    String minLines = findAttribute(atts, "android:minLines");
                    if (minLines != null)
                    {
                        String a = "A";
                        Size aSize = a.Measure(tv.FontSize, tv.FontFamily, tv.FontWeight);
                        tv.Height = aSize.Height * Convert.ToInt32(minLines) * DisplayMetrics.density;
                    }
                }
            }
		    return lgresult;
	    }

        /**
         * (Ignore)
         * Used to clone Layout objects
         */
        public static UIElement Clone(UIElement ret, UIElement toClone)
        {
            if (toClone is TextBox)
            {
                TextBox tv = (TextBox)ret;
                TextBox tvToClone = (TextBox)toClone;
                tv.Text = tvToClone.Text;
                tv.Foreground = new SolidColorBrush(((SolidColorBrush)tvToClone.Foreground).Color);
                if (tvToClone.FontSize != 11)
                    tv.FontSize = tvToClone.FontSize;

                /*String minLines = findAttribute(atts, "android:minLines");
                if(minLines != null)
                    tv.setMinLines(Int32.valueOf(minLines));*/
            }

            if (toClone is Border)
            {
                Border btv = (Border)ret;
                Border btvToClone = (Border)toClone;
                TextBlock tvToClone = (TextBlock)btvToClone.Child;
                TextBlock tv = (TextBlock)btv.Child;
                tv.Text = tvToClone.Text;
                tv.Foreground = new SolidColorBrush(((SolidColorBrush)tvToClone.Foreground).Color);
                if (tvToClone.FontSize != 11)
                    tv.FontSize = tvToClone.FontSize;
            }

            if (toClone is TextBlock)
            {
                TextBlock tv = (TextBlock)ret;
                TextBlock tvToClone = (TextBlock)toClone;
                tv.Text = tvToClone.Text;
                tv.Foreground = new SolidColorBrush(((SolidColorBrush)tvToClone.Foreground).Color);
                if(tvToClone.FontSize != 11)
                    tv.FontSize = tvToClone.FontSize;

                /*String minLines = findAttribute(atts, "android:minLines");
                if(minLines != null)
                    tv.setMinLines(Int32.valueOf(minLines));*/
            }

            if (toClone is Button)
            {
                Button tv = (Button)ret;
                Button tvToClone = (Button)toClone;

                tv.Content = tvToClone.Content;
                tv.Foreground = new SolidColorBrush(((SolidColorBrush)tvToClone.Foreground).Color);
                if (tvToClone.FontSize != 11)
                    tv.FontSize = tvToClone.FontSize;
            }

            if (toClone is Image)
            {
                Image iv = (Image)ret;
                Image ivToClone = (Image)toClone;

                if (ivToClone.Source != null)
                {
                    iv.Source = ivToClone.Source;
                }
                iv.Stretch = ivToClone.Stretch;
            }

            if (toClone is CheckBox)
            {
                CheckBox cb = (CheckBox)ret;
                CheckBox cbToClone = (CheckBox)toClone;
                cb.IsChecked = cbToClone.IsChecked;
            }

            if (toClone is ProgressBar)
            {
                ProgressBar pb = (ProgressBar)ret;
                ProgressBar pbToClone = (ProgressBar)toClone;

                pb.IsIndeterminate = pbToClone.IsIndeterminate;
                pb.Maximum = pbToClone.Maximum;
            }

            /*if (result is ) {
                RadioGroup rg = (RadioGroup)result;
                String cid = findAttribute(atts, "android:checkedButton");
                if (cid != null) {
                    rg.check(Int32.parseInt(cid));
                }
            }*/

            if (toClone is LinearLayout)
            {
                LinearLayout ll = (LinearLayout)ret;
                LinearLayout llToClone = (LinearLayout)toClone;

                ll.Orientation = llToClone.Orientation;
            }

            if(ret != null && ret is FrameworkElement && toClone is FrameworkElement)
            {
                FrameworkElement v = (FrameworkElement)ret;
                FrameworkElement vToClone = (FrameworkElement)toClone;

                v.VerticalAlignment = vToClone.VerticalAlignment;
                v.HorizontalAlignment = vToClone.HorizontalAlignment;
                v.Width = vToClone.Width;
                v.Height = vToClone.Height;
                v.Margin = new Thickness(vToClone.Margin.Left, vToClone.Margin.Top, vToClone.Margin.Right, vToClone.Margin.Bottom);
                v.SetWeight(vToClone.GetWeight());
                v.SetTotalWeight(vToClone.GetTotalWeight());

                if (vToClone is Control)
                {
                    if (((Control)vToClone).Background != null)
                        ((Control)v).Background = ((Control)vToClone).Background;
                }
                else if (vToClone is Border)
                {
                    if(((Border)vToClone).Background != null)
                        ((Border)v).Background = ((Border)vToClone).Background;
                }
                else if (vToClone is LinearLayout)
                {
                    if (((LinearLayout)vToClone).Background != null)
                        ((LinearLayout)v).Background = ((LinearLayout)vToClone).Background;
                }
                v.Visibility = vToClone.Visibility;
            }
            return ret;
        }
	
	    /**
	     * (Ignore)
	     */
	    private bool maybeSetBoolean(FrameworkElement v, String method, IEnumerable<XAttribute> atts, String arg) {
		    return maybeSetBoolean(v, method, findAttribute(atts, arg));
	    }
	
	    /**
	     * (Ignore)
	     */
	    public static bool isLayout(String name) {
		    return name.EndsWith("Layout") ||
				    name == "RadioGroup" ||
				    name == "TableRow" ||
				    name == "ScrollView";
	    }
	
	    /**
	     * (Ignore)
	     */
	    public static bool isLGLayout(String name) {
		    return (name.StartsWith("LG") && name.EndsWith("Layout")) ||
				    name == "LGRadioGroup" ||
				    name == "LGTableRow" ||
				    name == "LGScrollView";
	    }
	
	    /**
	     * (Ignore)
	     */
	    protected int lookupId(String id) {
		    /*int ix = id.indexOf("/");
		    if (ix != -1) {
			    String idName = id.substring(ix+1);
			    Int32 n = ids.get(idName);
			    if (n == null && id.startsWith("@+")) {
				    n = new Int32(idg++);
				    ids.put(idName, n);
			    }
			    if (n != null)
				    return n.intValue();
		    }*/
		    return -1;
	    }
	
	    /**
	     * (Ignore)
	     */
	    protected String findAttribute(IEnumerable<XAttribute> atts, String id) 
        {
            String[] arr = id.Split(':');
            XNamespace nspace = namespaces[arr[0]];
            foreach (XAttribute value in atts)
            {
                if(value.Name.NamespaceName == nspace
                    && value.Name.LocalName == arr[1])
                    return value.Value;
            }
			return null;
	    }
	
	    /**
	     * (Ignore)
	     */
	    protected LayoutParams loadLayoutParams(IEnumerable<XAttribute> atts, FrameworkElement v)
	    {
		    LayoutParams lps = new LayoutParams(true);
		    String width = findAttribute(atts, "android:layout_width");
		    String height = findAttribute(atts, "android:layout_height");
		    int w, h;
            w = LGDimensionParser.Instance.GetDimension(width);
            h = LGDimensionParser.Instance.GetDimension(height);
		
            if(w == DisplayMetrics.FILL_PARENT)
            {
                lps.halign = HorizontalAlignment.Stretch;
            }
            else if(w == DisplayMetrics.WRAP_CONTENT)
            {
                lps.halign = HorizontalAlignment.Left;
            }
            else
            {
                lps.halign = HorizontalAlignment.Left;
                lps.Width = w;
            }

            if(h == DisplayMetrics.FILL_PARENT)
            {
                lps.valign = VerticalAlignment.Stretch;
            }
            else if(h == DisplayMetrics.WRAP_CONTENT)
            {
                lps.valign = VerticalAlignment.Top;
            }
            else
            {
                lps.valign = VerticalAlignment.Top;
                lps.Height = h;
            }
		
		    String gravity = findAttribute(atts, "android:layout_gravity");
		    if (gravity != null) {
			    lps.Gravity = Convert.ToInt32(gravity);
		    }
		
		    String weight = findAttribute(atts, "android:layout_weight");
		    if (weight != null) {
			    lps.Weight = Convert.ToSingle(weight);
		    }
		
		    // Margin handling
		    // Contributed by Vishal Choudhary - Thanks!
		    String bottom = findAttribute(atts, "android:layout_marginBottom");
            String left = findAttribute(atts, "android:layout_marginLeft");
            String right = findAttribute(atts, "android:layout_marginRight");
            String top = findAttribute(atts, "android:layout_marginTop");
            String margin = findAttribute(atts, "android:layout_margin");
            if(margin != null)
            {
        	    bottom = margin;
        	    left = margin;
        	    right = margin;
        	    top = margin;
            }
            int bottomInt=0, leftInt=0, rightInt=0, topInt=0;
            if (bottom != null)
                bottomInt = LGDimensionParser.Instance.GetDimension(bottom);
            if (left != null)
                leftInt = LGDimensionParser.Instance.GetDimension(left);
            if (right != null)
                rightInt = LGDimensionParser.Instance.GetDimension(right);
            if (top != null)
                topInt = LGDimensionParser.Instance.GetDimension(top);
	    
	        lps.setMargins(leftInt, topInt, rightInt, bottomInt);
	    
	        String pbottom = findAttribute(atts, "android:layout_paddingBottom");
            String pleft = findAttribute(atts, "android:layout_paddingLeft");
            String pright = findAttribute(atts, "android:layout_paddingRight");
            String ptop = findAttribute(atts, "android:layout_paddingTop");
            String ppadding = findAttribute(atts, "android:layout_padding");
            if(ppadding != null)
            {
        	    pbottom = ppadding;
        	    pleft = ppadding;
        	    pright = ppadding;
        	    ptop = ppadding;
            }
            int pbottomInt=0, pleftInt=0, prightInt=0, ptopInt=0;
            if (pbottom != null)
                pbottomInt = LGDimensionParser.Instance.GetDimension(pbottom);
            if (pleft != null)
                pleftInt = LGDimensionParser.Instance.GetDimension(pleft);
            if (pright != null)
                prightInt = LGDimensionParser.Instance.GetDimension(pright);
            if (ptop != null)
                ptopInt = LGDimensionParser.Instance.GetDimension(ptop);
	    
	        lps.setPadding(pleftInt, ptopInt, prightInt, pbottomInt);
	    
	        return lps;
	    }
	
	    /**
	     * (Ignore)
	     */
	    protected LayoutParams loadLayoutParams(IEnumerable<XAttribute> atts, LGView current, FrameworkElement vg) 
        {
		    LayoutParams lps = new LayoutParams(true);
		
		    String width = findAttribute(atts, "android:layout_width");
		    String height = findAttribute(atts, "android:layout_height");
		    int w, h;
		    w = LGDimensionParser.Instance.GetDimension(width);
		    h = LGDimensionParser.Instance.GetDimension(height);

            if (w == DisplayMetrics.FILL_PARENT)
            {
                lps.halign = HorizontalAlignment.Stretch;
            }
            else if (w == DisplayMetrics.WRAP_CONTENT)
            {
                lps.halign = HorizontalAlignment.Left;
            }
            else
            {
                lps.halign = HorizontalAlignment.Left;
                lps.Width = w;
            }

            if (h == DisplayMetrics.FILL_PARENT)
            {
                lps.valign = VerticalAlignment.Stretch;
            }
            else if (h == DisplayMetrics.WRAP_CONTENT)
            {
                lps.valign = VerticalAlignment.Top;
            }
            else
            {
                lps.valign = VerticalAlignment.Top;
                lps.Height = h;
            }

            String gravity = findAttribute(atts, "android:layout_gravity");
			if (gravity != null) {
				//lps.Gravity = Convert.ToInt32(gravity);
				String[] arr = gravity.Split('|');
				foreach(String g in arr)
				{
                    if (g == "top")
                        lps.valign = VerticalAlignment.Top;
                    else if (g == "bottom")
                        lps.valign = VerticalAlignment.Bottom;
                    else if (g == "left")
                        lps.halign = HorizontalAlignment.Left;
                    else if (g == "right")
                        lps.halign = HorizontalAlignment.Right;
                    else if (g == "center_vertical")
                        lps.valign = VerticalAlignment.Center;
                    else if (g == "fill_vertical")
                        lps.valign = VerticalAlignment.Stretch;
                    else if (g == "center_horizontal")
                        lps.halign = HorizontalAlignment.Center;
                    else if (g == "fill_horizontal")
                        lps.halign = HorizontalAlignment.Stretch;
                    else if (g == "center")
                    {
                        lps.halign = HorizontalAlignment.Center;
                        lps.valign = VerticalAlignment.Center;
                    }
                    else if (g == "fill")
                    {
                        lps.halign = HorizontalAlignment.Stretch;
                        lps.valign = VerticalAlignment.Stretch;
                    }
				}
			}

            if (w == DisplayMetrics.FILL_PARENT)
            {
                lps.halign = HorizontalAlignment.Stretch;
            }
            if (h == DisplayMetrics.FILL_PARENT)
            {
                lps.valign = VerticalAlignment.Stretch;
            }

            String weight = findAttribute(atts, "android:layout_weight");
            if (weight != null)
            {
                lps.Weight = Convert.ToSingle(weight);
            }
		
		    /*if(lps is LinearLayout)
		    {*/
			    // Margin handling
			    // Contributed by Vishal Choudhary - Thanks!
			    String bottom = findAttribute(atts, "android:layout_marginBottom");
	            String left = findAttribute(atts, "android:layout_marginLeft");
	            String right = findAttribute(atts, "android:layout_marginRight");
	            String top = findAttribute(atts, "android:layout_marginTop");
	            String margin = findAttribute(atts, "android:margin");
	            if(margin != null)
	            {
	        	    bottom = margin;
	        	    left = margin;
	        	    right = margin;
	        	    top = margin;
	            }
	            int bottomInt=0, leftInt=0, rightInt=0, topInt=0;
	            if (bottom != null)
	                bottomInt = LGDimensionParser.Instance.GetDimension(bottom);
	            if (left != null)
	                leftInt = LGDimensionParser.Instance.GetDimension(left);
	            if (right != null)
	                rightInt = LGDimensionParser.Instance.GetDimension(right);
	            if (top != null)
	                topInt = LGDimensionParser.Instance.GetDimension(top);
		    
		        lps.setMargins(leftInt, topInt, rightInt, bottomInt);
		    
		        String pbottom = findAttribute(atts, "android:layout_paddingBottom");
	            String pleft = findAttribute(atts, "android:layout_paddingLeft");
	            String pright = findAttribute(atts, "android:layout_paddingRight");
	            String ptop = findAttribute(atts, "android:layout_paddingTop");
	            String ppadding = findAttribute(atts, "android:padding");
	            if(ppadding != null)
	            {
	        	    pbottom = ppadding;
	        	    pleft = ppadding;
	        	    pright = ppadding;
	        	    ptop = ppadding;
	            }
	            int pbottomInt=0, pleftInt=0, prightInt=0, ptopInt=0;
	            if (pbottom != null)
	                pbottomInt = LGDimensionParser.Instance.GetDimension(pbottom);
	            if (left != null)
	                pleftInt = LGDimensionParser.Instance.GetDimension(pleft);
	            if (right != null)
	                prightInt = LGDimensionParser.Instance.GetDimension(pright);
	            if (top != null)
	                ptopInt = LGDimensionParser.Instance.GetDimension(ptop);
		    
		        lps.setPadding(pleftInt, ptopInt, prightInt, pbottomInt);
		    //}
		
		    return lps;
	    }
	
	    /**
	     * (Ignore)
	     */
	    bool maybeSetBoolean(FrameworkElement view, String method, String value) {
		    if (value == null) {
			    return false;
		    }
		    value = value.ToLower();
		    Boolean boolValue;
		    if ("true" == value) {
			    boolValue = true;
		    } else if ("false" == value) {
			    boolValue = false;
		    } else {
			    return false;
		    }
		    /*try {
			    Method m = View.class.getMethod(method, boolean.class);
			    m.invoke(view, boolValue);
			    return true;
		    } catch (NoSuchMethodException ex) {
			    Log.e("ViewInflater", "No such method: " + method, ex);
		    } catch (IllegalArgumentException e) {
			    Log.e("ViewInflater", "Call", e);
		    } catch (IllegalAccessException e) {
			    Log.e("ViewInflater", "Call", e);
		    } catch (InvocationTargetException e) {
			    Log.e("ViewInflater", "Call", e);
		    }*/
		    return false;
	    }
	
	    /*String[] relative_strings = new String[]
	                            	           {"android:layout_above", 
	                            			    "android:layout_alignBaseline", 
	                            			    "android:layout_alignBottom", 
	                            			    "android:layout_alignLeft",
	                            			    "android:layout_alignParentBottom",
	                            			    "android:layout_alignParentLeft",
	                            			    "android:layout_alignParentRight",
	                            			    "android:layout_alignParentTop",
	                            			    "android:layout_alignRight", 
	                            			    "android:layout_alignTop", 
	                            			    "android:layout_below",
	                            			    "android:layout_centerHorizontal",
	                            			    "android:layout_centerInParent",
	                            			    "android:layout_centerVertical",
	                            			    "android:layout_toLeft",
	                            			    "android:layout_toRight"};
	                            	
	                            	    int[] relative_verbs = new int[]
	                            	           {RelativeLayout.ABOVE,
	                            			    RelativeLayout.ALIGN_BASELINE,
	                            			    RelativeLayout.ALIGN_BOTTOM,
	                            			    RelativeLayout.ALIGN_LEFT,
	                            			    RelativeLayout.ALIGN_PARENT_BOTTOM,
	                            			    RelativeLayout.ALIGN_PARENT_LEFT,
	                            			    RelativeLayout.ALIGN_PARENT_RIGHT,
	                            			    RelativeLayout.ALIGN_PARENT_TOP,
	                            			    RelativeLayout.ALIGN_RIGHT,
	                            			    RelativeLayout.ALIGN_TOP,
	                            			    RelativeLayout.BELOW,
	                            			    RelativeLayout.CENTER_HORIZONTAL,
	                            			    RelativeLayout.CENTER_IN_PARENT,
	                            			    RelativeLayout.CENTER_VERTICAL,
	                            			    RelativeLayout.LEFT_OF,
	                            			    RelativeLayout.RIGHT_OF,
	                            	          };*/

        #region LuaInterface Members

        public void RegisterEventFunction(string var, LuaTranslator lt)
        {
            
        }

        public string GetId()
        {
            return "LuaViewInflator";
        }

        #endregion
    }
}