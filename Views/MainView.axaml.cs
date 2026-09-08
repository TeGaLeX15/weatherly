// Views/MainView.axaml.cs
using Avalonia.Controls;
using Avalonia.Interactivity;
using Weatherly.ViewModels;

namespace Weatherly.Views;

public partial class MainView : UserControl
{
    public MainViewModel ViewModel { get; }

    public MainView()
    {
        InitializeComponent();

        ViewModel = new MainViewModel();

        DataContext = ViewModel;

        _ = LoadWeatherAsync();
    }

    private async Task LoadWeatherAsync()
    {
        await ViewModel.LoadAsync();
    }

    private async void RefreshButton_OnClick(
        object? sender,
        RoutedEventArgs e)
    {
        await ViewModel.LoadAsync();
    }
}