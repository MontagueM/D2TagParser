using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D2TagParser
{
    class Database
    {
        private string DatabasePath = "";
        private Dictionary<string, TagClass> DataDict;

        public Database(string DBPath)
        {
            DatabasePath = DBPath;
        }

        public TagClass GetClassObject(string TagClassString)
        {
            return DataDict[TagClassString];
        }

        public bool LoadExecutableDB()
        {
            if (DatabasePath == "")
            {
                Console.WriteLine("Invalid database path given");
                return false;
            }
            DataDict = new Dictionary<string, TagClass>();
            using (var connection = new SqliteConnection($"Data Source={DatabasePath}"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    SELECT ClassRef, Stride, Pointer0x38, Pointer0x40
                    FROM Classes
                ";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string TagClassString = reader.GetString(0);
                        TagClass Tag = new TagClass();
                        Tag.TagClassString = TagClassString;
                        Tag.Stride = reader.GetInt32(1);
                        Tag.Pointer0x38 = reader.GetInt32(2);
                        Tag.Pointer0x40 = reader.GetInt32(3);
                        if (!DataDict.ContainsKey(TagClassString))
                        {
                            DataDict.Add(TagClassString, Tag);
                        }
                    }
                    Console.WriteLine($"Finished reading database {DatabasePath}");
                }
            }
            return true;
        }
    }
}
