namespace BarkerSharper.Data;

/// <summary>
/// Bark 配置类，用于管理通知服务的基本配置，包括根 URL 和设备密钥。
/// </summary>
public class BarkConfiguration
{
    /// <summary>
    /// Bark 服务的根 URL，必须为绝对 URL。
    /// </summary>
    public Uri BaseUrl { get; }

    /// <summary>
    /// 用于设备身份验证的密钥。
    /// </summary>
    public string DeviceKey { get; }

    /// <summary>
    /// 创建一个新的 Bark 配置实例。
    /// </summary>
    /// <param name="baseUrl">Bark 服务的根 URL。</param>
    /// <param name="deviceKey">设备密钥，用于身份验证。</param>
    /// <exception cref="ArgumentNullException">当 <paramref name="baseUrl"/> 或 <paramref name="deviceKey"/> 为 null 时抛出。</exception>
    /// <exception cref="ArgumentException">当 <paramref name="baseUrl"/> 不是绝对 URL，或 <paramref name="deviceKey"/> 为空字符串时抛出。</exception>
    public BarkConfiguration(Uri baseUrl, string deviceKey)
    {
        BaseUrl = ValidateBaseUrl(baseUrl);
        DeviceKey = ValidateDeviceKey(deviceKey);
    }

    /// <summary>
    /// 验证 BaseUrl 是否为有效的绝对 URL。
    /// </summary>
    /// <param name="baseUrl">待验证的 URL。</param>
    /// <returns>验证通过的绝对 URL。</returns>
    /// <exception cref="ArgumentNullException">当 <paramref name="baseUrl"/> 为 null 时抛出。</exception>
    /// <exception cref="ArgumentException">当 <paramref name="baseUrl"/> 不是绝对 URL 时抛出。</exception>
    private static Uri ValidateBaseUrl(Uri baseUrl)
    {
        if (baseUrl == null)
            throw new ArgumentNullException(nameof(baseUrl), "BaseUrl 不能为空。");

        if (!baseUrl.IsAbsoluteUri)
            throw new ArgumentException("BaseUrl 必须是绝对 URL。", nameof(baseUrl));

        return baseUrl;
    }

    /// <summary>
    /// 验证设备密钥是否合法。
    /// </summary>
    /// <param name="deviceKey">待验证的设备密钥。</param>
    /// <returns>验证通过的设备密钥。</returns>
    /// <exception cref="ArgumentNullException">当 <paramref name="deviceKey"/> 为 null 时抛出。</exception>
    /// <exception cref="ArgumentException">当 <paramref name="deviceKey"/> 为空或仅包含空白字符时抛出。</exception>
    private static string ValidateDeviceKey(string deviceKey)
    {
        if (deviceKey == null)
            throw new ArgumentNullException(nameof(deviceKey), "DeviceKey 不能为空。");

        if (string.IsNullOrWhiteSpace(deviceKey))
            throw new ArgumentException("DeviceKey 不能是空字符串或仅包含空白字符。", nameof(deviceKey));

        return deviceKey;
    }
}