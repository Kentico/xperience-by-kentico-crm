# Usage Guide

## Screenshots

![Dynamics settings](../images/screenshots/Dynamics_CRM_settings.png "Dynamics CRM settings")

## CRM settings

There are 2 options how to fill settings:
- use CMS settings: CRM integration settings category is created after first run.
  This is primary option when you don't specify IConfiguration section during services registration.
- use application settings: appsettings.json (API config is recommended to have in [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-6.0&tabs=windows))

Integration uses OAuth client credentials scheme, so you have to setup your CRM environment to enable for using API with
client id and client secret:
- [Dynamics](https://learn.microsoft.com/en-us/power-apps/developer/data-platform/authenticate-oauth)
- [SalesForce](https://help.salesforce.com/s/articleView?id=sf.remoteaccess_oauth_client_credentials_flow.htm&type=5)

### CRM settings description

| Setting                 | Description                                                                          |
|-------------------------|--------------------------------------------------------------------------------------|
| Forms enabled           | If enabled form submissions for registered forms are sent to CRM Leads               |
| Contacts enabled (TBD)  | If enabled online marketing contacts are synced to CRM Leads or Contacts             |
| Ignore existing records | If enabled then no updates in CRM will be performed on records with same ID or email |
| CRM URL                 | Base Dynamics / SalesForce instance URL                                              |
| Client ID               | Client ID for OAuth 2.0 client credentials scheme                                    |
| Client secret           | Client secret for OAuth 2.0 client credentials scheme                                |

### Dynamics settings
Fill settings in CMS or use this appsettings:
```json
{
  "CMSDynamicsCRMIntegration": {
    "FormLeadsEnabled": true,
    "IgnoreExistingRecords": false,
    "ApiConfig": {
      "DynamicsUrl": "",
      "ClientId": "",
      "ClientSecret": ""
    }
  }
}
```

### SalesForce settings
Fill settings in CMS or use this app settings:
```json
{
  "CMSSalesForceCRMIntegration": {
    "FormLeadsEnabled": true,
    "IgnoreExistingRecords": false,
    "ApiConfig": {
      "SalesForceUrl": "",
      "ClientId": "",
      "ClientSecret": ""
    }
  }
}
```

You can also set specific API version for SalesForce REST API (default version is 59).

```json
{
  "CMSSalesForceCRMIntegration:ApiConfig:ApiVersion": 59
}
```

## Forms data - Leads integration

Configure mapping for each form between Kentico Form fields and Dynamics Lead entity fields:

### Dynamics Sales
Added form with auto mapping based on Form field mapping to Contacts atttibutes. Uses CMS settings:
```csharp
 // Program.cs

 var builder = WebApplication.CreateBuilder(args);

 // ...
 builder.Services.AddDynamicsFormLeadsIntegration(builder =>
    builder.AddFormWithContactMapping(DancingGoatContactUsItem.CLASS_NAME));
```

Same example but with using app setting in code (**CMS setting are ignored!**):

```csharp
 // Program.cs

 var builder = WebApplication.CreateBuilder(args);

 // ...
 builder.Services.AddDynamicsFormLeadsIntegration(builder =>
    builder.AddFormWithContactMapping(DancingGoatContactUsItem.CLASS_NAME), 
    builder.Configuration.GetSection(DynamicsIntegrationSettings.ConfigKeyName));
```

Example how to add form with auto mapping combined with custom mapping and custom validation:
```csharp
 // Program.cs

 var builder = WebApplication.CreateBuilder(args);

 // ...
 builder.Services.AddDynamicsFormLeadsIntegration(builder =>
    builder.AddFormWithContactMapping(DancingGoatContactUsItem.CLASS_NAME, b => b
            .MapField<DancingGoatContactUsItem, Lead>(c => c.UserMessage, e => e.EMailAddress1))
           .AddCustomValidation<CustomFormLeadsValidationService>());
```

Example how to add form with own mapping:
```csharp
 // Program.cs

 var builder = WebApplication.CreateBuilder(args);

 // ...
 builder.Services.AddDynamicsFormLeadsIntegration(builder =>
        builder.AddForm(DancingGoatContactUsItem.CLASS_NAME, //form class name
                c => c
                    .MapField("UserFirstName", "firstname")
                    .MapField<Lead>("UserLastName", e => e.LastName) //you can map to Lead object or use own generated Lead class
                    .MapField<DancingGoatContactUsItem, Lead>(c => c.UserEmail, e => e.EMailAddress1) //generated form class used
                    .MapField<BizFormItem, Lead>(b => b.GetStringValue("UserMessage", ""), e => e.Description) //general BizFormItem used
            ));
```

Example how to add form with custom converter.
Use this option when you need complex logic and need to use another service via DI:

```csharp
 // Program.cs

 var builder = WebApplication.CreateBuilder(args);

 // ...
 builder.Services.AddDynamicsFormLeadsIntegration(builder =>
     builder.AddFormWithConverter<SomeCustomConverter>(DancingGoatContactUsItem.CLASS_NAME));
```

### SalesForce

Added form with auto mapping based on Form field mapping to Contacts atttibutes. Uses CMS settings:
```csharp
 // Program.cs

 var builder = WebApplication.CreateBuilder(args);

 // ...
 builder.Services.AddSalesForceFormLeadsIntegration(builder =>
    builder.AddFormWithContactMapping(DancingGoatContactUsItem.CLASS_NAME));
```

Same example but with using app setting in code (**CMS setting are ignored!**):

```csharp
 // Program.cs

 var builder = WebApplication.CreateBuilder(args);

 // ...
 builder.Services.AddSalesForceFormLeadsIntegration(builder =>
    builder.AddFormWithContactMapping(DancingGoatContactUsItem.CLASS_NAME),
    builder.Configuration.GetSection(SalesForceIntegrationSettings.ConfigKeyName));
```

Example how to add form with auto mapping combined with custom mapping and custom validation:
```csharp
 // Program.cs

 var builder = WebApplication.CreateBuilder(args);

 // ...
 builder.Services.AddSalesForceFormLeadsIntegration(builder =>
    builder.AddFormWithContactMapping(DancingGoatContactUsItem.CLASS_NAME, b => b
            .MapField<DancingGoatContactUsItem>(c => c.UserMessage, e => e.Description))
        .AddCustomValidation<CustomFormLeadsValidationService>());
```

Example how to add form with own mapping:
```csharp
 // Program.cs

 var builder = WebApplication.CreateBuilder(args);

 // ...
 builder.Services.AddSalesForceFormLeadsIntegration(builder =>
        builder.AddForm(DancingGoatContactUsItem.CLASS_NAME, //form class name
                c => c
                    .MapField("UserFirstName", "FirstName") //option1: mapping based on source and target field names
                    .MapField("UserLastName", e => e.LastName) //option 2: mapping source name string -> member expression to SObject
                    .MapField<DancingGoatContactUsItem>(c => c.UserEmail, e => e.Email) //option 3: source mapping function from generated BizForm object -> member expression to SObject
                    .MapField<BizFormItem>(b => b.GetStringValue("UserMessage", ""), e => e.Description) //option 4: source mapping function general BizFormItem  -> member expression to SObject
            ));
```

Example how to add form with custom converter.
Use this option when you need complex logic and need to use another service via DI:

```csharp
 // Program.cs

 var builder = WebApplication.CreateBuilder(args);

 // ...
 builder.Services.AddSalesForceFormLeadsIntegration(builder =>
     builder.AddFormWithConverter<SomeCustomConverter>(DancingGoatContactUsItem.CLASS_NAME));
```
