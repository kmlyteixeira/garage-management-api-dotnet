using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using NSubstitute;
using OpenIddict.Abstractions;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.OpenIddict.Applications;
using Volo.Abp.OpenIddict.Scopes;
using Volo.Abp.PermissionManagement;
using Xunit;

#nullable disable
#pragma warning disable CS4014

namespace GarageManagement.OpenIddict;

public class OpenIddictDataSeedContributorTests
{
    private readonly IConfiguration _configuration;
    private readonly IOpenIddictApplicationRepository _applicationRepository;
    private readonly IAbpApplicationManager _applicationManager;
    private readonly IOpenIddictScopeRepository _scopeRepository;
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly IPermissionDataSeeder _permissionDataSeeder;
    private readonly IStringLocalizer<OpenIddictResponse> _localizer;
    private readonly OpenIddictDataSeedContributor _seedContributor;

    public OpenIddictDataSeedContributorTests()
    {
        _configuration = Substitute.For<IConfiguration>();
        _applicationRepository = Substitute.For<IOpenIddictApplicationRepository>();
        _applicationManager = Substitute.For<IAbpApplicationManager>();
        _scopeRepository = Substitute.For<IOpenIddictScopeRepository>();
        _scopeManager = Substitute.For<IOpenIddictScopeManager>();
        _permissionDataSeeder = Substitute.For<IPermissionDataSeeder>();
        _localizer = Substitute.For<IStringLocalizer<OpenIddictResponse>>();

        // Configure localizer to return localized strings with actual keys
        _localizer["NoClientSecretCanBeSetForPublicApplications"].Returns(
            new LocalizedString("NoClientSecretCanBeSetForPublicApplications", "NoClientSecretCanBeSetForPublicApplications"));
        _localizer["TheClientSecretIsRequiredForConfidentialApplications"].Returns(
            new LocalizedString("TheClientSecretIsRequiredForConfidentialApplications", "TheClientSecretIsRequiredForConfidentialApplications"));
        _localizer["InvalidRedirectUri", Arg.Any<object[]>()].Returns(
            x => new LocalizedString("InvalidRedirectUri", "InvalidRedirectUri"));
        _localizer["InvalidPostLogoutRedirectUri", Arg.Any<object[]>()].Returns(
            x => new LocalizedString("InvalidPostLogoutRedirectUri", "InvalidPostLogoutRedirectUri"));

        _seedContributor = new OpenIddictDataSeedContributor(
            _configuration,
            _applicationRepository,
            _applicationManager,
            _scopeRepository,
            _scopeManager,
            _permissionDataSeeder,
            _localizer
        );
    }

    [Fact]
    public async Task SeedAsync_Should_Create_Scopes_And_Applications()
    {
        // Arrange
        var context = new DataSeedContext();
        _scopeRepository.FindByNameAsync(Arg.Any<string>()).Returns((OpenIddictScope)null);
        var configSection = Substitute.For<IConfigurationSection>();
        configSection["GarageManagement_App:ClientId"].Returns((string)null);
        configSection["GarageManagement_Swagger:ClientId"].Returns((string)null);
        _configuration.GetSection("OpenIddict:Applications").Returns(configSection);

        // Act
        await _seedContributor.SeedAsync(context);

        // Assert
        await _scopeManager.Received(1).CreateAsync(Arg.Is<OpenIddictScopeDescriptor>(x =>
            x.Name == "GarageManagement" &&
            x.DisplayName == "GarageManagement API" &&
            x.Resources.Contains("GarageManagement")));
    }

    [Fact]
    public async Task CreateScopesAsync_Should_Not_Create_Scope_If_Already_Exists()
    {
        // Arrange
        var existingScope = Substitute.For<OpenIddictScope>();
        _scopeRepository.FindByNameAsync("GarageManagement").Returns(existingScope);

        // Act
        await _seedContributor.SeedAsync(new DataSeedContext());

        // Assert
        await _scopeManager.DidNotReceive().CreateAsync(Arg.Any<OpenIddictScopeDescriptor>());
    }

    [Fact]
    public async Task CreateApplicationsAsync_Should_Create_App_Client_When_Configured()
    {
        // Arrange
        _scopeRepository.FindByNameAsync(Arg.Any<string>()).Returns((OpenIddictScope)null);
        _applicationRepository.FindByClientIdAsync(Arg.Any<string>()).Returns((OpenIddictApplication)null);

        var configSection = Substitute.For<IConfigurationSection>();
        configSection["GarageManagement_App:ClientId"].Returns("garage-app");
        configSection["GarageManagement_Swagger:ClientId"].Returns((string)null);
        _configuration.GetSection("OpenIddict:Applications").Returns(configSection);

        // Act
        await _seedContributor.SeedAsync(new DataSeedContext());

        // Assert
        await _applicationManager.Received(1).CreateAsync(Arg.Is<AbpApplicationDescriptor>(x =>
            x.ClientId == "garage-app" &&
            x.DisplayName == "GarageManagement App" &&
            x.ClientType == OpenIddictConstants.ClientTypes.Public));
    }

