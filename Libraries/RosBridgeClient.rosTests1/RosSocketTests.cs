/*
© Siemens AG, 2017-2019
Author: Dr. Martin Bischoff (martin.bischoff@siemens.com)

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

// adjusted to remove the requirement of launching ros services besides the rosbridge server by Ian Arbouw (ian-arbouw-1996@hotmail.com)

using NUnit.Framework;
using RosSharp.RosBridgeClient;
using System;
using System.Collections.Generic;
using System.Text;
using Assert = NUnit.Framework.Assert;

using std_msgs = RosSharp.RosBridgeClient.MessageTypes.Std;
using std_srvs = RosSharp.RosBridgeClient.MessageTypes.Std;
using rosapi = RosSharp.RosBridgeClient.MessageTypes.Rosapi;
using Microsoft.Extensions.Configuration;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace RosSharp.RosBridgeClient.Tests
{
    [TestFixture()]
    public class RosSocketTests
    { 
        private static string Uri = "ws://localhost:9090";
        private static RosSocket RosSocket;
        private ManualResetEvent OnMessageReceived = new ManualResetEvent(false);
        private ManualResetEvent OnServiceReceived = new ManualResetEvent(false);
        private ManualResetEvent OnServiceProvided = new ManualResetEvent(false);

        [SetUp]
        public void Setup()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("secret.json", optional: false, reloadOnChange: true)
                .Build();
            Uri = config.GetSection("rosbridge_uri").Get<string>();
            RosSocket = new RosSocket(new RosBridgeClient.Protocols.WebSocketNetProtocol(Uri));
        }

        [TearDown]
        public void TearDown()
        {
            RosSocket.Close();
        }
        
        [Test]
        public void PubSubTest()
        {
            List<std_msgs.String> messages = new List<std_msgs.String>();
            var topic = "/pubsub_test";
            string pub_id = RosSocket.Advertise(typeof(std_msgs.String),topic);
            std_msgs.String message = new std_msgs.String
            {
                data = "publication test message data"
            };
            string sub_id = RosSocket.Subscribe<std_msgs.String>(topic,
                (std_msgs.String msg) => messages.Add(msg));
            RosSocket.Publish(pub_id, message);
            
            DateTime breaktime = DateTime.Now.AddSeconds(10);
            bool succes= false;
            while (DateTime.Now < breaktime)
            {
                if (messages.Where(o=>o.data == message.data).Any())
                {
                    succes = true;
                    break;
                }
                Thread.SpinWait(100);
            }


            RosSocket.Unsubscribe(sub_id);
            RosSocket.Unadvertise(pub_id);
           
            Assert.IsTrue(succes, "Failed to received Data");
        }


        [Test]
        public void PubSubTest2a()
        {
            List<std_msgs.String> messages = new List<std_msgs.String>();
            var topic = "/pubsub_test2";
            std_msgs.String message = new std_msgs.String
            {
                data = "publication test message data"
            };


            string sub_id = RosSocket.Subscribe<std_msgs.String>(topic,
                (string topic, std_msgs.String msg) => messages.Add(msg));
            Thread.SpinWait(100);
            string pub_id = RosSocket.Advertise(typeof(std_msgs.String), topic);
            RosSocket.Publish(pub_id, message);


            DateTime breaktime = DateTime.Now.AddSeconds(10);
            bool succes = false;
            while (DateTime.Now < breaktime)
            {
                if (messages.Where(o => o.data == message.data).Any())
                {
                    succes = true;
                    break;
                }
                Thread.SpinWait(100);
            }


            RosSocket.Unsubscribe(sub_id);
            RosSocket.Unadvertise(pub_id);

            Assert.IsTrue(succes, "Failed to received Data");
        }


        [Test]
        public void PubSubTest2b()
        {
            List<object> messages = new List<object>();
            var topic = "/pubsub_test2";
            std_msgs.String message = new std_msgs.String
            {
                data = "publication test message data"
            };


            string sub_id = RosSocket.Subscribe(typeof(std_msgs.String),topic,
                (string topic, object msg) => messages.Add(msg));
            Thread.SpinWait(100);
            string pub_id = RosSocket.Advertise(typeof(std_msgs.String), topic);
            RosSocket.Publish(pub_id, message);


            DateTime breaktime = DateTime.Now.AddSeconds(10);
            bool succes = false;
            while (DateTime.Now < breaktime)
            {
                if (messages.Where(o => o.GetType() == typeof(std_msgs.String) &&  ((std_msgs.String)o).data == message.data).Any())
                {
                    succes = true;
                    break;
                }
                Thread.SpinWait(100);
            }


            RosSocket.Unsubscribe(sub_id);
            RosSocket.Unadvertise(pub_id);

            Assert.IsTrue(succes, "Failed to received Data");
        }

        [Test]
        public void PubSubTestJson1()
        {
            List<std_msgs.String> messages = new List<std_msgs.String>();
            var topic = "/pubsub_test2";
            std_msgs.String message = new std_msgs.String
            {
                data = "publication test message data"
            };


            string sub_id = RosSocket.Subscribe(std_msgs.String.RosMessageName,topic,
                (string msg) =>
                {
                    messages.Add(JsonSerializer.Deserialize<std_msgs.String>(msg) ?? new std_msgs.String());
                    Console.WriteLine($"Received: {msg}");
                }
                );
            Thread.SpinWait(100);
            string pub_id = RosSocket.Advertise(typeof(std_msgs.String), topic);
            RosSocket.Publish(pub_id, message);


            DateTime breaktime = DateTime.Now.AddSeconds(10);
            bool succes = false;
            while (DateTime.Now < breaktime)
            {
                if (messages.Where(o => o.GetType() == typeof(std_msgs.String) && ((std_msgs.String)o).data == message.data).Any())
                {
                    succes = true;
                    break;
                }
                Thread.SpinWait(100);
            }


            RosSocket.Unsubscribe(sub_id);
            RosSocket.Unadvertise(pub_id);

            Assert.IsTrue(succes, "Failed to received Data");
        }


        [Test]
        public void PubSubTestJson2a()
        {
            List<string> messages = new List<string>();
            var topic = "/pubsub_test2";
            std_msgs.String message = new std_msgs.String
            {
                data = "publication test message data"
            };

            var json = JsonSerializer.Serialize(message);

            Console.WriteLine(json);
            string sub_id = RosSocket.Subscribe(typeof(std_msgs.String),topic,
                (string topic, string msg, string rosType) => {
                    msg = msg.Replace(": ", ":");
                    messages.Add(msg);
                    Console.WriteLine($"Received: {msg}, on topic {topic}, with type {rosType}");
                    });
            Thread.SpinWait(100);
            string pub_id = RosSocket.Advertise(typeof(std_msgs.String), topic);
            RosSocket.Publish(pub_id, message);


            DateTime breaktime = DateTime.Now.AddSeconds(10);
            bool succes = false;
            while (DateTime.Now < breaktime)
            {
                if (messages.Where(o => o == json).Any())
                {
                    succes = true;
                    break;
                }
                Thread.SpinWait(100);
            }


            RosSocket.Unsubscribe(sub_id);
            RosSocket.Unadvertise(pub_id);

            Assert.IsTrue(succes, "Failed to received Data");
        }



        [Test]
        public void PubSubTestJson2b()
        {
            List<string> messages = new List<string>();
            var topic = "/pubsub_test2";
            std_msgs.String message = new std_msgs.String
            {
                data = "publication test message data"
            };

            var json = JsonSerializer.Serialize(message);

            Console.WriteLine(json);
            string sub_id = RosSocket.Subscribe(std_msgs.String.RosMessageName, topic,
                (string topic, string msg, string rosType) => {
                    msg = msg.Replace(": ", ":");
                    messages.Add(msg);
                    Console.WriteLine($"Received: {msg}, on topic {topic}, with type {rosType}");
                });
            Thread.SpinWait(100);
            string pub_id = RosSocket.Advertise(typeof(std_msgs.String), topic);
            RosSocket.Publish(pub_id, message);


            DateTime breaktime = DateTime.Now.AddSeconds(10);
            bool succes = false;
            while (DateTime.Now < breaktime)
            {
                if (messages.Where(o => o == json).Any())
                {
                    succes = true;
                    break;
                }
                Thread.SpinWait(100);
            }


            RosSocket.Unsubscribe(sub_id);
            RosSocket.Unadvertise(pub_id);

            Assert.IsTrue(succes, "Failed to received Data");
        }

        //[Test]
        //public void SubscriptionTest()
        //{

        //    OnMessageReceived.WaitOne();
        //    OnMessageReceived.Reset();
        //    RosSocket.Unsubscribe(id);
        //    Thread.Sleep(100);
        //    Assert.IsTrue(true);
        //}

        //[Test]
        //public void ServiceCallTest()
        //{
        //    RosSocket.CallService<rosapi.GetParamRequest, rosapi.GetParamResponse>("/rosapi/get_param", ServiceCallHandler, new rosapi.GetParamRequest("/rosdistro", "default"));
        //    OnServiceReceived.WaitOne();
        //    OnServiceReceived.Reset();
        //    Assert.IsTrue(true);
        //}

        //[Test]
        //public void ServiceResponseTest()
        //{
        //    string id = RosSocket.AdvertiseService<std_srvs.TriggerRequest, std_srvs.TriggerResponse>("/service_response_test", ServiceResponseHandler);
        //    OnServiceProvided.WaitOne();
        //    OnServiceProvided.Reset();
        //    RosSocket.UnadvertiseService(id);
        //    Assert.IsTrue(true);
        //}

        private void SubscriptionHandler(std_msgs.String message)
        {
            OnMessageReceived.Set();
        }

        private void ServiceCallHandler(rosapi.GetParamResponse message)
        {
            OnServiceReceived.Set();
        }

        private bool ServiceResponseHandler(std_srvs.TriggerRequest arguments, out std_srvs.TriggerResponse result)
        {
            result = new std_srvs.TriggerResponse(true, "service response message");
            OnServiceProvided.Set();
            return true;
        }
    }
}