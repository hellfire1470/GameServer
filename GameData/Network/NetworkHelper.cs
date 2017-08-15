namespace Network
{
    public static class NetworkHelperExtention
    {
        public static GameData.Network.Packages.PackageType GetPackageType(byte[] data)
        {
            if (data == null)
            {
                return GameData.Network.Packages.PackageType.Unknown;
            }
            return (GameData.Network.Packages.PackageType)data[data.Length - 1];
        }
    }
}