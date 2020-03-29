namespace Polaris.Options
{
    public enum SetValueOutput
    {
        Failed = 0,
        SetCreated = 1,
        SetReplaced = 2
    }

    public enum DeleteOutput
    {
        None = 0,
        All = 1,
        Some = 2
    }

    public enum OptionFound
    {
        No = -1,
        OnlyDictionaryFound = 0,
        Yes = 1
    }
}