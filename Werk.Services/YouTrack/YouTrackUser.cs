using System;
using Werk.Utility;
using YouTrackSharp.Users;

namespace Werk.Services.YouTrack
{
    public class YouTrackUser
    {
        public string Login { get; init; }

        public string FullName { get; init; }

        public string AvatarUrl { get; init; }

        public string Email { get; init; }

        public bool IsGuest { get; init; }

        public bool IsOnline { get; init; }

        public YouTrackUser()
        {

        }

        public YouTrackUser(User user, Uri serverUri)
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
