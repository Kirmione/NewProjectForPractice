using NLog;

class Program
{
    // Инициализация логгера для текущего класса
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    // Главная точка входа в приложение
    static void Main(string[] args)
    {
        try
        {
            logger.Info("Запуск обработчика фида");

            // Создаем и инициализируем сервис для работы с RabbitMQ
            using var rabbitService = new RabbitMQService();

            // Объявляем очередь для расчетов количества товара
            rabbitService.DeclareQueue("quantity_calculation");

            var processor = new FeedProcessorLogic(rabbitService);

            // Запускаем наблюдение за папкой "feed_input" в текущей директории
            processor.StartWatching(Path.Combine(Directory.GetCurrentDirectory(), "feed_input"));

            Console.WriteLine("Обработчик фида запущен. Нажмите Enter для остановки...");
            Console.ReadLine();
            processor.StopWatching();
        }
        catch (Exception ex)
        {
            logger.Fatal(ex, "Критическая ошибка в работе обработчика фида");
            Console.ReadLine();
        }
    }
}