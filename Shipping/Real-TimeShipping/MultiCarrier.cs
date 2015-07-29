using CMS;
using CMS.Base;
using CMS.DataEngine;
using CMS.Ecommerce;
using CMS.EventLog;
using CMS.Globalization;
using CMS.Helpers;
using CMS.Membership;
using dotnetSHIP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

[assembly: RegisterCustomClass("MultiCarrier", typeof(Custom.MultiCarrier))]
namespace Custom
{
    public class MultiCarrier : ICarrierProvider
    {
        enum ShipmentCarrier
        {
            FedEx,
            UPS,
            USPS,
            Other
        }

        /// <summary>
        /// Carrier provider name.
        /// </summary>
        public string CarrierProviderName
        {
            get
            {
                return "Multi Carrier";
            }
        }


        public List<KeyValuePair<string, string>> GetServices()
        {
            SortedDictionary<string, string> mServices = new SortedDictionary<string, string>
      {
            {"PRIORITY_OVERNIGHT", "Multi - FedEx Priority Overnight"},
            {"STANDARD_OVERNIGHT", "Multi - FedEx Standard Overnight"},
            {"UPS_NEXT_DAY_AIR_SAVER", "Multi - UPS Next Day Air Saver"},
            {"UPS_NEXT_DAY_AIR", "Multi - UPS Next Day Air"},
            {"USPSFirstClassMail", "Multi - USPS First-Class Mail"},
            {"USPSPriorityMail2-Day", "Multi - USPS Priority Mail 2-Day"},
            {"USPSPriorityMailExpress1-Day", "Multi - USPS Priority Mail Express 1-Day"}
      };

            return mServices.ToList();
        }

        public decimal GetPrice(Delivery delivery, string currencyCode)
        {
            decimal decRate = 0;
            try
            {
                ShipmentCarrier scCarrier = new ShipmentCarrier();
                string strShippingOptionName = "";
                switch (delivery.ShippingOption.ShippingOptionName)
                {
                    case "Multi-FedExPriorityOvernight":
                        strShippingOptionName = "Priority Overnight";
                        scCarrier = ShipmentCarrier.FedEx;
                        break;
                    case "Multi-FedExStandardOvernight":
                        strShippingOptionName = "Standard Overnight";
                        scCarrier = ShipmentCarrier.FedEx;
                        break;
                    case "Multi-UPSNextDayAirSaver":
                        strShippingOptionName = "UPS  Next Day Air Saver®";
                        scCarrier = ShipmentCarrier.UPS;
                        break;
                    case "Multi-UPSNextDayAir":
                        strShippingOptionName = "UPS Next Day Air®";
                        scCarrier = ShipmentCarrier.UPS;
                        break;
                    case "Multi-USPSFirstClassMail":
                        strShippingOptionName = "First-Class Mail";
                        scCarrier = ShipmentCarrier.USPS;
                        break;
                    case "Multi-USPSPriorityMail2-Day":
                        strShippingOptionName = "Priority Mail 2-Day";
                        scCarrier = ShipmentCarrier.USPS;
                        break;
                    case "Multi-USPSPriorityMailExpress1-Day":
                        strShippingOptionName = "Priority Mail Express 1-Day";
                        scCarrier = ShipmentCarrier.USPS;
                        break;
                }

                Rates objRates = new Rates();

                CurrentUserInfo uinfo = MembershipContext.AuthenticatedUser;

                switch (scCarrier)
                {
                    case ShipmentCarrier.FedEx:
                        decRate = GetFedExRate(objRates, uinfo, delivery, strShippingOptionName);
                        break;
                    case ShipmentCarrier.UPS:
                        decRate = GetUPSRate(objRates, uinfo, delivery, strShippingOptionName);
                        break;
                    case ShipmentCarrier.USPS:
                        decRate = GetUSPSRate(objRates, uinfo, delivery, strShippingOptionName);
                        break;
                    default:
                        decRate = 10; //Set a default shipping rate in case there is an issue.
                        break;
                }
            }
            catch (Exception ex)
            {
                //Log the error
                EventLogProvider.LogException("MultiCarrier - GetPrice", "EXCEPTION", ex);
                //Set some base rate for the shipping
                decRate = 10;
            }
            return decRate;
        }

