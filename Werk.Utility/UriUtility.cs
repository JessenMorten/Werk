using System;

namespace Werk.Utility
{
    public static class UriUtility
    {
        public static Uri Create(string uri)
        {
            if (Uri.TryCreate(uri, UriKind.Absolute, out var result))
            {
                return result;
            }
            else
            {
                throw new InvalidOperationException($"Failed to create {nameof(Uri)} from '{uri}'");
            }
        }

        public static Uri WithEndingSlash(this Uri uri)
        {
            var result = uri;

            if (!uri.AbsoluteUri.EndsWith("/"))
            {
                result = new Uri(uri.AbsoluteUri + "/");
            }

            return result;
        }
    }
}
