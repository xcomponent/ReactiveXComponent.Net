﻿using System;
using System.Collections.Generic;
using XComponent.Runtime.Processing.HashCode;
using XComponent.Common.Logger.Logger;
using XComponent.Shared.Helpers;

namespace XComponent.HelloWorld.Common.EventHashCodeCalculator.HelloWorld
{
    public class CreateListenerEventHashCodeCalculator : IEventHashCodeCalculator
    {
		private XComponentLogger _logger = new XComponentLogger();

        public int[] Calculate(object rawEvent)
        {
			var hashCodes = new int[1];
			var typedEvent = rawEvent as XComponent.HelloWorld.UserObject.CreateListener;
			if(typedEvent == null)
			{
				_logger.Error(string.Format("{0}EventHashCodeCalculator should be called with event {1} instead of {2}. This may be due to a generation problem.", "CreateListener", "XComponent.HelloWorld.UserObject.CreateListener", rawEvent != null ? rawEvent.GetType().ToString() : string.Empty));
				return new int[0];
			}

            hashCodes[0] = 0;

			return hashCodes;
        }
    }
}