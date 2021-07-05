namespace Conventions.Domain.Services
{
    using System.Data.SQLite;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IConnectionFactory
    {
        Task<SQLiteConnection> CreateConnectionAsync(CancellationToken token = default);
    }
}
