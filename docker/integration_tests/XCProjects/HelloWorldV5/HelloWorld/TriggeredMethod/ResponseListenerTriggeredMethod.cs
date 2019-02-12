using System;
using XComponent.Common.ApiContext;
using XComponent.Common.Timeouts;
using XComponent.HelloWorld.Common;
using XComponent.HelloWorld.Common.Senders;
using XComponent.Shared;

namespace XComponent.HelloWorld.TriggeredMethod
{
    public static class ResponseListenerTriggeredMethod
    {
        public static void ExecuteOn_Up_Through_Count(XComponent.HelloWorld.UserObject.HelloWorldResponse helloWorldResponse, XComponent.HelloWorld.UserObject.ResponseListener responseListener, XComponent.HelloWorld.UserObject.ResponseListenerInternal responseListenerInternal, RuntimeContext context, ICountHelloWorldResponseOnUpResponseListenerSenderInterface sender)
        {
            responseListener.Count += 1;
        }

        public static void ExecuteOn_Up_Through_CreateListener(XComponent.HelloWorld.UserObject.CreateListener createListener, XComponent.HelloWorld.UserObject.ResponseListener responseListener, XComponent.HelloWorld.UserObject.ResponseListenerInternal responseListenerInternal, RuntimeContext context, ICreateListenerCreateListenerOnUpResponseListenerSenderInterface sender)
        {
            responseListenerInternal.OpenConnection();
        }
    }
}