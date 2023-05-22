using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;

namespace MyObjects.NHibernate
{
    public class NHibernateConfigurationBuilder
    {
        private readonly List<Assembly> modelAssemblies = new List<Assembly>();
        private readonly List<Type> modelTypes = new List<Type>();
        private readonly ModelMappingConventions modelMappingConventions = new ModelMappingConventions();
        private readonly IPersistenceStrategy persistenceStrategy;

        public NHibernateConfigurationBuilder(IPersistenceStrategy persistenceStrategy)
        {
            this.persistenceStrategy = persistenceStrategy;
        }

        public NHibernateConfigurationBuilder AddEntitiesFromAssemblyOf<T>()
        {
            return AddEntitiesFromAssembly(typeof(T).Assembly);
        }

        public NHibernateConfigurationBuilder AddEntitiesFromAssembly(Assembly assembly)
        {
            this.modelAssemblies.Add(assembly);
            return this;
        }

        public NHibernateConfigurationBuilder AddEntities(IEnumerable<Type> types)
        {
            this.modelTypes.AddRange(types);
            return this;
        }
        
        public NHibernateConfigurationBuilder AddEntity<T>()
        {
            this.modelTypes.Add(typeof(T));
            return this;
        }

        public NHibernateConfigurationBuilder AddUserType<T>() where T : UserType<T>
        {
            this.modelMappingConventions.AddUserType<T>();
            return this;
        }
        
        public Configuration Build()
        {
            var cfg = new Configuration();
            cfg.AddMapping(this.BuildModelMapping());
            this.persistenceStrategy.ApplyTo(cfg);
            return cfg;
        }

        private HbmMapping BuildModelMapping()
        {
            var modelMapper = new ConventionModelMapper();
            this.modelMappingConventions.ApplyTo(modelMapper);

            var modelTypes = this.modelAssemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(Entity).IsAssignableFrom(t) && t != typeof(Entity))
                .Concat(this.modelTypes);
            
            this.persistenceStrategy.ApplyTo(modelMapper);

            var mapping = modelMapper.CompileMappingFor(modelTypes);
            
            return mapping;
        }
    }
}