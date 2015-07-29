using CMS.Controls;
using CMS.DocumentEngine;
using CMS.ExtendedControls;
using CMS.Helpers;
using CMS.IO;
using CMS.Membership;
using CMS.Search;
using CMS.SiteProvider;
using CMS.WebAnalytics;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.Script.Services;

/// <summary>
/// Summary description for MyPredictiveSearch
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
// [System.Web.Script.Services.ScriptService]
public class MyPredictiveSearch : System.Web.Services.WebService
{

    public MyPredictiveSearch()
    {

        //Uncomment the following line if using designed components 
        //InitializeComponent(); 
    }

    [WebMethod]
    [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Json)]
    public string MyPredictiveSearchExecute(string searchText,
        string PredictiveSearchNoResultsContent,
        bool PredictiveSearchDisplayCategories,
        int PredictiveSearchMaxResults,
        string PredictiveSearchResultItemTransformationName,
        string PredictiveSearchMoreResultsContent,
        string searchURL,
        bool PredictiveSearchLogSearchActivity,
        bool PredictiveSearchTrackSearchKeywords,
        string PredictiveSearchDocumentTypes,
        string PredictiveSearchCultureCode,
        string PredictiveSearchCondition,
        string PredictiveSearchOptions,
        string PredictiveSearchMode,
        bool PredictiveSearchCombineWithDefaultCulture,
        string PredictiveSearchSort,
        string PredictiveSearchPath,
        bool PredictiveSearchCheckPermissions,
        string PredictiveSearchIndexes,
        bool PredictiveSearchBlockFieldOnlySearch,
        int DocumentID,
        int UserID)
    {

        DataSet results = PredictiveSearch(searchText, PredictiveSearchDocumentTypes,
        PredictiveSearchCultureCode,
        PredictiveSearchCondition,
        GetSearchOptionsEnum(PredictiveSearchOptions),
        GetSearchModeEnum(PredictiveSearchMode),
        PredictiveSearchCombineWithDefaultCulture,
        PredictiveSearchSort,
        PredictiveSearchPath,
        PredictiveSearchCheckPermissions,
        PredictiveSearchIndexes,
        PredictiveSearchMaxResults,
        PredictiveSearchBlockFieldOnlySearch);

        string resultHTML = RenderResults(results,
            searchText,
            PredictiveSearchNoResultsContent,
            PredictiveSearchDisplayCategories,
            PredictiveSearchMaxResults,
            PredictiveSearchResultItemTransformationName,
            PredictiveSearchMoreResultsContent,
            searchURL);


        LogPredictiveSearch(searchText,
            PredictiveSearchLogSearchActivity,
            PredictiveSearchTrackSearchKeywords,
            DocumentID,
            UserID, false);

        return resultHTML;
    }


    SearchModeEnum GetSearchModeEnum(string input)
    {
        SearchModeEnum result = SearchModeEnum.AllWords;

        if (input.ToLower() == SearchModeEnum.AnyWord.ToString().ToLower())
        {
            result = SearchModeEnum.AnyWord;
        }
        else if (input.ToLower() == SearchModeEnum.AnyWordOrSynonyms.ToString().ToLower())
        {
            result = SearchModeEnum.AnyWordOrSynonyms;
        }
        else if (input.ToLower() == SearchModeEnum.ExactPhrase.ToString().ToLower())
        {
            result = SearchModeEnum.ExactPhrase;
        }

        return result;
    }


    SearchOptionsEnum GetSearchOptionsEnum(string input)
    {
        SearchOptionsEnum result = SearchOptionsEnum.BasicSearch;

        if (input.ToLower() == SearchOptionsEnum.FullSearch.ToString().ToLower())
        {
            result = SearchOptionsEnum.FullSearch;
        }
        else if (input.ToLower() == SearchOptionsEnum.NoneSearch.ToString().ToLower())
        {
            result = SearchOptionsEnum.NoneSearch;
        }

        return result;
    }

    /// <summary>
    /// Renders search results into HTML string. 
    /// </summary>
    private static string RenderResults(DataSet results, string searchText, string PredictiveSearchNoResultsContent, bool PredictiveSearchDisplayCategories, int PredictiveSearchMaxResults, string PredictiveSearchResultItemTransformationName, string PredictiveSearchMoreResultsContent, string searchURL)
    {
        if (results == null)
        {
            // No results
            return String.IsNullOrEmpty(PredictiveSearchNoResultsContent) ? "" : "<div class='nonSelectable'>" + PredictiveSearchNoResultsContent + "</div>";
        }
        else
        {
            UIRepeater repSearchResults = new UIRepeater();
            IDictionary<string, DataView> indexCategories = new Dictionary<string, DataView>();
            StringWriter stringWriter = new StringWriter();

            // Display categories - create DataView for each index
            if (PredictiveSearchDisplayCategories)
            {
                foreach (DataRow row in results.Tables["results"].Rows)
                {
                    string index = (string)row["index"];

                    if (!indexCategories.ContainsKey(index))
                    {
                        indexCategories.Add(index, new DataView(results.Tables["results"], "index = '" + index + "'", "", DataViewRowState.CurrentRows));
                    }
                }
            }
            // Do not display categories - create DataView of whole table
            else
            {
                indexCategories.Add("results", new DataView(results.Tables["results"]));
            }

            // Render each index category
            foreach (var categories in indexCategories)
            {
                // Display categories
                if (PredictiveSearchDisplayCategories)
                {
                    SearchIndexInfo indexInfo = SearchIndexInfoProvider.GetSearchIndexInfo(categories.Key);
                    string categoryName = indexInfo == null ? String.Empty : indexInfo.IndexDisplayName;
                    repSearchResults.HeaderTemplate = new TextTransformationTemplate("<div class='predictiveSearchCategory nonSelectable'>" + categoryName + "</div>");
                }

                // Fill repeater with results
                repSearchResults.ItemTemplate = new TextTransformationTemplate(CacheHelper.Cache(cs => CMS.PortalEngine.TransformationInfoProvider.GetTransformation(PredictiveSearchResultItemTransformationName), new CacheSettings(60, PredictiveSearchResultItemTransformationName)).TransformationCode);
                repSearchResults.DataSource = categories.Value;
                repSearchResults.DataBind();
                repSearchResults.RenderControl(new HtmlTextWriter(stringWriter));
            }

            // More results
            if (PredictiveSearchMaxResults == results.Tables["results"].Rows.Count)
            {
                stringWriter.Write(String.Format(PredictiveSearchMoreResultsContent, URLHelper.UpdateParameterInUrl(searchURL, "searchtext", HttpUtility.UrlEncode(searchText))));
            }

            return stringWriter.ToString();
        }
    }



    private static DataSet PredictiveSearch(string searchText,
        string PredictiveSearchDocumentTypes,
        string PredictiveSearchCultureCode,
        string PredictiveSearchCondition,
        SearchOptionsEnum PredictiveSearchOptions,
        SearchModeEnum PredictiveSearchMode,
        bool PredictiveSearchCombineWithDefaultCulture,
        string PredictiveSearchSort,
        string PredictiveSearchPath,
        bool PredictiveSearchCheckPermissions,
        string PredictiveSearchIndexes,
        int PredictiveSearchMaxResults,
        bool PredictiveSearchBlockFieldOnlySearch)
    {
        // Prepare search text
        var docCondition = new DocumentSearchCondition(PredictiveSearchDocumentTypes, PredictiveSearchCultureCode, CultureHelper.GetDefaultCultureCode(SiteContext.CurrentSiteName), PredictiveSearchCombineWithDefaultCulture);
        var condition = new SearchCondition(PredictiveSearchCondition, PredictiveSearchMode, PredictiveSearchOptions, docCondition);

        string searchCondition = SearchSyntaxHelper.CombineSearchCondition(searchText, condition);

        // Prepare parameters
        SearchParameters parameters = new SearchParameters()
        {
            SearchFor = searchCondition,
            SearchSort = PredictiveSearchSort,
            Path = PredictiveSearchPath,
            ClassNames = PredictiveSearchDocumentTypes,
            CurrentCulture = PredictiveSearchCultureCode,
            DefaultCulture = CultureHelper.GetDefaultCultureCode(SiteContext.CurrentSiteName),
            CombineWithDefaultCulture = PredictiveSearchCombineWithDefaultCulture,
            CheckPermissions = PredictiveSearchCheckPermissions,
            SearchInAttachments = false,
            User = MembershipContext.AuthenticatedUser,
            SearchIndexes = PredictiveSearchIndexes,
            StartingPosition = 0,
            DisplayResults = PredictiveSearchMaxResults,
            NumberOfProcessedResults = 100 > PredictiveSearchMaxResults ? PredictiveSearchMaxResults : 100,
            NumberOfResults = 0,
            AttachmentWhere = null,
            AttachmentOrderBy = null,
            BlockFieldOnlySearch = PredictiveSearchBlockFieldOnlySearch,
        };

        // Search
        DataSet results = SearchHelper.Search(parameters);
        return results;
    }


    /// <summary>
    /// Logs "internal search" activity and web analytics "on-site" search keyword. 
    /// </summary>    
    [WebMethod]
    [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Json)]
    public void LogPredictiveSearchWS(string searchText,
        string PredictiveSearchNoResultsContent,
        bool PredictiveSearchDisplayCategories,
        int PredictiveSearchMaxResults,
        string PredictiveSearchResultItemTransformationName,
        string PredictiveSearchMoreResultsContent,
        string searchURL,
        bool PredictiveSearchLogSearchActivity,
        bool PredictiveSearchTrackSearchKeywords,
        string PredictiveSearchDocumentTypes,
        string PredictiveSearchCultureCode,
        string PredictiveSearchCondition,
        string PredictiveSearchOptions,
        string PredictiveSearchMode,
        bool PredictiveSearchCombineWithDefaultCulture,
        string PredictiveSearchSort,
        string PredictiveSearchPath,
        bool PredictiveSearchCheckPermissions,
        string PredictiveSearchIndexes,
        bool PredictiveSearchBlockFieldOnlySearch,
        int DocumentID,
        int UserID)
    {
        LogPredictiveSearch(searchText, PredictiveSearchLogSearchActivity, PredictiveSearchTrackSearchKeywords, DocumentID, UserID, true);
    }


    private static void LogPredictiveSearch(string keywords, bool PredictiveSearchLogSearchActivity, bool PredictiveSearchTrackSearchKeywords, int DocumentID, int UserID, bool forceLogging)
    {
        if (PredictiveSearchLogSearchActivity || PredictiveSearchTrackSearchKeywords || forceLogging)
        {
            TreeNode currentDocument = (TreeNode)CacheLastN(10, 60, new Func<int, int, TreeNode>(GetDocument), UserID, DocumentID);

            if (PredictiveSearchLogSearchActivity || forceLogging)
            {
                Activity internalSearch = new ActivityInternalSearch(keywords, currentDocument, AnalyticsContext.ActivityEnvironmentVariables);
                internalSearch.Log();
            }

            if (PredictiveSearchTrackSearchKeywords || forceLogging)
            {
                AnalyticsHelper.LogOnSiteSearchKeywords(SiteContext.CurrentSiteName, currentDocument.NodeAliasPath, currentDocument.DocumentCulture, keywords, 0, 1);
            }
        }
    }


    #region  "Caching"

    /// <summary>
    /// Dictionary to keep the last N cache item names
    /// </summary>
    static Dictionary<string, DateTime> LastCacheItemNames = new Dictionary<string, DateTime>();

    /// <summary>
    /// Caches the last N objects based on the time (last accessed time)
    /// </summary>
    /// <param name="lastN">Number of objects to cache</param>
    /// <param name="time">The time in minutes</param>
    /// <param name="method">The method getting the object</param>
    /// <param name="methodArgs">Parameters of the method (they are also used to identify the objects and generate the cache key)</param>
    /// <returns>Cached or retrieved object</returns>
    static object CacheLastN(int lastN, int time, Delegate method, params object[] methodArgs)
    {
        // now time
        DateTime nowTime = DateTime.Now;

        //Generating an unique cache item name for the cached data
        string safeCacheItemName = "last_n_";

        foreach (object argument in methodArgs)
        {
            // cache keys are always lower case
            safeCacheItemName += ValidationHelper.GetCodeName(argument).Replace('.', '_').ToLower() + "_";
        }

        safeCacheItemName = safeCacheItemName.TrimEnd('_');

        // if the cache item is available, update the date time stamp
        if (LastCacheItemNames.ContainsKey(safeCacheItemName))
        {
            LastCacheItemNames[safeCacheItemName] = nowTime;
        }
        else
        { 
            // remove the oldest item if we have already N items
            if (LastCacheItemNames.Count >= lastN)
            {
                string removedItemKey = LastCacheItemNames.OrderBy(key => key.Value).First().Key;
                LastCacheItemNames.Remove(removedItemKey);

                //remove it from the cache
                CacheHelper.TouchKey(removedItemKey);
            }

            // add the new item
            LastCacheItemNames.Add(safeCacheItemName, nowTime);
        }

        // let our cache method handle the retrieval and creation of the cache
        return CacheHelper.Cache(cs => method.DynamicInvoke(methodArgs), new CacheSettings(time, safeCacheItemName));
    }

    #endregion

    static TreeNode GetDocument(int UserID, int DocumentID)
    {
        return CMS.DocumentEngine.DocumentHelper.GetDocument(DocumentID, new TreeProvider(UserInfoProvider.GetUserInfo(UserID)));
    }

}
