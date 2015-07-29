using System;
using System.Data;
using System.Collections;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.PortalControls;
using CMS.Helpers;
using CMS.PortalEngine;

public partial class CMSWebParts_Viewers_Documents_DynamicViewer : CMSAbstractWebPart
{
    #region "Properties"

    /// <summary>
    /// Gets or sets path of source.
    /// </summary>
    public string DynamicViewerSourcePath
    {
        get
        {
            return URLHelper.GetVirtualPath(ValidationHelper.GetString(GetValue("DynamicViewerSourcePath"), ""));
        }
        set
        {
            SetValue("DynamicViewerSourcePath", value);
        }
    }

    public int LoadSize
    {
        get
        {
            return ValidationHelper.GetInteger(GetValue("LoadSize"), 1);
        }
        set
        {
            SetValue("LoadSize", value);
        }
    }

    public int LoadIndex
    {
        get
        {
            return ValidationHelper.GetInteger(GetValue("LoadIndex"), 0);
        }
        set
        {
            SetValue("LoadIndex", value);
        }
    }

    public string LoadMoreType
    {
        get
        {
            return ValidationHelper.GetString(GetValue("LoadMoreType"), "");
        }
        set
        {
            SetValue("LoadMoreType", value);
        }
    }

    public string LoadMoreText
    {
        get
        {
            return DataHelper.GetNotEmpty(ValidationHelper.GetString(GetValue("LoadMoreText"), "Load more"), "Load more");
        }
        set
        {
            SetValue("LoadMoreText", value);
        }
    }

    public string NoMoreRecordsText
    {
        get
        {
            return DataHelper.GetNotEmpty(ValidationHelper.GetString(GetValue("NoMoreRecordsText"), "There are no more records to be loaded"), "There are no more records to be loaded");
        }
        set
        {
            SetValue("NoMoreRecordsText", value);
        }
    }

    public string LoadEffect
    {
        get
        {
            return DataHelper.GetNotEmpty(ValidationHelper.GetString(GetValue("LoadEffect"), "none"), "none");
        }
        set
        {
            SetValue("LoadEffect", value);
        }
    }

    #endregion


    #region "Methods"

    /// <summary>
    /// Content loaded event handler.
    /// </summary>
    public override void OnContentLoaded()
    {
        base.OnContentLoaded();
        SetupControl();
    }


    /// <summary>
    /// Initializes the control properties.
    /// </summary>
    protected void SetupControl()
    {
        if (this.StopProcessing)
        {
            // Do not process
        }
        else
        {
            string webPartID = ValidationHelper.GetString(this.GetValue("WebPartControlID"), Guid.NewGuid().ToString());
            string contentMarkup = "<div class=\"" + webPartID + "\"><div id=\"" + webPartID + "\" class=\"DVContent\"></div>";
            if (LoadMoreType == "button")
            {
                contentMarkup += "<button id=\"Load" + webPartID + "\" class=\"DVLoadMore\">" + LoadMoreText + "</button>";
            }
            else
            {
                contentMarkup += "<a style=\"cursor:pointer;\" id=\"Load" + webPartID + "\" class=\"DVLoadMore\">" + LoadMoreText + "</a>";
            }
            contentMarkup += "</div>";
            DynamicViewerContent.Text = contentMarkup;

            ScriptHelper.RegisterScriptFile(this.Page, "http://ajax.googleapis.com/ajax/libs/jquery/1.11.1/jquery.min.js");

            string contentScript = "$" + webPartID + " = jQuery.noConflict();$" + webPartID + "(document).ready(function () {var it = " + LoadIndex + "; $" +
                webPartID + "(\"#Load" + webPartID + "\").click(function (e) { e.preventDefault(); $" + webPartID + ".ajax({ url: \"" + DynamicViewerSourcePath +
                ".aspx?i=\" + it + \"&s=" + LoadSize + "\", type: \"POST\", success: function (result) { it++; if (result!='') { $" + webPartID + "(\"#" + webPartID + "\")";
            switch (LoadEffect) {
                case "fade":
                    contentScript += ".append('<div id=\"" + webPartID + "_DVEffect'+it+'\" style=\"display:none;\">'+result+'</div>'); $" + webPartID + "('#" + webPartID + "_DVEffect'+it).fadeIn(\"slow\")";
                    break;
                case "slide":
                    contentScript += ".append('<div id=\"" + webPartID + "_DVEffect'+it+'\" style=\"display:none;\">'+result+'</div>'); $" + webPartID + "('#" + webPartID + "_DVEffect'+it).slideDown(\"slow\")";
                    break;
                default:
                    contentScript += ".append(result);";
                    break;
            }
            contentScript += " } else { $" + webPartID + "(\"#Load" + webPartID + "\").hide();$" +
                 webPartID + "(\"." + webPartID + "\").append('<div class=\"DVNoMoreRecords\">" + NoMoreRecordsText + "</div>'); } } }); }); });";
            
            ScriptHelper.RegisterStartupScript(Page, typeof(Page), webPartID, ScriptHelper.GetScript(contentScript));
        }
    }


    /// <summary>
    /// Reloads the control data.
    /// </summary>
    public override void ReloadData()
    {
        base.ReloadData();

        SetupControl();
    }

    #endregion
}



