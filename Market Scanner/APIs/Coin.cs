using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Market_Scanner.APIs{
    public class JsonResponse{
        public bool success;
        public string message;
        public List<Coin> result;
    }

    public class JsonResponse2{
        public bool success;
        public string message;
        public List<Tick> result;
    }

    public class Coin {
        public string marketName;
        public string high;
        public string low;
        public string volume;
        public string last;
        public string baseVolume;
        public string timeStamp;
        public string bid;
        public string ask;
        public string openBuyOrders;
        public string openSellOrders;
        public string prevDay;
        public string created;
        public string displayMarketName;
    }

    public class Tick{
        public string O; //Open
        public string H; //High
        public string L; //Low
        public string C; //Close
        public string V; //Volume
        public string T; //Timestamp
        public string BV; //Base Volume

        public Coin ToCoin(string name){
            Coin coin = new Coin{
                marketName = name,
                high = this.H,
                low = this.L,
                volume = this.V,
                last = this.C,
                baseVolume = this.BV,
                timeStamp = this.T,
                bid = "Unknown",
                ask = "Unknown",
                openBuyOrders = "Unknown",
                openSellOrders = "Unknown",
                prevDay = "Unknown",
                created = "Unknown",
                displayMarketName = "Unknown"
            };

            return coin;
        }
    }
}