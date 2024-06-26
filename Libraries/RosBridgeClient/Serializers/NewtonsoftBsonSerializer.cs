﻿/*
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

// Adding BSON (de-)seriliazation option
// Shimadzu corp , 2019, Akira NODA (a-noda@shimadzu.co.jp / you.akira.noda@gmail.com)
// Microsoft-libs only added 2022 by Chris Tacke (ctacke@gmail.com)
// Extended non-generic communication support 2024 by Ian Arbouw (ian-arbouw-1996@hotmail.com)

#if !MS_LIBS_ONLY

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace RosSharp.RosBridgeClient
{
    internal class NewtonsoftBsonSerializer : ISerializer
    {

        public DeserializedObject Deserialize(byte[] buffer)
            => (DeserializedObject)Deserialize(buffer, typeof(NewtonsoftBsonObject));

        public T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public object Deserialize(byte[] buffer, Type type)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream(buffer);
            Newtonsoft.Json.Bson.BsonDataReader reader = new Newtonsoft.Json.Bson.BsonDataReader(ms);
            return new JsonSerializer().Deserialize(reader, type);
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
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            Newtonsoft.Json.Bson.BsonDataWriter writer = new Newtonsoft.Json.Bson.BsonDataWriter(ms);
            JsonSerializer serializer = new JsonSerializer();
            serializer.Serialize(writer, obj);
            return ms.ToArray();
        }


    }

    internal class NewtonsoftBsonObject : DeserializedObject
    {
        private JObject jObject;

        internal NewtonsoftBsonObject(JObject _jObject)
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