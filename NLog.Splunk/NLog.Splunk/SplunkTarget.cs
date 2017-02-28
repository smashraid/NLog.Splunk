using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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
            json["Message"] = item.Message;
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
            StringBuilder builder = new StringBuilder();

            foreach (AsyncLogEventInfo item in logEvents)
            {
                string json = BuildLog(item.LogEvent);
                builder.Append(json);
                builder.AppendLine();
            }
            this.WriteLogMessage(builder.ToString());
        }

        private void WriteLogMessage(string builder)
        {           
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;         
            
            using (var client = new HttpClient())
            {               
                string url = $"{Host}/services/receivers/stream?source={Source}&sourcetype={SourceType}&index={Index}";
                var byteArray = Encoding.ASCII.GetBytes(string.Format("{0}:{1}", Username, Password));
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                var response = client.PostAsync(url, new StringContent(builder, Encoding.UTF8, "application/x-www-form-urlencoded")).Result;
                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsStringAsync().Result;
                }
            }
        }
    }
}
