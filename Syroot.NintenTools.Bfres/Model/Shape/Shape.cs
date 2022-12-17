using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using Syroot.NintenTools.NSW.Bfres.Core;
using System.ComponentModel;

namespace Syroot.NintenTools.NSW.Bfres
{
    /// <summary>
    /// Represents an FSHP section in a <see cref="Model"/> subfile.
    /// </summary>
    [DebuggerDisplay(nameof(Shape) + " {" + nameof(Name) + "}")]
    public class Shape : IResData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Shape"/> class.
        /// </summary>
        public Shape()
        {
            Name = "";
            Flags = ShapeFlags.HasVertexBuffer;
            MaterialIndex = 0;
            BoneIndex = 0;
            VertexBufferIndex = 0;
            RadiusArray = new List<float>();
            VertexSkinCount = 0;
            TargetAttribCount = 0;
            Meshes = new List<Mesh>();
            SkinBoneIndices = new List<ushort>();
            KeyShapeDict = new ResDict();
            KeyShapes = new List<KeyShape>();
            SubMeshBoundings = new List<Bounding>();
            SubMeshBoundingNodes = new List<BoundingNode>();
            SubMeshBoundingIndices = new List<ushort>();
            VertexBuffer = new VertexBuffer();
        }

        public void CreateEmptyMesh()
        {
            uint[] faces = new uint[6];
            faces[0] = 0;
            faces[1] = 1;
            faces[2] = 2;
            faces[3] = 1;
            faces[4] = 3;
            faces[5] = 2;

            var mesh = new Mesh();
            mesh.SetIndices(faces, GFX.IndexFormat.UInt16);
            mesh.SubMeshes.Add(new SubMesh() { Count = 6 });
            Meshes = new List<Mesh>();
            Meshes.Add(mesh);

            RadiusArray.Add(1.0f);

            //Set boundings for mesh
            SubMeshBoundings = new List<Bounding>();
            SubMeshBoundings.Add(new Bounding()
            {
                Center = new Maths.Vector3F(0,0,0),
                Extent = new Maths.Vector3F(50, 50, 50)
            });
            SubMeshBoundings.Add(new Bounding() //One more bounding for sub mesh
            {
                Center = new Maths.Vector3F(0, 0, 0),
                Extent = new Maths.Vector3F(50, 50, 50)
            });
            SubMeshBoundingIndices = new List<ushort>();
            SubMeshBoundingIndices.Add(0);
            SubMeshBoundingNodes = new List<BoundingNode>();
            SubMeshBoundingNodes.Add(new BoundingNode()
            {
                LeftChildIndex = 0,
                NextSibling = 0,
                SubMeshIndex = 0,
                RightChildIndex = 0,
                Unknown = 0,
                SubMeshCount = 1,
            });
        }

        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const string _signature = "FSHP";

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="Material"/> class from the given <paramref name="stream"/> which
        /// is optionally left open.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to load the data from.</param>
        /// <param name="leaveOpen"><c>true</c> to leave the stream open after reading, otherwise <c>false</c>.</param>
        public void Import(Stream stream,VertexBuffer vtx, bool leaveOpen = false)
        {
            using (ResFileLoader loader = new ResFileLoader(this, stream, leaveOpen))
            {
                loader.ImportShapes(vtx);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResFile"/> class from the file with the given
        /// <paramref name="fileName"/>.
        /// </summary>
        /// <param name="fileName">The name of the file to load the data from.</param>
        public void Import(string fileName, VertexBuffer vtx)
        {
            using (ResFileLoader loader = new ResFileLoader(this, fileName))
            {
                loader.ImportShapes(vtx);
            }
        }


        /// <summary>
        /// Saves the contents in the given <paramref name="stream"/> and optionally leaves it open
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to save the contents into.</param>
        /// <param name="leaveOpen"><c>true</c> to leave the stream open after writing, otherwise <c>false</c>.</param>
        public void Export(Stream stream, ResFile ResFile, bool leaveOpen = false)
        {
            using (ResFileSaver saver = new ResFileSaver(this, ResFile, stream, leaveOpen))
            {
                saver.ExportShape();
            }
        }

        /// <summary>
        /// Saves the contents in the file with the given <paramref name="fileName"/>.
        /// </summary>
        /// <param name="fileName">The name of the file to save the contents into.</param>
        public void Export(string fileName, ResFile ResFile)
        {
            using (ResFileSaver saver = new ResFileSaver(this, ResFile, fileName))
            {
                saver.ExportShape();
            }
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the name with which the instance can be referenced uniquely in <see cref="ResDict{Shape}"/>
        /// instances.
        /// </summary>
        [Category("Polygon")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets flags determining which data is available for this instance.
        /// </summary>
        [Browsable(false)]
        public ShapeFlags Flags { get; set; }

        [Category("Flags")]
        public bool HasVertexBuffer
        {
            get { return Flags.HasFlag(ShapeFlags.HasVertexBuffer); }
            set
            {
                if (value)
                    Flags |= ShapeFlags.HasVertexBuffer;
                else
                    Flags &= ShapeFlags.HasVertexBuffer;
            }
        }

        [Category("Flags")]
        public bool SubMeshBoundaryConsistent
        {
            get { return Flags.HasFlag(ShapeFlags.SubMeshBoundaryConsistent); }
            set
            {
                if (value)
                    Flags |= ShapeFlags.SubMeshBoundaryConsistent;
                else
                    Flags &= ShapeFlags.SubMeshBoundaryConsistent;
            }
        }

        /// <summary>
        /// Gets or sets the index of the material to apply to the shapes surface in the owning
        /// <see cref="Model.Materials"/> list.
        /// </summary>
        [Category("Polygon")]
        [DisplayName("Material Index")]
        public ushort MaterialIndex { get; set; }

        /// <summary>
        /// Gets or sets the index of the <see cref="Bone"/> to which this instance is directly attached to. The bone
        /// must be part of the skeleton referenced by the owning <see cref="Model.Skeleton"/> instance.
        /// </summary>
        [Category("Polygon")]
        [DisplayName("Bone Index")]
        public ushort BoneIndex { get; set; }

        /// <summary>
        /// Gets or sets the index of the <see cref="VertexBuffer"/> in the owning <see cref="Model.VertexBuffers"/>
        /// list.
        /// </summary>
        [Category("Polygon")]
        [DisplayName("Vertex Buffer Index")]
        public ushort VertexBufferIndex { get; set; }

        /// <summary>
        /// Gets or sets the bounding radius/radii spanning the shape for each LOD mesh
        /// </summary>
        [Category("Visibility")]
        [DisplayName("Bounding Radius")]
        public List<float> RadiusArray { get; set; }

        /// <summary>
        /// Gets or sets the number of bones influencing the vertices stored in this buffer. 0 influences equal
        /// rigidbodies (no skinning), 1 equal rigid skinning and 2 or more smooth skinning.
        /// </summary>
        [Category("Polygon")]
        [DisplayName("Max Vertex Skin Influence")]
        public byte VertexSkinCount { get; set; }

        /// <summary>
        /// Gets or sets a value with unknown purpose.
        /// </summary>
        [Category("Morph Data")]
        [DisplayName("Target Attribiute Count")]
        public byte TargetAttribCount { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="Meshes"/> which are used to represent different level of details of the
        /// shape.
        /// </summary>
        [Category("Polygon")]
        [DisplayName("LOD Meshes")]
        public List<Mesh> Meshes { get; set; }

        [Category("Bones")]
        [DisplayName("Bone Indices")]
        public List<ushort> SkinBoneIndices { get; set; }

        [Category("Morph Data")]
        [DisplayName("Key Shapes")]
        public List<KeyShape> KeyShapes { get; set; }

        [Category("Visibility")]
        [DisplayName("Bounding Boxes")]
        public List<Bounding> SubMeshBoundings { get; set; }

        [Browsable(false)]
        public ResDict KeyShapeDict { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="BoundingNode"/> instances forming the bounding tree with which parts of a mesh
        /// are culled when not visible.
        /// </summary>
        public List<BoundingNode> SubMeshBoundingNodes { get; set; }

        public List<ushort> SubMeshBoundingIndices { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="VertexBuffer"/> instance storing the data which forms the shape's surface. Saved
        /// depending on <see cref="VertexBufferIndex"/>.
        /// </summary>
        internal VertexBuffer VertexBuffer { get; set; }

        [Browsable(false)]
        public long RadiusOffset;

        [Browsable(false)]
        public long BoundingBoxArrayOffset;

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            loader.CheckSignature(_signature);
            if (loader.ResFile.VersionMajor2 >= 9)
                Flags = loader.ReadEnum<ShapeFlags>(false);
            else
                loader.LoadHeaderBlock();

            Name = loader.LoadString();
            VertexBuffer = loader.Load<VertexBuffer>();
            long MeshArrayOffset = loader.ReadOffset();
            long SkinBoneIndexListOffset = loader.ReadOffset();
            long KeyShapesArrayOffset = loader.ReadOffset();
            KeyShapeDict = loader.LoadDict();
            BoundingBoxArrayOffset = loader.ReadOffset();
            if (loader.ResFile.VersionMajor2 > 2 || loader.ResFile.VersionMajor > 0)
            {
                RadiusOffset = loader.ReadOffset();
                long UserPointer = loader.ReadInt64();
            }
            else
            {
                RadiusOffset = 0;
                long UserPointer = loader.ReadInt64();

                RadiusArray = new List<float>();
                RadiusArray.Add(loader.ReadSingle());
            }
            if (loader.ResFile.VersionMajor2 < 9)
                Flags = loader.ReadEnum<ShapeFlags>(true);
            ushort idx = loader.ReadUInt16();
            MaterialIndex = loader.ReadUInt16();
            BoneIndex = loader.ReadUInt16();
            VertexBufferIndex = loader.ReadUInt16();
            ushort numSkinBoneIndex = loader.ReadUInt16();
            VertexSkinCount = loader.ReadByte();
            byte numMesh = loader.ReadByte();
            byte numKeys = loader.ReadByte();
            byte numTargetAttr = loader.ReadByte();
            if (loader.ResFile.VersionMajor2 >= 9)
                loader.Seek(2); //padding
            else
                loader.Seek(6); //padding

            Meshes = numMesh == 0 ? new List<Mesh>() : loader.LoadList<Mesh>(numMesh, MeshArrayOffset).ToList(); 
            SkinBoneIndices = numSkinBoneIndex == 0 ? new List<ushort>() : loader.LoadCustom(() => loader.ReadUInt16s(numSkinBoneIndex), SkinBoneIndexListOffset)?.ToList();
            KeyShapes = numKeys == 0 ? new List<KeyShape>() : loader.LoadList<KeyShape>(numKeys, KeyShapesArrayOffset).ToList(); 

            if (RadiusOffset != 0)
                RadiusArray = numMesh == 0 ? new List<float>() : loader.LoadCustom(() => loader.ReadSingles(numMesh), RadiusOffset).ToList();

            int boundingboxCount = 0;
            for (int msh = 0; msh < numMesh; msh++)
            {
                for (int sub = 0; sub < Meshes[msh].SubMeshes.Count; sub++)
                {
                    boundingboxCount++;
                }
                boundingboxCount++;
            }
            SubMeshBoundings = boundingboxCount == 0 ? new List<Bounding>() : loader.LoadCustom(() => loader.ReadBoundings(boundingboxCount), BoundingBoxArrayOffset)?.ToList(); ;
            SubMeshBoundingNodes = new List<BoundingNode>();
        }

        internal void WriteBoudnings(ResFileSaver saver)
        {
            saver.Write(SubMeshBoundings);
        }

        internal long PosMeshArrayOffset;
        internal long PosSkinBoneIndicesOffset;
        internal long PosKeyShapesOffset;
        internal long PosKeyShapeDictOffset;
        internal long PosSubMeshBoundingsOffset;
        internal long PosRadiusArrayOffset;

        void IResData.Save(ResFileSaver saver)
        {
            saver.WriteSignature(_signature);
            if (saver.ResFile.VersionMajor2 >= 9)
                 saver.Write(Flags, false);
            else
                saver.Seek(12);

            if (SkinBoneIndices == null)
                SkinBoneIndices = new List<ushort>();

            saver.SaveRelocateEntryToSection(saver.Position, 8, 1, 0, ResFileSaver.Section1, "FSHP"); //      <------------ Entry Set
            saver.SaveString(Name);
            saver.Write(VertexBuffer.Position);
            PosMeshArrayOffset = saver.SaveOffset();
            PosSkinBoneIndicesOffset = saver.SaveOffset();
            PosKeyShapesOffset = saver.SaveOffset();
            PosKeyShapeDictOffset = saver.SaveOffset();
            PosSubMeshBoundingsOffset = saver.SaveOffset();
            PosRadiusArrayOffset = saver.SaveOffset();
            saver.Write(0L); //user pointer
            if (saver.ResFile.VersionMajor2 < 9)
                saver.Write(Flags, true);
            saver.Write((ushort)saver.CurrentIndex);
            saver.Write(MaterialIndex);
            saver.Write(BoneIndex);
            saver.Write(VertexBufferIndex);
            saver.Write((ushort)SkinBoneIndices.Count);
            saver.Write(VertexSkinCount);
            saver.Write((byte)Meshes.Count);
            saver.Write((byte)KeyShapeDict.Count);
            saver.Write(TargetAttribCount);

            if (saver.ResFile.VersionMajor2 >= 9)
                saver.Seek(2);
            else
                saver.Seek(6);
        }
    }

    /// <summary>
    /// Represents flags determining which data is available for <see cref="Shape"/> instances.
    /// </summary>
    [Flags]
    public enum ShapeFlags : uint
    {
        /// <summary>
        /// The <see cref="Shape"/> instance references a <see cref="VertexBuffer"/>.
        /// </summary>
        HasVertexBuffer = 1 << 1,

        /// <summary>
        /// The boundings in all submeshes are consistent.
        /// </summary>
        SubMeshBoundaryConsistent = 1 << 2
    }
}