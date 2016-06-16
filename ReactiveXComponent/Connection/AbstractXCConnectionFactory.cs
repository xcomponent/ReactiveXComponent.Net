using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveXComponent.RabbitMq;

namespace ReactiveXComponent.Connection
{
    public abstract class AbstractXCConnectionFactory
    {
        public abstract IXCConnection CreateConnection();
    }
}
