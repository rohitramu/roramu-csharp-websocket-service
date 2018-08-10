namespace RoRamu.WebSocket.Service
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public class Message
    {
        public static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
        };

        public string Id { get; }

        public string MessageType { get; }
        
        public object Body { get; }

        public Message(string id, string messageType, object body)
        {
            this.Id = id;
            this.MessageType = messageType ?? throw new ArgumentNullException(nameof(messageType));
            this.Body = body;
        }

        private string _cache = null;
        public string ToJsonString()
        {
            if (this._cache == null)
            {
                this._cache = JsonConvert.SerializeObject(this, Message.JsonSerializerSettings);
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
