﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitasysEHR.Models.Paymongo
{
    public class Source
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
            public int amount { get; set; }
            public Billing billing { get; set; }
            public string currency { get; set; }
            public bool livemode { get; set; }
            public Redirect redirect { get; set; }
            public string status { get; set; }
            public string type { get; set; }
            public int created_at { get; set; }
            public int updated_at { get; set; }
        }

        public class Billing
        {
            public Address address { get; set; }
            public string email { get; set; }
            public string name { get; set; }
            public string phone { get; set; }
        }

        public class Address
        {
            public string city { get; set; }
            public string country { get; set; }
            public string line1 { get; set; }
            public string line2 { get; set; }
            public string postal_code { get; set; }
            public string state { get; set; }
        }

        public class Redirect
        {
            public string checkout_url { get; set; }
            public string failed { get; set; }
            public string success { get; set; }
        }

    }
}
