using SalesApi.Common.Security;
using SalesApi.Domain.Common;

namespace SalesApi.Domain.Entities
{
    public class Sale : BaseEntity
    {
        public string SaleNumber { get; private set; }
        public DateTime Date { get; private set; }
        public string CustomerId { get; private set; }
        public string CustomerName { get; private set; }
        public string BranchId { get; private set; }
        public string BranchName { get; private set; }
        public bool IsCanceled { get; private set; }
        public decimal TotalAmount { get; set; }

        public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();

        protected Sale() { }

        public Sale(string saleNumber, string customerId, string customerName, string branchId, string branchName)
        {
            Id = Guid.NewGuid();
            SaleNumber = saleNumber;
            Date = DateTime.UtcNow;
            CustomerId = customerId;
            CustomerName = customerName;
            BranchId = branchId;
            BranchName = branchName;
            IsCanceled = false;
            TotalAmount = 0;
        }

        public void AddItem(Guid saleId, string productId, string productName, int quantity, decimal unitPrice)
        {
            if (quantity > 20)
            {
                throw new InvalidOperationException("Cannot sell more than 20 identical items.");
            }
            var discountPercentage = CalculateDiscount(quantity);
            var item = new SaleItem(saleId, productId, productName, quantity, unitPrice, discountPercentage);

            Items.Add(item);
            RecalculateTotal();
        }

        public void Cancel()
        {
            if (IsCanceled)
            {
                return;
            }
            IsCanceled = true;
        }

        private void RecalculateTotal()
        {
            TotalAmount = Items.Sum(i => i.TotalPrice);
        }

        private decimal CalculateDiscount(int quantity)
        {
            if (quantity >= 10)
            {
                return 0.20m;
            }
            if (quantity >= 4)
            {
                return 0.10m;
            }
            return 0;
        }
    }
}
