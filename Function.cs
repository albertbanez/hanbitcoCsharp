using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Newtonsoft.Json.Linq;
using System.Net.Http;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace hanbitcoCodetest
{
    public class input
    {
        public string currency { get; set; }
    }

    public class exchange
    {
        public string originPair { get; set; }
        public string last { get; set; }
    }

    public class Function
    {
        public JObject FunctionHandler(input input, ILambdaContext context)
        {
            HttpClient client = new HttpClient();

            HttpResponseMessage responseJson = client.GetAsync("https://s3.ap-northeast-2.amazonaws.com/hanbitco/price.json").Result;
            JObject pricejson = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(responseJson.Content.ReadAsStringAsync().Result);
            responseJson.Dispose();
            client.Dispose();

            string strCurrency = "KRW";

            JObject data = new JObject();

            foreach (var currency in pricejson)
            {
                if (string.IsNullOrEmpty(input.currency))
                {
                    if (currency.Key.Substring(4, 3) != strCurrency)
                    {
                        continue;
                    }
                }
                else
                {
                    if (currency.Key != input.currency.ToUpper() + "_" + strCurrency)
                    {
                        continue;
                    }
                }


                data[currency.Key] = new JObject();

                foreach (JProperty exchange in currency.Value)
                {
                    data[currency.Key][exchange.Name] = new JObject();
                    data[currency.Key][exchange.Name]["originPair"] = exchange.Value["originPair"];
                    data[currency.Key][exchange.Name]["last"] = exchange.Value["last"];
                }
            }

            string status = "";

            if (data == null || data.Count == 0)
            {
                status = "failed";
            }
            else
            {
                status = "success";
            }

            JObject result = new JObject
            {
                { "status", status },
                { "data", data }
            };

            return result;
        }
    }
}
