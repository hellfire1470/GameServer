
namespace GameData.Network.Packages
{
    public class LogoutRequest : Abstract.Request
    {
        public LogoutRequest() : base(PackageType.Logout) { }
    }
    public class LogoutResponse : Abstract.Response
    {
        public LogoutResponse() : base(PackageType.Logout) { }
    }
}
