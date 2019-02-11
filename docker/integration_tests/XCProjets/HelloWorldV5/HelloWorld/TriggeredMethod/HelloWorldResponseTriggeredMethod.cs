using System;
using XComponent.Common.ApiContext;
using XComponent.Common.Timeouts;
using XComponent.HelloWorld.Common;
using XComponent.HelloWorld.Common.Senders;
using XComponent.HelloWorld.UserObject;
using XComponent.Shared;
using XComponent.Shared.Failover;

namespace XComponent.HelloWorld.TriggeredMethod
{
    public static class HelloWorldResponseTriggeredMethod
    {
        public static void ExecuteOn_Published_Through_Ping(XComponent.HelloWorld.UserObject.Ping ping, XComponent.HelloWorld.UserObject.HelloWorldResponse helloWorldResponse, object object_InternalMember, RuntimeContext context, IPingPingOnPublishedHelloWorldResponseSenderInterface sender)
        {
            helloWorldResponse.PingCount += 1;
            context.AutomatedFailover = false;
            context.RecordPublicMemberChange(new PingCountModified(helloWorldResponse.PingCount));
        }

        public static void ExecuteOn_Published_Through_SayHello(XComponent.HelloWorld.UserObject.SayHello sayHello, XComponent.HelloWorld.UserObject.HelloWorldResponse helloWorldResponse, object object_InternalMember, RuntimeContext context, ISayHelloSayHelloOnPublishedHelloWorldResponseSenderInterface sender)
        {
            helloWorldResponse.OriginatorName = sayHello.Name;
            context.AutomatedFailover = false;
            context.RecordPublicMemberChange(new OriginatorNameChanged(helloWorldResponse.OriginatorName));
        }

        public static void ExecuteOn_Published_Through_SayGoodbye(XComponent.HelloWorld.UserObject.SayGoodbye sayGoodbye, XComponent.HelloWorld.UserObject.HelloWorldResponse helloWorldResponse, object object_InternalMember, RuntimeContext context, ISayGoodbyeSayGoodbyeOnPublishedHelloWorldResponseSenderInterface sender)
        {
            sender.Kill(context, new Kill());
        }

        public static void ExecuteOn_Published_Through_SayGoodbyeToAll(XComponent.HelloWorld.UserObject.SayGoodbyeToAll sayGoodbyeToAll, XComponent.HelloWorld.UserObject.HelloWorldResponse helloWorldResponse, object object_InternalMember, RuntimeContext context, ISayGoodbyeToAllSayGoodbyeToAllOnPublishedHelloWorldResponseSenderInterface sender)
        {
            sender.Kill(context, new Kill());
        }
    }
}