using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Security.Key
{
    public class Offlinekey
    {
        public string id { get; set; }
        public string key { get; set; }

        public Offlinekey(string id,string key)
        {
            this.id = id;
            this.key = key;
        }

        public Offlinekey(Offlinekey offline)
        {
            this.id = offline.id;
            this.key = offline.key;
        }

        public Offlinekey()
        { }
    }
}
