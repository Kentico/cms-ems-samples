using CMS;
using CMS.DataEngine;
using CMS.Ecommerce;
using CMS.EventLog;
using CMS.Globalization;
using CMS.Helpers;
using CMS.Membership;
using Custom.FexExWebService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services.Protocols;

[assembly: RegisterCustomClass("FedExCarrier ", typeof(Custom.FedExCarrier ))]
namespace Custom
{
    public class FedExCarrier : ICarrierProvider
    {
        /// <summary>
        /// Carrier provider name.
        /// </summary>
        public string CarrierProviderName
        {
            get
            {
                return "FedEx Carrier";
            }
        }

        public List<KeyValuePair<string, string>> GetServices()
        {
            SortedDictionary<string, string> mServices = new SortedDictionary<string, string>
      {
            {"PRIORITY_OVERNIGHT", "FedEx Priority Overnight"},
            {"STANDARD_OVERNIGHT", "FedEx Standard Overnight"}
      };

            return mServices.ToList();
        }

        public decimal GetPrice(Delivery delivery, string currencyCode)
        {
            decimal decRate = 0;
            try
            {
                ServiceType stType = new ServiceType();
                switch (delivery.ShippingOption.ShippingOptionName)
                {
                    case "FedExPriorityOvernight":
                        stType = ServiceType.PRIORITY_OVERNIGHT;
                        break;
                    case "FedExStandardOvernight":
                        stType = ServiceType.STANDARD_OVERNIGHT;
                        break;
                }

                CurrentUserInfo uinfo = MembershipContext.AuthenticatedUser;

                RateReply reply = new RateReply();
                // Cache the data for 10 minutes with a key
                using (CachedSection<RateReply> cs = new CachedSection<RateReply>(ref reply, 60, true, null, "FexExRatesAPI-" + stType.ToString().Replace(" ", "-") + uinfo.UserID + "-" + delivery.DeliveryAddress.AddressZip + "-" + ValidationHelper.GetString(delivery.Weight, "")))
                {
                    if (cs.LoadData)
                    {
                        //Create the request
                        RateRequest request = CreateRateRequest(delivery, stType);
                        //Create the service
                        RateService service = new RateService();
                        // Call the web service passing in a RateRequest and returning a RateReply
                        reply = service.getRates(request);
                        cs.Data = reply;
                    }
                    reply = cs.Data;
                }

                if (reply.HighestSeverity == NotificationSeverityType.SUCCESS)
                {
                    foreach (RateReplyDetail repDetail in reply.RateReplyDetails)
                    {
                        foreach (RatedShipmentDetail rsd in repDetail.RatedShipmentDetails)
                        {
                            //Add an offset to handle the differencse in the testing envinronment
                            decRate = ValidationHelper.GetDecimal(rsd.ShipmentRateDetail.TotalNetFedExCharge.Amount * 1.08m, 0);
                        }
                    }
                }
                else
                {
                    //Clean up the cached value so the next time the value is pulled again
                    CacheHelper.ClearCache("FexExRatesAPI-" + stType.ToString().Replace(" ", "-") + uinfo.UserID + "-" + delivery.DeliveryAddress.AddressZip + "-" + ValidationHelper.GetString(delivery.Weight, ""));
                }
            }
            catch (Exception ex)
            {
                //Log the error
                EventLogProvider.LogException("FedExCarrier - GetPrice", "EXCEPTION", ex);
                //Set some base rate for the shipping
                decRate = 10;
            }
            return decRate;
        }

        public bool CanDeliver(Delivery delivery)
        {
            return true;
        }

        public Guid GetConfigurationUIElementGUID()
        {
            return Guid.Empty;
        }

        public Guid GetServiceConfigurationUIElementGUID(string serviceName)
        {
            return Guid.Empty;
        }

        #region FedEx API Call

        private static RateRequest CreateRateRequest(Delivery delivery, ServiceType stType)
        {
            // Build a RateRequest
            RateRequest request = new RateRequest();
            //
            request.WebAuthenticationDetail = new WebAuthenticationDetail();
            request.WebAuthenticationDetail.UserCredential = new WebAuthenticationCredential();
            request.WebAuthenticationDetail.UserCredential.Key = strFedExUserCredentialKey;
            request.WebAuthenticationDetail.UserCredential.Password = strFedExUserCredentialPassword;
            //
            request.ClientDetail = new ClientDetail();
            request.ClientDetail.AccountNumber = strFedExAccountNumber;
            request.ClientDetail.MeterNumber = strFedExMeterNumber;
            //
            request.TransactionDetail = new TransactionDetail();
            request.TransactionDetail.CustomerTransactionId = "***Rate Request***"; // This is a reference field for the customer.  Any value can be used and will be provided in the response.
            //
            request.Version = new VersionId();
            //
            request.ReturnTransitAndCommit = true;
            request.ReturnTransitAndCommitSpecified = true;
            //
            SetShipmentDetails(request, delivery, stType);
            //
            return request;
        }

