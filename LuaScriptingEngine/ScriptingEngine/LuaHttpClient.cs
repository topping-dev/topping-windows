using System;
using System.Net;
using System.Windows;
using System.Collections.Generic;
using LuaScriptingEngine;
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
    [LuaClass("LuaHttpClient")]
    public class LuaHttpClient : LuaInterface
    {
        public String tag;
        
	    public HttpWebRequest httpClient;
	    public LuaTranslator onComplete = null;
	    public LuaTranslator onFail = null;
	    public String contentType = null;
        public Int32 timeout;

        /**
	     * Creates LuaHttpClient Object From Lua.
	     * @param tag
	     * @return LuaHttpClient
	     */
	    [LuaFunction(typeof(String))]
	    public static LuaHttpClient Create(String tag)
	    {
		    LuaHttpClient lhc = new LuaHttpClient();
		    lhc.tag = tag;
		    return lhc;
	    }
	
	    /**
	     * Sets the content type of the connection
	     * @param content type
	     */
	    [LuaFunction(typeof(String))]
	    public void SetContentType(String type)
	    {
		    contentType = type;
	    }
	
	    /**
	     * Start Form data.
	     * This is used to create multipart form data. After this use AppendPostData or AppendImageData.
	     * To end form use EndForm.
	     * return LuaObjectStore
	     */
	    [LuaFunction(false)]
	    public LuaObjectStore StartForm()
	    {
		    LuaObjectStore los = new LuaObjectStore();
		    Dictionary<String, Object> partList = new Dictionary<String, Object>();
		    los.obj = partList;
		    return los;
	    }
	
	    /**
	     * Add data to form.
	     * @param formData Form data created by StartForm.
	     * @param name id of the data.
	     * @param value value of the data.
	     */
	    [LuaFunction(typeof(Object), typeof(String), typeof(String))]
	    public void AppendPostData(Object formData, String name, String value)
	    {
		    LuaObjectStore los = (LuaObjectStore)formData;
		    Dictionary<String, Object> partList = (Dictionary<String, Object>)los.obj;
		    partList.Add("nuname", name);
	    }
	
	    /**
	     * Add file to form.
	     * @param formData Form data created by StartForm.
	     * @param name id of the data.
	     * @param file data of the file.
	     */
	    [LuaFunction(typeof(Object), typeof(String), typeof(String))]
	    public void AppendFileData(Object formData, String name, Object file)
	    {
		    LuaObjectStore los = (LuaObjectStore)formData;
		    Dictionary<String, Object> partList = (Dictionary<String, Object>)los.obj;
		    /*ByteArrayOutputStream bos = new ByteArrayOutputStream();
		    file.compress(CompressFormat.JPEG, 75, bos);
		    byte[] data = bos.toByteArray();*/
		    byte[] data = (byte[])file;
            partList.Add("image" + DateTime.Now.Ticks + ".jpg", data);
	    }
	
	    /**
	     * Finishes the form data created.
	     * @param formData Form data created by StartForm.
	     */
	    [LuaFunction(typeof(Object))]
	    public void EndForm(Object formData)
	    {
	    }
	
	    /**
	     * Start asynchronous load of form.
	     * @param url url to send.
	     * @param formData Form data finished by EndForm.
	     * @param tag tag that is used to identify connection.
	     */
	    [LuaFunction(typeof(String), typeof(Object), typeof(String))]
	    public void StartAsyncLoadForm(String url, Object formData, String tag)
	    {
		    LuaObjectStore los = (LuaObjectStore)formData;
		    Dictionary<String, Object> partList = (Dictionary<String, Object>)los.obj;
            httpClient = LuaEngine.Instance.GetHttpClient(this.tag, url);
            PostSubmitter ps = new PostSubmitter(httpClient, onComplete, onFail);
            ps.Submit(contentType);
	    }
	
	    /**
	     * Start asynchronous load.
	     * @param url url to send.
	     * @param data post data string.
	     * @param tag tag that is used to identify connection.
	     */
	    [LuaFunction(typeof(String), typeof(String), typeof(String))]
	    public void StartAsyncLoad(String url, String data, String tag)
	    {
		    String[] arr = data.Split('#');
            Dictionary<String, Object> dict = new Dictionary<String, Object>();

            bool useFor = true;
            foreach (String s in arr)
            {
                String[] arrIn = s.Split('=');
                if (arrIn.Length > 1)
                    dict.Add(arrIn[0], arrIn[1]);
                else
                {
                    useFor = false;
                    break;
                }
            }
            if(!useFor)
                dict.Add(data, null);

            httpClient = LuaEngine.Instance.GetHttpClient(this.tag, url);
            PostSubmitter ps = new PostSubmitter(httpClient, onComplete, onFail);
            ps.parameters = dict;
            ps.Submit(contentType);
	    }
	
	    /**
	     * Start asynchronous load.
	     * @param url url to send.
	     * @param tag tag that is used to identify connection.
	     */
	    [LuaFunction(typeof(String), typeof(String))]
	    public void StartAsyncLoadGet(String url, String tag)
	    {
            httpClient = LuaEngine.Instance.GetHttpClient(this.tag, url);
            PostSubmitter ps = new PostSubmitter(httpClient, onComplete, onFail);
            ps.Submit(contentType);
	    }
	
	    /**
	     * Start synchronous load.
	     * @param url url to send.
	     * @param data post data string.
	     * @return String value of returned data.
	     */
	    [LuaFunction(typeof(String), typeof(String))]
	    public String StartLoad(String url, String data)
	    {
            String[] arr = data.Split('#');
            Dictionary<String, Object> dict = new Dictionary<String, Object>();
            bool useFor = true;
            foreach (String s in arr)
            {
                String[] arrIn = s.Split('=');
                if (arrIn.Length > 1)
                    dict.Add(arrIn[0], arrIn[1]);
                else
                {
                    useFor = false;
                    break;
                }
            }
            if (!useFor)
                dict.Add(data, null);
            httpClient = LuaEngine.Instance.GetHttpClient(this.tag, url);
            SyncPostSubmitter ps = new SyncPostSubmitter(httpClient, timeout);
            ps.parameters = dict;
            return ps.Submit(contentType);
	    }
	
	    /**
	     * Start synchronous load.
	     * @param url url to send.
	     * @return String value of returned data.
	     */
	    [LuaFunction(typeof(String))]	
	    public String StartLoadGet(String url)
	    {
		    httpClient = LuaEngine.Instance.GetHttpClient(this.tag, url);
            SyncPostSubmitter ps = new SyncPostSubmitter(httpClient, timeout);
            return ps.Submit(contentType);
	    }
	
	    /**
	     * Set timeout of connection
	     * @param timeout timeout value seconds
	     */
	    [LuaFunction(typeof(Int32))]
	    public void SetTimeout(Int32 timeout)
	    {
		    this.timeout = timeout * 1000;
	    }
	
	    /**
	     * Frees the object.
	     */
	    [LuaFunction(false)]
	    public void Free()
	    {
		
	    }

        #region LuaInterface Members

        [LuaFunction(typeof(String), typeof(LuaTranslator))]
        public void RegisterEventFunction(string var, LuaTranslator lt)
        {
            if (var == "OnFinish")
            {
                onComplete = lt;
            }
            else if (var == "OnFail")
            {
                onFail = lt;
            }
        }

        public string GetId()
        {
            return "LuaHttpClient";
        }

        #endregion
    }
}
