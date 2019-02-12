using System;
using System.Collections.Generic;
using XComponent.Runtime.Processing.HashCode;
using XComponent.Shared.Helpers;

namespace XComponent.HelloWorld.Common.StateHashCodeCalculator.ResponseListener
{
    public class UpStateHashCodeCalculator : IStateHashCodeCalculator<XComponent.HelloWorld.UserObject.ResponseListener, XComponent.HelloWorld.UserObject.ResponseListenerInternal>
    {
        public HashSet<int> Calculate(XComponent.HelloWorld.UserObject.ResponseListener publicMember, XComponent.HelloWorld.UserObject.ResponseListenerInternal internalMember, out StateHashCodeCalculatorResultType resultType)
        {
			resultType = StateHashCodeCalculatorResultType.Zero;
			return null;
		}
    }
}
