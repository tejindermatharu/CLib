namespace CacheLib
{
    public class Cache
    {
        public object Value { get; set; }
        public DateTime WhenUsed { get; set; }

        public Cache(object value, DateTime whenUsed)
        {
            Value = value;
            WhenUsed = whenUsed;
        }

    }
}