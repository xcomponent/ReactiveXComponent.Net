using System;
using XComponent.Common.ApiContext;
using XComponent.Common.Timeouts;
using XComponent.HelloWorld.Common;
using XComponent.HelloWorld.Common.Senders;
using XComponent.HelloWorld.UserObject;
using XComponent.Shared;

namespace XComponent.HelloWorld.TriggeredMethod
{
    public static class HelloWorldTriggeredMethod
    {
        public static void ExecuteOn_Listening_Through_Init(XComponent.Common.Event.DefaultEvent defaultEvent, object object_PublicMember, object object_InternalMember, RuntimeContext context, IInitDefaultEventOnListeningHelloWorldSenderInterface sender)
        {
            sender.CreateListener(context, new CreateListener());
        }

        private static int _initializationCount = 0;

        public static void ExecuteOn_EntryPoint(object object_PublicMember, object object_InternalMember, RuntimeContext context, EntryPointSender sender)
        {
            _initializationCount++;

            if (_initializationCount > 1)
            {
                TriggeredMethodContext.Instance.GetDefaultLogger().Error("Component entrypoint initialized more than once!");
                System.Environment.Exit(-1);
            }
        }
    }
}