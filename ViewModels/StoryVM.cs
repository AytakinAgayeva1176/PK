using Microsoft.AspNetCore.Http;

namespace PK.ViewModels
{
    public class StoryVM
    {
        public string name { get; set; }
        public IFormFile thumb_image { get; set; }
        public IFormFile content_source { get; set; }
        public string link { get; set; }
        public string link_text { get; set; }
    }
}
