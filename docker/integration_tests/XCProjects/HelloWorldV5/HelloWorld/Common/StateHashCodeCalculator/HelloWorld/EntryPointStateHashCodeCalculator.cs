using System;
using System.Collections.Generic;
using XComponent.Runtime.Processing.HashCode;
using XComponent.Shared.Helpers;

namespace XComponent.HelloWorld.Common.StateHashCodeCalculator.HelloWorld
{
    public class EntryPointStateHashCodeCalculator : IStateHashCodeCalculator<Object, Object>
    {
        public HashSet<int> Calculate(Object publicMember, Object internalMember, out StateHashCodeCalculatorResultType resultType)
        {
			resultType = StateHashCodeCalculatorResultType.Zero;
			return null;
		}
    }
}
