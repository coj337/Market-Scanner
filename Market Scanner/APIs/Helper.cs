using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Market_Scanner.APIs;

namespace Market_Scanner.APIs{
    public class Helper {
        public static Dictionary<string, Dictionary<string, Coin>> coinsHistory = new Dictionary<string, Dictionary<string, Coin>>(); //coinsHistory[marketName][timeStamp] 

        public static async Task Initialize(){
            string url = "https://bittrex.com/api/v1.1/public/getmarketsummaries";
            //Get list of coins
            using (HttpClient client = new HttpClient()){
                using (HttpResponseMessage res = await client.GetAsync(url)){
                    using (HttpContent content = res.Content){
                        string data = await content.ReadAsStringAsync();
                        if (data != null){
                            List<Coin> coins = JsonConvert.DeserializeObject<JsonResponse>(data).result;
                            foreach (Coin coin in coins){
                                Dictionary<string, Coin> tDict = new Dictionary<string, Coin>();
                                tDict.Add(coin.timeStamp, coin);
                                coinsHistory.Add(coin.marketName, tDict);
                            }
                        }
                    }
                }
            }
        }

        public static bool CheckPriceChange(Coin coin, double price, int time) {
            double lastPrice = Convert.ToDouble(coinsHistory[coin.marketName].Last().Value.last);

            foreach (KeyValuePair<string, Coin> tCoin in coinsHistory[coin.marketName].Where(tCoin => tCoin.Value.timeStamp.CompareTo(coin.timeStamp) >= 0)){
                if (Convert.ToDouble(tCoin.Value.last) >= lastPrice * (price / 100 + 1)) //price / 100 + 1 == 1.price
                    return true;
            }
            return false;
        }

        public static async Task StartCollectorAsync() {
            string url = "";

            //Get historical data for list
            foreach (KeyValuePair<string, Dictionary<string, Coin>> coinNames in coinsHistory) {
                url = "https://bittrex.com/Api/v2.0/pub/market/GetTicks?marketName=" + coinNames.Key + "&tickInterval=oneMin&_=1499127220008";

                using (HttpClient client = new HttpClient()) {
                    using (HttpResponseMessage res = await client.GetAsync(url)) {
                        using (HttpContent content = res.Content) {
                            string data = await content.ReadAsStringAsync();
                            if (data != null){
                                foreach (Coin coin in JsonConvert.DeserializeObject<JsonResponse>(data).result) {
                                    coinsHistory[coin.marketName].Add(coin.timeStamp, coin);
                                }
                            }
                        }
                    }
                }
            }


            //Loop 5eva refreshing list
            url = "https://bittrex.com/api/v1.1/public/getmarketsummaries";
            while (true){
                using (HttpClient client = new HttpClient()){
                    using (HttpResponseMessage res = await client.GetAsync(url)){
                        using (HttpContent content = res.Content){
                            string data = await content.ReadAsStringAsync();
                            foreach (var coin in JsonConvert.DeserializeObject<JsonResponse>(data).result){
                                coinsHistory[coin.marketName].Add(coin.timeStamp, coin);
                            }
                        }
                    }
                }
            }
        }
    }
}
