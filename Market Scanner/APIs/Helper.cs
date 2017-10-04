using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Globalization;

namespace Market_Scanner.APIs{
    public class Helper {
        public static Dictionary<string, Dictionary<string, Coin>> coinsHistory = new Dictionary<string, Dictionary<string, Coin>>(); //coinsHistory[marketName][timeStamp] 

        public static async void Start(){
            await Initialize();
            await StartCollectorAsync();
        }

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
                                Dictionary<string, Coin> tDict = new Dictionary<string, Coin>{
                                    { coin.timeStamp, coin }
                                };
                                coinsHistory.Add(coin.marketName, tDict);
                            }
                        }
                    }
                }
            }
        }

        public static bool CheckPriceChange(Coin coin, double price, int time) {
            Dictionary<string, Coin> currentHistory = new Dictionary<string, Coin>(coinsHistory[coin.marketName]);
            double lastPrice = Convert.ToDouble(currentHistory.Last().Value.last);

            //Minus the time difference from the date to get the requested date to check
            DateTime oDate = Convert.ToDateTime(coin.timeStamp);
            oDate.AddMilliseconds(Convert.ToDouble(time) * -1); //Convert this to /remove/ milliseconds
            string ctime = oDate.ToString(DateTimeFormatInfo.CurrentInfo.SortableDateTimePattern);

            foreach (Coin tCoin in currentHistory.Values.Where(tCoin => tCoin.timeStamp.CompareTo(ctime) <= 0)){
                var a = lastPrice * (price / 100 + 1);
                if (Convert.ToDouble(tCoin.last) <= lastPrice * (price / 100 + 1)) //price / 100 + 1 == 1.price
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
                                foreach (Tick tick in JsonConvert.DeserializeObject<JsonResponse2>(data).result) {
                                    coinsHistory[coinNames.Key].Add(tick.T, tick.ToCoin(coinNames.Key));
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
                            foreach (Coin coin in JsonConvert.DeserializeObject<JsonResponse>(data).result){
                                try{
                                    if (!coinsHistory.ContainsKey(coin.timeStamp)) //Don't overwrite
                                        coinsHistory[coin.marketName].Add(coin.timeStamp, coin);
                                }
                                catch (ArgumentException) {

                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
