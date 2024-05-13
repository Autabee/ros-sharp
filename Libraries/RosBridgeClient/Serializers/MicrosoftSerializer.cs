/*
© Siemens AG, 2020
Author: Berkay Alp Cakal (berkay_alp.cakal.ct@siemens.com)

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at
<http://www.apache.org/licenses/LICENSE-2.0>.
Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RosSharp.RosBridgeClient
{
    class MicrosoftSerializer : ISerializer
    {
        public byte[] Serialize<T>(T obj)
            => Serialize(obj, typeof(T));
        public byte[] Serialize(object obj, Type type)
        {
            string json = JsonSerializer.Serialize(obj, obj.GetType());
            return Encoding.ASCII.GetBytes(json);
        }

        public DeserializedObject Deserialize(byte[] buffer)
        {
            string ascii = Encoding.ASCII.GetString(buffer, 0, buffer.Length);
            JsonElement jsonElement = JsonDocument.Parse(ascii).RootElement;
            return new MicrosoftJsonObject(jsonElement);
        }
        
        public T Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json);
        }

        public object Deserialize(byte[] buffer, Type type)
        {
            string ascii = Encoding.ASCII.GetString(buffer, 0, buffer.Length);
            JsonElement jsonElement = JsonDocument.Parse(ascii).RootElement;
            var obj =  JsonSerializer.Deserialize(ascii,type);
            if (obj == null)
            {
                throw new Exception("Failed to deserialize object");
            }
            return obj;
        }

        public object Deserialize(string rawData, Type type)
        {
            return JsonSerializer.Deserialize(rawData, type);
        }
    }

    internal class MicrosoftJsonObject : DeserializedObject
    {
        private JsonElement jObject;

        internal MicrosoftJsonObject(JsonElement _jObject)
        {
            jObject = _jObject;
        }

        internal override string GetProperty(string property)
        {
            return jObject.GetProperty(property).ToString();
        }
    }
}
