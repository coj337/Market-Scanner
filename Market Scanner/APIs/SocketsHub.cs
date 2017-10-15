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

        // Scanner Config
        private static Dictionary<string, List<Coin>> validCoins = new Dictionary<string, List<Coin>>(); //List of coins that meet the users conditions
        private static Dictionary<string, List<double>> priceChanges = new Dictionary<string, List<double>>();
        private static Dictionary<string, List<double>> volumeChanges = new Dictionary<string, List<double>>();

        // Attribute Filters
        private static Dictionary<string, List<string>> selectedPairs = new Dictionary<string, List<string>>();
        private static Dictionary<string, double> minPrice = new Dictionary<string, double>();
        private static Dictionary<string, double> maxPrice = new Dictionary<string, double>();
        private static Dictionary<string, double> minVolume = new Dictionary<string, double>();
        private static Dictionary<string, double> maxVolume = new Dictionary<string, double>();
        
        // Movement Filters
        private static Dictionary<string, double> priceChange = new Dictionary<string, double>();
        private static Dictionary<string, int> priceChangeTime = new Dictionary<string, int>();
        private static Dictionary<string, double> volumeChange = new Dictionary<string, double>();
        private static Dictionary<string, int> volumeChangeTime = new Dictionary<string, int>();

        public override Task OnConnected(){
            token[Context.ConnectionId] = new CancellationTokenSource();
            selectedPairs[Context.ConnectionId] = new List<string>(new string[] { "BTC", "USD", "ETH" });
            minPrice[Context.ConnectionId] = 0;
            maxPrice[Context.ConnectionId] = Double.MaxValue;
            minVolume[Context.ConnectionId] = 0;
            maxVolume[Context.ConnectionId] = Double.MaxValue;
            priceChange[Context.ConnectionId] = 0;
            priceChangeTime[Context.ConnectionId] = 0;
            volumeChange[Context.ConnectionId] = 0;
            volumeChangeTime[Context.ConnectionId] = 0;
            validCoins[Context.ConnectionId] = new List<Coin>();
            priceChanges[Context.ConnectionId] = new List<double>();
            volumeChanges[Context.ConnectionId] = new List<double>();

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled){
            token.Remove(Context.ConnectionId);
            selectedPairs.Remove(Context.ConnectionId);
            minPrice.Remove(Context.ConnectionId);
            maxPrice.Remove(Context.ConnectionId);
            minVolume.Remove(Context.ConnectionId);
            maxVolume.Remove(Context.ConnectionId);
            priceChange.Remove(Context.ConnectionId);
            priceChangeTime.Remove(Context.ConnectionId);
            volumeChange.Remove(Context.ConnectionId);
            volumeChangeTime.Remove(Context.ConnectionId);
            validCoins.Remove(Context.ConnectionId);
            priceChanges.Remove(Context.ConnectionId);
            volumeChanges.Remove(Context.ConnectionId);

            return base.OnDisconnected(stopCalled);
        }

        public void StartListeners(){
            Task.Run(() => {
                StartListener();
            });
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

        public void SetPriceChange(double changePercent, int newTime, string timeFormat){
            switch (timeFormat.Trim().ToLower()){
                case "seconds":
                    newTime *= 1000;
                    break;
                case "minutes":
                    newTime *= 60000;
                    break;
                case "hours":
                    newTime *= 3600000;
                    break;
            }
            priceChangeTime[Context.ConnectionId] = newTime;
            priceChange[Context.ConnectionId] = changePercent;
        }

        public void SetVolumeChange(double changePercent, int newTime, string timeFormat){
            switch (timeFormat.Trim().ToLower()){
                case "seconds":
                    newTime *= 1000;
                    break;
                case "minutes":
                    newTime *= 60000;
                    break;
                case "hours":
                    newTime *= 3600000;
                    break;
            }
            volumeChangeTime[Context.ConnectionId] = newTime;
            volumeChange[Context.ConnectionId] = changePercent;
        }

        public void TogglePair(string pair){
            if (selectedPairs[Context.ConnectionId].Contains(pair)) {
                selectedPairs[Context.ConnectionId].Remove(pair);
            }
            else{
                selectedPairs[Context.ConnectionId].Add(pair);
            }
        }

        public void UpdateTable(List<Coin> coins, List<double> priceChangePercent, List<double> volumeChangePercent){
            for (int i = 0; i < coins.Count(); i++)
                Clients.Client(Context.ConnectionId).updateTable(coins[i].marketName, coins[i].last, coins[i].volume, priceChangePercent[i], volumeChangePercent[i]);
        }

        private void StartListener(){
            while (true){
                double pChange = 10000000, vChange = 10000000;

                //Add the newest instance of each coin into a list
                List<Coin> coins = Helper.coinsHistory.Values.Select(x => x.Values.OrderBy(y => y.timeStamp).Last()).ToList();
                coins = coins.OrderBy(coin => coin.marketName).ToList();
                try{
                    //Empty the lists to stop infinite stacking
                    validCoins[Context.ConnectionId].Clear(); 
                    priceChanges[Context.ConnectionId].Clear();
                    volumeChanges[Context.ConnectionId].Clear();

                    //Find valid coins and update the table
                    foreach (Coin coin in coins.Where(coin =>
                                                        selectedPairs[Context.ConnectionId].Contains(coin.marketName.Substring(0, 3)) //Check selected base currencies
                                                     && Convert.ToDouble(coin.last) >= minPrice[Context.ConnectionId] //Check min price
                                                     && Convert.ToDouble(coin.last) <= maxPrice[Context.ConnectionId] //Check max price
                                                     && Convert.ToDouble(coin.volume) >= minVolume[Context.ConnectionId] //Check min volume
                                                     && Convert.ToDouble(coin.volume) <= maxVolume[Context.ConnectionId] //Check max volume
                                                     )){
                        pChange = Helper.CheckPriceChange(coin, priceChange[Context.ConnectionId], priceChangeTime[Context.ConnectionId]);
                        if (pChange != 10000000){ //Check price change over a period of time
                            validCoins[Context.ConnectionId].Add(coin);
                            priceChanges[Context.ConnectionId].Add(pChange);
                            volumeChanges[Context.ConnectionId].Add(0);
                            pChange = 10000000; //reset to infeasable value
                            vChange = 10000000; //reset to infeasable value
                        }
                    }
                    Clients.Client(Context.ConnectionId).clearTables();
                    UpdateTable(validCoins[Context.ConnectionId], priceChanges[Context.ConnectionId], volumeChanges[Context.ConnectionId]);
                    Clients.Client(Context.ConnectionId).lastUpdate();
                }catch (KeyNotFoundException) { }
            }
        }
    }
}
