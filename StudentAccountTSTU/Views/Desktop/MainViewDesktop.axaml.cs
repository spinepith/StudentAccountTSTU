using Avalonia.Controls;


namespace StudentAccountTSTU.Views.Desktop {
    public partial class MainViewDesktop : UserControl {
        public MainViewDesktop() {
            InitializeComponent();
        }

        private void OnPageChanged(object? sender, int? pageIndex) {
            if (DataContext is not ViewModels.MainViewModel viewModel)
                return;

            switch (pageIndex) {
                case 0:
                    viewModel.OpenHome();
                    break;

                case 1:
                    viewModel.OpenUserData();
                    break;

                case 2:
                    viewModel.OpenMarks();
                    break;

                case 3:
                    viewModel.OpenSchedule();
                    break;

                case 4:
                    viewModel.OpenReportCard();
                    break;

                case 5:
                    viewModel.OpenRating();
                    break;

                case 6:
                    viewModel.OpenSettings();
                    break;
            }
        }
    }
}