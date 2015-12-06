using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// REST service consumer class
/// </summary>
public class RESTConsumer
{
    string getUsersURLPath = "rest/cms.user?columns=username,firstname,lastname,email,fullname&format=json";

    /// <summary>
    /// Retrieves users from source service
    /// </summary>
    /// <param name="URL">Base Kentico URL, e.g.: http://localhost/Kentico82/ </param>
    /// <param name="username">User name to access REST service with</param>
    /// <param name="password">User's password</param>
    /// <returns>Collection of users</returns>
    public IEnumerable<CMS_User> GetUsersFeed(string URL, string username, string password)
    {
        using (var client = new HttpClient())
        {
            client.BaseAddress = new Uri(URL);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var byteArray = Encoding.ASCII.GetBytes(string.Format("{0}:{1}", username, password));
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            client.Timeout = new TimeSpan(0, 0, 30);

            Task<HttpResponseMessage> t = client.GetAsync(URL + getUsersURLPath);
            HttpResponseMessage response = t.Result;
            if (response.IsSuccessStatusCode)
            {
                Task<Rootobject> tsk = response.Content.ReadAsAsync<Rootobject>();
                Rootobject responseData = tsk.Result;
                return responseData.cms_users.FirstOrDefault().CMS_User;
            }
        }
        return null;
    }

    #region Helper DTO Classes
    
    class Rootobject
    {
        public Cms_Users[] cms_users { get; set; }
    }

    class Cms_Users
    {
        public CMS_User[] CMS_User { get; set; }
        public Totalrecord[] TotalRecords { get; set; }
    }

    class CMS_User
    {
        public string fullname { get; set; }
        public string firstname { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public string lastname { get; set; }
    }

    class Totalrecord
    {
        public string TotalRecords { get; set; }
    }
    #endregion
}