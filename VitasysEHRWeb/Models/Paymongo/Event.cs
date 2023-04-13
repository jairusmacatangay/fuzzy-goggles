using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitasysEHR.Models.Paymongo
{
    public class Event
    {
        public Data data { get; set; }

        public class Data
        {
            public string id { get; set; }
            public string type { get; set; }
            public Attributes attributes { get; set; }
        }

        public class Attributes
        {
            public string type { get; set; }
            public bool livemode { get; set; }
            public Data1 data { get; set; }
            public Previous_Data previous_data { get; set; }
            public int created_at { get; set; }
            public int updated_at { get; set; }
        }

        public class Data1
        {
            public string id { get; set; }
            public string type { get; set; }
            public Attributes1 attributes { get; set; }
        }

        public class Attributes1
        {
            public object access_url { get; set; }
            public int amount { get; set; }
            public string balance_transaction_id { get; set; }
            public object billing { get; set; }
            public string currency { get; set; }
            public object description { get; set; }
            public bool disputed { get; set; }
            public object external_reference_number { get; set; }
            public int fee { get; set; }
            public bool livemode { get; set; }
            public int net_amount { get; set; }
            public string origin { get; set; }
            public object payment_intent_id { get; set; }
            public object payout { get; set; }
            public Source source { get; set; }
            public string statement_descriptor { get; set; }
            public string status { get; set; }
            public object tax_amount { get; set; }
            public object[] refunds { get; set; }
            public object[] taxes { get; set; }
            public int available_at { get; set; }
            public int created_at { get; set; }
            public int paid_at { get; set; }
            public int updated_at { get; set; }
        }

        public class Source
        {
            public string id { get; set; }
            public string type { get; set; }
        }

        public class Previous_Data
        {
        }
    }
}
