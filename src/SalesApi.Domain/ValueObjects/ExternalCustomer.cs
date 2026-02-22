using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesApi.Domain.ValueObjects
{
    /// <summary>
    /// Represents a snapshot reference to an external Customer.
    /// This value object follows the External Identity pattern.
    /// </summary>
    public sealed class ExternalCustomer : ValueObject
    {
        /// <summary>
        /// Gets the unique identifier of the customer.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets the customer's name at the time of the transaction.
        /// </summary>
        public string Name { get; }

        private ExternalCustomer() { } // Required by EF Core

        public ExternalCustomer(Guid id, string name)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Customer Id is required.");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Customer name is required.");
            }

            Id = id;
            Name = name;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Id;
            yield return Name;
        }
    }
}
