using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Text;
using NxB.Domain.Common.Interfaces;

namespace NxB.Domain.Common.Model
{
    public class CompositeKey: ICompositeKey
    {
        public Guid Id { get; }
        public long FriendlyId { get; }

        private CompositeKey(Guid id, long friendlyId)
        {
            Id = id;
            FriendlyId = friendlyId;
        }

        public static CompositeKey Create(Guid? id, long? friendlyId)
        {
            if (id == null && friendlyId == null)
            {
                return null;
            }
            else if (id.HasValue && friendlyId.HasValue)
            {
                return new CompositeKey(id.Value, friendlyId.Value);
            }

            throw id == null
                ? new ArgumentNullException("Cannot create composite key when id is missing")
                : new ArgumentNullException("Cannot create composite key when friendly is missing");
        }

    }
}
