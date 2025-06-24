namespace ProductModels
{
    //Класс для простого продукта
    public class Product : ProductBase
    {
        //Список поставщиков
        public List<Supplier> Suppliers { get; set; } = new();

        //Список складов
        public List<Warehouse> Warehouses { get; set;} = new();

        //Тип и вызов базового класса
        public Product() : base("product") { }
    }

}


