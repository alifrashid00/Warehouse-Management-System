using Microsoft.EntityFrameworkCore;
using StockFlow.Application.Users;
using StockFlow.Domain.Users;

namespace StockFlow.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly StockFlowDbContext _dbContext;

    public UserRepository(StockFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken)
    {
        return await _dbContext.Users.AnyAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await _dbContext.Users.SingleOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _dbContext.Users.SingleOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken)
    {
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken)
    {
        user.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
