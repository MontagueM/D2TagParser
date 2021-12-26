using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D2TagParser
{
    enum PointerType
    {
        Unknown,
        Inline,
        Table,
        Absolute
    }

    /*
    InFileClassPointer:
        0x0 - target file hash
        0x4 - inline class that contains the offset at 0x8
        0x8 - offset within target file hash that points to another InFileClassPointer(uint64)

    The offset given is from the start of the file, it is not relative.It also always seems to point to the
    beginning of a class definition, which explains why it also gives the inline class so it can be parsed.

    I'm not really sure on the purpose of these, but to guess it would be something like keeping track of parent/child
    relationships or something in a linked list or something like it.

    The target file hash given can also point to itself, in which case its literally just used as an absolute pointer

    ////

    ClassHashZeros is just ClassHash but is 8 bytes long, last 4 bytes are always zeros
    I don't know what the classhash means though
    */
    enum FieldType : int
    {
        PointerNoClass = 2,
        Pointer = 3,
        Tag = 4,
        ClassHashZeros = 6,
        ClassHash = 8,
        InFileClassPointer = 9,
        Tag64 = 10,
        Hash = 11,
    }

    class Field
    {
        public int Offset;
        public FieldType Type;
    }

    class Hash
    {
        public int Offset;
        public string HashString;
        public string TypeString;
        private FieldType Type;

        public Hash(FieldType InType)
        {
            Type = InType;
        }

        static string LittleEndian(uint number)
        {
            byte[] bytes = BitConverter.GetBytes(number);
            string retval = "";
            foreach (byte b in bytes)
                retval += b.ToString("X2");
            return retval;
        }
        static string LittleEndian(ulong number)
        {
            byte[] bytes = BitConverter.GetBytes(number);
            string retval = "";
            foreach (byte b in bytes)
                retval += b.ToString("X2");
            return retval;
        }

        public void Parse(BinaryReader Handle, int InOffset)
        {
            Offset = InOffset;

            Handle.BaseStream.Seek(InOffset, SeekOrigin.Begin);
            if (Type == FieldType.Hash)
            {
                HashString = LittleEndian(Handle.ReadUInt32());
                TypeString = "Hash";
            }
            else if (Type == FieldType.Tag)
            {
                HashString = LittleEndian(Handle.ReadUInt32());
                TypeString = "TagHash";
            }
            else if (Type == FieldType.Tag64)
            {
                Handle.BaseStream.Seek(8, SeekOrigin.Current);
                HashString = LittleEndian(Handle.ReadUInt64());
                TypeString = "TagHash64";
            }
        }
    }

    class Pointer
    {
        private int DefinedOffset;
        private int Offset;
        public int Count;
        private PointerType Type;
        public ClassEntry InlineClass;
        public Table TableClass;

        static string LittleEndian(uint number)
        {
            byte[] bytes = BitConverter.GetBytes(number);
            string retval = "";
            foreach (byte b in bytes)
                retval += b.ToString("X2");
            return retval;
        }

        public bool Parse(BinaryReader Handle, int InOffset, Database db, Dictionary<string, List<Field>> FieldsDict)
        {
            DefinedOffset = InOffset;
            Handle.BaseStream.Seek(InOffset, SeekOrigin.Begin);
            Offset = Handle.ReadInt32() + InOffset;
            if (Offset == InOffset) return false;
            Handle.BaseStream.Seek(Offset - 4, SeekOrigin.Begin);
            uint InlineClassInt = Handle.ReadUInt32();
            Handle.BaseStream.Seek(Offset + 8, SeekOrigin.Begin);
            uint TableClassInt = Handle.ReadUInt32();

            if ((InlineClassInt & 0xFFFF0000) == 0x80800000 && InlineClassInt != 0x80809FB8)
            {
                // Is inline, we don't need any other info
                Type = PointerType.Inline;
                InlineClass = new ClassEntry(LittleEndian(InlineClassInt), Offset);
                InlineClass.Parse(Handle, db, FieldsDict);
                return true;
            }
            else if ((TableClassInt & 0xFFFF0000) == 0x80800000)
            {
                // Is table, we need to get the count, adjust offset
                Type = PointerType.Table;
                Handle.BaseStream.Seek(Offset, SeekOrigin.Begin);
                Count = Handle.ReadInt32();
                TableClass = new Table(LittleEndian(TableClassInt), Offset + 0x10, Count);
                TableClass.Parse(Handle, db, FieldsDict);
                if (Count != 0) return true;
                return false;
            }
            else
            {
                Type = PointerType.Unknown;
                return false;
            }
        }
    }
}
