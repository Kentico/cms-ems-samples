using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using CMS.SynchronizationEngine;
using CMS.Synchronization;
using CMS.DataEngine;
using CMS;
using CMS.Membership;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using CMS.Helpers;
using CMS.EventLog;
using CMS.Base;

[assembly: RegisterCustomClass("CustomIntegrationConnector", typeof(CustomIntegrationConnector))]

/// <summary>
/// Summary description for CustomIntegrationConnector
/// </summary>
public class CustomIntegrationConnector : BaseIntegrationConnector
{
    /// <summary>
    /// Initializes the connector name.
    /// </summary>
    public override void Init()
    {
        // Initializes the connector name (must match the code name of the connector object in the system)
        // GetType().Name uses the name of the class as the ConnectorName
        ConnectorName = GetType().Name;

        // Creates subscription for all user objects (predefined method)
        SubscribeToObjects(TaskProcessTypeEnum.AsyncSnapshot, UserInfo.OBJECT_TYPE);

    }

    public override IntegrationProcessResultEnum ProcessInternalTaskAsync(GeneralizedInfo infoObj, TranslationHelper translations, TaskTypeEnum taskType, TaskDataTypeEnum dataType, string siteName, out string errorMessage)
    {
        try
        {
            //Determine if the record exists in Azure Storage
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(SettingsKeyInfoProvider.GetValue("Custom.AzureStorageConnectionString"));

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "kenticousers" table.
            CloudTable table = tableClient.GetTableReference("kenticousers");

            // Create a retrieve operation that takes a customer entity.
            TableOperation retrieveOperation = TableOperation.Retrieve<KenticoUserEntity>(ValidationHelper.GetString(infoObj["UserGUID"], ""), ValidationHelper.GetString(infoObj["LastName"], ""));

            // Execute the operation.
            TableResult retrievedResult = table.Execute(retrieveOperation);

            // Assign the result to a CustomerEntity object.
            KenticoUserEntity existinguser = (KenticoUserEntity)retrievedResult.Result;

            //Check if the record already exists
            if (existinguser == null)
            {
                // create a new record
                KenticoUserEntity newuser = new KenticoUserEntity(ValidationHelper.GetString(infoObj["UserGUID"], ""), ValidationHelper.GetString(infoObj["LastName"], ""));
                newuser.firstname = ValidationHelper.GetString(infoObj["FirstName"], "");
                newuser.lastname = ValidationHelper.GetString(infoObj["LastName"], "");
                newuser.email = ValidationHelper.GetString(infoObj["Email"], "");

                // Create the Insert TableOperation
                TableOperation insertOperation = TableOperation.Insert(newuser);

                // Execute the operation.
                table.Execute(insertOperation);

                EventLogProvider.LogEvent("I", "CustomIntegrationConnector", "Information", "Record inserted!");
            }
            else
            {
                //update the record
                existinguser.firstname = ValidationHelper.GetString(infoObj["FirstName"], "");
                existinguser.lastname = ValidationHelper.GetString(infoObj["LastName"], "");
                existinguser.email = ValidationHelper.GetString(infoObj["Email"], "");

                // Create the Update TableOperation
                TableOperation updateOperation = TableOperation.Replace(existinguser);

                // Execute the operation.
                table.Execute(updateOperation);

                EventLogProvider.LogEvent("I", "CustomIntegrationConnector", "Information", "Record updated!");
            }

            //Set the error message to null and the response to OK
            errorMessage = null;
            return IntegrationProcessResultEnum.OK;
        }
        catch (Exception ex)
        {
            //There was a problem.
            errorMessage = ex.Message;
            return IntegrationProcessResultEnum.ErrorAndSkip;
        }
    }
}

public class KenticoUserEntity : TableEntity
{
    public KenticoUserEntity(string userguid, string lastname)
    {
        this.PartitionKey = userguid;
        this.RowKey = lastname;
    }

    public KenticoUserEntity() { }

    public string firstname { get; set; }
    public string lastname { get; set; }
    public string email { get; set; }

}
