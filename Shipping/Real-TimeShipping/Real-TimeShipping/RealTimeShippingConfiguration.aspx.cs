using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CMS.UIControls;
using CMS.Helpers;
using CMS.ExtendedControls.ActionsConfig;

public partial class RealTimeShippingConfiguration : CMSPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        CurrentMaster.HeaderActions.AddAction(new SaveAction(this));
        CurrentMaster.HeaderActions.ActionPerformed += HeaderActions_ActionPerformed;
    }

    void HeaderActions_ActionPerformed(object sender, CommandEventArgs e)
    {
        if(e.CommandName == "Save")
        {
            SettingsGroupViewer.SaveChanges();
        }
    }
}
