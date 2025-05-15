using System.Reflection;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Geometric.Reflection.ViewModels;

namespace Geometric.Reflection.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }

    private async void BrowseAssembly_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm) return;

        var files = await StorageProvider.OpenFilePickerAsync(new()
        {
            Title = "Выберите сборку",
            AllowMultiple = false,
            FileTypeFilter = [new("DLL") { Patterns = ["*.dll"] }]
        });

        if (files.Count > 0)
        {
            vm.AssemblyPath = files[0].Path.LocalPath;
            vm.LoadAssemblyCommand.Execute(null);
        }
    }

    private void OnClassSelected(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
            vm.LoadMethodsCommand.Execute(null);
    }

    private void OnMethodSelected(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
            vm.LoadParametersCommand.Execute(null);
    }
private void OnConstructorSelected(object sender, SelectionChangedEventArgs e)
{
    if (DataContext is MainWindowViewModel vm && e.AddedItems.Count > 0)
    {
        vm.SelectedConstructor = e.AddedItems[0] as ConstructorInfo;
        vm.LoadParametersForConstructorCommand.Execute(null);
    }
}
}