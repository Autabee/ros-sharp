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

// Microsoft-libs only added 2022 by Chris Tacke (ctacke@gmail.com)
//Extended non-generic communication support 2024 by Ian Arbouw (ian-arbouw-1996@hotmail.com)
#if !MS_LIBS_ONLY 

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text;

namespace RosSharp.RosBridgeClient
{
    internal class NewtonsoftJsonSerializer : ISerializer
    {

        public DeserializedObject Deserialize(byte[] buffer)
        {
            string ascii = Encoding.ASCII.GetString(buffer, 0, buffer.Length);
            JObject jObject = JsonConvert.DeserializeObject<JObject>(ascii);
            return new NewtonsoftJsonObject(jObject);
        }

        public T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public object Deserialize(byte[] buffer, Type type)
        {
            string ascii = Encoding.ASCII.GetString(buffer, 0, buffer.Length);
            JObject jObject = JsonConvert.DeserializeObject<JObject>(ascii);
            return new NewtonsoftJsonObject(jObject);
        }

        public object Deserialize(string json, Type type)
        {
            var jsonReader = new JsonTextReader(new System.IO.StringReader(json));
            return new JsonSerializer().Deserialize(jsonReader, type);
        }

        public byte[] Serialize<T>(T obj)
            => Serialize(obj, typeof(T));

        public byte[] Serialize(object obj, Type type)
        {
            string json = JsonConvert.SerializeObject(obj);
            return Encoding.ASCII.GetBytes(json);
        }
    }

    internal class NewtonsoftJsonObject : DeserializedObject
    {
        private JObject jObject;

        internal NewtonsoftJsonObject(JObject _jObject)
        {
            jObject = _jObject;
        }

        internal override string GetProperty(string property)
        {
            return jObject.GetValue(property).ToString();
        }
    }
}
#endif