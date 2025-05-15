using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Geometric.Models;

namespace Geometric.Reflection.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string? _assemblyPath;

    [ObservableProperty]
    private string? _statusMessage;
    [ObservableProperty]
    private object? _currentInstance; 

    [ObservableProperty]
    private ConstructorInfo? _selectedConstructor;

    public ObservableCollection<string> ClassNames { get; } = new();
    public ObservableCollection<MethodInfo> Methods { get; } = new();
    public ObservableCollection<ParameterViewModel> Parameters { get; } = new();
    public ObservableCollection<ConstructorInfo> Constructors { get; } = new();

    [ObservableProperty]
    private string? _selectedClassName;

    [ObservableProperty]
    private MethodInfo? _selectedMethod;

    [ObservableProperty]
    private string? _result;
[RelayCommand]
private void LoadConstructors()
{
    if (string.IsNullOrWhiteSpace(SelectedClassName) || string.IsNullOrWhiteSpace(AssemblyPath))
        return;

    try
    {
        var assembly = Assembly.LoadFrom(AssemblyPath);
        var type = assembly.GetType($"Geometric.Models.{SelectedClassName}");
        
        Constructors.Clear();
        var ctors = type?.GetConstructors(BindingFlags.Public | BindingFlags.Instance) ?? Array.Empty<ConstructorInfo>();
        
        foreach (var ctor in ctors)
        {
            Constructors.Add(ctor);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка загрузки конструкторов: {ex}");
    }
}
[RelayCommand]
private void LoadParametersForConstructor()
{
    Parameters.Clear();
    
    if (SelectedConstructor == null)
        return;

    foreach (var param in SelectedConstructor.GetParameters())
    {
        Parameters.Add(new ParameterViewModel
        {
            Name = param.Name!,
            Type = param.ParameterType,
            Value = GetDefaultValue(param.ParameterType)
        });
    }

    // Создаём объект и сохраняем его
    var paramValues = Parameters.Select(p => Convert.ChangeType(p.Value, p.Type)).ToArray();
    CurrentInstance = SelectedConstructor.Invoke(paramValues);
}
    [RelayCommand]
private void LoadAssembly()
{
    if (string.IsNullOrWhiteSpace(AssemblyPath))
    {
        StatusMessage = "Путь к сборке не указан";
        Console.WriteLine("Ошибка: Путь к сборке не указан");
        return;
    }

    try
    {
        Console.WriteLine($"Попытка загрузить сборку: {AssemblyPath}");
        var assembly = Assembly.LoadFrom(AssemblyPath);
        Console.WriteLine($"Сборка загружена: {assembly.FullName}");

        var shapeTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IShape).IsAssignableFrom(t))
            .ToList();

        Console.WriteLine($"Найдено классов: {shapeTypes.Count}");
        foreach (var type in shapeTypes)
        {
            Console.WriteLine($"Класс: {type.FullName}");
        }

        ClassNames.Clear();
        foreach (var type in shapeTypes)
        {
            ClassNames.Add(type.Name);
        }

        StatusMessage = $"Загружено {shapeTypes.Count} классов фигур";
    }
    catch (Exception ex)
    {
        StatusMessage = $"Ошибка загрузки сборки: {ex.Message}";
        Console.WriteLine($"Ошибка: {ex}");
    }
}

