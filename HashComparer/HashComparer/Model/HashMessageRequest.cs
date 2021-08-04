using Newtonsoft.Json;

namespace HashComparer.Model
{
    public class HashMessageRequest
    {
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
