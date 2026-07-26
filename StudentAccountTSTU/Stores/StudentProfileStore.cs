using System.Collections.Generic;

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
        
        switch (page) {
            case nameof(Pages.Schedule):
            case nameof(Pages.Lessons):
                Lessons = await webAccount.GetLessonsAsync(page);
                break;

            case nameof(Pages.Achievements):
                Groups = await webAccount.GetGroupsAsync(page);
                break;
        }
    }
}
