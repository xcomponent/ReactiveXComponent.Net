namespace XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.HelloWorld
{
    using System;
    using XComponent.Common.ApiContext;
    using XCClientAPICommon.Client;


    sealed public class Listening_State : IListening_State
    {
        readonly private IHelloWorldV5ApiCommunication communicationLayer;

        public event System.Action<HelloWorldInstance> InstanceUpdated;
    
        public Listening_State(IHelloWorldV5ApiCommunication communicationLayer)
        {
            this.communicationLayer = communicationLayer;
            this.communicationLayer.HelloWorld_HelloWorldInstanceUpdated += OnHelloWorldInstanceUpdated;
        }

        public void CreateListener(XComponent.Common.ApiContext.Context context, XComponent.HelloWorld.UserObject.CreateListener transitionEvent, Action<Exception> onExceptionOccured = null , Visibility visibility = Visibility.Public)
        {
            try
            {
                 this.communicationLayer.SendToHelloWorld(context, transitionEvent , visibility);
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
        public void SayHello(XComponent.Common.ApiContext.Context context, XComponent.HelloWorld.UserObject.SayHello transitionEvent, Action<Exception> onExceptionOccured = null , Visibility visibility = Visibility.Public)
        {
            try
            {
                 this.communicationLayer.SendToHelloWorld(context, transitionEvent , visibility);
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
        

        public void CreateListener(XComponent.Common.ApiContext.Context context, Action<Exception> onExceptionOccured = null , Visibility visibility = Visibility.Public)
        {
            try
            {
                 this.communicationLayer.SendToHelloWorld(context, default(XComponent.HelloWorld.UserObject.CreateListener) , visibility);
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
        public void SayHello(XComponent.Common.ApiContext.Context context, Action<Exception> onExceptionOccured = null , Visibility visibility = Visibility.Public)
        {
            try
            {
                 this.communicationLayer.SendToHelloWorld(context, default(XComponent.HelloWorld.UserObject.SayHello) , visibility);
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
        

        private void OnHelloWorldInstanceUpdated(HelloWorldInstance stdInstance)
        {
            if (stdInstance.StateCode == 1 && this.InstanceUpdated != null)
            {
                this.InstanceUpdated(stdInstance);
            }
        }
    }
}

