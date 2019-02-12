namespace XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.HelloWorldResponse
{
    using System;
    using XComponent.Common.ApiContext;
    using XCClientAPICommon.Client;


    sealed public class Published_State : IPublished_State
    {
        readonly private IHelloWorldV5ApiCommunication communicationLayer;

        public event System.Action<HelloWorldResponseInstance> InstanceUpdated;
    
        public Published_State(IHelloWorldV5ApiCommunication communicationLayer)
        {
            this.communicationLayer = communicationLayer;
            this.communicationLayer.HelloWorld_HelloWorldResponseInstanceUpdated += OnHelloWorldResponseInstanceUpdated;
        }

        public void Kill(XComponent.Common.ApiContext.Context context, XComponent.HelloWorld.UserObject.Kill transitionEvent, Action<Exception> onExceptionOccured = null )
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
        public void Ping(XComponent.Common.ApiContext.Context context, XComponent.HelloWorld.UserObject.Ping transitionEvent, Action<Exception> onExceptionOccured = null )
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
        public void SayGoodbyeToAll(XComponent.Common.ApiContext.Context context, XComponent.HelloWorld.UserObject.SayGoodbyeToAll transitionEvent, Action<Exception> onExceptionOccured = null )
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
        public void SayGoodbye(XComponent.Common.ApiContext.Context context, XComponent.HelloWorld.UserObject.SayGoodbye transitionEvent, Action<Exception> onExceptionOccured = null )
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
        

        public void Kill(XComponent.Common.ApiContext.Context context, Action<Exception> onExceptionOccured = null )
        {
            try
            {
                 this.communicationLayer.SendToHelloWorld(context, default(XComponent.HelloWorld.UserObject.Kill) );
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
        public void Ping(XComponent.Common.ApiContext.Context context, Action<Exception> onExceptionOccured = null )
        {
            try
            {
                 this.communicationLayer.SendToHelloWorld(context, default(XComponent.HelloWorld.UserObject.Ping) );
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
        public void SayGoodbyeToAll(XComponent.Common.ApiContext.Context context, Action<Exception> onExceptionOccured = null )
        {
            try
            {
                 this.communicationLayer.SendToHelloWorld(context, default(XComponent.HelloWorld.UserObject.SayGoodbyeToAll) );
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
        public void SayGoodbye(XComponent.Common.ApiContext.Context context, Action<Exception> onExceptionOccured = null )
        {
            try
            {
                 this.communicationLayer.SendToHelloWorld(context, default(XComponent.HelloWorld.UserObject.SayGoodbye) );
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
        

        private void OnHelloWorldResponseInstanceUpdated(HelloWorldResponseInstance stdInstance)
        {
            if (stdInstance.StateCode == 1 && this.InstanceUpdated != null)
            {
                this.InstanceUpdated(stdInstance);
            }
        }
    }
}

