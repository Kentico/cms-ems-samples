using System;
using System.Data;
using System.Web.UI.WebControls;
using CMS.EventManager;
using CMS.Helpers;
using CMS.Base;
using CMS.PortalEngine;
using CMS.SiteProvider;
using CMS.Membership;
using CMS.DocumentEngine;
using CMS.UIControls;
using CMS.ExtendedControls;

using TreeNode = CMS.DocumentEngine.TreeNode;
using CMS.Globalization;

public partial class CMSModules_EventManager_Controls_EventAttendees_List : CMSAdminControl
{
    #region "Variables"

    private bool mUsePostback;

    #endregion


    #region "Properties"

    /// <summary>
    /// Messages placeholder
    /// </summary>
    public override MessagesPlaceHolder MessagesPlaceHolder
    {
        get
        {
            return plcMess;
        }
    }


    /// <summary>
    /// Indicates if control is used on live site.
    /// </summary>
    public override bool IsLiveSite
    {
        get
        {
            return base.IsLiveSite;
        }
        set
        {
            plcMess.IsLiveSite = value;
            base.IsLiveSite = value;
        }
    }


    /// <summary>
    /// Attendees' EventID.
    /// </summary>
    public int EventID
    {
        get;
        set;
    }


    /// <summary>
    /// Use post back instead of redirect.
    /// </summary>
    public bool UsePostback
    {
        get
        {
            return mUsePostback;
        }
        set
        {
            mUsePostback = value;
            UniGrid.DelayedReload = value;
        }
    }


    /// <summary>
    /// ID of edited attendee.
    /// </summary>
    public int SelectedAttendeeID
    {
        get;
        set;
    }


    /// <summary>
    /// Stop processing.
    /// </summary>
    public override bool StopProcessing
    {
        get
        {
            return base.StopProcessing;
        }
        set
        {
            base.StopProcessing = value;
            UniGrid.StopProcessing = value;
        }
    }

    #endregion


    #region "Methods"

    protected void Page_Load(object sender, EventArgs e)
    {
        // Script for UniGri's edit action 
        ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "EditAttendee",
                                               ScriptHelper.GetScript("function EditAttendee(attendeeId){" +
                                                                      "location.replace('Events_Attendee_Edit.aspx?attendeeid=' + attendeeId + '&eventid=" + EventID + "'); }"));

