using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.PortalControls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class AzureSearchBox : CMSAbstractWebPart
{
    #region "Constants"

    private static Uri _serviceUri;
    private static HttpClient _httpClient;

    #endregion

    #region "Public properties"

    /// <summary>
    /// Gets or sets azure search service name
    /// </summary>
    public string AzureSearchServiceName
    {
        get
        {
            return ValidationHelper.GetString(GetValue("AzureSearchServiceName"), "");
        }
        set
        {
            SetValue("AzureSearchServiceName", value);
        }
    }

    /// <summary>
    /// Gets or sets azure search service key
    /// </summary>
    public string AzureSearchServiceKey
    {
        get
        {
            return ValidationHelper.GetString(GetValue("AzureSearchServiceKey"), "");
        }
        set
        {
            SetValue("AzureSearchServiceKey", value);
        }
    }

    /// <summary>
    /// Gets or sets azure search service key
    /// </summary>
    public string AzureSearchServiceIndexName
    {
        get
        {
            return ValidationHelper.GetString(GetValue("AzureSearchServiceIndexName"), "");
        }
        set
        {
            SetValue("AzureSearchServiceIndexName", value);
        }
    }

    #endregion

    #region Methods

    protected void Page_Load(object sender, EventArgs e)
    {
        //Build up the Azure Service path
        _serviceUri = new Uri("https://" + this.AzureSearchServiceName + ".search.windows.net");
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("api-key", this.AzureSearchServiceKey);

    }
    protected void bt_Click(object sender, EventArgs e)
    {
        try
        {
            StringBuilder sb = new StringBuilder();
            Button btn = sender as Button;
            switch (btn.ID)
            {
                case "btnCreateIndex": //Create the index in Azure Saerch
                    Uri uri = new Uri(_serviceUri, "/indexes/" + this.AzureSearchServiceIndexName);
                    HttpResponseMessage response = AzureSearchHelper.SendSearchRequest(_httpClient, HttpMethod.Get, uri);
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        CreateCatalogIndex();
                        sb.Append("Index created!");
                    }
                    else
                    {
                        sb.Append("Index exists!");
                    }
                    break;
                case "btnLoadIndex": //Populate the Azure Search Index
                    LoadIndex();
                    sb.Append("Index data loaded!");
                    break;
                case "btnSearch": //Search against the Azure Search
                    sb.Append(SearchIndex(ValidationHelper.GetString(txtSearch.Text, "")));
                    break;
                case "btnReset": //Do a whole lot of nothing, but make things look nice.
                    txtSearch.Text = "";
                    sb.Clear();
                    break;
            }
            lblResults.Text = sb.ToString();
        }
        catch (Exception ex)
        {
            lblResults.Text = ex.Message;
        }
    }

    /// <summary>
    /// This function will create the specified index.
    /// </summary>
    /// <returns>string - Results response</returns>
    private string CreateCatalogIndex()
    {
        try
        {
            var definition = new
            {
                Name = this.AzureSearchServiceIndexName,
                Fields = new[] 
                { 
                    new { Name = "DocumentID",Type = "Edm.String",Key = true,  Searchable = false, Filterable = false, Sortable = false, Facetable = false, Retrievable = true,  Suggestions = false },
                    new { Name = "DocumentName",Type = "Edm.String",Key = false, Searchable = true,  Filterable = false, Sortable = true,  Facetable = false, Retrievable = true,  Suggestions = true  }
                }
            };

            Uri uri = new Uri(_serviceUri, "/indexes");
            string json = AzureSearchHelper.SerializeJson(definition);
            HttpResponseMessage response = AzureSearchHelper.SendSearchRequest(_httpClient, HttpMethod.Post, uri, json);
            response.EnsureSuccessStatusCode();
            return "Index created.";
        }
        catch(Exception ex)
        {
            return "Index not created.<br />" + ex.Message;
        }
    }

    /// <summary>
    /// This function will load the index with documents for the current site
    /// </summary>
    /// <returns>string - Results response.</returns>
    private string LoadIndex()
    {
        try
        {
            // Get documents
            var documents = DocumentHelper.GetDocuments()
                .Types("CMS.MenuItem", "CMS.Folder")
                .OnSite(CurrentSiteName);

            StringBuilder sb = new StringBuilder();

            sb.Append("{");
            sb.Append("\"value\": [");
            int i = 1;
            foreach (var document in documents)
            {
                sb.Append("{");
                sb.Append("\"@search.action\":\"mergeOrUpload\",");
                sb.Append("\"DocumentID\":\"" + document.DocumentID + "\",");
                sb.Append("\"DocumentName\":\"" + document.DocumentName + "\"");
                sb.Append("}");
                if (i < documents.Count)
                {
                    sb.Append(",");
                }
                i += 1;
            }
            sb.Append("]");
            sb.Append("}");

            Uri uri = new Uri(_serviceUri, "/indexes/" + this.AzureSearchServiceIndexName + "/docs/index");
            string json = sb.ToString();
            HttpResponseMessage response = AzureSearchHelper.SendSearchRequest(_httpClient, HttpMethod.Post, uri, json);
            response.EnsureSuccessStatusCode();

            return "Index data loaded";
        }
        catch (Exception ex)
        {
            return "Index data not created.<br />" + ex.Message;
        }
    }

    /// <summary>
    /// This function will return a formatted string of the documents mathcing the specified search value
    /// </summary>
    /// <param name="strValue">string - Search value</param>
    /// <returns>string - Some totally awesome search results</returns>
    private string SearchIndex(string strValue)
    {
        StringBuilder sb = new StringBuilder();
        try
        {
            //Build up the search parameter
            string search = "&search=" + Uri.EscapeDataString(strValue);

            //Get the Azure Search records for the specified value
            if (strValue.Length > 2)
            {
                Uri uri = new Uri(_serviceUri, "/indexes/" + this.AzureSearchServiceIndexName + "/docs/suggest?" + search);
                HttpResponseMessage response = AzureSearchHelper.SendSearchRequest(_httpClient, HttpMethod.Get, uri);
                AzureSearchHelper.EnsureSuccessfulSearchResponse(response);

                dynamic results = AzureSearchHelper.DeserializeJson<dynamic>(response.Content.ReadAsStringAsync().Result);

                //Create a list of the results so we can loop over them and find the assoicated document
                IEnumerable<AzureResultItem> items = ((JArray)results["value"]).Select(x => new AzureResultItem
                {
                    documentid = (string)x["DocumentID"],
                    documentname = (string)x["@search.text"]
                }).ToList();

                foreach (AzureResultItem item in items)
                {
                    sb.Append(item.documentname + "<br />");
                    var doc = DocumentHelper.GetDocument(ValidationHelper.GetInteger(item.documentid, 0), null);
                    sb.Append("<a href=\"~" + doc.NodeAliasPath + "\">" + doc.NodeAliasPath + "</a><br /><br />");
                }
            }
            else
            {
                sb.Append("You must enter atleast 3 characters.");
            }
        }
        catch(Exception ex)
        {
            sb.Append(ex.Message);
        }
        return sb.ToString();
            
    }

    #endregion

    public class AzureResultItem
    {
        public string documentid;
        public string documentname;
    }
}