using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Logging;
using Avalonia.Markup.Xaml;
using ImageUtility.Common;
using ImageUtility.Dialogs;
using ImageUtility.Features.CustomTheme;
using ImageUtility.Features.Dashboard;
using ImageUtility.Features.Renamer;
using ImageUtility.Features.Resizer;
using ImageUtility.Features.Theming;
using ImageUtility.Interfaces;
using ImageUtility.Services;
using ImageUtility.ViewModels;
using ImageUtility.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using SukiUI.Controls;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using System.Linq;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace ImageUtility
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var services = new ServiceCollection();
                services.AddSingleton(desktop);
                var views = ConfigureViews(services);
                var provider = ConfigureServices(services);
                DataTemplates.Add(new ViewLocator(views));
                desktop.MainWindow = views.CreateView<MainWindowViewModel>(provider) as Window;
            }
            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
            {
                var services = new ServiceCollection();
                services.AddSingleton(singleView);
                var views = ConfigureViews(services);
                var provider = ConfigureServices(services);
                DataTemplates.Add(new ViewLocator(views));


                // Ideally, we want to create a MainView that host app content
                // and use it for both IClassicDesktopStyleApplicationLifetime and ISingleViewApplicationLifetime
                singleView.MainView = new SukiMainHost()
                {
                    Hosts = [
                        new SukiDialogHost
                    {
                        Manager = new SukiDialogManager()
                    }
                    ],
                    Content = views.CreateView<DialogViewModel>(provider)
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

        private static SukiViews ConfigureViews(ServiceCollection services)
        {
            return new SukiViews()

              // Add main view
              .AddView<MainWindow, MainWindowViewModel>(services)

             // Add pages
             .AddView<DashboardView, DashboardViewModel>(services)
             .AddView<RenamerView, RenamerViewModel>(services)
             .AddView<ThemingView, ThemingViewModel>(services)
             .AddView<ResizerView, ResizerViewModel>(services)
            // Add additional views
            .AddView<DialogView, DialogViewModel>(services)
            //.AddView<VmDialogView, VmDialogViewModel>(services)
            //.AddView<RecursiveView, RecursiveViewModel>(services)
            .AddView<CustomThemeDialogView, CustomThemeDialogViewModel>(services);
        }

        private static ServiceProvider ConfigureServices(ServiceCollection services)
        {
            //services.AddSingleton<ClipboardService>();
            services.AddSingleton<PageNavigationService>();
            services.AddSingleton<MainWindow>();
            services.AddSingleton<ISukiToastManager, SukiToastManager>();
            services.AddSingleton<ISukiDialogManager, SukiDialogManager>();
            services.AddSingleton<IRenamer, RenamerService>();
            services.AddLogging(loggerBuilder =>
            {
                loggerBuilder.ClearProviders();
                loggerBuilder.AddSerilog(new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day, outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .CreateLogger());
            });
            //services.AddDbContextFactory<AppDbContext>(options =>
            //{
            //    var config = new ConfigurationBuilder()
            //        .SetBasePath(Path.Combine(AppContext.BaseDirectory, "Configuration"))
            //        .AddJsonFile("config.json", optional: false)
            //        .Build();

            //    var connectionString = config.GetConnectionString("connectionstring");
            //    options.UseNpgsql(connectionString);

            //});

            return services.BuildServiceProvider();
        }
    }
}