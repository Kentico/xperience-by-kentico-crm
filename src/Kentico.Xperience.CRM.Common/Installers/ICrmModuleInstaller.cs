using CMS.DataEngine;
using CMS.FormEngine;

namespace Kentico.Xperience.CRM.Common.Installers;

public interface ICrmModuleInstaller
{
    bool IsInstalled();
    void Install();
}

public class CrmModuleInstaller : ICrmModuleInstaller
{
    public bool IsInstalled()
    {
        throw new NotImplementedException();
    }

    public void Install()
    {
        var testClass = DataClassInfo.New("test.TestClass");
        testClass.ClassName = "test.TestClass";
        testClass.ClassTableName = "test_TestClass";
        testClass.ClassDisplayName = "Test Class 1";
        testClass.ClassResourceID = 455;
        testClass.ClassType = ClassType.OTHER;

        var formInfo = FormHelper.GetBasicFormDefinition("TestItemID");

        var formItem2 = new FormFieldInfo();
        formItem2.Name = "TestName";
        formItem2.Visible = false;
        formItem2.Precision = 0;
        formItem2.Size = 200;
        formItem2.DataType = "text";
        formItem2.Enabled = true;
        
        formInfo.AddFormItem(formItem2);

        testClass.ClassFormDefinition = formInfo.GetXmlDefinition();
        DataClassInfoProvider.SetDataClassInfo(testClass);
    }
}