namespace GameData.Network.Packages
{
    public class LoginRequest : Abstract.Request
    {
        public LoginRequest() : base(PackageType.Login) { }
        public string Username;
        public string Password;
    }
    public class LoginResponse : Abstract.Response
    {
        public LoginResponse() : base(PackageType.Login) { }
        public ResultType Error;
        public Account Account;
    }
}
