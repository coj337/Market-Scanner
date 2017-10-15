using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Globalization;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Collections.Async;

namespace Market_Scanner.APIs{
    public class Helper {
        public static ConcurrentDictionary<string, ConcurrentDictionary<string, Coin>> coinsHistory = new ConcurrentDictionary<string, ConcurrentDictionary<string, Coin>>(); //coinsHistory[marketName][timeStamp] 

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
                                ConcurrentDictionary<string, Coin> tDict = new ConcurrentDictionary<string, Coin>{
                                    [coin.timeStamp] = coin
                                };
                                coinsHistory[coin.marketName] = tDict;
                            }
                        }
                    }
                }
            }
        }

        public static double CheckPriceChange(Coin coin, double price, int time) {
            List<Coin> currentHistory = coinsHistory[coin.marketName].Values.ToList().OrderBy(c => c.timeStamp).ToList(); ;
            var test = coinsHistory;
            //Minus the time difference from the date to get the requested date to check
            DateTime oDate = Convert.ToDateTime(currentHistory.Last().timeStamp);
            oDate = oDate.AddMilliseconds(Convert.ToDouble(time) * -1); //Convert this to /remove/ milliseconds
            string ctime = oDate.ToString(DateTimeFormatInfo.CurrentInfo.SortableDateTimePattern);

            var latest = currentHistory.Where(tCoin => ctime.CompareTo(tCoin.timeStamp) <= 0);
            if (latest.Count() > 0){
                double diff = CalculateChange(Convert.ToDouble(latest.First().last), Convert.ToDouble(latest.Last().last));
                if (price < 0) { //If looking for negative prices, only negative differences should be returned
                    if (diff < 0 && diff <= price)
                        return diff;
                }
                else if(price > 0){
                        if (diff > 0 && diff >= price)
                            return diff;
                }
                else{
                    return diff;
                }
            }
            return 10000000;
        }

        public static double CheckVolumeChange(Coin coin, double volume, int time){
            List<Coin> currentHistory = coinsHistory[coin.marketName].Values.ToList().OrderBy(c => c.timeStamp).ToList();

            //Minus the time difference from the date to get the requested date to check
            DateTime oDate = Convert.ToDateTime(currentHistory.Last().timeStamp);
            oDate = oDate.AddMilliseconds(Convert.ToDouble(time) * -1); //Convert this to /remove/ milliseconds
            string ctime = oDate.ToString(DateTimeFormatInfo.CurrentInfo.SortableDateTimePattern);

            var latest = currentHistory.Where(tCoin => ctime.CompareTo(tCoin.timeStamp) <= 0);
            if (latest.Count() > 0){
                double diff = CalculateChange(Convert.ToDouble(latest.First().volume), Convert.ToDouble(latest.Last().volume));
                if (volume < 0){ //If looking for negative volumes, only negative differences should be returned
                    if (diff < 0 && diff <= volume)
                        return diff;
                }
                else if (volume > 0){ //If looking for positive volumes, only negative differences should be returned
                    if (diff > 0 && diff >= volume)
                        return diff;
                }
                else{
                    return diff;
                }
            }
            return 10000000;
        }

        public static double CalculateChange(double previous, double current){
            try{
                if (previous == 0)
                    throw new InvalidOperationException();

                var change = current - previous;
                return change / previous * 100;
            }
            catch (InvalidOperationException) { return 10000000; }
        }

        public static async Task StartCollectorAsync() {
            string url = "";

            //Get historical data for list
            await coinsHistory.ParallelForEachAsync(async coinNames => {
                url = "https://bittrex.com/Api/v2.0/pub/market/GetTicks?marketName=" + coinNames.Key + "&tickInterval=oneMin&_=1499127220008";

                using (HttpClient client = new HttpClient()){
                    using (HttpResponseMessage res = await client.GetAsync(url)){
                        using (HttpContent content = res.Content){
                            string data = await content.ReadAsStringAsync();
                            if (data != null){
                                Parallel.ForEach(JsonConvert.DeserializeObject<JsonResponse2>(data).result, tick =>
                                {
                                    coinsHistory[coinNames.Key][tick.T] = tick.ToCoin(coinNames.Key);
                                });
                            }
                        }
                    }
                }
            }, maxDegreeOfParalellism: 200);

            //Loop 5eva refreshing list
            url = "https://bittrex.com/api/v1.1/public/getmarketsummaries";
            while (true){
                using (HttpClient client = new HttpClient()){
                    using (HttpResponseMessage res = await client.GetAsync(url)){
                        using (HttpContent content = res.Content){
                            string data = await content.ReadAsStringAsync();
                            Parallel.ForEach(JsonConvert.DeserializeObject<JsonResponse>(data).result, coin =>{
                                try{
                                    if (!coinsHistory.ContainsKey(coin.timeStamp)) //Don't overwrite
                                        coinsHistory[coin.marketName][coin.timeStamp] = coin;
                                }
                                catch (ArgumentException e){
                                }
                            });
                        }
                    }
                }
            }
        }
    }
}
