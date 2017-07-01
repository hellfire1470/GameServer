using System;
namespace Network
{
    namespace Helper
    {
        public static partial class NetworkHelper
        {
            public static Packages.PackageType GetPackageType(byte[] data)
            {
                if (data == null)
                {
                    return Packages.PackageType.Unknown;
                }
                return (Packages.PackageType)data[data.Length - 1];
            }

            public static Packages.Package DeserializeRequest(byte[] data)
            {
                switch (GetPackageType(data))
                {
                    case Packages.PackageType.Login:
                        return Deserialize<Packages.LoginRequest>(data);
                    case Packages.PackageType.SelectCharacter:
                        return Deserialize<Packages.SelectCharacterRequest>(data);
                    case Packages.PackageType.GetAccountCharacters:
                        return Deserialize<Packages.GetAccountCharactersRequest>(data);
                }
                return new Packages.Package();
            }
        }
    }
    namespace Packages
    {
        public enum PackageType
        {
            Unknown, Login, Logout, SelectCharacter, GetAccountCharacters
        }

        public class Package
        {
            public PackageType Type = PackageType.Unknown;
            public Package() { }
            public Package(PackageType type)
            {
                Type = type;
            }
        }

        public class Response : Package
        {
            public bool Success = false;
            public Response() : base() { }
            public Response(PackageType type, bool success = false) : base(type)
            {
                Success = success;
            }
        }

        public abstract class Request : Package
        {
            public Request(PackageType type) : base(type) { }
        }

        #region "Login"
        public class LoginRequest : Request
        {
            public LoginRequest() : base(PackageType.Login) { }
            public string Username;
            public string Password;
        }
        public class LoginResponse : Response
        {
            public LoginResponse() : base(PackageType.Login) { }
            public Data.Account Data;
        }
        #endregion
        #region "Logout"
        public enum LogoutStatus
        {
            CharacterSelection, TitleScreen
        }
        public class LogoutRequest : Request
        {
            public LogoutRequest() : base(PackageType.Logout) { }
            public LogoutStatus Status = LogoutStatus.TitleScreen;
        }
        public class LogoutResponse : Response
        {
            public LogoutResponse() : base(PackageType.Logout) { }
            public LogoutStatus Status;
        }
        #endregion
        #region "SelectCharacter"
        public class SelectCharacterRequest : Request
        {
            public SelectCharacterRequest() : base(PackageType.SelectCharacter) { }
            public long CharacterId;
        }
        public class SelectCharacterResponse : Response
        {
            public SelectCharacterResponse() : base(PackageType.SelectCharacter) { }
            public long CharacterId;
        }
        #endregion

        #region "GetAccountCharacters"
        public class GetAccountCharactersRequest : Request
        {
            public GetAccountCharactersRequest() : base(PackageType.GetAccountCharacters) { }
        }

        public class GetAccountCharactersResponse : Response
        {
            public GetAccountCharactersResponse() : base(PackageType.GetAccountCharacters) { }
            public Data.Character[] Characters;
        }
        #endregion
    }
}
