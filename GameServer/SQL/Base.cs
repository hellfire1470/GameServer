using System.Collections.Generic;

namespace GameServer.SQL
{
    public class Base : Database.SQL
    {
        public Base() : base(Settings.database_file, (Database.SQL sql) =>
        {
            Logger.Log("Generating Database File ... ", true);
            string sqlCmd = "";

            //MAP
            sqlCmd += "create table map(id INTEGER PRIMARY KEY AUTOINCREMENT, name VARCHAR[100]);";
            sqlCmd += "insert into map(name) values('Erzbisho');";

            //LOCATION
            sqlCmd += "create table location(id INTEGER PRIMARY KEY AUTOINCREMENT, mapid INTEGER, x FLOAT, y FLOAT, z FLOAT," +
           " CONSTRAINT FK_locationmapid FOREIGN KEY (mapid) REFERENCES map(id));";

            //ACCOUNT
            sqlCmd += "create table account(id INTEGER PRIMARY KEY AUTOINCREMENT, name VARCHAR[30] UNIQUE, password VARCHAR[256], maxcharacters INTEGER DEFAULT 3, gamekey VARCHAR[16] UNIQUE, banned BOOLEAN DEFAULT 0, bannedAt TIMESTAMP, banreason VARCHAR[256]);";

            //CHARACTER
            sqlCmd += "create table character(id INTEGER PRIMARY KEY AUTOINCREMENT, accountid INTEGER, name VERCHAR[30] UNIQUE, class INTEGER," +
           " race INTEGER, level INTEGER, exp INTEGER, locationid INTEGER, fraction INTEGER," +
           " CONSTRAINT FK_CharsAccountId FOREIGN KEY (accountid) REFERENCES account(id)," +
           " CONSTRAINT FK_charslocationid FOREIGN KEY (locationid) REFERENCES location(id));";
            sqlCmd += "create table charactermeta(aid INTEGER PRIMARY KEY AUTOINCREMENT, characterid INTEGER, key VARCHAR[100], value TEXT," +
                " CONSTRAINT FK_charactermetacid FOREIGN KEY (characterid) REFERENCES character(id));";

            //SKILL
            sqlCmd += "create table skill(id INTEGER PRIMARY KEY AUTOINCREMENT, rank INTEGER, name VARCHAR[100], description TEXT," +
           " cost FLOAT, targetfraction INTEGER);";
            sqlCmd += "create table skilllist(aid INTEGER PRIMARY KEY AUTOINCREMENT, skilllistid INTEGER, skillid INTEGER," +
                " CONSTRAINT FK_SkillListSkillId FOREIGN KEY (skillid) REFERENCES skill(id));";

            //STATS
            sqlCmd += "create table statlist(aid INTEGER PRIMARY KEY AUTOINCREMENT, statlistid INTEGER, stat INTEGER, value FLOAT);";

            //ITEM
            sqlCmd += "create table item(id INTEGER PRIMARY KEY AUTOINCREMENT, name VARCHAR[100], description TEXT, itemtype INTEGER," +
           " statlistid INTEGER, maxsocket INTEGER, quality INTEGER," +
           " CONSTRAINT FK_statlistid FOREIGN KEY (statlistid) REFERENCES statlist(statlistid));";
            sqlCmd += "create table iteminstance(id INTEGER PRIMARY KEY AUTOINCREMENT, itemid INTEGER," +
                " CONSTRAINT FK_iteminstanceitemid FOREIGN KEY (itemid) REFERENCES item(id));";
            sqlCmd += "create table iteminstancemeta(aid INTEGER PRIMARY KEY AUTOINCREMENT, iteminstanceid INTEGER, key VARCHAR[100], value TEXT," +
                " CONSTRAINT FK_iteminstancemetaitemid FOREIGN KEY (iteminstanceid) REFERENCES iteminstance(id));";

            //DROPS
            sqlCmd += "create table droplist(aid INTEGER PRIMARY KEY AUTOINCREMENT, droplistid INTEGER, itemid INTEGER, chance FLOAT," +
           " CONSTRAINT FK_droplistitemid FOREIGN KEY (itemid) REFERENCES item(id));";

            //ENTITY
            sqlCmd += "create table entity(id INTEGER PRIMARY KEY AUTOINCREMENT, textureid INTEGER, fraction INTEGER, skilllistid INTEGER," +
           " droplistid INTEGER, race INTEGER, name VARCHAR[100], quality INTEGER, life INTEGER, lifepl FLOAT, level INTEGER, levelrange INTEGER," +
           " ressourcetype INTEGER, ressource INTEGER, movespeeed FLOAT, atkspeed FLOAT, dps FLOAT, dpspl FLOAT, dpsrange FLOAT, dpsplrange FLOAT, exppl INTEGER," +
           " CONSTRAINT FK_entityskillistid FOREIGN KEY (skilllistid) REFERENCES skilllist(id)," +
           " CONSTRAINT FK_entitydroplisid FOREIGN KEY (droplistid) REFERENCES droplist(id));";
            sqlCmd += "create table entitymeta(aid INTEGER PRIMARY KEY AUTOINCREMENT, entityid INTEGER, metakey VARCHAR[100], metavalue TEXT," +
                " CONSTRAINT FK_entitymetaentityid FOREIGN KEY (entityid) REFERENCES entity(id));";
            sqlCmd += "create table entityinstance(id INTEGER PRIMARY KEY AUTOINCREMENT, entityid INTEGER, locationid INTEGER, level INTEGER," +
                " life INTEGER, ressource FLOAT, movespeed FLOAT, atkspeed FLOAT, dps FLOAT, dpsrange FLOAT," +
                " CONSTRAINT FK_entityinstancenetityid FOREIGN KEY (entityid) REFERENCES entity(id)," +
                " CONSTRAINT FK_entityinstancelocationid FOREIGN KEY (locationid) REFERENCES location(id));";

            sql.ExecuteNonQuery(sqlCmd);
            Logger.Log("Done");
        })
        {
            Logger.Log("Checking Database ... ", true);
            int rows = ExecuteQuery("SELECT name FROM sqlite_master WHERE type='table';").Count;
            if (rows != 16)
            {
                Environment.Stop("Database corrupted! Please remove " + Settings.database_file + " file and restart the server");
                return;
            }
            Logger.Log("Done");
        }




        public List<int> GetCharacterIds(int accountId)
        {
            Dictionary<int, Dictionary<string, string>> characters = ExecuteQuery("SELECT id FROM character WHERE accountid = @1", new object[] { accountId });
            List<int> characterIds = new List<int>();
            foreach (Dictionary<string, string> character in characters.Values)
            {
                characterIds.Add(int.Parse(character["id"]));
            }
            return characterIds;
        }
    }

}
