namespace ProductModels
{
    //Поставщики
    public class Supplier
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        //Кол-во товара (у поставщика)
        public int Quantity { get; set; }

        //Цена (у поставщика)
        public decimal Price { get; set; }
    }
}
