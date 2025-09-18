using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Munk.Utils.Object;
using NxB.Domain.Common.Enums;

namespace NxB.Dto.JobApi
{
    public class CreateAndSendVoucherDto : CreateAndSendDocumentDto
    {
        public Guid? ExistingVoucherId { get; set; }
        public bool SkipCreateIfVoucherExists { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime VoucherDate { get; set; }
        public decimal DueAmount { get; set; }

        [NoEmpty]
        public Guid AccountId { get; set; }
    }
}
