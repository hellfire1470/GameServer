using System;
using GameData;

namespace GameServer
{
    public class Character : Network.Data.Character
    {
        public static Character GetCharacterById(long Id)
        {
            Character character = new Character();
            character.Id = Id;
            character.Name = "Hellfire";
            character.Level = 2;
            character.Race = Race.Human;
            character.Class = Class.Rouge;
            character.Experience = 10000;
            character.Location = new Location()
            {
                CoordX = 0f,
                CoordY = 0f,
                CoordZ = 0f,
                Map = 0,
                Name = "Blup"
            };
            return character;
        }
    }
}
