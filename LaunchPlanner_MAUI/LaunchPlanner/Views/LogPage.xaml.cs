using LaunchPlanner.Core.ViewModels;

namespace LaunchPlanner.Views
{
    public partial class LogPage : ContentPage
    {
        public LogPage(LogViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
