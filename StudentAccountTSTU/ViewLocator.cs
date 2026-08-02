using System;
using System.Diagnostics.CodeAnalysis;

using Avalonia.Controls;
using Avalonia.Controls.Templates;

using StudentAccountTSTU.ViewModels;

namespace StudentAccountTSTU {
    /// <summary>
    /// Given a view model, returns the corresponding view if possible.
    /// </summary>
    [RequiresUnreferencedCode(
        "Default implementation of ViewLocator involves reflection which may be trimmed away.",
        Url = "https://docs.avaloniaui.net/docs/concepts/view-locator")]
    public class ViewLocator : IDataTemplate {
        public Control? Build(object? param) {
            if (param is null)
                return null;

            var viewModelType = param.GetType();
            var viewModelTypeName = viewModelType.FullName!;

            string viewTypeName;
            if (OperatingSystem.IsAndroid() || OperatingSystem.IsIOS())
                viewTypeName = viewModelTypeName
                    .Replace("SettingsViewModels", "SettingsViews", StringComparison.Ordinal)
                    .Replace("ViewModels", "Views.Mobile", StringComparison.Ordinal)
                    .Replace("ViewModel", "View", StringComparison.Ordinal) + "Mobile";
            else
                viewTypeName = viewModelTypeName
                    .Replace("SettingsViewModels", "SettingsViews", StringComparison.Ordinal)
                    .Replace("ViewModels", "Views.Desktop", StringComparison.Ordinal)
                    .Replace("ViewModel", "View", StringComparison.Ordinal);

            var viewType = viewModelType.Assembly.GetType(viewTypeName);

            if (viewType != null) {
                return (Control)Activator.CreateInstance(viewType)!;
            }

            return new TextBlock { Text = "Not Found: " + viewTypeName };
        }

        public bool Match(object? data) {
            return data is ViewModelBase;
        }
    }
}