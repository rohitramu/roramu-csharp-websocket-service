namespace RoRamu.WebSocket.Service
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public class Message
    {
        private static JsonSerializerSettings JsonSerializerSettings { get; } = new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
        };

        public string Id { get; }

        public string Type { get; }
        
        public object Body { get; }

        public Message(string id, string type, object body)
        {
            this.Id = id;
            this.Type = type ?? throw new ArgumentNullException(nameof(type));
            this.Body = body;
        }

        private string _cache = null;
        public string ToJsonString()
        {
            if (this._cache == null)
            {
                this._cache = JsonConvert.SerializeObject(new
                {
                    this.Id,
                    this.Type,
                    this.Body,
                }, Message.JsonSerializerSettings);
            }

            return this._cache;
        }

        public static Message FromJsonString(string json)
        {
            return JsonConvert.DeserializeObject<Message>(json, Message.JsonSerializerSettings);
        }

        public override string ToString()
        {
            return this.ToJsonString();
        }
    }
}
