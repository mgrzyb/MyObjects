using NHibernate.Cfg;
using NHibernate.Mapping.ByCode;

namespace MyObjects
{
    public interface IPersistenceStrategy
    {
        void ApplyTo(ConventionModelMapper modelMapper);
        void ApplyTo(Configuration configuration);
    }
}