    [Fact]
    public async Task CreateApplicationsAsync_Should_Create_Swagger_Client_When_Configured()
    {
        // Arrange
        _scopeRepository.FindByNameAsync(Arg.Any<string>()).Returns((OpenIddictScope)null);
        _applicationRepository.FindByClientIdAsync(Arg.Any<string>()).Returns((OpenIddictApplication)null);

        var configSection = Substitute.For<IConfigurationSection>();
        configSection["GarageManagement_App:ClientId"].Returns((string)null);
        configSection["GarageManagement_Swagger:ClientId"].Returns("swagger-client");
        configSection["GarageManagement_Swagger:RootUrl"].Returns("https://localhost:44301");
        _configuration.GetSection("OpenIddict:Applications").Returns(configSection);

        // Act
        await _seedContributor.SeedAsync(new DataSeedContext());

        // Assert
        await _applicationManager.Received(1).CreateAsync(Arg.Is<AbpApplicationDescriptor>(x =>
            x.ClientId == "swagger-client" &&
            x.DisplayName == "Swagger Application" &&
            x.ClientType == OpenIddictConstants.ClientTypes.Public &&
            x.RedirectUris.Any(u => u.ToString().Contains("swagger/oauth2-redirect.html"))));
    }

    [Fact]
    public async Task CreateApplicationsAsync_Should_Skip_When_ClientId_Is_Null()
    {
        // Arrange
        _scopeRepository.FindByNameAsync(Arg.Any<string>()).Returns((OpenIddictScope)null);
        
        var configSection = Substitute.For<IConfigurationSection>();
        configSection["GarageManagement_App:ClientId"].Returns((string)null);
        configSection["GarageManagement_Swagger:ClientId"].Returns((string)null);
        _configuration.GetSection("OpenIddict:Applications").Returns(configSection);

        // Act
        await _seedContributor.SeedAsync(new DataSeedContext());

        // Assert
        await _applicationManager.DidNotReceive().CreateAsync(Arg.Any<AbpApplicationDescriptor>());
    }

    [Fact]
    public async Task CreateApplicationsAsync_Should_Skip_When_ClientId_Is_WhiteSpace()
    {
        // Arrange
        _scopeRepository.FindByNameAsync(Arg.Any<string>()).Returns((OpenIddictScope)null);
        
        var configSection = Substitute.For<IConfigurationSection>();
        configSection["GarageManagement_App:ClientId"].Returns("   ");
        configSection["GarageManagement_Swagger:ClientId"].Returns((string)null);
        _configuration.GetSection("OpenIddict:Applications").Returns(configSection);

        // Act
        await _seedContributor.SeedAsync(new DataSeedContext());

        // Assert
        await _applicationManager.DidNotReceive().CreateAsync(Arg.Any<AbpApplicationDescriptor>());
    }

