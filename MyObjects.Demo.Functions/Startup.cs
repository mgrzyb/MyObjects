using Autofac;
using Autofac.Extensions.DependencyInjection.AzureFunctions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using MyObjects.Demo.Functions;
using MyObjects.Demo.Model.Orders.Commands;
using MyObjects.Demo.Model.Products;
using MyObjects.Functions;
using MyObjects.Infrastructure;
using MyObjects.NHibernate;
using MyObjects.Testing.NHibernate;

[assembly: FunctionsStartup(typeof(Startup))]

namespace MyObjects.Demo.Functions;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.UseAutofacServiceProviderFactory(ConfigureContainer);
    }

    public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
    {
        // this is optional and will bind IConfiguration with appsettings.json in
        // the container, like it is usually done in regular dotnet console and
        // web applications.
        // builder.UseAppSettings();
    }

    private IContainer ConfigureContainer(ContainerBuilder builder)
    {
        /*
        builder
            .Register(activator =>
            {
                // Example on how to bind settings from appsettings.json
                // to a class instance
                var section = activator.Resolve<IConfiguration>().GetSection(nameof(MySettings));

                var instance = section.Get<MySettings>();

                // If you expect IConfiguration to change (with reloadOnChange=true), use
                // token to rebind.
                ChangeToken.OnChange(
                    () => section.GetReloadToken(),
                    (state) => section.Bind(state),
                    instance);

                return instance;
            })
            .AsSelf()
            .SingleInstance();
            */

        builder.RegisterModule(new MediatorModule());
        
        builder.RegisterModule(new NHibernateModule(
            new TestPersistenceStrategy(), 
            builder => builder.AddEntitiesFromAssemblyOf<Product>()));
        
        builder.RegisterModule(new CommandsAndEventsModule<global::NHibernate.ISession>(
            typeof(Product).Assembly, true, true, true, true));
        
        builder.RegisterModule(new DurableTasksModule(typeof(Product).Assembly));
        
        builder.RegisterModule(new HttpFunctionsModule(typeof(Api.SalesOrderFunctions).Assembly));
        
        builder.RegisterType<NumberSequence>().AsImplementedInterfaces().SingleInstance();
        
        return builder.Build();
    }
    
    public class NumberSequence : INumberSequence
    {
        private int i = 1;
        public int Next()
        {
            return i++;
        }
    }
}
