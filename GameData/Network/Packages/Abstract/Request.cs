namespace GameData.Network.Packages.Abstract
{
    public abstract class Request : Package
    {
        protected Request(PackageType type) : base(type) { }
    }
}
