namespace RoRamu.WebSocket
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;

    public class Message
    {
        private static JsonSerializerSettings JsonSerializerSettings { get; } = new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
        };
        private static JsonSerializerSettings PrettyPrintJsonSerializerSettings { get; } = new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Formatting = Formatting.Indented,
        };

        public string Id { get; }

        public string Type { get; }

        internal JToken Body { get; }

        public Message(string id, string type, object body)
        {
            this.Id = id;
            this.Type = type ?? throw new ArgumentNullException(nameof(type));
            this.Body = JToken.FromObject(body);
        }

        public T GetBody<T>()
        {
            return this.Body.ToObject<T>();
        }

        public bool TryGetBody<T>(out T result)
        {
            try
            {
                result = this.Body.ToObject<T>();
                return true;
            }
            catch (JsonSerializationException e)
            {
                result = default(T);
                return false;
            }
        }

        private string _cache = null;
        private string _cache_pretty = null;
        public string ToJsonString(bool prettyPrint = false)
        {
            if (prettyPrint)
            {
                if (this._cache_pretty == null)
                {
                    this._cache_pretty = JsonConvert.SerializeObject(GetSerializableObject(), Message.PrettyPrintJsonSerializerSettings);
                }

                return this._cache_pretty;
            }
            else
            {
                if (this._cache == null)
                {
                    this._cache = JsonConvert.SerializeObject(GetSerializableObject(), Message.JsonSerializerSettings);
                }

                return this._cache;
            }
        }

        public static Message FromJsonString(string json)
        {
            return JsonConvert.DeserializeObject<Message>(json, Message.JsonSerializerSettings);
        }

        public override string ToString()
        {
            return this.ToJsonString(true);
        }

        private object GetSerializableObject()
        {
            return new
            {
                this.Id,
                this.Type,
                this.Body,
            };
        }
    }
}
