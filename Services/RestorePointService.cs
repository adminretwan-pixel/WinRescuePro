using System;
using System.Management;
using System.Threading.Tasks;

namespace WinRescuePro.Services
{
    public class RestorePointService
    {
        public async Task<bool> CreateRestorePointAsync(string description)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var scope = new ManagementScope(@"\\localhost\root\default");
                    {
                        scope.Connect();
                        
                        using (var managementClass = new ManagementClass(scope, 
                            new ManagementPath("SystemRestore"), null))
                        {
                            var parameters = managementClass.GetMethodParameters("CreateRestorePoint");
                            parameters["Description"] = description;
                            parameters["RestorePointType"] = 12; // MODIFY_SETTINGS
                            parameters["EventType"] = 100;
                            
                            var result = managementClass.InvokeMethod("CreateRestorePoint", parameters, null);
                            var returnValue = Convert.ToInt32(result["ReturnValue"]);
                            
                            return returnValue == 0;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Restore point error: {ex.Message}");
                    return false;
                }
            });
        }
    }
}
