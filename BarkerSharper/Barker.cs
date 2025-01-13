using System.Net;
using BarkerSharper.Data;
using BarkerSharper.Model;
using RestSharp;
using SharpExtensionKit;

namespace BarkerSharper;

public class Barker
{
    private readonly List<BarkConfiguration> _barkConfigurations = new();

    public Barker(BarkConfiguration barkConfiguration)
    {
        _barkConfigurations.Add(barkConfiguration ?? throw new ArgumentNullException(nameof(barkConfiguration)));
    }
    
    public Barker(List<BarkConfiguration> barkConfiguration)
    {
        _barkConfigurations = barkConfiguration ?? throw new ArgumentNullException(nameof(barkConfiguration));
    }

    /// <summary>
    /// 同步发送通知
    /// </summary>
    public List<BarkReceipt> Bark<T>(T notificationModel) where T : BarkNotificationBaseModel
    {
        List<BarkReceipt> receipts = new List<BarkReceipt>();
        foreach (var configuration in _barkConfigurations)
        {
            notificationModel.DeviceKey = configuration.DeviceKey; // Set the device key from configuration
            var request = BuildRequest(configuration.DeviceKey, notificationModel);
            var client = new RestClient(configuration.BaseUrl);
            var response = client.Execute(request);
            receipts.Add(ParseResponse(response));
        }
        return receipts;
    }

    /// <summary>
    /// 异步发送通知
    /// </summary>
    public async Task<List<BarkReceipt>> BarkAsync<T>(T notificationModel) where T : BarkNotificationBaseModel
    {
        List<BarkReceipt> receipts = new List<BarkReceipt>();
        foreach (var configuration in _barkConfigurations)
        {
            notificationModel.DeviceKey = configuration.DeviceKey; // Set the device key from configuration
            var request = BuildRequest(configuration.DeviceKey, notificationModel);
            var client = new RestClient(configuration.BaseUrl);
            var response = await client.ExecuteAsync(request);
            receipts.Add(ParseResponse(response));
        }
        return receipts;
    }

    private RestRequest BuildRequest<T>(string deviceKey, T notificationModel) where T : BarkNotificationBaseModel
    {
        if (notificationModel == null) throw new ArgumentNullException(nameof(notificationModel));

        notificationModel.Validate(); // Validate the notification model

        var request = new RestRequest("push", Method.Post)
            .AddHeader(Constant.JSONKEYWORD_CONTENTTYPE, $"{Constant.JSONKEYWORD_RESPONSE_TYPEJSON}; {Constant.JSONKEYWORD_RESPONSE_CHARSET_UTF8}");

        var payload = notificationModel.ToJson(); // Serialize the model to JSON
#if DEBUG
        Console.WriteLine(payload);
#endif
        request.AddBody(payload);

        return request;
    }

    private BarkReceipt ParseResponse(RestResponse response)
    {
        if (response.StatusCode != HttpStatusCode.OK)
            throw new System.InvalidOperationException($"Request failed with status code: {response.StatusCode}. Content: {response.Content}");

        if (string.IsNullOrWhiteSpace(response.Content))
            throw new InvalidOperationException("Response content is empty.");

        var responseBody = BarkNotificationResponseModel.FromJson(response.Content);

        if (responseBody.Message == Constant.RESPONSE_SUCCESS_TEXT)
            return new BarkReceipt(isSucceed: true, timeStamp: responseBody.Timestamp.ToLocalDateTime());
        else
            return new BarkReceipt(isSucceed: false);
    }
}