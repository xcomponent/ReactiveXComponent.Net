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

        protected readonly IRabbitMqConnection rabbitMqConnection;

        protected IModel Channel;

        protected string QueueName;        

        private IModel replyChannel;

        private QueueingBasicConsumer consumer;

        public event EventHandler<MessageEventArgs> MessageReceived;


        protected void OnMessageReceived(MessageEventArgs e)
        {
            EventHandler<MessageEventArgs> handler = this.MessageReceived;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected AbstractRabbitMqConsumer(string exchangeName, IRabbitMqConnection rabbitMqConnection)
        {
            this.rabbitMqConnection = rabbitMqConnection;
            this.ExchangeName = exchangeName;
        }

        protected abstract void SpecificStart();

        #region ICommunicator Members

        public bool IsStarted { get; protected set; }

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
                    var connection = rabbitMqConnection.GetConnection();
                    if (connection != null && connection.IsOpen)
                    {
                        this.Channel = connection.CreateModel();
                        this.Channel.ModelShutdown += ChannelOnModelShutdown;
                        this.Channel.ExchangeDeclare(this.ExchangeName, global::RabbitMQ.Client.ExchangeType.Topic);
                        this.QueueName = this.Channel.QueueDeclare().QueueName;

                        this.consumer = new QueueingBasicConsumer(this.Channel);
                        this.Channel.BasicConsume(this.QueueName, false, this.consumer);

                        this.SpecificStart();

                        this.replyChannel = connection.CreateModel();
                        this.replyChannel.ModelShutdown += ChannelOnModelShutdown;
                        this.IsStarted = true;
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
            if (this.ConnectionFailed != null) this.ConnectionFailed(shutdownEventArgs.ReplyText);
        }

        /// <summary>
        /// Stops the consumer, closing the channel it uses
        /// </summary>
        public void Stop()
        {
            this.IsStarted = false;
            this.Channel.ModelShutdown -= ChannelOnModelShutdown;
            this.replyChannel.ModelShutdown -= ChannelOnModelShutdown;


             this.consumer.OnCancel();
            this.Channel.Close();
            this.Channel = null;
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
                    BasicDeliverEventArgs e = consumer.Queue.Dequeue() as BasicDeliverEventArgs;
                    if (e != null)
                    {
                        this.Channel.BasicAck(e.DeliveryTag, false);

                        this.DispatchMessage(e);
                    }
                   
                }
                catch (EndOfStreamException ex)
                {
                    // The consumer was cancelled, the model closed, or the
                    // connection went away.
                    // throw new ConnectionFailureException("Consumer has been interrupted", ex);
                    this.IsStarted = false;

                    if (this.ConnectionFailed != null) this.ConnectionFailed("Consumer has been interrupted : " + ex.Message);
                }
            }
        }

        protected abstract void DispatchMessage(BasicDeliverEventArgs basicAckEventArgs);

    }
}
