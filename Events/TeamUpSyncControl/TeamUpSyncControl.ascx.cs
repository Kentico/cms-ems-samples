using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CMS.FormControls;
using CMS.Helpers;
using CMS.EventLog;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using System.Threading.Tasks;

public partial class TeamUpSyncControl : FormEngineUserControl
{
    #region Properties

    /// <summary>
    /// Base TeamUp calendar path  
    /// </summary>
    public string BaseCalendarPath
    {
        get
        {
            return ValidationHelper.GetString(GetValue("BaseCalendarPath"), "");
        }
        set
        {
            SetValue("BaseCalendarPath", value);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public string TeamUpKey
    {
        get
        {
            return ValidationHelper.GetString(GetValue("TeamUpKey"), "");
        }
        set
        {
            SetValue("TeamUpKey", value);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public string CalendarID
    {
        get
        {
            return ValidationHelper.GetString(GetValue("CalendarID"), "");
        }
        set
        {
            SetValue("CalendarID", value);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public string StartDateColumnName
    {
        get
        {
            return ValidationHelper.GetString(GetValue("StartDateColumnName"), "");
        }
        set
        {
            SetValue("StartDateColumnName", value);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public string EndDateColumnName
    {
        get
        {
            return ValidationHelper.GetString(GetValue("EndDateColumnName"), "");
        }
        set
        {
            SetValue("EndDateColumnName", value);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public string TitleColumnName
    {
        get
        {
            return ValidationHelper.GetString(GetValue("TitleColumnName"), "");
        }
        set
        {
            SetValue("TitleColumnName", value);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public string WhoColumnName
    {
        get
        {
            return ValidationHelper.GetString(GetValue("WhoColumnName"), "");
        }
        set
        {
            SetValue("WhoColumnName", value);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public string LocColumnName
    {
        get
        {
            return ValidationHelper.GetString(GetValue("LocColumnName"), "");
        }
        set
        {
            SetValue("LocColumnName", value);
        }
    }


    #endregion

    protected void Page_Load(object sender, EventArgs e)
    {
        btnCreateEvent.Visible = false;
        btnDeleteEvent.Visible = false;

        GetEvent();
    }

    /// <summary>
    /// Gets or sets the value entered into the field
    /// </summary>
    public override object Value
    {
        get
        {
            return hidEventID.Value;
        }
        set
        {
            hidEventID.Value = System.Convert.ToString(value);
        }
    }
    /// <summary>
    /// Returns an array of values of any other fields returned by the control.
    /// </summary>
    /// <returns>It returns an array where the first dimension is the attribute name and the second is its value.</returns>
    public override object[,] GetOtherValues()
    {
        object[,] array = new object[1, 2];
        return array;
    }

    /// <summary>
    /// Returns true because it's always valid
    /// </summary>
    public override bool IsValid()
    {
        return true;
    }

    protected void btn_Click(object sender, EventArgs e)
    {
        try
        {
            Button btn = sender as Button;
            switch (btn.ID)
            {
                case "btnGetEvent":
                    GetEvent();
                    break;
                case "btnCreateEvent":
                    CreateEvent();
                    break;
                case "btnDeleteEvent":
                    DeleteEvent();
                    break;
            }
        }
        catch (Exception ex)
        {
            EventLogProvider.LogException("TeamUpSyncControl", "EXCEPTION", ex);
            lblMessage.Text = ex.Message;
        }
    }

    /// <summary>
    /// This function will get the event from TeamUp
    /// </summary>
    protected void GetEvent()
    {
        bool blnEventExists = false;
        try
        {
            lblMessage.Text = "";
            lblEvent.Text = "";

            HttpClientHandler handler = new HttpClientHandler()
            {
                UseDefaultCredentials = true
            };

            using (var client = new HttpClient(handler))
            {
                client.BaseAddress = new Uri(this.BaseCalendarPath);

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                DateTime dtStart = ValidationHelper.GetDate(Form.GetFieldValue(this.StartDateColumnName), DateTime.Now);
                string strStart = dtStart.Year + "-" + dtStart.Month + "-" + dtStart.Day;
                DateTime dtEnd = ValidationHelper.GetDate(Form.GetFieldValue(this.EndDateColumnName), DateTime.Now);
                string strEnd = dtEnd.Year + "-" + dtEnd.Month + "-" + dtEnd.Day;

                HttpResponseMessage response = client.GetAsync("/" + this.TeamUpKey + "/events?startDate=" + strStart + "&endDate=" + strEnd).Result;

                if (response.IsSuccessStatusCode)
                {
                    Task<string> tskResponse = response.Content.ReadAsStringAsync();
                    string strResponse = tskResponse.Result;
                    dynamic jresp = JsonConvert.DeserializeObject(strResponse);
                    foreach (var item in jresp["data"])
                    {
                        //determine if the id matches the current item
                        if (item.id == hidEventID.Value)
                        {
                            lblEvent.Text = "id:" + item.id + "<br />start: " + item.start + "<br />end: " + item.end + "<br />title: " + item.title + "<br />who: " + item.who + "<br />loc: " + item.loc;
                            hidEventVersion.Value = item.ver;
                            blnEventExists = true;
                            break;
                        }
                    }
                }
            }
        }
        catch(Exception ex)
        {
            EventLogProvider.LogException("TeamUpSyncControl", "EXCEPTION", ex);
            lblMessage.Text = ex.Message;
        }

        if (blnEventExists)
        {
            btnCreateEvent.Visible = false;
            btnDeleteEvent.Visible = true;
        }
        else
        {
            hidEventID.Value = "";
            hidEventVersion.Value = "";
            lblEvent.Text = "Not synced to TeamUp";
            btnCreateEvent.Visible = true;
            btnDeleteEvent.Visible = false;
        }
    }

    /// <summary>
    /// This method will create the event in TeamUp
    /// </summary>
    protected void CreateEvent()
    {
        try
        {
            DateTime dtStart = ValidationHelper.GetDate(Form.GetFieldValue(this.StartDateColumnName), DateTime.Now);
            string strStart = dtStart.Year + "-" + dtStart.Month + "-" + dtStart.Day;
            DateTime dtEnd = ValidationHelper.GetDate(Form.GetFieldValue(this.EndDateColumnName), DateTime.Now);
            string strEnd = dtEnd.Year + "-" + dtEnd.Month + "-" + dtEnd.Day;

            //Check if the event is all day
            bool blnAllDay = false;
            if (dtStart == dtEnd)
            {
                blnAllDay = true;
            }

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(this.BaseCalendarPath + "/events");
            httpWebRequest.ContentType = "application/json; charset=utf-8";
            httpWebRequest.Method = "POST";
            httpWebRequest.Accept = "application/json; charset=utf-8";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string evetnjson = new JavaScriptSerializer().Serialize(new
                {
                    cid = this.CalendarID,
                    key = this.TeamUpKey,
                    title = ValidationHelper.GetString(Form.GetFieldValue(this.TitleColumnName), "Sample Calendar Entry"),
                    start = strStart,
                    end = strEnd,
                    who = ValidationHelper.GetString(Form.GetFieldValue(this.WhoColumnName), ""),
                    loc = ValidationHelper.GetString(Form.GetFieldValue(this.LocColumnName), ""),
                    ad = blnAllDay.ToString().ToLower()
                });

                streamWriter.Write(evetnjson);
                streamWriter.Flush();
                streamWriter.Close();

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var strResponse = streamReader.ReadToEnd();
                    dynamic jresp = JsonConvert.DeserializeObject(strResponse);
                    hidEventID.Value = jresp["data"]["id"].Value;
                    btnCreateEvent.Visible = false;
                }

                GetEvent();
            }
        }
        catch (Exception ex)
        {
            EventLogProvider.LogException("TeamUpSyncControl", "EXCEPTION", ex);
            lblMessage.Text = ex.Message;
        }
    }

    /// <summary>
    /// This method will delete the event from TeamUp.
    /// </summary>
    protected void DeleteEvent()
    {
        try
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(this.BaseCalendarPath + "/events/" + hidEventID.Value);
            httpWebRequest.ContentType = "application/json; charset=utf-8";
            httpWebRequest.Method = "DELETE";
            httpWebRequest.Accept = "application/json; charset=utf-8";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string eventjson = new JavaScriptSerializer().Serialize(new
                {
                    id = hidEventID.Value,
                    ver = hidEventVersion.Value
                });

                streamWriter.Write(eventjson);
                streamWriter.Flush();
                streamWriter.Close();

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var strResponse = streamReader.ReadToEnd();
                    dynamic jresp = JsonConvert.DeserializeObject(strResponse);
                    hidEventID.Value = "";
                    GetEvent();
                    lblMessage.Text = "Event Deleted!";
                }
            }
        }
        catch (Exception ex)
        {
            EventLogProvider.LogException("TeamUpSYncControl", "EXCEPTION", ex);
            lblMessage.Text = ex.Message;
        }
    }
}
