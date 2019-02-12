namespace XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.ResponseListener
{
    using System;
    using XComponent.Common.ApiContext;
    using XCClientAPICommon.Client;


    sealed public class Up_State : IUp_State
    {
        readonly private IHelloWorldV5ApiCommunication communicationLayer;

        public event System.Action<ResponseListenerInstance> InstanceUpdated;
    
        public Up_State(IHelloWorldV5ApiCommunication communicationLayer)
        {
            this.communicationLayer = communicationLayer;
            this.communicationLayer.HelloWorld_ResponseListenerInstanceUpdated += OnResponseListenerInstanceUpdated;
        }

        

        

        private void OnResponseListenerInstanceUpdated(ResponseListenerInstance stdInstance)
        {
            if (stdInstance.StateCode == 0 && this.InstanceUpdated != null)
            {
                this.InstanceUpdated(stdInstance);
            }
        }
    }
}

