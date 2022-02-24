using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace app.Services.Titan
{
    //Payments
    public class TitanPayments
    {
        [JsonProperty("IO", NullValueHandling = NullValueHandling.Ignore)]
        public List<TitanPaymentsItem> IO { get; set; }
    }

    public class TitanPaymentsItem 
    {
        [JsonProperty("IO", NullValueHandling = NullValueHandling.Ignore)]
        public IO IO { get; set; }
    }

    public class IO
    {
        public int no { get; set; }
        public float sum { get; set; }
    }
    
    //General Payments
    public class TitanGeneralPayments
    {
        public int id { get; set; }
        public int no { get; set; }
        public int beg_id { get; set; }
        public int oper_id { get; set; }
        public int datetime { get; set; }

        [JsonProperty("F", NullValueHandling = NullValueHandling.Ignore)]
        public List<TitanGeneralPaymentsItem> F { get; set; }

        [JsonProperty("R", NullValueHandling = NullValueHandling.Ignore)]
        public List<TitanGeneralPaymentsItem> R { get; set; }
        [JsonProperty("P", NullValueHandling = NullValueHandling.Ignore)]
        public List<TitanGeneralPaymentsItem> P { get; set; }
        [JsonProperty("VD", NullValueHandling = NullValueHandling.Ignore)]
        public List<TitanGeneralPaymentsItem> VD { get; set; }
    }
    public class TitanGeneralPaymentsItem
    {
        [JsonProperty("C", NullValueHandling = NullValueHandling.Ignore)]
        public C C { get; set; }

        [JsonProperty("S", NullValueHandling = NullValueHandling.Ignore)]
        public S S { get; set; }

        [JsonProperty("D", NullValueHandling = NullValueHandling.Ignore)]
        public D D { get; set; }

        [JsonProperty("P", NullValueHandling = NullValueHandling.Ignore)]
        public P P { get; set; }

        [JsonProperty("VD", NullValueHandling = NullValueHandling.Ignore)]
        public VD VD { get; set; }
    }

    public class VD
    {
        [JsonProperty("no")]
        public string No { get; set; }
    }

    public class C
    {
        [JsonProperty("cm")]
        public string Cm { get; set; }
    }

    public class D
    {
        [JsonProperty("prc")]
        public long Prc { get; set; }
    }

    public class P
    {
        [JsonProperty("no")]
        public int No { get; set; }

        [JsonProperty("sum")]
        public float Sum { get; set; }
    }

    public class S
    {
        [JsonProperty("code")]
        public ulong Code { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("qty")]
        public float Qty { get; set; }

        [JsonProperty("price")]
        public float Price { get; set; }

        [JsonProperty("dep")]
        public int Dep { get; set; }

        [JsonProperty("grp")]
        public int Grp { get; set; }

        [JsonProperty("tax")]
        public int Tax { get; set; }
    }
}
