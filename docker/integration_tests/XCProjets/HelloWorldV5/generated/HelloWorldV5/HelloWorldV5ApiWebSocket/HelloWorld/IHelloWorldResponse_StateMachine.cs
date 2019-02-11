namespace XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld
{
    using System;
    using System.Collections.Generic;
    using XComponent.Common.ApiContext;
    using XCClientAPICommon.Client;
    using XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.HelloWorldResponse;

    public interface IHelloWorldResponse_StateMachine : INotifyInstanceUpdated<XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.HelloWorldResponseInstance>
    {
        /// <summary>
        ///
        /// </summary>
        IPublished_State Published_State { get ; }
        /// <summary>
        ///
        /// </summary>
        IGone_State Gone_State { get ; }
        
        /// <summary>
        ///Retrieves a snapshot of HelloWorldResponse state machines
        /// </summary>
        ///<param name="filter">This parameter is used to filter the snapshot request with a linq expression. The linq expression is applied to the public member</param>
        ///<param name="timeout">Snapshot timeout</param>
        /// <param name="chunkSize">Maximum number of items to be sent by the server in every snapshot chunk or <code>null</code> or 0 if unlimited</param>
        List<XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.HelloWorldResponseInstance> GetSnapshot(string filter = null, int timeout = 10000, int? chunkSize = null);
    
        ///<summary>
        ///Sends the XComponent.HelloWorld.UserObject.Kill event using a context. 
        ///</summary>
        ///<param name="context">Client Api context. Used to target a specific instance of state machine</param>
        ///<param name="transitionEvent"></param>
        ///<param name="onExceptionOccured">This event is triggered if an exception is raised. If this option is set, the exception will be catch. If this option is not set, the exception will be thrown.</param>
        ///
        void SendEvent(XComponent.Common.ApiContext.Context context, XComponent.HelloWorld.UserObject.Kill transitionEvent, Action<Exception> onExceptionOccured = null );
        ///<summary>
        ///Sends the XComponent.HelloWorld.UserObject.Ping event using a context. 
        ///</summary>
        ///<param name="context">Client Api context. Used to target a specific instance of state machine</param>
        ///<param name="transitionEvent"></param>
        ///<param name="onExceptionOccured">This event is triggered if an exception is raised. If this option is set, the exception will be catch. If this option is not set, the exception will be thrown.</param>
        ///
        void SendEvent(XComponent.Common.ApiContext.Context context, XComponent.HelloWorld.UserObject.Ping transitionEvent, Action<Exception> onExceptionOccured = null );
        ///<summary>
        ///Sends the XComponent.HelloWorld.UserObject.SayGoodbyeToAll event using a context. 
        ///</summary>
        ///<param name="context">Client Api context. Used to target a specific instance of state machine</param>
        ///<param name="transitionEvent"></param>
        ///<param name="onExceptionOccured">This event is triggered if an exception is raised. If this option is set, the exception will be catch. If this option is not set, the exception will be thrown.</param>
        ///
        void SendEvent(XComponent.Common.ApiContext.Context context, XComponent.HelloWorld.UserObject.SayGoodbyeToAll transitionEvent, Action<Exception> onExceptionOccured = null );
        ///<summary>
        ///Sends the XComponent.HelloWorld.UserObject.SayGoodbye event using a context. 
        ///</summary>
        ///<param name="context">Client Api context. Used to target a specific instance of state machine</param>
        ///<param name="transitionEvent"></param>
        ///<param name="onExceptionOccured">This event is triggered if an exception is raised. If this option is set, the exception will be catch. If this option is not set, the exception will be thrown.</param>
        ///
        void SendEvent(XComponent.Common.ApiContext.Context context, XComponent.HelloWorld.UserObject.SayGoodbye transitionEvent, Action<Exception> onExceptionOccured = null );
        

        ///<summary>
        ///Sends the XComponent.HelloWorld.UserObject.Kill event
        ///</summary>
        ///<param name="evt"></param>
        ///<param name="onExceptionOccured">This event is triggered if an exception is raised. If this option is set, the exception will be catch. If this option is not set, the exception will be thrown.</param>
        ///
        void SendEvent(XComponent.HelloWorld.UserObject.Kill evt, Action<Exception> onExceptionOccured = null );
        ///<summary>
        ///Sends the XComponent.HelloWorld.UserObject.Ping event
        ///</summary>
        ///<param name="evt"></param>
        ///<param name="onExceptionOccured">This event is triggered if an exception is raised. If this option is set, the exception will be catch. If this option is not set, the exception will be thrown.</param>
        ///
        void SendEvent(XComponent.HelloWorld.UserObject.Ping evt, Action<Exception> onExceptionOccured = null );
        ///<summary>
        ///Sends the XComponent.HelloWorld.UserObject.SayGoodbye event
        ///</summary>
        ///<param name="evt"></param>
        ///<param name="onExceptionOccured">This event is triggered if an exception is raised. If this option is set, the exception will be catch. If this option is not set, the exception will be thrown.</param>
        ///
        void SendEvent(XComponent.HelloWorld.UserObject.SayGoodbye evt, Action<Exception> onExceptionOccured = null );
        ///<summary>
        ///Sends the XComponent.HelloWorld.UserObject.SayGoodbyeToAll event
        ///</summary>
        ///<param name="evt"></param>
        ///<param name="onExceptionOccured">This event is triggered if an exception is raised. If this option is set, the exception will be catch. If this option is not set, the exception will be thrown.</param>
        ///
        void SendEvent(XComponent.HelloWorld.UserObject.SayGoodbyeToAll evt, Action<Exception> onExceptionOccured = null );
                
    }
}
