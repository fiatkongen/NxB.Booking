using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Munk.Utils.Object;
using NxB.Domain.Common.Enums;

namespace NxB.Dto.JobApi
{
    public class CreateAndSendDocumentDto
    {
        public Guid? DocumentTemplateId { get; set; }
        public Guid? DocumentTemplateSmsId { get; set; }
        public string EmailPrefix { get; set; }

        public string Languages { get; set; }
        public Guid? SaveId { get; set; }

        public Guid? CustomerId { get; set; }   //must provide customerId or accountId
        public Guid? AccountId { get; set; }

        [NoEmpty]
        public int FriendlyOrderId { get; set; }
        public Guid OrderId { get; set; }

        public Guid? BatchItemId { get; set; }

        public List<Guid> ForceHideNameForResourceIds { get; set; } = new();

        public PdfGenerationPriority PdfGenerationPriority { get; set; } = PdfGenerationPriority.Default;
    }
}