        public decimal GetFedExRate(Rates objRates, CurrentUserInfo uinfo, Delivery delivery, string strShippingOptionName)
        {
            decimal decRate = 0;

            try
            {
                // Cache the data for 10 minutes with a key
                using (CachedSection<Rates> cs = new CachedSection<Rates>(ref objRates, 60, true, null, "FexExRates-" + uinfo.UserID + "-" + delivery.DeliveryAddress.AddressZip + "-" + ValidationHelper.GetString(delivery.Weight, "")))
                {
                    if (cs.LoadData)
                    {

                        //Get real-time shipping rates from FedEx using dotNETShip
                        Ship objShip = new Ship();
                        objShip.FedExLogin = strFedExLogin;  // "Account number, meter number, key, password"
                        objShip.FedExURLTest = SettingsKeyInfoProvider.GetBoolValue("MultiCarrierTestMode");
                        objShip.OrigZipPostal = SettingsKeyInfoProvider.GetValue("SourceZip", "90001");
                        string[] strCountryState = SettingsKeyInfoProvider.GetValue("SourceCountryState", "US").Split(';');
                        CountryInfo ci = CountryInfoProvider.GetCountryInfo(ValidationHelper.GetString(strCountryState[0], "USA"));
                        objShip.OrigCountry = ci.CountryTwoLetterCode;
                        StateInfo si = StateInfoProvider.GetStateInfo(ValidationHelper.GetString(strCountryState[1], "California"));
                        objShip.OrigStateProvince = si.StateCode;

                        objShip.DestZipPostal = delivery.DeliveryAddress.AddressZip;
                        objShip.DestCountry = delivery.DeliveryAddress.GetCountryTwoLetterCode();
                        objShip.DestStateProvince = delivery.DeliveryAddress.GetStateCode();

                        objShip.Weight = (float)delivery.Weight;

                        objShip.Rate("FedEx");

                        cs.Data = objShip.Rates;
                    }

                    objRates = cs.Data;
                }
                if (objRates.Count > 0)
                {
                    foreach (Rate rate in objRates)
                    {
                        if (rate.Name.ToLower() == strShippingOptionName.ToLower())
                        {
                            decRate = ValidationHelper.GetDecimal(rate.Charge, 0) * 1.08m;
                            break;
                        }
                    }
                }
                else
                {
                    CacheHelper.ClearCache("FexExRates-" + uinfo.UserID + "-" + delivery.DeliveryAddress.AddressZip + "-" + ValidationHelper.GetString(delivery.Weight, ""));
                }
            }
            catch (Exception ex)
            {
                //Log the error
                EventLogProvider.LogException("MultiCarrier - GetFedExRate", "EXCEPTION", ex);
                //Set some base rate for the shipping
                decRate = 10;
            }

            return decRate;
        }

