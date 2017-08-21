namespace Bot_Application1.Models
{
    using Newtonsoft.Json;

    public class FacebookShareButton
    {
        public FacebookShareButton()
        {
            this.Type = "element_share";
        }

        [JsonProperty("type")]
        public string Type { get; set; }

        public override string ToString()
        {
            return $"type: {this.Type}";
        }

    }
}