using System;

namespace PK.Models
{
    public class Shortcut 
    {
        public string id { get; set; }
        public string uniq_id { get; set; }
        public string user_id { get; set; }
        public int selected { get; set; }
        public string link { get; set; }
    }
}
