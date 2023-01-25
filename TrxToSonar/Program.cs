using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TrxToSonar.Model.Sonar;

namespace TrxToSonar
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            using ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            var app = new CommandLineApplication
            {
                Name = "Trx To Sonar",
                Description = ""
            };

            app.HelpOption("-?|-h|--help");

            CommandOption solutionDirectoryOption =
                app.Option("-d", "Solution Directory to parse.", CommandOptionType.SingleValue);
            CommandOption outputOption = app.Option("-o", "Output filename.", CommandOptionType.SingleValue);
            CommandOption absolutePathOption = app.Option("-a|--absolute", "Use Absolute Path", CommandOptionType.NoValue);
            CommandOption usePDBFileOption = app.Option("-p|--pdb", "Use pdb file", CommandOptionType.NoValue);

            app.OnExecute(
                () =>
                {
                    if (solutionDirectoryOption.HasValue() && outputOption.HasValue())
                    {
                        var converter = serviceProvider.GetService<IConverter>();
                        SonarDocument sonarDocument = converter.Parse(solutionDirectoryOption.Value(), absolutePathOption.HasValue(), usePDBFileOption.HasValue());
                        converter.Save(sonarDocument, outputOption.Value());
                    }
                    else
                    {
                        app.ShowHint();
                    }

                    return 0;
                });

            app.Execute(args);
        }

        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddLogging(builder => builder.AddConsole());
            serviceCollection.AddSingleton<IConverter, Converter>();
        }
    }
}
