using StockApi.Domain.Enums;

namespace StockApi.Domain.Entities
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public CategoryEnum Category { get; set; }
        public int Quantity { get; set; }

        public Product()
        {
        }

        public Product(Guid id, string name, string description, decimal price, CategoryEnum category, int quantity)
        {
            Id = id;
            Name = name;
            Description = description;
            Price = price;
            Category = category;
            Quantity = quantity;
        }
    }
}
