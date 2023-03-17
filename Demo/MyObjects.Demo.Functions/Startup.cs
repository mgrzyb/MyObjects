using Autofac;
using Autofac.Extensions.DependencyInjection.AzureFunctions;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
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
        /*builder.RegisterMyObjects(c =>
        {
            c.
        });*/
        builder.RegisterModule(new MediatorModule());
        
        builder.RegisterModule(new NHibernateModule(
            new TestPersistenceStrategy(), builder =>
            {
                builder.AddEntitiesFromAssemblyOf<Product>();
                builder.AddEntity<DurableTask>();
            }));
        
        builder.RegisterModule(new CommandsAndEventsModule<global::NHibernate.ISession>(
            typeof(Product).Assembly, true, true, true, true));
        
        builder.RegisterModule(new DurableTasksModule(typeof(Product).Assembly));
        
        builder.RegisterModule(new HttpFunctionsModule(typeof(Api.SalesOrderFunctions).Assembly));
        
        builder.RegisterType<NumberSequence>().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<DurableTaskFunctions.ScheduleRetryWhenDurableTaskCreated>().AsImplementedInterfaces();
        builder.RegisterType<EmailSender>().AsImplementedInterfaces();
        builder.RegisterType<DurableEmailSender>().AsSelf();
        builder.RegisterType<DurableEmailSender.SendEmailHandler>().AsImplementedInterfaces();
        builder.Register(context =>
        {
            var configuration = context.Resolve<IConfiguration>();
            var queueClient = new QueueClient(configuration.GetConnectionString("DurableTaskQueue"), "durable-tasks");
            queueClient.CreateIfNotExists();
            return queueClient;
        }).Named<QueueClient>("durable-tasks");
            
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
