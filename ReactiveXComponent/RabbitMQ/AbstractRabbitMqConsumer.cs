namespace ReactiveXComponent.RabbitMQ
{
    using System;
    using System.IO;
    using System.Threading;

    using global::RabbitMQ.Client;
    using global::RabbitMQ.Client.Events;
    using global::RabbitMQ.Client.Exceptions;

    public delegate void ConnectionFailure(string reason);

    public abstract class AbstractRabbitMqConsumer : IConsumer
    {
        protected readonly string ExchangeName;

        protected readonly IRabbitMqConnection RabbitMqConnection;

        protected IModel Channel;

        protected string QueueName;        

        private IModel _replyChannel;

        private EventingBasicConsumer _consumer;

        public event EventHandler<MessageEventArgs> MessageReceived;


        protected void OnMessageReceived(MessageEventArgs e)
        {
            EventHandler<MessageEventArgs> handler = this.MessageReceived;
            handler?.Invoke(this, e);
        }

        protected AbstractRabbitMqConsumer(string exchangeName, IRabbitMqConnection rabbitMqConnection)
        {
            RabbitMqConnection = rabbitMqConnection;
            ExchangeName = exchangeName;
        }

        protected abstract void SpecificStart();

        #region ICommunicator Members

        public bool IsStarted { get; private set; }

        /// <summary>
        /// Starts the current consumer, starting a thread that will listen to the specified exchange with the given routing key
        /// <exception cref="ConnectionFailureException">When start fails</exception>
        /// </summary>
        public void Start()
        {
            try
            {
                if (!IsStarted)
                {
                    var connection = RabbitMqConnection.GetConnection();
                    if (connection != null && connection.IsOpen)
                    {
                        Channel = connection.CreateModel();
                        Channel.ModelShutdown += ChannelOnModelShutdown;
                        Channel.ExchangeDeclare(this.ExchangeName, global::RabbitMQ.Client.ExchangeType.Topic);
                        QueueName = Channel.QueueDeclare().QueueName;

                        _consumer = new EventingBasicConsumer(Channel);
                        Channel.BasicConsume(QueueName, false, _consumer);

                        SpecificStart();

                        _replyChannel = connection.CreateModel();
                        _replyChannel.ModelShutdown += ChannelOnModelShutdown;
                        IsStarted = true;
                        var thread = new Thread(this.Listen);
                        thread.Start();
                    }
                }
            }
            catch (OperationInterruptedException e)
            {
                //exchange may not exists !!
                throw new Exception("Start consumer failure", e);
            }
        }

        private void ChannelOnModelShutdown(object sender, ShutdownEventArgs shutdownEventArgs)
        {
            ConnectionFailed?.Invoke(shutdownEventArgs.ReplyText);
        }

        /// <summary>
        /// Stops the consumer, closing the channel it uses
        /// </summary>
        public void Stop()
        {
            IsStarted = false;
            Channel.ModelShutdown -= ChannelOnModelShutdown;
            _replyChannel.ModelShutdown -= ChannelOnModelShutdown;


            _consumer.OnCancel();
            Channel.Close();
            Channel = null;
        }

        #endregion

        /// <summary>
        /// Event raised when the connection used by the publisher is stopped externally
        /// </summary>
        public event ConnectionFailure ConnectionFailed;

        private void Listen()
        {
            while (IsStarted)
            {
                try
                {
                    _consumer.Received += (o, e) =>
                    {
                            Channel.BasicAck(e.DeliveryTag, false);

                            DispatchMessage(e);
                    };
                }
                catch (EndOfStreamException ex)
                {
                    // The consumer was cancelled, the model closed, or the
                    // connection went away.
                    // throw new ConnectionFailureException("Consumer has been interrupted", ex);
                    IsStarted = false;

                    ConnectionFailed?.Invoke("Consumer has been interrupted : " + ex.Message);
                }
            }
        }

        protected abstract void DispatchMessage(BasicDeliverEventArgs basicAckEventArgs);

    }
}
