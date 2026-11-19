using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace App_FrontEnd.Pages;

[Authorize]
public class UserProfile : PageModel
{
    private readonly ITokenAcquisition _tokenAcquisition;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public string FirstName { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string EntraOid { get; set; } = string.Empty;
    public string CompetitorLicense { get; set; } = string.Empty;
    public string MarshalLicense { get; set; } = string.Empty;
    public string FlagUrl { get; set; } = string.Empty;
    public string PersonRowKey { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string SuccessMessage { get; set; } = string.Empty;

    public SelectList Countries { get; set; } = new SelectList(new List<CountryItem>(), "Code", "Name");
    public SelectList Regions { get; set; } = new SelectList(new List<string>());

    public UserProfile(ITokenAcquisition tokenAcquisition, IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _tokenAcquisition = tokenAcquisition;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    public async Task OnGetAsync()
    {
        await LoadReferenceDataAsync();
        await LoadProfileAsync();
    }

    public async Task<IActionResult> OnPostAsync(string firstName, string surname, string country, string region)
    {
        await LoadReferenceDataAsync();

        var baseUrl = (_configuration["apiRallyingIsLifeBaseURL"] ?? "").TrimEnd('/');
        var scopes = _configuration["DownstreamApiScopes"]?.Split(',').Select(s => s.Trim()).ToArray() ?? new[] { "Api.Write" };

        try
        {
            var token = await _tokenAcquisition.GetAccessTokenForUserAsync(scopes);
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var request = new
            {
                FirstName = firstName,
                Surname = surname,
                Country = country,
                Region = region
            };

            var response = await client.PatchAsJsonAsync($"{baseUrl}/api/userprofile", request);

            if (response.IsSuccessStatusCode)
            {
                SuccessMessage = "Profile updated successfully.";
                await LoadProfileAsync();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                ErrorMessage = $"Failed to update profile: {response.StatusCode} - {errorContent}";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error updating profile: {ex.Message}";
        }

        return Page();
    }

    private async Task LoadReferenceDataAsync()
    {
        var baseUrl = (_configuration["apiRallyingIsLifeBaseURL"] ?? "").TrimEnd('/');
        var scopes = _configuration["DownstreamApiScopes"]?.Split(',').Select(s => s.Trim()).ToArray() ?? new[] { "Api.Read" };

        try
        {
            var token = await _tokenAcquisition.GetAccessTokenForUserAsync(scopes);
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var countryList = new List<CountryItem>
            {
                new() { Code = "GB", Name = "United Kingdom" },
                new() { Code = "IE", Name = "Ireland" },
                new() { Code = "FR", Name = "France" },
                new() { Code = "DE", Name = "Germany" },
                new() { Code = "ES", Name = "Spain" },
                new() { Code = "IT", Name = "Italy" },
                new() { Code = "BE", Name = "Belgium" },
                new() { Code = "NL", Name = "Netherlands" },
                new() { Code = "FI", Name = "Finland" },
                new() { Code = "SE", Name = "Sweden" },
                new() { Code = "NO", Name = "Norway" },
                new() { Code = "DK", Name = "Denmark" },
                new() { Code = "AT", Name = "Austria" },
                new() { Code = "CH", Name = "Switzerland" },
                new() { Code = "PL", Name = "Poland" },
                new() { Code = "CZ", Name = "Czech Republic" },
                new() { Code = "US", Name = "United States" },
                new() { Code = "CA", Name = "Canada" },
                new() { Code = "AU", Name = "Australia" },
                new() { Code = "NZ", Name = "New Zealand" }
            }.OrderBy(c => c.Name).ToList();

            Countries = new SelectList(countryList, "Code", "Name");
            Regions = new SelectList(new List<string> { "England", "Scotland", "Wales", "Northern Ireland" });
        }
        catch
        {
            Countries = new SelectList(new List<CountryItem> { new() { Code = "GB", Name = "United Kingdom" } }, "Code", "Name");
            Regions = new SelectList(new List<string> { "England", "Scotland", "Wales", "Northern Ireland" });
        }
    }

    private async Task LoadProfileAsync()
    {
        var baseUrl = (_configuration["apiRallyingIsLifeBaseURL"] ?? "").TrimEnd('/');
        var scopes = _configuration["DownstreamApiScopes"]?.Split(',').Select(s => s.Trim()).ToArray() ?? new[] { "Api.Read" };

        try
        {
            var token = await _tokenAcquisition.GetAccessTokenForUserAsync(scopes);
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"{baseUrl}/api/userprofile");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<UserProfileApiResponse>();
                if (data != null)
                {
                    PersonRowKey = data.personRowKey;
                    FirstName = data.firstName;
                    Surname = data.surname;
                    Country = data.country;
                    Region = data.region;
                    EntraOid = data.entraOid;
                    CompetitorLicense = data.competitorLicense;
                    MarshalLicense = data.marshalLicense;
                    FlagUrl = data.flagUrl;
                }
            }
            else
            {
                ErrorMessage = $"Failed to load profile: {response.StatusCode}";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading profile: {ex.Message}";
        }
    }

    public class CountryItem
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class UserProfileApiResponse
    {
        public string personRowKey { get; set; } = string.Empty;
        public string firstName { get; set; } = string.Empty;
        public string surname { get; set; } = string.Empty;
        public string country { get; set; } = string.Empty;
        public string region { get; set; } = string.Empty;
        public string entraOid { get; set; } = string.Empty;
        public string competitorLicense { get; set; } = string.Empty;
        public string marshalLicense { get; set; } = string.Empty;
        public string flagUrl { get; set; } = string.Empty;
    }
}