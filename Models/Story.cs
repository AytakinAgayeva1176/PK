using PK.Helpers.Enums;

namespace PK.Models
{
    public class Story
    {
        public string id { get; set; }
        public string uniq_id { get; set; }
        public string name { get; set; }
        public string thumb_image { get; set; }
        public string type { get; set; }
        public string content_source { get; set; }
        public string link { get; set; }
        public string status { get; set; }
        public string link_text { get; set; }
    }
}

