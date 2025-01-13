using System.ComponentModel;
using System.Reflection;

namespace SharpExtensionKit;

public static class DescriptionExtensions
{
    /// <summary>
    /// 获取对象上的 [Description] 特性值。
    /// </summary>
    /// <param name="value">要读取描述的值。</param>
    /// <returns>返回描述信息，没有则返回空字符串。</returns>
    public static string GetDescription(this object value)
    {
        if (value == null)
            return string.Empty;

        // 获取运行时的实际类型
        var type = value.GetType();

        // 使用 UnderlyingSystemType 来获取底层实现类型
        var systemType = type.UnderlyingSystemType;

        // 如果是 Nullable<T>，提取基础类型
        var underlyingType = Nullable.GetUnderlyingType(systemType) ?? systemType;

        // 如果是枚举类型，获取枚举字段的 Description 特性
        
            var field = underlyingType.GetField(value.ToString());
            if (field != null)
            {
                var description = field.GetCustomAttribute<DescriptionAttribute>();
                return description?.Description ?? string.Empty;
            }
        

        // 如果是普通类型，检查其 Description 特性
        var typeDescription = underlyingType.GetCustomAttribute<DescriptionAttribute>();
        return typeDescription?.Description ?? string.Empty;
    }
}