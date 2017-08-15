using GameData.Network;

namespace GameServer.Network
{
    public static class Converter
    {
        public static Character ConvertCharacter(SQL.Character character)
        {
            return new Character
            {
                Id = character.Id,
                Class = character.Class,
                Experience = character.Exp,
                Level = character.Level,
                Location = character.Location,
                Name = character.Name,
                Race = character.Race,
                Fraction = character.Fraction
            };
        }
    }
}