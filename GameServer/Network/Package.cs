using GameData.Network.Packages;
using Network;
using GameServer.Server;

namespace GameServer.Network
{
    public class Package
    {
        public delegate void PackageAction(Connection connection, NetworkReceiveEventArgs e);
        public PackageType Type { get; private set; }
        public PackageAction Action { get; private set; }

        public Package(PackageType type, PackageAction action)
        {
            Type = type;
            Action = action;
        }
    }
}
