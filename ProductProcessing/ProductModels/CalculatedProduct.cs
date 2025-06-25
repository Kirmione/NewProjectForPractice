namespace ProductModels
{
    //Рассчет кол-ва
    public class CalculatedQuantityProduct : ProductBase
    {
        //Кол-во склад
        public int WarehouseQuantity { get; set; }

        //Кол-во поставщик
        public int SupplierQuantity { get; set; }

        //Общее кол-во
        public int TotalQuantity { get; set; }

        //Списки поставщиков, субпродуктов и складов
        public List<Supplier> Suppliers { get; set; } = new();
        public List<SubProduct> SubProducts { get; set; } = new();
        public List<Warehouse> Warehouses { get; set; } = new();

        //Тип и вызов базового класса
        public CalculatedQuantityProduct(string type) : base(type) { }
    }

    //Расчет цены
    public class CalculatedPriceProduct : CalculatedQuantityProduct
    {
        //Цена склад
        public decimal WarehousePrice { get; set; }

        //Цена поставщик
        public decimal SupplierPrice { get; set; }

        //Минимальная цена
        public decimal MinPrice { get; set; }

        //Тип и вызов базового класса
        public CalculatedPriceProduct(string type) : base(type) { }
    }
}
