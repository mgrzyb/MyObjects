
using System.Threading.Tasks;

namespace MyObjects;

public interface IDurableTask<in T>
{
    Task Run(T args);
}

public interface IDurableTaskQueue<TArgs>
{
    Task Enqueue<T>(TArgs args) where T : IDurableTask<TArgs>;
}