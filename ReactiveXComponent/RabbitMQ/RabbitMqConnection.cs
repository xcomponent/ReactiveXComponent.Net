namespace ReactiveXComponent.RabbitMQ
{
    using System;
    using System.Threading;
    using global::RabbitMQ.Client;


    public class RabbitMqConnection : IRabbitMqConnection
    {
        private const string CannotReachBroker = "Cannot connect to broker ";

        private const int TimeoutConnection = 10000;

        private bool _disposed = false;

        private IConnection _connection;
        private ConnectionFactory _factory;
        

        public RabbitMqConnection(BusDetails busDetails)
        {
            this.Init(busDetails);
        }

        public void Close()
        {
            this._connection.Close();
        }

        private void Init(BusDetails busDetails)
        {
            try
            {
                this._factory = new ConnectionFactory()
                {
                    UserName = busDetails.Username ?? "test",
                    Password = busDetails.Password ?? "test",
                    VirtualHost = ConnectionFactory.DefaultVHost,
                    HostName = busDetails.Host,
                    Port = busDetails.Port,
                    Protocol = Protocols.DefaultProtocol
                };
            }
            catch(Exception e)
            {
                throw new Exception( "RabbitMQ Connection init failed", e);
            }
        }

        public IConnection GetConnection()
        {
            if (this._connection == null || !this._connection.IsOpen)
            {
                this._connection = this.CreateConnection();
            }

            return this._connection;
        }

        private IConnection CreateConnection()
        {
            if (this._factory != null)
            {
                var lockEvent = new AutoResetEvent(false);
                string errorMessage = null;
                ThreadPool.QueueUserWorkItem((obj) =>
                    {
                        try
                        {
                            this._connection = this._factory.CreateConnection();
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

            return this._connection;
        }

        private void Dispose(bool disposed)
        {
            if (disposed)
                return;
            else
            {
                Close();
            }
            _disposed = true;

        } 
        public void Dispose()
        {
            Dispose(_disposed);
            GC.SuppressFinalize(this);
        }
    }
}
