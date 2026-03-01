using System;
using SalesApi.Domain.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesApi.Domain.ValueObjects
{
    /// <summary>
    /// Represents a snapshot reference of an external Branch.
    /// 
    /// This Value Object follows the External Identity pattern,
    /// meaning it stores minimal branch data required by the Sale aggregate
    /// without creating a direct dependency on the Branch aggregate.
    ///
    /// It preserves historical consistency even if the original branch
    /// changes over time.
    /// </summary>
    public sealed class ExternalBranch : ValueObject
    {
        /// <summary>
        /// Gets the unique identifier of the branch.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets the branch name at the time of the transaction.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the branch code or number (if applicable).
        /// </summary>
        public string Code { get; }

        private ExternalBranch() { } // Required by EF Core

        /// <summary>
        /// Creates a new instance of <see cref="ExternalBranch"/>.
        /// </summary>
        /// <param name="id">Branch unique identifier.</param>
        /// <param name="name">Branch display name.</param>
        /// <param name="code">Branch business code.</param>
        public ExternalBranch(Guid id, string name, string code)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Branch Id is required.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Branch name is required.", nameof(name));
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Branch code is required.", nameof(code));
            }

            Id = id;
            Name = name.Trim();
            Code = code.Trim();
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Id;
            yield return Name;
            yield return Code;
        }
    }
}
