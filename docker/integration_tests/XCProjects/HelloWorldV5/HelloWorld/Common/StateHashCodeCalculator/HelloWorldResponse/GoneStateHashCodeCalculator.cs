using System;
using System.Collections.Generic;
using XComponent.Runtime.Processing.HashCode;
using XComponent.Shared.Helpers;

namespace XComponent.HelloWorld.Common.StateHashCodeCalculator.HelloWorldResponse
{
    public class GoneStateHashCodeCalculator : IStateHashCodeCalculator<XComponent.HelloWorld.UserObject.HelloWorldResponse, Object>
    {
        public HashSet<int> Calculate(XComponent.HelloWorld.UserObject.HelloWorldResponse publicMember, Object internalMember, out StateHashCodeCalculatorResultType resultType)
        {
            resultType = StateHashCodeCalculatorResultType.Empty;
			return null;
		}
    }
}
