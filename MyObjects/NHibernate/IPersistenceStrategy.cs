using NHibernate.Cfg;
using NHibernate.Mapping.ByCode;

namespace MyObjects.NHibernate
{
    public interface IPersistenceStrategy
    {
        void ApplyTo(ConventionModelMapper modelMapper);
        void ApplyTo(Configuration configuration);
    }
}