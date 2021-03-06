// ReSharper disable ArrangeThisQualifier

using System.Threading.Tasks;
using MyObjects.Testing;
using NHibernate.Proxy;
using NUnit.Framework;

namespace MyObjects.Tests
{
    public class EntityTests : TestFixtureBase
    {
        public EntityTests() 
            : base(builder => builder.AddEntity<A>())
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
                Assert.IsFalse(a.Equals(b));
                Assert.IsFalse(b.Equals(a));
                Assert.IsFalse(b.Equals(a));
            });
        }

        public class A : AggregateRoot
        {
        }
    }
}