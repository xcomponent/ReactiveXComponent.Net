namespace XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using XComponent.Common.ApiContext;
    using XCClientAPICommon.Client;
    using XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.HelloWorldResponse;

    sealed public class HelloWorldResponse_StateMachine : IHelloWorldResponse_StateMachine
    {
        public enum HelloWorldResponseStateEnum
        {
		    Published = 1,
		    Gone = 0,
		    FatalError = -2147483648,
		    
        }

        readonly private IHelloWorldV5ApiCommunication communicationLayer;

        /// <summary>
        ///
        /// </summary>
        public IPublished_State Published_State { get ; set ; }
        /// <summary>
        ///
        /// </summary>
        public IGone_State Gone_State { get ; set ; }
        
        /// <summary>
        ///Retrieves a snapshot of HelloWorldResponse state machines
        /// </summary>
        ///<param name="filter">This parameter is used to filter the snapshot request with a linq expression. The linq expression is applied to the public member</param>
        ///<param name="timeout">Snapshot timeout</param>
        /// <param name="chunkSize">Maximum number of items to be sent by the server in every snapshot chunk or <code>null</code> or 0 if unlimited</param>
        public List<XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.HelloWorldResponseInstance> GetSnapshot(string filter = null, int timeout = 10000, int? chunkSize = null)
        {
            if (!string.IsNullOrWhiteSpace(filter))
            {
                try
                {
                    XComponent.Common.Event.ApiProxy.SnapshotOptions.CheckFilter<XComponent.HelloWorld.UserObject.HelloWorldResponse>(filter);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(ex.Message, "filter");
                }
            }

            var response = communicationLayer.GetSnapshot((int)HelloWorld_Component.StdEnum.HelloWorldResponse, (int)HelloWorld_Component.ComponentCode, filter, timeout, chunkSize);
            var result = new List<XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.HelloWorldResponseInstance>();
            if (response != null)
            {
                foreach (var snapshotItem in response.Items)
                {
                    result.Add(new XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.HelloWorldResponseInstance(snapshotItem.PublicMember, new Context(snapshotItem.StateMachineId, snapshotItem.WorkerId, snapshotItem.ComponentCode, snapshotItem.StateMachineCode, snapshotItem.StateCode, null), snapshotItem.StateCode, snapshotItem.StateMachineCode));
                }
            }
       
            return result;
        }

    
        public HelloWorldResponse_StateMachine(IHelloWorldV5ApiCommunication communicationLayer)
        {
            this.communicationLayer = communicationLayer;
            this.communicationLayer.HelloWorld_HelloWorldResponseInstanceUpdated += this.OnInstanceUpdated;
            this.Published_State = new Published_State(this.communicationLayer);
            this.Gone_State = new Gone_State(this.communicationLayer);
            
        }

        ///<summary>
        ///Sends the XComponent.HelloWorld.UserObject.Kill event using a context. 
        ///</summary>
        ///<param name="context">Client Api context. Used to target a specific instance of state machine</param>
        ///<param name="transitionEvent"></param>
        ///<param name="onExceptionOccured">This event is triggered if an exception is raised. If this option is set, the exception will be catch. If this option is not set, the exception will be thrown.</param>
        ///
        public void SendEvent(XComponent.Common.ApiContext.Context context, XComponent.HelloWorld.UserObject.Kill transitionEvent, Action<Exception> onExceptionOccured = null )
        {
            try
            {
                this.communicationLayer.SendToHelloWorld(context, transitionEvent );
            }
            catch(Exception ex)
            {
                if( onExceptionOccured != null )
                {
		            onExceptionOccured(ex);
                }
	            else
	            {
		            throw ex;
	            }
            }
        }

        ///<summary>
        ///Sends the XComponent.HelloWorld.UserObject.Ping event using a context. 
        ///</summary>
        ///<param name="context">Client Api context. Used to target a specific instance of state machine</param>
        ///<param name="transitionEvent"></param>
        ///<param name="onExceptionOccured">This event is triggered if an exception is raised. If this option is set, the exception will be catch. If this option is not set, the exception will be thrown.</param>
        ///
        public void SendEvent(XComponent.Common.ApiContext.Context context, XComponent.HelloWorld.UserObject.Ping transitionEvent, Action<Exception> onExceptionOccured = null )
        {
            try
            {
                this.communicationLayer.SendToHelloWorld(context, transitionEvent );
            }
            catch(Exception ex)
            {
                if( onExceptionOccured != null )
                {
		            onExceptionOccured(ex);
                }
	            else
	            {
		            throw ex;
	            }
            }
        }

        ///<summary>
        ///Sends the XComponent.HelloWorld.UserObject.SayGoodbyeToAll event using a context. 
        ///</summary>
        ///<param name="context">Client Api context. Used to target a specific instance of state machine</param>
        ///<param name="transitionEvent"></param>
        ///<param name="onExceptionOccured">This event is triggered if an exception is raised. If this option is set, the exception will be catch. If this option is not set, the exception will be thrown.</param>
        ///
        public void SendEvent(XComponent.Common.ApiContext.Context context, XComponent.HelloWorld.UserObject.SayGoodbyeToAll transitionEvent, Action<Exception> onExceptionOccured = null )
        {
            try
            {
                this.communicationLayer.SendToHelloWorld(context, transitionEvent );
            }
            catch(Exception ex)
            {
                if( onExceptionOccured != null )
                {
		            onExceptionOccured(ex);
                }
	            else
	            {
		            throw ex;
	            }
            }
        }

        ///<summary>
        ///Sends the XComponent.HelloWorld.UserObject.SayGoodbye event using a context. 
        ///</summary>
        ///<param name="context">Client Api context. Used to target a specific instance of state machine</param>
        ///<param name="transitionEvent"></param>
        ///<param name="onExceptionOccured">This event is triggered if an exception is raised. If this option is set, the exception will be catch. If this option is not set, the exception will be thrown.</param>
        ///
        public void SendEvent(XComponent.Common.ApiContext.Context context, XComponent.HelloWorld.UserObject.SayGoodbye transitionEvent, Action<Exception> onExceptionOccured = null )
        {
            try
            {
                this.communicationLayer.SendToHelloWorld(context, transitionEvent );
            }
            catch(Exception ex)
            {
                if( onExceptionOccured != null )
                {
		            onExceptionOccured(ex);
                }
	            else
	            {
		            throw ex;
	            }
            }
        }

        

        ///<summary>
        ///Sends the XComponent.HelloWorld.UserObject.Kill event
        ///</summary>
        ///<param name="evt"></param>
        ///<param name="onExceptionOccured">This event is triggered if an exception is raised. If this option is set, the exception will be catch. If this option is not set, the exception will be thrown.</param>
        ///
        public void SendEvent(XComponent.HelloWorld.UserObject.Kill evt, Action<Exception> onExceptionOccured = null )
        {

            try
            {
                this.communicationLayer.SendEventToHelloWorld(HelloWorld_Component.StdEnum.HelloWorldResponse, evt );
            }
            catch(Exception ex)
            {
                if( onExceptionOccured != null )
                {
		            onExceptionOccured(ex);
                }
	            else
	            {
		            throw ex;
	            }
            }
        }
        ///<summary>
        ///Sends the XComponent.HelloWorld.UserObject.Ping event
        ///</summary>
        ///<param name="evt"></param>
        ///<param name="onExceptionOccured">This event is triggered if an exception is raised. If this option is set, the exception will be catch. If this option is not set, the exception will be thrown.</param>
        ///
        public void SendEvent(XComponent.HelloWorld.UserObject.Ping evt, Action<Exception> onExceptionOccured = null )
        {

            try
            {
                this.communicationLayer.SendEventToHelloWorld(HelloWorld_Component.StdEnum.HelloWorldResponse, evt );
            }
            catch(Exception ex)
            {
                if( onExceptionOccured != null )
                {
		            onExceptionOccured(ex);
                }
	            else
	            {
		            throw ex;
	            }
            }
        }
        ///<summary>
        ///Sends the XComponent.HelloWorld.UserObject.SayGoodbye event
        ///</summary>
        ///<param name="evt"></param>
        ///<param name="onExceptionOccured">This event is triggered if an exception is raised. If this option is set, the exception will be catch. If this option is not set, the exception will be thrown.</param>
        ///
        public void SendEvent(XComponent.HelloWorld.UserObject.SayGoodbye evt, Action<Exception> onExceptionOccured = null )
        {

            try
            {
                this.communicationLayer.SendEventToHelloWorld(HelloWorld_Component.StdEnum.HelloWorldResponse, evt );
            }
            catch(Exception ex)
            {
                if( onExceptionOccured != null )
                {
		            onExceptionOccured(ex);
                }
	            else
	            {
		            throw ex;
	            }
            }
        }
        ///<summary>
        ///Sends the XComponent.HelloWorld.UserObject.SayGoodbyeToAll event
        ///</summary>
        ///<param name="evt"></param>
        ///<param name="onExceptionOccured">This event is triggered if an exception is raised. If this option is set, the exception will be catch. If this option is not set, the exception will be thrown.</param>
        ///
        public void SendEvent(XComponent.HelloWorld.UserObject.SayGoodbyeToAll evt, Action<Exception> onExceptionOccured = null )
        {

            try
            {
                this.communicationLayer.SendEventToHelloWorld(HelloWorld_Component.StdEnum.HelloWorldResponse, evt );
            }
            catch(Exception ex)
            {
                if( onExceptionOccured != null )
                {
		            onExceptionOccured(ex);
                }
	            else
	            {
		            throw ex;
	            }
            }
        }
        


        ///<summary>
        ///This event is triggered when the HelloWorldResponse state machine is updated
        ///</summary>
        public event System.Action<XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.HelloWorldResponseInstance> InstanceUpdated;

        private void OnInstanceUpdated(XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.HelloWorldResponseInstance instance)
        {
            if ((this.InstanceUpdated != null))
            {
                this.InstanceUpdated(instance);
            }
        }
    }
}
