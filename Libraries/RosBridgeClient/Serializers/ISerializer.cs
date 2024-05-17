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

// Extended non-generic communication support 2024 by Ian Arbouw (ian-arbouw-1996@hotmail.com)

using System;

namespace RosSharp.RosBridgeClient
{
    public interface ISerializer
    {
        DeserializedObject Deserialize(byte[] rawData);
        T Deserialize<T>(string json);
        object Deserialize(byte[] rawData, Type type);
        object Deserialize(string json, Type type);
        byte[] Serialize<T>(T obj);
        byte[] Serialize(object obj, Type type);
    }

    public abstract class DeserializedObject
    {
        internal abstract string GetProperty(string property);
    }
}