namespace Talifun.Crusher.CommandLine.Options
{
    public delegate void OptionAction<TKey, TValue>(TKey key, TValue value);
}
