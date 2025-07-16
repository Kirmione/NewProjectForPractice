using RabbitMQ.Client.Events;
using NLog;
using System.Text;

class Program
{
    // Инициализация логгера
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    static void Main(string[] args)
    {
        try
        {
            logger.Info("Запуск калькулятора количества");

            // Создание и настройка сервиса RabbitMQ
            using var rabbitService = new RabbitMQService();
            // Объявление очередей для получения сообщений и отправки результатов
            rabbitService.DeclareQueue("quantity_calculation");
            rabbitService.DeclareQueue("price_calculation");

            // Инициализация логики калькулятора количества
            var calculator = new QuantityCalculatorLogic(rabbitService);

            // Создание потребителя сообщений RabbitMQ
            var consumer = new EventingBasicConsumer(rabbitService.Channel);
            // Событие получения сообщения
            consumer.Received += (model, ea) =>
            {
                try
                {
                    // Декодирование и обработка сообщения
                    string message = Encoding.UTF8.GetString(ea.Body.ToArray());
                    calculator.ProcessMessage(message);
                    // Подтверждение успешной обработки
                    rabbitService.Channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Ошибка при обработке сообщения");
                    rabbitService.Channel.BasicNack(ea.DeliveryTag, false, false);
                }
            };

            // Начало прослушивания очереди
            rabbitService.Channel.BasicConsume(
                queue: "quantity_calculation",
                autoAck: false,
                consumerTag: "",
                noLocal: false,
                exclusive: false,
                arguments: null,
                consumer: consumer);

            Console.WriteLine("Калькулятор количества запущен. Нажмите Enter для остановки...");
            Console.ReadLine();
        }
        catch (Exception ex)
        {
            logger.Fatal(ex, "Критическая ошибка в работе калькулятора количества");
            Console.ReadLine();
        }
    }
}