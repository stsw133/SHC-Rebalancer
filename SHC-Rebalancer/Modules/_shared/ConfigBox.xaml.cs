using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace SHC_Rebalancer;
/// <summary>
/// Interaction logic for ConfigBox.xaml
/// </summary>
public partial class ConfigBox : StswComboBox
{
    public StswCommand<object?> ReloadConfigsCommand { get; }
    public StswAsyncCommand AddConfigCommand { get; }
    public StswAsyncCommand RenameConfigCommand { get; }
    public StswAsyncCommand OpenConfigCommand { get; }
    public StswAsyncCommand OpenDirectoryCommand { get; }
    public StswAsyncCommand RemoveConfigCommand { get; }

    public ConfigBox()
    {
        InitializeComponent();

        ReloadConfigsCommand = new(ReloadConfigs);
        AddConfigCommand = new(AddConfig);
        RenameConfigCommand = new(RenameConfig, () => !SettingsService.Instance.Settings.SelectedConfigs[Type].In(null, "vanilla"));
        OpenConfigCommand = new(OpenConfig, () => SettingsService.Instance.Settings.SelectedConfigs[Type] != null);
        OpenDirectoryCommand = new(OpenDirectory, () => SettingsService.Instance.Settings.SelectedConfigs[Type] != null);
        RemoveConfigCommand = new(RemoveConfig, () => !SettingsService.Instance.Settings.SelectedConfigs[Type].In(null, "vanilla"));
    }

    /// OnApplyTemplate
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (!string.IsNullOrEmpty(Type))
        {
            SubControls = [.. (IStswSubControl[])Resources["SubControls"]];
            //foreach (var subControl in SubControls)
            //    BindingOperations.SetBinding((FrameworkElement)subControl, IsEnabledProperty, new Binding(nameof(IsReadOnly))
            //    {
            //        Source = this,
            //        Converter = StswBoolConverter.Instance,
            //        ConverterParameter = "!"
            //    });

            if (SubControls?[1] is ItemsControl menuButton)
            {
                menuButton.DataContext = this;

                var buttons = menuButton.Items.Cast<StswButton>().ToList();
                for (var i = menuButton.Items.Count - 1; i >= 0; i--)
                {
                    if (buttons[i].Tag == null)
                        continue;

                    if (Type == "aiv" && buttons[i].Tag.ToString() != "aiv")
                        menuButton.Items.Remove(buttons[i]);
                    else if (Type != "aiv" && buttons[i].Tag.ToString() == "aiv")
                        menuButton.Items.Remove(buttons[i]);
                }
            }
        }
    }



    /// ReloadConfigs
    private void ReloadConfigs(object? parameter)
    {
        try
        {
            var selectedRebalance = SettingsService.Instance.Settings.SelectedConfigs[Type];

            if (!StorageService.Configs.ContainsKey(Type))
                StorageService.Configs[Type] = [];

            var newConfigs = StorageService.LoadConfigs(Type)[Type];

            StorageService.Configs[Type].Clear();
            foreach (var item in newConfigs)
                StorageService.Configs[Type].Add(item);

            if (StorageService.Configs[Type].Any(x => x.GetPropertyValue(nameof(ConfigModel.Name))?.ToString() == selectedRebalance))
                SettingsService.Instance.Settings.SelectedConfigs[Type] = selectedRebalance;
            else if (StorageService.Configs[Type].Count > 0)
                SettingsService.Instance.Settings.SelectedConfigs[Type] = StorageService.Configs[Type].First().GetPropertyValue(nameof(ConfigModel.Name))!.ToString()!;
        }
        catch (Exception ex)
        {
            StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name, true);
        }
    }

    /// AddConfig
    private async Task AddConfig()
    {
        try
        {
            await StswContentDialog.Show(new NewConfigContext(Type), "MainContentDialog");

            var selectedRebalance = SettingsService.Instance.Settings.SelectedConfigs[Type];
            if (StorageService.Configs[Type].Any(x => x.GetPropertyValue(nameof(ConfigModel.Name))?.ToString() == selectedRebalance))
                SettingsService.Instance.Settings.SelectedConfigs[Type] = selectedRebalance;
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name, true);
        }
    }

    /// RenameConfig
    private async Task RenameConfig()
    {
        try
        {
            await StswContentDialog.Show(new NewConfigContext(Type, SettingsService.Instance.Settings.SelectedConfigs[Type]), "MainContentDialog");

            var selectedRebalance = SettingsService.Instance.Settings.SelectedConfigs[Type];
            var config = StorageService.Configs[Type].FirstOrDefault(x => x.GetPropertyValue(nameof(ConfigModel.Name))?.ToString() == selectedRebalance);
            config?.GetType().GetProperty(nameof(ConfigModel.Name))?.SetValue(config, selectedRebalance);

            SelectedItem = null;
            await Dispatcher.InvokeAsync(() => SelectedItem = config, System.Windows.Threading.DispatcherPriority.Background);
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name, true);
        }
    }

    /// OpenConfig
    private async Task OpenConfig()
    {
        try
        {
            var filePath = Path.Combine(StorageService.ConfigsPath, Type, SettingsService.Instance.Settings.SelectedConfigs[Type] + ".json");

            if (!File.Exists(filePath))
                throw new IOException("File for selected config does not exist!");

            StswFn.OpenFile(filePath);
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name, true);
        }
    }
    
    /// OpenDirectory
    private async Task OpenDirectory()
    {
        try
        {
            var directoryPath = Path.Combine(StorageService.ConfigsPath, Type, SettingsService.Instance.Settings.SelectedConfigs[Type] ?? string.Empty);

            if (!Directory.Exists(directoryPath))
                throw new IOException("Directory for selected config does not exist!");

            StswFn.OpenFile(directoryPath);
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name, true);
        }
    }

    /// RemoveConfig
    private async Task RemoveConfig()
    {
        try
        {
            if (SettingsService.Instance.Settings.SelectedConfigs[Type] == "vanilla")
            {
                await StswMessageDialog.Show("`vanilla` config cannot be removed.", "Information", null, StswDialogButtons.OK, StswDialogImage.Information);
                return;
            }

            var filePath = Path.Combine(StorageService.ConfigsPath, Type, SettingsService.Instance.Settings.SelectedConfigs[Type] + ".json");
            if (await StswMessageDialog.Show($"Are you sure you want to remove '{SettingsService.Instance.Settings.SelectedConfigs[Type]}' config?", "Confirmation", null, StswDialogButtons.YesNo, StswDialogImage.Question) == true)
            {
                File.Delete(filePath);
                StorageService.Configs[Type].Remove(StorageService.Configs[Type].First(x => x.GetPropertyValue(nameof(ConfigModel.Name))?.ToString() == SettingsService.Instance.Settings.SelectedConfigs[Type]));
            }
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name, true);
        }
    }



    /// NotifyConfigsChanged
    public void NotifyConfigsChanged(string name)
    {
        if (DataContext is StswObservableObject observableObject)
            observableObject.GetType().GetMethod("OnPropertyChanged", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(observableObject, [name]);
    }

    /// Type
    public string Type
    {
        get => (string)GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }
    public static readonly DependencyProperty TypeProperty
        = DependencyProperty.Register(
            nameof(Type),
            typeof(string),
            typeof(ConfigBox),
            new PropertyMetadata(default(string), null, CoerceTypeValue)
        );
    private static object CoerceTypeValue(DependencyObject d, object baseValue) => baseValue is string str ? str.ToLower() : baseValue;
}
