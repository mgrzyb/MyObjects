using MediatR;

namespace MyObjects.NHibernate;

public abstract class CommandHandler<TCommand> : CommandHandlerWithAdvancedSession<global::NHibernate.ISession, TCommand> where TCommand : IRequest
{
    protected CommandHandler(IDependencies dependencies) : base(dependencies)
    {
    }
}

public abstract class CommandHandler<TCommand, TResponse> : CommandHandlerWithAdvancedSession<global::NHibernate.ISession, TResponse, TCommand> where TCommand : IRequest<TResponse>
{
    protected CommandHandler(IDependencies dependencies) : base(dependencies)
    {
    }
}