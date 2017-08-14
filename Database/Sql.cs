using System.IO;
using Mono.Data.Sqlite;
using System.Collections.Generic;

namespace Database
{
    public class SQL
    {
        private readonly string _path;
        private readonly SqliteConnection _connection;

        public delegate void OnCreatedHandler(SQL sql);

        public SQL(string path, OnCreatedHandler OnCreated = null)
        {
            _path = path;
            bool raiseOnCreatedEvent = false;
            if (!File.Exists(path))
            {
                SqliteConnection.CreateFile(path);
                raiseOnCreatedEvent = true;
            }

            _connection = new SqliteConnection("Data Source=" + path + ";Version=3;");
            _connection.Open();

            if (raiseOnCreatedEvent && OnCreated != null)
                OnCreated(this);
        }

        public long LastInsertId()
        {
            SqliteCommand cmd = new SqliteCommand("select last_insert_rowid()", _connection);
            object scalar = cmd.ExecuteScalar();
            return scalar == null ? -1 : (long)scalar;
        }

        public int ExecuteNonQuery(string sql, object[] parameters = null)
        {
            SqliteCommand cmd = new SqliteCommand(sql, _connection);
            if (parameters != null)
            {
                int currentParameter = 0;
                foreach (object param in parameters)
                {
                    cmd.Parameters.Add(new SqliteParameter("@" + ++currentParameter, param));
                }
            }
            return cmd.ExecuteNonQuery();
        }

        public Dictionary<int, Dictionary<string, string>> ExecuteQuery(string sql, object[] parameters = null)
        {
            Dictionary<int, Dictionary<string, string>> ret = new Dictionary<int, Dictionary<string, string>>();

            SqliteCommand cmd = new SqliteCommand(sql, _connection);
            if (parameters != null)
            {
                int currentParameter = 0;
                foreach (object param in parameters)
                {
                    cmd.Parameters.Add(new SqliteParameter("@" + ++currentParameter, param));
                }
            }
            SqliteDataReader dr = cmd.ExecuteReader();

            int row = 0;

            while (dr.Read())
            {

                int field_count = dr.FieldCount;
                Dictionary<string, string> data = new Dictionary<string, string>();
                for (int field = 0; field < field_count; field++)
                {
                    data[dr.GetName(field)] = dr.GetValue(field).ToString();
                }
                ret[row++] = data;
            }
            return ret;
        }
    }
}
