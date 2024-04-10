using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity;

namespace MyObjects.Identity;

public abstract class UserStore<T> : IUserStore<T> where T : AggregateRoot, IIdentityUser
{
    protected readonly ISession Session;

    protected UserStore(ISession session)
    {
        this.Session = session;
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
        await this.Session.Save(user);
        return IdentityResult.Success;
    }

    public Task<IdentityResult> UpdateAsync(T user, CancellationToken cancellationToken)
    {
        return Task.FromResult(IdentityResult.Success);
    }

    public async Task<IdentityResult> DeleteAsync(T user, CancellationToken cancellationToken)
    {
        await this.Session.Delete(user);
        return IdentityResult.Success;
    }

    public Task<T> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        #pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
        return this.Session.TryResolve(new Reference<T>(int.Parse(userId)));
        #pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
    }

    public abstract Task<T> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken);
}