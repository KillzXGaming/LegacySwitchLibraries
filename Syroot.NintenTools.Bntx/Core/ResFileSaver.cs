using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Syroot.BinaryData;
using Syroot.Maths;
using System.Security.Cryptography;

namespace Syroot.NintenTools.NSW.Bntx.Core
{
    /// <summary>
    /// Saves the hierachy and data of a <see cref="Bntx.BntxFile"/>.
    /// </summary>
    public class BntxFileSaver : BinaryDataWriter
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets a data block alignment typically seen with <see cref="Buffer.Data"/>.
        /// </summary>
        internal const uint AlignmentSmall = 0x40;

        //For RLT
        internal const int Section1 = 1;
        internal const int Section2 = 2;

        // ---- FIELDS -------------------------------------------------------------------------------------------------

        private uint _ofsFileName;
        private uint _ofsFileSize;
        private uint _ofsStringPool;
        private uint _ofsRelocationTable;
        private uint Section1Size;
        private long _ofsMemoryPool;
        private long _ofsTextureDataBlock;
        private long _ofsEndOfBlock;
        private long DataBlockPosition;
        private List<long> _savedHeaderBlockPositions;
        private List<ItemEntry> _savedItems;
        private IDictionary<string, StringEntry> _savedStrings;
        private IDictionary<object, BlockEntry> _savedBlocks;
        private List<RelocationSection> _savedRelocatedSections;
        private List<RelocationEntry> _savedSection1Entries;
        private List<RelocationEntry> _savedSection2Entries;
        private List<long> _savedMipMapOffsets;


        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="BntxFileSaver"/> class saving data from the given
        /// <paramref name="bntxFile"/> into the specified <paramref name="stream"/> which is optionally left open.
        /// </summary>
        /// <param name="bntxFile">The <see cref="Bntx.bntxFile"/> instance to save data from.</param>
        /// <param name="stream">The <see cref="Stream"/> to save data into.</param>
        /// <param name="leaveOpen"><c>true</c> to leave the stream open after writing, otherwise <c>false</c>.</param>
        internal BntxFileSaver(BntxFile bntxFile, Stream stream, bool leaveOpen)
            : base(stream, Encoding.ASCII, leaveOpen)
        {
            ByteOrder = ByteOrder.LittleEndian;
            BntxFile = bntxFile;
        }

