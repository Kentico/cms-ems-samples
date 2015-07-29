using System.Data;

using CMS.Helpers;
using CMS.DataEngine;
using CMS.Ecommerce;
using CMS.DocumentEngine;
using CMS.EventManager;

namespace Custom.CustomEventManager
{
    public class CustomEventHelper
    {
        // New columns' names
        public const string COLUMN_ATTENDEE_ORDERID = "AttendeeOrderId";
        public const string COLUMN_ATTENDEE_PAYMENTCOMPLETED = "AttendeePaymentCompleted";

        /// <summary>
        /// Adds two new columns (AttendeeOrderId, AttendeePaymentCompleted) to the table Events_Attendee
        /// </summary>
        public static void AddAttendeeTableColumns()
        {
            const string className = "cms.eventattendee";
            const string tableName = "Events_Attendee";
            var dc = DataClassInfoProvider.GetDataClassInfo(className);
            if (dc != null)
            {
                // Add new table columns        
                var tm = new TableManager("CMSConnectionString"); 
                tm.AddTableColumn(tableName, COLUMN_ATTENDEE_ORDERID, "int", true, null);
                tm.AddTableColumn(tableName, COLUMN_ATTENDEE_PAYMENTCOMPLETED, "bit", true, null);
                
                // Update XML schema
                dc.ClassXmlSchema = tm.GetXmlSchema(tableName);
                DataClassInfoProvider.SetDataClassInfo(dc);
                
            }
        }


        /// <summary>
        /// Create attendees for each product of the specified order.
        /// </summary>
        /// <param name="cart">Shopping cart object with order data.</param>
        public static void SetAttendees(ShoppingCartInfo cart)
        {
            if ((cart != null) && (cart.Customer != null))
            {                
                // Is order payment completed?
                var order = OrderInfoProvider.GetOrderInfo(cart.OrderId);
                var isPaymentCompleted = false;
                if (order != null)
                {
                    isPaymentCompleted = order.OrderIsPaid;
                }

                // Build WHERE condition to get specified tree nodes which represent specified products of the order
                var where = string.Empty;
                foreach (var item in cart.CartItems)
                {
                    where += item.SKUID + ",";                    
                }

                // Trim ending comma from WHERE condition
                if (where != "")
                {
                    where = where.Remove(where.Length - 1);
                    where = "NODESKUID IN (" + where + ")";
                }

                // Remove old attendees
                DeleteAttendees(cart.OrderId);

                // Select events (tree nodes) that represents specified products of the order
                var tree = new TreeProvider();
                DataSet ds = tree.SelectNodes(cart.SiteName, "/%", TreeProvider.ALL_CULTURES, false, "cms.bookingevent", where);

                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        // Get tree node ID of the event
                        int nodeId = ValidationHelper.GetInteger(dr["NodeID"], 0);

                        // Get product ID
                        int skuId = ValidationHelper.GetInteger(dr["SKUID"], 0);

                        // Get product units
                        int units = GetSKUShoppingCartUnits(cart, skuId);

                        // Create attendees and assign them to the specified event
                        for (int i = 1; i < units + 1; i++)
                        {                            
                            var attendee = new EventAttendeeInfo
                            {
                                AttendeeFirstName = cart.Customer.CustomerFirstName,
                                AttendeeLastName = cart.Customer.CustomerLastName + " (" + i + ")",
                                AttendeeEmail = cart.Customer.CustomerEmail,
                                AttendeeEventNodeID = nodeId
                            };
                            attendee.SetValue(COLUMN_ATTENDEE_ORDERID, cart.OrderId);
                            attendee.SetValue(COLUMN_ATTENDEE_PAYMENTCOMPLETED, isPaymentCompleted);

                            // Set attendee phone from billing address
                            var address = cart.ShoppingCartBillingAddress;
                            if (address != null)
                            {
                                attendee.AttendeePhone = address.AddressPhone;
                            }

                            EventAttendeeInfoProvider.SetEventAttendeeInfo(attendee);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Returns DataSet of attendees which are joined with the specified order
        /// </summary>
        /// <param name="orderId">Order ID.</param>    
        public static DataSet GetAttendees(int orderId)
        {
            // Prepare the parameters
            var parameters = new QueryDataParameters {{"@OrderId", orderId}};

            var conn = ConnectionHelper.GetConnection();
            return conn.ExecuteQuery("SELECT * FROM Events_Attendee WHERE " + COLUMN_ATTENDEE_ORDERID + " = @OrderID", parameters, QueryTypeEnum.SQLQuery, false);
        }


        /// <summary>
        /// Delete attendees of the specified order.
        /// </summary>
        /// <param name="orderId">Order ID.</param>    
        public static void DeleteAttendees(int orderId)
        {
            // Get attendees that are joind with the specified order
            var ds = GetAttendees(orderId);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                foreach(DataRow dr in ds.Tables[0].Rows)
                {
                    // Delete attendee one by one to run extra actions which are performed when event attendee is deleted
                    EventAttendeeInfoProvider.DeleteEventAttendeeInfo(new EventAttendeeInfo(dr));
                }
            }  
        }


        /// <summary>
        /// Returns sum of all units of the specified SKU in the shopping cart. 
        /// </summary>
        /// <param name="skuId">SKU ID.</param>        
        private static int GetSKUShoppingCartUnits(ShoppingCartInfo cart, int skuId)
        {
            if (cart == null)
            {
                return 0;
            }
            
            int result = 0;

            // Calculate units
            foreach (ShoppingCartItemInfo cartItem in cart.CartItems)
            {
                if (cartItem.SKUID == skuId)
                {
                    // Add units
                    result += cartItem.CartItemUnits;
                }
            }

            return result;
        }
    }
}
