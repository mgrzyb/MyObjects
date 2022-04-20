using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NHibernate.Mapping.ByCode;

namespace MyObjects.NHibernate
{
    class ModelMappingConventions
    {
        private readonly Dictionary<Type, Type> userTypes = new Dictionary<Type, Type>();

        public void AddUserType<T>() where T : UserType<T>
        {
            this.userTypes.Add(typeof(T).GetProperty(nameof(UserType<T>.Instance)).PropertyType, typeof(T));
        }

        public void ApplyTo(ConventionModelMapper modelMapper)
        {
            modelMapper.BeforeMapClass += (inspector, type, customizer) =>
            {
                customizer.Table($"`{type.Name}`");

                customizer.Id(mapper =>
                {
                    mapper.Generator(Generators.Identity);
                    mapper.Access(Accessor.Field);
                });

                customizer.Version(type.GetProperty(nameof(Entity.Version)), mapper => { });
            };

            modelMapper.BeforeMapManyToOne += (inspector, member, customizer) =>
            {
                customizer.Column(mapper => mapper.Name(member.ToColumnName() + "Id"));
                customizer.ForeignKey(
                    $"FK_{member.GetContainerEntity(inspector).Name.ToUpper()}_{member.LocalMember.Name.ToUpper()}");
                customizer.Index(
                    ($"IDX_{member.GetContainerEntity(inspector).Name.ToUpper()}_{member.LocalMember.Name.ToUpper()}"));

                if (member.LocalMember.GetPropertyOrFieldType().IsSubclassOf(typeof(AggregateRoot)) == false)
                    customizer.Cascade(Cascade.Persist);

                var uniqueAttribute = member.LocalMember.GetCustomAttribute<UniqueAttribute>();
                if (uniqueAttribute != null)
                {
                    if (string.IsNullOrEmpty(uniqueAttribute.Key) == false)
                    {
                        customizer.UniqueKey($"UQ_{uniqueAttribute.Key}");
                    }
                    else
                    {
                        customizer.Unique(true);
                    }
                }
            };

            modelMapper.BeforeMapManyToMany += (inspector, member, customizer) =>
            {
                customizer.Column(mapper => mapper.Name(member.ToColumnName().TrimEnd('s') + "Id"));
                customizer.ForeignKey(
                    $"FK_{member.GetContainerEntity(inspector).Name.ToUpper()}_{member.LocalMember.Name.ToUpper()}_{member.ToColumnName().TrimEnd('s') + "Id"}");
            };

            modelMapper.BeforeMapProperty += (inspector, member, customizer) =>
            {
                var memberType = member.LocalMember.GetPropertyOrFieldType();
                var uniqueAttribute = member.LocalMember.GetCustomAttribute<UniqueAttribute>();
                if (uniqueAttribute != null)
                {
                    if (string.IsNullOrEmpty(uniqueAttribute.Key) == false)
                    {
                        customizer.UniqueKey($"UQ_{uniqueAttribute.Key}");
                    }
                    else
                    {
                        customizer.Unique(true);
                    }
                }

                var indexAttribute = member.LocalMember.GetCustomAttribute<IndexAttribute>();
                if (indexAttribute != null)
                {
                    customizer.Index(
                        $"IDX_{member.GetContainerEntity(inspector).Name.ToUpper()}_{member.ToColumnName().ToUpper()}");
                }

                if (this.userTypes.TryGetValue(memberType, out var userType))
                {
                    customizer.Type(userType, null);
                }

                if (memberType == typeof(byte[]))
                {
                    customizer.Length(int.MaxValue);
                }
            };

            modelMapper.BeforeMapJoinedSubclass += (inspector, type, customizer) =>
            {
                customizer.Key(mapper => mapper.Column(columnMapper => columnMapper.Name($"{type.BaseType.Name}Id")));
                customizer.Key(mapper => mapper.ForeignKey($"FK_{type.Name.ToUpper()}_{type.BaseType.Name.ToUpper()}"));
            };

            modelMapper.BeforeMapBag += OnBeforeMapCollection;
            modelMapper.BeforeMapList += OnBeforeMapCollection;
            modelMapper.BeforeMapList += (inspector, member, customizer) =>
            {
                customizer.Inverse(false);
                customizer.Index(m => m.Column("`Index`"));
            };
            modelMapper.BeforeMapSet += OnBeforeMapCollection;
            modelMapper.BeforeMapIdBag += OnBeforeMapCollection;

            modelMapper.BeforeMapElement += (inspector, member, customizer) => { customizer.Column("Value"); };

            modelMapper.IsEntity((type, b) => typeof(Entity).IsAssignableFrom(type) && type != typeof(AggregateRoot));
            modelMapper.IsRootEntity((type, b) =>
                type.BaseType == typeof(Entity) || type.BaseType == typeof(AggregateRoot));
            modelMapper.IsVersion((property, b) => property.Name == nameof(Entity.Version));
            modelMapper.IsProperty((info, declared) => declared || this.userTypes.ContainsKey(info.GetPropertyOrFieldType()));
            modelMapper.IsOneToMany((memberInfo, declared) =>
            {
                if (declared)
                    return true;

                var collectionType = memberInfo.GetPropertyOrFieldType();

                return collectionType.IsEnumerableOf<Entity>() &&
                       collectionType.IsEnumerableOf<AggregateRoot>() == false;
            });

            modelMapper.IsManyToMany((memberInfo, declared) =>
                declared || memberInfo.GetPropertyOrFieldType().IsEnumerableOf<AggregateRoot>());
            modelMapper.IsList((info, declared) =>
            {
                if (declared)
                    return true;

                var backingField = info.DeclaringType.GetField(info.Name,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase);

                var propertyType = backingField?.GetPropertyOrFieldType() ?? info.GetPropertyOrFieldType();
                var isGenericList = propertyType.IsGenericType &&
                                    propertyType.GetGenericTypeDefinition() == typeof(IList<>);
                return isGenericList;
            });
        }

        private static void OnBeforeMapCollection(IModelInspector inspector, PropertyPath member,
            ICollectionPropertiesMapper customizer)
        {
            customizer.Key(mapper =>
            {
                mapper.Column(columnMapper => columnMapper.Name(member.GetContainerEntity(inspector).Name + "Id"));
                if (inspector.IsManyToManyItem(member.LocalMember))
                {
                    mapper.ForeignKey(
                        $"FK_{member.GetContainerEntity(inspector).Name.ToUpper()}_{member.LocalMember.Name.ToUpper()}_{member.GetContainerEntity(inspector).Name + "Id"}");
                }
                else
                {
                    var a = member.LocalMember.GetPropertyOrFieldType();
                    if (a.IsEnumerable(out var elementType))
                    {
                        mapper.ForeignKey(
                            $"FK_{elementType.Name.ToUpper()}_{member.GetContainerEntity(inspector).Name.ToUpper()}");
                    }
                }
            });

            if (inspector.IsManyToManyItem(member.LocalMember))
            {
                customizer.Table(member.LocalMember.DeclaringType.Name + "_" + member.LocalMember.Name);
            }
            else
            {
                if (member.LocalMember.GetPropertyOrFieldType().IsEnumerable(out var elementType) &&
                    typeof(Entity).IsAssignableFrom(elementType))
                {
                    var parentRef = elementType
                        .GetProperties()
                        .Single(p => p.PropertyType == member.GetContainerEntity(inspector));

                    customizer.Key(o => o.Column($"{parentRef.Name}Id"));
                    customizer.Inverse(true);
                }

                customizer.Cascade(Cascade.All | Cascade.DeleteOrphans);
            }
        }
    }
}