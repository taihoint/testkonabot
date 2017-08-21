namespace Bot_Application1.Models
{
    using Newtonsoft.Json;

    public class FacebookChannelData
    {
        [JsonProperty("attachment")]
        public FacebookAttachment Attachment
        {
            get;
            internal set;
        }
    }
}