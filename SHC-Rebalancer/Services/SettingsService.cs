using Microsoft.Extensions.Configuration;
using System.IO;
using System.Text.Json;

namespace SHC_Rebalancer;

/// SettingsService
public class SettingsService
{
    private static readonly Lazy<SettingsService> _instance = new(() => new SettingsService());
    public static SettingsService Instance => _instance.Value;

    private readonly string _filePath = "appsettings.json";
    private readonly IConfigurationRoot _configuration;

    public AppSettings Settings { get; private set; } = new();

    private SettingsService()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(_filePath, optional: true, reloadOnChange: true);

        _configuration = builder.Build();
        Settings = new AppSettings();
        ReloadSettings();

        _configuration.GetReloadToken().RegisterChangeCallback(_ => ReloadSettings(), null);
    }

    /// ReloadSettings
    private void ReloadSettings()
    {
        _configuration.GetSection("AppSettings").Bind(Settings);
    }

    /// SaveSettings
    public void SaveSettings()
    {
        try
        {
            var json = JsonSerializer.Serialize(new { AppSettings = Settings }, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
        catch (Exception ex)
        {
            StswMessageDialog.Show(ex, $"Error while saving settings.", true);
        }
    }
}

/// AppSettings
public class AppSettings : StswObservableObject
{
    /// GamePath
    public string GamePath
    {
        get => _gamePath;
        set => SetProperty(ref _gamePath, value);
    }
    private string _gamePath = string.Empty;

    /// TermsAccepted
    public bool TermsAccepted
    {
        get => _termsAccepted;
        set => SetProperty(ref _termsAccepted, value);
    }
    private bool _termsAccepted;

    /// IncludeUCP
    public bool IncludeUCP
    {
        get => _includeUCP;
        set => SetProperty(ref _includeUCP, value);
    }
    private bool _includeUCP = true;

    /// IncludeOptions
    public bool IncludeOptions
    {
        get => _includeOptions;
        set => SetProperty(ref _includeOptions, value);
    }
    private bool _includeOptions = true;

    /// SelectedConfigs
    public ObservableDictionary<string, string?> SelectedConfigs
    {
        get => _selectedConfigs;
        set => SetProperty(ref _selectedConfigs, value);
    }
    private ObservableDictionary<string, string?> _selectedConfigs = [];
    
    /// SelectedOptions
    public ObservableDictionary<string, object?> SelectedOptions
    {
        get => _selectedOptions;
        set => SetProperty(ref _selectedOptions, value);
    }
    private ObservableDictionary<string, object?> _selectedOptions = [];
}
