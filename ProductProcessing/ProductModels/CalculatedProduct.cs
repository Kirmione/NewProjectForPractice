namespace ProductModels
{
    internal class CalculatedProduct : ProductBase
    {
        //Кол-во склад
        public int WarehouseQuantity { get; set; }

        //Кол-во поставщик
        public int SupplierQuantity { get; set; }

        //Общее кол-во
        public int TotalQuantity { get; set; }

        //Цена склад
        public decimal WarehousePrice { get; set; }

        //Цена поставщик
        public decimal SupplierPrice { get; set; }

        //Минимальная цена
        public decimal MinPrice { get; set; }

        //Списки поставщиков, субпродуктов и складов
        public List<Supplier> Suppliers { get; set; } = new();
        public List<SubProduct> SubProducts { get; set; } = new();
        public List<Warehouse> Warehouses { get; set; } = new();

        //Тип и вызов базового класса
        public CalculatedProduct(string type) : base(type) { }
    }
}
