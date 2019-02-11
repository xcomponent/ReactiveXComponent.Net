namespace XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.HelloWorldResponse
{
    using System;
    using XComponent.Common.ApiContext;
    using XCClientAPICommon.Client;

    public interface IPublished_State : INotifyInstanceUpdated<HelloWorldResponseInstance>
    {            
        ///<summary>
        ///Sends the XComponent.HelloWorld.UserObject.Kill event using a context. 
        ///</summary>
        ///<param name="context">Client Api context. Used to target a specific instance of state machine</param>
        ///<param name="transitionEvent"></param>
        ///<param name="onExceptionOccured">This event is triggered if an exception is raised. If this option is set, the exception will be catch. If this option is not set, the exception will be thrown.</param>
        ///
        void Kill(XComponent.Common.ApiContext.Context context, XComponent.HelloWorld.UserObject.Kill transitionEvent, Action<Exception> onExceptionOccured = null );
        ///<summary>
        ///Sends the XComponent.HelloWorld.UserObject.Ping event using a context. 
        ///</summary>
        ///<param name="context">Client Api context. Used to target a specific instance of state machine</param>
        ///<param name="transitionEvent"></param>
        ///<param name="onExceptionOccured">This event is triggered if an exception is raised. If this option is set, the exception will be catch. If this option is not set, the exception will be thrown.</param>
        ///
        void Ping(XComponent.Common.ApiContext.Context context, XComponent.HelloWorld.UserObject.Ping transitionEvent, Action<Exception> onExceptionOccured = null );
        ///<summary>
        ///Sends the XComponent.HelloWorld.UserObject.SayGoodbyeToAll event using a context. 
        ///</summary>
        ///<param name="context">Client Api context. Used to target a specific instance of state machine</param>
        ///<param name="transitionEvent"></param>
        ///<param name="onExceptionOccured">This event is triggered if an exception is raised. If this option is set, the exception will be catch. If this option is not set, the exception will be thrown.</param>
        ///
        void SayGoodbyeToAll(XComponent.Common.ApiContext.Context context, XComponent.HelloWorld.UserObject.SayGoodbyeToAll transitionEvent, Action<Exception> onExceptionOccured = null );
        ///<summary>
        ///Sends the XComponent.HelloWorld.UserObject.SayGoodbye event using a context. 
        ///</summary>
        ///<param name="context">Client Api context. Used to target a specific instance of state machine</param>
        ///<param name="transitionEvent"></param>
        ///<param name="onExceptionOccured">This event is triggered if an exception is raised. If this option is set, the exception will be catch. If this option is not set, the exception will be thrown.</param>
        ///
        void SayGoodbye(XComponent.Common.ApiContext.Context context, XComponent.HelloWorld.UserObject.SayGoodbye transitionEvent, Action<Exception> onExceptionOccured = null );
        

        ///<summary>
        ///Triggers the Kill transition using a context. 
        /// <para></para>
        ///</summary>
        ///<param name="context">Client Api context. Used to target a specific instance of state machine</param>
        ///<param name="onExceptionOccured">This event is triggered if an exception is raised. If this option is set, the exception will be catch. If this option is not set, the exception will be thrown.</param>
        ///
        void Kill(XComponent.Common.ApiContext.Context context, Action<Exception> onExceptionOccured = null );
        ///<summary>
        ///Triggers the Ping transition using a context. 
        /// <para></para>
        ///</summary>
        ///<param name="context">Client Api context. Used to target a specific instance of state machine</param>
        ///<param name="onExceptionOccured">This event is triggered if an exception is raised. If this option is set, the exception will be catch. If this option is not set, the exception will be thrown.</param>
        ///
        void Ping(XComponent.Common.ApiContext.Context context, Action<Exception> onExceptionOccured = null );
        ///<summary>
        ///Triggers the SayGoodbyeToAll transition using a context. 
        /// <para></para>
        ///</summary>
        ///<param name="context">Client Api context. Used to target a specific instance of state machine</param>
        ///<param name="onExceptionOccured">This event is triggered if an exception is raised. If this option is set, the exception will be catch. If this option is not set, the exception will be thrown.</param>
        ///
        void SayGoodbyeToAll(XComponent.Common.ApiContext.Context context, Action<Exception> onExceptionOccured = null );
        ///<summary>
        ///Triggers the SayGoodbye transition using a context. 
        /// <para></para>
        ///</summary>
        ///<param name="context">Client Api context. Used to target a specific instance of state machine</param>
        ///<param name="onExceptionOccured">This event is triggered if an exception is raised. If this option is set, the exception will be catch. If this option is not set, the exception will be thrown.</param>
        ///
        void SayGoodbye(XComponent.Common.ApiContext.Context context, Action<Exception> onExceptionOccured = null );
        
    }
}