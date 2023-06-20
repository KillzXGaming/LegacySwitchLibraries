using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Syroot.BinaryData;
using Syroot.Maths;

namespace Syroot.NintenTools.NSW.Bfres.Core
{
    /// <summary>
    /// Loads the hierachy and data of a <see cref="Bfres.ResFile"/>.
    /// </summary>
    public class ResFileLoader : BinaryDataReader
    {
        // ---- FIELDS -------------------------------------------------------------------------------------------------

        private IDictionary<uint, IResData> _dataMap;

        internal List<long> RelocatedPointers = new List<long>();

        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ResFileLoader"/> class loading data into the given
        /// <paramref name="resFile"/> from the specified <paramref name="stream"/> which is optionally left open.
        /// </summary>
        /// <param name="resFile">The <see cref="Bfres.ResFile"/> instance to load data into.</param>
        /// <param name="stream">The <see cref="Stream"/> to read data from.</param>
        /// <param name="leaveOpen"><c>true</c> to leave the stream open after reading, otherwise <c>false</c>.</param>
        internal ResFileLoader(ResFile resFile, Stream stream, bool leaveOpen = false)
            : base(stream, Encoding.ASCII, leaveOpen)
        {
            ByteOrder = ByteOrder.LittleEndian;
            ResFile = resFile;
            _dataMap = new Dictionary<uint, IResData>();
        }

        internal ResFileLoader(Material material, Stream stream, bool leaveOpen = false)
             : base(stream, Encoding.ASCII, leaveOpen)
        {
            ResFile = new ResFile();

            ByteOrder = ByteOrder.LittleEndian;
            Material = material;
            _dataMap = new Dictionary<uint, IResData>();
        }

        internal ResFileLoader(Shape shape, Stream stream, bool leaveOpen = false)
              : base(stream, Encoding.ASCII, leaveOpen)
        {
            ResFile = new ResFile();
            ByteOrder = ByteOrder.LittleEndian;
            Shape = shape;
            _dataMap = new Dictionary<uint, IResData>();
        }

        internal ResFileLoader(Model model, ResFile resFile, Stream stream, bool leaveOpen = false)
              : base(stream, Encoding.ASCII, leaveOpen)
        {
            ByteOrder = ByteOrder.LittleEndian;
            Model = model;
            ResFile = resFile;
            _dataMap = new Dictionary<uint, IResData>();
        }

        internal ResFileLoader(SkeletalAnim skeletalAnim, Stream stream, bool leaveOpen = false)
              : base(stream, Encoding.ASCII, leaveOpen)
        {
            ResFile = new ResFile();
            ByteOrder = ByteOrder.LittleEndian;
            SkeletalAnim = skeletalAnim;
            _dataMap = new Dictionary<uint, IResData>();
        }

        internal ResFileLoader(MaterialAnim materialAnim, Stream stream, bool leaveOpen = false)
              : base(stream, Encoding.ASCII, leaveOpen)
        {
            ResFile = new ResFile();
            ByteOrder = ByteOrder.LittleEndian;
            MaterialAnim = materialAnim;
            _dataMap = new Dictionary<uint, IResData>();
        }

        internal ResFileLoader(VisibilityAnim visibilityAnim, Stream stream, bool leaveOpen = false)
              : base(stream, Encoding.ASCII, leaveOpen)
        {
            ResFile = new ResFile();
            ByteOrder = ByteOrder.LittleEndian;
            VisibilityAnim = visibilityAnim;
            _dataMap = new Dictionary<uint, IResData>();
        }

        internal ResFileLoader(ShapeAnim shapeAnim, Stream stream, bool leaveOpen = false)
              : base(stream, Encoding.ASCII, leaveOpen)
        {
            ResFile = new ResFile();
            ByteOrder = ByteOrder.LittleEndian;
            ShapeAnim = shapeAnim;
            _dataMap = new Dictionary<uint, IResData>();
        }

        internal ResFileLoader(SceneAnim sceneAnim, Stream stream, bool leaveOpen = false)
              : base(stream, Encoding.ASCII, leaveOpen)
        {
            ResFile = new ResFile();
            ByteOrder = ByteOrder.LittleEndian;
            SceneAnim = sceneAnim;
            _dataMap = new Dictionary<uint, IResData>();
        }

        internal ResFileLoader(Skeleton skeleton, Stream stream, bool leaveOpen = false)
              : base(stream, Encoding.ASCII, leaveOpen)
        {
            ResFile = new ResFile();
            ByteOrder = ByteOrder.LittleEndian;
            Skeleton = skeleton;
            _dataMap = new Dictionary<uint, IResData>();
        }

