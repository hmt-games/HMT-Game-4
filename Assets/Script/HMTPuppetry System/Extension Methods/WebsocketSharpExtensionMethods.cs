using Newtonsoft.Json.Linq;
using WebSocketSharp.Server;
using System.Net;
using Newtonsoft.Json;

namespace HMT.Puppetry {
    public static class WebsocketSharpExtensionMethods {
        public static JObject GetJsonPostData(this HttpRequestEventArgs e) {
            var req = e.Request;
            string json;
            using (var reader = new System.IO.StreamReader(req.InputStream, req.ContentEncoding)) {
                json = reader.ReadToEnd();
            }
            return JObject.Parse(json);
        }

        public static void SendJsonResponse(this HttpRequestEventArgs e, JObject json) {
            var response = e.Response;
            var buffer = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(json));

            response.StatusCode = (int)HttpStatusCode.OK;
            response.ContentType = "application/json";
            response.ContentEncoding = System.Text.Encoding.UTF8;
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.Close();
        }

        public static void SendBasicResponse(this HttpRequestEventArgs e, int statusCode, string statusMessage, string content=null) {
            var response = e.Response;
            if (content == null) {
                content = statusMessage;
            }
            var buffer = System.Text.Encoding.UTF8.GetBytes(content);
            response.StatusCode = statusCode;
            response.StatusDescription = statusMessage;
            response.ContentType = "text/plain";
            response.ContentEncoding = System.Text.Encoding.UTF8;
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.Close();
        }

    }


}