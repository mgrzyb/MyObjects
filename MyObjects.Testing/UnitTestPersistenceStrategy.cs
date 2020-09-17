using System;
using Microsoft.Data.Sqlite;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Mapping.ByCode;

namespace MyObjects.Testing
{
    public class UnitTestPersistenceStrategy : IPersistenceStrategy
    {
        public void ApplyTo(ConventionModelMapper modelMapper)
        {
            modelMapper.BeforeMapClass += (inspector, type, customizer) =>
            {
                customizer.DynamicUpdate(true);
            };
            
            modelMapper.BeforeMapProperty += (inspector, member, customizer) =>
            {
                var memberType = member.LocalMember.GetPropertyOrFieldType();

                if (memberType == typeof(DateTimeOffset) || memberType == typeof(DateTimeOffset?))
                {
                    customizer.Type<SqliteDateTimeOffsetType>();
                }
            };
        }

        public void ApplyTo(Configuration cfg)
        {
            var sqliteConnectionString = new SqliteConnectionStringBuilder()
            {
                DataSource = $"UT-{Guid.NewGuid().ToString()}.sqlite",
                Mode = SqliteOpenMode.Memory,
                Cache = SqliteCacheMode.Shared,                
            };

            cfg.DataBaseIntegration(properties =>
            {
                properties.LogFormattedSql = true;
                properties.LogSqlInConsole = true;
                properties.Driver<SQLite20Driver>();
                properties.Dialect<SQLiteDialect>();
                properties.SchemaAction = SchemaAutoAction.Create;
                properties.ConnectionString = sqliteConnectionString.ToString();
            });
        }
    }
}