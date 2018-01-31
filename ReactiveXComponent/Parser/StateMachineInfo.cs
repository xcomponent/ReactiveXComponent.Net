namespace ReactiveXComponent.Parser
{
    public class StateMachineInfo
    {
        public string StateMachineName { get; }
        public int StateMachineCode { get; }

        public StateMachineInfo(string stateMachineName, int stateMachineCode)
        {
            StateMachineName = stateMachineName;
            StateMachineCode = stateMachineCode;
        }
    }
}
