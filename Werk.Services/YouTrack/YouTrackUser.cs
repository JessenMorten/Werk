using System;
using Werk.Utility;
using YouTrackSharp.Users;

namespace Werk.Services.YouTrack
{
    public class YouTrackUser
    {
        public string Login { get; }

        public string FullName { get; }

        public string AvatarUrl { get; }

        public string Email { get; }

        public bool IsGuest { get; }

        public bool IsOnline { get; }

        internal YouTrackUser(User user, Uri serverUri)
        {
            Login = user.Login;
            FullName = user.FullName;
            AvatarUrl = $"{serverUri.WithEndingSlash().AbsoluteUri.Replace("/youtrack/", string.Empty)}{user.AvatarUrl}";
            Email = user.Email;
            IsGuest = user.IsGuest;
            IsOnline = user.IsOnline;
        }
    }
}
