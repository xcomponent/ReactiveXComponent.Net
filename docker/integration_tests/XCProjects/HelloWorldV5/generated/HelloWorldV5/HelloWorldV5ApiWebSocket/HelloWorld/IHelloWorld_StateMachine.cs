namespace XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld
{
    using System;
    using System.Collections.Generic;
    using XComponent.Common.ApiContext;
    using XCClientAPICommon.Client;
    using XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.HelloWorld;

    public interface IHelloWorld_StateMachine : INotifyInstanceUpdated<XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.HelloWorldInstance>
    {
        /// <summary>
        ///
        /// </summary>
        IEntryPoint_State EntryPoint_State { get ; }
        /// <summary>
        ///
        /// </summary>
        IListening_State Listening_State { get ; }
        
        /// <summary>
        ///Retrieves a snapshot of HelloWorld state machines
        /// </summary>
        ///<param name="filter">This parameter is used to filter the snapshot request with a linq expression. The linq expression is applied to the public member</param>
        ///<param name="timeout">Snapshot timeout</param>
        /// <param name="chunkSize">Maximum number of items to be sent by the server in every snapshot chunk or <code>null</code> or 0 if unlimited</param>
        List<XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.HelloWorldInstance> GetSnapshot(string filter = null, int timeout = 10000, int? chunkSize = null);
    
        ///<summary>
        ///Sends the XComponent.HelloWorld.UserObject.CreateListener event using a context. 
        ///</summary>
        ///<param name="context">Client Api context. Used to target a specific instance of state machine</param>
        ///<param name="transitionEvent"></param>
        ///<param name="onExceptionOccured">This event is triggered if an exception is raised. If this option is set, the exception will be catch. If this option is not set, the exception will be thrown.</param>
        ///<param name="visibility">If parameter is set to Visibility.Private, the event will be sent with a private topic.</param>
        void SendEvent(XComponent.Common.ApiContext.Context context, XComponent.HelloWorld.UserObject.CreateListener transitionEvent, Action<Exception> onExceptionOccured = null , Visibility visibility = Visibility.Public, string topic = null);
        ///<summary>
        ///Sends the XComponent.HelloWorld.UserObject.SayHello event using a context. 
        ///</summary>
        ///<param name="context">Client Api context. Used to target a specific instance of state machine</param>
        ///<param name="transitionEvent"></param>
        ///<param name="onExceptionOccured">This event is triggered if an exception is raised. If this option is set, the exception will be catch. If this option is not set, the exception will be thrown.</param>
        ///<param name="visibility">If parameter is set to Visibility.Private, the event will be sent with a private topic.</param>
        void SendEvent(XComponent.Common.ApiContext.Context context, XComponent.HelloWorld.UserObject.SayHello transitionEvent, Action<Exception> onExceptionOccured = null , Visibility visibility = Visibility.Public, string topic = null);
        

        ///<summary>
        ///Sends the XComponent.HelloWorld.UserObject.CreateListener event
        ///</summary>
        ///<param name="evt"></param>
        ///<param name="onExceptionOccured">This event is triggered if an exception is raised. If this option is set, the exception will be catch. If this option is not set, the exception will be thrown.</param>
        ///<param name="visibility">If parameter is set to Visibility.Private, the event will be sent with a private topic.</param>
        void SendEvent(XComponent.HelloWorld.UserObject.CreateListener evt, Action<Exception> onExceptionOccured = null , Visibility visibility = Visibility.Public, string topic = null);
        ///<summary>
        ///Sends the XComponent.HelloWorld.UserObject.SayHello event
        ///</summary>
        ///<param name="evt"></param>
        ///<param name="onExceptionOccured">This event is triggered if an exception is raised. If this option is set, the exception will be catch. If this option is not set, the exception will be thrown.</param>
        ///<param name="visibility">If parameter is set to Visibility.Private, the event will be sent with a private topic.</param>
        void SendEvent(XComponent.HelloWorld.UserObject.SayHello evt, Action<Exception> onExceptionOccured = null , Visibility visibility = Visibility.Public, string topic = null);
                
    }
}
