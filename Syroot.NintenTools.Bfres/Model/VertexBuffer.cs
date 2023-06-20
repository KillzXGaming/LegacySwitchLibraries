using System.Collections.Generic;
using System.IO;
using Syroot.NintenTools.NSW.Bfres.Core;
using Syroot.NintenTools.NSW.Bfres.Helpers;

namespace Syroot.NintenTools.NSW.Bfres
{
    /// <summary>
    /// Represents a data buffer holding vertices for a <see cref="Model"/> subfile.
    /// </summary>
    public class VertexBuffer : IResData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Mesh"/> class.
        /// </summary>
        public VertexBuffer()
        {
            VertexSkinCount = 0;
            MemoryPool = new MemoryPool();
            StrideArray = new List<VertexBufferStride>();
            VertexBufferSizeArray = new List<VertexBufferSize>();
            AttributeDict = new ResDict();
            Attributes = new List<VertexAttrib>();
            Buffers = new List<buffData>();
        }

        public void CreateEmptyVertexBuffer()
        {
            var positions = new Maths.Vector4F[4];
            positions[0] = new Maths.Vector4F(-1, 0, 1, 0);
            positions[1] = new Maths.Vector4F(-1, 0, 1, 0);
            positions[2] = new Maths.Vector4F(-1, 0, 1, 0);
            positions[3] = new Maths.Vector4F(-1, 0, 1, 0);
            var normals = new Maths.Vector4F[4];
            normals[0] = new Maths.Vector4F(0, -1, 0, 0);
            normals[1] = new Maths.Vector4F(0, -1, 0, 0);
            normals[2] = new Maths.Vector4F(0, -1, 0, 0);
            normals[3] = new Maths.Vector4F(0, -1, 0, 0);

            VertexBufferHelper helpernx = new VertexBufferHelper(new VertexBuffer(), Syroot.BinaryData.ByteOrder.LittleEndian);
            List<VertexBufferHelperAttrib> atrib = new List<VertexBufferHelperAttrib>();

            VertexBufferHelperAttrib position = new VertexBufferHelperAttrib();
            position.Name = "_p0";
            position.Data = positions;
            position.Format = GFX.AttribFormat.Format_32_32_32_Single;
            atrib.Add(position);

            VertexBufferHelperAttrib normal = new VertexBufferHelperAttrib();
            normal.Name = "_n0";
            normal.Data = normals;
            normal.Format = GFX.AttribFormat.Format_10_10_10_2_SNorm;
            atrib.Add(normal);

            helpernx.Attributes = atrib;
            var VertexBuffer = helpernx.ToVertexBuffer();
            VertexSkinCount = VertexBuffer.VertexSkinCount;
            AttributeDict = VertexBuffer.AttributeDict;
            Attributes = VertexBuffer.Attributes;
            Buffers = VertexBuffer.Buffers;
            VertexBufferSizeArray = VertexBuffer.VertexBufferSizeArray;
            StrideArray = VertexBuffer.StrideArray;
        }

        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const string _signature = "FVTX";
        
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the number of bones influencing the vertices stored in this buffer. 0 influences equal
        /// rigidbodies (no skinning), 1 equal rigid skinning and 2 or more smooth skinning.
        /// </summary>
        public uint VertexSkinCount { get; set; }

        /// <summary>
        /// Gets the number of vertices stored by the <see cref="Buffers"/>. It is calculated from the size of the first
        /// <see cref="Buffer"/> in bytes divided by the <see cref="StrideArray"/>.
        /// </summary>
        public uint VertexCount;

        public uint GetTotalBufferSize()
        {
            uint TotalSize = 0;
            foreach (buffData buff in Buffers)
            {
                TotalSize += (uint)buff.buffSize;
            }
            return TotalSize;
        }

        public uint SetVertexBufferArrayOffset(ResFileSaver saver)
        {
            //Add all previous buffers until it reaches current one
            //This adds all face buffers (those goes first) then the vertex ones

            uint Pos = (uint)BufferInfo.BufferOffset;

            uint TotalSize = Pos;

            if (saver.Shape != null)
            {
                foreach (Mesh msh in saver.Shape.Meshes)
                {
                    if (TotalSize % 8 != 0) TotalSize = TotalSize + (8 - (TotalSize % 8));
                    TotalSize += (uint)msh.Data.Length;
                }
                return TotalSize - Pos;
            }

            foreach (Model fmdl in saver.ResFile.Models)
            {
                foreach (Shape shp in fmdl.Shapes)
                {
                    foreach (Mesh msh in shp.Meshes)
                    {
                        if (TotalSize % 8 != 0) TotalSize = TotalSize + (8 - (TotalSize % 8));
                        TotalSize += (uint)msh.Data.Length;
                    }
                }
            }
            foreach (Model fmdl in saver.ResFile.Models)
            {
                foreach (VertexBuffer vtx in fmdl.VertexBuffers)
                {
                    foreach (buffData buff in vtx.Buffers)
                    {
                        if (TotalSize % 8 != 0) TotalSize = TotalSize + (8 - (TotalSize % 8));
                        if (vtx == this)
                            return TotalSize - Pos;

                        TotalSize += (uint)buff.Data.Length;
                    }
                }
            }

            TotalSize = TotalSize - Pos;

            return TotalSize;
        }

