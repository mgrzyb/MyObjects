using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MyObjects.Demo.Model;
using MyObjects.Identity;
using NHibernate.Linq;

namespace MyObjects.Demo.Functions.Infrastructure;

public class UserStore : UserStore<User>
{
    public UserStore(ISession session) : base(session)
    {
    }

    public override Task<User> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        return this.Session.Query<User>().Where(u => u.NormalizedUserName == normalizedUserName).SingleOrDefaultAsync(cancellationToken: cancellationToken);
    }}