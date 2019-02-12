namespace XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.HelloWorld
{
    using System;
    using XComponent.Common.ApiContext;
    using XCClientAPICommon.Client;


    sealed public class EntryPoint_State : IEntryPoint_State
    {
        readonly private IHelloWorldV5ApiCommunication communicationLayer;

        public event System.Action<HelloWorldInstance> InstanceUpdated;
    
        public EntryPoint_State(IHelloWorldV5ApiCommunication communicationLayer)
        {
            this.communicationLayer = communicationLayer;
            this.communicationLayer.HelloWorld_HelloWorldInstanceUpdated += OnHelloWorldInstanceUpdated;
        }

        

        

        private void OnHelloWorldInstanceUpdated(HelloWorldInstance stdInstance)
        {
            if (stdInstance.StateCode == 0 && this.InstanceUpdated != null)
            {
                this.InstanceUpdated(stdInstance);
            }
        }
    }
}

