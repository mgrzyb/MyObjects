using System;

namespace MyObjects;

public class ConcurrencyViolationException : Exception
{
    public ConcurrencyViolationException(string entityName, object id, Exception e) : this($"Concurrency violation occured when tyring to update or delete {entityName}({id})", e)
    {
    }
        
    public ConcurrencyViolationException(IEntity entity) : this($"Concurrency violation occured when tyring to update or delete {entity.GetReference()}")
    {
    }

    public ConcurrencyViolationException(string? message, Exception? innerException = null) : base(message, innerException)
    {
    }
}