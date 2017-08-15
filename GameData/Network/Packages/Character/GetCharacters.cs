namespace GameData.Network.Packages
{
    public class GetCharactersRequest : Abstract.Request
    {
        public GetCharactersRequest() : base(PackageType.GetCharacters) { }
    }

    public class GetCharactersResponse : Abstract.Response
    {
        public GetCharactersResponse() : base(PackageType.GetCharacters) { }
        public Character[] Characters;
    }
}
