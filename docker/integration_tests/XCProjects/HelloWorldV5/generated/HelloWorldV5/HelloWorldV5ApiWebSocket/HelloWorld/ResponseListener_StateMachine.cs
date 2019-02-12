namespace XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using XComponent.Common.ApiContext;
    using XCClientAPICommon.Client;
    using XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.ResponseListener;

    sealed public class ResponseListener_StateMachine : IResponseListener_StateMachine
    {
        public enum ResponseListenerStateEnum
        {
		    Up = 0,
		    FatalError = -2147483648,
		    
        }

        readonly private IHelloWorldV5ApiCommunication communicationLayer;

        /// <summary>
        ///
        /// </summary>
        public IUp_State Up_State { get ; set ; }
        
        /// <summary>
        ///Retrieves a snapshot of ResponseListener state machines
        /// </summary>
        ///<param name="filter">This parameter is used to filter the snapshot request with a linq expression. The linq expression is applied to the public member</param>
        ///<param name="timeout">Snapshot timeout</param>
        /// <param name="chunkSize">Maximum number of items to be sent by the server in every snapshot chunk or <code>null</code> or 0 if unlimited</param>
        public List<XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.ResponseListenerInstance> GetSnapshot(string filter = null, int timeout = 10000, int? chunkSize = null)
        {
            if (!string.IsNullOrWhiteSpace(filter))
            {
                try
                {
                    XComponent.Common.Event.ApiProxy.SnapshotOptions.CheckFilter<XComponent.HelloWorld.UserObject.ResponseListener>(filter);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(ex.Message, "filter");
                }
            }

            var response = communicationLayer.GetSnapshot((int)HelloWorld_Component.StdEnum.ResponseListener, (int)HelloWorld_Component.ComponentCode, filter, timeout, chunkSize);
            var result = new List<XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.ResponseListenerInstance>();
            if (response != null)
            {
                foreach (var snapshotItem in response.Items)
                {
                    result.Add(new XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.ResponseListenerInstance(snapshotItem.PublicMember, new Context(snapshotItem.StateMachineId, snapshotItem.WorkerId, snapshotItem.ComponentCode, snapshotItem.StateMachineCode, snapshotItem.StateCode, null), snapshotItem.StateCode, snapshotItem.StateMachineCode));
                }
            }
       
            return result;
        }

    
        public ResponseListener_StateMachine(IHelloWorldV5ApiCommunication communicationLayer)
        {
            this.communicationLayer = communicationLayer;
            this.communicationLayer.HelloWorld_ResponseListenerInstanceUpdated += this.OnInstanceUpdated;
            this.Up_State = new Up_State(this.communicationLayer);
            
        }

        

        


        ///<summary>
        ///This event is triggered when the ResponseListener state machine is updated
        ///</summary>
        public event System.Action<XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.ResponseListenerInstance> InstanceUpdated;

        private void OnInstanceUpdated(XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.ResponseListenerInstance instance)
        {
            if ((this.InstanceUpdated != null))
            {
                this.InstanceUpdated(instance);
            }
        }
    }
}
