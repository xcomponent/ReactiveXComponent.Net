namespace XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.HelloWorld
{
    using System;
    using XComponent.Common.ApiContext;
    using XCClientAPICommon.Client;

    public interface IListening_State : INotifyInstanceUpdated<HelloWorldInstance>
    {            
        ///<summary>
        ///Sends the XComponent.HelloWorld.UserObject.CreateListener event using a context. 
        ///</summary>
        ///<param name="context">Client Api context. Used to target a specific instance of state machine</param>
        ///<param name="transitionEvent"></param>
        ///<param name="onExceptionOccured">This event is triggered if an exception is raised. If this option is set, the exception will be catch. If this option is not set, the exception will be thrown.</param>
        ///<param name="visibility">If parameter is set to Visibility.Private, the event will be sent with a private topic.</param>
        void CreateListener(XComponent.Common.ApiContext.Context context, XComponent.HelloWorld.UserObject.CreateListener transitionEvent, Action<Exception> onExceptionOccured = null , Visibility visibility = Visibility.Public);
        ///<summary>
        ///Sends the XComponent.HelloWorld.UserObject.SayHello event using a context. 
        ///</summary>
        ///<param name="context">Client Api context. Used to target a specific instance of state machine</param>
        ///<param name="transitionEvent"></param>
        ///<param name="onExceptionOccured">This event is triggered if an exception is raised. If this option is set, the exception will be catch. If this option is not set, the exception will be thrown.</param>
        ///<param name="visibility">If parameter is set to Visibility.Private, the event will be sent with a private topic.</param>
        void SayHello(XComponent.Common.ApiContext.Context context, XComponent.HelloWorld.UserObject.SayHello transitionEvent, Action<Exception> onExceptionOccured = null , Visibility visibility = Visibility.Public);
        

        ///<summary>
        ///Triggers the CreateListener transition using a context. 
        /// <para></para>
        ///</summary>
        ///<param name="context">Client Api context. Used to target a specific instance of state machine</param>
        ///<param name="onExceptionOccured">This event is triggered if an exception is raised. If this option is set, the exception will be catch. If this option is not set, the exception will be thrown.</param>
        ///<param name="visibility">If parameter is set to Visibility.Private, the event will be sent with a private topic.</param>
        void CreateListener(XComponent.Common.ApiContext.Context context, Action<Exception> onExceptionOccured = null , Visibility visibility = Visibility.Public);
        ///<summary>
        ///Triggers the SayHello transition using a context. 
        /// <para></para>
        ///</summary>
        ///<param name="context">Client Api context. Used to target a specific instance of state machine</param>
        ///<param name="onExceptionOccured">This event is triggered if an exception is raised. If this option is set, the exception will be catch. If this option is not set, the exception will be thrown.</param>
        ///<param name="visibility">If parameter is set to Visibility.Private, the event will be sent with a private topic.</param>
        void SayHello(XComponent.Common.ApiContext.Context context, Action<Exception> onExceptionOccured = null , Visibility visibility = Visibility.Public);
        
    }
}