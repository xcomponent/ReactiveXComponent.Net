﻿using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReactiveXComponent.Common;
using JsonSerializer = ReactiveXComponent.Serializer.JsonSerializer;

namespace ReactiveXComponent.WebSocket
{
    public class WebSocketMessageHelper
    {
        private static volatile JsonSerializer _serializer;
        private static readonly object SerializerLock = new object();

        private static JsonSerializer Serializer
        {
            get
            {
                if (_serializer == null)
                {
                    lock (SerializerLock)
                    {
                        if (_serializer == null)
                        {
                            _serializer = new JsonSerializer();
                        }
                    }
                }

                return _serializer;
            }
        }

        public static string SerializeRequest(string requestKey, WebSocketEngineHeader header, object message, string componentCode = null, string topic = null)
        {
            string jsonMessage;
            if (message == null)
            {
                jsonMessage = null;
            }
            else
            {
                jsonMessage = SerializeToString(message);
            }

            string jsonContent;
            if (header == null)
            {
                jsonContent = jsonMessage;
            }
            else
            {
                var webSocketMessage = new WebSocketPacket {
                    Header = header,
                    JsonMessage = jsonMessage,
                };

                jsonContent = SerializeToString(webSocketMessage);
            }

            string beforeJson = SerializeBeforeJsonPart(requestKey, componentCode, topic);
            return $"{beforeJson} {jsonContent}{Environment.NewLine}";
        }

        public static string SerializeBeforeJsonPart(string requestKey, string componentCode, string topic)
        {
            StringBuilder beforeJson = new StringBuilder(requestKey);

            if (!string.IsNullOrEmpty(topic))
            {
                beforeJson.Append($" {topic}");
            }

            if (!string.IsNullOrEmpty(componentCode))
            {
                beforeJson.Append($" {componentCode}");
            }

            return beforeJson.ToString();
        }

        public static WebSocketPacket DeserializePacket(WebSocketMessage request)
        {
            var jResult = DeserializeString(request.Json) as JObject;
            var packet = jResult?.ToObject<WebSocketPacket>();
            return packet;
        }

        public static object DeserializeMessage(WebSocketPacket packet)
        {
            var jsonMessage = packet.JsonMessage;
            if (jsonMessage == "0" || jsonMessage == null)
            {
                return null;
            }

            object message = DeserializeString(jsonMessage);
            return message;
        }

        public static WebSocketMessage DeserializeRequest(string request)
        {
            int firstCurlyBrace = request.IndexOf('{');
            int requestTerminatorIndex = request.LastIndexOf(Environment.NewLine, StringComparison.InvariantCulture);
            if (requestTerminatorIndex < 0)
            {
                requestTerminatorIndex = request.Length;
            }

            string json = request.Substring(firstCurlyBrace, requestTerminatorIndex - firstCurlyBrace);
            var beforeJson = request.Substring(0, firstCurlyBrace).TrimEnd();
            string[] tokensBeforeJson = !string.IsNullOrEmpty(beforeJson)? beforeJson.Split() : new string[]{};

            // the request is of one of the types 
            //      command {json}, e.g. subscribe {json} (this is a request from the client)
            //      topic componentCode {json} (this is an input event)
            //      requestKey topic componentCode {json} (this is a response to a snapshot request, an output or an error)

            string key;
            string topic;
            string componentCode;
            if (tokensBeforeJson.Length == 1)
            {
                key = tokensBeforeJson[0];
                topic = null;
                componentCode = null;
            }
            else if (tokensBeforeJson.Length == 2)
            {
                string firstPartBeforeJson = tokensBeforeJson[0];

                int firstDotIndex = firstPartBeforeJson.IndexOf('.');

                // Assume it is an input on a custom type of topic if it doesn't have the format  topic = {key}.*.*.* (e.g. output.1_0.engine1.OrderService.CreationFacade)
                key = firstDotIndex > 0 ? firstPartBeforeJson.Substring(0, firstDotIndex) : WebSocketCommand.Input;
                topic = firstDotIndex == 0? tokensBeforeJson[0].Substring(1, tokensBeforeJson[0].Length - 1): tokensBeforeJson[0];
                componentCode = tokensBeforeJson[1];
            }
            else if (tokensBeforeJson.Length == 3)
            {
                key = !string.IsNullOrEmpty(tokensBeforeJson[0]) ? tokensBeforeJson[0] : WebSocketCommand.Input;
                topic = tokensBeforeJson[1];
                componentCode = tokensBeforeJson[2];
            }
            else
            {
                string errorMessage;

                if (tokensBeforeJson.Length == 0)
                {
                    errorMessage = $"Invalid request received, with no command, topic and component code: {request}";
                }
                else
                {
                    errorMessage = $"Invalid request received, with too many parts before json: {request}";
                }

                throw new InvalidOperationException(errorMessage);
            }
            IsValidKey(key);

            return new WebSocketMessage(key, topic, json, componentCode);
        }

        public static object DeserializeString(string jsonMessage)
        {
            byte[] rawMessage = Encoding.UTF8.GetBytes(jsonMessage);
            using (MemoryStream stream = new MemoryStream(rawMessage))
            {
                return Serializer.Deserialize(stream);
            }
        }

        private static string SerializeToString(object message)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                Serializer.Serialize(stream, message);
                stream.Flush();

                string serializedMessage = Encoding.UTF8.GetString(stream.ToArray());
                return serializedMessage;
            }
        }

        private static void IsValidKey(string key)
        {
            if (string.Equals(key, WebSocketCommand.Input) || string.Equals(key, WebSocketCommand.Error)
                || string.Equals(key, WebSocketCommand.Subscribe) || string.Equals(key, WebSocketCommand.Unsubscribe)
                || string.Equals(key, WebSocketCommand.Snapshot) || string.Equals(key, WebSocketCommand.Update)) return;
            throw new InvalidOperationException($"Invalid command received: {key}");
        }

        public static string DeserializeSnapshot(string receivedMessage)
        {
            var snapshotHeader = JsonConvert.DeserializeObject<WebSocketHeader>(receivedMessage);
            var zipedMessage = JsonConvert.DeserializeObject<SnapshotItems>(snapshotHeader.JsonMessage);
            byte[] compressedMessage = Convert.FromBase64String(zipedMessage.Items);
            string message;
            using (var compressedStream = new MemoryStream(compressedMessage))
            {
                using (var decompressedMessage = new MemoryStream())
                {
                    using (var unzippedMessage = new GZipStream(compressedStream, CompressionMode.Decompress))
                    {
                        unzippedMessage.CopyTo(decompressedMessage);
                    }
                    message = Encoding.UTF8.GetString(decompressedMessage.ToArray());
                }
            }
            return message;
        }
    }
}
