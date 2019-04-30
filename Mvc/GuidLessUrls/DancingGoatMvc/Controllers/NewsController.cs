using System.Linq;
using System.Web.Mvc;

using DancingGoat.Filters;
using DancingGoat.Models.News;
using DancingGoat.Repositories;

namespace DancingGoat.Controllers
{
    public class NewsController : RepetitivePageController
    {
        public NewsController(IRepetitivePageRepository repetitivePageRepository) : base(repetitivePageRepository)
        {
        }

        public ActionResult Index()
        {
            var viewModel = PageRepository
                .GetPages<CMS.DocumentEngine.Types.DancingGoatMvc.News>()
                .AddColumns("Title", "NewsText")
                .OrderBy("DocumentPublishFrom", "DocumentCreatedWhen")
                .ToList()
                .Select(newsPage => MapViewModel(newsPage));

            return View(viewModel);
        }

        [GuidHeaderAdder(GuidHeaderName = pageGuidHeaderName)]
        public ActionResult Detail(string urlSlug)
        {
            var newsPage = PageRepository
                .GetPage<CMS.DocumentEngine.Types.DancingGoatMvc.News>(urlSlug)
                .AddColumns("Title", "NewsText")
                .ToList()
                .FirstOrDefault();

            if (newsPage != null)
            {
                //Response.AppendHeader(pageGuidHeaderName, newsPage.DocumentGUID.ToString());

                var viewModel = MapViewModel(newsPage);

                // Variant 1: return a view.
                return View(viewModel);

                // Variant 2: redirect to another page (local/external)
                //return RedirectToAction("Redirected", new { urlSlug });
            }

            return HttpNotFound();
        }

        public ActionResult Redirected(string urlSlug)
        {
            return Content(urlSlug);
        }

        protected NewsViewModel MapViewModel(CMS.DocumentEngine.Types.DancingGoatMvc.News newsPage) => new NewsViewModel
        {
            PageGuid = newsPage.DocumentGUID,
            Title = newsPage.Title,
            NewsText = newsPage.NewsText,
            PublicationDate = newsPage.PublicationDate,
            UrlSlug = newsPage.UrlSlug
        };
    }
}