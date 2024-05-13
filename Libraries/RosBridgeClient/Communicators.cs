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

Non-generic communication support 2022 by Chris Tacke (ctacke@gmail.com)
*/

using System;
using System.Collections.Generic;

namespace RosSharp.RosBridgeClient
{
    public delegate void ServiceResponseHandler<T>(T t) where T : Message;
    public delegate void SubscriptionHandler<T>(T t) where T : Message;
    public delegate bool ServiceCallHandler<Tin, Tout>(Tin tin, out Tout tout) where Tin : Message where Tout : Message;

    // DEV NOTE: Not thrilled with the name, but trying to keep backward compatibility
    // This allows subscribers to know what topic the incoming data is actually for
    public delegate void SubscriptionHandler2<T>(string topic, T t) where T : Message;
    public delegate void SubscriptionHandler2(string topic, object data);

    internal abstract class Communicator
    {
        public static string GetRosName<T>() where T : Message
        {
            return (string)typeof(T).GetField("RosMessageName").GetRawConstantValue();
        }

        public static string GetRosName(Type messageType)
        {
            if (!typeof(Message).IsAssignableFrom(messageType))
            {
                throw new ArgumentException("messageType parameter must derive from type 'Message'");
            }

            return (string)messageType.GetField("RosMessageName").GetRawConstantValue();
        }
    }

    internal class Publisher : Communicator
    {
        internal string Id { get; }
        internal string Topic { get; }
        internal Type MessageType { get; }

        public Publisher(Type messageType, string id, string topic, out Advertisement advertisement)
        {
            MessageType = messageType;
            Id = id;
            Topic = topic;
            advertisement = new Advertisement(Id, Topic, GetRosName(messageType));
        }

        internal Communication Publish(Message message)
        {
            return new Publication(Id, Topic, message);
        }

        internal Unadvertisement Unadvertise()
        {
            return new Unadvertisement(Id, Topic);
        }
    }

    internal class Publisher<T> : Publisher where T : Message
    {
        internal Publisher(string id, string topic, out Advertisement advertisement)
            : base(typeof(T), id, topic, out advertisement)
        {
        }

        internal Communication Publish(T message)
        {
            return new Publication<T>(Id, Topic, message);
        }
    }

    internal abstract class Subscriber : Communicator
    {
        internal abstract string Id { get; }
        internal abstract string Topic { get; }
        internal abstract Type TopicType { get; }
        internal abstract Subscription Subscription { get; }


        internal abstract void Receive(string message, ISerializer serializer);

        internal Unsubscription Unsubscribe()
        {
            return new Unsubscription(Id, Topic);
        }
    }

    internal class Subscriber<T> : Subscriber where T : Message
    {

        internal SubscriptionHandler<T> SubscriptionHandler { get; }

        internal override string Id { get; }

        internal override string Topic { get; }

        internal override Type TopicType { get => typeof(T); }

        internal override Subscription Subscription { get; }

        internal Subscriber(string id, string topic, SubscriptionHandler<T> subscriptionHandler, out Subscription subscription, int throttle_rate = 0, int queue_length = 1, int fragment_size = int.MaxValue, string compression = "none")
        {
            Id = id;
            Topic = topic;
            SubscriptionHandler = subscriptionHandler;
            this.Subscription = new Subscription(id, Topic, GetRosName<T>(), throttle_rate, queue_length, fragment_size, compression);
            subscription = this.Subscription;
        }

        internal override void Receive(string message, ISerializer serializer)
        {
            SubscriptionHandler?.Invoke(serializer.Deserialize<T>(message));
        }
    }


    internal class Subscriber2 : Subscriber
    {

        internal override string Id { get; }

        internal override string Topic { get; }

        internal override Type TopicType { get; }

        internal override Subscription Subscription { get; }

        internal SubscriptionHandler2 SubscriptionHandler { get; }

