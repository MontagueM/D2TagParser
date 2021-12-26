using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D2TagParser
{
    class Tag : ClassEntry
    {
        private string TagString;
        private string FilePath;

        public Tag(string FilePath, string Reference) : base(Reference, 0)
        {
            this.Reference = Reference;
            this.Offset = 0;
            this.FilePath = FilePath;
        }
        public void Process(Database db, Dictionary<string, List<Field>> FieldsDict)
        {
            // Open handle to file
            BinaryReader Handle = new BinaryReader(File.Open(FilePath, FileMode.Open));
            
            // Parse the file recursively
            Parse(Handle, db, FieldsDict);
        }

        public void ExportJSON(string ExportDirectory)
        {
            string sz = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllTextAsync(ExportDirectory + "testJSON.json", sz);
        }

        public void ConvertClassJSON()
        {

        }
    }
}
