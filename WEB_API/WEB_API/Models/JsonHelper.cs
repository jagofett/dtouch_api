using System;

namespace WEB_API.Models
{
    public class JsonImage
    {
        public byte[] ImageBytes { get; set; }
    }
    public class JsonResponse
    {
        public String message { get; set; }
        public bool error { get; set; } 
    }

}