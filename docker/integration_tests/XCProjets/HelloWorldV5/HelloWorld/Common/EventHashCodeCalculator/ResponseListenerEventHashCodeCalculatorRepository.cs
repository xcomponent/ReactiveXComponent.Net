using System.Collections.Generic;
using XComponent.Runtime.Processing.HashCode;

namespace XComponent.HelloWorld.Common.EventHashCodeCalculator
{
	public class ResponseListenerEventHashCodeCalculatorRepository : IEventHashCodeCalculatorRepository
	{
		Dictionary<int, IEventHashCodeCalculator> _calculators;

		public ResponseListenerEventHashCodeCalculatorRepository()
		{
			_calculators = new Dictionary<int, IEventHashCodeCalculator>();
			_calculators.Add(11, new XComponent.HelloWorld.Common.EventHashCodeCalculator.ResponseListener.HelloWorldResponseEventHashCodeCalculator());
		}

		public Dictionary<int, IEventHashCodeCalculator> GetEventHashCodeCalculators()
		{
			return _calculators;
		}
	}
}