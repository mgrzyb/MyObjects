using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.AggregateService;
using MediatR;
using MyObjects.NHibernate;
using NUnit.Framework;

namespace MyObjects.Testing
{
    public class DomainModelTestFixtureBase : TestFixtureBase
    {
        private readonly Assembly modelAssembly;
        protected readonly List<object> DomainEvents = new List<object>();

        protected ContainerBuilder containerBuilder;
        private IContainer container;

        public DomainModelTestFixtureBase(Assembly modelAssembly) : base(builder => builder.AddEntitiesFromAssembly(modelAssembly))
        {
            this.modelAssembly = modelAssembly;
        }

        [SetUp]
        public void Setup()
        {
            this.containerBuilder = new ContainerBuilder();
            this.containerBuilder.Register(c =>
            {
                var componentCtx = c.Resolve<IComponentContext>();
                return new Mediator(type => componentCtx.Resolve(type));
            }).AsImplementedInterfaces().InstancePerLifetimeScope();

            this.containerBuilder.RegisterAggregateService<CommandHandler.IDependencies>();
            
            this.containerBuilder.RegisterCommandHandlersFromAssembly(this.modelAssembly, transactional: false, emitEvents: false);

            this.containerBuilder.RegisterGeneric(typeof(HandlerAdapter<,>)).AsImplementedInterfaces();
            
            this.containerBuilder.RegisterType<NHibernateSession>().AsImplementedInterfaces();
            this.containerBuilder.RegisterInstance(this).As<DomainModelTestFixtureBase>();
            this.containerBuilder.Register(c => this.OpenSession()).InstancePerLifetimeScope()
                .AsImplementedInterfaces();
            
            this.DomainEvents.Clear();
        }

        protected async Task<T> When<T>(Command<T> command)
        {            
            using (var scope = this.EnsureContainer().BeginLifetimeScope())
            {
                var session = scope.Resolve<global::NHibernate.ISession>();
                using (var transaction = session.BeginTransaction())
                {
                    this.Dummy = new Dummy(scope.Resolve<ISession>());
                    var result = await scope.Resolve<IMediator>().Send(command);
                    transaction.Commit();
                    this.CaptureDomainEvents(session);
                    return result;
                }
            }
        }

        protected async Task When(Command command)
        {
            using (var scope = this.EnsureContainer().BeginLifetimeScope())
            {
                var session = scope.Resolve<global::NHibernate.ISession>();
                using (var transaction = session.BeginTransaction())
                {
                    this.Dummy = new Dummy(scope.Resolve<ISession>());
                    await scope.Resolve<IMediator>().Send(command);
                    transaction.Commit();
                    this.CaptureDomainEvents(session);
                }
            }                        
        }

        protected async Task When<THandler>(Func<ISession, THandler, Task> when)
        {            
            using (var scope = this.EnsureContainer().BeginLifetimeScope())
            {
                var session = scope.Resolve<global::NHibernate.ISession>();
                using (var transaction = session.BeginTransaction())
                {
                    this.Dummy = new Dummy(scope.Resolve<ISession>());
                    var handler = scope.Resolve<THandler>();
                    await when(scope.Resolve<ISession>(), handler);
                    transaction.Commit();
                    this.CaptureDomainEvents(session);
                }
            }
        }

        private ILifetimeScope EnsureContainer()
        {
            if (this.container == null)
                this.container = this.containerBuilder.Build();
            return this.container;
        }
        
        private void CaptureDomainEvents(global::NHibernate.ISession session)
        {
            var sessionImplementation = session.GetSessionImplementation();

            var aggregateRoots = sessionImplementation.PersistenceContext.EntityEntries.Keys
                .OfType<AggregateRoot>();
            
            foreach (var aggregateRoot in aggregateRoots.ToList())
            {
                while (aggregateRoot.TryDequeueDomainEvent(out var e))
                {
                    this.DomainEvents.Add(e);
                }
            }
        }

        protected void GivenService<T>() where T : class
        {
            this.containerBuilder.RegisterType<T>();
        }

        protected void GivenService<T>(Func<ISession, T> service) where T : class
        {
            this.containerBuilder.Register(context => service(context.Resolve<ISession>()))
                .InstancePerLifetimeScope()
                .AsImplementedInterfaces()
                .AsSelf();           
        }
        
        public class HandlerAdapter<T, K> : IRequestHandler<T, K> where T : IRequest<K>
        {
            private readonly IHandler<T, K> handler;

            public HandlerAdapter(IHandler<T, K> handler)
            {
                this.handler = handler;
            }

            public Task<K> Handle(T request, CancellationToken cancellationToken)
            {
                return this.handler.Handle(request, cancellationToken);
            }
        }
    }
}