        internal BntxFileSaver(Texture texture, BntxFile bntxFile, Stream stream, bool leaveOpen)
            : base(stream, Encoding.ASCII, leaveOpen)
        {
            ByteOrder = ByteOrder.LittleEndian;
            Texture = texture;
            BntxFile = bntxFile;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BntxFileSaver"/> class for the file with the given
        /// <paramref name="fileName"/>.
        /// </summary>
        /// <param name="BntxFile">The <see cref="Bntx.BntxFile"/> instance to save.</param>
        /// <param name="fileName">The name of the file to save the data into.</param>
        internal BntxFileSaver(BntxFile BntxFile, string fileName)
            : this(BntxFile, new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read), false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BntxFileSaver"/> class for the file with the given
        /// <paramref name="fileName"/>.
        /// </summary>
        /// <param name="BntxFile">The <see cref="Bntx.BntxFile"/> instance to save.</param>
        /// <param name="fileName">The name of the file to save the data into.</param>
        internal BntxFileSaver(Texture texture, BntxFile BntxFile, string fileName)
            : this(texture, BntxFile, new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read), false)
        {
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the saved <see cref="Bntx.BntxFile"/> instance.
        /// </summary>
        internal BntxFile BntxFile { get; }

        /// <summary>
        /// Gets the saved <see cref="Bfres.Texture"/> instance.
        /// </summary>
        internal Texture Texture { get; }

        /// <summary>
        /// Gets the current index when writing lists or dicts.
        /// </summary>
        internal int CurrentIndex { get; private set; }

        // ---- METHODS (INTERNAL) -------------------------------------------------------------------------------------

        internal int round_up(int x, int y)
        {
            return ((x - 1) | (y - 1)) + 1;
        }

        internal void SetupLists()
        {
            // Create queues fetching the names for the string pool and data blocks to store behind the headers.
            _savedItems = new List<ItemEntry>();
            _savedStrings = new SortedDictionary<string, StringEntry>();
            _savedBlocks = new Dictionary<object, BlockEntry>();
            _savedHeaderBlockPositions = new List<long>();
            _savedRelocatedSections = new List<RelocationSection>();
            _savedSection1Entries = new List<RelocationEntry>();
            _savedSection2Entries = new List<RelocationEntry>();
            _savedMipMapOffsets = new List<long>();
        }

        /// <summary>
        /// Starts serializing the data from the <see cref="BntxFile"/> root.
        /// </summary>
        internal void Execute()
        {
            SetupLists();

            // Store the headers recursively and satisfy offsets to them, then the string pool and data blocks.
            ((IResData)BntxFile).Save(this);

            List<long> offsets = new List<long>();

            Align(8);
            WriteOffset(BntxFile.TextureArrayOffset);
            foreach (var tex in BntxFile.Textures)
            {
                offsets.Add(SaveOffset());
            }

            SetupStringPool();

            Align(8);
            WriteOffset(BntxFile.TextureDictOffset);
            ((IResData)BntxFile.TextureDict).Save(this);


            int offs = 0;

            Align(8);
            foreach (var tex in BntxFile.Textures)
            {
                WriteOffset(offsets[offs++]);
                ((IResData)tex).Save(this);
            }

            Section1Size = (uint)Position;

            Seek(16, SeekOrigin.Current);

            var alignment = round_up((int)Position, BntxFile.DataAlignment) - (int)Position;

            if (alignment != 0)
                Position += alignment - 16;

            DataBlockPosition = Position;

            WriteTextureBlock();

            bool CanWrite = SetupRelocationTable();

            WriteRelocationTable(CanWrite);

            using (TemporarySeek(_ofsStringPool, SeekOrigin.Begin))
            {
                WriteStrings();
            }

            //Now determine block sizes!!
            //A note regarding these. They don't use alignment
            //Any that do (buffers, external files) use the WriteBlocks method

            for (int i = 0; i < _savedHeaderBlockPositions.Count; i++)
            {
                Position = _savedHeaderBlockPositions[i];

                if (i == 0)
                {
                    Write((ushort)(_savedHeaderBlockPositions[1] - 4)); //Write the next block offset
                }
                else if (i == _savedHeaderBlockPositions.Count - 1)
                {
                    Write(0);
                    Write(_ofsEndOfBlock - _savedHeaderBlockPositions[i]); //Size of string table to relocation table
                }
                else
                {
                    if (i < _savedHeaderBlockPositions.Count - 1)
                    {
                        uint blockSize = (uint)(_savedHeaderBlockPositions[i + 1] - _savedHeaderBlockPositions[i]);
                        WriteHeaderBlock(blockSize, blockSize);
                    }
                }
            }

            Position = _ofsTextureDataBlock;
            Write(DataBlockPosition);

            // Save final file size into root header at the provided offset.
            Position = _ofsFileSize;
            Write((uint)BaseStream.Length);
            Flush();
        }
        internal bool SetupRelocationTable()
        {
            Align(BntxFile.DataAlignment);

            if (BntxFile.WriteOriginalRLT())
            {
                return false;
            }


            RelocationSection BntxFileMainSect;
            RelocationSection BufferSect;

            long RelocationTableOffset = Position;

            int EntryIndex = 0;
            uint EntryPos = 0;

            _savedSection1Entries = _savedSection1Entries.OrderBy(o => o.Position).ToList();
            _savedSection2Entries = _savedSection2Entries.OrderBy(o => o.Position).ToList();

            bool PrintDebug = true;

            if (PrintDebug)
            {
                foreach (RelocationEntry entry in _savedSection1Entries)
                {
                    Debug.WriteLine("Pos = " + entry.Position + " " + entry.StructCount + " " + entry.OffsetCount + " " + entry.PadingCount + " " + entry.Hint);
                }
                foreach (RelocationEntry entry in _savedSection2Entries)
                {
                    Debug.WriteLine("Pos = " + entry.Position + " " + entry.StructCount + " " + entry.OffsetCount + " " + entry.PadingCount + " " + entry.Hint);
                }
            }

            BntxFileMainSect = new RelocationSection(EntryPos, EntryIndex, Section1Size, _savedSection1Entries);
            EntryIndex += _savedSection1Entries.Count;
            BufferSect = new RelocationSection((uint)DataBlockPosition, EntryIndex, (uint)(RelocationTableOffset - DataBlockPosition), _savedSection2Entries);

            _savedRelocatedSections.Add(BntxFileMainSect);
            _savedRelocatedSections.Add(BufferSect);

            return true;
        }
        internal void SaveMipMapOffsets()
        {
            _savedMipMapOffsets.Add(Position);
            Write(long.MaxValue);
        }

        internal void ExportTexture()
        {
            SetupLists();
            //Write the header
            WriteSignature("_SubTEX ");
            long BlockOffset = Position;
            byte[] ReserveSpace = new byte[40];
            Write(ReserveSpace);
            ((IResData)Texture).Save(this);

            WriteOffset(BlockOffset);
            WriteTextureBlock();

            WriteStrings();
        }

        /// <summary>
        /// Save pointer array to be relocated in section 1
        /// </summary>
        [DebuggerStepThrough]
        internal void SaveRelocateEntryToSection(long pos, uint OffsetCount, uint StructCount, uint PaddingCount,uint SectionNumber, string Hint)
        {
            if (OffsetCount > 255)
            {
                SaveRelocateEntryToSection(pos, 255, StructCount, PaddingCount, SectionNumber, Hint);

                long NewPos = pos + 255 * sizeof(long);

                SaveRelocateEntryToSection(NewPos, OffsetCount - 255, StructCount, PaddingCount, SectionNumber, Hint);
            }
            else
            {
                if (SectionNumber == Section1)
                    _savedSection1Entries.Add(new RelocationEntry((uint)pos, OffsetCount, StructCount, PaddingCount, Hint));
                if (SectionNumber == Section2)
                    _savedSection2Entries.Add(new RelocationEntry((uint)pos, OffsetCount, StructCount, PaddingCount, Hint));
            }
        }
        internal void SaveFileNameString(string Name, bool Relocate = false)
        {
            _ofsFileName = (uint)Position;
            Write(0);
        }

        internal void SaveRelocationTable()
        {
            _ofsRelocationTable = (uint)Position;
            Write(0);
        }
        
        internal void SaveMemoryPool()
        {
            _ofsMemoryPool = Position;
            Write(0L);
        }
        internal void SaveTextureDataBlocks()
        {
            _ofsTextureDataBlock = Position;
            Write(0L);
        }

        /// <summary>
        /// Reserves space for an offset and size for header block.
        /// </summary>
        [DebuggerStepThrough]
        internal void SaveHeaderBlock(bool IsBinaryHeader = false)
        {
            _savedHeaderBlockPositions.Add(Position);
            if (IsBinaryHeader) //Binary header is just a uint with no long offset
                Write((ushort)0);
            else
                WriteHeaderBlock(0, 0L);
        }

        /// <summary>
        /// Reserves space for an offset to the <paramref name="resData"/> written later.
        /// </summary>
        /// <param name="resData">The <see cref="IResData"/> to save.</param>
        /// <param name="index">The index of the element, used for instances referenced by a <see cref="ResDict"/>.
        /// </param>
        [DebuggerStepThrough]
        internal void Save(IResData resData, int index = -1)
        {
            if (resData == null)
            {
                Write(0);
                return;
            }
            if (TryGetItemEntry(resData, ItemEntryType.ResData, out ItemEntry entry))
            {
                entry.Offsets.Add((uint)Position);
                entry.Index = index;
            }
            else
            {
                _savedItems.Add(new ItemEntry(resData, ItemEntryType.ResData, (uint)Position, index: index));
            }
            Write(UInt32.MaxValue);
            Write(0);

        }

        /// <summary>
        /// Reserves space for the <see cref="Bntx.BntxFile"/> file size field which is automatically filled later.
        /// </summary>
        [DebuggerStepThrough]
        internal void SaveFieldFileSize()
        {
            _ofsFileSize = (uint)Position;
            Write(0);
        }

        /// <summary>
        /// Reserves space for the <see cref="Bntx.BntxFile"/> string pool size and offset fields which are automatically
        /// filled later.
        /// </summary>
        [DebuggerStepThrough]
        internal void SaveFieldStringPool()
        {
            _ofsStringPool = (uint)Position;
            Write(0L);
        }

        /// <summary>
        /// Reserves space for an offset to the <paramref name="list"/> written later.
        /// </summary>
        /// <typeparam name="T">The type of the <see cref="IResData"/> elements.</typeparam>
        /// <param name="list">The <see cref="IList{T}"/> to save.</param>
        [DebuggerStepThrough]
        internal void SaveList<T>(IEnumerable<T> list, bool IsUint32 = false)
            where T : IResData, new()
        {
            if (list?.Count() == 0)
            {
                Write((long)0);
                return;
            }
            // The offset to the list is the offset to the first element.
            if (TryGetItemEntry(list.First(), ItemEntryType.ResData, out ItemEntry entry))
            {
                entry.Offsets.Add((uint)Position);
                entry.Index = 0;
            }
            else
            {
                // Queue all elements of the list.
                int index = 0;
                foreach (T element in list)
                {
                    if (index == 0)
                    {
                        // Add with offset to the first item for the list.
                        _savedItems.Add(new ItemEntry(element, ItemEntryType.ResData, (uint)Position, index: index));
                    }
                    else
                    {
                        // Add without offsets existing yet.
                        _savedItems.Add(new ItemEntry(element, ItemEntryType.ResData, index: index));
                    }
                    index++;
                }
            }
            Write(UInt32.MaxValue);

            if (IsUint32 == false) //Add 0s to read as Uint64 (Default). A few offsets like relocation table would set this to true
                Write(0);
        }

        /// <summary>
        /// Reserves space for an offset to the <paramref name="dict"/> written later.
        /// </summary>
        /// <typeparam name="T">The type of the <see cref="IResData"/> element values.</typeparam>
        /// <param name="dict">The <see cref="ResDict"/> to save.</param>
        [DebuggerStepThrough]
        internal void SaveDict(ResDict dict)
        {
            if (dict?.Count == 0)
            {
                Write(0L);
                return;
            }
            if (TryGetItemEntry(dict, ItemEntryType.Dict, out ItemEntry entry))
            {
                entry.Offsets.Add((uint)Position);
            }
            else
            {
                _savedItems.Add(new ItemEntry(dict, ItemEntryType.Dict, (uint)Position));
            }
            Write(UInt32.MaxValue);
            Write(0);
        }

        /// <summary>
        /// Reserves space for an offset to the <paramref name="data"/> written later with the
        /// <paramref name="callback"/>.
        /// </summary>
        /// <param name="data">The data to save.</param>
        /// <param name="callback">The <see cref="Action"/> to invoke to write the data.</param>
        [DebuggerStepThrough]
        internal void SaveCustom(object data, Action callback)
        {
            if (data == null)
            {
                Write((long)0);
                return;
            }
            if (TryGetItemEntry(data, ItemEntryType.Custom, out ItemEntry entry))
            {
                entry.Offsets.Add((uint)Position);
            }
            else
            {
                _savedItems.Add(new ItemEntry(data, ItemEntryType.Custom, (uint)Position, callback: callback));
            }
            Write(UInt32.MaxValue);
            Write(0);
        }

        /// <summary>
        /// Reserves space for an offset to the <paramref name="str"/> written later in the string pool with the
        /// specified <paramref name="encoding"/>.
        /// </summary>
        /// <param name="str">The name to save.</param>
        /// <param name="encoding">The <see cref="Encoding"/> in which the name will be stored.</param>
        [DebuggerStepThrough]
        internal void SaveString(string str, Encoding encoding = null)
        {
            if (str == null)
            {
                Write(0L);
                return;
            }
            if (_savedStrings.TryGetValue(str, out StringEntry entry))
            {
                entry.Offsets.Add((uint)Position);
            }
            else
            {
                _savedStrings.Add(str, new StringEntry((uint)Position, encoding));
            }

            Write(UInt32.MaxValue);
            Write(0);
        }

        //Load all bntx strings first. The string pool will be re written after to satisfy offsets
        internal void SetupStringPool()
        {
            Dictionary<string, Encoding> BntxStrings = new Dictionary<string, Encoding>();
            foreach (var tex in BntxFile.Textures)
            {
                if (!BntxStrings.ContainsKey(tex.Name))
                    BntxStrings.Add(tex.Name, null);

                if (tex.UserData == null)
                    tex.UserData = new List<UserData>();
                if (tex.UserDataDict == null)
                    tex.UserDataDict = new ResDict();

                foreach (var userdata in tex.UserData)
                {
                    if (!BntxStrings.ContainsKey(userdata.Name))
                        BntxStrings.Add(userdata.Name, null);

                    if (userdata.Type == UserDataType.String || 
                        userdata.Type == UserDataType.WString)
                    {
                        foreach (var data in userdata.GetValueStringArray())
                        {
                            if (!BntxStrings.ContainsKey(data) && userdata.Type == UserDataType.String)
                                BntxStrings.Add(data, Encoding.UTF8);

                            if (!BntxStrings.ContainsKey(data) && userdata.Type == UserDataType.WString)
                                BntxStrings.Add(data, Encoding.Unicode);
                        }
                    }
                }
            }

            if (!BntxStrings.ContainsKey(BntxFile.Name))
                BntxStrings.Add(BntxFile.Name, null);

            BntxStrings.Add("", null);

            // Add default strings first
            Dictionary<string, Encoding> sorted = new Dictionary<string, Encoding>();
            foreach (string str in BntxFile.StringTable.Strings.Values)
            {
                if (BntxStrings.ContainsKey(str))
                    sorted.Add(str, BntxStrings[str]);
            }

            foreach (string entry in BntxStrings.Keys)
            {
                if (!sorted.ContainsKey(entry))
                    sorted.Add(entry, BntxStrings[entry]); //Add new strings to the end
            }

            Align(4);
            uint stringPoolStart = (uint)Position;
            WriteSignature("_STR");
            SaveHeaderBlock();
            _ofsStringPool = (uint)Position;
            Write(sorted.Count - 1);
            uint stringPoolOffset = (uint)Position;

            uint _ofsFileNameString = 0;
            foreach (var entry in sorted)
            {
                Console.WriteLine("SSTR " + entry.Key);

                if (entry.Key == BntxFile.Name)
                {
                    if (!_savedStrings.ContainsKey(entry.Key))
                    {
                        _savedStrings.Add(entry.Key, new StringEntry((uint)Position));
                        _ofsFileNameString = (uint)Position;
                    }
                    else
                        continue;
                }

                Write((short)entry.Key.Length);

                // Write the name.
                Write(entry.Key, BinaryStringFormat.ZeroTerminated,
                    entry.Value != null ? entry.Value : Encoding);

                Align(2);
            }

            uint _ofsEndOfStringTable = (uint)Position;

            //Save the file name. Goes directly to name instead of size
            using (TemporarySeek(_ofsFileName, SeekOrigin.Begin))
            {
                if (_ofsFileNameString != 0)
                    Write(_ofsFileNameString + 2);
            }
        }

        public class StringData
        {
            public string Text;
            public Encoding Encoding;

            public StringData(string text, Encoding encoding = null) {
                Text = text;
                Encoding = encoding;
            }
        }

        /// <summary>
        /// Reserves space for offsets to the <paramref name="strings"/> written later in the string pool with the
        /// specified <paramref name="encoding"/>
        /// </summary>
        /// <param name="strings">The names to save.</param>
        /// <param name="encoding">The <see cref="Encoding"/> in which the names will be stored.</param>
        [DebuggerStepThrough]
        internal void SaveStrings(IEnumerable<string> strings, Encoding encoding = null)
        {
            foreach (string str in strings)
            {
                SaveString(str, encoding);
            }
        }

        /// <summary>
        /// Reserves space for an offset to the <paramref name="data"/> written later in the data block pool.
        /// </summary>
        /// <param name="data">The data to save.</param>
        /// <param name="alignment">The alignment to seek to before invoking the callback.</param>
        /// <param name="callback">The <see cref="Action"/> to invoke to write the data.</param>
        [DebuggerStepThrough]
        internal void SaveBlock(object data, uint alignment, Action callback)
        {
            if (data == null)
            {
                Write(0L);
                return;
            }
            if (_savedBlocks.TryGetValue(data, out BlockEntry entry))
            {
                entry.Offsets.Add((uint)Position);
            }
            else
            {
                _savedBlocks.Add(data, new BlockEntry((uint)Position, alignment, callback));
            }
            Write(UInt32.MaxValue);
            Write(0);
        }

        /// <summary>
        /// Writes a Bntx signature consisting of 4 ASCII characters encoded as an <see cref="UInt32"/>.
        /// </summary>
        /// <param name="value">A valid signature.</param>
        internal void WriteSignature(string value)
        {
            Write(Encoding.ASCII.GetBytes(value));
        }

        internal long SaveOffset()
        {
            long pos = Position;
            Write(0L);
            return pos;
        }

        // ---- METHODS (PRIVATE) --------------------------------------------------------------------------------------

        public void SaveUserData(IList<UserData> userData, long Target)
        {
            Align(8);
            SaveRelocateEntryToSection(Position, 2, (uint)userData.Count, 6, Section1, "user Data"); //      <------------ Entry Set
            WriteOffset(Target);
            foreach (UserData data in userData)
                ((IResData)data).Save(this);
        }

        public void SaveUserDataData(IList<UserData> userData)
        {
            int numStrings = userData.Sum(item => item.GetValueStringArray().Length);

            if ((numStrings) != 0)
                SaveRelocateEntryToSection(Position, (uint)(numStrings), 1, 0, Section1, "user Data info strings"); //      <------------ Entry Set

            foreach (UserData data in userData)
            {
                if (data.Type == UserDataType.String && data.GetValueStringArray().Length > 0)
                {
                    WriteOffset(data.DataOffset);
                    data.SaveData(this);
                    Align(8);
                }
            }
            foreach (UserData data in userData)
            {
                if (data.Type == UserDataType.WString && data.GetValueStringArray().Length > 0)
                {
                    WriteOffset(data.DataOffset);
                    data.SaveData(this);
                    Align(8);
                }
            }
            foreach (UserData data in userData)
            {
                if (data.Type == UserDataType.Int32 && data.GetValueInt32Array().Length > 0)
                {
                    WriteOffset(data.DataOffset);
                    data.SaveData(this);
                    Align(8);
                }
            }
            foreach (UserData data in userData)
            {
                if (data.Type == UserDataType.Single && data.GetValueSingleArray().Length > 0)
                {
                    WriteOffset(data.DataOffset);
                    data.SaveData(this);
                    Align(8);
                }
            }
            foreach (UserData data in userData)
            {
                if (data.Type == UserDataType.Byte && data.GetValueByteArray().Length > 0)
                {
                    WriteOffset(data.DataOffset);
                    data.SaveData(this);
                }
            }
        }

        private void WriteHeaderBlock(uint Size, long Offset)
        {
            Write(Size);
            Write(Offset);
        }

        private uint round_up(uint x, uint y)
        {
            return ((x - 1) | (y - 1)) + 1;
        }

        private void WriteTextureBlock()
        {
            WriteSignature("BRTD");
            SaveHeaderBlock();

            if (Texture != null)
            {
                int mip = 0;
                long BlockOffset = Position;
                foreach (long offset in Texture.MipOffsets)
                {
                    using (TemporarySeek(_savedMipMapOffsets[mip], SeekOrigin.Begin))
                    {
                        Write(BlockOffset + offset);
                    }
                    mip++;
                }
                foreach (var data in Texture.TextureData)
                    Write(data[0]);
            }
            else
            {
                int mip = 0;
                int curTex = 0;
                foreach (Texture tex in BntxFile.Textures)
                {
                    //     if (curTex != 0)
                    //         Align(tex.texture.Alignment);
                    long BlockOffset = Position;

                    foreach (long offset in tex.MipOffsets)
                    {
                        using (TemporarySeek(_savedMipMapOffsets[mip], SeekOrigin.Begin))
                        {
                            Write(BlockOffset + offset);
                        }
                        mip++;
                    }
                    foreach (List<byte[]> data in tex.TextureData)
                        Write(data[0]);
                    

                    curTex++;
                }
            }
        }

        private bool TryGetItemEntry(object data, ItemEntryType type, out ItemEntry entry)
        {
            foreach (ItemEntry savedItem in _savedItems)
            {
                if (savedItem.Data.Equals(data) && savedItem.Type == type)
                {
                    entry = savedItem;
                    return true;
                }
            }
            entry = null;
            return false;
        }
        private void WriteRelocationTable(bool CanWrite)
        {
            uint relocationTableOffset = (uint)Position;

            using (TemporarySeek(_ofsRelocationTable, SeekOrigin.Begin))
            {
                Write(relocationTableOffset);
            }

            _ofsEndOfBlock = (uint)Position + 4;

            if (!CanWrite)
            {
                Write(BntxFile.OriginalRLTChunk);
                return;
            }

            WriteSignature("_RLT");
            Write(relocationTableOffset);
            Write(_savedRelocatedSections.Count);
            Write(0); //padding

            foreach (RelocationSection section in _savedRelocatedSections)
            {
                Write(0L); //padding
                Write(section.Position);
                Write(section.Size);
                Write(section.EntryIndex);
                Write(section.Entries.Count);
            }

            foreach (RelocationSection section in _savedRelocatedSections)
            {
                foreach (RelocationEntry entry in section.Entries)
                {
                    Write(entry.Position);
                    Write((ushort)entry.StructCount);
                    Write((byte)entry.OffsetCount);
                    Write((byte)entry.PadingCount);
                }
            }
        }

            

        private void WriteStrings()
        {
            // Sort the strings ordinally.
            Dictionary<string, StringEntry> sorted = new Dictionary<string, StringEntry>();
            foreach (string str in BntxFile.StringTable.Strings.Values)
            {
                if (_savedStrings.ContainsKey(str) && !sorted.ContainsKey(str))
                    sorted.Add(str, _savedStrings[str]);
            }

            foreach (KeyValuePair<string, StringEntry> entry in _savedStrings)
            {
                if (!sorted.ContainsKey(entry.Key))
                    sorted.Add(entry.Key, entry.Value); //Add new strings to the end
            }

            Write(sorted.Count - 1);
            uint stringPoolOffset = (uint)Position;

            uint _ofsFileNameString = 0;
            foreach (KeyValuePair<string, StringEntry> entry in sorted)
            {
                if (entry.Key == BntxFile.Name)
                    _ofsFileNameString = (uint)Position;

                // Align and satisfy offsets.
                using (TemporarySeek())
                {
                    SatisfyOffsets(entry.Value.Offsets, (uint)Position);
                }

                Write((short)entry.Key.Length);

                long startPos = Position;

                // Write the name.
                Write(entry.Key, BinaryStringFormat.ZeroTerminated, entry.Value.Encoding ?? Encoding);

                ushort length = (ushort)(Position - startPos - 1);
                using (TemporarySeek(startPos - 2, SeekOrigin.Begin)) {
                    Write(length);
                }

                Align(2);
            }

            uint _ofsEndOfStringTable = (uint)Position;

            if (_ofsFileName != 0)
            {
                //Save the file name. Goes directly to name instead of size
                using (TemporarySeek(_ofsFileName, SeekOrigin.Begin))
                {
                    if (_ofsFileNameString != 0)
                        Write(_ofsFileNameString + 2);
                }
            }
   

            // Save string pool offset and size in main file header.
            uint stringPoolSize = (uint)(Position - stringPoolOffset);
        }

        private void WriteBlocks()
        {
            foreach (KeyValuePair<object, BlockEntry> entry in _savedBlocks)
            {
                // Align and satisfy offsets.
                if (entry.Value.Alignment != 0) Align((int)entry.Value.Alignment);
                using (TemporarySeek())
                {
                    SatisfyOffsets(entry.Value.Offsets, (uint)Position);
                }

                // Write the data.
                entry.Value.Callback.Invoke();
            }
        }

        public void WriteOffset(long offset)
        {
            long pos = Position;
            using (TemporarySeek(offset, SeekOrigin.Begin))
            {
                Write(pos);
            }
        }

        private void WriteOffsets()
        {
            using (TemporarySeek())
            {
                foreach (ItemEntry entry in _savedItems)
                {
                    SatisfyOffsets(entry.Offsets, entry.Target.Value);
                }
            }
        }

        private void SatisfyOffsets(IEnumerable<uint> offsets, uint target)
        {
            foreach (uint offset in offsets)
            {
                Position = offset;
                Write((int)(target));
            }
        }

        // ---- STRUCTURES ---------------------------------------------------------------------------------------------

        [DebuggerDisplay("{" + nameof(Type) + "} {" + nameof(Data) + "}")]
        private class ItemEntry
        {
            internal object Data;
            internal ItemEntryType Type;
            internal List<uint> Offsets;
            internal uint? Target;
            internal Action Callback;
            internal int Index;

            internal ItemEntry(object data, ItemEntryType type, uint? offset = null, uint? target = null,
                Action callback = null, int index = -1)
            {
                Data = data;
                Type = type;
                Offsets = new List<uint>();
                if (offset.HasValue) // Might be null for enumerable entries to resolve references to them later.
                {
                    Offsets.Add(offset.Value);
                }
                Callback = callback;
                Target = target;
                Index = index;
            }
        }

        private enum ItemEntryType
        {
            List, Dict, ResData, Custom
        }

        private class RelocationSection
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

        private class RelocationEntry
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

        private class StringEntry
        {
            internal List<uint> Offsets;
            internal Encoding Encoding;

            internal StringEntry(uint offset, Encoding encoding = null)
            {
                Offsets = new List<uint>(new uint[] { offset });
                Encoding = encoding;
            }
        }

        private class BlockEntry
        {
            internal List<uint> Offsets;
            internal uint Alignment;
            internal Action Callback;

            internal BlockEntry(uint offset, uint alignment, Action callback)
            {
                Offsets = new List<uint> { offset };
                Alignment = alignment;
                Callback = callback;
            }
        }
    }
}