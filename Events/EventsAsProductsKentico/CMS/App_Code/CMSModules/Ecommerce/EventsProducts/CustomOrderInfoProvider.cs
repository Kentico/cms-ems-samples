using System.Data;

using CMS.Ecommerce;
using CMS.Helpers;
using CMS.EventManager;

using Custom.CustomEventManager;

/// <summary>
/// Makes sure that when an order is saved (with the payment status) the attendees get changed status whether they are paid or not.
/// </summary>
public class CustomOrderInfoProvider : OrderInfoProvider
{
    #region "Example: Events as products"

    /// <summary>
    /// Sets (updates or inserts) specified order.
    /// </summary>
    /// <param name="orderObj">Order to be set</param>  
    protected override void SetOrderInfoInternal(OrderInfo orderObj)
    {
        // Add or update the order
        base.SetOrderInfoInternal(orderObj);

        // Remember whether the order is paid
        bool isPaid = ((orderObj != null) && (orderObj.OrderIsPaid));
      
        if (isPaid)
        {
            // Get all attendees that are connected with the current order
            var ds = CustomEventHelper.GetAttendees(orderObj.OrderID);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    // Update attendee payment status
                    var attendee = new EventAttendeeInfo(dr);
                    attendee.SetValue(CustomEventHelper.COLUMN_ATTENDEE_PAYMENTCOMPLETED, true);
                    attendee.Update();
                }
            }
        }
    }


    /// <summary>
    /// Deletes all the assigned attendees to Events as products.
    /// </summary>
    /// <param name="orderObj">Order object.</param>
    protected override void  DeleteOrderInfoInternal(OrderInfo orderObj)
    {        
        if (orderObj != null)
        {            
            // Delete all attendees that are connected with the given order
            CustomEventHelper.DeleteAttendees(orderObj.OrderID);

            // Delete the order
            base.DeleteOrderInfoInternal(orderObj);
        }
    }

    #endregion
}