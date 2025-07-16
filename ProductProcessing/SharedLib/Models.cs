// Базовый класс данных продукта
public class ProductData
{
    public string Id { get; set; } // Идентификатор продукта
    public string Type { get; set; } // Тип продукта (product/set/variant)
    public Supplier[] Suppliers { get; set; } // Массив поставщиков
    public SubProduct[] SubProducts { get; set; } // Массив подпродуктов
    public Warehouse[] Warehouses { get; set; } // Массив складов
}

// Класс с рассчитанными количествами
public class CalculatedQuantityData : ProductData
{
    public int WarehouseQuantity { get; set; } // Количество на складе
    public int SupplierQuantity { get; set; } // Количество у поставщиков
    public int Quantity { get; set; } // Общее количество
}

// Класс с рассчитанными ценами
public class CalculatedPriceData : CalculatedQuantityData
{
    public decimal WarehousePrice { get; set; } // Цена на складе
    public decimal SupplierPrice { get; set; } // Цена у поставщиков
    public decimal MinPrice { get; set; } // Минимальная цена
}

// Класс поставщика
public class Supplier
{
    public string Id { get; set; } // Идентификатор поставщика
    public int Quantity { get; set; } // Доступное количество
    public decimal Price { get; set; } // Цена
}

// Класс подпродукта
public class SubProduct
{
    public string Id { get; set; } // Идентификатор подпродукта
    public Supplier[] Suppliers { get; set; } // Поставщики подпродукта
    public Warehouse[] Warehouses { get; set; } // Склады подпродукта
}

// Класс склада
public class Warehouse
{
    public string Id { get; set; } // Идентификатор склада
    public int Quantity { get; set; } // Количество на складе
    public decimal Price { get; set; } // Цена на складе
}