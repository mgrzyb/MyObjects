using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace MyObjects
{
    public class Command : IRequest
    {
        public abstract class Handler<TCommand> : CommandHandler, ICommandHandler<TCommand>
            where TCommand : IRequest
        {
            protected Handler(IDependencies dependencies) : base(dependencies)
            {
            }

            async Task<Unit> IHandler<TCommand, Unit>.Handle(TCommand command,
                CancellationToken cancellationToken)
            {
                await this.Handle(command, cancellationToken).ConfigureAwait(false);
                return Unit.Value;
            }

            public Task Handle(TCommand command)
            {
                return this.Handle(command, CancellationToken.None);
            }

            public abstract Task Handle(TCommand command, CancellationToken cancellationToken);
        }
    }

    public class Command<TResponse> : IRequest<TResponse>
    {
        public abstract class Handler<TCommand> : CommandHandler, ICommandHandler<TCommand, TResponse>
            where TCommand : IRequest<TResponse>
        {
            protected Handler(IDependencies dependencies) : base(dependencies)
            {
            }

            async Task<TResponse> IHandler<TCommand, TResponse>.Handle(TCommand command,
                CancellationToken cancellationToken)
            {
                return await this.Handle(command, cancellationToken).ConfigureAwait(false);
            }

            public Task<TResponse> Handle(TCommand command)
            {
                return this.Handle(command, CancellationToken.None);
            }

            public abstract Task<TResponse> Handle(TCommand command, CancellationToken cancellationToken);
        }
    }
}