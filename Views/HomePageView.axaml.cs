
using Avalonia.Controls;
using Avalonia.Threading;
using YT2ITUNES.ViewModels;

namespace YT2ITUNES.Views;

public partial class HomePageView : UserControl
{
    public HomePageViewModel? ViewModel => DataContext as HomePageViewModel;
    public HomePageView()
    {
        InitializeComponent();

        this.AttachedToVisualTree += (_, __) =>
        {
            ViewModel?.SetScrollToEndAction(ScrollToEnd);
        };

    }

    private void ScrollToEnd()
    {
        Dispatcher.UIThread.Post(() =>
        {
            ConsoleScrollViewer?.ScrollToEnd();
        }
        );
    }

}