using System;
using System.Threading.Tasks;

using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.SiteProvider;

namespace UniqueUrlSlugEditor
{
    public class UniqueUrlSlugEditorControl : TextBoxControl
    {
        protected const string urlSlugFieldName = "UrlSlug";

        protected override CMSTextBox TextBox
        {
            get;
        }

        /// <summary>
        /// Parametrizable response header name.
        /// </summary>
        public string PageGuidHeaderName
        {
            get => ValidationHelper.GetString(GetValue(nameof(PageGuidHeaderName)), string.Empty);
            set => SetValue(nameof(PageGuidHeaderName), value);
        }

        public UniqueUrlSlugEditorControl()
        {
            TextBox = new CMSTextBox
            {
                ID = "txtUrl"
            };
        }

        protected override void OnInit(EventArgs e)
        {
            Controls.Add(TextBox);
            base.OnInit(e);
        }

        public override bool IsValid()
        {
            var node = Form.Data as TreeNode;

            // Check if the control runs within a context of a page.
            if (node != null)
            {
                // Get SiteInfo that contains the presentation URL.
                var siteInfo = SiteInfoProvider.GetSiteInfo(SiteContext.CurrentSiteID);
                var urlRoot = siteInfo != null && siteInfo.SitePresentationURL.EndsWith("/") ? siteInfo.SitePresentationURL : siteInfo.SitePresentationURL + "/";

                // Get the page with the URL pattern.
                var page = DocumentHelper.GetDocument(node, new TreeProvider());
                var dataClassInfo = DataClassInfoProvider.GetDataClassInfo(page?.ClassName);

                if (dataClassInfo != null)
                {
                    // Set the new URL slug value.
                    node.SetValue(urlSlugFieldName, Value);

                    // Resolve macros in the URL pattern to get the relative URL of the page in the MVC app.
                    var resolver = MacroResolver.GetInstance(false);
                    resolver?.SetAnonymousSourceData(node);
                    var newRelativeUrl = ResolveUrlPattern(dataClassInfo, resolver);

                    // Build the final URL out of the presentation URL and the resolved relative URL.
                    var fullUrl = $"{urlRoot}{newRelativeUrl}";

                    // Ping the MVC app.
                    var urlChecker = new UrlChecker(10, PageGuidHeaderName);
                    var checkResult = Task.Run(() => urlChecker.GetSuccessAndResponseHeaderFromUrlAsync(fullUrl)).Result;

                    return (checkResult.IsSuccess
                        && page != null
                        && page.DocumentGUID.Equals(checkResult.LastHeaderValue))
                        || (!checkResult.IsSuccess);
                }
            }

            return false;
        }

        private static string ResolveUrlPattern(DataClassInfo dataClassInfo, MacroResolver resolver) =>
            resolver?.ResolveMacros(dataClassInfo?.ClassURLPattern)?.TrimStart('~', '/');
    }
}
