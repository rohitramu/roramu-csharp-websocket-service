namespace RoRamu.WebSocket
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Represents a JSON message which can be sent over websockets using the protocol implemented by
    /// this library.
    /// </summary>
    /// <remarks>
    /// Every message has 3 properties: <c>id</c>, <c>type</c>, and <c>body</c>.
    /// <br/>
    /// The <c>id</c> property is a string which is provided by a caller in order to track responses
    /// to requests. This can be null.
    /// <br/>
    /// The <c>type</c> property indicates how the message should be handled.  Services will have a
    /// different message handler mapped to each supported message type.  This cannot be null.
    /// <br/>
    /// The <c>body</c> property contains any data provided by the caller as part of the message.
    /// The handling of this property is dependent on the implementation of the service.
    /// </remarks>
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

        /// <summary>
        /// The message ID.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The message type.
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// The message body.
        /// </summary>
        internal JToken Body { get; }

        /// <summary>
        /// Creates a new message.
        /// </summary>
        /// <param name="id">The message ID.</param>
        /// <param name="type">The message type.  Cannot be null.</param>
        /// <param name="body">The message body.</param>
        public Message(string id, string type, object body)
        {
            this.Id = id;
            this.Type = type ?? throw new ArgumentNullException(nameof(type));
            this.Body = JToken.FromObject(body);
        }

        /// <summary>
        /// Deserializes the message body into the given type and throws an exception if it fails.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the message body into.</typeparam>
        /// <returns>The deserialized message body.</returns>
        public T GetBody<T>()
        {
            return this.Body.ToObject<T>();
        }

        /// <summary>
        /// Attempts to deserialize the message body into the given type.
        /// </summary>
        /// <param name="result">The deserialized message body if successful, otherwise null.</param>
        /// <typeparam name="T">The type to deserialize the message body into.</typeparam>
        /// <returns>True if deserialization was successful, otherwise false.</returns>
        public bool TryGetBody<T>(out T result)
        {
            try
            {
                result = this.Body.ToObject<T>();
                return true;
            }
            catch (JsonSerializationException)
            {
                result = default(T);
                return false;
            }
        }

        private string _cache = null;
        private string _cache_pretty = null;

        /// <summary>
        /// Serializes the message into a JSON string.
        /// </summary>
        /// <param name="prettyPrint">Whether or not to pretty-print the JSON string.</param>
        /// <returns>The message serialized as a JSON string.</returns>
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

        /// <summary>
        /// Deserializes a JSON string into a <see cref="RoRamu.WebSocket.Message" /> object.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized <see cref="RoRamu.WebSocket.Message" /> object.</returns>
        public static Message FromJsonString(string json)
        {
            return JsonConvert.DeserializeObject<Message>(json, Message.JsonSerializerSettings);
        }

        /// <summary>
        /// Serializes the message into a JSON string.  This method has the same behavior as calling
        /// <c>Message.ToJsonString(true)</c>.
        /// </summary>
        /// <returns>The message serialized as a JSON string.</returns>
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
