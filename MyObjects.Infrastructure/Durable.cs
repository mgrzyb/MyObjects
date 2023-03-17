using System.Reflection;
using Castle.DynamicProxy;

namespace MyObjects.Infrastructure;

public class DurableImpl<TService> : Durable<TService> where TService : class
{
    private readonly IDurableTaskQueue<Tuple<string, object[]>> queue;
    private readonly ProxyGenerator generator = new();

    public DurableImpl(IDurableTaskQueue<Tuple<string, object[]>> queue)
    {
        this.queue = queue;
    }

    public override async Task Enqueue(Func<TService, Task> call)
    {
        var interceptor = new Interceptor();
        var f = generator.CreateInterfaceProxyWithoutTarget<TService>(interceptor);
        await call(f);
        await this.queue.Enqueue<Handler>(Tuple.Create(interceptor.Method.Name, interceptor.Args));
    }

    public class Handler : IDurableTaskHandler<Tuple<string, object[]>>
    {
        private readonly TService service;

        public Handler(TService service)
        {
            this.service = service;
        }

        public Task Run(Tuple<string, object[]> args)
        {
            const BindingFlags invokeMethodFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod;

            return this.service.GetType().InvokeMember(args.Item1, invokeMethodFlags, null, this.service, args.Item2) as Task;
        }
    }

    public class Interceptor : IInterceptor
    {
        public object[] Args { get; private set; }
        public MethodInfo Method { get; private set; }

        public void Intercept(IInvocation invocation)
        {
            this.Args = invocation.Arguments;
            this.Method = invocation.Method;

            var isAsync = invocation.Method.ReturnType.IsAssignableTo(typeof(Task));
            if (isAsync)
                invocation.ReturnValue = invocation.Method.ReturnType == typeof(Task) ? Task.CompletedTask
                    : CreateTaskFromDefaultResult(invocation.Method.ReturnType.GetGenericArguments()[0]);
            else
                invocation.ReturnValue = CreateDefaultValue(invocation.Method.ReturnType);
        }

        private static Task CreateTaskFromDefaultResult(Type resultType)
        {
            return (Task)typeof(Task)
                .GetMethod(nameof(Task.FromResult), BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod)
                .MakeGenericMethod(new [] { resultType }).Invoke(null, new [] {CreateDefaultValue(resultType)});
        }

        private static object? CreateDefaultValue(Type valueType)
        {
            return valueType.IsValueType ? Activator.CreateInstance(valueType) : null;
        }
    }
}