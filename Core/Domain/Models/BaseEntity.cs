using System;

namespace Domain.Models
{
    public abstract class BaseEntity<TKey>
    {
        public TKey Id { get; protected set; } = default!;

        public DateTimeOffset CreatedAt { get; protected set; }

        public DateTimeOffset? UpdatedAt { get; protected set; }

        protected BaseEntity()
        {
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public void MarkAsUpdated(DateTimeOffset updatedAt)
        {
            UpdatedAt = updatedAt.ToUniversalTime();
        }
    }
}
