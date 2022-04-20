using MyObjects.NHibernate;

namespace MyObjects.Testing
{
    public class Dummy
    {
        public readonly ISession Session;

        public Dummy(ISession session)
        {
            this.Session = session;
        }
    }
}