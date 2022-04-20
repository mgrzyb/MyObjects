using MediatR;
using MyObjects.NHibernate;

namespace MyObjects
{
    public interface ICommandHandler<in TCommand, TResult> : IHandler<TCommand, TResult> where TCommand : IRequest<TResult> 
    {
    }

    public interface ICommandHandler<in TCommand> : ICommandHandler<TCommand, Unit> where TCommand : IRequest<Unit>
    {
    }

    public abstract class CommandHandler
    {
        protected readonly ISession Session;
        
        protected CommandHandler(IDependencies dependencies)
        {
            this.Session = dependencies.Session;
        }

        public interface IDependencies
        {
            ISession Session { get; }
        }
    }
}