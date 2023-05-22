using Autofac;

namespace MyObjects.Infrastructure.Setup;

public static class DurableTasksSetupExtensions
{
    public static MyObjectsRegistration UseDurableTasks(
        this MyObjectsRegistration myObjectsRegistration)
    {
        myObjectsRegistration.AddEntityType<DurableTask>();

        var builder = myObjectsRegistration.Builder;

        builder.RegisterType<DurableTaskQueueFactory>().AsSelf().AsImplementedInterfaces().InstancePerLifetimeScope();
        builder.RegisterGeneric((c, type) =>
        {
            var f = c.Resolve<IDurableTaskQueueFactory>();
            return f.GetType().GetMethod(nameof(IDurableTaskQueueFactory.Create)).MakeGenericMethod(type)
                .Invoke(f, new object[] { });
        }).As(typeof(IDurableTaskQueue<>));
        builder.RegisterType<RunDurableTask.Handler>().AsImplementedInterfaces();

        builder.RegisterAssemblyTypes(myObjectsRegistration.Assemblies.ToArray())
            .Where(t => t.IsClosedTypeOf(typeof(IDurableTaskHandler<>)))
            .AsSelf()
            .AsImplementedInterfaces();

        builder.RegisterGeneric(typeof(DurableImpl<>)).As(typeof(Durable<>));
        builder.RegisterGeneric(typeof(DurableImpl<>.Handler)).AsSelf();
        builder.RegisterType<DurableTaskService>().AsImplementedInterfaces();

        return myObjectsRegistration;
    }
}