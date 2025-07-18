
using Avalonia.Controls;
using YT2ITUNES.ViewModels;

namespace YT2ITUNES.Views;

public partial class SettingsPage: UserControl 
{
    private ScrollViewer consoleScrollViewer;
    public SettingsPage()
    {

    }

    public void ScrollConsoleToEnd()
    {
        consoleScrollViewer.ScrollToEnd();
    }

}