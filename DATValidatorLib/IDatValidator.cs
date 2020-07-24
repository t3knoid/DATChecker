namespace DATValidatorLib
{
    public interface IDatValidator
    {
        CompleteDelegate CompleteDelegateCallBack { get;  }
        LoggingDelegate LoggingDelegateCallback { get; }
        StatusDelegate StatusDelegateCallBack { get; }
    }
}