using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Net.Http;
using System.Web;
using System.Web.UI.WebControls;
using Microsoft.ProjectOxford.Emotion;
using Flurl;
using Flurl.Http;
using Microsoft.ProjectOxford.Emotion.Contract;
using Newtonsoft.Json;

namespace CSHttpClientSample
{
    static class Program
    {
        private static Dictionary<string, List<string>> dic = new Dictionary<string, List<string>>();

        static void Main()
        {
            int cnt = 0; // костыль, потому что иначе вылетает exception
            foreach (var cur in FindUrls())
            {
                RunEmotion(cur.contentUrl);
            }
            File.WriteAllText(@"result.json", JsonConvert.SerializeObject(dic));
            Console.WriteLine("End...");
            Console.ReadLine();
        }

        internal static dynamic FindUrls()
        {
            var baseurl = @"https://api.cognitive.microsoft.com/bing/v5.0/images/search";
            var task = baseurl.SetQueryParams(new { q = "people laugh", count = 10 })
                .WithHeader("Ocp-Apim-Subscription-Key", "4d73a2cc3b1448fba22e0db6efa3ed4b")
                .GetJsonAsync();
            task.Wait();
            return task.Result.value;
        }

        internal static void RunEmotion(string url)
        {
            var client = new EmotionServiceClient("962ed5507adc49fdb1e17926f23b02da");
            var task = client.RecognizeAsync(url);
            task.Wait();
            foreach (var cur in task.Result)
            {
                var def = new KeyValuePair<string, float>("Neutral", cur.Scores.Neutral);
                foreach (var item in cur.Scores.ToRankedList())
                {
                    if (item.Value > def.Value)
                        def = item;
                }
                if (!dic.ContainsKey(url))
                    dic.Add(url, new List<string> { def.Key });
                else dic[url].Add(def.Key);
            }
        }
    }
}