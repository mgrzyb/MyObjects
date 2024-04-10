using System;
using Autofac;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection.AzureFunctions;
using AutoMapper;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyObjects.Demo.Functions;
using MyObjects.Demo.Functions.GraphQL;
using MyObjects.Demo.Functions.GraphQL.ProductCategories;
using MyObjects.Demo.Functions.Infrastructure;
using MyObjects.Demo.Functions.Model;
using MyObjects.Demo.Model;
using MyObjects.Demo.Model.Products;
using MyObjects.Functions;
using MyObjects.Identity;
using MyObjects.Infrastructure;
using MyObjects.Infrastructure.Setup;
using MyObjects.NHibernate;
using MyObjects.Testing.NHibernate;

[assembly: FunctionsStartup(typeof(Startup))]

namespace MyObjects.Demo.Functions;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.UseAutofacServiceProviderFactory(ConfigureContainer);
        
        builder.Services.AddMvcCore().AddNewtonsoftJson(options => 
        {
           options.SerializerSettings.Converters.Add(new NewtonsoftReferenceConverter()); 
        });
        
        builder.AddGraphQLFunction(apiRoute: "/graphql")
            .AddType<ProductCategoryProjectionType>()
            .AddQueryType<GraphQLQueryType>()
            .AddMutationType<GraphQLMutation>(descriptor => descriptor.Name("Mutation"))
            .AddProjections()
            .AddFiltering();
    }

    public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
    {
        // this is optional and will bind IConfiguration with appsettings.json in
        // the container, like it is usually done in regular dotnet console and
        // web applications.
        builder.UseAppSettings();
    }

    private IContainer ConfigureContainer(ContainerBuilder builder)
    {
        builder.AddMyObjects(typeof(Product).Assembly)
            .UseDurableTasks()
            .UseDomainEventBus()
            .UseNHibernate(new TestPersistenceStrategy())
            .AddCommandHandlers(c => c
                .EmitDomainEvents()
                .FlushQueues()
                .RunInTransaction()
                .RunDurableTasks()
                .RunInLifetimeScope()
                .Retry())
            .AddDomainEventHandlers();
        
        builder.RegisterModule(new HttpFunctionsModule(typeof(Api.SalesOrderFunctions).Assembly));
        
        builder.RegisterType<NumberSequence>().AsImplementedInterfaces().SingleInstance();

        builder.RegisterType<DurableTaskFunctions.ScheduleRetryWhenDurableTaskCreated>().AsImplementedInterfaces();
        builder.RegisterType<EmailSender>().AsImplementedInterfaces();
        builder.RegisterType<DurableEmailSender>().AsSelf();
        builder.RegisterType<DurableEmailSender.SendEmailHandler>().AsImplementedInterfaces();
        
        builder.Register(StorageQueueFactory("DurableTaskQueue", "durable-tasks")).Named<QueueClient>("durable-tasks");
        builder.Register(StorageQueueFactory("DomainEventsQueue", "domain-events")).Named<QueueClient>("domain-events");

        builder.RegisterType<DurableStorageQueue>().WithParameter(new NamedParameter("queueName", "domain-events"))
            .Named<IDurableQueue>("domain-events-queue");
        builder.RegisterDecorator<IDurableQueue>(queue => new BatchingQueueDecorator(queue, 5), fromKey: "domain-events-queue")
            .Named<IDurableQueue>("domain-events").As<BatchingQueueDecorator>();
        
        builder.RegisterType<DurableStorageQueue.Handler>().AsSelf();
        
        builder.Register(context => new Mapper(new MapperConfiguration(config =>
        {
            config.AddMaps(typeof(SalesOrderProfile).Assembly);
        }), context.Resolve)).SingleInstance();

        builder.RegisterGeneric(typeof(ProjectedQuery<,>)).AsSelf();

        builder.RegisterGeneric(typeof(StorageQueueAdapter<>))
            .WithParameter(ResolvedParameter.ForNamed<IDurableQueue>("domain-events"))
            .AsImplementedInterfaces();
        
        builder.RegisterType<AutoDomainEventMessageMapper<AggregateCreated<Product>>>().AsImplementedInterfaces();
        
        builder.RegisterType<UserStore>().AsImplementedInterfaces().AsSelf().As(typeof(UserStore<User>));
        return builder.Build();
    }

    private Func<IComponentContext, QueueClient> StorageQueueFactory(string connectionStringName, string queueName)
    {
        return context =>
        {
            var configuration = context.Resolve<IConfiguration>();
            var queueClient = new QueueClient(configuration.GetConnectionString(connectionStringName), queueName);
            queueClient.CreateIfNotExists();
            return queueClient;
        };
    }
}