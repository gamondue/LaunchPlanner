using LaunchPlanner.Core.Services;
using LaunchPlanner.Core.ViewModels;

namespace LaunchPlanner.Views
{
    public partial class EventsPage : ContentPage
    {
        public EventsPage(EventsViewModel viewModel, EventStore store)
        {
            InitializeComponent();
            BindingContext = viewModel;
            store.EnsureLoaded();
        }
    }
}
