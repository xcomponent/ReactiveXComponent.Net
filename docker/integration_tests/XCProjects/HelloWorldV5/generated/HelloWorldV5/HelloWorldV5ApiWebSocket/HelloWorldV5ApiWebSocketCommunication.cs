using XCClientAPICommon.Client;
using XComponent.Communication.ClientApi;
using XComponent.Shared.Api;

namespace XComponent.HelloWorldV5.HelloWorldV5Api
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Collections.Generic;
    using XComponent.Common.Processing;
    using XComponent.Common.ApiContext;
    using XComponent.Communication;
    using XComponent.Communication.Serialization;
    using XCClientAPICommon.Client;
    using System.Timers;
    using XComponent.Shared.Api.Communication.ClientApi;

    public class HelloWorldV5ApiWebSocketCommunication : AbstractWebSocketClientApiCommunication, IHelloWorldV5ApiCommunication
    {
        private string GetHeaderTopic(Visibility visibility, string topic)
        {
            return visibility == Visibility.Public ? null : (topic ?? PrivateTopic);
        } 
        
        private XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.HelloWorldInstance HelloWorldEntryPoint;
        
        public XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.HelloWorldInstance GetHelloWorldEntryPoint()
        {
            if ((this.HelloWorldEntryPoint == null))
            {
                return new XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.HelloWorldInstance(null, Context.CreateEmptyContext(), -1, -1);
            }
            return this.HelloWorldEntryPoint;
        }

        public void SendToHelloWorld(Context context, XComponent.HelloWorld.UserObject.CreateListener evt, Visibility visibility = Visibility.Public, string topic = null)
        {
            if ((context == null))
            {
                throw new EmptyContextException();
            }
            var evnt = evt;
            if ((evnt == null))
            {
                evnt = new XComponent.HelloWorld.UserObject.CreateListener();
            }
            
            string headerTopic = GetHeaderTopic(visibility, topic);
            var header = new Header(context.StateCode, context.StateMachineId, context.WorkerId, context.StateMachineCode, context.ComponentCode, 10, (int)IncomingEventType.Transition, null , headerTopic, string.IsNullOrEmpty(context.MessageType) ? evnt.GetType().FullName : context.MessageType, this.SessionData, null);
            this.publishersMap[context.ComponentCode][context.StateMachineCode].Send(header, evnt);
        }
        public void SendToHelloWorld(Context context, XComponent.HelloWorld.UserObject.SayHello evt, Visibility visibility = Visibility.Public, string topic = null)
        {
            if ((context == null))
            {
                throw new EmptyContextException();
            }
            var evnt = evt;
            if ((evnt == null))
            {
                evnt = new XComponent.HelloWorld.UserObject.SayHello();
            }
            
            string headerTopic = GetHeaderTopic(visibility, topic);
            var header = new Header(context.StateCode, context.StateMachineId, context.WorkerId, context.StateMachineCode, context.ComponentCode, 20, (int)IncomingEventType.Transition, null , headerTopic, string.IsNullOrEmpty(context.MessageType) ? evnt.GetType().FullName : context.MessageType, this.SessionData, null);
            this.publishersMap[context.ComponentCode][context.StateMachineCode].Send(header, evnt);
        }
        public void SendToHelloWorld(Context context, XComponent.HelloWorld.UserObject.Kill evt, Visibility visibility = Visibility.Public, string topic = null)
        {
            if ((context == null))
            {
                throw new EmptyContextException();
            }
            var evnt = evt;
            if ((evnt == null))
            {
                evnt = new XComponent.HelloWorld.UserObject.Kill();
            }
            
            string headerTopic = GetHeaderTopic(visibility, topic);
            var header = new Header(context.StateCode, context.StateMachineId, context.WorkerId, context.StateMachineCode, context.ComponentCode, 12, (int)IncomingEventType.Transition, null , headerTopic, string.IsNullOrEmpty(context.MessageType) ? evnt.GetType().FullName : context.MessageType, this.SessionData, null);
            this.publishersMap[context.ComponentCode][context.StateMachineCode].Send(header, evnt);
        }
        public void SendToHelloWorld(Context context, XComponent.HelloWorld.UserObject.Ping evt, Visibility visibility = Visibility.Public, string topic = null)
        {
            if ((context == null))
            {
                throw new EmptyContextException();
            }
            var evnt = evt;
            if ((evnt == null))
            {
                evnt = new XComponent.HelloWorld.UserObject.Ping();
            }
            
            string headerTopic = GetHeaderTopic(visibility, topic);
            var header = new Header(context.StateCode, context.StateMachineId, context.WorkerId, context.StateMachineCode, context.ComponentCode, 14, (int)IncomingEventType.Transition, null , headerTopic, string.IsNullOrEmpty(context.MessageType) ? evnt.GetType().FullName : context.MessageType, this.SessionData, null);
            this.publishersMap[context.ComponentCode][context.StateMachineCode].Send(header, evnt);
        }
        public void SendToHelloWorld(Context context, XComponent.HelloWorld.UserObject.SayGoodbyeToAll evt, Visibility visibility = Visibility.Public, string topic = null)
        {
            if ((context == null))
            {
                throw new EmptyContextException();
            }
            var evnt = evt;
            if ((evnt == null))
            {
                evnt = new XComponent.HelloWorld.UserObject.SayGoodbyeToAll();
            }
            
            string headerTopic = GetHeaderTopic(visibility, topic);
            var header = new Header(context.StateCode, context.StateMachineId, context.WorkerId, context.StateMachineCode, context.ComponentCode, 19, (int)IncomingEventType.Transition, null , headerTopic, string.IsNullOrEmpty(context.MessageType) ? evnt.GetType().FullName : context.MessageType, this.SessionData, null);
            this.publishersMap[context.ComponentCode][context.StateMachineCode].Send(header, evnt);
        }
        public void SendToHelloWorld(Context context, XComponent.HelloWorld.UserObject.SayGoodbye evt, Visibility visibility = Visibility.Public, string topic = null)
        {
            if ((context == null))
            {
                throw new EmptyContextException();
            }
            var evnt = evt;
            if ((evnt == null))
            {
                evnt = new XComponent.HelloWorld.UserObject.SayGoodbye();
            }
            
            string headerTopic = GetHeaderTopic(visibility, topic);
            var header = new Header(context.StateCode, context.StateMachineId, context.WorkerId, context.StateMachineCode, context.ComponentCode, 18, (int)IncomingEventType.Transition, null , headerTopic, string.IsNullOrEmpty(context.MessageType) ? evnt.GetType().FullName : context.MessageType, this.SessionData, null);
            this.publishersMap[context.ComponentCode][context.StateMachineCode].Send(header, evnt);
        }
        

        public void SendEventToHelloWorld(XComponent.HelloWorld.UserObject.CreateListener evt, Visibility visibility = Visibility.Public, string topic = null)
        {
            var evnt = evt;
            if ((evnt == null))
            {
                evnt = new XComponent.HelloWorld.UserObject.CreateListener();
            }
            SendEventToHelloWorld(HelloWorld_Component.StdEnum.HelloWorld, evnt, visibility, topic);
        }
        public void SendEventToHelloWorld(HelloWorld_Component.StdEnum stdEnum, XComponent.HelloWorld.UserObject.CreateListener evt, Visibility visibility = Visibility.Public, string topic = null)
        {
            var evnt = evt;
            if ((evnt == null))
            {
                evnt = new XComponent.HelloWorld.UserObject.CreateListener();
            }
            string headerTopic = GetHeaderTopic(visibility, topic);
            var header = new Header(null, null, null, (int)stdEnum, (int)HelloWorld_Component.ComponentCode, 10, (int)IncomingEventType.Transition, null, headerTopic, evnt.GetType().FullName, this.SessionData, null);
            this.publishersMap[(int)HelloWorld_Component.ComponentCode][(int)stdEnum].Send(header, evnt);
        }
        public void SendEventToHelloWorld(XComponent.HelloWorld.UserObject.SayHello evt, Visibility visibility = Visibility.Public, string topic = null)
        {
            var evnt = evt;
            if ((evnt == null))
            {
                evnt = new XComponent.HelloWorld.UserObject.SayHello();
            }
            SendEventToHelloWorld(HelloWorld_Component.StdEnum.HelloWorld, evnt, visibility, topic);
        }
        public void SendEventToHelloWorld(HelloWorld_Component.StdEnum stdEnum, XComponent.HelloWorld.UserObject.SayHello evt, Visibility visibility = Visibility.Public, string topic = null)
        {
            var evnt = evt;
            if ((evnt == null))
            {
                evnt = new XComponent.HelloWorld.UserObject.SayHello();
            }
            string headerTopic = GetHeaderTopic(visibility, topic);
            var header = new Header(null, null, null, (int)stdEnum, (int)HelloWorld_Component.ComponentCode, 20, (int)IncomingEventType.Transition, null, headerTopic, evnt.GetType().FullName, this.SessionData, null);
            this.publishersMap[(int)HelloWorld_Component.ComponentCode][(int)stdEnum].Send(header, evnt);
        }
        public void SendEventToHelloWorld(XComponent.HelloWorld.UserObject.Kill evt, Visibility visibility = Visibility.Public, string topic = null)
        {
            var evnt = evt;
            if ((evnt == null))
            {
                evnt = new XComponent.HelloWorld.UserObject.Kill();
            }
            SendEventToHelloWorld(HelloWorld_Component.StdEnum.HelloWorldResponse, evnt, visibility, topic);
        }
        public void SendEventToHelloWorld(HelloWorld_Component.StdEnum stdEnum, XComponent.HelloWorld.UserObject.Kill evt, Visibility visibility = Visibility.Public, string topic = null)
        {
            var evnt = evt;
            if ((evnt == null))
            {
                evnt = new XComponent.HelloWorld.UserObject.Kill();
            }
            string headerTopic = GetHeaderTopic(visibility, topic);
            var header = new Header(null, null, null, (int)stdEnum, (int)HelloWorld_Component.ComponentCode, 12, (int)IncomingEventType.Transition, null, headerTopic, evnt.GetType().FullName, this.SessionData, null);
            this.publishersMap[(int)HelloWorld_Component.ComponentCode][(int)stdEnum].Send(header, evnt);
        }
        public void SendEventToHelloWorld(XComponent.HelloWorld.UserObject.Ping evt, Visibility visibility = Visibility.Public, string topic = null)
        {
            var evnt = evt;
            if ((evnt == null))
            {
                evnt = new XComponent.HelloWorld.UserObject.Ping();
            }
            SendEventToHelloWorld(HelloWorld_Component.StdEnum.HelloWorldResponse, evnt, visibility, topic);
        }
        public void SendEventToHelloWorld(HelloWorld_Component.StdEnum stdEnum, XComponent.HelloWorld.UserObject.Ping evt, Visibility visibility = Visibility.Public, string topic = null)
        {
            var evnt = evt;
            if ((evnt == null))
            {
                evnt = new XComponent.HelloWorld.UserObject.Ping();
            }
            string headerTopic = GetHeaderTopic(visibility, topic);
            var header = new Header(null, null, null, (int)stdEnum, (int)HelloWorld_Component.ComponentCode, 14, (int)IncomingEventType.Transition, null, headerTopic, evnt.GetType().FullName, this.SessionData, null);
            this.publishersMap[(int)HelloWorld_Component.ComponentCode][(int)stdEnum].Send(header, evnt);
        }
        public void SendEventToHelloWorld(XComponent.HelloWorld.UserObject.SayGoodbye evt, Visibility visibility = Visibility.Public, string topic = null)
        {
            var evnt = evt;
            if ((evnt == null))
            {
                evnt = new XComponent.HelloWorld.UserObject.SayGoodbye();
            }
            SendEventToHelloWorld(HelloWorld_Component.StdEnum.HelloWorldResponse, evnt, visibility, topic);
        }
        public void SendEventToHelloWorld(HelloWorld_Component.StdEnum stdEnum, XComponent.HelloWorld.UserObject.SayGoodbye evt, Visibility visibility = Visibility.Public, string topic = null)
        {
            var evnt = evt;
            if ((evnt == null))
            {
                evnt = new XComponent.HelloWorld.UserObject.SayGoodbye();
            }
            string headerTopic = GetHeaderTopic(visibility, topic);
            var header = new Header(null, null, null, (int)stdEnum, (int)HelloWorld_Component.ComponentCode, 18, (int)IncomingEventType.Transition, null, headerTopic, evnt.GetType().FullName, this.SessionData, null);
            this.publishersMap[(int)HelloWorld_Component.ComponentCode][(int)stdEnum].Send(header, evnt);
        }
        public void SendEventToHelloWorld(XComponent.HelloWorld.UserObject.SayGoodbyeToAll evt, Visibility visibility = Visibility.Public, string topic = null)
        {
            var evnt = evt;
            if ((evnt == null))
            {
                evnt = new XComponent.HelloWorld.UserObject.SayGoodbyeToAll();
            }
            SendEventToHelloWorld(HelloWorld_Component.StdEnum.HelloWorldResponse, evnt, visibility, topic);
        }
        public void SendEventToHelloWorld(HelloWorld_Component.StdEnum stdEnum, XComponent.HelloWorld.UserObject.SayGoodbyeToAll evt, Visibility visibility = Visibility.Public, string topic = null)
        {
            var evnt = evt;
            if ((evnt == null))
            {
                evnt = new XComponent.HelloWorld.UserObject.SayGoodbyeToAll();
            }
            string headerTopic = GetHeaderTopic(visibility, topic);
            var header = new Header(null, null, null, (int)stdEnum, (int)HelloWorld_Component.ComponentCode, 19, (int)IncomingEventType.Transition, null, headerTopic, evnt.GetType().FullName, this.SessionData, null);
            this.publishersMap[(int)HelloWorld_Component.ComponentCode][(int)stdEnum].Send(header, evnt);
        }
        
        

        public event Action<XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.ResponseListenerInstance> HelloWorld_ResponseListenerInstanceUpdated;
        
        private void NotifyHelloWorld_ResponseListenerInstanceUpdated(object sender, MessageEventArgs e)
        {
            if ((this.HelloWorld_ResponseListenerInstanceUpdated != null))
            {
                this.HelloWorld_ResponseListenerInstanceUpdated(new XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.ResponseListenerInstance(
                    e.MessageReceived, new Context(e.Header.StateMachineId, e.Header.WorkerId.Value, e.Header.ComponentCode, e.Header.StateMachineCode.Value, e.Header.StateCode.Value, null, null, e.Header.ErrorMessage), e.Header.StateCode.Value, e.Header.StateMachineCode.Value));
            }
        }
        public event Action<XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.HelloWorldResponseInstance> HelloWorld_HelloWorldResponseInstanceUpdated;
        
        private void NotifyHelloWorld_HelloWorldResponseInstanceUpdated(object sender, MessageEventArgs e)
        {
            if ((this.HelloWorld_HelloWorldResponseInstanceUpdated != null))
            {
                this.HelloWorld_HelloWorldResponseInstanceUpdated(new XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.HelloWorldResponseInstance(
                    e.MessageReceived, new Context(e.Header.StateMachineId, e.Header.WorkerId.Value, e.Header.ComponentCode, e.Header.StateMachineCode.Value, e.Header.StateCode.Value, null, null, e.Header.ErrorMessage), e.Header.StateCode.Value, e.Header.StateMachineCode.Value));
            }
        }
        

        public event Action<XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.HelloWorldInstance> HelloWorld_HelloWorldInstanceUpdated;
        
        private void NotifyHelloWorld_HelloWorldInstanceUpdated(object sender, MessageEventArgs e)
        {
            if ((this.HelloWorld_HelloWorldInstanceUpdated != null))
            {
                this.HelloWorld_HelloWorldInstanceUpdated(
                    new XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.HelloWorldInstance(
                        e.MessageReceived, 
                        new Context(e.Header.StateMachineId, e.Header.WorkerId.Value, e.Header.ComponentCode, e.Header.StateMachineCode.Value, e.Header.StateCode.Value, null, null, e.Header.ErrorMessage), 
                        e.Header.StateCode.Value, 
                        e.Header.StateMachineCode.Value));
            }
        }

        public event System.Action<XCClientAPICommon.Error.RuntimeError> HelloWorldErrorReceived;
        
        private void OnHelloWorldErrorReceived(object sender, MessageEventArgs e)
        {
            if ((this.HelloWorldErrorReceived != null))
            {
                var errorMessage = e.MessageReceived as XComponent.Common.Processing.ErrorMessage;
                XCClientAPICommon.Error.RuntimeErrorType runtimeErrorType;
                if ((errorMessage.ErrorType == XComponent.Common.Processing.ErrorType.Warning))
                {
                    runtimeErrorType = XCClientAPICommon.Error.RuntimeErrorType.WARNING;
                }
                else
                {
                    runtimeErrorType = XCClientAPICommon.Error.RuntimeErrorType.ERROR;
                }
                this.HelloWorldErrorReceived(new XCClientAPICommon.Error.RuntimeError(errorMessage.Message, errorMessage.Component, errorMessage.StateMachine, errorMessage.State, e.Header.StateMachineId, new Context(e.Header.StateMachineId, e.Header.WorkerId.Value, e.Header.ComponentCode, e.Header.StateMachineCode.Value, e.Header.StateCode.Value, null, null, e.Header.ErrorMessage), runtimeErrorType) {Stacktrace = errorMessage.Stacktrace});
            }
        }
        
      
      
        public virtual bool Init(string configFile, out XCClientAPICommon.Client.InitReport report, IClientApiConfigurationOverride communicationOverride = null)
        {
            report = new XCClientAPICommon.Client.InitReport();
            if ((alreadyStarted == true))
            {
                consumersMap[HelloWorld_Component.ComponentCode][(int)HelloWorld_Component.StdEnum.HelloWorld].MessageReceived -= NotifyHelloWorld_HelloWorldInstanceUpdated;
                consumersMap[HelloWorld_Component.ComponentCode][(int)HelloWorld_Component.StdEnum.ResponseListener].MessageReceived -= NotifyHelloWorld_ResponseListenerInstanceUpdated;
                consumersMap[HelloWorld_Component.ComponentCode][(int)HelloWorld_Component.StdEnum.HelloWorldResponse].MessageReceived -= NotifyHelloWorld_HelloWorldResponseInstanceUpdated;
                
                errorConsumerMap[HelloWorld_Component.ComponentCode].MessageReceived -= OnHelloWorldErrorReceived;
                
            }
            else
            {
                Init(configFile, new List<int>() { HelloWorld_Component.ComponentCode,}, communicationOverride);
            }
            StartConsumers();

            this.privateConsumers.ForEach(c =>
                                              {
                                                  c.MessageReceived += OnPrivateMessageReceived;
                                              });
            
            this.HelloWorldEntryPoint = null;
            consumersMap[HelloWorld_Component.ComponentCode][(int)HelloWorld_Component.StdEnum.HelloWorld].MessageReceived += NotifyHelloWorld_HelloWorldInstanceUpdated;
            consumersMap[HelloWorld_Component.ComponentCode][(int)HelloWorld_Component.StdEnum.ResponseListener].MessageReceived += NotifyHelloWorld_ResponseListenerInstanceUpdated;
            consumersMap[HelloWorld_Component.ComponentCode][(int)HelloWorld_Component.StdEnum.HelloWorldResponse].MessageReceived += NotifyHelloWorld_HelloWorldResponseInstanceUpdated;
            
            errorConsumerMap[HelloWorld_Component.ComponentCode].MessageReceived += OnHelloWorldErrorReceived;
            

            this.alreadyStarted = true;

            var snapshotHelloWorldEntryPoint = GetSnapshot((int)HelloWorld_Component.StdEnum.HelloWorld,HelloWorld_Component.ComponentCode, null, TimeOut, null);
            const string HelloWorldComponent = "HelloWorld";
          
            if (snapshotHelloWorldEntryPoint == null)
            {
                report.ComponentsInitFailed.Add(HelloWorldComponent);
                return false;
            }
            else
            {
                SnapshotItem first = snapshotHelloWorldEntryPoint.Items.FirstOrDefault();
                if (first == null)
                {
                    report.ComponentsInitFailed.Add(HelloWorldComponent);
                }
                else
                {
                    report.ComponentsInitSucceeded.Add(HelloWorldComponent);
                    this.HelloWorldEntryPoint = new XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld.HelloWorldInstance(
                        first.PublicMember,
                        new Context(first.StateMachineId, first.WorkerId, first.ComponentCode, first.StateMachineCode, first.StateCode, null, null, null),
                        first.StateCode,
                        first.StateMachineCode);
                }
            }

            
           
            return !report.ComponentsInitFailed.Any();
        }

        protected override void OnPrivateMessageReceived(object sender, MessageEventArgs args) 
        {
            if (args.Header.StateMachineCode != null && args.Header.ComponentCode == HelloWorld_Component.ComponentCode && args.Header.StateMachineCode.Value == (int)HelloWorld_Component.StdEnum.HelloWorld)
            {
                NotifyHelloWorld_HelloWorldInstanceUpdated(sender, args);
            }
            if (args.Header.StateMachineCode != null && args.Header.ComponentCode == HelloWorld_Component.ComponentCode && args.Header.StateMachineCode.Value == (int)HelloWorld_Component.StdEnum.ResponseListener)
            {
                NotifyHelloWorld_ResponseListenerInstanceUpdated(sender, args);
            }
            if (args.Header.StateMachineCode != null && args.Header.ComponentCode == HelloWorld_Component.ComponentCode && args.Header.StateMachineCode.Value == (int)HelloWorld_Component.StdEnum.HelloWorldResponse)
            {
                NotifyHelloWorld_HelloWorldResponseInstanceUpdated(sender, args);
            }
                                                                                           
        }
    }
}

