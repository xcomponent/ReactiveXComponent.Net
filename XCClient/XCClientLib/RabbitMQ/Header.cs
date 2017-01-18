namespace XCClientLib.RabbitMQ
{
    public class Header
    {
        public long StateMachineCode { get; set; }

        public long ComponentCode { get; set; }

        public long EngineCode { get; set; }

        public long EventCode { get; set; }

        public string MessageType { get; set; }

        public string PublishTopic { get; set; }
    }
}
