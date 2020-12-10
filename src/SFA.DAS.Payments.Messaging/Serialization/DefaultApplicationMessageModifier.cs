using System;

namespace SFA.DAS.Payments.Messaging.Serialization
{
    public class DefaultApplicationMessageModifier: IApplicationMessageModifier
    {
        public Type EventType { get; } = typeof(object);

        public object Modify(object applicationMessage)
        {
            return applicationMessage;
        }
    }
}