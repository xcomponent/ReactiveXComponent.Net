namespace XCClientLib.RabbitMQ
{
    using System;
    using System.Threading;
    using global::RabbitMQ.Client;


    public class RabbitMqConnection : IRabbitMqConnection
    {
        private const string CannotReachBroker = "Cannot connect to broker ";

        private const int TimeoutConnection = 10000;

        private IConnection connection;
        private ConnectionFactory factory;

        public RabbitMqConnection(BusDetails busDetails)
        {
            this.Init(busDetails);
        }

        public void Close()
        {
            this.connection.Close();
        }

        private void Init(BusDetails busDetails)
        {
            try
            {
                this.factory = new ConnectionFactory()
                {
                    UserName = busDetails.Username != null ? busDetails.Username : "test",
                    Password = busDetails.Password != null ? busDetails.Password : "test",
                    VirtualHost = ConnectionFactory.DefaultVHost,
                    HostName = busDetails.Host,
                    Port = busDetails.Port,
                    Protocol = Protocols.DefaultProtocol
                };
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public IConnection GetConnection()
        {
            if (this.connection == null || !this.connection.IsOpen)
            {
                this.connection = this.CreateConnection();
            }

            return this.connection;
        }

        private IConnection CreateConnection()
        {
            if (this.factory != null)
            {
                var lockEvent = new AutoResetEvent(false);
                string errorMessage = null;
                ThreadPool.QueueUserWorkItem((obj) =>
                    {
                        try
                        {
                            this.connection = this.factory.CreateConnection();
                            lockEvent.Set();
                        }
                        catch(Exception ex)
                        {
                            errorMessage = ex.Message;
                        }
                    });
                if (!lockEvent.WaitOne(TimeoutConnection))
                {
                    throw new Exception(CannotReachBroker, null);
                }

                if (errorMessage != null)
                {
                    throw new Exception(errorMessage);
                }                                
            }

            return this.connection;
        }

        public void Dispose()
        {
            this.Close();
        }
    }
}
