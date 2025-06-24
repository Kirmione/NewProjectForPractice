namespace ProductModels
{
    //Класс для вариантов продукта
    public class ProductVariant : ProductBase   
    {
        //Список вариантов с использование субпродуктов
        public List<SubProduct> SubProducts { get; set; } = new();

        //Тип и вызов базового класса
        public ProductVariant() : base("variant") { }


    }
}
