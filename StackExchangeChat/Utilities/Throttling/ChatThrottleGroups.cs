using System;
using Utilities.Throttling;

namespace StackExchangeChat.Utilities.Throttling
{
    public class ChatThrottleGroups
    {
        public static ThrottleGroup WebRequestThrottle = new ThrottleGroup("SE.WebRequest");
    }
}
