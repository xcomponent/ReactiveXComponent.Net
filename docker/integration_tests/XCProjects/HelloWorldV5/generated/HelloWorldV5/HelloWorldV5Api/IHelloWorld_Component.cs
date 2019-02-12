using System;
using XCClientAPICommon.Error;
using XComponent.Common.ApiContext;
using XComponent.Shared.Api.Client;
using XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld;

namespace XComponent.HelloWorldV5.HelloWorldV5Api
{
    public interface IHelloWorld_Component : IMonitorableComponent
    {
        IHelloWorld_StateMachine HelloWorld_StateMachine { get ; }

        IHelloWorldResponse_StateMachine HelloWorldResponse_StateMachine { get ; }

        IResponseListener_StateMachine ResponseListener_StateMachine { get ; }


        event Action<RuntimeError> HelloWorldErrorReceived;

        HelloWorldInstance GetEntryPoint();
    }
}