namespace XComponent.HelloWorldV5.HelloWorldV5Api
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Collections.Generic;
    using XComponent.Common.Processing;
    using XComponent.Common.ApiContext;
    using XComponent.Communication;
    using XComponent.Communication.Serialization;
    using XCClientAPICommon.Client;
 

    public interface IHelloWorldV5ApiCommunication: XComponent.Communication.ClientApi.IClientAPICommunication
    {
        SnapshotResponse GetSnapshot(int stateMachineCode, int componentCode, string filter, int timeout, int? chunkSize);

        XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.HelloWorldInstance GetHelloWorldEntryPoint();

        void SendToHelloWorld(Context context, XComponent.HelloWorld.UserObject.CreateListener evt, Visibility visibility = Visibility.Public, string topic = null);
        void SendToHelloWorld(Context context, XComponent.HelloWorld.UserObject.SayHello evt, Visibility visibility = Visibility.Public, string topic = null);
        void SendToHelloWorld(Context context, XComponent.HelloWorld.UserObject.Kill evt, Visibility visibility = Visibility.Public, string topic = null);
        void SendToHelloWorld(Context context, XComponent.HelloWorld.UserObject.Ping evt, Visibility visibility = Visibility.Public, string topic = null);
        void SendToHelloWorld(Context context, XComponent.HelloWorld.UserObject.SayGoodbyeToAll evt, Visibility visibility = Visibility.Public, string topic = null);
        void SendToHelloWorld(Context context, XComponent.HelloWorld.UserObject.SayGoodbye evt, Visibility visibility = Visibility.Public, string topic = null);
        

        void SendEventToHelloWorld(XComponent.HelloWorld.UserObject.CreateListener evt, Visibility visibility = Visibility.Public, string topic = null);
        void SendEventToHelloWorld(HelloWorld_Component.StdEnum stdEnum, XComponent.HelloWorld.UserObject.CreateListener evt, Visibility visibility = Visibility.Public, string topic = null);
        void SendEventToHelloWorld(XComponent.HelloWorld.UserObject.SayHello evt, Visibility visibility = Visibility.Public, string topic = null);
        void SendEventToHelloWorld(HelloWorld_Component.StdEnum stdEnum, XComponent.HelloWorld.UserObject.SayHello evt, Visibility visibility = Visibility.Public, string topic = null);
        void SendEventToHelloWorld(XComponent.HelloWorld.UserObject.Kill evt, Visibility visibility = Visibility.Public, string topic = null);
        void SendEventToHelloWorld(HelloWorld_Component.StdEnum stdEnum, XComponent.HelloWorld.UserObject.Kill evt, Visibility visibility = Visibility.Public, string topic = null);
        void SendEventToHelloWorld(XComponent.HelloWorld.UserObject.Ping evt, Visibility visibility = Visibility.Public, string topic = null);
        void SendEventToHelloWorld(HelloWorld_Component.StdEnum stdEnum, XComponent.HelloWorld.UserObject.Ping evt, Visibility visibility = Visibility.Public, string topic = null);
        void SendEventToHelloWorld(XComponent.HelloWorld.UserObject.SayGoodbye evt, Visibility visibility = Visibility.Public, string topic = null);
        void SendEventToHelloWorld(HelloWorld_Component.StdEnum stdEnum, XComponent.HelloWorld.UserObject.SayGoodbye evt, Visibility visibility = Visibility.Public, string topic = null);
        void SendEventToHelloWorld(XComponent.HelloWorld.UserObject.SayGoodbyeToAll evt, Visibility visibility = Visibility.Public, string topic = null);
        void SendEventToHelloWorld(HelloWorld_Component.StdEnum stdEnum, XComponent.HelloWorld.UserObject.SayGoodbyeToAll evt, Visibility visibility = Visibility.Public, string topic = null);
        
        

        event Action<XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.ResponseListenerInstance> HelloWorld_ResponseListenerInstanceUpdated;
        event Action<XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.HelloWorldResponseInstance> HelloWorld_HelloWorldResponseInstanceUpdated;
        

        event Action<XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.HelloWorldInstance> HelloWorld_HelloWorldInstanceUpdated;
        
        event System.Action<XCClientAPICommon.Error.RuntimeError> HelloWorldErrorReceived;
        
        

        bool Init(string configFile, out XCClientAPICommon.Client.InitReport report, IClientApiConfigurationOverride communicationOverrides = null);
    }
}

