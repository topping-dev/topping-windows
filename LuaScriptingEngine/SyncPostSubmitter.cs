using System;
using System.Net;
using System.Windows;
using System.Threading;
using System.IO;
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

namespace LuaScriptingEngine
{
    public class SyncPostSubmitter
    {
        HttpWebRequest httpClient;
        Mutex mtx;

        public string url { get; set; }
        public Dictionary<string, object> parameters { get; set; }
        string boundary = "----------" + DateTime.Now.Ticks.ToString();
        String result = "";
        Int32 timeout = 60000;

        public SyncPostSubmitter(HttpWebRequest httpClient, Int32 timeout)
        {
            this.httpClient = httpClient;
            mtx = new Mutex(false, "SyncHttpClient");
            this.timeout = timeout;
        }

        public String Submit(String contentType)
        {
            mtx.WaitOne(timeout);
            if (httpClient == null)
                httpClient = (HttpWebRequest)WebRequest.Create(new Uri(url));
            httpClient.Method = "POST";
            if (contentType == null)
                httpClient.ContentType = string.Format("multipart/form-data; boundary={0}", boundary);
            else
                httpClient.ContentType = contentType;
            httpClient.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallback), httpClient);
            mtx.WaitOne(timeout);
            mtx.ReleaseMutex();
            return result;
        }

        private void GetRequestStreamCallback(IAsyncResult asynchronousResult)
        {
            HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;
            Stream postStream = request.EndGetRequestStream(asynchronousResult);

            //(postStream, parameters);
#if !NETFX_CORE
            postStream.Close();
#endif

            request.BeginGetResponse(new AsyncCallback(GetResponseCallback), request);
        }

        private void GetResponseCallback(IAsyncResult asynchronousResult)
        {
            HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(asynchronousResult);
            Stream streamResponse = response.GetResponseStream();
            StreamReader streamRead = new StreamReader(streamResponse);
            result = streamRead.ReadToEnd();
#if !NETFX_CORE
            streamResponse.Close();
            streamRead.Close();
            // Release the HttpWebResponse
            response.Close();
#endif
            mtx.ReleaseMutex();
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
