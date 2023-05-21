using System.Collections.Generic;
using Syroot.Maths;
using Syroot.NintenTools.NSW.Bfres.Core;
using System;

namespace Syroot.NintenTools.NSW.Bfres
{
    /// <summary>
    /// Load and display information of RLT
    /// </summary>
    public class RelocationTableTest : IResData
    {
        private const string _signature = "_RLT";

        public IList<Section> sections { get; set; }
        public IList<Entry> entries { get; set; }

        public class Section
        {
            public uint position;
            public uint size;
            public int entryIndex;
            public int entryCount;
        }

        public class Entry
        {
            public uint position;
            public uint structCount;
            public int offsetCount;
            public int paddingCount;
        }


        public List<RelocationEntry> _savedSection1Entries;
        public List<long> _savedAnimCurvePointers;
        public List<long> _savedBoneAnimPointers;
        public List<long> _savedSkeletonAnimPointers;

        public class RelocationSection
        {
            internal List<RelocationEntry> Entries;
            internal int EntryIndex;
            internal uint Size;
            internal uint Position;

            internal RelocationSection(uint position, int entryIndex, uint size, List<RelocationEntry> entries)
            {
                Position = position;
                EntryIndex = entryIndex;
                Size = size;
                Entries = entries;
            }
        }

        public class RelocationEntry
        {
            internal uint Position;
            internal uint PadingCount;
            internal uint StructCount;
            internal uint OffsetCount;
            internal string Hint;

            internal RelocationEntry(uint position, uint offsetCount, uint structCount, uint padingCount, string hint)
            {
                Position = position;
                StructCount = structCount;
                OffsetCount = offsetCount;
                PadingCount = padingCount;
                Hint = hint;
            }
        }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            sections = new List<Section>();
            _savedSection1Entries = new List<RelocationEntry>();
            _savedAnimCurvePointers = new List<long>();
            _savedBoneAnimPointers = new List<long>();
            _savedSkeletonAnimPointers = new List<long>();

            loader.CheckSignature(_signature);
            uint position = loader.ReadUInt32();
            uint sectionCount = loader.ReadUInt32();
            uint padding = loader.ReadUInt32();

            Console.WriteLine("----------------------------------------------------------");
            Console.WriteLine("Testing RLT offsets....");

            for (int i = 0; i < sectionCount; i++)
            {
                ulong paddingsec = loader.ReadUInt64();

                Section section = new Section();
                section.position = loader.ReadUInt32();
                section.size = loader.ReadUInt32();
                section.entryIndex = loader.ReadInt32();
                section.entryCount = loader.ReadInt32();
                sections.Add(section);
                Console.WriteLine("----------------------------------------------------------");
                Console.WriteLine("Section " + i);
                if (i == 0)
                    Console.Write(" Start of file --> end of string table \n");
                if (i == 1)
                    Console.Write(" Index buffer \n");
                if (i == 2)
                    Console.Write(" Vertex buffer \n");
                if (i == 3)
                    Console.Write(" Memory pool \n");
                if (i == 3)
                    Console.Write(" External files \n");

                Console.WriteLine($"Position {section.position}");
                Console.WriteLine($"Size {section.size}");
                Console.WriteLine($"EntryIndex {section.entryIndex}");
                Console.WriteLine($"EntryCount {section.entryCount}");

            }
            Console.WriteLine("----------------------------------------------------------");
            for (int i = 0; i < sectionCount; i++)
            {
                entries = new List<Entry>();

                for (int e = 0; e < sections[i].entryCount; e++)
                {
                    //  Console.WriteLine("----------------------------------------------------------");
                    Entry entry = new Entry();
                    entry.position = loader.ReadUInt32();
                    entry.structCount = loader.ReadUInt16();
                    entry.offsetCount = loader.ReadByte();
                    entry.paddingCount = loader.ReadByte();
                    Console.WriteLine("\n----------------------------------------------------------");
                    Console.WriteLine("Entry " + e);
                    Console.Write(" Position " + entry.position);
                    Console.Write(" Offset count " + entry.offsetCount);
                    Console.Write(" Struct count " + entry.structCount);
                    Console.WriteLine(" Padding count " + entry.paddingCount);


                    entries.Add(entry);

                    Console.Write(" pointer");
                    using (loader.TemporarySeek(entry.position, System.IO.SeekOrigin.Begin))
                    {
                        for (int s = 0; s < entry.structCount; s++)
                        {
                            for (int off = 0; off < entry.offsetCount; off++)
                            {
                                long pos = loader.Position;

                                if (!loader.RelocatedPointers.Contains(pos))
                                    loader.RelocatedPointers.Add(pos);

                                long offset = loader.ReadInt64();

                                Console.Write(" " + offset);

                                try
                                {
                                    using (loader.TemporarySeek(offset, System.IO.SeekOrigin.Begin))
                                    {
                                    }
                                }
                                catch
                                {
                                    throw new ResException($"Invalid relocation pointer section {i} entry {e} pointer {offset}");
                                }
                            }
                            //Paddings
                            loader.ReadInt64s(entry.paddingCount);
                        }
                    }
                }
            }
        }

        void IResData.Save(ResFileSaver saver)
        {

        }

        private void CheckValidOffset()
        {

        }
    }
}