    [Fact]
    public async Task CreateApplicationAsync_Should_Throw_When_Public_Client_Has_Secret()
    {
        // Arrange
        _applicationRepository.FindByClientIdAsync(Arg.Any<string>()).Returns((OpenIddictApplication)null);

        var method = typeof(OpenIddictDataSeedContributor)
            .GetMethod("CreateApplicationAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act & Assert
        await Should.ThrowAsync<Exception>(async () =>
        {
            await (Task)method.Invoke(_seedContributor, new object[]
            {
                "test-client",
                OpenIddictConstants.ClientTypes.Public,
                OpenIddictConstants.ConsentTypes.Implicit,
                "Test Display",
                "secret123",
                new List<string> { OpenIddictConstants.GrantTypes.Password },
                new List<string> { "GarageManagement" },
                null,
                null,
                null,
                null
            });
        });
    }

    [Fact]
    public async Task CreateApplicationAsync_Should_Throw_When_Confidential_Client_Has_No_Secret()
    {
        // Arrange
        _applicationRepository.FindByClientIdAsync(Arg.Any<string>()).Returns((OpenIddictApplication)null);

        var method = typeof(OpenIddictDataSeedContributor)
            .GetMethod("CreateApplicationAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act & Assert
        await Should.ThrowAsync<Exception>(async () =>
        {
            await (Task)method.Invoke(_seedContributor, new object[]
            {
                "test-client",
                OpenIddictConstants.ClientTypes.Confidential,
                OpenIddictConstants.ConsentTypes.Implicit,
                "Test Display",
                null,
                new List<string> { OpenIddictConstants.GrantTypes.Password },
                new List<string> { "GarageManagement" },
                null,
                null,
                null,
                null
            });
        });
    }

    [Theory]
    [InlineData("https://localhost:44301/swagger/oauth2-redirect.html", false)]  // Valid URI
    public async Task CreateApplicationAsync_Should_Validate_RedirectUri(string redirectUri, bool shouldThrow)
    {
        // Arrange
        _applicationRepository.FindByClientIdAsync(Arg.Any<string>()).Returns((OpenIddictApplication)null);

        // Act & Assert
        if (shouldThrow)
        {
            var exception = Should.Throw<BusinessException>(async () =>
            {
                var method = typeof(OpenIddictDataSeedContributor)
                    .GetMethod("CreateApplicationAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                await (Task)method.Invoke(_seedContributor, new object[]
                {
                    "test-client",
                    OpenIddictConstants.ClientTypes.Public,
                    OpenIddictConstants.ConsentTypes.Implicit,
                    "Test Display",
                    null,
                    new List<string> { OpenIddictConstants.GrantTypes.AuthorizationCode },
                    new List<string> { "GarageManagement" },
                    null,
                    redirectUri,
                    null,
                    null
                });
            });

            exception.Message.ShouldContain("InvalidRedirectUri");
        }
        else
        {
            var method = typeof(OpenIddictDataSeedContributor)
                .GetMethod("CreateApplicationAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            await (Task)method.Invoke(_seedContributor, new object[]
            {
                "test-client",
                OpenIddictConstants.ClientTypes.Public,
                OpenIddictConstants.ConsentTypes.Implicit,
                "Test Display",
                null,
                new List<string> { OpenIddictConstants.GrantTypes.AuthorizationCode },
                new List<string> { "GarageManagement" },
                null,
                redirectUri,
                null,
                null
            });

            await _applicationManager.Received(1).CreateAsync(Arg.Any<AbpApplicationDescriptor>());
        }
    }

    [Fact]
    public async Task CreateApplicationAsync_Should_Set_Required_Permissions_For_Password_Grant()
    {
        // Arrange
        _applicationRepository.FindByClientIdAsync(Arg.Any<string>()).Returns((OpenIddictApplication)null);
        AbpApplicationDescriptor createdApp = null;
        
        _applicationManager.CreateAsync(Arg.Do<AbpApplicationDescriptor>(app => createdApp = app));

        var method = typeof(OpenIddictDataSeedContributor)
            .GetMethod("CreateApplicationAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        await (Task)method.Invoke(_seedContributor, new object[]
        {
            "test-client",
            OpenIddictConstants.ClientTypes.Public,
            OpenIddictConstants.ConsentTypes.Implicit,
            "Test Display",
            null,
            new List<string> { OpenIddictConstants.GrantTypes.Password },
            new List<string> { "GarageManagement" },
            null,
            null,
            null,
            null
        });

        // Assert
        createdApp.ShouldNotBeNull();
        createdApp.Permissions.ShouldContain(OpenIddictConstants.Permissions.GrantTypes.Password);
        createdApp.Permissions.ShouldContain(OpenIddictConstants.Permissions.Endpoints.Token);
        createdApp.Permissions.ShouldContain(OpenIddictConstants.Permissions.Endpoints.Revocation);
        createdApp.Permissions.ShouldContain(OpenIddictConstants.Permissions.Endpoints.Introspection);
    }

    [Fact]
    public async Task CreateApplicationAsync_Should_Set_Required_Permissions_For_AuthorizationCode_Grant()
    {
        // Arrange
        _applicationRepository.FindByClientIdAsync(Arg.Any<string>()).Returns((OpenIddictApplication)null);
        AbpApplicationDescriptor createdApp = null;
        
        _applicationManager.CreateAsync(Arg.Do<AbpApplicationDescriptor>(app => createdApp = app));

        var method = typeof(OpenIddictDataSeedContributor)
            .GetMethod("CreateApplicationAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        await (Task)method.Invoke(_seedContributor, new object[]
        {
            "swagger-client",
            OpenIddictConstants.ClientTypes.Public,
            OpenIddictConstants.ConsentTypes.Implicit,
            "Swagger",
            null,
            new List<string> { OpenIddictConstants.GrantTypes.AuthorizationCode },
            new List<string> { "GarageManagement" },
            "https://localhost:44301",
            "https://localhost:44301/swagger/oauth2-redirect.html",
            null,
            null
        });

        // Assert
        createdApp.ShouldNotBeNull();
        createdApp.Permissions.ShouldContain(OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode);
        createdApp.Permissions.ShouldContain(OpenIddictConstants.Permissions.ResponseTypes.Code);
        createdApp.Permissions.ShouldContain(OpenIddictConstants.Permissions.Endpoints.Authorization);
        createdApp.Permissions.ShouldContain(OpenIddictConstants.Permissions.Endpoints.Token);
    }

    [Fact]
    public async Task CreateApplicationAsync_Should_Include_Scopes_In_Permissions()
    {
        // Arrange
        _applicationRepository.FindByClientIdAsync(Arg.Any<string>()).Returns((OpenIddictApplication)null);
        AbpApplicationDescriptor createdApp = null;
        
        _applicationManager.CreateAsync(Arg.Do<AbpApplicationDescriptor>(app => createdApp = app));

        var method = typeof(OpenIddictDataSeedContributor)
            .GetMethod("CreateApplicationAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var scopes = new List<string>
        {
            OpenIddictConstants.Permissions.Scopes.Profile,
            OpenIddictConstants.Permissions.Scopes.Email,
            "GarageManagement"
        };

        // Act
        await (Task)method.Invoke(_seedContributor, new object[]
        {
            "test-client",
            OpenIddictConstants.ClientTypes.Public,
            OpenIddictConstants.ConsentTypes.Implicit,
            "Test",
            null,
            new List<string> { OpenIddictConstants.GrantTypes.Password },
            scopes,
            null,
            null,
            null,
            null
        });

        // Assert
        createdApp.ShouldNotBeNull();
        createdApp.Permissions.ShouldContain(OpenIddictConstants.Permissions.Scopes.Profile);
        createdApp.Permissions.ShouldContain(OpenIddictConstants.Permissions.Scopes.Email);
    }

    [Fact]
    public async Task CreateApplicationAsync_Should_Create_New_Client_When_Not_Exists()
    {
        // Arrange
        _applicationRepository.FindByClientIdAsync("new-client").Returns((OpenIddictApplication)null);

        var method = typeof(OpenIddictDataSeedContributor)
            .GetMethod("CreateApplicationAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        await (Task)method.Invoke(_seedContributor, new object[]
        {
            "new-client",
            OpenIddictConstants.ClientTypes.Public,
            OpenIddictConstants.ConsentTypes.Implicit,
            "New Client",
            null,
            new List<string> { OpenIddictConstants.GrantTypes.Password },
            new List<string> { "GarageManagement" },
            null,
            null,
            null,
            null
        });

        // Assert
        await _applicationManager.Received(1).CreateAsync(Arg.Any<AbpApplicationDescriptor>());
        await _applicationManager.DidNotReceive().UpdateAsync(Arg.Any<AbpApplicationDescriptor>());
    }

    [Fact]
    public async Task CreateApplicationAsync_Should_Seed_Permissions_When_Provided()
    {
        // Arrange
        _applicationRepository.FindByClientIdAsync("test-client").Returns((OpenIddictApplication)null);

        var permissions = new List<string> { "GarageManagement.Customers.Create", "GarageManagement.Customers.Update" };

        var method = typeof(OpenIddictDataSeedContributor)
            .GetMethod("CreateApplicationAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        await (Task)method.Invoke(_seedContributor, new object[]
        {
            "test-client",
            OpenIddictConstants.ClientTypes.Public,
            OpenIddictConstants.ConsentTypes.Implicit,
            "Test",
            null,
            new List<string> { OpenIddictConstants.GrantTypes.Password },
            new List<string> { "GarageManagement" },
            null,
            null,
            null,
            permissions
        });

        // Assert
        await _permissionDataSeeder.Received(1).SeedAsync(
            "C",
            "test-client",
            Arg.Is<IEnumerable<string>>(p => p.SequenceEqual(permissions)),
            null
        );
    }

    [Fact]
    public async Task CreateApplicationAsync_Should_Accept_Valid_RedirectUri()
    {
        // Arrange
        _applicationRepository.FindByClientIdAsync(Arg.Any<string>()).Returns((OpenIddictApplication)null);
        AbpApplicationDescriptor createdApp = null;
        
        _applicationManager.CreateAsync(Arg.Do<AbpApplicationDescriptor>(app => createdApp = app));

        var method = typeof(OpenIddictDataSeedContributor)
            .GetMethod("CreateApplicationAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Act
        await (Task)method.Invoke(_seedContributor, new object[]
        {
            "swagger-client",
            OpenIddictConstants.ClientTypes.Public,
            OpenIddictConstants.ConsentTypes.Implicit,
            "Swagger",
            null,
            new List<string> { OpenIddictConstants.GrantTypes.AuthorizationCode },
            new List<string> { "GarageManagement" },
            "https://localhost:44301",
            "https://localhost:44301/swagger/oauth2-redirect.html",
            null,
            null
        });

        // Assert
        createdApp.ShouldNotBeNull();
        createdApp.RedirectUris.ShouldContain(new Uri("https://localhost:44301/swagger/oauth2-redirect.html"));
        await _applicationManager.Received(1).CreateAsync(Arg.Any<AbpApplicationDescriptor>());
    }
}
