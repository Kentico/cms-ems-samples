
using System.Linq;
using CMS;
using CMS.Ecommerce;
using CMS.Globalization;
using CMS.Helpers;
using System;
using CMS.EventLog;

[assembly: RegisterCustomProvider(typeof(CustomShippingOptionInfoProvider))]

/// <summary>
/// Sample shipping option info provider. 
/// </summary>
public class CustomShippingOptionInfoProvider : ShippingOptionInfoProvider
{
    /// <summary>
    /// Ensures that the shipping option is applicable only if user is not purchasing one fo the excluded products
    /// </summary>
    /// <param name="cart">Shopping cart data.</param>
    /// <param name="shippingOption">Shipping option which is being checked for applicability.</param>
    /// <returns>True if the shipping option is allowed to be applied for the current cart items, otherwise returns false.</returns>
    protected override bool IsShippingOptionApplicableInternal(ShoppingCartInfo cart, ShippingOptionInfo shippingOption)
    {
        bool blnOptionAllowed = true;

        try
        {
            // Does not check availability if shopping cart or shipping option object is not available
            if ((cart == null) || (shippingOption == null))
            {
                return true;
            }

            // Gets data for the ShippingOptionExcludedProducts field
            var shippingOptionExcludedProducts = shippingOption.GetValue("ShippingOptionExcludedProducts");

            // Does not check availability if no products were permitted
            if (shippingOptionExcludedProducts == null)
            {
                return true;
            }

            // Parses retrieved data
            var excludedProducts = shippingOptionExcludedProducts.ToString().Split(';');

            // Loop through the cart to see if it contains any of the selected products.
            // If so, do not allow the shipping option
            foreach (ShoppingCartItemInfo sci in cart.CartItems)
            {
                if (excludedProducts.Contains(ValidationHelper.GetString(sci.SKUID, "")))
                {
                    //Set the flag to false so the option is not allowed
                    blnOptionAllowed = false;
                    break;
                }
            }
        }
        catch(Exception ex)
        {
            EventLogProvider.LogException("CustomShippingOptionInfoProvider - IsShippingOptionApplicableInternal", "EXCEPTION", ex);
        }

        return blnOptionAllowed;
    }
}