namespace ProductModels
{
    //Склад
    public class Warehouse
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        //Кол-во (на складе)
        public int Quantity { get; set; }

        //Цена (на складе)
        public decimal Price { get; set; }
    }
}
