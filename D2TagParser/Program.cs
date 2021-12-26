using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace D2TagParser
{
    class Program
    {
        static void Main(string[] args)
        {
            string TagToParse = "C:/Users/monta/Downloads/tagparse.bin";
            Console.WriteLine($"Parsing file {TagToParse}");
            string DatabasePath = "exe.db";
            Database db = new Database(DatabasePath);
            db.LoadExecutableDB();

            // Read fields dict from json file
            Dictionary<string, List<Field>> FieldsDict = JsonConvert.DeserializeObject<Dictionary<string, List<Field>>>(File.ReadAllText("fields3313.json"));

            Tag tag = new Tag(TagToParse, "B8978080");
            tag.Process(db, FieldsDict);
            tag.ExportJSON("C:/Users/monta/Downloads/");
            var a = 0;
        }
    }
}
