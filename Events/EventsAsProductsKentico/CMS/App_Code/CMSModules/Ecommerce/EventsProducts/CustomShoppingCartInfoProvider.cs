using CMS.Ecommerce;

using Custom.CustomEventManager;

/// <summary>
/// Creates the attendees when an Event as product is bought as part of an order.
/// </summary>
public class CustomShoppingCartInfoProvider : ShoppingCartInfoProvider
{
    #region "Example: Events as products"

    protected override void SetOrderInternal(ShoppingCartInfo cartObj, bool generateInvoice)
    {
        // Create an order from the shopping cart data
        base.SetOrderInternal(cartObj, generateInvoice);

        // Init event attendees from the shopping cart data
        CustomEventHelper.SetAttendees(cartObj);
    }

    #endregion
}