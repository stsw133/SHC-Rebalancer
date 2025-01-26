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
            var filePath = Path.Combine(Storage.ConfigsPath, Type, Name + ".json");

            if (IsEditing)
            {
                if (Storage.Configs[Type].Cast<IConfigModel>().Any(x => x.Name == Name) && Name != Settings.Default["ConfigName_" + Type].ToString())
                {
                    await StswMessageDialog.Show("Selected name is already taken!", "Blockade", null, StswDialogButtons.OK, StswDialogImage.Blockade);
                    return;
                }

                var selectedFilePath = Path.Combine(Storage.ConfigsPath, Type, Settings.Default["ConfigName_" + Type].ToString() + ".json");
                if (!File.Exists(selectedFilePath))
                {
                    await StswMessageDialog.Show("Edited file does not exist!", "Error", null, StswDialogButtons.OK, StswDialogImage.Error);
                    return;
                }

                File.Move(selectedFilePath, filePath);
                Storage.Configs[Type].Cast<IConfigModel>().First(x => x.Name == Settings.Default["ConfigName_" + Type].ToString()!).Name = Name;
                Settings.Default["ConfigName_" + Type] = Name;

                StswContentDialog.Close("MainContentDialog");
            }
            else
            {
                if (Storage.Configs[Type].Cast<IConfigModel>().Any(x => x.Name == Name))
                {
                    await StswMessageDialog.Show("Selected name is already taken!", "Blockade", null, StswDialogButtons.OK, StswDialogImage.Blockade);
                    return;
                }

                var baseFilePath = Path.Combine(Storage.ConfigsPath, Type, BasedOn + ".json");
                if (!File.Exists(baseFilePath) && !IsEditing)
                {
                    await StswMessageDialog.Show("File for base config does not exist!", "Error", null, StswDialogButtons.OK, StswDialogImage.Error);
                    return;
                }

                if (!File.Exists(filePath))
                {
                    File.Copy(baseFilePath, filePath);
                    if (!Storage.Configs[Type].Cast<IConfigModel>().Any(x => x.Name == Name))
                    {
                        var newConfig = Storage.ReadJsonFileAsModel<IConfigModel>(filePath, Type)!;
                        newConfig.Name = Name;
                        Storage.Configs[Type].Add(newConfig);
                    }
                    Settings.Default["ConfigName_" + Type] = Name;

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
    public IEnumerable<string?> ConfigNames => Storage.Configs[Type].Select(x => x.GetPropertyValue("Name")?.ToString());

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