[RelayCommand]
private void LoadMethods()
{
    if (string.IsNullOrWhiteSpace(SelectedClassName) || string.IsNullOrWhiteSpace(AssemblyPath))
    {
        Console.WriteLine("Не выбран класс или не указан путь к сборке");
        return;
    }

    try
    {
        Console.WriteLine($"Загрузка методов для класса: {SelectedClassName}");
        var assembly = Assembly.LoadFrom(AssemblyPath);
        var type = assembly.GetTypes().FirstOrDefault(t => t.Name == SelectedClassName);
        
        if (type == null)
        {
            StatusMessage = "Класс не найден";
            Console.WriteLine($"Класс {SelectedClassName} не найден в сборке");
            return;
        }

        Methods.Clear();
        
        // Получаем все публичные методы экземпляра (включая унаследованные)
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => !m.IsSpecialName) // исключаем методы свойств и событий
            .OrderBy(m => m.Name)
            .ToList();

        // Добавляем свойства как виртуальные методы
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in properties)
        {
            if (prop.GetMethod != null)
            {
                Methods.Add(prop.GetMethod);
            }
            if (prop.SetMethod != null)
            {
                Methods.Add(prop.SetMethod);
            }
        }

        Console.WriteLine($"Найдено методов и свойств: {Methods.Count}");
        foreach (var method in Methods)
        {
            Console.WriteLine($"{method.Name} ({(method.IsSpecialName ? "property" : "method")})");
        }

        StatusMessage = $"Загружено {Methods.Count} методов и свойств";
    }
    catch (Exception ex)
    {
        StatusMessage = $"Ошибка загрузки методов: {ex.Message}";
        Console.WriteLine($"Ошибка при загрузке методов: {ex}");
    }
}

    [RelayCommand]
    private void LoadParameters()
    {
        Parameters.Clear();
        
        if (SelectedMethod == null)
            return;

        foreach (var param in SelectedMethod.GetParameters())
        {
            Parameters.Add(new ParameterViewModel
            {
                Name = param.Name!,
                Type = param.ParameterType,
                Value = GetDefaultValue(param.ParameterType)
            });
        }
    }

