using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Max2Babylon
{
    public static class WebServer
    {
        private static readonly HttpListener listener;
        private static Task runningTask;

        const string HtmlResponseText = @"
<!DOCTYPE html>
<html xmlns='http://www.w3.org/1999/xhtml'>
<head>
    <title>BabylonJS Sandbox - View glTF, glb, obj and babylon files</title>
    <meta name='description' content='Viewer for glTF, glb, obj and babylon files powered by BabylonJS' />
    <meta name='keywords' content='Babylon.js, Babylon, BabylonJS, glTF, glb, obj, viewer, online viewer, 3D model viewer, 3D, webgl' />
    <meta name='viewport' content='width=device-width, user-scalable=no, initial-scale=1'>

    <link rel='shortcut icon' href='https://www.babylonjs.com/img/favicon/favicon.ico'>
    <link rel='apple-touch-icon' sizes='57x57' href='https://www.babylonjs.com/img/favicon/apple-icon-57x57.png'>
    <link rel='apple-touch-icon' sizes='60x60' href='https://www.babylonjs.com/img/favicon/apple-icon-60x60.png'>
    <link rel='apple-touch-icon' sizes='72x72' href='https://www.babylonjs.com/img/favicon/apple-icon-72x72.png'>
    <link rel='apple-touch-icon' sizes='76x76' href='https://www.babylonjs.com/img/favicon/apple-icon-76x76.png'>
    <link rel='apple-touch-icon' sizes='114x114' href='https://www.babylonjs.com/img/favicon/apple-icon-114x114.png'>
    <link rel='apple-touch-icon' sizes='120x120' href='https://www.babylonjs.com/img/favicon/apple-icon-120x120.png'>
    <link rel='apple-touch-icon' sizes='144x144' href='https://www.babylonjs.com/img/favicon/apple-icon-144x144.png'>
    <link rel='apple-touch-icon' sizes='152x152' href='https://www.babylonjs.com/img/favicon/apple-icon-152x152.png'>
    <link rel='apple-touch-icon' sizes='180x180' href='https://www.babylonjs.com/img/favicon/apple-icon-180x180.png'>
    <link rel='icon' type='image/png' sizes='192x192' href='https://www.babylonjs.com/img/favicon/android-icon-192x192.png'>
    <link rel='icon' type='image/png' sizes='32x32' href='https://www.babylonjs.com/img/favicon/favicon-32x32.png'>
    <link rel='icon' type='image/png' sizes='96x96' href='https://www.babylonjs.com/img/favicon/favicon-96x96.png'>
    <link rel='icon' type='image/png' sizes='16x16' href='https://www.babylonjs.com/img/favicon/favicon-16x16.png'>
    <link rel='manifest' href='https://www.babylonjs.com/img/favicon/manifest.json'>
    <meta name='msapplication-TileColor' content='#ffffff'>
    <meta name='msapplication-TileImage' content='https://www.babylonjs.com/img/favicon/ms-icon-144x144.png'>
    <meta name='msapplication-config' content='https://www.babylonjs.com/img/favicon/browserconfig.xml'>
    <meta name='theme-color' content='#ffffff'>
    <meta charset='UTF-8'>

    <link href='https://sandbox.babylonjs.com/index.css' rel='stylesheet' />
    <script src='https://code.jquery.com/pep/0.4.2/pep.min.js'></script>

    <script src='https://preview.babylonjs.com/cannon.js'></script>
    <script src='https://preview.babylonjs.com/Oimo.js'></script>
    <script src='https://preview.babylonjs.com/babylon.js'></script>
    <script src='https://preview.babylonjs.com/inspector/babylon.inspector.bundle.js'></script>

    <script src='https://preview.babylonjs.com/loaders/babylonjs.loaders.min.js'></script>
    <script src='https://preview.babylonjs.com/serializers/babylonjs.serializers.min.js'></script>
    <script src='https://preview.babylonjs.com/materialsLibrary/babylonjs.materials.min.js'></script>
</head>
<body>
    <p id='droptext'>Drag and drop gltf, glb, obj or babylon files to view them</p>
    <canvas id='renderCanvas' touch-action='none'></canvas>
    <div id='logo'>
    </div>
    <div id='footer' class='footer'>
        <div id='animationBar'>
            <div class='dropdown'>
                <div id='dropdownBtn'>
                    <img src='https://sandbox.babylonjs.com/Assets/Icon_Up.svg' id='chevronUp'>
                    <img src='https://sandbox.babylonjs.com/Assets/Icon_Down.svg' id='chevronDown'>
                    <span id='dropdownLabel'></span>
                </div>
                <div id='dropdownContent'>
                </div>
            </div>
            <div class='row'>
                <button id='playBtn' class='pause'>
                    <img id='playImg' src='https://sandbox.babylonjs.com/Assets/Icon_Play.svg'>
                    <img id='pauseImg' src='https://sandbox.babylonjs.com/Assets/Icon_Pause.svg'>
                </button>
                <input id='slider' type='range' min='0' max='100' value='0' step='any'>
            </div>
        </div>               
        <div class='footerRight'>
            <a href='javascript:void(null);' id='btnFullscreen' class='hidden'><img src='https://sandbox.babylonjs.com/Assets/Icon_Fullscreen.svg' alt='Switch the scene to full screen' title='Switch the scene to full screen' /></a> 
            <a href='javascript:void(null);' id='btnInspector' class='hidden'><img src='https://sandbox.babylonjs.com/Assets/Icon_EditModel.svg' alt='Display inspector' title='Display inspector' /></a> 
            <a href='javascript:void(null);'>
                <div class='custom-upload' title='Open your scene from your hard drive (.babylon, .gltf, .glb, .obj)'>
                    <input type='file' id='files' multiple />
                </div>
            </a>
        </div>
    </div>
    <div id='errorZone'></div>
    <script src='https://sandbox.babylonjs.com/index.js'></script>

	<script type='text/javascript'>
        assetUrl = '###SCENE###';
        loadFromAssetUrl();
    </script>

</body>
</html>
";

        public const int Port = 45478;

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

                listener.Prefixes.Add("http://localhost:" + Port + "/");
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
                    if (string.IsNullOrEmpty(url.LocalPath) || url.LocalPath == "/")
                    {

                        var responseText = HtmlResponseText.Replace("###SCENE###", SceneFilename+"?once="+r.Next());
                        WriteResponse(context, responseText);
                    }
                    else
                    {
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
