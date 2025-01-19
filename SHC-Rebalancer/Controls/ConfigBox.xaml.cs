using System.IO;
using System.Reflection;
using System.Windows;

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
    public StswAsyncCommand RemoveConfigCommand { get; }

    public ConfigBox()
    {
        InitializeComponent();

        ReloadConfigsCommand = new(ReloadConfigs);
        AddConfigCommand = new(AddConfig);
        RenameConfigCommand = new(RenameConfig, () => Settings.Default[ConfigName]?.ToString() != null && Settings.Default[ConfigName]?.ToString() != "vanilla");
        OpenConfigCommand = new(OpenConfig, () => Settings.Default[ConfigName]?.ToString() != null);
        RemoveConfigCommand = new(RemoveConfig, () => Settings.Default[ConfigName]?.ToString() != null && Settings.Default[ConfigName]?.ToString() != "vanilla");
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (!string.IsNullOrEmpty(Type))
            SubControls = new((IStswSubControl[])Resources["SubControls"]);
    }



    /// ReloadConfigs
    private void ReloadConfigs(object? parameter)
    {
        try
        {
            var selectedRebalance = Settings.Default[ConfigName].ToString()!;

            Storage.Configs[Type] = Storage.LoadConfigs(Type)[Type].Cast<object>().ToList();
            NotifyConfigsChanged("Configs_" + Type);

            if (Storage.Configs[Type].Any(x => x.GetPropertyValue("Name")?.ToString() == selectedRebalance))
                Settings.Default[ConfigName] = selectedRebalance;
            else if (Storage.Configs[Type].Count > 0)
                Settings.Default[ConfigName] = Storage.Configs[Type].First().GetPropertyValue("Name");
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

            var selectedRebalance = Settings.Default[ConfigName].ToString()!;
            NotifyConfigsChanged("Configs_" + Type);

            if (Storage.Configs[Type].Any(x => x.GetPropertyValue("Name")?.ToString() == selectedRebalance))
                Settings.Default[ConfigName] = selectedRebalance;
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
            await StswContentDialog.Show(new NewConfigContext(Type, Settings.Default[ConfigName].ToString()!), "MainContentDialog");

            var selectedRebalance = Settings.Default[ConfigName].ToString()!;
            NotifyConfigsChanged("Configs_" + Type);

            if (Storage.Configs[Type].Any(x => x.GetPropertyValue("Name")?.ToString() == selectedRebalance))
                Settings.Default[ConfigName] = selectedRebalance;
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
            var filePath = Path.Combine(Storage.PathConfigs, Type, Settings.Default[ConfigName].ToString() + ".json");

            if (!File.Exists(filePath))
                throw new IOException("File for selected config does not exist!");

            StswFn.OpenFile(filePath);
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
            if (Settings.Default[ConfigName].ToString() == "vanilla")
            {
                await StswMessageDialog.Show("`vanilla` config cannot be removed.", "Information", null, StswDialogButtons.OK, StswDialogImage.Information);
                return;
            }

            var filePath = Path.Combine(Storage.PathConfigs, Type, Settings.Default[ConfigName] + ".json");
            if (await StswMessageDialog.Show($"Are you sure you want to remove '{Settings.Default[ConfigName]}' config?", "Confirmation", null, StswDialogButtons.YesNo, StswDialogImage.Question) == true)
            {
                File.Delete(filePath);
                Storage.Configs[Type].Remove(Storage.Configs[Type].FirstOrDefault(x => x.GetPropertyValue("Name")?.ToString() == Settings.Default[ConfigName].ToString())!);
                NotifyConfigsChanged("Configs_" + Type);
            }
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name, true);
        }
    }



    /// ConfigName
    public string ConfigName => "ConfigName_" + Type;

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
