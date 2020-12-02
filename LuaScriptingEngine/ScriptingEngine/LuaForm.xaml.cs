using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using ScriptingEngine.LuaUI;
#if WP8
using Windows.Networking.Proximity;
using NdefLibrary.Ndef;
using System.Runtime.InteropServices.WindowsRuntime;
using LoggerNamespace;
#endif

namespace ScriptingEngine
{
    [LuaClass("LuaForm")]
    public partial class LuaForm : PhoneApplicationPage, LuaInterface
    {
        public static Queue<LuaContext> ContextQueue = new Queue<LuaContext>();
        public static System.Windows.Navigation.NavigationService navService;
        public static LuaForm activeForm;
        protected LuaContext luaContext;
        protected String luaId;
        protected LGView view;
        protected bool mainPage = false;

        private static bool FirstInit = true;

#if WP8
        //NFC
        ProximityDevice proximityDevice;
        private long nfcSubsId;
#endif

        public LuaForm()
        {
            InitializeComponent();
            LuaEngine luaEngine = LuaEngine.Instance;
            if (FirstInit)
            {
                mainPage = true;
                luaContext = LuaContext.CreateLuaContext(this);
                luaEngine.Startup();
                luaId = luaEngine.GetMainForm();
                String initUI = luaEngine.GetMainUI();
                if (initUI != "")
                {
                    LuaViewInflator inflater = new LuaViewInflator(luaContext);
                    this.view = inflater.ParseFile(initUI, null);
                    Content = view.GetView();
                }
                else
                    luaEngine.OnGuiEvent(this, LuaEngine.GuiEvents.GUI_EVENT_CREATE, luaContext);
            }
            else
            {
                luaContext = ContextQueue.Dequeue();
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
 	        base.OnNavigatedTo(e);
            LuaForm.navService = NavigationService;
            LuaForm.activeForm = this;
            if (FirstInit)
                FirstInit = false;
            else if(!mainPage)
            {
                luaId = NavigationContext.QueryString["luaId"];
                String initUI = NavigationContext.QueryString["ui"];
                if (initUI != "")
                {
                    LuaViewInflator inflater = new LuaViewInflator(luaContext);
                    this.view = inflater.ParseFile(initUI, null);
                    Content = view.GetView();
                }
                else
                    LuaEngine.Instance.OnGuiEvent(this, LuaEngine.GuiEvents.GUI_EVENT_CREATE, luaContext);
            }
#if WP8
            proximityDevice = ProximityDevice.GetDefault();
            if (proximityDevice != null)
            {
                nfcSubsId = proximityDevice.SubscribeForMessage("NDEF", (device, message) =>
                {
                    Logger.Log(LogType.CONSOLE, LogLevel.ERROR, "**** Subs Ndef ****");
                    // Parse raw byte array to NDEF message
                    byte[] rawMsg = message.Data.ToArray();
                    NdefMessage ndefMessage = NdefMessage.FromByteArray(rawMsg);

                    Dictionary<Int32, Dictionary<String, String>> nfcData = new Dictionary<Int32, Dictionary<String, String>>();
                    int count = 0;
                    foreach (NdefRecord record in ndefMessage)
                    {
                        Dictionary<String, String> recordDict = new Dictionary<String, String>();
                        Type recordType = record.CheckSpecializedType(false);
                        if (recordType == typeof(NdefUriRecord))
                        {
                            NdefUriRecord uriRecord = new NdefUriRecord(record);
                            recordDict.Add("type", "uri");
                            recordDict.Add("uri", uriRecord.Uri);
                            nfcData.Add(count++, recordDict);
                        }
                        else if (recordType == typeof(NdefSmartUriRecord))
                        {
                            NdefSmartUriRecord sUriRecord = new NdefSmartUriRecord(record);
                            recordDict.Add("type", "spr");
                            recordDict.Add("title", sUriRecord.Titles[0].Text);
                            recordDict.Add("uri", sUriRecord.Uri);
                        }
                    }
                    LuaEngine.Instance.OnGuiEvent(this, LuaEngine.GuiEvents.GUI_EVENT_NFC, luaContext, nfcData);
                });
            }
#endif
            LuaEngine.Instance.OnGuiEvent(this, LuaEngine.GuiEvents.GUI_EVENT_RESUME, luaContext);
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
#if WP8
            if (proximityDevice != null)
            {
                proximityDevice.StopSubscribingForMessage(nfcSubsId);
            }
#endif
            LuaEngine.Instance.OnGuiEvent(this, LuaEngine.GuiEvents.GUI_EVENT_PAUSE, luaContext);
        }

        protected override void OnRemovedFromJournal(System.Windows.Navigation.JournalEntryRemovedEventArgs e)
        {
            base.OnRemovedFromJournal(e);
            LuaEngine.Instance.OnGuiEvent(this, LuaEngine.GuiEvents.GUI_EVENT_DESTROY, luaContext);
        }

        /**
	     * Creates LuaForm Object From Lua.
	     * Form that created will be sent on GUI_EVENT_CREATE event.
	     * @param lc
	     * @param luaId
	     */
	    [LuaFunction(typeof(LuaContext), typeof(String))]
	    public static void Create(LuaContext lc, String luaId)
	    {
            LuaForm.ContextQueue.Enqueue(lc);
            LuaForm.activeForm.Dispatcher.BeginInvoke(() =>
            {
#if WP8
                LuaForm.navService.Navigate(new Uri("/LuaScriptingEngine8;component/ScriptingEngine/LuaForm.xaml?luaId=" + luaId + "&ui=\"\""));
#else
                LuaForm.navService.Navigate(new Uri("/LuaScriptingEngine7;component/ScriptingEngine/LuaForm.xaml?luaId=" + luaId + "&ui=\"\""));
#endif
            });
	    }
	
	    /**
	     * Creates LuaForm Object From Lua with ui.
	     * Form that created will be sent on GUI_EVENT_CREATE event.
	     * @param lc
	     * @param luaId
	     * @param ui
	     */
	    [LuaFunction(typeof(LuaContext), typeof(String), typeof(String))]
	    public static void CreateWithUI(LuaContext lc, String luaId, String ui)
	    {
		    LuaForm.ContextQueue.Enqueue(lc);
            LuaForm.activeForm.Dispatcher.BeginInvoke(() =>
            {
#if WP8
                LuaForm.navService.Navigate(new Uri("/LuaScriptingEngine8;component/ScriptingEngine/LuaForm.xaml?luaId=" + luaId + "&ui=" + ui, UriKind.Relative));
#else
                LuaForm.navService.Navigate(new Uri("/LuaScriptingEngine7;component/ScriptingEngine/LuaForm.xaml?luaId=" + luaId + "&ui=" + ui, UriKind.Relative));
#endif
            });  
	    }

	    /**
	     * Creates LuaForm Object From Lua for tabs.
	     * @param lc
	     * @param luaId
	     * @return NativeObject
	     */
	    [LuaFunction(typeof(LuaContext), typeof(String))]
	    public static Object CreateForTab(LuaContext lc, String luaId)
	    {
            return LuaForm.activeForm;
	    }
	
	    /**
	     * Gets Active LuaForm
	     * @return LuaForm
	     */
	    [LuaFunction(false)]
	    public static LuaForm GetActiveForm()
	    {
		    return LuaForm.activeForm;
	    }
	
	    /**
	     * Gets LuaContext value of form
	     * @return LuaContext
	     */
	    [LuaFunction(false)]
	    public LuaContext GetContext()
	    {
		    return luaContext;
	    }
	
	    [LuaFunction(typeof(String))]
	    public LGView GetViewById(String lId)
	    {
		    //return MainActivity.GeneralGetViewById(lId);
		    return this.view.GetViewById(lId);
	    }
	
	    /**
	     * Gets the view.
	     * @return LGView
	     */
	    [LuaFunction(false)]
	    public LGView GetView()
	    {
		    return view;
	    }
	
	    /**
	     * Sets the view to render.
	     * @param v
	     */
	    [LuaFunction(typeof(LGView))]
	    public void SetView(LGView v)
	    {
		    view = v;
            Content = v;
	    }
	
	    /**
	     * Sets the xml file of the view to render.
	     * @param xml
	     */
	    [LuaFunction(typeof(String))]
	    public void SetViewXML(String xml)
	    {
		    LuaViewInflator inflater = new LuaViewInflator(luaContext);
		    view = inflater.ParseFile(xml, null);
            Content = view.GetView();		
	    }
	
	    /**
	     * Sets the title of the screen.
	     * @param str
	     */
	    [LuaFunction(typeof(String))]
	    public void SetTitle(String str)
	    {
            Title = str;
	    }
	
	    /**
	     * Closes the form
	     */
	    [LuaFunction(false)]
	    public void Close()
	    {
            NavigationService.GoBack();
	    }
	
	    /**
	     * Frees the created object.
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
            if (luaId != null)
                return luaId;
            return "LuaForm";
        }

        #endregion
    }
}