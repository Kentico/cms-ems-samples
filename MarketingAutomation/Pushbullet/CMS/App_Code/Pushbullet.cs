using System.Net;
using System.Collections.Generic;
using System.Collections.Specialized;
using Newtonsoft.Json;

using CMS;
using CMS.OnlineMarketing;
using CMS.IO;

public static class Pushbullet
{

    enum RequestType
    {
        GET,
        POST
    }


    public class Device
    {
        public bool active { get; set; }
        public string iden { get; set; }
        public double created { get; set; }
        public double modified { get; set; }
        public string type { get; set; }
        public string kind { get; set; }
        public string nickname { get; set; }
        public string manufacturer { get; set; }
        public string model { get; set; }
        public int app_version { get; set; }
        public string fingerprint { get; set; }
        public bool pushable { get; set; }
        public string push_token { get; set; }
    }

    public class DevicesResult
    {
        public List<object> aliases { get; set; }
        public List<object> clients { get; set; }
        public List<object> contacts { get; set; }
        public List<Device> devices { get; set; }
        public List<object> grants { get; set; }
        public List<object> pushes { get; set; }
    }


    /// <summary>
    /// Pushes the notification to the devices
    /// </summary>
    /// <param name="accessToken">The Pushbullet API access token from www.pushbullet.com/account</param>
    /// <param name="deviceName">The device name where the message should be displayed (if left blank all devices are notified)</param>
    /// <param name="data">The data to be pushed (more info on docs.pushbullet.com/v2/pushes/)</param>
    public static void Push(string accessToken, string deviceName, NameValueCollection data)
    {

        // JSON request URL for getting all the devices to push to
        string url = "https://api.pushbullet.com/v2/devices";

        // getting the devices
        string json = RunRequest(RequestType.GET, url, null, "Authorization", "Bearer " + accessToken);

        DevicesResult deserializedProduct = JsonConvert.DeserializeObject<DevicesResult>(json);

        url = "https://api.pushbullet.com/v2/pushes";

        foreach (Device device in deserializedProduct.devices)
        {
            if (string.IsNullOrEmpty(deviceName) || device.nickname.ToLower().Contains(deviceName.ToLower()))
            {
                data["device_iden"] = device.iden;

                string result = RunRequest(RequestType.POST, url, data, "Authorization", "Bearer " + accessToken);
            }
        }
    }


    /// <summary>
    /// Running a GET/POST request
    /// </summary>
    /// <returns>Response as string</returns>
    /// <param name="type">GET or POST</param>
    /// <param name="url">URL of the request</param>
    /// <param name="data"></param>
    /// <param name="headers">Headers (max 2 values)</param>
    /// <returns>Result (JSON for GET, otherwise 'ok')</returns>
    private static string RunRequest(RequestType type, string url, NameValueCollection data, params string[] headers)
    {

        string result = "";

        if (type == RequestType.GET)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            request.Headers.Add(headers[0], headers[1]);

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            System.IO.Stream resStream = response.GetResponseStream();

            result = (new System.IO.StreamReader(resStream)).ReadToEnd();
        }


        if (type == RequestType.POST)
        {
            using (var wb = new WebClient())
            {
                wb.Headers.Add(headers[0], headers[1]);
                var response = wb.UploadValues(url, "POST", data);

                result = "ok";
            }
        }

        return result;
    }
}