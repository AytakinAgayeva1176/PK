using System.ComponentModel.DataAnnotations;

namespace PK.ViewModels
{
    public class SuggestionVM
    {

        public string user_id { get; set; }
        public string session { get; set; }
        public string reference_uri { get; set; }
        public int point { get; set; }

        [ScaffoldColumn(true)]
        [StringLength(255, ErrorMessage = "The ThumbnailPhotoFileName value cannot exceed 255 characters. ")]
        public string content { get; set; }
    }
}
