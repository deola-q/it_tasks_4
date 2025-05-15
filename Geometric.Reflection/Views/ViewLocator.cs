using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Geometric.Reflection.ViewModels;

namespace Geometric.Reflection.Views;  // Изменили пространство имен

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        if (data is null)
            return null;
        
        var name = data.GetType().FullName!
            .Replace("ViewModels", "Views")
            .Replace("ViewModel", "View");
        
        var type = Type.GetType(name);

        return type != null 
            ? (Control)Activator.CreateInstance(type)! 
            : new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data) => data is ViewModelBase;
}