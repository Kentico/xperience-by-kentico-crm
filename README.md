# Xperience by Kentico CRM

[![CI: Build and Test](https://github.com/Kentico/xperience-by-kentico-crm/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/Kentico/xperience-by-kentico-crm/actions/workflows/ci.yml)

## Description

Xperience by Kentico integrations with Microsoft Dynamics and Salesforce Sales Cloud

## Library Version Matrix

The versions of this library are supported by the following versions of Xperience by Kentico

| Xperience Version | Library Version |
| ----------------- | --------------- |
| >= 27.0.1         | 1.x             |

### Dependencies

- [ASP.NET Core 6.0](https://dotnet.microsoft.com/en-us/download)
- [Xperience by Kentico](https://docs.xperience.io/xp/changelog)

## Package Installation

### Dynamics Sales integration
Add the package to your application using the .NET CLI

```powershell
dotnet add package Kentico.Xperience.CRM.Dynamics
```

### SalesForce Sales integration
Add the package to your application using the .NET CLI

```powershell
dotnet add package Kentico.Xperience.CRM.SalesForce
```

## Quick Start

1. Fill CRM (Dynamics/SalesForce) settings
2. Register services and setup form-lead mapping
3. Start to use

### CRM settings

Integration uses OAuth client credentials scheme.
Fill and add this settings to appsettings.json (API config is recommended to have in appsecrets.json)

#### Dynamics settings

```json
{
  "DynamicsCRMIntegration": {
    "FormLeadsEnabled": true,
    "ApiConfig": {
      "DynamicsUrl": "",
      "ClientId": "",
      "ClientSecret": ""
    }
  }
}
```

#### SalesForce settings

```json
{
  "SalesForceCRMIntegration": {
    "FormLeadsEnabled": true,
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
  "SalesForceCRMIntegration:ApiConfig:ApiVersion": 59
}
```


### Forms data - Leads integration

Configure mapping for each form between Kentico Form fields and Dynamics Lead entity fields:

#### Dynamics Sales
   ```csharp
    // Program.cs

    var builder = WebApplication.CreateBuilder(args);

    // ...
   
   builder.Services.AddDynamicsCrmLeadsIntegration(builder =>
            builder.AddForm(DancingGoatContactUsItem.CLASS_NAME, //form class name
                    c => c
                        .MapField("UserFirstName", "firstname")
                        .MapField<Lead>("UserLastName", e => e.LastName) //you can map to Lead object or use own generated Lead class
                        .MapField<DancingGoatContactUsItem, Lead>(c => c.UserEmail, e => e.EMailAddress1) //generated form class used
                        .MapField<BizFormItem, Lead>(b => b.GetStringValue("UserMessage", ""), e => e.Description) //general BizFormItem used
                )
                .ExternalIdField("new_kenticoid") //optional custom field when you want updates to work
        ,
        builder.Configuration.GetSection(DynamicsIntegrationSettings.ConfigKeyName)); //config section with settings
   ```

You can also register custom validation service to handle if given form item should be processed to CRM:

```csharp
    //call this after AddDynamicsCrmLeadsIntegration registration
    builder.Services.AddCustomFormLeadsValidationService<CustomFormLeadsValidationService>();
```

#### SalesForce

   ```csharp
    // Program.cs

    var builder = WebApplication.CreateBuilder(args);

    // ...
   
   builder.Services.AddSalesForceCrmLeadsIntegration(builder =>
        builder.AddForm(DancingGoatContactUsItem.CLASS_NAME, //form class name
                c => c
                    .MapField("UserFirstName", "FirstName") //option1: mapping based on source and target field names
                    .MapField("UserLastName",
                        e => e.LastName) //option 2: mapping source name string -> member expression to SObject
                    .MapField<DancingGoatContactUsItem>(c => c.UserEmail, e => e.Email)
                    //option 3: source mapping function from generated BizForm object  -> member expression to SObject
                    .MapField<BizFormItem>(b => b.GetStringValue("UserMessage", ""), e => e.Description)
                //option 4: source mapping function general BizFormItem  -> member expression to SObject
            )            
            .ExternalIdField("KenticoID") //optional custom field when you want updates to work
    ,
    builder.Configuration.GetSection(SalesForceIntegrationSettings.ConfigKeyName)); //config section with settings
   ```

You can also register custom validation service to handle if given form item should be processed to CRM:

```csharp
    //call this after AddSalesForceCrmLeadsIntegration registration
    builder.Services.AddCustomFormLeadsValidationService<CustomFormLeadsValidationService>();
```

## Contributing

To see the guidelines for Contributing to Kentico open source software, please see [Kentico's `CONTRIBUTING.md`](https://github.com/Kentico/.github/blob/main/CONTRIBUTING.md) for more information and follow the [Kentico's `CODE_OF_CONDUCT`](https://github.com/Kentico/.github/blob/main/CODE_OF_CONDUCT.md).

Instructions and technical details for contributing to **this** project can be found in [Contributing Setup](./docs/Contributing-Setup.md).

## License

Distributed under the MIT License. See [`LICENSE.md`](./LICENSE.md) for more information.

## Support

This project has  **Kentico Labs limited support**.

See [`SUPPORT.md`](https://github.com/Kentico/.github/blob/main/SUPPORT.md#full-support) for more information.

For any security issues see [`SECURITY.md`](https://github.com/Kentico/.github/blob/main/SECURITY.md).
