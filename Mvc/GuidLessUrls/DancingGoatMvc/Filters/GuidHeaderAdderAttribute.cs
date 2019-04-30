using System;
using System.Web.Mvc;

using DancingGoat.Models;

namespace DancingGoat.Filters
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class GuidHeaderAdderAttribute : ActionFilterAttribute
    {
        public string GuidHeaderName { get; set; }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);

            if (string.IsNullOrEmpty(GuidHeaderName))
            {
                throw new Exception($"The {nameof(GuidHeaderName)} property must not be null or empty.");
            }

            var result = filterContext.Result as ViewResult;

            if (result != null && result.Model is IRepetitivePageViewModel)
            {
                var model = result.Model as IRepetitivePageViewModel;

                filterContext.RequestContext.HttpContext.Response.AppendHeader(GuidHeaderName,
                    model.PageGuid.ToString());
            }

            //throw new Exception($"The action method did not return a {nameof(ViewResult)} result with a model that implements {nameof(IRepetitivePageViewModel)}.");
        }
    }
}