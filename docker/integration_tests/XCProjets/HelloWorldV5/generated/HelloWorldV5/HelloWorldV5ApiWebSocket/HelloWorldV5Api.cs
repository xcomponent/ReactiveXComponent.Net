using XCClientAPICommon.Client;
using XComponent.Communication.ClientApi;
using XComponent.Shared.Api.Communication;

namespace XComponent.HelloWorldV5.HelloWorldV5Api
{
    using System;
    using System.Reflection;
    using XComponent.Common.ApiContext;

    /// <summary>
    /// 
    /// </summary>
    public class HelloWorldV5Api : XCClientAPICommon.Client.AbstractClientApi
    {

        /// <summary>
        ///This property is used to communicate with HelloWorld
        /// <para></para>
        /// </summary>
        public HelloWorld_Component HelloWorld_Component { get ; protected set ; }
        

        public HelloWorldV5Api()
        {
            this.communicationLayer = new HelloWorldV5ApiWebSocketCommunication();
            this.communicationLayer.ClientApi = this;
            this.communicationLayer.CloseConnectionOnDispose = true;
            this.HelloWorld_Component = new HelloWorld_Component(this.communicationLayer);
            #pragma warning disable 618 
                Uri clientApiUri = new Uri(Assembly.GetExecutingAssembly().CodeBase, true);
            #pragma warning restore 618 
            string clientApiFullPath = clientApiUri.AbsolutePath;
            if (clientApiUri.IsUnc)
            {
                clientApiFullPath = clientApiUri.LocalPath;
            }
            base.InitClientApi(clientApiFullPath, this.communicationLayer.CommunicationType);
        }

        public HelloWorldV5Api(int timeOut):this()
        {
            this.communicationLayer.TimeOut = timeOut;
        }

        void communicationLayer_Disconnected()
        {
            NotifyDisconnected();
        }

        /// <summary>
        ///The RabbitMq connection is shared by all client Apis instances. 
        /// <para>When a client Api is disposed,  the RabbitMq thread is destroyed for all instances.</para>
        /// <para>If you have several instances of your api set this property to false or use the helper ApiInstancesManager ()</para>
        /// </summary>
        public bool CloseConnectionOnDispose
        {
            get
            {
                return this.communicationLayer.CloseConnectionOnDispose;
            }
            set
            {
                this.communicationLayer.CloseConnectionOnDispose = value;
            }
        }

        public string SessionData
        {
            get
            {
                return this.communicationLayer.SessionData;
            }
            set
            {
                this.communicationLayer.SessionData = value;
            }
        }
        /// <summary>
        ///Use this property to set a specific private topic.
        /// <para>Warning: The topic should be set before client Api initialization.</para>
        /// <para>This topic is used to send private events.</para>
        /// </summary>
        public string PrivateCommunicationIdentifier
        {
            get
            {
                return this.communicationLayer.PrivateTopic;
            }
            set
            {
            this.communicationLayer.PrivateTopic = value;
            }
        }

        public void AddPrivateGroupTopic(string groupTopic)
        {
            this.communicationLayer.AddPrivateGroupTopic(groupTopic);
        }


        public void RemovePrivateGroupTopic(string groupTopic)
        {
            this.communicationLayer.RemovePrivateGroupTopic(groupTopic);
        }

        public void AddStreamsHookFactory(IClientApiStreamsHookFactory iStreamsHookFactory)
        {
            this.communicationLayer.ClientApiStreamsHookFactory = iStreamsHookFactory;
        }

        public IClientApiStreamsHookFactory GetStreamsHookFactory()
        {
            return this.communicationLayer.ClientApiStreamsHookFactory;
        }
        /// <summary>
        ///Set TimeOut (in mms) for retrieving EntryPoint
        /// <para>Default value is: 10000</para>
        /// </summary>
        override public int TimeOut
        {
            get
            {
                return this.communicationLayer.TimeOut;
            }
            set
            {
                this.communicationLayer.TimeOut = value;
            }
        }

        /// <summary>
        ///Returns the type of communication
        /// <para>Possible values: BUS, IN_MEMORY, WEB_SOCKET</para>
        /// </summary>
        override public CommunicationType CommunicationType
        {
            get
            {
                return this.communicationLayer.CommunicationType;
            }
        }

        override public void Dump()
        {
            this.communicationLayer.Dump();
        }

        public override string DefaultXcApiFileName 
        { 
            get
            {
                if( CommunicationType == CommunicationType.WEB_SOCKET)
                {
                    return "WebSocket_" + "HelloWorldV5Api.xcApi";  
                }
                return "HelloWorldV5Api.xcApi";
            } 
        }

        private IHelloWorldV5ApiCommunication communicationLayer;

        public override bool Init(out InitReport report, IClientApiConfigurationOverride configurationOverride = null)
        {
            return InerInit(DefaultXcApiFileName, out report, configurationOverride);
        }
        protected override bool InnerInit(out XCClientAPICommon.Client.InitReport report, XCClientAPICommon.Client.IClientApiConfigurationOverride configurationOverride = null)
        {
            return this.communicationLayer.Init("DeploymentConfiguration.xml", out report, configurationOverride);
        }

        protected override bool InnerInit(string configFile, out XCClientAPICommon.Client.InitReport report, XCClientAPICommon.Client.IClientApiConfigurationOverride configurationOverride = null)
        {
            return this.communicationLayer.Init(configFile, out report, configurationOverride);
        }

        /// <summary>
        ///Dispose the Client Api and the communication layer
        /// </summary>
        public override void Dispose()
        {
            this.communicationLayer.ClientApi = null;
            base.Dispose();
            this.communicationLayer.Dispose();
        }
    }
}

