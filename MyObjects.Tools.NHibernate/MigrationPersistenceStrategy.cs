using MyObjects.NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Mapping.ByCode;

namespace MyObjects.Tools.NHibernate
{
    public class MigrationPersistenceStrategy : IPersistenceStrategy
    {
        private readonly string connectionString;
        private readonly bool logSqlInConsole;

        public MigrationPersistenceStrategy(string connectionString, bool logSqlInConsole = false)
        {
            this.connectionString = connectionString;
            this.logSqlInConsole = logSqlInConsole;
        }

        public void ApplyTo(ConventionModelMapper modelMapper)
        {
        }

        public void ApplyTo(Configuration cfg)
        {
            cfg.DataBaseIntegration(properties =>
            {
                properties.ConnectionString = this.connectionString;
                properties.LogSqlInConsole = this.logSqlInConsole;
                properties.LogFormattedSql = true;
                properties.Driver<SqlClientDriver>();
                properties.Dialect<MsSqlAzure2008Dialect>();
                properties.SchemaAction = SchemaAutoAction.Validate;
            });
        }
    }
}