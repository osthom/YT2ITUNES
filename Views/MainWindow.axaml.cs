using Avalonia.Controls;
using System;
using YT2ITUNES.ViewModels;

namespace YT2ITUNES.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new NavigatorViewModel();
    }


}