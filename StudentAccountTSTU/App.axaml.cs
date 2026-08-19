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
using StudentAccountTSTU.Stores;
using StudentAccountTSTU.ViewModels;
using StudentAccountTSTU.Views.Desktop;
using StudentAccountTSTU.Views.Mobile;

namespace StudentAccountTSTU {
    public partial class App : Application {
        public static IServiceProvider Services { get; private set; } = null!;
        public static Avalonia.Controls.TopLevel? TopLevel { get; set; }

        public override void Initialize() {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted() {
            var services = new ServiceCollection();

            services.AddSingleton<WebAccount.Interfaces.IHttpService, HttpService>();
            services.AddSingleton<Settings>(provider => Settings.Load());

            services.AddSingleton<WebAccount.WebAccount>(
                provider => {
                    var httpService = provider.GetRequiredService<WebAccount.Interfaces.IHttpService>();
                    var settings = provider.GetRequiredService<Settings>();
                    return new WebAccount.WebAccount(httpService, settings.BaseURL);
                }
            );

            services.AddSingleton<StudentProfileStore>(
                provider => {
                    var webAccount = provider.GetRequiredService<WebAccount.WebAccount>();
                    return new StudentProfileStore(webAccount);
                }
            );

            Services = services.BuildServiceProvider();

            var mainViewModel = new MainViewModel();
            DataContext = mainViewModel;

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                desktop.MainWindow = new MainWindow {
                    DataContext = mainViewModel
                };
            }
            else if (ApplicationLifetime is IActivityApplicationLifetime singleViewFactoryApplicationLifetime) {
                singleViewFactoryApplicationLifetime.MainViewFactory = () => new MainViewMobile { DataContext = mainViewModel };
            }
            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform) {
                singleViewPlatform.MainView = new MainViewMobile { DataContext = mainViewModel };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}