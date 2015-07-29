using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

// These are Kentico things to support WebAPI
// They are here for performance purposes, so that the WebAPI doesn't go through the whole solution, looking for classes that inherit from ApiController
using CMS.WebApi;

using CustomWebApi;

[assembly: RegisterApiController(typeof(MyWebAPIController))]

// All of the rest is WebAPI thing, so no Kentico stuff in here
namespace CustomWebApi
{
    public class MyWebAPIController : ApiController
    {
        public HttpResponseMessage Get()
        {
            // You can return variety of things in here, for more see http://www.asp.net/web-api/overview/getting-started-with-aspnet-web-api/action-results
            return Request.CreateResponse(HttpStatusCode.OK, new { Data = "test data" });
        }

        public HttpResponseMessage Get(int id)
        {
            // You can return variety of things in here, for more see http://www.asp.net/web-api/overview/getting-started-with-aspnet-web-api/action-results
            return Request.CreateResponse(HttpStatusCode.OK, new { Data = "test data", ID = id });
        }
    }
}