using System.IO;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using Syroot.NintenTools.NSW.Bfres.Core;
using System.Linq;

namespace Syroot.NintenTools.NSW.Bfres
{
    /// <summary>
    /// Represents an FMDL subfile in a <see cref="ResFile"/>, storing model vertex data, skeletons and used materials.
    /// </summary>
    [DebuggerDisplay(nameof(Model) + " {" + nameof(Name) + "}")]
    public class Model : IResData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Model"/> class.
        /// </summary>
        public Model()
        {
            Name = "";
            Path = "";
            Skeleton = new Skeleton();
            VertexBuffers = new List<VertexBuffer>();
            Shapes = new List<Shape>();
            Materials = new List<Material>();
            UserData = new List<UserData>();

            ShapeDict = new ResDict();
            MaterialDict = new ResDict();
            UserDataDict = new ResDict();
        }

        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        public const string _signature = "FMDL";

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="Material"/> class from the given <paramref name="stream"/> which
        /// is optionally left open.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to load the data from.</param>
        /// <param name="leaveOpen"><c>true</c> to leave the stream open after reading, otherwise <c>false</c>.</param>
        public void Import(Stream stream, ResFile resFile, bool leaveOpen = false)
        {
            using (ResFileLoader loader = new ResFileLoader(this, resFile, stream, leaveOpen))
            {
                loader.ImportModel();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResFile"/> class from the file with the given
        /// <paramref name="fileName"/>.
        /// </summary>
        /// <param name="fileName">The name of the file to load the data from.</param>
        public void Import(string fileName, ResFile resFile)
        {
            using (ResFileLoader loader = new ResFileLoader(this, resFile, fileName))
            {
                loader.ImportModel();
            }
        }

        /// <summary>
        /// Saves the contents in the given <paramref name="stream"/> and optionally leaves it open
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to save the contents into.</param>
        /// <param name="leaveOpen"><c>true</c> to leave the stream open after writing, otherwise <c>false</c>.</param>
        public void Export(Stream stream, ResFile resFile, bool leaveOpen = false)
        {
            using (ResFileSaver saver = new ResFileSaver(this, resFile, stream, leaveOpen))
            {
                saver.ExportModel();
            }
        }

        /// <summary>
        /// Saves the contents in the file with the given <paramref name="fileName"/>.
        /// </summary>
        /// <param name="fileName">The name of the file to save the contents into.</param>
        public void Export(string fileName, ResFile resFile)
        {
            using (ResFileSaver saver = new ResFileSaver(this, resFile, fileName))
            {
                saver.ExportModel();
            }
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the name with which the instance can be referenced uniquely in <see cref="ResDict{Model}"/>
        /// instances.
        /// </summary>
        [Browsable(true)]
        [Category("Model")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the path of the file which originally supplied the data of this instance.
        /// </summary>
        [Browsable(true)]
        [Category("Model")]
        public string Path { get; set; }

        /// <summary>
        /// Gets the <see cref="Skeleton"/> instance to deform the model with animations.
        /// </summary>
        [Browsable(false)]
        public Skeleton Skeleton { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="VertexBuffer"/> instances storing the vertex data used by the
        /// <see cref="Shapes"/>.
        /// </summary>
        [Browsable(false)]
        public IList<VertexBuffer> VertexBuffers { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Shape"/> instances forming the surface of the model.
        /// </summary>
        [Browsable(false)]
        public ResDict ShapeDict { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Shape"/> instances forming the surface of the model.
        /// </summary>
        [Browsable(false)]
        public IList<Shape> Shapes { get; set; }


        /// <summary>
        /// Gets or sets the <see cref="Material"/> names.
        /// </summary>
        [Browsable(false)]
        public ResDict MaterialDict { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Material"/> instance applied on the <see cref="Shapes"/> to color their surface.
        /// </summary>
        [Browsable(false)]
        public IList<Material> Materials { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<ShaderAssign> ShaderAssigns = new List<ShaderAssign>();

        /// <summary>
        /// Gets or sets customly attached <see cref="UserData"/> names.
        /// </summary>
        [Browsable(false)]
        public ResDict UserDataDict { get; set; }

        /// <summary>
        /// Gets or sets customly attached <see cref="UserData"/> instances.
        /// </summary>
        [Browsable(false)]
        public IList<UserData> UserData { get; set; }

        [Browsable(true)]
        [ReadOnly(true)]
        [Category("Model")]
        [DisplayName("Vertex Buffer Count")]
        public int VertexBufferCount
        {
            get
            {
                if (VertexBuffers != null)
                    return VertexBuffers.Count;
                else
                    return 0;
            }
        }

        [Browsable(true)]
        [ReadOnly(true)]
        [Category("Model")]
        [DisplayName("Shape Count")]
        public int ShapeCount
        {
            get
            {
                if (Shapes != null)
                    return Shapes.Count;
                else
                    return 0;
            }
        }

        [Browsable(true)]
        [ReadOnly(true)]
        [Category("Model")]
        [DisplayName("Material Count")]
        public int MaterialCount
        {
            get
            {
                if (Materials != null)
                    return Materials.Count;
                else
                    return 0;
            }
        }

        [Browsable(true)]
        [ReadOnly(true)]
        [Category("Model")]
        [DisplayName("User Data Count")]
        public int UserDataCount
        {
            get
            {
                if (UserData != null)
                    return UserData.Count;
                else
                    return 0;
            }
        }

        /// <summary>
        /// Gets the total number of vertices to process when drawing this model.
        /// </summary>
        /// <remarks>This excludes vertices which are not processed by any shader. However, the exact value does not
        /// seem to matter, so the total count of all vertices is taken to keep things trivial for now.</remarks>
        [Browsable(true)]
        [ReadOnly(true)]
        [Category("Model")]
        [DisplayName("Total Vertex Count")]
        public uint TotalVertexCount
        {
            get
            {
                uint count = 0;
                foreach (VertexBuffer vertexBuffer in VertexBuffers)
                {
                    count += vertexBuffer.VertexCount;
                }
                return count;
            }
        }

        public IList<MaterialParserV10.ShaderAssignV10> ShaderAssign = new List<MaterialParserV10.ShaderAssignV10>();

        internal class Ofs
        {
            internal long FMDLOffset = 0;

        }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        private uint UnknownFlag;

        private ushort ShaderAssignCount = 1;

        void IResData.Load(ResFileLoader loader)
        {
            loader.CheckSignature(_signature);

            if (loader.ResFile.VersionMajor2 >= 9)
                UnknownFlag = loader.ReadUInt32();
            else
                loader.LoadHeaderBlock();

            Name                      = loader.LoadString();
            Path                      = loader.LoadString();
            Skeleton                  = loader.Load<Skeleton>();
            long VertexArrayOffset    = loader.ReadOffset();
            long ShapeArrayOffset     = loader.ReadOffset();
            ShapeDict                 = loader.LoadDict();
            long MaterialArrayOffset  = loader.ReadOffset();
            if (loader.ResFile.VersionMajor2 == 9)
                loader.ReadOffset(); //padding?

            MaterialDict = loader.LoadDict();

            if (loader.ResFile.VersionMajor2 >= 10)
                ShaderAssignOffset = loader.ReadOffset();

            long UserDataOffset       = loader.ReadOffset();
            UserDataDict              = loader.LoadDict();


            long UserPointer          = loader.ReadInt64();

            ushort numVertexBuffer    = loader.ReadUInt16();
            ushort numShape           = loader.ReadUInt16();
            ushort numMaterial        = loader.ReadUInt16();

            ushort numUserData = 0;
            if (loader.ResFile.VersionMajor2 >= 9)
            {
                ShaderAssignCount = loader.ReadUInt16(); 
                numUserData = loader.ReadUInt16();
                loader.ReadUInt16(); //padding?
                uint padding = loader.ReadUInt32();
            }
            else
            {
                numUserData = loader.ReadUInt16();
                uint totalVertexCount = loader.ReadUInt32();
                uint padding = loader.ReadUInt32();
            }

            Shapes = loader.LoadList<Shape>(numShape, ShapeArrayOffset);
            ShaderAssign = loader.LoadList<MaterialParserV10.ShaderAssignV10>(ShaderAssignCount, ShaderAssignOffset);
            Materials = loader.LoadList<Material>(numMaterial, MaterialArrayOffset);
            UserData = loader.LoadList<UserData>(numUserData, UserDataOffset);
            VertexBuffers = loader.LoadList<VertexBuffer>(numVertexBuffer, VertexArrayOffset);

            foreach (var assign in ShaderAssign)
                assign.IsAnimationBinded = true;
        }

        internal long SkeletonOffset;
        internal long VertexBufferOffset;
        internal long ShapeOffset;
        internal long ShapeDictOffset;
        internal long MaterialsOffset;
        internal long MaterialsDictOffset;
        internal long PosUserDataOffset;
        internal long PosUserDataDictOffset;
        internal long ShaderAssignOffset;

        void IResData.Save(ResFileSaver saver)
        {
            //Add all shader assign that is bindable
            if (saver.ResFile.VersionMajor2 >= 10)
            {
                var shaderAssignList = this.Materials.Where(x => x.ShaderAssign.IsAnimationBinded).Select(x => (MaterialParserV10.ShaderAssignV10)x.ShaderAssign);
                this.ShaderAssign = shaderAssignList.ToList();
            }

            saver.WriteSignature(_signature);
            if (saver.ResFile.VersionMajor2 >= 9)
                saver.Write(UnknownFlag);
            else
                saver.SaveHeaderBlock();

            if (saver.ResFile.VersionMajor2 == 9)
                saver.SaveRelocateEntryToSection(saver.Position, 10, 1, 0, ResFileSaver.Section1, "Model");
            else if (saver.ResFile.VersionMajor2 >= 10)
                saver.SaveRelocateEntryToSection(saver.Position, 11, 1, 0, ResFileSaver.Section1, "Model"); 
            else
                saver.SaveRelocateEntryToSection(saver.Position, 10, 1, 0, ResFileSaver.Section1, "Model"); 

            saver.SaveString(Name);
            saver.SaveString(Path);
            SkeletonOffset = saver.SaveOffset();
            VertexBufferOffset = saver.SaveOffset();
            ShapeOffset = saver.SaveOffset();
            ShapeDictOffset = saver.SaveOffset();
            MaterialsOffset = saver.SaveOffset();
            MaterialsDictOffset = saver.SaveOffset();

            if (saver.ResFile.VersionMajor2 >= 10)
                saver.SaveList(ShaderAssign);

            if (saver.ResFile.VersionMajor2 == 9)
                saver.Write(0L);

            PosUserDataOffset = saver.SaveOffset();
            PosUserDataDictOffset = saver.SaveOffset();

            saver.Write(0L);
            saver.Write((ushort)VertexBuffers.Count);
            saver.Write((ushort)Shapes.Count);
            saver.Write((ushort)Materials.Count);
            if (saver.ResFile.VersionMajor2 >= 9)
            {
                saver.Write((ushort)ShaderAssign.Count);
                saver.Write((ushort)UserData.Count);
                saver.Write((ushort)0);
                saver.Write(0);
            }
            else
            {
                saver.Write((ushort)UserData.Count);
                saver.Write(TotalVertexCount);
                saver.Write(0);
            }
        }
    }
}