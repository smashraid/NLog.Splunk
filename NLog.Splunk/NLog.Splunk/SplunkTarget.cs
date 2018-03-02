using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog.Common;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;

namespace NLog.Splunk
{
    [Target("Splunk")]
    public class SplunkTarget : TargetWithLayout
    {
        [RequiredParameter]
        public string Host { get; set; }
        [RequiredParameter]
        public string Username { get; set; }
        [RequiredParameter]
        public string Password { get; set; }
        [RequiredParameter]
        public string Index { get; set; }
        [RequiredParameter]
        public string Source { get; set; }
        [RequiredParameter]
        public string SourceType { get; set; }

        private static readonly HttpClient Client = new HttpClient();

        protected override void Write(LogEventInfo logEvent)
        {
            string json = BuildLog(logEvent);
            WriteLogMessage(json);
        }

        private string BuildLog(LogEventInfo item)
        {
            JObject json = new JObject();
            json["Timestamp"] = item.TimeStamp;
            json["Level"] = item.Level.Name.ToUpper();
            json["Message"] = item.FormattedMessage;
            if (item.Exception != null)
            {
                json["Exception"] = new JObject();
                json["Exception"]["Message"] = item.Exception.Message;
                json["Exception"]["StackTrace"] = item.Exception.StackTrace;
            }
            if (item.Properties.Count > 0)
            {
                foreach (KeyValuePair<object, object> keyValuePair in item.Properties)
                {
                    string[] key = keyValuePair.Key.ToString().Split('.');
                    if (key.Length > 1)
                    {
                        if (json[key[0]] == null)
                        {
                            json[key[0]] = new JObject();
                        }
                        json[key[0]][key[1]] = keyValuePair.Value.ToString();
                    }
                    else
                    {
                        json[keyValuePair.Key] = keyValuePair.Value.ToString();
                    }
                }
            }

            return json.ToString(Formatting.None);
        }

        protected override void Write(AsyncLogEventInfo[] logEvents)
        {
            try
            {
                StringBuilder builder = new StringBuilder();

                foreach (AsyncLogEventInfo item in logEvents)
                {
                    string json = BuildLog(item.LogEvent);
                    builder.Append(json);
                    builder.AppendLine();
                }
                this.WriteLogMessage(builder.ToString());
            }
            catch (Exception ex)
            {
                foreach (AsyncLogEventInfo logEventInfo in logEvents)
                {
                    logEventInfo.Continuation(ex);
                }
            }
            foreach (AsyncLogEventInfo logEventInfo in logEvents)
            {
                logEventInfo.Continuation(null);
            }
        }

        private void WriteLogMessage(string builder)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

            string url = $"{Host}/services/receivers/stream?source={Source}&sourcetype={SourceType}&index={Index}";
            var byteArray = Encoding.ASCII.GetBytes(string.Format("{0}:{1}", Username, Password));
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            var response = Client.PostAsync(url, new StringContent(builder, Encoding.UTF8, "application/x-www-form-urlencoded")).Result;
            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsStringAsync().Result;
            }
        }
    }
}
