using System;
namespace GameData.Network.Packages
{
    public class JoinWorldRequest : Abstract.Request
    {
        public JoinWorldRequest() : base(PackageType.JoinWorld) { }
        public Character Character;
    }
    public class JoinWorldResponse : Abstract.Response
    {
        public JoinWorldResponse() : base(PackageType.JoinWorld) { }
    }
}
