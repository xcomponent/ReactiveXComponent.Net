using System.ComponentModel;
using XComponent.Communication.Serialization;
using XComponent.Communication.Serialization.Json;
using System.Text;
using System.IO;

namespace XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld
{
    using System;
    using XComponent.Common.ApiContext;


    [System.Serializable()]
    sealed public class HelloWorldResponseInstance : AbstractInstance
    {
		private static volatile ISerializer _serializer;
		private static readonly object SerializerLock = new object();
		
        private const string s_StateMachineName = "HelloWorldResponse";
        
        [JsonConverter(typeof(InstanceCreatorJsonConverter<XComponent.HelloWorld.UserObject.HelloWorldResponse>))]
        private object InnerPublicMember { get; set; }

        private XComponent.HelloWorld.UserObject.HelloWorldResponse publicMember;

        public XComponent.HelloWorld.UserObject.HelloWorldResponse PublicMember
        {
            get
            {
                if (((this.publicMember == null) && (this.InnerPublicMember != null)))
                {
                    this.publicMember = this.InnerPublicMember as XComponent.HelloWorld.UserObject.HelloWorldResponse;
                    if (this.publicMember == null)
                    {
                        // try to deserialize a json object - this is required for public members deserialized via SnapshotItem
                        var jObjectWrapper = this.InnerPublicMember as JObjectWrapper;
                        if (jObjectWrapper != null)
                        {
                            this.publicMember = jObjectWrapper.ToObject<XComponent.HelloWorld.UserObject.HelloWorldResponse>();
                        }
                        else
                        {
                            
                            var deserializableObject = this.InnerPublicMember;
                            if (deserializableObject != null)
                            {
								var stringyfied = deserializableObject.ToString();
								var serializer =
									XComponent.Communication.Serialization.SerializerFactory.Instance.CreateSerializer();
								// convert string to stream
								byte[] byteArray = Encoding.UTF8.GetBytes(stringyfied);
								using (MemoryStream stream = new MemoryStream(byteArray))
								{
									this.publicMember = (XComponent.HelloWorld.UserObject.HelloWorldResponse) serializer.Deserialize(stream, "XComponent.HelloWorld.UserObject.HelloWorldResponse");
								}
                            }
                         }
                    }
                }
                return this.publicMember;
            }
        }
		
		private ISerializer Serializer 
		{
			get 
			{
				if (_serializer == null) 
				{
					lock (SerializerLock)
					{
						if (_serializer == null)
						{
							_serializer = SerializerFactory.Instance.CreateSerializer();
						}
					}
				}
				
				return _serializer;
			}
		}

        public HelloWorldResponseInstance()
        {
        }

        public HelloWorldResponseInstance(object publicMember, Context context, int stateCode, int stateMachineCode) :
            base(stateCode, stateMachineCode)
        {
            this.InnerPublicMember = publicMember;
            this.context = context;
        }

        protected override string ConvertStateCodeToString(int stateCode)
        {
            return ((HelloWorldResponse_StateMachine.HelloWorldResponseStateEnum)stateCode).ToString();
        }

        protected override string ConvertStateMachineCodeToString(int stateMachineCode)
        {
            return s_StateMachineName;
        }

        readonly private Context context;

        public Context Context
        {
            get
            {
                return this.context;
            }
        }

        public string ErrorMessage
        {
            get
            {
                return Context.ErrorMessage;
            }
        }
    }
}
