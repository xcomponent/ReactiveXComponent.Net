using System;
using XCClientAPICommon.Error;
using XComponent.Common.ApiContext;
using XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld;

namespace XComponent.HelloWorldV5.HelloWorldV5Api
{
    public sealed class HelloWorld_Component : IHelloWorld_Component
    {
		public enum StdEnum
		{
			HelloWorld = -69981087,
			HelloWorldResponse = -343862282,
			ResponseListener = -1152526010,
		}

        private readonly IHelloWorldV5ApiCommunication _communicationLayer;

        public const int ComponentCode = -69981087;
	
        public IHelloWorld_StateMachine HelloWorld_StateMachine { get ; private set ; }
	
        public IHelloWorldResponse_StateMachine HelloWorldResponse_StateMachine { get ; private set ; }
	
        public IResponseListener_StateMachine ResponseListener_StateMachine { get ; private set ; }
	
        public event Action<RuntimeError> HelloWorldErrorReceived;

        public HelloWorld_Component(IHelloWorldV5ApiCommunication communicationLayer)
        {
            _communicationLayer = communicationLayer;
			HelloWorld_StateMachine = new HelloWorld_StateMachine(_communicationLayer);
			HelloWorldResponse_StateMachine = new HelloWorldResponse_StateMachine(_communicationLayer);
			ResponseListener_StateMachine = new ResponseListener_StateMachine(_communicationLayer);
            _communicationLayer.HelloWorldErrorReceived += OnHelloWorldErrorReceived;
        }

        public HelloWorldInstance GetEntryPoint()
        {
            return _communicationLayer.GetHelloWorldEntryPoint();
        }

        private void OnHelloWorldErrorReceived(XCClientAPICommon.Error.RuntimeError errorMessage)
        {
            if (HelloWorldErrorReceived != null)
            {
                HelloWorldErrorReceived(errorMessage);
            }
        }
    }
}