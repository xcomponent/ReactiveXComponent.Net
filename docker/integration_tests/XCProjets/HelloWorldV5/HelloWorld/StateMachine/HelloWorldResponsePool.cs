﻿////------------------------------------------------------------------------------
//// <auto-generated>
////     This code was generated by XCTools.
////     Changes to this file may cause incorrect behavior and will be lost if
////     the code is regenerated.
//// </auto-generated>
////------------------------------------------------------------------------------
using System;
using XComponent.Runtime.StateMachine;
using XComponent.Runtime.StateMachine.Pool;

namespace XComponent.HelloWorld.StateMachine
{
    public class HelloWorldResponsePool : AbstractStateMachinePool
    {
        protected override IStateMachine CreateInstance()
        {
			return new HelloWorldResponse();
        }
    }
}