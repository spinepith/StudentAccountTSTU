using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;

namespace StudentAccountTSTU.ViewModels {
    internal abstract class ViewModelBase : ObservableObject {
        protected async Task InitializeWithCacheAsync(Dictionary<string, Task?> activeLoadingTasks, string taskKey, Func<Task> initTaskFactory, Func<Task> loadFromCache, string? alternativeTaskKey = null) {
            if (alternativeTaskKey is not null && activeLoadingTasks.TryGetValue(alternativeTaskKey, out var altTask) && altTask is not null && !altTask.IsCompleted) {
                await altTask;
                await loadFromCache();
                return;
            }

            if (activeLoadingTasks.TryGetValue(taskKey, out var existingTask) && existingTask is not null && !existingTask.IsCompleted) {
                await existingTask;
                await loadFromCache();
                return;
            }

            var initTask = initTaskFactory();
            activeLoadingTasks[taskKey] = initTask;
            await initTask;
            activeLoadingTasks[taskKey] = null;
        }
    }
}
