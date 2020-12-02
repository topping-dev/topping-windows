using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ScriptingEngine.LuaUI;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ScriptingEngine
{
    public class LuaInternalParameter
    {

        Dictionary<String, String> paramDict = new Dictionary<String, String>();

        public String this[String key]
        {
            get
            {
                return paramDict[key];
            }
            set
            {
                paramDict[key] = value;
            }
        }
    }

    [LuaClass("LuaForm")]
    public sealed partial class LuaForm : Page, LuaInterface
    {
        public static Queue<LuaContext> ContextQueue = new Queue<LuaContext>();
        public static LuaForm navService;
        public static LuaForm activeForm;
        protected LuaContext luaContext;
        protected String luaId;
        protected LGView view;
        protected bool mainPage = false;

        private static bool FirstInit = true;

        public LuaForm()
        {
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

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            LuaForm.navService = this;
            LuaForm.activeForm = this;
            if (FirstInit)
                FirstInit = false;
            else if (!mainPage)
            {
                LuaInternalParameter lip = (LuaInternalParameter)e.Parameter;
                luaId = lip["luaId"];
                String initUI = lip["ui"];
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

        protected override void OnNavigatedFrom(NavigationEventArgs e)
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

        //    LuaEngine.Instance.OnGuiEvent(this, LuaEngine.GuiEvents.GUI_EVENT_DESTROY, luaContext);

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
            System.Windows.Dispatcher.BeginInvoke(() =>
            {
                LuaInternalParameter lip = new LuaInternalParameter();
                lip["luaid"] = luaId;
                lip["ui"] = "";
                LuaForm.navService.Frame.Navigate(typeof(LuaForm), lip);
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
            System.Windows.Dispatcher.BeginInvoke(() =>
            {
                LuaInternalParameter lip = new LuaInternalParameter();
                lip["luaid"] = luaId;
                lip["ui"] = ui;
                LuaForm.navService.Frame.Navigate(typeof(LuaForm), lip);
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
            //Title = str;
        }

        /**
         * Closes the form
         */
        [LuaFunction(false)]
        public void Close()
        {
            LuaForm.navService.Frame.GoBack();
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
