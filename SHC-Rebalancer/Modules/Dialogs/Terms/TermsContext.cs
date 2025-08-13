using System.Reflection;
using System.Windows;

namespace SHC_Rebalancer;
public partial class TermsContext : StswObservableObject
{
    [StswCommand]
    async Task AcceptTerms(bool parameter)
    {
        try
        {
            if (parameter)
            {
                SettingsService.Instance.Settings.TermsAccepted = true;
                StswContentDialog.Close("MainContentDialog");
            }
            else Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            await StswMessageDialog.Show(ex, MethodBase.GetCurrentMethod()?.Name);
        }
    }
    
    [StswObservableProperty] bool _termsAccepted = SettingsService.Instance.Settings.TermsAccepted;
}
