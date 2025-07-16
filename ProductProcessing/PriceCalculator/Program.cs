using RabbitMQ.Client.Events;
using NLog;
using System.Text;

class Program
{
    // Инициализация логгера для записи событий
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    static void Main(string[] args)
    {
        try
        {
            logger.Info("Запуск калькулятора цен");

            // Создание и настройка сервиса RabbitMQ
            using var rabbitService = new RabbitMQService();
            // Объявление очереди для получения сообщений
            rabbitService.DeclareQueue("price_calculation");

            // Создание директории для выходных файлов
            string outputDir = Path.Combine(Directory.GetCurrentDirectory(), "output");
            // Инициализация логики калькулятора цен
            var calculator = new PriceCalculatorLogic(outputDir);

            // Создание потребителя сообщений RabbitMQ
            var consumer = new EventingBasicConsumer(rabbitService.Channel);
            // Подписка на событие получения сообщения
            consumer.Received += (model, ea) =>
            {
                try
                {
                    // Декодирование сообщения из байтов в строку
                    string message = Encoding.UTF8.GetString(ea.Body.ToArray());
                    // Обработка сообщения калькулятором
                    calculator.ProcessMessage(message);
                    // Подтверждение успешной обработки сообщения
                    rabbitService.Channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Ошибка при обработке сообщения");
                    // Отказ от обработки сообщения
                    rabbitService.Channel.BasicNack(ea.DeliveryTag, false, false);
                }
            };

            // Начало прослушивания очереди
            rabbitService.Channel.BasicConsume(
                queue: "price_calculation",
                autoAck: false,  
                consumerTag: "",
                noLocal: false,
                exclusive: false,
                arguments: null,
                consumer: consumer);

            Console.WriteLine("Калькулятор цен запущен. Нажмите Enter для остановки...");
            Console.ReadLine();
        }
        catch (Exception ex)
        {
            logger.Fatal(ex, "Критическая ошибка в работе калькулятора цен");
            Console.ReadLine();
        }
    }
}