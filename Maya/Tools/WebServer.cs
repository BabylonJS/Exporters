using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Threading.Tasks;

namespace Maya2Babylon
{
    public static class WebServer
    {
        
        private static readonly HttpListener listener;
        private static Task runningTask;

        public const int Port = 45478;
        public const string prefix = "http://localhost:45478/";
        public const string url = "http://sandbox.babylonjs.com/?assetUrl=http://localhost:45478/";

        public static bool IsSupported { get; private set; }

        static WebServer()
        {
            try
            {
                listener = new HttpListener();

                if (!HttpListener.IsSupported)
                {
                    IsSupported = false;
                    return;
                }

                listener.Prefixes.Add(prefix);
                listener.Start();


                runningTask = Task.Run(() => Listen());

                IsSupported = true;
            }
            catch
            {
                IsSupported = false;
            }
        }

        public static string SceneFilename { get; set; }
        public static string SceneFolder { get; set; }
        static Random r = new Random();
        static void Listen()
        {
            try
            {
                while (listener.IsListening)
                {
                    var context = listener.GetContext();
                    var request = context.Request;
                    var url = request.Url;

                    context.Response.AddHeader("Cache-Control", "no-cache");
                    context.Response.AppendHeader("Access-Control-Allow-Origin", "*");  // Allow CROS

                    try
                    {
                        var path = Path.Combine(SceneFolder, HttpUtility.UrlDecode(url.PathAndQuery.Substring(1)));
                        var questionMarkIndex = path.IndexOf("?");
                        if (questionMarkIndex != -1)
                        {
                            path = path.Substring(0, questionMarkIndex);
                        }
                        var hashIndex = path.IndexOf("#");
                        if (hashIndex != -1)
                        {
                            path = path.Substring(0, hashIndex);
                        }
                        var buffer = File.ReadAllBytes(path);
                        WriteResponse(context, buffer);
                    }
                    catch
                    {
                        context.Response.StatusCode = 404;
                        context.Response.Close();
                    }
                }
            }
            catch
            {
            }
        }

        static void WriteResponse(HttpListenerContext context, string s)
        {
            WriteResponse(context.Response, s);
        }

        static void WriteResponse(HttpListenerContext context, byte[] buffer)
        {
            WriteResponse(context.Response, buffer);
        }

        static void WriteResponse(HttpListenerResponse response, string s)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(s);
            WriteResponse(response, buffer);
        }

        static void WriteResponse(HttpListenerResponse response, byte[] buffer)
        {
            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        }
    }
}
