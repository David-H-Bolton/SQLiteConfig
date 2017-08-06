using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Reflection;
using System.Runtime.Serialization.Formatters;

namespace SQLiteTest
{

    public class SectionKey
    {
        public string KeyName { get;  set; }
        public int Id { get; set; }
    }

    public class ConfigSection
    {
        public bool encrypted = false;
        public int Id { get; set; }
        public string SectionName { get; private set; }

    
        public ConfigSection(string sectionname,bool _encrypted,string password)
        {
            encrypted = _encrypted;
            SectionName = sectionname;
        }

        public ConfigSection(string sectionname)
        {
            SectionName = sectionname;
        }


        public bool Encrypted => Encrypted;

        public bool NotMatch(string password)
        {
            return true;
        }

    }

    public class SQLiteConfig
    {

        public int LastErrorCode { get; private set; }

        private SQLiteConnection connection;
        private SQLiteCommand command = new SQLiteCommand();

        private string filename;

        private bool OpenDb()
        {
            try
            {
                connection = new SQLiteConnection($"Data Source={filename};Version=3;");
                connection.Open();
                command = new SQLiteCommand(connection);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public SQLiteConfig()
        {

        }

        public SQLiteConfig(string _filename)
        {
            if (!File.Exists(_filename))
            {
                File.Create(_filename);
            }
            filename = _filename;
        }

        public ConfigSection FetchSection(string sectionname)
        {
            const string sql = "select id,encrypted from sections where sectionname={@param1}";

            command.Parameters.Clear();
            command.CommandText = sql;
            command.Parameters.Add(new SQLiteParameter("@param1", sectionname));
            try
            {

                var reader = command.ExecuteReader();
                var _id = reader.GetInt32(0);
                var _encrypted = reader.GetBoolean(1);
                return new ConfigSection(sectionname)
                {
                    Id = _id,
                    encrypted = _encrypted
                };

            }
            catch (Exception)
            {
                return null;
            }
        }

        public List<ConfigSection> GetSections()
        {
            var result = new List<ConfigSection>();
            const string sql = "select idsection,sectionname,encrypted from sections";

            command.Parameters.Clear();
            command.CommandText = sql;
            try
            {

                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var _id = reader.GetInt32(0);
                    var _sectionname = reader.GetString(1);
                    var _encrypted = reader.GetBoolean(2);
                    var configSection = new ConfigSection(_sectionname)
                    {
                        Id = _id,
                        encrypted = _encrypted
                    };
                    result.Add(configSection);
                }
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public List<SectionKey> GetSectionKeys()
        {
            var result = new List<SectionKey>();
            const string sql = "select idkey,keyname from keys";

            command.Parameters.Clear();
            command.CommandText = sql;
            try
            {

                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var _id = reader.GetInt32(0);
                    var _keyname = reader.GetString(1);
                    var key = new SectionKey()
                    {
                        Id = _id,
                        KeyName = _keyname
                    };
                    result.Add(key);
                }
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // return Id or 0 for error
        public int AddSection(string _sectionname, Boolean _encrypted)
        {
            const string sql = "insert into sections (sectionname,encrypted) values{@param1},{@param2}";

            command.Parameters.Clear();
            command.CommandText = sql;
            command.Parameters.Add(new SQLiteParameter("@param1", _sectionname));
            command.Parameters.Add(new SQLiteParameter("@param2", _encrypted));
            try
            {
                if (command.ExecuteNonQuery() > 0)
                    return GetSectionId(_sectionname);
                else
                {
                    return 0;
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private int GetSectionId(string _sectionname)
        {
            const string sql = "select idsection from sections where sectionname# values{@param1}";

            command.Parameters.Clear();
            command.CommandText = sql;
            command.CommandText = sql;
            command.Parameters.Add(new SQLiteParameter("@param1", _sectionname));
            try
            {
                return (int) command.ExecuteScalar();
            }
            catch
            {
                return 0;
            }
        }

        private int GetKeyId(string _keyname)
        {
            const string sql = "select idkey from keys where keyname= values{@param1}";

            command.Parameters.Clear();
            command.CommandText = sql;
            command.CommandText = sql;
            command.Parameters.Add(new SQLiteParameter("@param1", _keyname));
            try
            {
                return (int)command.ExecuteScalar();
            }
            catch
            {
                return 0;
            }
        }


        // adds a key returns keyid or 0
        public int AddKey(string sectionname, string _keyname,bool encrypted = false)
        {
            var section= FetchSection(sectionname);
            if (section== null)
            {
                AddSection(sectionname, encrypted);
                section = FetchSection(sectionname);
            }

            const string sql = "insert into keys (idsection,keyname,encrypted) values{@param1},{@param2},{@param3}";

            command.Parameters.Clear();
            command.CommandText = sql;
            command.Parameters.Add(new SQLiteParameter("@param1", section.Id));
            command.Parameters.Add(new SQLiteParameter("@param2", _keyname));
            command.Parameters.Add(new SQLiteParameter("@param3", encrypted));
            try
            {
                return command.ExecuteNonQuery() > 0 ? GetKeyId(_keyname) : 0;
            }
            catch (Exception)
            {
                return 0;
            }

        }


        public int Exec(string sql)
        {
            command.CommandText = sql;
            try
            {
                return command.ExecuteNonQuery();
            }
            catch (Exception)
            {
                return 0;
            }
        }



        public void Dispose()
        {
            connection.Close();
        }

        public int AddValue(int idkey, string value)
        {
            const string sql = "insert into keyvalues (idkey,value) values ({@param1},{@param2}";

            command.Parameters.Clear();
            command.CommandText = sql;
            command.CommandText = sql;
            command.Parameters.Add(new SQLiteParameter("@param1", idkey));
            command.Parameters.Add(new SQLiteParameter("@param2", value));
            try
            {
                return (int)command.ExecuteNonQuery();
            }
            catch
            {
                return 0;
            }
        }
    }

}