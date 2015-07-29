using System;

public partial class ModifyAttendeesTable : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        // Add two new tables to the CMS_Attendee table 
        Custom.CustomEventManager.CustomEventHelper.AddAttendeeTableColumns();
    }
}
