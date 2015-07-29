using System.Web.Http;

using CMS;
using CMS.DataEngine;

using CustomWebApi;

[assembly: RegisterModule(typeof(CustomWebApiModule))]

namespace CustomWebApi
{
    /// <summary>
    /// Represents the Web API module.
    /// </summary>
    public class CustomWebApiModule : Module
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomWebApi.CustomWebApiModule"/> class.
        /// </summary>
        public CustomWebApiModule()
            : base(new CustomWebApiModuleMetadata())
        {

        }


        /// <summary>
        /// Initializes the module.
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}