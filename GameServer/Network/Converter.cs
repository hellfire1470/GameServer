using GameData.Network;

namespace GameServer.Network
{
    public static class Converter
    {
        public static Character ConvertCharacter(SQL.CharacterData character)
        {
            return new Character();//Character.Load(character.Id);
        }
    }
}