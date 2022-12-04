using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace MyObjects
{
    public interface ICommandHandler<in TCommand, TResult>
        where TCommand : IRequest<TResult>
    {
        Task<TResult> Handle(TCommand command, CancellationToken cancellationToken);
    }

    public interface ICommandHandler<in TCommand> : ICommandHandler<TCommand, Unit> where TCommand : IRequest<Unit>
    {
    }

    public abstract class CommandHandlerBase
    {
        protected CommandHandlerBase(IDependencies dependencies)
        {
            Session = dependencies.Session;
        }

        protected ISession Session { get; }

        public interface IDependencies
        {
            ISession Session { get; }
        }
    }

    public abstract class CommandHandler<TCommand> : CommandHandlerBase, ICommandHandler<TCommand>
        where TCommand : IRequest<Unit>
    {
        protected CommandHandler(IDependencies dependencies) : base(dependencies)
        {
        }

        async Task<Unit> ICommandHandler<TCommand, Unit>.Handle(TCommand command,
            CancellationToken cancellationToken)
        {
            await this.Handle(command, cancellationToken).ConfigureAwait(false);
            return Unit.Value;
        }

        public async Task Handle(TCommand command)
        {
            await this.Handle(command, CancellationToken.None).ConfigureAwait(false);;
        }

        public abstract Task Handle(TCommand command, CancellationToken cancellationToken);
    }

    public abstract class CommandHandler<TCommand, TResponse> : CommandHandlerBase, ICommandHandler<TCommand, TResponse>
        where TCommand : IRequest<TResponse>
    {
        protected CommandHandler(IDependencies dependencies) : base(dependencies)
        {
        }

        async Task<TResponse> ICommandHandler<TCommand, TResponse>.Handle(TCommand command,
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

    public abstract class CommandHandlerWithAdvancedSession<TAdvancedSession>
    {
        protected readonly ISession<TAdvancedSession> Session;

        protected CommandHandlerWithAdvancedSession(IDependencies dependencies)
        {
            this.Session = dependencies.Session;
        }

        public interface IDependencies
        {
            ISession<TAdvancedSession> Session { get; }
        }
    }

    public abstract class CommandHandlerWithAdvancedSession<TAdvancedSession, TCommand> : CommandHandlerWithAdvancedSession<TAdvancedSession>,
        ICommandHandler<TCommand>
        where TCommand : IRequest
    {
        protected CommandHandlerWithAdvancedSession(IDependencies dependencies) : base(dependencies)
        {
        }

        async Task<Unit> ICommandHandler<TCommand, Unit>.Handle(TCommand command,
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

    public abstract class CommandHandlerWithAdvancedSession<TAdvancedSession, TResponse, TCommand> : CommandHandlerWithAdvancedSession<TAdvancedSession>, ICommandHandler<TCommand, TResponse>
        where TCommand : IRequest<TResponse>
    {
        protected CommandHandlerWithAdvancedSession(IDependencies dependencies) : base(dependencies)
        {
        }

        async Task<TResponse> ICommandHandler<TCommand, TResponse>.Handle(TCommand command,
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