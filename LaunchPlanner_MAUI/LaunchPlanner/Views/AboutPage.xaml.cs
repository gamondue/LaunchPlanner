using LaunchPlanner.Core.ViewModels;

namespace LaunchPlanner.Views
{
    public partial class AboutPage : ContentPage
    {
        public AboutPage(AboutViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
