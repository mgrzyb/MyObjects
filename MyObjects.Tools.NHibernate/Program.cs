using System.CommandLine;
using System.CommandLine.Invocation;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using DotNetConfig;
using MyObjects.NHibernate;
using NHibernate.Tool.hbm2ddl;
using Cmd = System.CommandLine.Command;

namespace MyObjects.Tools.NHibernate
{
    public static class CommandExtensions
    {
        public static Cmd HandledBy(this Cmd command, Delegate handler)
        {
            command.Handler = CommandHandler.Create(handler);
            return command;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var addMigration = new Cmd("add-migration")
            {
                new Option<string>("modelProjectPath"),
                new Option<string>("migrationsNamespace"),
                new Option<string>("migrationsDirectory"),
            };

            var runMigrations = new Cmd("run-migrations");
            
            var command = new RootCommand()
            {
                new Option<string>("connectionString"),
                new Option<string>("migrationsProjectPath"),
                
                addMigration.HandledBy(AddMigration),
                runMigrations.HandledBy(RunMigrations)
            }.WithConfigurableDefaults("nh");

            command.Invoke(args);
        }


        private static void AddMigration(string connectionString, string modelProjectPath, string migrationsNamespace, string migrationsProjectPath, string migrationsDirectory)
        {
            var modelProjectFile = LocateProjectFile(modelProjectPath);
            var migrationsProjectFile = LocateProjectFile(migrationsProjectPath);
            
            var modelAssembly = LoadModelAssembly(modelProjectFile);
            var schemaUpdateScript = GenerateSchemaUpdateScript(modelAssembly, connectionString);
            
            var migrationName = DateTimeOffset.Now.ToString("yyyyMMddHHmmss");

            using var templateStreamReader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream($"{typeof(Program).Namespace}.MigrationClassTemplate.cs_"));
            var template = templateStreamReader.ReadToEnd();

            var migrationsOutputPath = Path.Combine(migrationsProjectFile.DirectoryName, migrationsDirectory);
            Directory.CreateDirectory(migrationsOutputPath);
            
            File.WriteAllText(Path.Combine(migrationsOutputPath, $"{migrationName}.cs"), template
                .Replace("{$Directory}", Path.GetRelativePath(migrationsProjectFile.DirectoryName, migrationsOutputPath))    
                .Replace("{$Namespace}", migrationsNamespace)    
                .Replace("{$Name}", migrationName));
            File.WriteAllText(Path.Combine(migrationsOutputPath, $"{migrationName}.sql"), schemaUpdateScript);            
        }
        
        
        private static void RunMigrations(string connectionString, string migrationsProjectPath)
        {
            var migrationsProjectFile = LocateProjectFile(migrationsProjectPath);
            var modelAssembly = LoadModelAssembly(migrationsProjectFile);
            
            Console.WriteLine("Not implemented yet. Use dotnet-fm tool from FluentMigrator to run the migrations.");
        }

        private static Assembly LoadModelAssembly(FileInfo? projectFile)
        {
            if (projectFile is null || !projectFile.Exists)
                throw new InvalidOperationException("Project file not found");

            var csProj = XDocument.Load(projectFile.FullName);
            var assemblyNameElement = csProj.Descendants().SingleOrDefault(e => e.Name == "AssemblyName");

            var assemblyName = assemblyNameElement != null ? assemblyNameElement.Value : Path.GetFileNameWithoutExtension(projectFile.Name);

            var modelAssemblyFile = Directory.EnumerateFiles(
                Path.Combine(projectFile.DirectoryName, "bin"), $"{assemblyName}.dll", SearchOption.AllDirectories)
                .Select(p => new FileInfo(p))
                .MaxBy(f => f.LastWriteTime);
            
            if (modelAssemblyFile is null)
                throw new InvalidOperationException("Failed to locate model assembly");

            var modelAssembly = Assembly.LoadFrom(modelAssemblyFile.FullName);
            return modelAssembly;
        }
        
        private static FileInfo? LocateProjectFile(string modelProjectPath)
        {
            if (Path.HasExtension(modelProjectPath) && Path.GetExtension(modelProjectPath).Equals(".csproj", StringComparison.OrdinalIgnoreCase))
            {
                var projectFile = new FileInfo(modelProjectPath);
                return projectFile.Exists ? projectFile : null;
            }
            else
            {
                var projectFilePath = Directory.EnumerateFiles(modelProjectPath, "*.csproj").SingleOrDefault();
                return projectFilePath is null ? null : new FileInfo(projectFilePath);
            }
        }

        private static string GenerateSchemaUpdateScript(Assembly modelAssembly, string connectionString)
        {
            var configuration = new NHibernateConfigurationBuilder(new MigrationPersistenceStrategy(connectionString))
                .AddEntitiesFromAssembly(modelAssembly)
                .Build();

            var scriptBuilder = new StringBuilder();
            var schemaUpdate = new SchemaUpdate(configuration);
            schemaUpdate.Execute((s => scriptBuilder.AppendLine(s)), false);
            if (schemaUpdate.Exceptions.Any())
                throw new AggregateException(schemaUpdate.Exceptions);
            var schemaUpdateScript = scriptBuilder.ToString();
            return schemaUpdateScript;
        }
    }
}