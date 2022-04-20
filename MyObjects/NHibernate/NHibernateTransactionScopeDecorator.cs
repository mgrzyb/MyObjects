using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace MyObjects.NHibernate
{
    public class NHibernateTransactionScopeDecorator<TCommand, TResult> : IHandler<TCommand, TResult>
        where TCommand : IRequest<TResult>
    {
        private readonly IHandler<TCommand, TResult> innerHandler;
        private readonly global::NHibernate.ISession session;

        public NHibernateTransactionScopeDecorator(IHandler<TCommand, TResult> innerHandler,
            global::NHibernate.ISession session)
        {
            this.innerHandler = innerHandler;
            this.session = session;
        }

        public async Task<TResult> Handle(TCommand request, CancellationToken cancellationToken)
        {
            using (var transaction = this.session.BeginTransaction())
            {
                var result = await this.innerHandler.Handle(request, cancellationToken);                
                transaction.Commit();
                return result;
            }
        }
    } 
}