        private void SaveBuffers()
        {
            //Set the size and stride for buffers. 
            VertexBufferSizeArray.Clear();
            StrideArray.Clear();
            for (int buff = 0; buff < Buffers.Count; buff++)
            {
                VertexBufferSize buffSize = new VertexBufferSize();
                buffSize.Size = (uint)Buffers[buff].buffSize;
                VertexBufferSizeArray.Add(buffSize);

                VertexBufferStride buffStride = new VertexBufferStride();
                buffStride.Stride = (uint)Buffers[buff].Stride;
                StrideArray.Add(buffStride);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="MemoryPool"/> instance storing buffer data at runtime
        /// </summary>
        public MemoryPool MemoryPool { get; set; }

        /// <summary>
        /// The size of a full vertex in bytes.
        /// </summary>
        public IList<VertexBufferStride> StrideArray { get; set; }

        public IList<VertexBufferSize> VertexBufferSizeArray { get; set; }

        /// <summary>
        /// Gets or sets the dictionary of <see cref="VertexAttrib"/> instances describing how to interprete data in the
        /// <see cref="Buffers"/>.
        /// </summary>
        public ResDict AttributeDict { get; set; }

        public IList<VertexAttrib> Attributes { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="buffData"/> instances storing raw unformatted vertex data.
        /// </summary>
        public IList<buffData> Buffers { get; set; }

        public class buffData
        {
            public int buffSize;
            public int Stride;
            public int DataOffset;
            public byte[] Data;

        }

        private uint GPUBufferAlignent = 8;

        // ---- METHODS ------------------------------------------------------------------------------------------------

        private uint UnknownFlag;

        void IResData.Load(ResFileLoader loader)
        {
            loader.CheckSignature(_signature);
            if (loader.ResFile.VersionMajor2 >= 9)
                UnknownFlag = loader.ReadUInt32(); 
            else
                loader.LoadHeaderBlock();

            long ofsVertexAttribList    = loader.ReadOffset(); // Only load dict.
            AttributeDict               = loader.LoadDict();
            MemoryPool                  = loader.Load<MemoryPool>();
            long unk                    = loader.ReadOffset();
            if (loader.ResFile.VersionMajor2 > 2 || loader.ResFile.VersionMajor > 0)
                loader.ReadOffset();// unk2
            long VertexBufferSizeOffset = loader.ReadOffset();
            long VertexStrideSizeOffset = loader.ReadOffset();
            long padding                = loader.ReadInt64();
            int BufferOffset            = loader.ReadInt32();
            byte numVertexAttrib        = loader.ReadByte();
            byte numBuffer              = loader.ReadByte();
            ushort Idx                  = loader.ReadUInt16();
            VertexCount                 = loader.ReadUInt32();
            VertexSkinCount = loader.ReadUInt16();
            GPUBufferAlignent = loader.ReadUInt16();

            //So buffers work like this
            //They grab the index buffer offset from memory info section
            //This goes to a section in the memory pool which stores all the buffer data, including faces aswell
            //To obtain a list of all the buffer data, it would be by the index buffer offset + BufferOffset

            StrideArray           = loader.LoadList<VertexBufferStride>(numBuffer, VertexStrideSizeOffset);
            VertexBufferSizeArray = loader.LoadList<VertexBufferSize>(numBuffer, VertexBufferSizeOffset);
            Attributes            = loader.LoadList<VertexAttrib>(numVertexAttrib, ofsVertexAttribList);

            //Extreemly hacky atm. Wil redo
            Buffers = new List<buffData>();
            using (loader.TemporarySeek(BufferInfo.BufferOffset + BufferOffset, SeekOrigin.Begin))
            {
                for (int buff = 0; buff < numBuffer; buff++)
                {
                    buffData buffer = new buffData();
                    buffer.buffSize = (int)VertexBufferSizeArray[buff].Size;
                    buffer.Stride = (int)StrideArray[buff].Stride;

                    loader.Align(8);
                    buffer.Data = loader.ReadBytes(buffer.buffSize);
                    Buffers.Add(buffer);
                }
            }
        }
        internal long AttributeOffset;
        internal long AttributeDictOffset;
        internal long UnkBufferOffset;
        internal long UnkBuffer2Offset;
        internal long BufferSizeArrayOffset;
        internal long StideArrayOffset;
        internal long Position;

        void IResData.Save(ResFileSaver saver)
        {
            Position = saver.Position;
            SaveBuffers();
            saver.WriteSignature(_signature);
            if (saver.ResFile.VersionMajor2 >= 9)
                saver.Write(UnknownFlag);
            else
                saver.Seek(12);
            saver.SaveRelocateEntryToSection(saver.Position, 2, 1, 0, ResFileSaver.Section1, "FVTX"); //      <------------ Entry Set
            AttributeOffset = saver.SaveOffset();
            AttributeDictOffset = saver.SaveOffset();
            if (MemoryPool != null)
                saver.SaveRelocateEntryToSection(saver.Position, 1, 1, 0, ResFileSaver.Section4, "Memory pool"); //      <------------ Entry Set
            saver.SaveMemoryPoolPointer();

            saver.SaveRelocateEntryToSection(saver.Position, 4, 1, 0, ResFileSaver.Section1, "Vertex buffer info"); //      <------------ Entry Set   
            UnkBufferOffset = saver.SaveOffset();
            UnkBuffer2Offset = saver.SaveOffset();
            BufferSizeArrayOffset = saver.SaveOffset();
            StideArrayOffset = saver.SaveOffset();
            saver.Write(0L); //padding
            saver.Write(SetVertexBufferArrayOffset(saver)); //Buffer Offset
            saver.Write((byte)Attributes.Count);
            saver.Write((byte)Buffers.Count);
            saver.Write((ushort)saver.CurrentIndex);
            saver.Write(VertexCount);
            saver.Write((ushort)VertexSkinCount);
            saver.Write((ushort)GPUBufferAlignent);
        }
    }
}