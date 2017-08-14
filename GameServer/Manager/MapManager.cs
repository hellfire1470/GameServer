using System.Collections.Generic;
using Database;
using GameData;

public static class MapManager
{
    private static Dictionary<int, Map> _maps = new Dictionary<int, Map>();

    public static Map GetMap(int id)
    {
        if (_maps.ContainsKey(id))
            return _maps[id];
        return null;
    }

    public static void AddMap(Map map){
        _maps[map.Id] = map;
    }

    public static void LoadMaps(SQL sql){
        Dictionary<int, Dictionary<string, string>> maps = sql.ExecuteQuery("select * from map");
        foreach (Dictionary<string, string> map in maps.Values)
        {
            AddMap(new Map(){ Id = int.Parse(map["id"]), Name = map["name"]});
        }
    }
}