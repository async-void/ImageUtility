using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Messaging;
using ImageUtility.Common;
using ImageUtility.Converters;
using ImageUtility.Dialogs;
using ImageUtility.Features.Converting;
using ImageUtility.Features.CustomTheme;
using ImageUtility.Features.Dashboard;
using ImageUtility.Features.Help;
using ImageUtility.Features.Renamer;
using ImageUtility.Features.Resizer;
using ImageUtility.Interfaces;
using ImageUtility.Services;
using ImageUtility.Utilities;
using ImageUtility.ViewModels;
using ImageUtility.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using SukiUI.Controls;
using SukiUI.Dialogs;
using SukiUI.MessageBox;
using SukiUI.Models;
using SukiUI.Toasts;
using System;
using System.Threading.Tasks;
using Velopack;

namespace ImageUtility
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            VelopackApp.Build().Run();
            AvaloniaXamlLoader.Load(this);
            Task.Run(async () => await UpdateApp());
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
           // async Task AppUpdateTask() => await UpdateApp();
        }

        private static SukiViews ConfigureViews(ServiceCollection services)
        {
            return new SukiViews()

              // Add main view
              .AddView<MainWindow, MainWindowViewModel>(services)

             // Add pages
             .AddView<DashboardView, DashboardViewModel>(services)
             .AddView<RenamerView, RenamerViewModel>(services)
             .AddView<ConverterView, ConverterViewModel>(services)
             //.AddView<ThemingView, ThemingViewModel>(services)
             .AddView<ResizerView, ResizerViewModel>(services)
             .AddView<HelpView, HelpViewModel>(services)  
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
            services.AddSingleton<IResizer, ResizerService>();
            services.AddSingleton<IHelpProvider, HelpProviderService>();
            services.AddSingleton<IFileUtilities, FileUtilities>();
            services.AddSingleton<IImageConverter, PngConverter>();
            services.AddSingleton<IImageConverter, JpgConverter>();
            services.AddSingleton<IImageConverter, WebpConverter>();
            services.AddSingleton<IJsonData, JsonDataProviderService>();
            services.AddSingleton<IChartBuilder, StatsChartBuilderService>();
            services.AddSingleton<ConversionService>();
            services.AddSingleton<SukiColorTheme>();
            services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);
            services.AddLogging(loggerBuilder =>
            {
                loggerBuilder.ClearProviders();
                loggerBuilder.AddSerilog(new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day, outputTemplate: "[{Timestamp:MM/dd/yyyy HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
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

        private static async Task UpdateApp()
        {
            var mrg = new UpdateManager(@"C:\Users\glaro\OneDrive\Desktop\ImageUtility_Suki_Publish\Releases");

            // Check for updates
            var newVersion = await mrg.CheckForUpdatesAsync();
            if (newVersion == null) return;

            var msgBox = new SukiMessageBoxHost
            {
                ActionButtonsPreset = SukiMessageBoxButtons.YesNo,
                IconPreset = SukiMessageBoxIcons.Information,
                Header = "New Version Available",
                Content = "There is a new version available, would you like to update?"
            };
            var msgBoxResult = await SukiMessageBox.ShowDialog(msgBox);
            if (msgBoxResult is SukiMessageBoxResult.Yes)
            {
                // Apply updates
                await mrg.DownloadUpdatesAsync(newVersion);
                // install new version
                mrg.ApplyUpdatesAndRestart(newVersion);
            }
        }
    }
}