﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by XCTools.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using XComponent.Common.ApiContext;
using XComponent.Runtime.Communication;
using XComponent.HelloWorld.Common;

namespace XComponent.HelloWorld.Common.Senders
{
    public class InitDefaultEventOnListeningHelloWorldSender : AbstractSender, IInitDefaultEventOnListeningHelloWorldSenderInterface
    {
		public void CreateListener(RuntimeContext context, string privateTopic = null)
		{
			_internalAPI.Send(context, 10, default(XComponent.HelloWorld.UserObject.CreateListener), privateTopic);
		}


		public void CreateListener(RuntimeContext context, XComponent.HelloWorld.UserObject.CreateListener transitionEvent, string privateTopic = null)
		{
			_internalAPI.Send(context, 10, transitionEvent, privateTopic);
		}


		public void SendEvent(XComponent.HelloWorld.UserObject.CreateListener evt, string privateTopic = null)
		{
			_internalAPI.SendEvent(-69981087,(int) StdEnum.HelloWorld, 10, evt, privateTopic);
		}

		public void SendEvent(StdEnum stdEnum, XComponent.HelloWorld.UserObject.CreateListener evt, string privateTopic = null)
		{
			_internalAPI.SendEvent(-69981087,(int) stdEnum, 10, evt, privateTopic);
		}

    }

    public class CountHelloWorldResponseOnUpResponseListenerSender : AbstractSender, ICountHelloWorldResponseOnUpResponseListenerSenderInterface
    {


    }

    public class CreateListenerCreateListenerOnUpResponseListenerSender : AbstractSender, ICreateListenerCreateListenerOnUpResponseListenerSenderInterface
    {


    }

    public class PingPingOnPublishedHelloWorldResponseSender : AbstractSender, IPingPingOnPublishedHelloWorldResponseSenderInterface
    {
		public void Kill(RuntimeContext context)
		{
			_internalAPI.Send(context, 12, default(XComponent.HelloWorld.UserObject.Kill), null);
		}


		public void Kill(RuntimeContext context, XComponent.HelloWorld.UserObject.Kill transitionEvent)
		{
			_internalAPI.Send(context, 12, transitionEvent, null);
		}


		public void SendEvent(XComponent.HelloWorld.UserObject.Kill evt)
		{
			_internalAPI.SendEvent(-69981087,(int) StdEnum.HelloWorldResponse, 12, evt, null);
		}

		public void SendEvent(StdEnum stdEnum, XComponent.HelloWorld.UserObject.Kill evt)
		{
			_internalAPI.SendEvent(-69981087,(int) stdEnum, 12, evt, null);
		}

    }

    public class SayGoodbyeSayGoodbyeOnPublishedHelloWorldResponseSender : AbstractSender, ISayGoodbyeSayGoodbyeOnPublishedHelloWorldResponseSenderInterface
    {
		public void Kill(RuntimeContext context)
		{
			_internalAPI.Send(context, 12, default(XComponent.HelloWorld.UserObject.Kill), null);
		}


		public void Kill(RuntimeContext context, XComponent.HelloWorld.UserObject.Kill transitionEvent)
		{
			_internalAPI.Send(context, 12, transitionEvent, null);
		}


		public void SendEvent(XComponent.HelloWorld.UserObject.Kill evt)
		{
			_internalAPI.SendEvent(-69981087,(int) StdEnum.HelloWorldResponse, 12, evt, null);
		}

		public void SendEvent(StdEnum stdEnum, XComponent.HelloWorld.UserObject.Kill evt)
		{
			_internalAPI.SendEvent(-69981087,(int) stdEnum, 12, evt, null);
		}

    }

    public class SayGoodbyeToAllSayGoodbyeToAllOnPublishedHelloWorldResponseSender : AbstractSender, ISayGoodbyeToAllSayGoodbyeToAllOnPublishedHelloWorldResponseSenderInterface
    {
		public void Kill(RuntimeContext context)
		{
			_internalAPI.Send(context, 12, default(XComponent.HelloWorld.UserObject.Kill), null);
		}


		public void Kill(RuntimeContext context, XComponent.HelloWorld.UserObject.Kill transitionEvent)
		{
			_internalAPI.Send(context, 12, transitionEvent, null);
		}


		public void SendEvent(XComponent.HelloWorld.UserObject.Kill evt)
		{
			_internalAPI.SendEvent(-69981087,(int) StdEnum.HelloWorldResponse, 12, evt, null);
		}

		public void SendEvent(StdEnum stdEnum, XComponent.HelloWorld.UserObject.Kill evt)
		{
			_internalAPI.SendEvent(-69981087,(int) stdEnum, 12, evt, null);
		}

    }

    public class SayHelloSayHelloOnPublishedHelloWorldResponseSender : AbstractSender, ISayHelloSayHelloOnPublishedHelloWorldResponseSenderInterface
    {
		public void Kill(RuntimeContext context)
		{
			_internalAPI.Send(context, 12, default(XComponent.HelloWorld.UserObject.Kill), null);
		}


		public void Kill(RuntimeContext context, XComponent.HelloWorld.UserObject.Kill transitionEvent)
		{
			_internalAPI.Send(context, 12, transitionEvent, null);
		}


		public void SendEvent(XComponent.HelloWorld.UserObject.Kill evt)
		{
			_internalAPI.SendEvent(-69981087,(int) StdEnum.HelloWorldResponse, 12, evt, null);
		}

		public void SendEvent(StdEnum stdEnum, XComponent.HelloWorld.UserObject.Kill evt)
		{
			_internalAPI.SendEvent(-69981087,(int) stdEnum, 12, evt, null);
		}

    }

    public class EntryPointSender : AbstractSender, IEntryPointSenderInterface
    {


    }

}