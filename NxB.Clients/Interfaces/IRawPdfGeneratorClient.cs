using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using NxB.Dto.PdfGeneratorApi;

namespace NxB.Clients.Interfaces
{
    public interface IRawPdfGeneratorClient : IAuthorizeClient
    {
        Task<Stream> GeneratePdfStream(GeneratePdfRawDto generatePdfRawDto);
    }
}
