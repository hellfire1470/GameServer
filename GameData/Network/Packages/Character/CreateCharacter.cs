namespace GameData.Network.Packages
{
    public class CreateCharacterRequest : Abstract.Request
    {
        public CreateCharacterRequest() : base(PackageType.CreateCharacter) { }
        public Character CharacterData;
    }
    public class CreateCharacterResponse : Abstract.Response
    {
        public CreateCharacterResponse() : base(PackageType.CreateCharacter) { }
        public ResultType Error;
    }
}
