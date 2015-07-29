using System.Collections.Specialized;

using CMS;
using CMS.OnlineMarketing;


[assembly: RegisterCustomClass("PushbulletMarketingAutomation", typeof(PushbulletMarketingAutomation))]
public class PushbulletMarketingAutomation:ContactAutomationAction
{
    public override void Execute()
    {

        // authorization access API token from https://www.pushbullet.com/account
        string accessToken = GetResolvedParameter<string>("PushbulletApiToken", string.Empty);

        // data to be posted
        var data = new NameValueCollection();
        data["type"] = GetResolvedParameter<string>("MessageType", string.Empty);
        data["title"] = GetResolvedParameter<string>("MessageTitle", string.Empty);
        data["body"] = GetResolvedParameter<string>("MessageBody", string.Empty);
        data["url"] = string.IsNullOrEmpty(GetResolvedParameter<string>("MessageUrl", string.Empty)) ? "" : CMS.Helpers.URLHelper.ResolveUrl(GetResolvedParameter<string>("MessageUrl", string.Empty));

        // notified devices
        string deviceName = GetResolvedParameter<string>("DeviceName", string.Empty);

        Pushbullet.Push(accessToken, deviceName, data);
    }
}