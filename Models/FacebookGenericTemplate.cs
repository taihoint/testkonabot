namespace Bot_Application1.Models
{
    using Newtonsoft.Json;

    public class FacebookGenericTemplate
    {
        public FacebookGenericTemplate()
        {
            this.TemplateType = "generic";
        }

        [JsonProperty("template_type")]
        public string TemplateType { get; set; }

        [JsonProperty("elements")]
        public object[] Elements { get; set; }
    }
}