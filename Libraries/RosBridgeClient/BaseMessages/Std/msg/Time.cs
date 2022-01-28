/*
© Siemens AG, 2017-2018
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

ROS1/ROS2 delineation 2022 by Chris Tacke (ctacke@gmail.com)
*/

namespace RosSharp.RosBridgeClient.MessageTypes.Std
{
    public class Time : Message
    {
        public const string RosMessageName = "std_msgs/Time";
#if !ROS1 // ROS2 changed these
        [System.Text.Json.Serialization.JsonPropertyName("sec")]
#endif
        public uint secs { get; set; }

#if !ROS1
        [System.Text.Json.Serialization.JsonPropertyName("nanosec")]
#endif
        public uint nsecs { get; set; }

        public Time()
        {
            secs = 0;
            nsecs = 0;
        }

        public Time(uint secs, uint nsecs)
        {
            this.secs = secs;
            this.nsecs = nsecs;
        }
    }
}
