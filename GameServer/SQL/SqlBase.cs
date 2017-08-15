using System.Collections.Generic;
using GameData;

namespace GameServer.SQL
{

    public class Skill
    {
        public int Id { get; set; }
        public int Rank { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public float Cost { get; set; }
        // todo: do not use Fraction for targets. Use Enemy or Ally
        public FractionType Target { get; set; }
    }

    public class Stat
    {
        public StatType Type { get; set; }
        public float Value { get; set; }
    }

    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ItemType Type { get; set; }
        public List<Stat> Stats { get; set; }
        public int MaxSockets { get; set; }
        public ItemQuality Quality { get; set; }
    }

    public class ItemInstance
    {
        public int InstanceId { get; set; }
        public int ItemId { get; set; }
        public Dictionary<int, Dictionary<string, string>> Meta { get; set; }
    }

    public class ItemDrop
    {
        public Item Item { get; set; }
        public float Chance { get; set; }
    }

    public class DropList
    {
        public int Id { get; set; }
        public List<ItemDrop> Items { get; set; }
    }

    public class Entity
    {
        public int Id { get; set; }
        public int TextureId { get; set; }
        public FractionType Fraction { get; set; }
        public int SkillListId { get; set; }
        public int DropListId { get; set; }
        public RaceType Race { get; set; }
        public string Name { get; set; }
        public EntityQuality Quality { get; set; }
        public int Life { get; set; }
        public int LifePL { get; set; }
        public int Level { get; set; }
        public int LevelRange { get; set; }
        public RessourceType RessourceType { get; set; }
        public float Ressource { get; set; }
        public float RessourcePL { get; set; }
        public int MoveSpeed { get; set; }
        public float AtkSpeed { get; set; }
        public float DPS { get; set; }
        public float DPSRange { get; set; }
        public float DPSPL { get; set; }
        public float DPSPLRange { get; set; }
        public float Exp { get; set; }
        public float ExpPL { get; set; }
        public Dictionary<string, string> Meta { get; set; }
    }

    public class EntityInstance
    {
        public int InstanceId { get; set; }
        public int EntityId { get; set; }
        public Location Location { get; set; }
        public int Level { get; set; }
        public int Life { get; set; }
        public float Ressource { get; set; }
        public float MoveSpeed { get; set; }
        public float AtkSpeed { get; set; }
        public float DPS { get; set; }
        public float DPSRange { get; set; }
    }



}
