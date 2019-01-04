using Utilities.Throttling;

namespace StackExchangeApi
{
    public class ApiThrottleGroups
    {
        public static ThrottleGroup ApiThrottleGroup = new ThrottleGroup("SE.Api");
    }
}