[RelayCommand]
private void ExecuteMethod()
{
    if (SelectedMethod == null || SelectedClassName == null || string.IsNullOrWhiteSpace(AssemblyPath))
    {
        AppendToResult("Ошибка: Не выбран метод или класс, или не указан путь к сборке");
        return;
    }

    try
    {
        var assembly = Assembly.LoadFrom(AssemblyPath);
        var type = assembly.GetType($"Geometric.Models.{SelectedClassName}") 
                   ?? throw new InvalidOperationException("Тип не найден");

        AppendToResult($"\n=== Начало выполнения метода {SelectedMethod.Name} ===");

        // Проверка соответствия текущего экземпляра выбранному классу
        if (CurrentInstance != null && CurrentInstance.GetType() != type)
        {
            AppendToResult($"Обнаружено несоответствие типов: текущий {CurrentInstance.GetType().Name}, требуется {type.Name}");
            AppendToResult("Создаем новый экземпляр нужного типа...");
            CurrentInstance = null;
        }

        // Создание нового экземпляра при необходимости
        if (CurrentInstance == null)
        {
            AppendToResult("Создание нового экземпляра...");
            
            // Пытаемся использовать выбранный конструктор
            if (SelectedConstructor != null)
            {
                AppendToResult($"Используем выбранный конструктор: {SelectedConstructor}");
                
                var ctorParams = SelectedConstructor.GetParameters();
                var ctorValues = new object?[ctorParams.Length];
                
                for (int i = 0; i < ctorParams.Length; i++)
                {
                    if (i < Parameters.Count && Parameters[i].Value != null)
                    {
                        ctorValues[i] = Convert.ChangeType(Parameters[i].Value, ctorParams[i].ParameterType);
                        AppendToResult($"  Параметр {ctorParams[i].Name} ({ctorParams[i].ParameterType.Name}) = {ctorValues[i]}");
                    }
                    else
                    {
                        ctorValues[i] = GetDefaultValue(ctorParams[i].ParameterType);
                        AppendToResult($"  Параметр {ctorParams[i].Name} ({ctorParams[i].ParameterType.Name}) = [по умолчанию] {ctorValues[i]}");
                    }
                }
                
                CurrentInstance = SelectedConstructor.Invoke(ctorValues);
            }
            else
            {
                // Используем первый доступный конструктор с параметрами по умолчанию
                var defaultCtor = type.GetConstructors().FirstOrDefault();
                if (defaultCtor == null)
                    throw new InvalidOperationException("Нет доступного конструктора");

                var defaultParams = defaultCtor.GetParameters();
                var defaultValues = defaultParams.Select(p => 
                {
                    var value = GetDefaultValue(p.ParameterType);
                    AppendToResult($"  Параметр конструктора: {p.Name} ({p.ParameterType.Name}) = {value}");
                    return value;
                }).ToArray();
                
                CurrentInstance = defaultCtor.Invoke(defaultValues);
            }
            
            AppendToResult($"Создан новый экземпляр: {CurrentInstance?.ToString() ?? "null"}");
        }
        else
        {
            AppendToResult($"Используется существующий экземпляр: {CurrentInstance?.ToString() ?? "null"}");
        }

        // Обработка set-методов
        if (SelectedMethod.Name.StartsWith("set_"))
        {
            var propertyName = SelectedMethod.Name.Substring(4);
            if (Parameters.Count == 0 || Parameters[0].Value == null)
            {
                AppendToResult("Ошибка: Не указано значение для установки свойства");
                return;
            }

            var paramValue = Convert.ChangeType(Parameters[0].Value, SelectedMethod.GetParameters()[0].ParameterType);
            
            // Получаем текущее значение свойства
            var getMethod = CurrentInstance?.GetType().GetMethod($"get_{propertyName}");
            var oldValue = getMethod?.Invoke(CurrentInstance, null);
            
            AppendToResult($"\nИзменение свойства {propertyName}:");
            AppendToResult($"  Старое значение: {oldValue ?? "null"}");
            AppendToResult($"  Новое значение: {paramValue}");
            
            // Устанавливаем новое значение
            SelectedMethod.Invoke(CurrentInstance, new[] { paramValue });
            
            // Получаем обновленное значение для проверки
            var newValue = getMethod?.Invoke(CurrentInstance, null);
            AppendToResult($"  Проверка: {newValue ?? "null"}");
            
            StatusMessage = $"Свойство {propertyName} изменено";
            return;
        }

        // Обработка обычных методов
        AppendToResult($"\nВызов метода {SelectedMethod.Name}");
        
        object? result;
        if (SelectedMethod.GetParameters().Length > 0)
        {
            var methodParams = SelectedMethod.GetParameters();
            var methodValues = new object?[methodParams.Length];
            
            for (int i = 0; i < methodParams.Length; i++)
            {
                if (i < Parameters.Count && Parameters[i].Value != null)
                {
                    methodValues[i] = Convert.ChangeType(Parameters[i].Value, methodParams[i].ParameterType);
                    AppendToResult($"  Параметр {methodParams[i].Name} ({methodParams[i].ParameterType.Name}) = {methodValues[i]}");
                }
                else
                {
                    methodValues[i] = GetDefaultValue(methodParams[i].ParameterType);
                    AppendToResult($"  Параметр {methodParams[i].Name} ({methodParams[i].ParameterType.Name}) = [по умолчанию] {methodValues[i]}");
                }
            }
            
            result = SelectedMethod.Invoke(CurrentInstance, methodValues);
        }
        else
        {
            AppendToResult("  Метод без параметров");
            result = SelectedMethod.Invoke(CurrentInstance, null);
        }

        AppendToResult($"Результат выполнения: {result ?? "void"}");
        StatusMessage = "Метод успешно выполнен";
    }
    catch (Exception ex)
    {
        AppendToResult($"\nОШИБКА: {ex.Message}");
        StatusMessage = "Ошибка выполнения метода";
        
        // При ошибках типа сбрасываем текущий экземпляр
        if (ex.InnerException is TargetInvocationException tie && 
            tie.InnerException is ArgumentException)
        {
            CurrentInstance = null;
            AppendToResult("Текущий экземпляр сброшен из-за ошибки типа");
        }
    }
    finally
    {
        AppendToResult("=== Завершение выполнения метода ===");
    }
}

private void AppendToResult(string? text)
{
    if (string.IsNullOrEmpty(text)) return;
    
    Result = string.IsNullOrEmpty(Result) 
        ? text 
        : Result + "\n" + text;
}
[RelayCommand]
private void ShowCurrentState()
{
    if (CurrentInstance == null)
    {
        AppendToResult("Нет активного экземпляра");
        return;
    }
    
    AppendToResult("\n=== Текущее состояние объекта ===");
    AppendToResult(CurrentInstance.ToString());
    
    // Дополнительно выводим все свойства
    var type = CurrentInstance.GetType();
    foreach (var prop in type.GetProperties())
    {
        try 
        {
            var value = prop.GetValue(CurrentInstance);
            AppendToResult($"  {prop.Name}: {value}");
        }
        catch {}
    }
}
    private static object? GetDefaultValue(Type type)
    {
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }
    partial void OnSelectedClassNameChanged(string? value)
{
    CurrentInstance = null; // Сбрасываем текущий экземпляр при смене класса
    LoadMethodsCommand.Execute(null);
}
}

public class ParameterViewModel
{
    public string Name { get; set; } = string.Empty;
    public Type Type { get; set; } = typeof(object);
    public object? Value { get; set; }
}