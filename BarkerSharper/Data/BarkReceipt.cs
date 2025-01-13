namespace BarkerSharper.Data;

public class BarkReceipt
{
    private readonly bool _isSucceed = false;
    private readonly DateTime? _timeStamp = null;

    public BarkReceipt(bool isSucceed, DateTime? timeStamp = null)
    {
        _isSucceed = isSucceed;

        if (isSucceed && timeStamp is null)
        {
            throw new ArgumentNullException(nameof(timeStamp), "Succeed but with null timestamp.");
        }

        _timeStamp = timeStamp;
    }

    public bool IsSucceed => _isSucceed;
}