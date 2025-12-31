namespace PolkadotRoots.Helpers
{
    public class TimeDateHelper
    {
        public static (string start, string end) FormatTimes(long? start, long? end)
        {
            DateTimeOffset? s = FromUnixMaybe(start);
            DateTimeOffset? e = FromUnixMaybe(end);

            string startText = s.HasValue ? s.Value.ToLocalTime().ToString("ddd, MMM d | HH:mm") : "TBA";
            string endText = e.HasValue ? e.Value.ToLocalTime().ToString("ddd, MMM d | HH:mm") : "TBA";

            return (startText, endText);
        }
        private static DateTimeOffset? FromUnixMaybe(long? val)
        {
            if (val is null) return null;
            try
            {
                var v = val.Value;

                if (val < 1_000_000_000_000)
                    return DateTimeOffset.FromUnixTimeSeconds(v);

                return DateTimeOffset.FromUnixTimeMilliseconds(v);
            }
            catch { return null; }
        }

    }
}
