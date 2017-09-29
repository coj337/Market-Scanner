using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Market_Scanner.APIs;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading;

namespace Market_Scanner{
    public class SocketsHub : Hub{
        private static Dictionary<string, CancellationTokenSource> token = new Dictionary<string, CancellationTokenSource>();

        //Scanner Config
        private static Dictionary<string, int> time = new Dictionary<string, int>();
        
        //Filters
        private static Dictionary<string, List<string>> selectedPairs = new Dictionary<string, List<string>>();
        private static Dictionary<string, double> minPrice = new Dictionary<string, double>();
        private static Dictionary<string, double> maxPrice = new Dictionary<string, double>();
        private static Dictionary<string, double> minVolume = new Dictionary<string, double>();
        private static Dictionary<string, double> maxVolume = new Dictionary<string, double>();


        public override Task OnConnected(){
            token[Context.ConnectionId] = new CancellationTokenSource();
            time[Context.ConnectionId] = 1000;
            selectedPairs[Context.ConnectionId] = new List<string>(new string[] { "BTC", "USD", "ETH" });
            minPrice[Context.ConnectionId] = 0;
            maxPrice[Context.ConnectionId] = Double.MaxValue;
            minVolume[Context.ConnectionId] = 0;
            maxVolume[Context.ConnectionId] = Double.MaxValue;

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled){
            token.Remove(Context.ConnectionId);
            time.Remove(Context.ConnectionId);
            selectedPairs.Remove(Context.ConnectionId);
            minPrice.Remove(Context.ConnectionId);
            maxPrice.Remove(Context.ConnectionId);
            minVolume.Remove(Context.ConnectionId);
            maxVolume.Remove(Context.ConnectionId);

            return base.OnDisconnected(stopCalled);
        }

        public void StartListeners(){
            StartListener();
        }

        public void ChangeDelay(int newTime, string timeFormat){
            switch (timeFormat.Trim().ToLower()){
                case "milliseconds":
                    break;
                case "seconds":
                    newTime *= 1000;
                    break;
                case "minutes":
                    newTime *= 60000;
                    break;
            }
            time[Context.ConnectionId] = newTime;
            token[Context.ConnectionId].Cancel();
        }

        public void ChangeMaxPrice(double price){
            maxPrice[Context.ConnectionId] = price;
        }

        public void ChangeMinPrice(double price){
            minPrice[Context.ConnectionId] = price;
        }

        public void ChangeMaxVolume(double volume){
            maxVolume[Context.ConnectionId] = volume;
        }

        public void ChangeMinVolume(double volume){
            minVolume[Context.ConnectionId] = volume;
        }

        public void TogglePair(string pair){
            if (selectedPairs[Context.ConnectionId].Contains(pair)) {
                selectedPairs[Context.ConnectionId].Remove(pair);
            }
            else{
                selectedPairs[Context.ConnectionId].Add(pair);
            }
        }

        public void UpdateTable(Coin coin){
            Clients.Client(Context.ConnectionId).updateTable(coin.marketName, coin.last, coin.volume);
        }

        private async void StartListener(){
            while (true){
                List<Coin> coins = new List<Coin>();
                string url = "https://bittrex.com/api/v1.1/public/getmarketsummaries";

                using (HttpClient client = new HttpClient()){
                    using (HttpResponseMessage res = await client.GetAsync(url)){
                        using (HttpContent content = res.Content){
                            string data = await content.ReadAsStringAsync();
                            if (data != null){
                                coins = JsonConvert.DeserializeObject<JsonResponse>(data).result;
                            }
                        }
                    }
                }

                Clients.Client(Context.ConnectionId).clearTables();
                foreach (Coin coin in coins.Where(coin =>
                                                   selectedPairs[Context.ConnectionId].Contains(coin.marketName.Substring(0, 3)) //Check selected base currencies
                                                && Convert.ToDouble(coin.last) >= minPrice[Context.ConnectionId] //Check min price
                                                && Convert.ToDouble(coin.last) <= maxPrice[Context.ConnectionId] //Check max price
                                                && Convert.ToDouble(coin.volume) >= minVolume[Context.ConnectionId] //Check min volume
                                                && Convert.ToDouble(coin.volume) <= maxVolume[Context.ConnectionId] //Check max volume
                                                )){
                    UpdateTable(coin);
                }
                Clients.Client(Context.ConnectionId).lastUpdate();

                try{
                    await Task.Delay(time[Context.ConnectionId], token[Context.ConnectionId].Token); //await {time} {timeFormat} before executing loop again
                }
                catch (TaskCanceledException){
                    token[Context.ConnectionId] = new CancellationTokenSource(); //Reset cancellation token
                }
            }
        }

        public class JsonResponse{
            public bool success;
            public string message;
            public List<Coin> result;
        }
    }
}
