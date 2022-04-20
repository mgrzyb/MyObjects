using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core;
using MediatR;
using MyObjects.NHibernate;

namespace MyObjects.Testing
{
    public static class ContainerBuilderExtensions
    {
        public static void RegisterCommandHandlersFromAssemblyOf<T>(this ContainerBuilder builder, bool emitEvents = true, bool transactional = true)
        {
            builder.RegisterCommandHandlersFromAssembly(typeof(T).Assembly, emitEvents, transactional);
        }

        public static void RegisterCommandHandlersFromAssembly(this ContainerBuilder builder, Assembly assembly, bool emitEvents = true, bool transactional = true)
        {
            string fromKey = null;

            if (transactional)
            {
                var f = fromKey;
                fromKey = Guid.NewGuid().ToString();

                builder.RegisterGenericDecorator(
                    typeof(NHibernateTransactionScopeDecorator<,>),
                    typeof(IHandler<,>),
                    fromKey,
                    f);
            }

            if (emitEvents)
            {
                var f = fromKey;
                fromKey = Guid.NewGuid().ToString();
                builder.RegisterGenericDecorator(
                    typeof(DomainEventEmittingDecorator<,>),
                    typeof(IHandler<,>),
                    fromKey,
                    toKey: f);
            }

            builder.RegisterAssemblyTypes(assembly)
                .Where(type => type.IsClosedTypeOf(typeof(ICommandHandler<,>)))
                .As(type => type.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandler<,>))
                    .Select(i => fromKey != null ? (Autofac.Core.Service) new KeyedService(fromKey, i) : new TypedService(i)));
        }


        public static void RegisterDomainEventHandlersFromAssemblyOf<T>(this ContainerBuilder builder, bool emitEvents = true)
        {
            string fromKey = null;
            if (emitEvents)
            {
                fromKey = Guid.NewGuid().ToString();
                builder.RegisterGenericDecorator(
                    typeof(DomainEventEmittingDecorator<>),
                    typeof(INotificationHandler<>),
                    fromKey);
            }

            builder.RegisterAssemblyTypes(typeof(T).Assembly)
                .Where(t => t.IsClosedTypeOf(typeof(IDomainEventHandler<>)))
                .As(type => type.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationHandler<>))
                    .Select(i => fromKey != null ? (Autofac.Core.Service)new KeyedService(fromKey, i) : new TypedService(i)));
        }
    }
 }