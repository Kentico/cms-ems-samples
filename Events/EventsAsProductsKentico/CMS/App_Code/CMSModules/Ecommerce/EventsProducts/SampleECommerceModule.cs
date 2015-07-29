using CMS.Base;
using CMS.Core;
using CMS.Ecommerce;

/// <summary>
/// Sample e-commerce module class. Partial class ensures correct registration.
/// </summary>
[SampleECommerceModuleLoader]
public partial class CMSModuleLoader
{
    #region "Macro methods loader attribute"

    /// <summary>
    /// Module registration
    /// </summary>
    private class SampleECommerceModuleLoaderAttribute : CMSLoaderAttribute
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SampleECommerceModuleLoaderAttribute()
        {
            // Require E-commerce module to load properly
            RequiredModules = new string[] { ModuleName.ECOMMERCE };
        }


        /// <summary>
        /// Initializes the module
        /// </summary>
        public override void Init()
        {         
            // -- Uncomment this line to register the CustomOrderInfoProvider programmatically
            OrderInfoProvider.ProviderObject = new CustomOrderInfoProvider();

            // -- Uncomment this line to register the CustomShoppingCartInfoProvider programmatically
            ShoppingCartInfoProvider.ProviderObject = new CustomShoppingCartInfoProvider();
        }
    }

    #endregion
}