using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Munk.AspNetCore.Sql;

namespace NxB.BookingApi.Infrastructure
{
    public class TallyDbConnectionFactory : IDbConnectionFactory
    {
        public SqlConnection Create()
        {
//            return new SqlConnection("Server=tcp:tcon.next-stay-booking.dk,1433;Initial Catalog=TCon;Persist Security Info=False;User ID=sa;Password=Pinkfiat1;MultipleActiveResultSets=True;Encrypt=False;TrustServerCertificate=False;Connection Timeout=60;");
            return new SqlConnection("Server=tcp:mssql-prod-u32541.vm.elestio.app,18698;Initial Catalog=TCon;Persist Security Info=False;User ID=sa;Password=XcAcIHMVPW-b16S-8wr2X6l3!2P_G;MultipleActiveResultSets=True;Encrypt=False;TrustServerCertificate=False;Connection Timeout=60;");
        }
    }
}