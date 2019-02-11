using System;
using System.Collections.Generic;
using XComponent.Runtime.Processing.HashCode;
using XComponent.Shared.Helpers;

namespace XComponent.HelloWorld.Common.StateHashCodeCalculator.HelloWorldResponse
{
    public class PublishedStateHashCodeCalculator : IStateHashCodeCalculator<XComponent.HelloWorld.UserObject.HelloWorldResponse, Object>
    {
        public HashSet<int> Calculate(XComponent.HelloWorld.UserObject.HelloWorldResponse publicMember, Object internalMember, out StateHashCodeCalculatorResultType resultType)
        {
            var hashCodeForPropertyOriginatorNameOfPublicMember = object.ReferenceEquals(publicMember.OriginatorName, null) ? 0 : publicMember.OriginatorName.GetHashCode();

			var hashcodes = new HashSet<int>();
			resultType = StateHashCodeCalculatorResultType.Other;
            hashcodes.Add(0);
            hashcodes.Add( hashCodeForPropertyOriginatorNameOfPublicMember);
            return hashcodes;
        }
    }
}
