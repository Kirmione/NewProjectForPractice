namespace ProductModels
{
    //Абстрактный базовый класс (общие свойства)
    public abstract class ProductBase
    {
        public Guid Id { get; set; }

        //Тип продукта
        public string Type { get; set; }

        //Конструктор (базовый)
        protected ProductBase(string type) 
        {
            Type = type;
            Id = Guid.NewGuid();
        }
    }
}
