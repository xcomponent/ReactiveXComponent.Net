namespace XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.HelloWorldResponse
{
    using System;
    using XComponent.Common.ApiContext;
    using XCClientAPICommon.Client;


    sealed public class Gone_State : IGone_State
    {
        readonly private IHelloWorldV5ApiCommunication communicationLayer;

        public event System.Action<HelloWorldResponseInstance> InstanceUpdated;
    
        public Gone_State(IHelloWorldV5ApiCommunication communicationLayer)
        {
            this.communicationLayer = communicationLayer;
            this.communicationLayer.HelloWorld_HelloWorldResponseInstanceUpdated += OnHelloWorldResponseInstanceUpdated;
        }

        

        

        private void OnHelloWorldResponseInstanceUpdated(HelloWorldResponseInstance stdInstance)
        {
            if (stdInstance.StateCode == 0 && this.InstanceUpdated != null)
            {
                this.InstanceUpdated(stdInstance);
            }
        }
    }
}

