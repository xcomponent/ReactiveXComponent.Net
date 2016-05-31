namespace ReactiveXComponent.RabbitMQ
{
    public class Header
    {
        public long StateMachineCode { get; set; }

        public long ComponentCode { get; set; }

        public long EngineCode { get; set; }

        public long EventCode { get; set; }

        public string MessageType { get; set; }

        public string PublishTopic { get; set; }

        public override bool Equals(object obj)
        {
            var toCompareWith = obj as Header;
            if (toCompareWith == null)
                return false;
            else
                return (StateMachineCode == toCompareWith.StateMachineCode
                        && ComponentCode == toCompareWith.ComponentCode
                        && EngineCode == toCompareWith.EngineCode
                        && EventCode == toCompareWith.EventCode
                        && MessageType == toCompareWith.MessageType
                        && PublishTopic == toCompareWith.PublishTopic);
        }
    }
}
