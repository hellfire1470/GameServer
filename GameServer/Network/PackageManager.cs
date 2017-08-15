using Network;
using GameData.Network.Packages;
using System.Collections.Generic;
using GameServer.Server;

namespace GameServer.Network
{
    public static class PackageManager
    {
        private static readonly Dictionary<PackageType, Package> _dictPackages = new Dictionary<PackageType, Package>();

        public static void RegisterPackage(Package package)
        {
            _dictPackages[package.Type] = package;
        }

        public static void HandlePackage(Connection connection, NetworkReceiveEventArgs args)
        {
            PackageType type = NetworkHelperExtention.GetPackageType(args.Data);

            if (_dictPackages.ContainsKey(type))
            {
                _dictPackages[type].Action(connection, args);
            }
        }
    }
}