namespace DATValidatorLib
{
    public interface IDatValidator
    {
        event DatValidator.ValidationStatusEventHandler OnValidateStatus;
        event DatValidator.ValidationCompleteEventHandler OnValidateComplete;
        event DatValidator.ValidationErrorEventHandler OnValidateError;
        event DatValidator.ValidationProgressEventHandler OnValidateProgress;
    }
}