using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NxB.Domain.Common.Interfaces;
using NxB.Clients.Interfaces;
using NxB.Dto.PdfGeneratorApi;

namespace NxB.Clients
{
    public class RawPdfGeneratorClient : NxBAdministratorClient, IRawPdfGeneratorClient
    {
        public RawPdfGeneratorClient(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public async Task<Stream> GeneratePdfStream(GeneratePdfRawDto generatePdfRawDto)
        {
            var url = $"/NxB.Services.App/NxB.PdfGeneratorApi/raw";
            var pdfStream = await this.PostStream(url, generatePdfRawDto);
            return pdfStream;
        }
    }
}
