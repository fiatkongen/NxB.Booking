using Microsoft.Data.SqlClient;

namespace Munk.AspNetCore.Sql
{
    public interface IDbConnectionFactory
    {
        SqlConnection Create();
    }
}