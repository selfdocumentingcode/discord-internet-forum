using Microsoft.EntityFrameworkCore;

namespace DifBot.Data.Repositories;

public class ForumRepositoryFactory
{
    private readonly IDbContextFactory<BotDbContext> _dbContextFactory;

    public ForumRepositoryFactory(IDbContextFactory<BotDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public ForumRepository CreateRepository()
    {
        var dbContext = _dbContextFactory.CreateDbContext();

        return new ForumRepository(dbContext);
    }
}
