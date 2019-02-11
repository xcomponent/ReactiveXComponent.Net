namespace XComponent.HelloWorldV5.HelloWorldV5Api.HelloWorld
{
    using System;
    using XComponent.Common.ApiContext;


    [System.Serializable()]
    sealed public class HelloWorldInstance : AbstractInstance
    {
        private const string s_StateMachineName = "HelloWorld";

        public HelloWorldInstance()
        {
        }

        public HelloWorldInstance(object publicMember, Context context, int stateCode, int stateMachineCode) :
            base(stateCode, stateMachineCode)
        {
            this.context = context;
        }

        protected override string ConvertStateCodeToString(int stateCode)
        {
            return ((HelloWorld_StateMachine.HelloWorldStateEnum)stateCode).ToString();
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
