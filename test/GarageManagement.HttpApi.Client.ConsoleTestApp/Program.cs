using System.Threading.Tasks;
using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GarageManagement.HttpApi.Client.ConsoleTestApp;

class Program
{
    static async Task Main(string[] args)
    {
        LoadDotEnv();
        await CreateHostBuilder(args).RunConsoleAsync();
    }

    private static void LoadDotEnv()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory != null)
        {
            var envPath = Path.Combine(directory.FullName, ".env");
            if (File.Exists(envPath))
            {
                foreach (var line in File.ReadAllLines(envPath))
                {
                    var trimmedLine = line.Trim();
                    if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("#") || !trimmedLine.Contains('='))
                    {
                        continue;
                    }

                    var separatorIndex = trimmedLine.IndexOf('=');
                    var key = trimmedLine[..separatorIndex].Trim();
                    var value = trimmedLine[(separatorIndex + 1)..].Trim().Trim('"');
                    Environment.SetEnvironmentVariable(key, value);
                }

                return;
            }

            directory = directory.Parent;
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .AddAppSettingsSecretsJson()
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<ConsoleTestAppHostedService>();
            });
}
