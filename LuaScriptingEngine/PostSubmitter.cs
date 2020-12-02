using System;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using System.IO;
using ScriptingEngine;
using System.Text;

namespace LuaScriptingEngine
{
    public class PostSubmitter
    {
        public string url { get; set; }
        public Dictionary<string, object> parameters { get; set; }
        string boundary = "----------" + DateTime.Now.Ticks.ToString();
        private HttpWebRequest httpclient;
        private LuaTranslator onComplete;
        private LuaTranslator onFail;

        public PostSubmitter() { }

        public PostSubmitter(HttpWebRequest httpclient, LuaTranslator onComplete, LuaTranslator onFail)
        {
            this.httpclient = httpclient;
            this.onComplete = onComplete;
            this.onFail = onFail;
        }

        public void Submit(String contentType)
        {
            // Prepare web request...
            if(httpclient == null)
                httpclient = (HttpWebRequest)WebRequest.Create(new Uri(url));
            if(httpclient.Method != "POST")
                httpclient.Method = "POST";
            if (contentType == null)
                httpclient.ContentType = string.Format("multipart/form-data; boundary={0}", boundary);
            else
                httpclient.ContentType = contentType;

            httpclient.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallback), httpclient);
        }

        private void GetRequestStreamCallback(IAsyncResult asynchronousResult)
        {
            HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;
            Stream postStream = request.EndGetRequestStream(asynchronousResult);

            if(httpclient.ContentType == string.Format("multipart/form-data; boundary={0}", boundary))
                writeMultipartObject(postStream, parameters);
            else
                writeText(postStream, parameters);
#if !NETFX_CORE
            postStream.Close();
#endif

            request.BeginGetResponse(new AsyncCallback(GetResponseCallback), request);
        }

        private void GetResponseCallback(IAsyncResult asynchronousResult)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(asynchronousResult);
                Stream streamResponse = response.GetResponseStream();
                StreamReader streamRead = new StreamReader(streamResponse);
                StringWriter sw = new StringWriter();
                char[] cache = new char[2048];
                int read = 0;
                while((read = streamRead.Read(cache, 0, 2048)) != 0)
                {
                    sw.Write(cache, 0, read);
                }
                String data = sw.ToString();
                if(onComplete != null)
                    onComplete.CallIn(data);
#if !NETFX_CORE
                streamResponse.Close();
                streamRead.Close();
                // Release the HttpWebResponse
                response.Close();
#endif
            }
            catch (Exception ex)
            {
                LoggerNamespace.Log.e("LuaHttpClient", ex.ToString());
                if(onFail != null)
                    onFail.CallIn(ex.Message);
            }
        }

        public void writeText(Stream stream, object data)
        {
            StreamWriter writer = new StreamWriter(stream);
            if (data != null)
            {
                foreach (var entry in data as Dictionary<string, object>)
                {
                    byte[] strb = Encoding.UTF8.GetBytes(entry.Key);
                    writer.BaseStream.Write(strb, 0, strb.Length);
                    //writer.Write(entry.Key);
                    break;
                }
            }
            //writer.WriteLine();
            writer.Flush();
        }

        public void writeMultipartObject(Stream stream, object data)
        {
            StreamWriter writer = new StreamWriter(stream);
            if (data != null)
            {
                foreach (var entry in data as Dictionary<string, object>)
                {
                    WriteEntry(writer, entry.Key, entry.Value);
                }
            }
            writer.Write("--");
            writer.Write(boundary);
            writer.WriteLine("--");
            writer.Flush();
        }

        private void WriteEntry(StreamWriter writer, string key, object value)
        {
            if (value != null)
            {
                writer.Write("--");
                writer.WriteLine(boundary);
                if (value is byte[])
                {
                    byte[] ba = value as byte[];

                    writer.WriteLine(@"Content-Disposition: form-data; name=""{0}""; filename=""{1}""", key, "sentPhoto.jpg");
                    writer.WriteLine(@"Content-Type: application/octet-stream");
                    //writer.WriteLine(@"Content-Type: image / jpeg");
                    writer.WriteLine(@"Content-Length: " + ba.Length);
                    writer.WriteLine();
                    writer.Flush();
                    Stream output = writer.BaseStream;

                    output.Write(ba, 0, ba.Length);
                    output.Flush();
                    writer.WriteLine();
                }
                else
                {
                    writer.WriteLine(@"Content-Disposition: form-data; name=""{0}""", key);
                    writer.WriteLine();
                    writer.WriteLine(value.ToString());
                }
            }
        }
    }
}