        private static void SetShipmentDetails(RateRequest request, Delivery delivery, ServiceType stType)
        {
            request.RequestedShipment = new RequestedShipment();
            request.RequestedShipment.ShipTimestamp = DateTime.Now; // Shipping date and time
            request.RequestedShipment.ShipTimestampSpecified = true;
            request.RequestedShipment.DropoffType = DropoffType.REGULAR_PICKUP; //Drop off types are BUSINESS_SERVICE_CENTER, DROP_BOX, REGULAR_PICKUP, REQUEST_COURIER, STATION
            request.RequestedShipment.ServiceType = stType; // Service types are STANDARD_OVERNIGHT, PRIORITY_OVERNIGHT, FEDEX_GROUND ...
            request.RequestedShipment.ServiceTypeSpecified = true;
            request.RequestedShipment.PackagingType = PackagingType.FEDEX_PAK; // Packaging type FEDEX_BOK, FEDEX_PAK, FEDEX_TUBE, YOUR_PACKAGING, ...
            request.RequestedShipment.PackagingTypeSpecified = true;
            //
            SetOrigin(request, delivery);
            //
            SetDestination(request, delivery);
            //
            SetPackageLineItems(request, delivery);
            //
            request.RequestedShipment.PackageCount = ValidationHelper.GetString(request.RequestedShipment.RequestedPackageLineItems.Length, "1");
        }

        private static void SetOrigin(RateRequest request, Delivery delivery)
        {
            request.RequestedShipment.Shipper = new Party();
            request.RequestedShipment.Shipper.Address = new Address();
            request.RequestedShipment.Shipper.Address.StreetLines = new string[1] { SettingsKeyInfoProvider.GetValue("SourceStreet", "123 Street") };
            request.RequestedShipment.Shipper.Address.City = SettingsKeyInfoProvider.GetValue("SourceCity", "Los Angeles");
            string[] strCountryState = SettingsKeyInfoProvider.GetValue("SourceCountryState", "US").Split(';');
            CountryInfo ci = CountryInfoProvider.GetCountryInfo(ValidationHelper.GetString(strCountryState[0], "USA"));
            request.RequestedShipment.Shipper.Address.CountryCode = ci.CountryTwoLetterCode;
            StateInfo si = StateInfoProvider.GetStateInfo(ValidationHelper.GetString(strCountryState[1], "California"));
            request.RequestedShipment.Shipper.Address.StateOrProvinceCode = si.StateCode;
            request.RequestedShipment.Shipper.Address.PostalCode = SettingsKeyInfoProvider.GetValue("SourceZip", "90001");
        }

        private static void SetDestination(RateRequest request, Delivery delivery)
        {
            request.RequestedShipment.Recipient = new Party();
            request.RequestedShipment.Recipient.Address = new Address();
            request.RequestedShipment.Recipient.Address.StreetLines = new string[1] { delivery.DeliveryAddress.AddressLine1 };
            request.RequestedShipment.Recipient.Address.City = delivery.DeliveryAddress.AddressCity;
            request.RequestedShipment.Recipient.Address.PostalCode = delivery.DeliveryAddress.AddressZip;
            request.RequestedShipment.Recipient.Address.CountryCode = delivery.DeliveryAddress.GetCountryTwoLetterCode();
            request.RequestedShipment.Recipient.Address.StateOrProvinceCode = delivery.DeliveryAddress.GetStateCode();
        }

        private static void SetPackageLineItems(RateRequest request, Delivery delivery)
        {
            int i = 1;
            List<RequestedPackageLineItem> lstItems = new List<RequestedPackageLineItem>();
            foreach (DeliveryItem item in delivery.Items)
            {
                SKUInfo sku = item.Product;
                RequestedPackageLineItem ritem = new RequestedPackageLineItem();                
                ritem.SequenceNumber = i.ToString(); // package sequence number
                ritem.GroupPackageCount = i.ToString();
                // package weight
                ritem.Weight = new Weight();
                ritem.Weight.Units = WeightUnits.LB;
                ritem.Weight.UnitsSpecified = true;
                ritem.Weight.Value = ValidationHelper.GetDecimal(sku.SKUWeight, 1) * item.Amount;
                ritem.Weight.ValueSpecified = true;
                // package dimensions
                ritem.Dimensions = new Dimensions();
                ritem.Dimensions.UnitsSpecified = false;
                lstItems.Add(ritem);
                i += 1;
            }
            request.RequestedShipment.RequestedPackageLineItems = lstItems.ToArray();
        }

        #endregion


        #region Settings

        private static string strFedExUserCredentialKey = "[YOUR KEY]";
        private static string strFedExUserCredentialPassword = "[YOUR PASSWORD]";
        private static string strFedExAccountNumber = "[YOUR ACCOUNT NUMBER]";
        private static string strFedExMeterNumber = "[YOUR METER NUMBER]";

        #endregion

    }
}