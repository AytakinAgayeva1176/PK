using System.ComponentModel.DataAnnotations;

namespace PK.Models
{
    public class Suggestion
    {
        public string id { get; set; }
        public string uniq_id { get; set; }
        public string user_id { get; set; }
        public string session { get; set; }
        public string reference_uri { get; set; }
        public int point { get; set; }

        [ScaffoldColumn(true)]
        [StringLength(255, ErrorMessage = "The ThumbnailPhotoFileName value cannot exceed 255 characters. ")]
        public string content { get; set; }
    }
}

