using System.Linq;
using System.Threading;
using System.Web;

using CMS.DocumentEngine;
using CMS.SiteProvider;
using Kentico.Content.Web.Mvc;
using Kentico.Web.Mvc;

namespace DancingGoat.Repositories.Implementation
{
    public class RepetitivePageRepository : IRepetitivePageRepository
    {
        private const string urlSlugFieldName = "UrlSlug";

        private readonly string[] _coreColumns =
        {
            "NodeGUID", "DocumentID", "NodeID", "DocumentGUID", "DocumentPublishFrom", "DocumentCreatedWhen", urlSlugFieldName
        };

        public bool IsPreviewEnabled => HttpContext.Current.Kentico().Preview().Enabled;

        public string PreviewCulture => HttpContext.Current.Kentico().Preview().CultureName;

        /// <summary>
        /// Gets a basic query for multiple pages.
        /// </summary>
        /// <typeparam name="TPage">Type of the Kentico page.</typeparam>
        /// <returns>Query that returns either the latest or the published versions of pages.</returns>
        public DocumentQuery<TPage> GetPages<TPage>()
            where TPage : TreeNode, new()
        {
            var query = DocumentHelper.GetDocuments<TPage>();

            if (IsPreviewEnabled)
            {
                query = query
                    .Columns(_coreColumns.Concat(new[] { "NodeSiteId" }))
                    .OnSite(SiteContext.CurrentSiteName)
                    .LatestVersion()
                    .Published(false)
                    .Culture(PreviewCulture);
            }
            else
            {
                query = query
                    .Columns(_coreColumns)
                    .OnSite(SiteContext.CurrentSiteName)
                    .Published()
                    .PublishedVersion()
                    .Culture(Thread.CurrentThread.CurrentUICulture.Name);
            }

            return query;
        }

        /// <summary>
        /// Gets a query for a single page by its URL slug.
        /// </summary>
        /// <typeparam name="TPage">Type of the Kentico page.</typeparam>
        /// <param name="urlSlug">URL slug.</param>
        /// <returns>Query that returns the page.</returns>
        public DocumentQuery<TPage> GetPage<TPage>(string urlSlug)
            where TPage : TreeNode, new()
            =>
            GetPages<TPage>()
                .TopN(1)
                .WhereEquals(urlSlugFieldName, urlSlug);
    }
}