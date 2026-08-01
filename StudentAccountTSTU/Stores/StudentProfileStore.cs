using System.Collections.Generic;
using System.IO;

using StudentAccountTSTU.Services;

using WebAccount.Models;


namespace StudentAccountTSTU.Stores; 

internal class StudentProfileStore {
    private WebAccount.WebAccount webAccount;

    internal IReadOnlyList<string>? Groups  { get; private set; }
    internal IReadOnlyList<string>? Lessons { get; private set; }

    public StudentProfileStore(WebAccount.WebAccount webAccount) {
        this.webAccount = webAccount;
        webAccount.PageLoaded += OnPageLoaded;
    }

    private async void OnPageLoaded(string? page) {
        if (page is null)
            return;

        string path;
        switch (page) {
            /* ДИСЦИПЛИНЫ ПАРСЯТСЯ В ФАЙЛЕ LessonsViewModel.cs */
            //case nameof(Pages.Lessons):
            //    Lessons = await webAccount.GetLessonsAsync(page);
            //    await FileStorage.SaveAsync(Lessons, Path.Combine("Data", "Lessons.json"));
            //    break;

            case nameof(Pages.Achievements):
                System.Diagnostics.Debug.WriteLine(page);
                path = Path.Combine("Data", "Groups.json");
                if (!FileStorage.CheckExists(path)) {
                    Groups = await webAccount.GetGroupsAsync(page);
                    await FileStorage.SaveAsync(Groups, path);
                }
                break;
        }
    }
}
