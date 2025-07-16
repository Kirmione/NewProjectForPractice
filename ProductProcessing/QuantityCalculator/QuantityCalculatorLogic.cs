using System.Text.Json;
using NLog;

public class QuantityCalculatorLogic
{
    // Логгер для записи событий
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    // Сервис для работы с RabbitMQ
    private readonly RabbitMQService rabbitService;

    public QuantityCalculatorLogic(RabbitMQService rabbitService)
    {
        this.rabbitService = rabbitService;
    }

    // Основной метод обработки сообщения
    public void ProcessMessage(string message)
    {
        try
        {
            logger.Info("Начало обработки сообщения для расчета количества");

            // Десериализация JSON в массив продуктов
            var products = JsonSerializer.Deserialize<ProductData[]>(message);
            if (products == null)
            {
                logger.Warn("Получено пустое сообщение или ошибка десериализации");
                return;
            }

            // Расчет количеств для всех продуктов
            var calculatedProducts = products.Select(CalculateQuantities).ToArray();
            // Сериализация результатов в JSON
            string resultMessage = JsonSerializer.Serialize(calculatedProducts);

            // Отправка результатов в очередь для расчета цен
            rabbitService.PublishMessage("price_calculation", resultMessage);
            logger.Info("Результаты расчета количества отправлены в очередь для расчета цен");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Ошибка при обработке сообщения");
        }
    }

    // Метод расчета количеств для одного продукта
    private CalculatedQuantityData CalculateQuantities(ProductData product)
    {
        // Создание объекта результата с копией исходных данных
        var result = new CalculatedQuantityData
        {
            Id = product.Id,
            Type = product.Type,
            Suppliers = product.Suppliers,
            SubProducts = product.SubProducts,
            Warehouses = product.Warehouses
        };

        // Блок расчета количества на складе
        try
        {
            result.WarehouseQuantity = product.Type switch
            {
                // Для обычного продукта - сумма по всем складам
                "product" => product.Warehouses?.Sum(w => w.Quantity) ?? 0,
                // Для наборов и вариантов - минимальное количество среди подпродуктов
                "set" or "variant" => product.SubProducts?
                    .Min(sp => sp.Warehouses?.Sum(w => w.Quantity) ?? 0) ?? 0,
                _ => 0  // Для неизвестного типа
            };
        }
        catch (Exception ex)
        {
            logger.Error(ex, $"Ошибка при расчете WarehouseQuantity для продукта {product.Id}");
            result.WarehouseQuantity = 0;
        }

        // Блок расчета количества у поставщиков (только для обычных продуктов)
        try
        {
            result.SupplierQuantity = product.Type == "product"
                ? product.Suppliers?.Sum(s => s.Quantity) ?? 0
                : 0;
        }
        catch (Exception ex)
        {
            logger.Error(ex, $"Ошибка при расчете SupplierQuantity для продукта {product.Id}");
            result.SupplierQuantity = 0;
        }

        // Блок расчета общего количества
        try
        {
            result.Quantity = result.WarehouseQuantity + result.SupplierQuantity;
        }
        catch (Exception ex)
        {
            logger.Error(ex, $"Ошибка при расчете Quantity для продукта {product.Id}");
            result.Quantity = 0;
        }

        return result;
    }
}