        // Refresh parent frame header
        ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "RefreshHeader",
                                               ScriptHelper.GetScript("function RefreshHeader() {if (parent.frames['eventsHeader']) { " +
                                                                      "parent.frames['eventsHeader'].location.replace(parent.frames['eventsHeader'].location); }} \n"));

        /* START Events as products */
        
        InitOrderPropertiesScripts();
        UniGrid.OnExternalDataBound += UniGrid_OnExternalDataBound;

        /* END Events as products */

        //Unigrid settings
        UniGrid.OnAction += UniGrid_OnAction;
        UniGrid.ZeroRowsText = GetString("Events_List.NoAttendees");
        UniGrid.HideControlForZeroRows = false;

        if (UsePostback)
        {
            UniGrid.GridName = "~/CMSModules/EventManager/Tools/Events_Attendee_List_Control.xml";
        }
        else
        {
            UniGrid.GridName = "~/CMSModules/EventManager/Tools/Events_Attendee_List.xml";
        }

        if (EventID > 0)
        {
            UniGrid.WhereCondition = "AttendeeEventNodeId = " + EventID;
        }
    }

    /* START Events as products */

    private void InitOrderPropertiesScripts()
    {
        // Get URL of the order properties page
        string url = UIContextHelper.GetElementUrl("CMS.Ecommerce", "OrderProperties", false);

        // Register dialog script
        ScriptHelper.RegisterDialogScript(this.Page);

        // Register script for opening a new order properties
        ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "OpenOrder",
            ScriptHelper.GetScript("function OpenOrder(orderId) { modalDialog('" + url + "?orderid=' + orderId + '&objectid=' + orderId, 'OrderProperties', 1024, 600); } \n"));
    }

    object UniGrid_OnExternalDataBound(object sender, string sourceName, object parameter)
    {
        if (sourceName.ToLower() == "orderid")
        {
            // Get order ID
            string orderId = ValidationHelper.GetString(parameter, "");
            if (orderId != "")
            {
                // Create a link to order properties
                LinkButton link = new LinkButton();
                link.Text = orderId;
                link.OnClientClick = "OpenOrder(" + orderId + ");";
                return link;
            }
        }

        return parameter;
    }


    /* END Events as products */


    protected override void OnPreRender(EventArgs e)
    {
        ShowEventInfo();
    }


    private void ShowEventInfo()
    {
        string eventCapacity = "0";
        string eventTitle = "";
        string registeredAttendees = null;

        DataSet ds = EventProvider.GetEvent(EventID, SiteContext.CurrentSiteName, "EventCapacity, EventName, AttendeesCount");
        if (!DataHelper.DataSourceIsEmpty(ds))
        {
            eventCapacity = ValidationHelper.GetInteger(ds.Tables[0].Rows[0]["EventCapacity"], 0).ToString();
            eventTitle = ValidationHelper.GetString(ds.Tables[0].Rows[0]["EventName"], "");
            registeredAttendees = ValidationHelper.GetString(ds.Tables[0].Rows[0]["AttendeesCount"], "");
        }

        string message = ValidationHelper.GetInteger(eventCapacity, 0) > 0 ? String.Format(GetString("Events_Edit.RegisteredAttendeesOfCapacity"), HTMLHelper.HTMLEncode(eventTitle), registeredAttendees, eventCapacity) : String.Format(GetString("Events_Edit.RegisteredAttendeesNoLimit"), HTMLHelper.HTMLEncode(eventTitle), registeredAttendees);
        ShowInformation(message);
    }


    /// <summary>
    /// Handles the UniGrid's OnAction event.
    /// </summary>
    /// <param name="actionName">Name of item (button) that throws event</param>
    /// <param name="actionArgument">ID (value of Primary key) of corresponding data row</param>
    protected void UniGrid_OnAction(string actionName, object actionArgument)
    {
        // Check 'Modify' permission (because of delete action in unigrid)
        if (!CheckPermissions("cms.eventmanager", "Modify"))
        {
            return;
        }

        switch (actionName)
        {
            case "delete":
                EventAttendeeInfoProvider.DeleteEventAttendeeInfo(ValidationHelper.GetInteger(actionArgument, 0));
                // Refresh parent frame header
                ltlScript.Text = ScriptHelper.GetScript("RefreshHeader();");
                UniGrid.ReloadData();
                ShowEventInfo();
                break;

            case "sendemail":
                // Resend invitation email
                TreeProvider mTree = new TreeProvider(MembershipContext.AuthenticatedUser);
                TreeNode node = mTree.SelectSingleNode(EventID);

                EventAttendeeInfo eai = EventAttendeeInfoProvider.GetEventAttendeeInfo(ValidationHelper.GetInteger(actionArgument, 0));

                if ((node != null) && (node.NodeClassName.EqualsCSafe("cms.bookingevent", true)) && (eai != null))
                {
                    EventProvider.SendInvitation(SiteContext.CurrentSiteName, node, eai, TimeZoneHelper.ServerTimeZone);

                    ShowConfirmation(GetString("eventmanager.invitationresend"));
                }
                break;

            case "edit":
                SelectedAttendeeID = ValidationHelper.GetInteger(actionArgument, 0);
                break;
        }
    }


    /// <summary>
    /// Reloads data.
    /// </summary>
    public override void ReloadData()
    {
        base.ReloadData();

        UniGrid.WhereCondition = "AttendeeEventNodeId = " + EventID;
        UniGrid.ReloadData();
    }

    #endregion
}