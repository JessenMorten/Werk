using System;

namespace Werk.App
{
    public static class FormattingExtensions
    {
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
