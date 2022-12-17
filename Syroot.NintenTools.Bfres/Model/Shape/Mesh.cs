using System;
using System.Collections.Generic;
using System.IO;
using Syroot.BinaryData;
using Syroot.NintenTools.NSW.Bfres.Core;
using Syroot.NintenTools.NSW.Bfres.GFX;

namespace Syroot.NintenTools.NSW.Bfres
{
    /// <summary>
    /// Represents the surface net of a <see cref="Shape"/> section, storing information on which
    /// index <see cref="Buffer"/> to use for referencing vertices of the shape, mostly used for different levels of
    /// detail (LoD) models.
    /// </summary>
    public class Mesh : IResData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Mesh"/> class.
        /// </summary>
        public Mesh()
        {
            MemoryPool = new MemoryPool();
            bufferSize = new BufferSize();
            PrimitiveType = PrimitiveType.Triangles;
            IndexFormat = IndexFormat.UInt16;
            SubMeshes = new List<SubMesh>();
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the <see cref="MemoryPool"/> instance storing buffer data at runtime
        /// </summary>
        public MemoryPool MemoryPool { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="BufferSize"/> instance storing buffer size
        /// </summary>
        public BufferSize bufferSize { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="PrimitiveType"/> which determines how indices are used to form polygons.
        /// </summary>
        public PrimitiveType PrimitiveType { get; set; }

        /// <summary>
        /// Gets the <see cref="IndexFormat"/> determining the data type of the indices in the
        /// <see cref="IndexBuffer"/>.
        /// </summary>
        public IndexFormat IndexFormat { get; private set; }

        /// <summary>
        /// Gets the number of indices stored in the <see cref="IndexBuffer"/>.
        /// </summary>
        public uint IndexCount
        {
            get
            {
                // Sum indices in all bufferings together, even if only first is mostly used.
                int elementCount = 0;
                int formatSize = FormatSize;

                int bufferingSize = Data.Length;
                if (bufferingSize % formatSize != 0)
                {
                    throw new InvalidDataException($"Cannot form complete indices.");
                }
                elementCount += bufferingSize / formatSize;

                return (uint)elementCount;
            }
        }

        public byte[] Data;

        /// <summary>
        /// Gets or sets the list of <see cref="SubMesh"/> instances which split up a mesh into parts which can be
        /// hidden if they are not visible to optimize rendering performance.
        /// </summary>
        public IList<SubMesh> SubMeshes { get; set; }

        /// <summary>
        /// Gets or sets the offset to the first vertex element of a <see cref="VertexBuffer"/> to reference by indices.
        /// </summary>
        public uint FirstVertex { get; set; }

        public long DataOffset;

        internal int FormatSize
        {
            get
            {
                switch (IndexFormat)
                {
                    case IndexFormat.UnsignedByte:
                        return sizeof(byte);
                    case IndexFormat.UInt16:
                        return sizeof(ushort);
                    case IndexFormat.UInt32:
                        return sizeof(uint);
                    default:
                        throw new ArgumentException($"Invalid {nameof(IndexFormat)} {IndexFormat}.", nameof(IndexFormat));
                }
            }
        }

        internal ByteOrder FormatByteOrder
        {
            get
            {
                switch (IndexFormat)
                {
                    case IndexFormat.UInt16:
                    case IndexFormat.UInt32:
                        return ByteOrder.LittleEndian;
                    default:
                        throw new ArgumentException($"Invalid {nameof(IndexFormat)} {IndexFormat}.", nameof(IndexFormat));
                }
            }
        }

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Returns the indices stored in the <see cref="IndexBuffer"/> as <see cref="UInt32"/> instances.
        /// </summary>
        /// <returns>The indices stored in the <see cref="IndexBuffer"/>.</returns>
        public IEnumerable<uint> GetIndices()
        {
            using (BinaryDataReader reader = new BinaryDataReader(new MemoryStream(Data)))
            {
                reader.ByteOrder = ByteOrder.LittleEndian;

                // Read and return the elements.
                uint elementCount = IndexCount;
                switch (IndexFormat)
                {
                    case IndexFormat.UInt16:
                        for (; elementCount > 0; elementCount--)
                        {
                            yield return reader.ReadUInt16();
                        }
                        break;
                    default:
                        for (; elementCount > 0; elementCount--)
                        {
                            yield return reader.ReadUInt32();
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Stores the given <paramref name="indices"/> in the <see cref="IndexBuffer"/> in the provided
        /// <paramref name="format"/>, or the current <see cref="IndexFormat"/> if none was specified.
        /// </summary>
        /// <param name="indices">The indices to store in the <see cref="IndexBuffer"/>.</param>
        /// <param name="format">The <see cref="IndexFormat"/> to use or <c>null</c> to use the current format.
        /// </param>
        public void SetIndices(IList<uint> indices, IndexFormat? format = null)
        {
            IndexFormat = format ?? IndexFormat;
            Data = new byte[indices.Count * FormatSize];
            using (BinaryDataWriter writer = new BinaryDataWriter(new MemoryStream(Data, true)))
            {
                writer.ByteOrder = FormatByteOrder;

                // Write the elements.
                switch (IndexFormat)
                {
                    case IndexFormat.UInt16:
                        foreach (uint index in indices)
                        {
                            writer.Write((ushort)index);
                        }
                        break;
                    default:
                        writer.Write(indices);
                        break;
                }
            }
        }

        public uint SetFaceBufferOffset(ResFileSaver saver)
        {
            //Add all previous buffers until it reaches current one
            uint TotalSize = 0;

            if (saver.Shape != null)
            {
                foreach (Mesh msh in saver.Shape.Meshes)
                {
                    if (msh == this)
                        return TotalSize;

                    TotalSize += (uint)msh.Data.Length;
                    if (TotalSize % 8 != 0) TotalSize = TotalSize + (8 - (TotalSize % 8));
                }
                return TotalSize;
            }

            foreach (Model fmdl in saver.ResFile.Models)
            {
                foreach (Shape shp in fmdl.Shapes)
                {
                    foreach (Mesh msh in shp.Meshes)
                    {
                        if (msh == this)
                            return TotalSize;

                        TotalSize += (uint)msh.Data.Length;
                        if (TotalSize % 8 != 0) TotalSize = TotalSize + (8 - (TotalSize % 8));
                    }
                }
            }
            return TotalSize;
        }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            long SubMeshArrayOffset = loader.ReadOffset();
            MemoryPool = loader.Load<MemoryPool>();
            long Buffer = loader.ReadOffset();
            bufferSize = loader.Load<BufferSize>();
            uint FaceBufferOffset = loader.ReadUInt32();
            PrimitiveType = loader.ReadEnum<PrimitiveType>(true);
            IndexFormat = loader.ReadEnum<IndexFormat>(true);
            uint indexCount = loader.ReadUInt32();
            FirstVertex = loader.ReadUInt32();
            ushort numSubMesh = loader.ReadUInt16();
            ushort padding = loader.ReadUInt16();
            SubMeshes = loader.LoadList<SubMesh>(numSubMesh, SubMeshArrayOffset);

            DataOffset = (uint)BufferInfo.BufferOffset + FaceBufferOffset;

            //Load buffer data from mem block
            Data = loader.LoadCustom(() => loader.ReadBytes((int)bufferSize.Size), DataOffset);


        }

        internal long PosSubMeshesOffset;
        internal long PosBufferUnkOffset;
        internal long PosBufferSizeOffset;

        void IResData.Save(ResFileSaver saver)
        {
            bufferSize = new BufferSize();
            bufferSize.Size = (uint)Data.Length;

            saver.SaveRelocateEntryToSection(saver.Position, 1, 1, 0, ResFileSaver.Section1, "Mesh"); //      <------------ Entry Set
            PosSubMeshesOffset = saver.SaveOffset();
            if (MemoryPool != null)
                saver.SaveRelocateEntryToSection(saver.Position, 1, 1, 0, ResFileSaver.Section4, "Memory pool"); //      <------------ Entry Set
            saver.SaveMemoryPoolPointer();
            saver.SaveRelocateEntryToSection(saver.Position, 2, 1, 0, ResFileSaver.Section1, "Mesh buffer info"); //      <------------ Entry Set
            PosBufferUnkOffset = saver.SaveOffset();
            PosBufferSizeOffset = saver.SaveOffset();
            saver.Write(SetFaceBufferOffset(saver)); //face buffer
            saver.Write(PrimitiveType, true);
            saver.Write(IndexFormat, true);
            saver.Write(IndexCount);
            saver.Write(FirstVertex);
            saver.Write((ushort)SubMeshes.Count);
            saver.Seek(2);
        }
    }
}