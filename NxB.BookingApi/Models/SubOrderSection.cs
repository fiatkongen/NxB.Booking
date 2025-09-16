using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Munk.AspNetCore;
using Munk.Utils.Object;
using NxB.Domain.Common.Interfaces;

namespace NxB.BookingApi.Models
{
    public class SubOrderSection : ICreateAudit
    {
        [Required]
        [NoEmpty]
        public Guid Id { get; set; }

        [Required]
        [NoEmpty]
        public Guid CreateAuthorId { get; set; }
        public DateTime CreateDate { get; set; }

        [Required]
        [NoEmpty]
        public Guid SubOrderId { get; set; }
        public SubOrderArticle SubOrder { get; set; }

        public List<OrderLine> OrderLines { get; set; } = new();

        public int Index { get; set; }

        public SubOrderSection()
        {
            CreateDate = DateTime.Now.ToEuTimeZone();
        }

        public List<OrderLine> BuildUnRevertedOrderLines(Guid createAuthorId)
        {
            return OrderLines.OrderBy(x => x.Index).Select(x => x.UnRevertOrderLine(createAuthorId)).ToList();
        }
    }
}
