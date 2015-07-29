using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Linq;

namespace WebApplication1
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        public string requestBaseUrl = "http://[your url]/rest"; // The URL of the REST request (base URL + resource path)
        public string requestData = ""; // Data for POST or PUT requests

        public string responseDescription; // Stores the description of the response status
        public string responseUserData = ""; // Stores data retrieved by the request
        public string responseRoleData = ""; // Stores data retrieved by the request

        public string strAuithentication = "[username]:[password]";

        public HttpWebRequest request;

        protected void Page_Load(object sender, EventArgs e)
        {
            SetMessage("");
            if (!Page.IsPostBack)
            {
                GetRoles();
            }
        }

        protected void GetRoles()
        {
            try
            {
                requestBaseUrl = requestBaseUrl + "/cms.role/site/DancingGoat?format=xml"; // The URL of the REST request (base URL + resource path)


                // Creates the REST request
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestBaseUrl);

                // Sets the HTTP method of the request
                request.Method = "GET";

                // Authorizes the request using Basic authentication
                request.Headers.Add("Authorization: Basic " + Base64Encode(strAuithentication));

                // Gets the REST response
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                // Stores the description of the response status
                responseDescription = (Int32)response.StatusCode + " - " + response.StatusDescription;

                // Gets the response data
                using (Stream responseStream = response.GetResponseStream())
                {
                    if (responseStream != null)
                        using (StreamReader reader = new StreamReader(responseStream))
                        {
                            //responseData = HttpUtility.HtmlEncode(reader.ReadToEnd());
                            responseUserData = reader.ReadToEnd();
                        }
                }

                XDocument document = XDocument.Parse(responseUserData);

                foreach (XElement role in document.Descendants("CMS_Role"))
                {
                    cblRoles.Items.Add(new ListItem(role.Element("RoleDisplayName").Value, role.Element("RoleID").Value));
                }
            }
            catch (Exception ex)
            {
                SetMessage(ex.Message);
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                //Create the User
                string requestFullUrl = requestBaseUrl + "/cms.user/currentsite"; // The URL of the REST request (base URL + resource path)
                requestData = String.Format(
                    @"
                    <CMS_User><UserName>Sample{0}</UserName>
                    <FullName>Content editor</FullName>
                    <Email>{0}@localhost.local</Email>
                    <UserEnabled>true</UserEnabled>
                    <UserIsEditor>true</UserIsEditor>
                    <UserPassword>{1}</UserPassword>
                    </CMS_User>
                    ",
                    DateTime.Now.Millisecond,
                    pwPassword.Text
                );

                // Creates the REST request
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestFullUrl);

                // Sets the HTTP method of the request
                request.Method = "POST";

                // Authorizes the request using Basic authentication
                request.Headers.Add("Authorization: Basic " + Base64Encode(strAuithentication));

                // Submits data for POST or PUT requests
                if ((request.Method == "POST") || (request.Method == "PUT"))
                {
                    request.ContentType = "text/xml";

                    Byte[] bytes = Encoding.GetEncoding("utf-8").GetBytes(requestData);
                    request.ContentLength = bytes.Length;

                    using (Stream writeStream = request.GetRequestStream())
                    {
                        writeStream.Write(bytes, 0, bytes.Length);
                    }
                }

                // Gets the REST response
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                // Stores the description of the response status
                responseDescription = (Int32)response.StatusCode + " - " + response.StatusDescription;

                // Gets the response data
                using (Stream responseStream = response.GetResponseStream())
                {
                    if (responseStream != null)
                        using (StreamReader reader = new StreamReader(responseStream))
                        {
                            sb.Append(reader.ReadToEnd());
                        }
                }

                //Get the new user id from the response
                string strUserID = "";
                XDocument document = XDocument.Parse(Convert.ToString(sb));

                foreach (XElement role in document.Descendants("CMS_User"))
                {
                    strUserID = role.Element("UserID").Value;
                }


                //Add the user to roles
                //Create the User
                requestFullUrl = requestBaseUrl + "/cms.userrole/currentsite"; // The URL of the REST request (base URL + resource path)
                foreach (ListItem li in cblRoles.Items)
                {
                    if (li.Selected)
                    {
                        requestData = String.Format(
                            @"
                            <CMS_UserRole>
                            <UserID>{0}</UserID>
                            <RoleID>{1}</RoleID>
                            </CMS_UserRole>
                            ",
                            strUserID,
                            li.Value
                        );

                        // Creates the REST request
                        request = (HttpWebRequest)WebRequest.Create(requestFullUrl);

                        // Sets the HTTP method of the request
                        request.Method = "POST";

                        // Authorizes the request using Basic authentication
                        request.Headers.Add("Authorization: Basic " + Base64Encode(strAuithentication));

                        // Submits data for POST or PUT requests
                        if ((request.Method == "POST") || (request.Method == "PUT"))
                        {
                            request.ContentType = "text/xml";

                            Byte[] bytes = Encoding.GetEncoding("utf-8").GetBytes(requestData);
                            request.ContentLength = bytes.Length;

                            using (Stream writeStream = request.GetRequestStream())
                            {
                                writeStream.Write(bytes, 0, bytes.Length);
                            }
                        }

                        // Gets the REST response
                        HttpWebResponse response2 = (HttpWebResponse)request.GetResponse();

                        // Stores the description of the response status
                        responseDescription = (Int32)response2.StatusCode + " - " + response2.StatusDescription;

                        // Gets the response data
                        using (Stream responseStream = response2.GetResponseStream())
                        {
                            if (responseStream != null)
                            {
                                using (StreamReader reader = new StreamReader(responseStream))
                                {
                                    sb.Append(reader.ReadToEnd());
                                }
                            }
                        }
                    }
                }


                SetMessage(Convert.ToString(sb));
            }
            catch (Exception ex)
            {
                SetMessage(ex.Message);
            }
        }

        private void SetMessage(string strMessage)
        {
            if (strMessage != "")
            {
                txtResults.Text = strMessage;
                //txtResults.Visible = true;
            }
            else
            {
                //txtResults.Visible = false;
            }
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }
}