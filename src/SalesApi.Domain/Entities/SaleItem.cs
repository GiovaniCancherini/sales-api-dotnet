using SalesApi.Domain.Common;

namespace SalesApi.Domain.Entities
{
    public class SaleItem : BaseEntity
    {
        public Guid SaleId { get; private set; }
        public string ProductId { get; private set; }
        public string ProductName { get; private set; }
        public int Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }
        public decimal DiscountPercentage { get; private set; }
        public decimal TotalPrice { get; private set; }

        public Sale Sale { get; private set; }

        protected SaleItem() { }

        public SaleItem(Guid saleId, string productId, string productName, int quantity, decimal unitPrice, decimal discountPercentage)
        {
            Id = Guid.NewGuid();
            SaleId = saleId;
            ProductId = productId;
            ProductName = productName;
            Quantity = quantity;
            UnitPrice = unitPrice;
            DiscountPercentage = discountPercentage;

            var discountValue = unitPrice * quantity * discountPercentage;
            TotalPrice = (unitPrice * quantity) - discountValue;
        }

        public SaleItem(Guid saleId, string productId, string productName, int quantity, decimal unitPrice, decimal discountPercentage, Sale sale)
        {
            Id = Guid.NewGuid();
            SaleId = saleId;
            ProductId = productId;
            ProductName = productName;
            Quantity = quantity;
            UnitPrice = unitPrice;
            DiscountPercentage = discountPercentage;

            var discountValue = unitPrice * quantity * discountPercentage;
            TotalPrice = (unitPrice * quantity) - discountValue;

            Sale = sale;
        }
    }
}
