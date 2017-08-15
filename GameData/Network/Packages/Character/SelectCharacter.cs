namespace GameData.Network.Packages
{
    public class SelectCharacterRequest : Abstract.Request
    {
        public SelectCharacterRequest() : base(PackageType.SelectCharacter) { }
        public int CharacterId;
    }
    public class SelectCharacterResponse : Abstract.Response
    {
        public SelectCharacterResponse() : base(PackageType.SelectCharacter) { }
        public int CharacterId;
    }
}
