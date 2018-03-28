using GameData.Environment.Location;
using GameData.Account.Character;

namespace GameData.Network.Packages
{
    public class MoveCharacterRequest : Abstract.Request
    {
        public MoveCharacterRequest() : base(PackageType.MoveCharacter) { }
        public Location NewLocation;
        public AnimationType AnimationType;
    }
    public class MoveCharacterResponse : Abstract.Response
    {
        public MoveCharacterResponse() : base(PackageType.MoveCharacter) { }
    }
}
