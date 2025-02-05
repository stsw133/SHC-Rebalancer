using System.IO;
using System.Reflection;

namespace SHC_Rebalancer;
public class NewConfigContext : StswObservableObject
{
    public StswAsyncCommand SaveChangesCommand { get; }

    public NewConfigContext(string type = "", string name = "")
    {
        Type = type.ToLower();
        Name = name;
        IsEditing = !string.IsNullOrEmpty(name);

        SaveChangesCommand = new(SaveChanges, () => !string.IsNullOrEmpty(Name));
    }



    /// SaveChanges
    public async Task SaveChanges()
    {
        try
        {
            var filePath = Path.Combine(StorageService.ConfigsPath, Type, Name + ".json");

            if (IsEditing)
            {
                if (StorageService.Configs[Type].Cast<ConfigModel>().Any(x => x.Name == Name) && Name != SettingsService.Instance.Settings.SelectedConfigs[Type])
                {
                    await StswMessageDialog.Show("Selected name is already taken!", "Blockade", null, StswDialogButtons.OK, StswDialogImage.Blockade);
                    return;
                }

                var selectedFilePath = Path.Combine(StorageService.ConfigsPath, Type, SettingsService.Instance.Settings.SelectedConfigs[Type] + ".json");
                if (!File.Exists(selectedFilePath))
                {
                    await StswMessageDialog.Show("Edited file does not exist!", "Error", null, StswDialogButtons.OK, StswDialogImage.Error);
                    return;
                }

                File.Move(selectedFilePath, filePath);
                StorageService.Configs[Type].Cast<ConfigModel>().First(x => x.Name == SettingsService.Instance.Settings.SelectedConfigs[Type]).Name = Name;
                SettingsService.Instance.Settings.SelectedConfigs[Type] = Name;

                StswContentDialog.Close("MainContentDialog");
            }
            else
            {
                if (StorageService.Configs[Type].Cast<ConfigModel>().Any(x => x.Name == Name))
                {
                    await StswMessageDialog.Show("Selected name is already taken!", "Blockade", null, StswDialogButtons.OK, StswDialogImage.Blockade);
                    return;
                }

                var baseFilePath = Path.Combine(StorageService.ConfigsPath, Type, BasedOn + ".json");
                if (!File.Exists(baseFilePath) && !IsEditing)
                {
                    await StswMessageDialog.Show("File for base config does not exist!", "Error", null, StswDialogButtons.OK, StswDialogImage.Error);
                    return;
                }

                if (!File.Exists(filePath))
                {
                    File.Copy(baseFilePath, filePath);
                    StorageService.Configs[Type].Add(StorageService.LoadConfigs(Type, Name)[Type].First()!);
                    SettingsService.Instance.Settings.SelectedConfigs[Type] = Name;

                    StswContentDialog.Close("MainContentDialog");
                }
            }
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name, true);
        }
    }



    /// BasedOn
    public string BasedOn
    {
        get => _basedOn;
        set => SetProperty(ref _basedOn, value);
    }
    private string _basedOn = "vanilla";

    /// ConfigNames
    public IEnumerable<string?> ConfigNames => StorageService.Configs[Type].Select(x => x.GetPropertyValue(nameof(ConfigModel.Name))?.ToString());

    /// IsEditing
    public bool IsEditing
    {
        get => _isEditing;
        set => SetProperty(ref _isEditing, value);
    }
    private bool _isEditing;

    /// Name
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
    private string _name = string.Empty;

    /// Type
    public string Type
    {
        get => _type;
        set => SetProperty(ref _type, value);
    }
    private string _type = string.Empty;
}
