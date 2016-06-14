
namespace ReactiveXComponent.Configuration
{
    public class XCConfiguration
    {
        private readonly Parser.Parser _parser;
        private readonly Tags _tags;

        public XCConfiguration(Parser.Parser parser)
        {
            _parser = parser;
            _tags = new Tags();
        }

        public void Init()
        {
            _parser.Parse(_tags);
        }

        public string GetConnectionType()
        {
            return _parser.GetConnectionType();
        }

        public long GetStateMachineCode(string component, string stateMachine)
        {
            return _parser.GetStateMachineCode(component, stateMachine);
        }

        public long GetComponentCode(string component)
        {
            return _parser.GetComponentCode(component);
        }

        public int GetPublisherEventCode(string evnt)
        {
            return _parser.GetPublisherEventCode(evnt);
        }


    }

   

}
