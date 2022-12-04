using MediatR;

namespace MyObjects.NHibernate;

public abstract class DomainEventHandler<TEvent> : DomainEventHandlerWithAdvancedSession<global::NHibernate.ISession, TEvent> where TEvent : INotification
{
    protected DomainEventHandler(IDependencies dependencies) : base(dependencies)
    {
    }
}