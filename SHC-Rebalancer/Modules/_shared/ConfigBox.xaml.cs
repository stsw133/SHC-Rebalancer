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
    public ConfigBox()
    {
        InitializeComponent();
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (!string.IsNullOrEmpty(Type))
        {
            SubControls = [.. (IStswSubControl[])Resources["SubControls"]];

            if (SubControls?[0] is Button menuButton1)
            {
                menuButton1.Command = StswCommands.Clear;
                menuButton1.CommandParameter = this;
            }

            if (SubControls?[1] is ItemsControl menuButton2)
            {
                menuButton2.DataContext = this;
            }
        }
    }

    [StswCommand]
    void ReloadConfigs(object? parameter)
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
            StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name);
        }
    }

    [StswCommand]
    async Task AddConfig()
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
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name);
        }
    }

    [StswCommand(ConditionMethodName = nameof(RenameConfigCondition))]
    async Task RenameConfig()
    {
        try
        {
            await StswContentDialog.Show(new NewConfigContext(Type, SettingsService.Instance.Settings.SelectedConfigs[Type]!), "MainContentDialog");

            var selectedRebalance = SettingsService.Instance.Settings.SelectedConfigs[Type];
            var config = StorageService.Configs[Type].FirstOrDefault(x => x.GetPropertyValue(nameof(ConfigModel.Name))?.ToString() == selectedRebalance);
            config?.GetType().GetProperty(nameof(ConfigModel.Name))?.SetValue(config, selectedRebalance);

            SelectedItem = null;
            await Dispatcher.InvokeAsync(() => SelectedItem = config, System.Windows.Threading.DispatcherPriority.Background);
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name);
        }
    }
    bool RenameConfigCondition() => !SettingsService.Instance.Settings.SelectedConfigs[Type].In(null, "vanilla");

    [StswCommand(ConditionMethodName = nameof(OpenConfigCondition))]
    async Task OpenConfig()
    {
        try
        {
            var filePath = Path.Combine(StorageService.ConfigsPath, Type, SettingsService.Instance.Settings.SelectedConfigs[Type] + ".json");

            if (!File.Exists(filePath))
                throw new IOException("File for selected config does not exist!");

            StswFn.OpenPath(filePath);
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name);
        }
    }
    bool OpenConfigCondition() => SettingsService.Instance.Settings.SelectedConfigs[Type] != null;

    [StswCommand(ConditionMethodName = nameof(OpenDirectoryCondition))]
    async Task OpenDirectory()
    {
        try
        {
            var directoryPath = Path.Combine(StorageService.ConfigsPath, Type, SettingsService.Instance.Settings.SelectedConfigs[Type] ?? string.Empty);
            if (!Directory.Exists(directoryPath))
                directoryPath = Path.Combine(StorageService.ConfigsPath, Type);

            if (!Directory.Exists(directoryPath))
                throw new IOException("Directory for selected config does not exist!");

            StswFn.OpenPath(directoryPath);
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name);
        }
    }
    bool OpenDirectoryCondition() => SettingsService.Instance.Settings.SelectedConfigs[Type] != null;

    [StswCommand(ConditionMethodName = nameof(RemoveConfigCondition))]
    async Task RemoveConfig()
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
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name);
        }
    }
    bool RemoveConfigCondition() => !SettingsService.Instance.Settings.SelectedConfigs[Type].In(null, "vanilla");

    public void NotifyConfigsChanged(string name)
    {
        if (DataContext is StswObservableObject observableObject)
            observableObject.GetType().GetMethod("OnPropertyChanged", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(observableObject, [name]);
    }

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
