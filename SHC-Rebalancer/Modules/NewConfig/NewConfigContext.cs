using System.IO;
using System.Reflection;

namespace SHC_Rebalancer;
public class NewConfigContext : StswObservableObject
{
    public StswAsyncCommand SaveChangesCommand { get; }

    public NewConfigContext(bool isEditing = false)
    {
        IsEditing = isEditing;
        if (isEditing)
            Name = Settings.Default.RebalanceName;

        SaveChangesCommand = new(SaveChanges, () => !string.IsNullOrEmpty(Name));
    }



    /// SaveChanges
    public async Task SaveChanges()
    {
        try
        {
            var filePath = Path.Combine(Storage.PathRebalances, Name + ".json");
            
            if (IsEditing)
            {
                if (Storage.Rebalances.ContainsKey(Name) && Name != Settings.Default.RebalanceName)
                {
                    await StswMessageDialog.Show("Selected name is already taken!", "Blockade", null, StswDialogButtons.OK, StswDialogImage.Blockade);
                    return;
                }

                var selectedFilePath = Path.Combine(Storage.PathRebalances, Settings.Default.RebalanceName + ".json");
                if (File.Exists(selectedFilePath))
                {
                    File.Move(selectedFilePath, filePath);
                    Storage.Rebalances.ChangeKey(Settings.Default.RebalanceName, Name);
                    Storage.Rebalances[Name].Key = Name;
                    Settings.Default.RebalanceName = Name;

                    StswContentDialog.Close("MainContentDialog");
                }
            }
            else
            {
                if (Storage.Rebalances.ContainsKey(Name))
                {
                    await StswMessageDialog.Show("Selected name is already taken!", "Blockade", null, StswDialogButtons.OK, StswDialogImage.Blockade);
                    return;
                }

                var baseFilePath = Path.Combine(Storage.PathRebalances, BasedOn + ".json");
                if (!File.Exists(baseFilePath) && !IsEditing)
                {
                    await StswMessageDialog.Show("File for base rebalance does not exist!", "Error", null, StswDialogButtons.OK, StswDialogImage.Error);
                    return;
                }

                if (!File.Exists(filePath))
                {
                    File.Copy(baseFilePath, filePath);
                    Storage.Rebalances.TryAdd(Name, Storage.Rebalances[BasedOn]);
                    Storage.Rebalances[Name].Key = Name;
                    Settings.Default.RebalanceName = Name;

                    StswContentDialog.Close("MainContentDialog");
                }
            }
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name, true);
        }
    }



    /// BaseOnRebalanceName
    public string BasedOn
    {
        get => _basedOn;
        set => SetProperty(ref _basedOn, value);
    }
    private string _basedOn = "vanilla";

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
}
