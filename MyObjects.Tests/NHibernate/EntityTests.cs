// ReSharper disable ArrangeThisQualifier
using System.Threading.Tasks;
using MyObjects.Testing.NHibernate;
using NHibernate.Proxy;
using NUnit.Framework;

namespace MyObjects.Tests
{
    public class EntityTests : NHibernateDomainModelTestFixture
    {
        public EntityTests() : base(typeof(EntityTests).Assembly)
        {
        }

        [Test]
        public async Task Given_proxied_and_unproxied_entities_Equals_should_work_as_expected()
        {
            var aRef = await Given(session => session.Save(new A()));
            
            await Then(async session =>
            {
                var p = session.Advanced.Load<A>(aRef.Id);
                var a = (A) session.Advanced.GetSessionImplementation().PersistenceContext.Unproxy(await session.Resolve(aRef));
                
                Assert.IsTrue(p.IsProxy());
                Assert.IsFalse(a.IsProxy());

                Assert.IsTrue(a.Equals(p));
                Assert.IsTrue(a == p);
                Assert.IsTrue(p.Equals(a));
                Assert.IsTrue(p == a);
                
                var b = new A();
                Assert.IsFalse(a.Equals(b));
                Assert.IsFalse(a == b);
                Assert.IsFalse(b.Equals(a));
                Assert.IsFalse(b == a);
            });
        }

        [Test]
        public async Task When_entity_update_causes_version_mismatch_ConcurrencyExceptionIsThrown()
        {
            var aRef = await Given(session => session.Save(new A()));

            Assert.ThrowsAsync<ConcurrencyViolationException>(async () => await When(async s =>
            {
                var a = await s.Resolve(aRef);
                await s.Advanced.CreateSQLQuery("UPDATE A SET Version = 10").ExecuteUpdateAsync();
                a.Foo = "Bar";
            }));
        }

        public class A : AggregateRoot
        {
            public virtual string Foo { get; set; }
        }
    }
}