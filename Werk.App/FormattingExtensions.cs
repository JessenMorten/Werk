using System;

namespace Werk.App
{
    public static class FormattingExtensions
    {
        public static string ToAgoMessage(this TimeSpan timeSpan)
        {
            if (timeSpan.TotalMinutes < 1)
            {
                var unit = (int)timeSpan.TotalSeconds == 1 ? "second" : "seconds";
                return $"{(int)timeSpan.TotalSeconds} {unit} ago";
            }
            else if (timeSpan.TotalHours < 1)
            {
                var unit = (int)timeSpan.TotalMinutes == 1 ? "minute" : "minutes";
                return $"{(int)timeSpan.TotalMinutes} {unit} ago";
            }
            else if (timeSpan.TotalDays < 1)
            {
                var unit = (int)timeSpan.TotalHours == 1 ? "hour" : "hours";
                return $"{(int)timeSpan.TotalHours} {unit} ago";
            }
            else
            {
                var unit = (int)timeSpan.TotalDays == 1 ? "day" : "days";
                return $"{(int)timeSpan.TotalDays} {unit} ago";
            }
        }

        public static string ToHourAndMinuteFormat(this double hours)
        {
            var wholeHours = (int)hours;
            var wholeMinutes = (int)Math.Round((hours - wholeHours) * 60);

            if (hours >= 1 && wholeMinutes > 0)
            {
                return $"{wholeHours}h {wholeMinutes}m";
            }
            else if (hours >= 1)
            {
                return $"{wholeHours}h";
            }
            else
            {
                return $"{wholeMinutes}m";
            }
        }
    }
}
