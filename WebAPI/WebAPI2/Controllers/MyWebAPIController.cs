using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using CustomWebApi;

// All of the rest is WebAPI thing, so no Kentico stuff in here
namespace CustomWebApi
{
    public class MyWebAPIController : ApiController
    {
        [Route("customapi2")] 
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