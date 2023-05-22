using Autofac;
using MediatR;
using MyObjects.Infrastructure;
using Module = Autofac.Module;

namespace MyObjects
{
    public class MyObjectsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c =>
            {
                var componentCtx = c.Resolve<IComponentContext>();
                return new Mediator(type => componentCtx.Resolve(type));
            }).AsImplementedInterfaces().InstancePerLifetimeScope();
            
            builder.RegisterDecorator<EventEmittingSessionDecorator, ISession>();
            builder.RegisterGenericDecorator(typeof(EventEmittingSessionDecorator<>), typeof(ISession<>));        }
    }
}