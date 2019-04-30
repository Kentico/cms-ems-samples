using System;
using System.Web.Mvc;

using DancingGoat.Repositories;

namespace DancingGoat.Controllers
{
    public abstract class RepetitivePageController : Controller
    {
        protected const string pageGuidHeaderName = "DancingGoat-PageGuid";

        protected IRepetitivePageRepository PageRepository { get; }

        public RepetitivePageController(IRepetitivePageRepository repetitivePageRepository)
        {
            PageRepository = repetitivePageRepository ?? throw new ArgumentNullException(nameof(repetitivePageRepository));
        }
    }
}