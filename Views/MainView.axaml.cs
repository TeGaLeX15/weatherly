// Views/MainView.axaml.cs
using Avalonia.Controls;
using Avalonia.Interactivity;
using Weatherly.ViewModels;

namespace Weatherly.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();

        DataContext = new MainViewModel();

        AttachedToVisualTree += async (_, _) =>
        {
            if (DataContext is MainViewModel viewModel)
            {
                await viewModel.LoadAsync();
            }
        };
    }

    private async void RefreshButton_OnClick(
        object? sender,
        RoutedEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            await viewModel.LoadAsync();
        }
    }

    private void SettingsButton_OnClick(
        object? sender,
        RoutedEventArgs e)
    {
        // Настройки добавим следующим этапом.
    }
}