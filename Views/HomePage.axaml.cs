
using Avalonia.Controls;
using YT2ITUNES.ViewModels;

namespace YT2ITUNES.Views;

public partial class HomePage : UserControl
{
    private ScrollViewer consoleScrollViewer;
    public HomePage()
    {
    }

    public void ScrollConsoleToEnd()
    {
        consoleScrollViewer.ScrollToEnd();
    }

}