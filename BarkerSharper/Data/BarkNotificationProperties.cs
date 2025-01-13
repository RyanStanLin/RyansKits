using System.ComponentModel;
using SharpExtensionKit;

namespace BarkerSharper.Data;

/*
[Obsolete]
public class BarkNotificationBasicProperties
{
    [Description("title")] 
    public string Title { get; set; }
    [Description("body")] 
    public string Body { get; set; }
}

[Obsolete]
public class BarkNotificationOptionalProperties
{
    private int? _volume;
    private string? _icon;
    private int? _badge;
    
    [Description("subtitle")]
    public string? Subtitle { get; set; }

    /// <summary>
    /// 推送中断级别（如亮屏通知等）。
    /// </summary>
    [Description("level")]
    public NotificationLevel? Level { get; set; }

    /// <summary>
    /// 推送角标，为正整数。
    /// </summary>
    [Description("badge")]
    public int? Badge
    {
        get => _badge;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(Badge), "Badge 不能为负值。");
            _badge = value;
        }
    }
    
    [Description("autoCopy")]
    public bool? AutoCopy { get; set; }

    /// <summary>
    /// 自定义复制的内容。
    /// </summary>
    [Description("copy")]
    public string? Copy { get; set; }

    /// <summary>
    /// 推送铃声设置。
    /// </summary>
    [Description("sound")]
    public NotificationSound? Sound { get; set; }

    /// <summary>
    /// 是否重复播放铃声，`true` 为重复播放。
    /// </summary>
    [Description("call")]
    public bool? Call { get; set; }

    /// <summary>
    /// 自定义图标 URL，需为有效链接。
    /// </summary>
    [Description("icon")]
    public string? Icon
    {
        get => _icon;
        set
        {
            if (!string.IsNullOrWhiteSpace(value) && !Uri.TryCreate(value, UriKind.Absolute, out _))
                throw new ArgumentException("Icon 必须为有效的 URL 地址。", nameof(Icon));
            _icon = value;
        }
    }

    /// <summary>
    /// 消息分组标识，分组显示时使用。
    /// </summary>
    [Description("group")]
    public string? Group { get; set; }

    /// <summary>
    /// 加密内容。
    /// </summary>
    [Description("ciphertext")]
    public string? Ciphertext { get; set; }

    /// <summary>
    /// 重要警告的通知音量（默认为 5），值范围在 0 到 10 之间。
    /// </summary>
    [Description("volume")]
    public int? Volume
    {
        get => _volume;
        set
        {
            if (value < 0 || value > 10)
                throw new ArgumentOutOfRangeException(nameof(Volume), "Volume 必须在 0 到 10 之间。");
            _volume = value ?? 5;
        }
    }

    /// <summary>
    /// 是否保存推送消息，`true` 为保存。
    /// </summary>
    [Description("isArchive")]
    public bool? IsArchive { get; set; }

    /// <summary>
    /// 点击推送时跳转的 URL。
    /// </summary>
    [Description("url")]
    public Uri? Url { get; set; }
    
    public static object CreatePayload(string _deviceKey, BarkNotificationBasicProperties basicProperties, BarkNotificationOptionalProperties? optionalProperties)
    {
        return new
        {
            basicProperties.Title,
            basicProperties.Body,
            device_key = _deviceKey,
            subtitle = optionalProperties?.Subtitle,
            level = optionalProperties?.Level?.GetDescription(),
            badge = optionalProperties?.Badge,
            autoCopy = optionalProperties?.AutoCopy,
            copy = optionalProperties?.Copy,
            sound = optionalProperties?.Sound?.GetDescription(),
            icon = optionalProperties?.Icon,
            group = optionalProperties?.Group,
            ciphertext = optionalProperties?.Ciphertext,
            volume = optionalProperties?.Volume is >= 0 and <= Constant.BARK_NOTIFICATION_VOLUME_MAX ? optionalProperties.Volume : throw new ArgumentOutOfRangeException(nameof(optionalProperties.Volume), $"Volume must be between 0 and {Constant.BARK_NOTIFICATION_VOLUME_MAX}."),
            isArchive = optionalProperties?.IsArchive,
            url = optionalProperties?.Url?.ToString()
        };
    }
}
*/

public enum NotificationLevel
{
    [Description("critical")] Critical,
    [Description("active")] Active,
    [Description("timeSensitive")] TimeSensitive,
    [Description("passive")] Passive
}

public enum NotificationSound
{
    [Description("alarm")] Alarm,

    [Description("anticipate")] Anticipate,

    [Description("bell")] Bell,

    [Description("birdsong")] Birdsong,

    [Description("bloom")] Bloom,

    [Description("calypso")] Calypso,

    [Description("chime")] Chime,

    [Description("choo")] Choo,

    [Description("descent")] Descent,

    [Description("electronic")] Electronic,

    [Description("fanfare")] Fanfare,

    [Description("glass")] Glass,

    [Description("gotosleep")] GoToSleep,

    [Description("healthnotification")] HealthNotification,

    [Description("horn")] Horn,

    [Description("ladder")] Ladder,

    [Description("mailsent")] MailSent,

    [Description("minuet")] Minuet,

    [Description("multiwayinvitation")] MultiwayInvitation,

    [Description("newmail")] NewMail,

    [Description("newsflash")] NewsFlash,

    [Description("noir")] Noir,

    [Description("paymentsuccess")] PaymentSuccess,

    [Description("shake")] Shake,

    [Description("sherwoodforest")] SherwoodForest,

    [Description("silence")] Silence,

    [Description("spell")] Spell,

    [Description("suspense")] Suspense,

    [Description("telegraph")] Telegraph,

    [Description("tiptoes")] Tiptoes,

    [Description("typewriters")] Typewriters,

    [Description("update")] Update
}