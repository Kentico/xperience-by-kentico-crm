using Kentico.Xperience.CRM.Salesforce.Configuration;
using Microsoft.Extensions.Options;
using Salesforce.OpenApi;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text.Json;
using System.Web;
using SalesforceApiClient = Salesforce.OpenApi.SalesforceApiClient;

namespace Kentico.Xperience.CRM.Salesforce.Synchronization;

internal class SalesforceApiService : ISalesforceApiService
{
    private readonly HttpClient httpClient;
    private readonly IOptionsSnapshot<SalesforceIntegrationSettings> integrationSettings;
    private readonly SalesforceApiClient apiClient;
    
    private static JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.General)
    {
        Converters = { new DateTimeOffsetConverter() }
    };

    public SalesforceApiService(
        HttpClient httpClient,
        IOptionsSnapshot<SalesforceIntegrationSettings> integrationSettings
    )
    {
        this.httpClient = httpClient;
        this.integrationSettings = integrationSettings;

        apiClient = new SalesforceApiClient(httpClient);
    }

    public async Task<SaveResult> CreateLeadAsync(LeadSObject lead)
        => await apiClient.LeadPOSTAsync(MediaTypeNames.Application.Json, lead);


    public async Task UpdateLeadAsync(string id, LeadSObject leadSObject)
        => await apiClient.LeadPATCHAsync(id, MediaTypeNames.Application.Json, leadSObject);


    public async Task<LeadSObject?> GetLeadById(string id, string? fields = null)
        => await apiClient.LeadGET2Async(id, fields);

    public async Task<string?> GetLeadByEmail(string email)
        => await GetEntityIdByEmail(email, "Lead");

    public async Task<SaveResult> CreateContactAsync(ContactSObject contact)
        => await apiClient.ContactPOSTAsync(MediaTypeNames.Application.Json, contact);

    public async Task UpdateContactAsync(string id, ContactSObject contact)
        => await apiClient.ContactPATCHAsync(id, MediaTypeNames.Application.Json, contact);

    public async Task<ContactSObject?> GetContactById(string id, string? fields = null)
        => await apiClient.ContactGET2Async(id, fields);

    public async Task<string?> GetContactByEmail(string email)
        => await GetEntityIdByEmail(email, "Contact");

    public async Task<IEnumerable<LeadSObject>> GetModifiedLeadsAsync(DateTime lastSync)
        => await GetModifiedRecords<LeadSObject>("Lead", lastSync);

    public async Task<IEnumerable<ContactSObject>> GetModifiedContactsAsync(DateTime lastSync)
        => await GetModifiedRecords<ContactSObject>("Contact", lastSync);

    private async Task<IEnumerable<TModel>> GetModifiedRecords<TModel>(string entityName, DateTime lastSyc)
    where TModel : class
    {
        var apiVersion = integrationSettings.Value.ApiConfig.ApiVersion.ToString("F1", CultureInfo.InvariantCulture);
        using var request = new HttpRequestMessage(HttpMethod.Get,
            $"/services/data/v{apiVersion}/query?q=SELECT+FIELDS(ALL)+FROM+{entityName}+WHERE+LastModifiedDate+>=+{lastSyc.ToUniversalTime():O}+LIMIT+200");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
        var response = await httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            var queryResult = await response.Content.ReadFromJsonAsync<QueryResult<TModel>>(SerializerOptions);
            return queryResult?.Records ?? Enumerable.Empty<TModel>();
        }
        else
        {
            string responseMessage = await response.Content.ReadAsStringAsync();
            throw new ApiException("Unexpected response", (int)response.StatusCode, responseMessage, null!, null);
        }
    }

    private async Task<string?> GetEntityIdByEmail(string email, string entityName)
    {
        var apiVersion = integrationSettings.Value.ApiConfig.ApiVersion.ToString("F1", CultureInfo.InvariantCulture);
        using var request = new HttpRequestMessage(HttpMethod.Get,
                $"/services/data/v{apiVersion}/query?q=SELECT+Id+FROM+{entityName}+WHERE+Email='{HttpUtility.UrlEncode(email)}'+ORDER+BY+CreatedDate+DESC");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
        var response = await httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            var queryResult = await response.Content.ReadFromJsonAsync<QueryResult<LeadSObject>>(SerializerOptions);
            return queryResult?.Records.FirstOrDefault()?.Id;
        }
        else
        {
            string responseMessage = await response.Content.ReadAsStringAsync();
            throw new ApiException("Unexpected response", (int)response.StatusCode, responseMessage, null!, null);
        }
    }
}