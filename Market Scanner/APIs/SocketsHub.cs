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
        private CancellationTokenSource token = new CancellationTokenSource();
        private int time;

        public void StartListeners(){
            time = 1000;
            StartListener();
        }

        public void ChangeDelay(int time, string timeFormat){
            switch (timeFormat.Trim().ToLower()){
                case "milliseconds":
                    break;
                case "seconds":
                    time *= 1000;
                    break;
                case "minutes":
                    time *= 60000;
                    break;
            }
            this.time = time;
        }

        public void UpdateTable(Coin coin){
            Clients.All.updateTable(coin.marketName, coin.last, coin.volume);
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

                Clients.All.clearTables();
                foreach (Coin coin in coins.Where(x => Convert.ToDouble(x.last) > 0.1)){
                    UpdateTable(coin);
                }

                try{
                    await Task.Delay(time, token.Token); //await {time} {timeFormat} before executing loop again
                }
                catch (TaskCanceledException){
                    break;
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