        internal Subscriber2(string id, string topic, SubscriptionHandler2 subscriptionHandler, Type type, out Subscription subscription, int throttle_rate = 0, int queue_length = 1, int fragment_size = int.MaxValue, string compression = "none")
        {
            if (!typeof(Message).IsAssignableFrom(type))
            {
                throw new ArgumentException("type parameter must derive from type 'Message'");
            }

            Id = id;
            Topic = topic;
            SubscriptionHandler = subscriptionHandler;
            TopicType = type;

            Subscription = new Subscription(id, Topic, GetRosName(type), throttle_rate, queue_length, fragment_size, compression);
            subscription = Subscription;
        }
        internal override void Receive(string message, ISerializer serializer)
        {
            SubscriptionHandler?.Invoke(Topic, serializer.Deserialize(message, TopicType));
        }
    }

    internal class Subscriber2<T> : Subscriber where T : Message
    {

        internal override string Id { get; }

        internal override string Topic { get; }

        internal override Type TopicType { get; }

        internal override Subscription Subscription { get; }

        internal SubscriptionHandler2<T> SubscriptionHandler { get; }

        internal Subscriber2(string id, string topic, SubscriptionHandler2<T> subscriptionHandler, out Subscription subscription, int throttle_rate = 0, int queue_length = 1, int fragment_size = int.MaxValue, string compression = "none")
        {
            Id = id;
            Topic = topic;
            SubscriptionHandler = subscriptionHandler;
            TopicType = typeof(T);

            Subscription = new Subscription(id, Topic, GetRosName<T>(), throttle_rate, queue_length, fragment_size, compression);
            subscription = Subscription;
        }
        internal override void Receive(string message, ISerializer serializer)
        {
            SubscriptionHandler?.Invoke(Topic, (T)serializer.Deserialize(message, TopicType));
        }
    }


    internal abstract class ServiceProvider : Communicator
    {
        internal abstract string Service { get; }

        internal abstract Communication Respond(string id, string message, ISerializer serializer);

        internal ServiceUnadvertisement UnadvertiseService()
        {
            return new ServiceUnadvertisement(Service);
        }
    }

    internal class ServiceProvider<Tin, Tout> : ServiceProvider where Tin : Message where Tout : Message
    {
        internal override string Service { get; }
        internal ServiceCallHandler<Tin, Tout> ServiceCallHandler;
        internal ServiceProvider(string service, ServiceCallHandler<Tin, Tout> serviceCallHandler, out ServiceAdvertisement serviceAdvertisement)
        {
            Service = service;
            ServiceCallHandler = serviceCallHandler;
            serviceAdvertisement = new ServiceAdvertisement(service, GetRosName<Tin>());
        }

        internal override Communication Respond(string id, string message, ISerializer serializer)
        {
            bool isSuccess = ServiceCallHandler.Invoke(serializer.Deserialize<Tin>(message), out Tout result);
            return new ServiceResponse<Tout>(id, Service, result, isSuccess);
        }
    }

    internal abstract class ServiceConsumer
    {
        internal abstract string Id { get; }
        internal abstract string Service { get; }
        internal abstract void Consume(string message, ISerializer serializer);
    }

    internal class ServiceConsumer<Tin, Tout> : ServiceConsumer where Tin : Message where Tout : Message
    {
        internal override string Id { get; }
        internal override string Service { get; }
        internal ServiceResponseHandler<Tout> ServiceResponseHandler;

        internal ServiceConsumer(string id, string service, ServiceResponseHandler<Tout> serviceResponseHandler, out Communication serviceCall, Tin serviceArguments)
        {
            Id = id;
            Service = service;
            ServiceResponseHandler = serviceResponseHandler;
            serviceCall = new ServiceCall<Tin>(id, service, serviceArguments);
        }
        internal override void Consume(string message, ISerializer serializer)
        {
            ServiceResponseHandler.Invoke(serializer.Deserialize<Tout>(message));
        }
    }
}
