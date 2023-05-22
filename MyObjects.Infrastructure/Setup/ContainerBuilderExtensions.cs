using System.Reflection;
using Autofac;
using MyObjects.Infrastructure;

namespace MyObjects.Demo.Functions;

public static class ContainerBuilderExtensions
{
    public static MyObjectsRegistration AddMyObjects(this ContainerBuilder builder, params Assembly[] assemblies)
    {
        builder.RegisterModule(new MyObjectsModule());

        return new MyObjectsRegistration(builder, assemblies);
    }
}