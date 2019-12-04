using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.Net;

namespace WebApiOnly.Utils
{

    public class UtilsJsonManager 
    {
        public enum ResponseEnum
        {
            TransacionSucces,
            TransactionFailed,
        }

        public static Dictionary<string, JToken> GetJsonData(string Ip)
        {
            string baseApiAddress = ConfigurationManager.AppSettings["IpAPI"];

            var json = new WebClient().DownloadString(baseApiAddress + Ip);
            Dictionary<string,JToken> values = JsonConvert.DeserializeObject<Dictionary<string, JToken>>(json);

            return values;         
        }

        internal static Dictionary<string, JToken> GetJsonDictionary(object jsonObject)
        {
            JObject jObject = jsonObject as JObject;
            Dictionary<string, JToken> jsonDictionary = new Dictionary<string, JToken>();
            foreach (var x in jObject)
            {
                if (!jsonDictionary.ContainsKey(x.Key))
                {
                    jsonDictionary.Add(x.Key, x.Value);
                }
            }
            return jsonDictionary;
        }
    }
}