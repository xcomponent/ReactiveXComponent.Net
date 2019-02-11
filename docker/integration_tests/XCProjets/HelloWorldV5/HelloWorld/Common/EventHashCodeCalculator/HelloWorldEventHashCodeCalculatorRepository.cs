using System.Collections.Generic;
using XComponent.Runtime.Processing.HashCode;

namespace XComponent.HelloWorld.Common.EventHashCodeCalculator
{
	public class HelloWorldEventHashCodeCalculatorRepository : IEventHashCodeCalculatorRepository
	{
		Dictionary<int, IEventHashCodeCalculator> _calculators;

		public HelloWorldEventHashCodeCalculatorRepository()
		{
			_calculators = new Dictionary<int, IEventHashCodeCalculator>();
			_calculators.Add(6, new XComponent.HelloWorld.Common.EventHashCodeCalculator.HelloWorld.DefaultEventEventHashCodeCalculator());
			_calculators.Add(10, new XComponent.HelloWorld.Common.EventHashCodeCalculator.HelloWorld.CreateListenerEventHashCodeCalculator());
			_calculators.Add(20, new XComponent.HelloWorld.Common.EventHashCodeCalculator.HelloWorld.SayHelloEventHashCodeCalculator());
		}

		public Dictionary<int, IEventHashCodeCalculator> GetEventHashCodeCalculators()
		{
			return _calculators;
		}
	}
}