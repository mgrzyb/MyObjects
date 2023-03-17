using System.Reflection;
using Autofac;
using Newtonsoft.Json;

namespace MyObjects.Infrastructure;

public class DurableTask : AggregateRoot
{
    private static JsonSerializerSettings serializerSettings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.All
    };

    protected virtual string HandlerTypeAssemblyQualifiedName { get; set; }

    protected virtual string SerializedArgs { get; set; }


    private Type? handlerType;

    public virtual Type GetHandlerType()
    {
        if (this.handlerType is null)
        {
            var handlerType = Type.GetType(this.HandlerTypeAssemblyQualifiedName);
            this.handlerType = handlerType;
        }

        return this.handlerType;
    }

    private object? args;

    public virtual object GetArgs()
    {
        if (args is null)
        {
            this.args = JsonConvert.DeserializeObject(this.SerializedArgs, serializerSettings);
        }

        return this.args;
    }

    protected DurableTask()
    {
    }

    public DurableTask(Type handlerType, object args)
    {
        this.handlerType = handlerType;
        this.HandlerTypeAssemblyQualifiedName = handlerType.AssemblyQualifiedName;

        this.args = args;
        this.SerializedArgs = JsonConvert.SerializeObject(args, serializerSettings);
    }

    public virtual DurableTaskHandler CreateHandler(IComponentContext componentContext)
    {
        return new DurableTaskHandler(componentContext.Resolve(this.GetHandlerType()));
    }

    public class DurableTaskHandler
    {
        private readonly object innerHandler;

        public DurableTaskHandler(object innerHandler)
        {
            this.innerHandler = innerHandler;
        }

        public Task Run(object args)
        {
            const BindingFlags invokeMethodFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod;
            
            return innerHandler.GetType().InvokeMember(nameof(IDurableTaskHandler<object>.Run), invokeMethodFlags, null, this.innerHandler, new[] {args}) as Task;
        }
    }
}