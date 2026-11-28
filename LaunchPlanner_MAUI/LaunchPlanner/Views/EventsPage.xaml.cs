using LaunchPlanner.Core.Services;
using LaunchPlanner.Core.ViewModels;
using Microsoft.Maui.Layouts;

namespace LaunchPlanner.Views
{
    public partial class EventsPage : ContentPage
    {
        public FlexDirection EventListDirection { get; private set; } = FlexDirection.Column;

        public EventsPage(EventsViewModel viewModel, EventStore store)
        {
            InitializeComponent();
#if ANDROID
            // Sul telefono il modulo e l'elenco si dividono la pagina a metà
            PageGrid.RowDefinitions[0].Height = GridLength.Star;
            FormScroll.MaximumHeightRequest = double.PositiveInfinity;
#endif
            BindingContext = viewModel;
            store.EnsureLoaded();
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            var direction = width > height ? FlexDirection.Row : FlexDirection.Column;
            if (EventListDirection == direction)
            {
                return;
            }

            EventListDirection = direction;
            OnPropertyChanged(nameof(EventListDirection));
        }
    }
}
