﻿/*
© Siemens AG, 2019
Author: Sifan Ye (sifan.ye@siemens.com)

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at
<http://www.apache.org/licenses/LICENSE-2.0>.
Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

Microsoft-libs only added 2022 by Chris Tacke (ctacke@gmail.com)

*/

namespace RosSharp.RosBridgeClient.Protocols
{
#if MS_LIBS_ONLY
    public enum Protocol { WebSocketNET };

    public class ProtocolInitializer
    {
        public static IProtocol GetProtocol(Protocol protocol, string serverURL)
        {
            switch (protocol)
            {
                case Protocol.WebSocketNET:
                    return new WebSocketNetProtocol(serverURL);
                default:
                    return null;
            }
        }
    }
#else
    public enum Protocol { WebSocketSharp, WebSocketNET };

    public class ProtocolInitializer
    {
        public static IProtocol GetProtocol(Protocol protocol, string serverURL)
        {
            switch (protocol)
            {
                case Protocol.WebSocketSharp:
                    return new WebSocketSharpProtocol(serverURL);
                case Protocol.WebSocketNET:
                    return new WebSocketNetProtocol(serverURL);
                default:
                    return null;
            }
        }
    }
#endif
}
