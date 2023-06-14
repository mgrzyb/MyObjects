using MediatR;

namespace MyObjects.NHibernate
{
    public class TransactionalCommandHandlerDecorator<TCommand, TResult> : ICommandHandler<TCommand, TResult>
        where TCommand : IRequest<TResult>
    {
        private readonly ICommandHandler<TCommand, TResult> innerHandler;
        private readonly ITransactionRunner runner;

        public TransactionalCommandHandlerDecorator(ICommandHandler<TCommand, TResult> innerHandler, ITransactionRunner runner)
        {
            this.innerHandler = innerHandler;
            this.runner = runner;
        }

        public Task<TResult> Handle(TCommand request, CancellationToken cancellationToken)
        {
            return this.runner.RunInTransaction(() => this.innerHandler.Handle(request, cancellationToken));
        }
    }
}