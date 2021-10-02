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

        internal YouTrackUser(User user)
        {
            Login = user.Login;
            FullName = user.FullName;
            AvatarUrl = user.AvatarUrl;
            Email = user.Email;
            IsGuest = user.IsGuest;
            IsOnline = user.IsOnline;
        }
    }
}
