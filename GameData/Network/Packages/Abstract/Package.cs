namespace GameData.Network.Packages.Abstract
{
    public abstract class Package
    {
        public PackageType Type = PackageType.Unknown;
        protected Package() { }
        protected Package(PackageType type)
        {
            Type = type;
        }
    }
}
