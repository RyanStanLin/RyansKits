using System.Text;

namespace SharpExtensionKit;

public static class IEnumerableExtensions
{
    /// <summary>
    ///     Extracts specific properties of type <typeparamref name="TTarget" /> from a collection of type
    ///     <typeparamref name="TSource" />.
    /// </summary>
    /// <typeparam name="TSource">The type of elements in the source collection.</typeparam>
    /// <typeparam name="TTarget">The type of the target properties to extract.</typeparam>
    /// <param name="source">The source collection to traverse.</param>
    /// <param name="extractor">A function to extract the target property from each source element.</param>
    /// <returns>An IEnumerable of extracted properties of type <typeparamref name="TTarget" />.</returns>
    public static IEnumerable<TTarget> ExtractItems<TSource, TTarget>(
        this IEnumerable<TSource> source,
        Func<TSource, TTarget> extractor)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (extractor == null) throw new ArgumentNullException(nameof(extractor));
        //if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

        foreach (var item in source)
        {
            TTarget result = default;
            try
            {
                result = extractor(item);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error extracting property from item: {item}");
            }

            if (result != null) yield return result;
        }
    }
    
    /// <summary>
    /// Converts an IEnumerable<string> into a single string by concatenating each element directly.
    /// </summary>
    /// <param name="source">The collection of strings to be concatenated.</param>
    /// <returns>A single concatenated string.</returns>
    public static string JoinToString(this IEnumerable<string> source)
    {
        if (source == null) 
            throw new ArgumentNullException(nameof(source), "Source collection cannot be null.");

        var stringBuilder = new StringBuilder();
    
        foreach (var str in source)
        {
            stringBuilder.Append(str);
        }

        return stringBuilder.ToString();
    }
}