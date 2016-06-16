using System.IO;
using ReactiveXComponent.Parser;

namespace ReactiveXComponent.Configuration
{
    public class XCConfiguration : IXCConfiguration
    {
        private readonly XCApiConfigParser _parser;

        public XCConfiguration(Parser.XCApiConfigParser parser)
        {
            _parser = parser;
        }

        public void Init(Stream xcApiStream)
        {
            _parser.Parse(xcApiStream);
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
