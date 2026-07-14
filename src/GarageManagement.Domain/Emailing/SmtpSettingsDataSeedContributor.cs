using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.SettingManagement;

namespace GarageManagement.Emailing;

/* Writes SMTP credentials into Setting Management so ISettingManager encrypts the password
 * before persisting it. Reading Abp.Mailing.Smtp.Password straight from configuration would
 * fail decryption, since ABP always expects that setting's value to already be encrypted. */
public class SmtpSettingsDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private const string SmtpHostSettingName = "Abp.Mailing.Smtp.Host";
    private const string SmtpPortSettingName = "Abp.Mailing.Smtp.Port";
    private const string SmtpUserNameSettingName = "Abp.Mailing.Smtp.UserName";
    private const string SmtpPasswordSettingName = "Abp.Mailing.Smtp.Password";
    private const string SmtpEnableSslSettingName = "Abp.Mailing.Smtp.EnableSsl";
    private const string SmtpUseDefaultCredentialsSettingName = "Abp.Mailing.Smtp.UseDefaultCredentials";
    private const string DefaultFromAddressSettingName = "Abp.Mailing.DefaultFromAddress";
    private const string DefaultFromDisplayNameSettingName = "Abp.Mailing.DefaultFromDisplayName";

    private readonly IConfiguration _configuration;
    private readonly ISettingManager _settingManager;

    public SmtpSettingsDataSeedContributor(IConfiguration configuration, ISettingManager settingManager)
    {
        _configuration = configuration;
        _settingManager = settingManager;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        var smtp = _configuration.GetSection("Smtp");
        var userName = smtp["UserName"];

        if (string.IsNullOrWhiteSpace(userName))
        {
            return;
        }

        await SetIfNotEmptyAsync(SmtpUserNameSettingName, userName);
        await SetIfNotEmptyAsync(SmtpPasswordSettingName, smtp["Password"]);
        await SetIfNotEmptyAsync(SmtpHostSettingName, smtp["Host"]);
        await SetIfNotEmptyAsync(SmtpPortSettingName, smtp["Port"]);
        await SetIfNotEmptyAsync(SmtpEnableSslSettingName, smtp["EnableSsl"]);
        await SetIfNotEmptyAsync(SmtpUseDefaultCredentialsSettingName, smtp["UseDefaultCredentials"]);
        await SetIfNotEmptyAsync(DefaultFromAddressSettingName, smtp["DefaultFromAddress"] ?? userName);
        await SetIfNotEmptyAsync(DefaultFromDisplayNameSettingName, smtp["DefaultFromDisplayName"]);
    }

    private async Task SetIfNotEmptyAsync(string settingName, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        await _settingManager.SetGlobalAsync(settingName, value);
    }
}
