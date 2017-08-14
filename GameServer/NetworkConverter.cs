using System;
namespace GameServer
{
    public static class NetworkConverter
    {
        public static Network.Data.Character NetworkCharacter(SqlCharacter character)
        {
            return new Network.Data.Character()
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