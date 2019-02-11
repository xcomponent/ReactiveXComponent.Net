using System.Collections.Generic;
using XComponent.Runtime.Processing.HashCode;

namespace XComponent.HelloWorld.Common.EventHashCodeCalculator
{
	public class HelloWorldResponseEventHashCodeCalculatorRepository : IEventHashCodeCalculatorRepository
	{
		Dictionary<int, IEventHashCodeCalculator> _calculators;

		public HelloWorldResponseEventHashCodeCalculatorRepository()
		{
			_calculators = new Dictionary<int, IEventHashCodeCalculator>();
			_calculators.Add(12, new XComponent.HelloWorld.Common.EventHashCodeCalculator.HelloWorldResponse.KillEventHashCodeCalculator());
			_calculators.Add(14, new XComponent.HelloWorld.Common.EventHashCodeCalculator.HelloWorldResponse.PingEventHashCodeCalculator());
			_calculators.Add(18, new XComponent.HelloWorld.Common.EventHashCodeCalculator.HelloWorldResponse.SayGoodbyeEventHashCodeCalculator());
			_calculators.Add(19, new XComponent.HelloWorld.Common.EventHashCodeCalculator.HelloWorldResponse.SayGoodbyeToAllEventHashCodeCalculator());
		}

		public Dictionary<int, IEventHashCodeCalculator> GetEventHashCodeCalculators()
		{
			return _calculators;
		}
	}
}