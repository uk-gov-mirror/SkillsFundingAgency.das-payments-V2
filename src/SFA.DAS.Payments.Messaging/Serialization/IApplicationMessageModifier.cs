using System;

namespace SFA.DAS.Payments.Messaging.Serialization
{
    public interface IApplicationMessageModifier
    {
        Type EventType { get; }

        object Modify(object applicationMessage);
    }
}