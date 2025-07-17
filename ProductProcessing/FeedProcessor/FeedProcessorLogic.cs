using NLog;

public class FeedProcessorLogic
{
    // Логгер для записи событий
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    // Сервис для работы с RabbitMQ
    private readonly RabbitMQService rabbitService;

    // Наблюдатель за файловой системой
    private FileSystemWatcher fileWatcher;

    // Часть для сервиса RabbitMQ
    public FeedProcessorLogic(RabbitMQService rabbitService)
    {
        this.rabbitService = rabbitService;
    }

    // Метод для начала наблюдения за директорией
    public void StartWatching(string directoryPath)
    {
        try
        {
            // Проверяем существование директории, создаем если нет
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                logger.Info($"Создана директория для наблюдения: {directoryPath}");
            }

            // Инициализируем наблюдатель за файлами
            fileWatcher = new FileSystemWatcher(directoryPath, "*.json")
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
            };

            // Подписываемся на событие создания файла
            fileWatcher.Created += OnFileCreated;

            // Включаем генерацию событий
            fileWatcher.EnableRaisingEvents = true;

            logger.Info($"Наблюдение за директорией {directoryPath} начато");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Ошибка при запуске наблюдения за директорией");
            throw;
        }
    }

    // Обработчик события создания файла
    private void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        try
        {
            logger.Info($"Обнаружен новый файл: {e.Name}");

            // Даем файлу время для завершения записи (1 секунда)
            System.Threading.Thread.Sleep(1000);

            // Читаем содержимое файла
            string fileContent = File.ReadAllText(e.FullPath);

            // Отправляем содержимое в очередь RabbitMQ
            rabbitService.PublishMessage("quantity_calculation", fileContent);

            logger.Info($"Файл {e.Name} успешно отправлен в очередь");

            // Архивируем обработанный файл в поддиректорию processed
            string processedDir = Path.Combine(Path.GetDirectoryName(e.FullPath), "processed");
            Directory.CreateDirectory(processedDir);
            File.Move(e.FullPath, Path.Combine(processedDir, e.Name));
        }
        catch (Exception ex)
        {
            logger.Error(ex, $"Ошибка при обработке файла {e.Name}");
        }
    }

    // Метод для остановки наблюдения
    public void StopWatching()
    {
        fileWatcher?.Dispose();
        logger.Info("Наблюдение за директорией остановлено");
    }
}