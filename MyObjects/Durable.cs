using System;
using System.Threading.Tasks;

namespace MyObjects;

public abstract class Durable<TService> where TService : class
{
    public abstract Task Enqueue(Func<TService, Task> call);
}