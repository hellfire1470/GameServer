using GameData;

namespace Network
{
    namespace Data
    {
		public class Account
		{
			public int Id;
			public string Username;
		}

        public class Character
        {
            public long Id;
            public string Name;
            public int Level;
            public Race Race;
            public Class Class;
            public long Experience;
            public Location Location;
        }
    }
}
