using NUnit.Framework;
using RosSharp.RosBridgeClient;
using System;
using System.Collections.Generic;
using System.Text;
using Assert = NUnit.Framework.Assert;

using std_msgs = RosSharp.RosBridgeClient.MessageTypes.Std;
using std_srvs = RosSharp.RosBridgeClient.MessageTypes.Std;
using rosapi = RosSharp.RosBridgeClient.MessageTypes.Rosapi;

namespace RosSharp.RosBridgeClient.Tests
{
    [TestFixture()]
    public class RosSocketTests
    {
        
        // on ROS system:
        // launch before starting:
        // roslaunch rosbridge_server rosbridge_websocket.launch
        // rostopic echo /publication_test
        // rostopic pub /subscription_test std_msgs/String "subscription test message data"

        // launch after starting:
        // rosservice call /service_response_test

        private static readonly string Uri = "ws://localhost:9090";
        private static RosSocket RosSocket;
        private ManualResetEvent OnMessageReceived = new ManualResetEvent(false);
        private ManualResetEvent OnServiceReceived = new ManualResetEvent(false);
        private ManualResetEvent OnServiceProvided = new ManualResetEvent(false);

        [SetUp]
        public void Setup()
        {

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
        public void PubSubTest2_a()
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