# Xperience by Kentico CRM

[![CI: Build and Test](https://github.com/Kentico/xperience-by-kentico-crm/actions/workflows/ci.yml/badge.svg)](https://github.com/Kentico/xperience-by-kentico-crm/actions/workflows/ci.yml)

## Description

Xperience by Kentico integrations with Microsoft Dynamics and Salesforce Sales Cloud

## Library Version Matrix

The versions of this library are supported by the following versions of Xperience by Kentico

| Xperience Version | Library Version |
|-------------------|-----------------|
| >= 28.0.0         | 0.9             |

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

## Screenshots

![Dynamics settings](/images/screenshots/Dynamics_CRM_settings.png "Dynamics CRM settings")

## Quick Start

1. Fill CRM (Dynamics/SalesForce) settings (in CMS or appsettings.json)
2. Register services and setup form-lead mapping
3. Start to use

### CRM settings

There are 2 options how to fill settings:
- use CMS settings: CRM integration settings category is created after first run.
This is primary option when you don't specify IConfiguration section during services registration. 
- use application settings: [appsettings.json](./docs/Usage-Guide.md#crm-settings) (API config is recommended to have in [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-6.0&tabs=windows))

### Forms data - Leads integration

Configure mapping for each form between Kentico Form fields and Dynamics Lead entity fields:

#### Dynamics Sales
Added form with auto mapping based on Form field mapping to Contacts atttibutes. Uses CMS settings:
```csharp
 // Program.cs

 var builder = WebApplication.CreateBuilder(args);

 // ...
 builder.Services.AddDynamicsFormLeadsIntegration(builder =>
    builder.AddFormWithContactMapping(DancingGoatContactUsItem.CLASS_NAME));
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

#### SalesForce

Added form with auto mapping based on Form field mapping to Contacts atttibutes. Uses CMS settings:
```csharp
 // Program.cs

 var builder = WebApplication.CreateBuilder(args);

 // ...
 builder.Services.AddSalesForceFormLeadsIntegration(builder =>
    builder.AddFormWithContactMapping(DancingGoatContactUsItem.CLASS_NAME));
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

## Full Instructions

View the [Usage Guide](./docs/Usage-Guide.md) for more detailed instructions.

## Projects

| Project                              | Description                                                                              |
|--------------------------------------|------------------------------------------------------------------------------------------|
| src/Kentico.Xperience.CRM.Dynamics   | Xperience by Kentico Dynamics Sales CRM integration library                              |
| src/Kentico.Xperience.CRM.SalesForce | Xperience by Kentico SalesForce CRM integration library                                  |
| src/Kentico.Xperience.CRM.Common     | Xperience by Kentico common integration functionality (used by Dynamics/SalesForce libs) |
| examples/DancingGoat                 | Example project to showcase CRM integration                                              |

## Contributing

To see the guidelines for Contributing to Kentico open source software, please see [Kentico's `CONTRIBUTING.md`](https://github.com/Kentico/.github/blob/main/CONTRIBUTING.md) for more information and follow the [Kentico's `CODE_OF_CONDUCT`](https://github.com/Kentico/.github/blob/main/CODE_OF_CONDUCT.md).

Instructions and technical details for contributing to **this** project can be found in [Contributing Setup](./docs/Contributing-Setup.md).

## License

Distributed under the MIT License. See [`LICENSE.md`](./LICENSE.md) for more information.

## Support

This project has **Kentico Labs limited support**.

See [`SUPPORT.md`](https://github.com/Kentico/.github/blob/main/SUPPORT.md#full-support) for more information.

For any security issues see [`SECURITY.md`](https://github.com/Kentico/.github/blob/main/SECURITY.md).
