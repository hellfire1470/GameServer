namespace GameData.Network.Packages.Abstract
{
    public abstract class Response : Package
    {
        public bool Success = false;
        private Response() { }
        protected Response(PackageType type, bool success = false) : base(type)
        {
            Success = success;
        }
    }
}
