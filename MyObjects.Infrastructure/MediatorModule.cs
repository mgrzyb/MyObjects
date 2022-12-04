using Autofac;
using MediatR;
using Module = Autofac.Module;

namespace MyObjects
{
    public class MediatorModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c =>
            {
                var componentCtx = c.Resolve<IComponentContext>();
                return new Mediator(type => componentCtx.Resolve(type));
            }).AsImplementedInterfaces().InstancePerLifetimeScope();

            builder.RegisterType<MediatorDomainEventBus>().AsImplementedInterfaces().InstancePerLifetimeScope();
        }
    }
}