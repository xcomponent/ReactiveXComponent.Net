using System;
using System.Collections.Generic;
using XComponent.Runtime.Processing.HashCode;
using XComponent.Common.Logger.Logger;
using XComponent.Shared.Helpers;

namespace XComponent.HelloWorld.Common.EventHashCodeCalculator.HelloWorldResponse
{
    public class SayGoodbyeToAllEventHashCodeCalculator : IEventHashCodeCalculator
    {
		private XComponentLogger _logger = new XComponentLogger();

        public int[] Calculate(object rawEvent)
        {
			var hashCodes = new int[1];
			var typedEvent = rawEvent as XComponent.HelloWorld.UserObject.SayGoodbyeToAll;
			if(typedEvent == null)
			{
				_logger.Error(string.Format("{0}EventHashCodeCalculator should be called with event {1} instead of {2}. This may be due to a generation problem.", "SayGoodbyeToAll", "XComponent.HelloWorld.UserObject.SayGoodbyeToAll", rawEvent != null ? rawEvent.GetType().ToString() : string.Empty));
				return new int[0];
			}
            var hashCodeForPropertyWithNameOfEvent = object.ReferenceEquals(typedEvent.WithName, null) ? 0 : typedEvent.WithName.GetHashCode();

            hashCodes[0] = hashCodeForPropertyWithNameOfEvent;

			return hashCodes;
        }
    }
}
