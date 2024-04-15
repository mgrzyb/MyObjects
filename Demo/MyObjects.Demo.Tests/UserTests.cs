using NUnit.Framework;
using System.Threading.Tasks;
using MyObjects.Identity;
using MyObjects.Identity.Events;

using MyObjects.Demo.Model.Identity;
using MyObjects.Demo.Model.Identity.Commands;

using MyObjects.Testing;
using MyObjects.Tests;

namespace MyObjects.Demo.UnitTests
{
    public class UserTests : DomainModelTestFixture
    {
        [Test]
        public async Task CreateUser()
        {
            var result = await When(new CreateUser(new UsernameAndPasswordCredentials("user", "password")));

            await Then(async session =>
            {
                Assert.That(result.IsT0);
                var user = await session.Resolve(result.AsT0);
                Assert.That(user.Identity.Username, Is.EqualTo("user"));
            });
        }

        [Test]
        public async Task Given_UserIdentity_AuthenticateUser_with_correct_credentials_returns_User_ref()
        {
            var userRef = await Given(async s =>
            {
                var identity = new UserIdentity(new UsernameAndPasswordCredentials("user", "password"));
                await s.Save(identity);
                return await s.Save(new User(identity));
            });

            var authenticatedUserRef = await When(new AuthenticateUser(new UsernameAndPasswordCredentials("user", "password")));

            Assert.That(authenticatedUserRef, Is.EqualTo(userRef));
            Assert.That(this.DomainEvents, Has.One.Event<UserIdentityVerified>());
        }

        [Test]
        public async Task Given_UserIdentity_AuthenticateUser_with_incorrect_username_returns_null()
        {
            var userRef = await Given(async s =>
            {
                var identity = new UserIdentity(new UsernameAndPasswordCredentials("user", "password"));
                await s.Save(identity);
                return await s.Save(new User(identity));
            });

            var authenticatedUserRef = await When(new AuthenticateUser(new UsernameAndPasswordCredentials("wrong user", "password")));

            Assert.That(authenticatedUserRef, Is.Null);
        }

        [Test]
        public async Task Given_UserIdentity_AuthenticateUser_with_incorrect_password_returns_null()
        {
            var userRef = await Given(async s =>
            {
                var identity = new UserIdentity(new UsernameAndPasswordCredentials("user", "password"));
                await s.Save(identity);
                return await s.Save(new User(identity));
            });

            var authenticatedUserRef = await When(new AuthenticateUser(new UsernameAndPasswordCredentials("user", "wrong password")));

            Assert.That(authenticatedUserRef, Is.Null);
            Assert.That(this.DomainEvents, Has.One.Event<UserIdentityVerificationFailed>());
        }

    }
}
