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
        public List<Tick> ticks;
    }

    public class Tick{
        public string O;
        public string H;
        public string L;
        public string C;
        public string V; //Volume
        public string T; //Timestamp
        public string BV; //Base Volume
    }
}