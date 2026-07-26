using System;
using System.Linq;
using System.Reflection;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;

using Microsoft.Extensions.DependencyInjection;

using StudentAccountTSTU.Services;
using StudentAccountTSTU.ViewModels;
using StudentAccountTSTU.Views.Desktop;
using StudentAccountTSTU.Views.Mobile;

namespace StudentAccountTSTU {
    public partial class App : Application {
        public static IServiceProvider Services { get; private set; } = null!;

        public override void Initialize() {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted() {
            var services = new ServiceCollection();

            services.AddSingleton<WebAccount.Interfaces.IHttpService, Services.HttpService>();
            services.AddSingleton<WebAccount.WebAccount>();
            services.AddSingleton(Settings.Load());

            Services = services.BuildServiceProvider();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                desktop.MainWindow = new MainWindow {
                    DataContext = new MainViewModel()
                };
            }
            else if (ApplicationLifetime is IActivityApplicationLifetime singleViewFactoryApplicationLifetime) {
                singleViewFactoryApplicationLifetime.MainViewFactory = () => new MainViewMobile { DataContext = new MainViewModel() };
            }
            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform) {
                singleViewPlatform.MainView = new MainViewMobile { DataContext = new MainViewModel() };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}