namespace ProductModels
{
    //Класс для субпродукта (компонент сета и варианта)
    public class SubProduct
    {
        public Guid Id { get; set; } 

        //Список складов с субпродуктом
        public List<Warehouse> Warehouse { get; set; } = new();

        ////Список поставщиков с субпродуктом
        public List<Supplier> Suppliers { get; set; } = new();

    }
    //Класс для субпродукта
    public class ProductSet : ProductBase
    {
        //Список субпродуктов (в сете)
        public List<SubProduct> SubProducts { get; set; } = new();

        //Тип и вызов базового класса
        public ProductSet() : base("set") { }
    }
}
