using System.Net;
using SpeedrunDotComAPI.Links;

namespace WinstonBot.Models
{
    public class RunErrorModel
    {
        public HttpStatusCode Status { get; set; }
        public string Message { get; set; }
        public string[] Errors { get; set; }
        public LinkModel[] Links { get; set; }
    }
}