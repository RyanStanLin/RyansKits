namespace SharpExtensionKit;

public static class TimestampExtensions
{
    /// <summary>
    /// Converts a Unix timestamp to a UTC DateTime object.
    /// </summary>
    /// <param name="timestamp">Unix timestamp (in seconds). Default to 0 if no input.</param>
    /// <returns>A DateTime object representing the UTC time.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the timestamp is negative.</exception>
    public static DateTime ToUtcDateTime(this long timestamp)
    {
        // Ensure timestamp is not negative
        if (timestamp < 0)
            throw new ArgumentOutOfRangeException(nameof(timestamp), "The timestamp cannot be negative.");

        return DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime;
    }

    /// <summary>
    /// Converts a Unix timestamp to a local DateTime object.
    /// </summary>
    /// <param name="timestamp">Unix timestamp (in seconds). Default to 0 if no input.</param>
    /// <returns>A DateTime object representing the local time.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the timestamp is negative.</exception>
    public static DateTime ToLocalDateTime(this long timestamp)
    {
        // Ensure timestamp is not negative
        if (timestamp < 0)
            throw new ArgumentOutOfRangeException(nameof(timestamp), "The timestamp cannot be negative.");

        return DateTimeOffset.FromUnixTimeSeconds(timestamp).LocalDateTime;
    }
}