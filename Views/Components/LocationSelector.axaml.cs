// Views/Components/LocationSelector.axaml.cs
using Avalonia.Controls;
using Avalonia.Interactivity;
using Weatherly.ViewModels;
using Weatherly.Services;

namespace Weatherly.Views.Components;

public partial class LocationSelector : UserControl
{
    private TextBox? _searchTextBox;

    public LocationSelector()
    {
        InitializeComponent();

        _searchTextBox =
            this.FindControl<TextBox>("SearchTextBox");
    }

    private void LocationButton_OnClick(
        object? sender,
        RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
        {
            return;
        }

        viewModel.IsLocationPickerOpen =
            !viewModel.IsLocationPickerOpen;

        if (viewModel.IsLocationPickerOpen)
        {
            _searchTextBox?.Focus();
            _searchTextBox?.SelectAll();
        }
    }

    private async void SearchTextBox_OnTextChanged(
        object? sender,
        TextChangedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
        {
            return;
        }

        await viewModel.SearchLocationsAsync(
            _searchTextBox?.Text ?? string.Empty);
    }

    private async void LocationResult_OnClick(
        object? sender,
        RoutedEventArgs e)
    {
        if (sender is not Button button ||
            button.Tag is not Weatherly.Models.Location location ||
            DataContext is not MainViewModel viewModel)
        {
            return;
        }

        await viewModel.SelectLocationAsync(location);
    }
}