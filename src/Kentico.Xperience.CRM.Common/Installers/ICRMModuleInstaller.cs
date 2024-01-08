using CMS.Base;
using Kentico.Xperience.CRM.Common.Constants;

namespace Kentico.Xperience.CRM.Common.Installers;

public interface ICRMModuleInstaller
{
    void Install(string crmType);
}