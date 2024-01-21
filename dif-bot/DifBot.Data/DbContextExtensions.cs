using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DifBot.Data;

public static class DbContextExtensions
{
    public static async Task<TEntity?> FindEntityAsync<TEntity, TKey>(this DbSet<TEntity> dbSet, TKey key, CancellationToken cancellationToken)
        where TEntity : class
        where TKey : struct
    {
        return await dbSet.FindAsync(new object[] { key }, cancellationToken);
    }

    public static async Task<TEntity?> FindEntityAsync<TEntity, TKey1, TKey2>(this DbSet<TEntity> dbSet, TKey1 key1, TKey2 key2, CancellationToken cancellationToken)
        where TEntity : class
        where TKey1 : struct
        where TKey2 : struct
    {
        return await dbSet.FindAsync(new object[] { key1, key2 }, cancellationToken);
    }
}
