namespace XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld
{
    using System;
    using System.Collections.Generic;
    using XComponent.Common.ApiContext;
    using XCClientAPICommon.Client;
    using XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.ResponseListener;

    public interface IResponseListener_StateMachine : INotifyInstanceUpdated<XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.ResponseListenerInstance>
    {
        /// <summary>
        ///
        /// </summary>
        IUp_State Up_State { get ; }
        
        /// <summary>
        ///Retrieves a snapshot of ResponseListener state machines
        /// </summary>
        ///<param name="filter">This parameter is used to filter the snapshot request with a linq expression. The linq expression is applied to the public member</param>
        ///<param name="timeout">Snapshot timeout</param>
        /// <param name="chunkSize">Maximum number of items to be sent by the server in every snapshot chunk or <code>null</code> or 0 if unlimited</param>
        List<XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.ResponseListenerInstance> GetSnapshot(string filter = null, int timeout = 10000, int? chunkSize = null);
    
        

                
    }
}
