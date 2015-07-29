using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using CMS;
using CMS.Base;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Scheduler;
using CMS.WebFarmSync;

[assembly: RegisterCustomClass("WebFarmServerCleaner", typeof(WebFarmServerCleaner))]

/// <summary>
/// Scheduled task registration loader. 
/// </summary>
[WebFarmServerCleanerModuleLoader]
public partial class CMSModuleLoader
{
    /// <summary>
    /// Loader registration
    /// </summary>
    private class WebFarmServerCleanerModuleLoader : CMSLoaderAttribute
    {
        /// <summary>
        /// Registers the web farm server cleaner scheduled task
        /// </summary>
        public override void Init()
        {
            // Don't create task if it already exists
            if (TaskInfoProvider.GetTasks().WhereEquals("TaskName", "WebFarmServerCleaner").HasResults())
            {
                return;
            }

            // Set the interval for every hour
            var interval = new TaskInterval
            {
                Every = 1,
                Period = SchedulingHelper.PERIOD_HOUR,
                StartTime = DateTime.Now,
                BetweenEnd = DateTime.Today.AddMinutes(-1),
                Days = new ArrayList(Enum.GetNames(typeof(DayOfWeek)))
            };

            // Register the scheduled task
            var task = new TaskInfo
            {
                TaskAssemblyName = Assembly.GetExecutingAssembly().GetName().Name,
                TaskClass = "WebFarmServerCleaner",
                TaskDisplayName = "Web farm server cleaner",
                TaskInterval = SchedulingHelper.EncodeInterval(interval),
                TaskName = "WebFarmServerCleaner",
                TaskNextRunTime = DateTime.Now,
                TaskEnabled = true,
                TaskData = String.Empty
            };
            TaskInfoProvider.SetTaskInfo(task);
        }
    }
}


/// <summary>
/// Implementation of web farm server cleaner scheduled task
/// </summary>
public class WebFarmServerCleaner : ITask
{
    private int mRemoveAfterHours = -1;

    /// <summary>
    /// Time after which the web farm server will be deleted.
    /// </summary>
    public int RemoveAfterHours
    {
        get
        {
            if (mRemoveAfterHours <= 0)
            {
                mRemoveAfterHours = ValidationHelper.GetInteger(SettingsHelper.AppSettings["CMSWebFarmServerCleanerRemoveAfterHours"], 24);
            }
            return mRemoveAfterHours;
        }
    }


    /// <summary>
    /// Cleans old web farm servers.
    /// </summary>
    /// <param name="task">Task info</param>
    public string Execute(TaskInfo task)
    {
        if (!WebSyncHelper.DeleteGeneratedWebFarmServers)
        {
            return String.Empty;
        }

        try
        {
            // Delete old servers with all their tasks
            WebFarmServerTaskInfoProvider.GetWebFarmServerTasks()
                .WhereNull("ErrorMessage")
                .WhereIn("TaskID", WebFarmTaskInfoProvider.GetWebFarmTasks()
                    .Where("TaskCreated", QueryOperator.LessThan, DateTime.Now.AddHours(-RemoveAfterHours))
                    .Column("TaskID"))
                .Column("ServerID")
                .GetListResult<int>()
                .ToList()
                .ForEach(WebFarmServerInfoProvider.DeleteWebFarmServerInfo);
        }
        catch (Exception e)
        {
            EventLogProvider.LogException("WebFarmServerCleaner", "EXCEPTION", e);
            return e.Message;
        }

        return String.Empty;
    }
}