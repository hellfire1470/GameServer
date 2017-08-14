
using GameData;

namespace Network
{
    // NetworkHelper Extention
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
    }


    namespace Packages
    {
        public enum PackageType
        {
            Unknown, Login, Logout, SelectCharacter, GetAccountCharacters, CreateCharacter, MoveCharacter
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
            public ErrorResult Error;
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
            public int CharacterId;
        }
        public class SelectCharacterResponse : Response
        {
            public SelectCharacterResponse() : base(PackageType.SelectCharacter) { }
            public int CharacterId;
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

        #region "MoveCharacter"
        public class MoveCharacterRequest : Request
        {
            public MoveCharacterRequest() : base(PackageType.MoveCharacter) { }
            public Location NewLocation;
            public CharacterAnimationType AnimationType;
        }
        public class MoveCharacterResponse : Response
        {
            public MoveCharacterResponse() : base(PackageType.MoveCharacter) { }
        }
        #endregion

        #region "Create Character"
        public class CreateCharacterRequest : Request {
            public CreateCharacterRequest() : base(PackageType.CreateCharacter) { }
            public Data.Character CharacterData;
        }
        public class CreateCharacterResponse : Response {
            public CreateCharacterResponse() : base(PackageType.CreateCharacter) { }
            public ErrorResult Error;
        }
#endregion
    }
}
