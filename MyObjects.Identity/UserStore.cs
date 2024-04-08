using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity;

namespace MyObjects.Identity;

class UserStore<T> : IUserStore<T> where T : AggregateRoot, IIdentityUser
{
    private readonly ISession session;
    private Expression<Func<T,bool>> normalizedUsernameAccessor;

    public UserStore(ISession session)
    {
        this.session = session;
    }
    
    public void Dispose()
    {
    }

    public Task<string> GetUserIdAsync(T user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.Id.ToString());
    }

    public Task<string?> GetUserNameAsync(T user, CancellationToken cancellationToken)
    {
        return Task.FromResult<string?>(user.UserName);
    }

    public Task SetUserNameAsync(T user, string? userName, CancellationToken cancellationToken)
    {
        user.UserName = userName;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedUserNameAsync(T user, CancellationToken cancellationToken)
    {
        return Task.FromResult<string?>(user.NormalizedUserName);
    }

    public Task SetNormalizedUserNameAsync(T user, string normalizedName, CancellationToken cancellationToken)
    {
        user.NormalizedUserName = normalizedName;
        return Task.CompletedTask;
    }

    public async Task<IdentityResult> CreateAsync(T user, CancellationToken cancellationToken)
    {
        await this.session.Save(user);
        return IdentityResult.Success;
    }

    public Task<IdentityResult> UpdateAsync(T user, CancellationToken cancellationToken)
    {
        return Task.FromResult(IdentityResult.Success);
    }

    public async Task<IdentityResult> DeleteAsync(T user, CancellationToken cancellationToken)
    {
        await this.session.Delete(user);
        return IdentityResult.Success;
    }

    public Task<T?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        return this.session.TryResolve(new Reference<T>(int.Parse(userId)));
    }

    public async Task<T?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        return await this.session.Query<T>().Where(u => u.NormalizedUserName == normalizedUserName).SingleOrDefaultAsync();
    }
}