        internal ResFileLoader(Bone bone, Stream stream, bool leaveOpen = false)
              : base(stream, Encoding.ASCII, leaveOpen)
        {
            ResFile = new ResFile();
            ByteOrder = ByteOrder.LittleEndian;
            Bone = bone;
            _dataMap = new Dictionary<uint, IResData>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResFileLoader"/> class from the file with the given
        /// <paramref name="fileName"/>.
        /// </summary>
        /// <param name="resFile">The <see cref="Bfres.ResFile"/> instance to load data into.</param>
        /// <param name="fileName">The name of the file to load the data from.</param>
        internal ResFileLoader(ResFile resFile, string fileName)
            : this(resFile, new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
        }

        internal ResFileLoader(Model model, ResFile resFile, string fileName)
             : this(model, resFile, new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
        }

        internal ResFileLoader(SkeletalAnim skeletonAnim, string fileName)
             : this(skeletonAnim, new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
        }

        internal ResFileLoader(MaterialAnim materialAnim, string fileName)
             : this(materialAnim, new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
        }

        internal ResFileLoader(VisibilityAnim visibilityAnim, string fileName)
     : this(visibilityAnim, new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
        }

        internal ResFileLoader(SceneAnim sceneAnim, string fileName)
             : this(sceneAnim, new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
        }

        internal ResFileLoader(ShapeAnim shapeAnim, string fileName)
             : this(shapeAnim, new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
        }

        internal ResFileLoader(Skeleton skeleton, string fileName)
             : this(skeleton, new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
        }

        internal ResFileLoader(Bone bone, string fileName)
             : this(bone, new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
        }

        internal ResFileLoader(Material material, string fileName)
             : this(material, new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
        }

        internal ResFileLoader(Shape shape, string fileName)
             : this(shape, new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
        }



        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the loaded <see cref="Bfres.ResFile"/> instance.
        /// </summary>
        internal ResFile ResFile { get; }

        /// <summary>
        /// Gets the loaded <see cref="Bfres.Model"/> instance.
        /// </summary>
        internal Model Model { get; }

        /// <summary>
        /// Gets the saved <see cref="Bfres.MaterialAnim"/> instance.
        /// </summary>
        internal MaterialAnim MaterialAnim { get; }

        /// <summary>
        /// Gets the saved <see cref="Bfres.SceneAnim"/> instance.
        /// </summary>
        internal SceneAnim SceneAnim { get; }

        /// <summary>
        /// Gets the saved <see cref="Bfres.ShapeAnim"/> instance.
        /// </summary>
        internal ShapeAnim ShapeAnim { get; }

        /// <summary>
        /// Gets the saved <see cref="Bfres.VisibilityAnim"/> instance.
        /// </summary>
        internal VisibilityAnim VisibilityAnim { get; }

        /// <summary>
        /// Gets the saved <see cref="Bfres.SkeletalAnim"/> instance.
        /// </summary>
        internal SkeletalAnim SkeletalAnim { get; }

        /// <summary>
        /// Gets the loaded <see cref="Bfres.Material"/> instance.
        /// </summary>
        internal Material Material { get; }

        /// <summary>
        /// Gets the saved <see cref="Bfres.Skeleton"/> instance.
        /// </summary>
        internal Skeleton Skeleton { get; }

        /// <summary>
        /// Gets the saved <see cref="Bfres.Bone"/> instance.
        /// </summary>
        internal Bone Bone { get; }

        /// <summary>
        /// Gets the loaded <see cref="Bfres.Shape"/> instance.
        /// </summary>
        internal Shape Shape { get; }

        // ---- METHODS (INTERNAL) -------------------------------------------------------------------------------------

        private void ReadImportedFileHeader()
        {
            ByteOrder = ByteOrder.BigEndian;

            Seek(8, SeekOrigin.Begin);
            uint Version = ReadUInt32();

            if (Version != 0)
                ResFile.SetVersionInfo(Version);
            ByteOrder = ByteOrder.LittleEndian;

            Seek(0x30, SeekOrigin.Begin);
        }

        internal void ImportMaterials()
        {
            ReadImportedFileHeader();
            ((IResData)Material).Load(this);
        }
        internal void ImportSkeleton()
        {
            ReadImportedFileHeader();
            ((IResData)Skeleton).Load(this);
        }
        internal void ImportBone()
        {
            ReadImportedFileHeader();
            Seek(4, SeekOrigin.Current);
            ((IResData)Bone).Load(this);
        }
        internal void ImportSkeletonAnimations()
        {
            ReadImportedFileHeader();
            ((IResData)SkeletalAnim).Load(this);
        }
        internal void ImportMaterialAnims()
        {
            ReadImportedFileHeader();
            ((IResData)MaterialAnim).Load(this);
        }
        internal void ImportBoneVisualAnimations()
        {
            ReadImportedFileHeader();
            ((IResData)VisibilityAnim).Load(this);
        }
        internal void ImportShapeAnimations()
        {
            ReadImportedFileHeader();
            ((IResData)ShapeAnim).Load(this);
        }
        internal void ImportSceneAnimations()
        {
            ReadImportedFileHeader();
            ((IResData)SceneAnim).Load(this);
        }
        internal void ImportModel()
        {
            ReadImportedFileHeader();

            long test = BufferInfo.BufferOffset;
            long BufferInfPos = ReadInt64();
            using (TemporarySeek(BufferInfPos, SeekOrigin.Begin))
            {
                Seek(8);
                BufferInfo.BufferOffset = ReadInt64();
            }
            ((IResData)Model).Load(this);

            BufferInfo.BufferOffset = test;
        }
        internal void ImportShapes(VertexBuffer vtx)
        {
            ReadImportedFileHeader();
            Seek(48, SeekOrigin.Begin);

            long test = BufferInfo.BufferOffset;

            long VtxPos = ReadInt64();
            long BufferInfPos = ReadInt64();
            using (TemporarySeek(BufferInfPos, SeekOrigin.Begin))
            {
                Seek(8);
                BufferInfo.BufferOffset = ReadInt64();

            }

            using (TemporarySeek(VtxPos, SeekOrigin.Begin))
            {
                ((IResData)vtx).Load(this);
            }

            Seek(64, SeekOrigin.Begin);
            ((IResData)Shape).Load(this);

            BufferInfo.BufferOffset = test;
        }

        /// <summary>
        /// Starts deserializing the data from the <see cref="ResFile"/> root.
        /// </summary>
        internal void Execute()
        {
            // Load the raw data into structures recursively.
            ((IResData)ResFile).Load(this);
        }
        
        /// <summary>
        /// Reads and returns an <see cref="IResData"/> instance of type <typeparamref name="T"/> from the following
        /// offset or returns <c>null</c> if the read offset is 0.
        /// </summary>
        /// <typeparam name="T">The type of the <see cref="IResData"/> to read.</typeparam>
        /// <returns>The <see cref="IResData"/> instance or <c>null</c>.</returns>
        [DebuggerStepThrough]
        internal T Load<T>(bool Relocated = false)
            where T : IResData, new()
        {
            long offset = ReadOffset();
            if (offset == 0) return default(T);

            if (Relocated == true)
            {
            }

            // Seek to the instance data and load it.
            using (TemporarySeek(offset, SeekOrigin.Begin))
            {
                return ReadResData<T>();
            }
        }

        internal T Load<T>(uint offset)
         where T : IResData, new()
        {
            if (offset == 0) return default(T);

            // Seek to the instance data and load it.
            using (TemporarySeek(offset, SeekOrigin.Begin))
            {
                return ReadResData<T>();
            }
        }

        /// <summary>
        /// Reads and returns an instance of arbitrary type <typeparamref name="T"/> from the following offset with the
        /// given <paramref name="callback"/> or returns <c>null</c> if the read offset is 0.
        /// </summary>
        /// <typeparam name="T">The type of the data to read.</typeparam>
        /// <param name="callback">The callback to read the instance data with.</param>
        /// <param name="offset">The optional offset to use instead of reading a following one.</param>
        /// <returns>The data instance or <c>null</c>.</returns>
        /// <remarks>Offset required for ExtFile header (offset specified before size).</remarks>
        [DebuggerStepThrough]
        internal T LoadCustom<T>(Func<T> callback, long? offset = null)
        {
            offset = offset ?? ReadOffset();
            if (offset == 0) return default(T);

            using (TemporarySeek(offset.Value, SeekOrigin.Begin))
            {
                return callback.Invoke();
            }
        }

        /// <summary>
        /// Reads and returns an <see cref="ResDict{T}"/> instance with elements of type <typeparamref name="T"/> from
        /// the following offset or returns an empty instance if the read offset is 0.
        /// </summary>
        /// <typeparam name="T">The type of the <see cref="IRes Data"/> elements.</typeparam>
        /// <returns>The <see cref="ResDict{T}"/> instance.</returns>
        [DebuggerStepThrough]
        internal ResDict LoadDict()
        {
            long offset = ReadOffset();
            if (offset == 0) return new ResDict();

            using (TemporarySeek(offset, SeekOrigin.Begin))
            {
                ResDict dict = new ResDict();
                ((IResData)dict).Load(this);
                return dict;
            }
        }

        /// <summary>
        /// Reads and returns an <see cref="IList{T}"/> instance with <paramref name="count"/> elements of type
        /// <typeparamref name="T"/> from the following offset or returns <c>null</c> if the read offset is 0.
        /// </summary>
        /// <typeparam name="T">The type of the <see cref="IResData"/> elements.</typeparam>
        /// <param name="count">The number of elements to expect for the list.</param>
        /// <param name="offset">The optional offset to use instead of reading a following one.</param>
        /// <returns>The <see cref="IList{T}"/> instance or <c>null</c>.</returns>
        /// <remarks>Offset required for FMDL FVTX lists (offset specified before count).</remarks>
        [DebuggerStepThrough]
        internal IList<T> LoadList<T>(int count, long? offset = null)
            where T : IResData, new()
        {
            List<T> list = new List<T>(count);
            offset = offset ?? ReadOffset();
            if (offset == 0 || count == 0) return list;

            // Seek to the list start and read it.
            using (TemporarySeek(offset.Value, SeekOrigin.Begin))
            {
                for (; count > 0; count--)
                {
                    list.Add(ReadResData<T>());
                }
                return list;
            }
        }

        /// <summary>
        /// Reads and returns a <see cref="String"/> instance from the following offset or <c>null</c> if the read
        /// offset is 0.
        /// </summary>
        /// <param name="encoding">The optional encoding of the text.</param>
        /// <returns>The read text.</returns>
        [DebuggerStepThrough]
        internal string LoadString(Encoding encoding = null)
        {
            long offset = ReadOffset();
            if (offset == 0) return null;

            if (StringCache.Strings.ContainsKey(offset))
                return StringCache.Strings[offset];

            encoding = encoding ?? Encoding;
            using (TemporarySeek(offset, SeekOrigin.Begin))
            {
                short size = ReadInt16();
                return ReadString(BinaryStringFormat.ZeroTerminated, encoding);
            }
        }
        internal void LoadHeaderBlock()
        {
            uint offset = ReadUInt32();
            long size   = ReadInt64();

            SetHeaderBlock(offset, size);
        }
        internal byte[] SetHeaderBlock(uint Offset, long Size)
        {
            using (TemporarySeek(Offset, SeekOrigin.Begin))
            {
                return ReadBytes((int)Size);
            }
        }

        /// <summary>
        /// Reads and returns <paramref name="count"/> <see cref="String"/> instances from the following offsets.
        /// </summary>
        /// <param name="count">The number of instances to read.</param>
        /// <param name="encoding">The optional encoding of the texts.</param>
        /// <returns>The read texts.</returns>
        [DebuggerStepThrough]
        internal IList<string> LoadStrings(int count, Encoding encoding = null)
        {
            long[] offsets = ReadOffsets(count);

            encoding = encoding ?? Encoding;
            string[] names = new string[offsets.Length];
            using (TemporarySeek())
            {
                for (int i = 0; i < offsets.Length; i++)
                {
                    long offset = offsets[i];
                    if (offset == 0) continue;

                    if (StringCache.Strings.ContainsKey(offset))
                    {
                        names[i] = StringCache.Strings[offset];
                    }
                    else
                    {
                        Position = offset;
                        short size = ReadInt16();
                        names[i] = ReadString(BinaryStringFormat.ZeroTerminated, encoding);
                    }
                }
                return names;
            }
        }
        
        /// <summary>
        /// Reads a BFRES signature consisting of 4 ASCII characters encoded as an <see cref="UInt32"/> and checks for
        /// validity.
        /// </summary>
        /// <param name="validSignature">A valid signature.</param>
        internal void CheckSignature(string validSignature)
        {
            // Read the actual signature and compare it.
            string signature = ReadString(sizeof(uint), Encoding.ASCII);
            if (signature != validSignature)
            {
                throw new ResException($"Invalid signature, expected '{validSignature}' but got '{signature}'.");
            }
        }

        /// <summary>
        /// Reads a BFRES offset which is the absolute address.
        /// </summary>
        /// <returns>The absolute address of the offset.</returns>
        internal long ReadOffset(bool Relocated = false)
        {
            long pos = this.Position;
            long offset = ReadInt64();
            //if (offset != 0 && pos > 100 && !RelocatedPointers.Contains(pos))
               // throw new Exception();

            return offset == 0 ? 0 : offset;
        }

        /// <summary>
        /// Reads BFRES offsets which is the absolute addresses.+
        /// </summary>
        /// <param name="count">The number of offsets to read.</param>
        /// <returns>The absolute addresses of the offsets.</returns>
        internal long[] ReadOffsets(int count)
        {
            long[] values = new long[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = ReadOffset();
            }
            return values;
        }

        // ---- METHODS (PRIVATE) --------------------------------------------------------------------------------------
        
        [DebuggerStepThrough]
        private T ReadResData<T>()
            where T : IResData, new()
        {
            uint offset = (uint)Position;

            // Same data can be referenced multiple times. Load it in any case to move in the stream, needed for lists.
            T instance = new T();
            instance.Load(this);

            // If possible, return an instance already representing the data.
            if (_dataMap.TryGetValue(offset, out IResData existingInstance))
            {
                return (T)existingInstance;
            }
            else
            {
                _dataMap.Add(offset, instance);
                return instance;
            }
        }
    }
}
