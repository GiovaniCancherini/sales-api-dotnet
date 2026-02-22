using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesApi.Domain.ValueObjects
{
    /// <summary>
    /// Represents a snapshot reference to an external Product.
    /// </summary>
    public sealed class ExternalProduct : ValueObject
    {
        /// <summary>
        /// Gets the unique identifier of the product.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets the product name at the time of sale.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the product SKU.
        /// </summary>
        public string SKU { get; }

        public ExternalProduct(Guid id, string name, string sku)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Product Id is required.");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Product name is required.");
            }

            if (string.IsNullOrWhiteSpace(sku))
            {
                throw new ArgumentException("Product SKU is required.");
            }

            Id = id;
            Name = name;
            SKU = sku;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Id;
            yield return Name;
            yield return SKU;
        }
    }
}
