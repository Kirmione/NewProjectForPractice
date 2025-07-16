using System.Text.Json;
using NLog;

// Класс для расчета цен продуктов
public class PriceCalculatorLogic
{
    // Логгер для записи событий
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    // Директория для сохранения результатов
    private readonly string outputDirectory;

    public PriceCalculatorLogic(string outputDirectory)
    {
        this.outputDirectory = outputDirectory;
        Directory.CreateDirectory(outputDirectory); // Создаем директорию, если ее нет
    }

    // Основной метод обработки сообщения с данными о продуктах
    public void ProcessMessage(string message)
    {
        try
        {
            logger.Info("Начало обработки сообщения для расчета цен");

            // Десериализация JSON в массив продуктов
            var products = JsonSerializer.Deserialize<CalculatedQuantityData[]>(message);

            if (products == null)
            {
                logger.Warn("Получено пустое сообщение или ошибка десериализации");
                return;
            }

            // Расчет цен для всех продуктов и сохранение результатов
            var calculatedProducts = products.Select(CalculatePrices).ToArray();
            SaveResultsToFile(calculatedProducts);

            logger.Info("Результаты расчета цен сохранены в файл");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Ошибка при обработке сообщения");
        }
    }

    // Метод расчета цен для одного продукта
    private CalculatedPriceData CalculatePrices(CalculatedQuantityData product)
    {
        // Создаем объект результата с копией всех исходных данных
        var result = new CalculatedPriceData
        {
            Id = product.Id,
            Type = product.Type,
            WarehouseQuantity = product.WarehouseQuantity,
            SupplierQuantity = product.SupplierQuantity,
            Quantity = product.Quantity,
            Suppliers = product.Suppliers,
            SubProducts = product.SubProducts,
            Warehouses = product.Warehouses
        };

        // Блок расчета цены на складе с обработкой разных типов продуктов
        try
        {
            result.WarehousePrice = product.Type switch
            {
                // Для обычного продукта - средняя цена по складам
                "product" => product.Warehouses?.Any() == true
                    ? product.Warehouses.Average(w => w.Price)
                    : 0,

                // Для набора - сумма цен подпродуктов
                "set" => product.SubProducts?
                    .Sum(sp => sp.Warehouses?.Any() == true
                        ? sp.Warehouses.Average(w => w.Price)
                        : 0) ?? 0,

                // Для варианта - минимальная цена среди подпродуктов
                "variant" => product.SubProducts?
                    .Min(sp => sp.Warehouses?.Any() == true
                        ? sp.Warehouses.Average(w => w.Price)
                        : decimal.MaxValue) ?? 0,

                _ => 0 // Для неизвестного типа
            };

            if (result.WarehousePrice == decimal.MaxValue)
                result.WarehousePrice = 0;
        }
        catch (Exception ex)
        {
            logger.Error(ex, $"Ошибка при расчете WarehousePrice для продукта {product.Id}");
            result.WarehousePrice = 0;
        }

        // Блок расчета цены от поставщика (только для обычных продуктов)
        try
        {
            result.SupplierPrice = product.Type == "product" && product.Suppliers?.Any() == true
                ? product.Suppliers.Min(s => s.Price) // Минимальная цена поставщика
                : 0; // Для наборов и вариантов
        }
        catch (Exception ex)
        {
            logger.Error(ex, $"Ошибка при расчете SupplierPrice для продукта {product.Id}");
            result.SupplierPrice = 0;
        }

        // Блок расчета минимальной цены
        try
        {
            var pricesToCompare = new[] { result.WarehousePrice, result.SupplierPrice }
                .Where(p => p > 0) // Исключаем нулевые цены
                .ToArray();

            result.MinPrice = pricesToCompare.Any() ? pricesToCompare.Min() : 0;
        }
        catch (Exception ex)
        {
            logger.Error(ex, $"Ошибка при расчете MinPrice для продукта {product.Id}");
            result.MinPrice = 0;
        }

        return result;
    }

    // Метод сохранения результатов в JSON-файл
    private void SaveResultsToFile(CalculatedPriceData[] products)
    {
        try
        {
            // Генерация имени файла с временем
            string fileName = $"results_{DateTime.Now:yyyyMMddHHmmssfff}.json";
            string filePath = Path.Combine(outputDirectory, fileName);

            // Настройки сериализации
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(products, options);

            // Запись в файл
            File.WriteAllText(filePath, json);
            logger.Info($"Результаты сохранены в файл: {filePath}");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Ошибка при сохранении результатов в файл");
        }
    }
}