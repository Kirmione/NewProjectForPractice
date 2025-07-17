using RabbitMQ.Client;  
using NLog;            
using System.Text;     

// Класс для работы с RabbitMQ
public class RabbitMQService : IDisposable
{
    // Логгер для записи событий
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    // Подключение к RabbitMQ
    private IConnection connection;

    // Канал для работы с очередями
    private IModel channel;

    private readonly string hostName;

    // Свойство для доступа к каналу с проверкой подключения
    public IModel Channel
    {
        get
        {
            EnsureConnected();  // Проверяем/восстанавливаем подключение
            return channel;     
        }
    }

    public RabbitMQService(string hostName = "localhost")
    {
        this.hostName = hostName;  
        InitializeConnection();
    }

    // Метод для инициализации подключения с повторными попытками
    private void InitializeConnection()
    {
        int attempt = 0;                  
        const int maxAttempts = 20;        
        const int delayMs = 10000;         

        // Цикл попыток подключения
        while (attempt < maxAttempts)
        {
            try
            {
                var factory = new ConnectionFactory() { HostName = hostName };

                // Устанавливаем подключение
                connection = factory.CreateConnection();
                channel = connection.CreateModel();

                logger.Info("Подключение к RabbitMQ установлено");
                return;
            }
            catch (Exception ex)
            {
                attempt++;

                // Логируем ошибку
                logger.Error(ex, $"Ошибка при подключении к RabbitMQ (попытка {attempt}/{maxAttempts})");

                // Ожидание перед следующей попыткой
                if (attempt < maxAttempts)
                {
                    Thread.Sleep(delayMs);
                }
            }
        }

        // Если все попытки исчерпаны
        logger.Error("Не удалось установить подключение к RabbitMQ после нескольких попыток");
        throw new InvalidOperationException("Не удалось подключиться к RabbitMQ");
    }

    // Метод для проверки и восстановления подключения
    private void EnsureConnected()
    {
        // Проверяем состояние подключения и канала
        if (connection == null || !connection.IsOpen || channel == null || channel.IsClosed)
        {
            logger.Warn("Подключение к RabbitMQ разорвано. Пытаюсь переподключиться...");
            InitializeConnection();
        }
    }

    // Метод для объявления очереди
    public void DeclareQueue(string queueName, bool durable = true)
    {
        try
        {
            EnsureConnected();

            // Объявляем очередь с параметрами
            channel.QueueDeclare(
                queue: queueName,      
                durable: durable,     
                exclusive: false,     
                autoDelete: false,    
                arguments: null);     

            logger.Info($"Очередь {queueName} объявлена");
        }
        catch (Exception ex)
        {
            logger.Error(ex, $"Ошибка при объявлении очереди {queueName}");
            throw; 
        }
    }

    // Метод для отправки сообщения в очередь
    public void PublishMessage(string queueName, string message)
    {
        try
        {
            EnsureConnected();  

            // Преобразуем сообщение в байты
            var body = Encoding.UTF8.GetBytes(message);

            // Создаем свойства сообщения
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            // Публикуем сообщение
            channel.BasicPublish(
                exchange: "",              
                routingKey: queueName,     
                basicProperties: properties, 
                body: body);               

            logger.Info($"Сообщение отправлено в очередь {queueName}");
        }
        catch (Exception ex)
        {
            logger.Error(ex, $"Ошибка при отправке сообщения в очередь {queueName}");
            throw; 
        }
    }

    // Освобождение ресурсов
    public void Dispose()
    {
        channel?.Close();
        connection?.Close();

        logger.Info("Подключение к RabbitMQ закрыто");
    }
}