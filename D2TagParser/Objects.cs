using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D2TagParser
{
    class ClassEntry
    {
        public string Reference;
        public int Offset;
        public int Size;
        public List<Pointer> Pointers;
        public List<Hash> Hashes;
        private List<Field> Fields;
        private byte[] Data;

        public ClassEntry(string Reference, int Offset)
        {
            this.Reference = Reference;
            this.Offset = Offset;
        }

        public void ParseFields(Dictionary<string, List<Field>> FieldsDict)
        {
            Fields = FieldsDict[Reference];
        }

        private void ParsePointers(BinaryReader Handle, Database db, Dictionary<string, List<Field>> FieldsDict)
        {
            Pointers = new List<Pointer>();
            foreach (Field field in Fields)
            {
                if (field.Type == FieldType.Pointer)
                {
                    Pointer pointer = new Pointer();
                    bool success = pointer.Parse(Handle, Offset + field.Offset, db, FieldsDict);
                    if (success == true)
                    {
                        Pointers.Add(pointer);
                    }
                }
            }
        }

        private void ParseHashes(BinaryReader Handle)
        {
            Hashes = new List<Hash>();
            foreach (Field field in Fields)
            {
                if (field.Type == FieldType.Hash || field.Type == FieldType.Tag || field.Type == FieldType.Tag64)
                {
                    Hash hash = new Hash(field.Type);
                    hash.Parse(Handle, Offset + field.Offset);
                    Hashes.Add(hash);
                }
            }
        }

        public void Parse(BinaryReader Handle, Database db, Dictionary<string, List<Field>> FieldsDict)
        {
            Size = db.GetClassObject(Reference).Stride;
            ParseFields(FieldsDict);
            ParsePointers(Handle, db, FieldsDict);
            ParseHashes(Handle);
        }
    }

    class Table
    {
        public string ClassString;
        private int Offset;
        public int Count;
        public List<ClassEntry> Entries;
        private ClassEntry ClassObject;

        public Table(string ClassString, int Offset, int Count)
        {
            this.ClassString = ClassString;
            this.Offset = Offset;
            this.Count = Count;
        }

        public void Parse(BinaryReader Handle, Database db, Dictionary<string, List<Field>> FieldsDict)
        {
            ClassObject = new ClassEntry(ClassString, 0);
            ClassObject.ParseFields(FieldsDict);
            Entries = new List<ClassEntry>();
            for (int i = 0; i < Count; i++)
            {
                ClassEntry entry = new ClassEntry(ClassString, Offset + i * ClassObject.Size);
                entry.Parse(Handle, db, FieldsDict);
                Entries.Add(entry);
            }
        }
    }
}