        public decimal GetUPSRate(Rates objRates, CurrentUserInfo uinfo, Delivery delivery, string strShippingOptionName)
        {
            decimal decRate = 0;

            try
            {
                // Cache the data for 10 minutes with a key
                using (CachedSection<Rates> cs = new CachedSection<Rates>(ref objRates, 60, true, null, "UPS-" + uinfo.UserID + "-" + delivery.DeliveryAddress.AddressZip + "-" + ValidationHelper.GetString(delivery.Weight, "")))
                {
                    if (cs.LoadData)
                    {

                        //Get real-time shipping rates from UPS using dotNETShip
                        Ship objShip = new Ship();
                        objShip.UPSLogin = strUPSLogin;
                        objShip.UPSTestMode = SettingsKeyInfoProvider.GetBoolValue("MultiCarrierTestMode");
                        objShip.OrigZipPostal = SettingsKeyInfoProvider.GetValue("SourceZip", "90001");
                        string[] strCountryState = SettingsKeyInfoProvider.GetValue("SourceCountryState", "US").Split(';');
                        CountryInfo ci = CountryInfoProvider.GetCountryInfo(ValidationHelper.GetString(strCountryState[0], "USA"));
                        objShip.OrigCountry = ci.CountryTwoLetterCode;
                        StateInfo si = StateInfoProvider.GetStateInfo(ValidationHelper.GetString(strCountryState[1], "California"));
                        objShip.OrigStateProvince = si.StateCode;

                        objShip.DestZipPostal = delivery.DeliveryAddress.AddressZip;
                        objShip.DestCountry = delivery.DeliveryAddress.GetCountryTwoLetterCode();
                        objShip.DestStateProvince = delivery.DeliveryAddress.GetStateCode();

                        objShip.Weight = (float)delivery.Weight;

                        objShip.Rate("UPS");

                        cs.Data = objShip.Rates;
                    }

                    objRates = cs.Data;
                }

                foreach (Rate rate in objRates)
                {
                    if (rate.Name.ToLower() == strShippingOptionName.ToLower())
                    {
                        decRate = ValidationHelper.GetDecimal(rate.Charge, 0);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                //Log the error
                EventLogProvider.LogException("MultiCarrier - GetUPSRate", "EXCEPTION", ex);
                //Set some base rate for the shipping
                decRate = 10;
            }

            return decRate;
        }

        public decimal GetUSPSRate(Rates objRates, CurrentUserInfo uinfo, Delivery delivery, string strShippingOptionName)
        {
            decimal decRate = 0;

            try
            {
                // Cache the data for 10 minutes with a key
                using (CachedSection<Rates> cs = new CachedSection<Rates>(ref objRates, 60, true, null, "USPS-" + uinfo.UserID + "-" + delivery.DeliveryAddress.AddressZip + "-" + ValidationHelper.GetString(delivery.Weight, "")))
                {
                    if (cs.LoadData)
                    {

                        //Get real-time shipping rates from USPS using dotNETShip
                        Ship objShip = new Ship();
                        objShip.USPSLogin = strUSPSLogin;
                        objShip.OrigZipPostal = SettingsKeyInfoProvider.GetValue("SourceZip", "90001");
                        string[] strCountryState = SettingsKeyInfoProvider.GetValue("SourceCountryState", "US").Split(';');
                        CountryInfo ci = CountryInfoProvider.GetCountryInfo(ValidationHelper.GetString(strCountryState[0], "USA"));
                        objShip.OrigCountry = ci.CountryTwoLetterCode;
                        StateInfo si = StateInfoProvider.GetStateInfo(ValidationHelper.GetString(strCountryState[1], "California"));
                        objShip.OrigStateProvince = si.StateCode;

                        objShip.DestZipPostal = delivery.DeliveryAddress.AddressZip;
                        objShip.DestCountry = delivery.DeliveryAddress.GetCountryTwoLetterCode();
                        objShip.DestStateProvince = delivery.DeliveryAddress.GetStateCode();

                        objShip.Length = 12;
                        objShip.Width = 12;
                        objShip.Height = 12;

                        objShip.Weight = (float)delivery.Weight;

                        objShip.Rate("USPS");

                        cs.Data = objShip.Rates;
                    }

                    objRates = cs.Data;
                }

                foreach (Rate rate in objRates)
                {
                    if (rate.Name.ToLower() == strShippingOptionName.ToLower())
                    {
                        decRate = ValidationHelper.GetDecimal(rate.Charge, 0);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                //Log the error
                EventLogProvider.LogException("MultiCarrier - GetUSPSRate", "EXCEPTION", ex);
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


        #region Carrier Logins

        private static string strFedExLogin = "[YOUR FEDEX LOGIN]";
        private static string strUPSLogin = "[YOUR UPS LOGIN]";
        private static string strUSPSLogin = "[YOUR USPS LOGIN]";

        #endregion

    }
}