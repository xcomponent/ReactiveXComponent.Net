namespace XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using XComponent.Common.ApiContext;
    using XCClientAPICommon.Client;
    using XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.HelloWorld;

    sealed public class HelloWorld_StateMachine : IHelloWorld_StateMachine
    {
        public enum HelloWorldStateEnum
        {
		    EntryPoint = 0,
		    Listening = 1,
		    FatalError = -2147483648,
		    
        }

        readonly private IHelloWorldV5ApiCommunication communicationLayer;

        /// <summary>
        ///
        /// </summary>
        public IEntryPoint_State EntryPoint_State { get ; set ; }
        /// <summary>
        ///
        /// </summary>
        public IListening_State Listening_State { get ; set ; }
        
        /// <summary>
        ///Retrieves a snapshot of HelloWorld state machines
        /// </summary>
        ///<param name="filter">This parameter is used to filter the snapshot request with a linq expression. The linq expression is applied to the public member</param>
        ///<param name="timeout">Snapshot timeout</param>
        /// <param name="chunkSize">Maximum number of items to be sent by the server in every snapshot chunk or <code>null</code> or 0 if unlimited</param>
        public List<XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.HelloWorldInstance> GetSnapshot(string filter = null, int timeout = 10000, int? chunkSize = null)
        {
            if (!string.IsNullOrWhiteSpace(filter))
            {
                try
                {
                    XComponent.Common.Event.ApiProxy.SnapshotOptions.CheckFilter<System.Object>(filter);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(ex.Message, "filter");
                }
            }

            var response = communicationLayer.GetSnapshot((int)HelloWorld_Component.StdEnum.HelloWorld, (int)HelloWorld_Component.ComponentCode, filter, timeout, chunkSize);
            var result = new List<XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.HelloWorldInstance>();
            if (response != null)
            {
                foreach (var snapshotItem in response.Items)
                {
                    result.Add(new XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.HelloWorldInstance(snapshotItem.PublicMember, new Context(snapshotItem.StateMachineId, snapshotItem.WorkerId, snapshotItem.ComponentCode, snapshotItem.StateMachineCode, snapshotItem.StateCode, null), snapshotItem.StateCode, snapshotItem.StateMachineCode));
                }
            }
       
            return result;
        }

    
        public HelloWorld_StateMachine(IHelloWorldV5ApiCommunication communicationLayer)
        {
            this.communicationLayer = communicationLayer;
            this.communicationLayer.HelloWorld_HelloWorldInstanceUpdated += this.OnInstanceUpdated;
            this.EntryPoint_State = new EntryPoint_State(this.communicationLayer);
            this.Listening_State = new Listening_State(this.communicationLayer);
            
        }

        ///<summary>
        ///Sends the XComponent.HelloWorld.UserObject.CreateListener event using a context. 
        ///</summary>
        ///<param name="context">Client Api context. Used to target a specific instance of state machine</param>
        ///<param name="transitionEvent"></param>
        ///<param name="onExceptionOccured">This event is triggered if an exception is raised. If this option is set, the exception will be catch. If this option is not set, the exception will be thrown.</param>
        ///<param name="visibility">If parameter is set to Visibility.Private, the event will be sent with a private topic.</param>
        public void SendEvent(XComponent.Common.ApiContext.Context context, XComponent.HelloWorld.UserObject.CreateListener transitionEvent, Action<Exception> onExceptionOccured = null , Visibility visibility = Visibility.Public, string topic = null)
        {
            try
            {
                this.communicationLayer.SendToHelloWorld(context, transitionEvent , visibility, topic);
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
        ///Sends the XComponent.HelloWorld.UserObject.SayHello event using a context. 
        ///</summary>
        ///<param name="context">Client Api context. Used to target a specific instance of state machine</param>
        ///<param name="transitionEvent"></param>
        ///<param name="onExceptionOccured">This event is triggered if an exception is raised. If this option is set, the exception will be catch. If this option is not set, the exception will be thrown.</param>
        ///<param name="visibility">If parameter is set to Visibility.Private, the event will be sent with a private topic.</param>
        public void SendEvent(XComponent.Common.ApiContext.Context context, XComponent.HelloWorld.UserObject.SayHello transitionEvent, Action<Exception> onExceptionOccured = null , Visibility visibility = Visibility.Public, string topic = null)
        {
            try
            {
                this.communicationLayer.SendToHelloWorld(context, transitionEvent , visibility, topic);
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
        ///Sends the XComponent.HelloWorld.UserObject.CreateListener event
        ///</summary>
        ///<param name="evt"></param>
        ///<param name="onExceptionOccured">This event is triggered if an exception is raised. If this option is set, the exception will be catch. If this option is not set, the exception will be thrown.</param>
        ///<param name="visibility">If parameter is set to Visibility.Private, the event will be sent with a private topic.</param>
        public void SendEvent(XComponent.HelloWorld.UserObject.CreateListener evt, Action<Exception> onExceptionOccured = null , Visibility visibility = Visibility.Public, string topic = null)
        {

            try
            {
                this.communicationLayer.SendEventToHelloWorld(HelloWorld_Component.StdEnum.HelloWorld, evt , visibility, topic);
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
        ///Sends the XComponent.HelloWorld.UserObject.SayHello event
        ///</summary>
        ///<param name="evt"></param>
        ///<param name="onExceptionOccured">This event is triggered if an exception is raised. If this option is set, the exception will be catch. If this option is not set, the exception will be thrown.</param>
        ///<param name="visibility">If parameter is set to Visibility.Private, the event will be sent with a private topic.</param>
        public void SendEvent(XComponent.HelloWorld.UserObject.SayHello evt, Action<Exception> onExceptionOccured = null , Visibility visibility = Visibility.Public, string topic = null)
        {

            try
            {
                this.communicationLayer.SendEventToHelloWorld(HelloWorld_Component.StdEnum.HelloWorld, evt , visibility, topic);
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
        ///This event is triggered when the HelloWorld state machine is updated
        ///</summary>
        public event System.Action<XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.HelloWorldInstance> InstanceUpdated;

        private void OnInstanceUpdated(XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.HelloWorldInstance instance)
        {
            if ((this.InstanceUpdated != null))
            {
                this.InstanceUpdated(instance);
            }
        }
    }
}
