# Contributing Setup

## Required Software

The requirements to setup, develop, and build this project are listed below.

### .NET Runtime

.NET SDK 7.0 or newer

- <https://dotnet.microsoft.com/en-us/download/dotnet/7.0>
- See `global.json` file for specific SDK requirements

### Node.js Runtime

- [Node.js](https://nodejs.org/en/download) 18.12.0 or newer
- [NVM for Windows](https://github.com/coreybutler/nvm-windows) to manage multiple installed versions of Node.js
- See `engines` in the solution `package.json` for specific version requirements

### C# Editor

- VS Code
- Visual Studio
- Rider

### Database

SQL Server 2019 or newer compatible database

- [SQL Server Linux](https://learn.microsoft.com/en-us/sql/linux/sql-server-linux-setup?view=sql-server-ver15)
- [Azure SQL Edge](https://learn.microsoft.com/en-us/azure/azure-sql-edge/disconnected-deployment)

### SQL Editor

- MS SQL Server Management Studio
- Azure Data Studio

## Sample Project

### Database Setup

Running the sample project requires creating a new Xperience by Kentico database using the included template.

Change directory in your console to `./examples/DancingGoat` and follow the instructions in the Xperience
documentation on [creating a new database](https://docs.xperience.io/xp26/developers-and-admins/installation#Installation-CreatetheprojectdatabaseCreateProjectDatabase).

### Admin Customization

To run the Sample app Admin customization in development mode, add the following to your [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-7.0&tabs=windows#secret-manager) for the application.

```json
"CMSAdminClientModuleSettings": {
  "kentico-xperience-integrations-crm": {
    "Mode": "Proxy",
    "Port": 3009
  }
}
```

## Admin UI Client

The administration UI customizations of this integration live in
`./src/Kentico.Xperience.CRM.Common/Client`. The built bundle is embedded in
`Kentico.Xperience.CRM.Common.dll` as an admin client module named `kentico-xperience-integrations-crm`.

The organization and project names have to stay in step in four places, otherwise the admin cannot resolve
the module at runtime and the custom pages render empty:

| Where | What |
| ----- | ---- |
| `Client/webpack.config.js` | `orgName`, `projectName` |
| `Kentico.Xperience.CRM.Common.csproj` | `AdminOrgName`, `AdminClientPath/ProjectName` |
| `Admin/CRMAdminModule.cs` | `CRMAdminClientModule.ORGANIZATION_NAME`, `PROJECT_NAME` |
| User Secrets, for Proxy mode | the `CMSAdminClientModuleSettings` key |

### Building the client

```powershell
cd ./src/Kentico.Xperience.CRM.Common/Client
npm ci
npm run build
```

- A **Release** build of the project runs `npm ci` and `npm run build` itself, so CI needs nothing more than
  Node.js on the agent.
- A **Debug** build does **not**, and instead picks up whatever is already in `Client/dist`. Run
  `npm run build` (or `npm run build:dev` for readable output) once after cloning, and again after changing
  anything under `Client/src`.
- `Client/dist` is not committed.

### Developing the client

`npm start` serves the module from the webpack dev server on port 3009 with hot reload. Add the
`CMSAdminClientModuleSettings` entry above to the sample application's User Secrets so the administration
loads the module from there instead of from the assembly.

`npm run build` runs ESLint first and fails on any violation, so lint locally before pushing.

## Development Workflow

1. Create a new branch with one of the following prefixes

   - `feat/` - for new functionality
   - `refactor/` - for restructuring of existing features
   - `fix/` - for bugfixes

1. Run `dotnet format` against the `Kentico.Xperience.CRM.sln` solution

   > use `dotnet: format` VS Code task.

1. Commit changes, with a commit message preferably following the [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/#summary) convention.

1. Once ready, create a PR on GitHub. The PR will need to have all comments resolved and all tests passing before it will be merged.

   - The PR should have a helpful description of the scope of changes being contributed.
   - Include screenshots or video to reflect UX or UI updates
   - Indicate if new settings need to be applied when the changes are merged - locally or in other environments
