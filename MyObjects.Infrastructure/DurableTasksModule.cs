using System.Reflection;
using Autofac;
using Autofac.Builder;
using Module = Autofac.Module;

namespace MyObjects.Infrastructure;

public class DurableTasksModule : Module
{
    private readonly Assembly modelAssembly;

    public DurableTasksModule(Assembly modelAssembly)
    {
        this.modelAssembly = modelAssembly;
    }

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<DurableTaskQueueFactory>().AsSelf().AsImplementedInterfaces().InstancePerLifetimeScope();
        builder.RegisterGeneric((c, type) =>
        {
            var f = c.Resolve<IDurableTaskQueueFactory>();
            return f.GetType().GetMethod(nameof(IDurableTaskQueueFactory.Create)).MakeGenericMethod(type).Invoke(f, new object[] {});
        }).As(typeof(IDurableTaskQueue<>));
        builder.RegisterType<RunDurableTask.Handler>().AsImplementedInterfaces();

        builder.RegisterAssemblyTypes(this.modelAssembly).Where(t => t.IsClosedTypeOf(typeof(IDurableTaskHandler<>)))
            .AsSelf()
            .AsImplementedInterfaces();

        builder.RegisterGeneric(typeof(DurableImpl<>)).As(typeof(Durable<>));
        builder.RegisterGeneric(typeof(DurableImpl<>.Handler)).AsSelf();
        builder.RegisterType<DurableTaskService>().AsImplementedInterfaces();
    }
}