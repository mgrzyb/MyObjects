using Autofac;
using MyObjects.Infrastructure;

namespace MyObjects.Demo.Functions;

public static class EventBusSetupExtensions
{
    public static MyObjectsRegistration UseDomainEventBus(this MyObjectsRegistration registration)
    {
        registration.Builder.RegisterType<MediatorDomainEventBus>().AsImplementedInterfaces().InstancePerLifetimeScope();
        return registration;
    }
}