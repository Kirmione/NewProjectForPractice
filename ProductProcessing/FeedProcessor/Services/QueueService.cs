using RabbitMQ.Client;
using System.Text;


namespace FeedProcessor.Services
{
    public class QueueService : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _queueName;
        
        //Для создания очереди
        public QueueService(string localhost, string queueName)
        {
            _queueName = queueName;
            var factory = new ConnectionFactory() { HostName = localhost };

            //Соединение с RabbitMQ
            _connection = factory.CreateConnection();

            //Канал для работы с очередью
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false);
        }

        //Отправка сообщения
        public void SendMessage(string message) 
        { 
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: "", routingKey: _queueName, body: body);
        }

        //Освобождение ресурсов
        public void Dispose() 
        {
            _channel.Dispose();
            _connection.Dispose();
        }

    }
}
