using Avalonia.Controls;
using YT2ITUNES.ViewModels;

namespace YT2ITUNES.Views;

public partial class MainWindow : Window
{
    private ScrollViewer consoleScrollViewer;
    public MainWindow()
    {
        InitializeComponent();
        consoleScrollViewer = this.FindControl<ScrollViewer>("ConsoleScrollViewer");
    }

    public void ScrollConsoleToEnd()
    {
        consoleScrollViewer.ScrollToEnd();
    }

}