using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Syroot.NintenTools.NSW.Bfres.Core
{
    /// <summary>
    /// Saves the hierachy and data of a <see cref="Bfres.ResFile"/>.
    /// </summary>
    public class ResFileSaver : BinaryDataWriter
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets a data block alignment typically seen with <see cref="Buffer.Data"/>.
        /// </summary>
        internal const int AlignmentVertexBuffer = 8; //8 for vertex buffers

        //For RLT
        internal const int Section1 = 1;
        internal const int Section2 = 2;
        internal const int Section3 = 3;
        internal const int Section4 = 4;
        internal const int Section5 = 5;

        // ---- FIELDS -------------------------------------------------------------------------------------------------

        public bool SaveIndexBufferRuntimeData = false;
        public bool SaveVertexBufferRuntimeData = false;

        //These to save pointer info back to
        private uint _ofsFileName;
        private uint _ofsFileNameString;
        private uint _ofsFileSize;
        private uint _ofsStringPool;
        private uint _ofsIndexBuffer;
        private uint _ofsRelocationTable;
        private uint _ofsEndOfBlock;
        //Needed to determine section sizes for RLT
        private uint _ofsEndOfStringTable; //Section 1 size
        private long bufferInfoOffset; //Section 2 position (No entries)
        private uint _ofsVertexBuffer; //Section 3 position
        private long memoryPoolOffset; //Section 4 position
        private long _ofsTotalBufferSize;
        private uint _ofsMemoryPool;
        private uint _MemoryPoolSize; //Section 4 size
        private uint _ofsExternalFileBlock; //Section 5 position
        private List<long> _savedMemoryPoolPointers; //Section 4 entries

        private List<RelocationEntry> _savedSection1Entries;
        private List<RelocationEntry> _savedSection2Entries;
        private List<RelocationEntry> _savedSection3Entries;
        private List<RelocationEntry> _savedSection4Entries;
        private List<RelocationEntry> _savedSection5Entries;
        private string _fileName;
        private List<ItemEntry> _savedItems;
        private List<long> _savedHeaderBlockPositions;
        private IDictionary<string, StringEntry> _savedStrings;
        private IDictionary<object, BlockEntry> _savedBlocks;
        private List<RelocationSection> _savedRelocatedSections;

        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ResFileSaver"/> class saving data from the given
        /// <paramref name="resFile"/> into the specified <paramref name="stream"/> which is optionally left open.
        /// </summary>
        /// <param name="resFile">The <see cref="Bfres.ResFile"/> instance to save data from.</param>
        /// <param name="stream">The <see cref="Stream"/> to save data into.</param>
        /// <param name="leaveOpen"><c>true</c> to leave the stream open after writing, otherwise <c>false</c>.</param>
        internal ResFileSaver(ResFile resFile, Stream stream, bool leaveOpen)
            : base(stream, Encoding.ASCII, leaveOpen)
        {
            ByteOrder = ByteOrder.LittleEndian;
            ResFile = resFile;
        }

        internal ResFileSaver(Model model, ResFile resFile, Stream stream, bool leaveOpen)
            : base(stream, Encoding.ASCII, leaveOpen)
        {
            ByteOrder = ByteOrder.LittleEndian;
            Model = model;
            ResFile = resFile;
        }


        internal ResFileSaver(MaterialAnim materialAnim, ResFile resFile, Stream stream, bool leaveOpen)
            : base(stream, Encoding.ASCII, leaveOpen)
        {
            ByteOrder = ByteOrder.LittleEndian;
            MaterialAnim = materialAnim;
            ResFile = resFile;
        }

        internal ResFileSaver(ShapeAnim shapeAnim, ResFile resFile, Stream stream, bool leaveOpen)
            : base(stream, Encoding.ASCII, leaveOpen)
        {
            ByteOrder = ByteOrder.LittleEndian;
            ShapeAnim = shapeAnim;
            ResFile = resFile;
        }

        internal ResFileSaver(SceneAnim sceneAnim, ResFile resFile, Stream stream, bool leaveOpen)
            : base(stream, Encoding.ASCII, leaveOpen)
        {
            ByteOrder = ByteOrder.LittleEndian;
            SceneAnim = sceneAnim;
            ResFile = resFile;
        }

        internal ResFileSaver(VisibilityAnim visibilityAnim, ResFile resFile, Stream stream, bool leaveOpen)
            : base(stream, Encoding.ASCII, leaveOpen)
        {
            ByteOrder = ByteOrder.LittleEndian;
            VisibilityAnim = visibilityAnim;
            ResFile = resFile;
        }

        internal ResFileSaver(Material material, ResFile resFile, Stream stream, bool leaveOpen)
            : base(stream, Encoding.ASCII, leaveOpen)
        {
            ByteOrder = ByteOrder.LittleEndian;
            Material = material;
            ResFile = resFile;
        }

        internal ResFileSaver(Shape shape, ResFile resFile, Stream stream, bool leaveOpen)
            : base(stream, Encoding.ASCII, leaveOpen)
        {
            ByteOrder = ByteOrder.LittleEndian;
            Shape = shape;
            ResFile = resFile;
        }

        internal ResFileSaver(Skeleton skeleton, ResFile resFile, Stream stream, bool leaveOpen)
            : base(stream, Encoding.ASCII, leaveOpen)
        {
            ByteOrder = ByteOrder.LittleEndian;
            Skeleton = skeleton;
            ResFile = resFile;
        }

        internal ResFileSaver(Bone bone, ResFile resFile, Stream stream, bool leaveOpen)
    : base(stream, Encoding.ASCII, leaveOpen)
        {
            ByteOrder = ByteOrder.LittleEndian;
            Bone = bone;
            ResFile = resFile;
        }

        internal ResFileSaver(SkeletalAnim skeletonAnim, ResFile resFile, Stream stream, bool leaveOpen)
            : base(stream, Encoding.ASCII, leaveOpen)
        {
            ByteOrder = ByteOrder.LittleEndian;
            SkeletalAnim = skeletonAnim;
            ResFile = resFile;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResFileSaver"/> class for the file with the given
        /// <paramref name="fileName"/>.
        /// </summary>
        /// <param name="resFile">The <see cref="Bfres.ResFile"/> instance to save.</param>
        /// <param name="fileName">The name of the file to save the data into.</param>
        internal ResFileSaver(ResFile resFile, string fileName)
            : this(resFile, new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read), false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResFileSaver"/> class for the file with the given
        /// <paramref name="fileName"/>.
        /// </summary>
        /// <param name="model">The <see cref="Bfres.Model"/> instance to save.</param>
        /// <param name="fileName">The name of the file to save the data into.</param>
        internal ResFileSaver(Model model, ResFile resFile, string fileName)
            : this(model, resFile, new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read), false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResFileSaver"/> class for the file with the given
        /// <paramref name="fileName"/>.
        /// </summary>
        /// <param name="skeletonAnim">The <see cref="Bfres.SkeletalAnim"/> instance to save.</param>
        /// <param name="fileName">The name of the file to save the data into.</param>
        internal ResFileSaver(SkeletalAnim skeletonAnim, ResFile resFile, string fileName)
            : this(skeletonAnim, resFile, new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read), false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResFileSaver"/> class for the file with the given
        /// <paramref name="fileName"/>.
        /// </summary>
        /// <param name="materialAnim">The <see cref="Bfres.MaterialAnim"/> instance to save.</param>
        /// <param name="fileName">The name of the file to save the data into.</param>
        internal ResFileSaver(MaterialAnim materialAnim, ResFile resFile, string fileName)
            : this(materialAnim, resFile, new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read), false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResFileSaver"/> class for the file with the given
        /// <paramref name="fileName"/>.
        /// </summary>
        /// <param name="materialAnim">The <see cref="Bfres.ShapeAnim"/> instance to save.</param>
        /// <param name="fileName">The name of the file to save the data into.</param>
        internal ResFileSaver(ShapeAnim shapeAnim, ResFile resFile, string fileName)
            : this(shapeAnim, resFile, new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read), false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResFileSaver"/> class for the file with the given
        /// <paramref name="fileName"/>.
        /// </summary>
        /// <param name="sceneAnim">The <see cref="Bfres.SceneAnim"/> instance to save.</param>
        /// <param name="fileName">The name of the file to save the data into.</param>
        internal ResFileSaver(SceneAnim sceneAnim, ResFile resFile, string fileName)
            : this(sceneAnim, resFile, new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read), false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResFileSaver"/> class for the file with the given
        /// <paramref name="fileName"/>.
        /// </summary>
        /// <param name="visibilityAnim">The <see cref="Bfres.VisibilityAnim"/> instance to save.</param>
        /// <param name="fileName">The name of the file to save the data into.</param>
        internal ResFileSaver(VisibilityAnim visibilityAnim, ResFile resFile, string fileName)
            : this(visibilityAnim, resFile, new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read), false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResFileSaver"/> class for the file with the given
        /// <paramref name="fileName"/>.
        /// </summary>
        /// <param name="material">The <see cref="Bfres.Material"/> instance to save.</param>
        /// <param name="fileName">The name of the file to save the data into.</param>
        internal ResFileSaver(Material material, ResFile resFile, string fileName)
            : this(material, resFile, new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read), false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResFileSaver"/> class for the file with the given
        /// <paramref name="fileName"/>.
        /// </summary>
        /// <param name="shape">The <see cref="Bfres.Shape"/> instance to save.</param>
        /// <param name="fileName">The name of the file to save the data into.</param>
        internal ResFileSaver(Shape shape, ResFile resFile, string fileName)
            : this(shape, resFile, new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read), false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResFileSaver"/> class for the file with the given
        /// <paramref name="fileName"/>.
        /// </summary>
        /// <param name="bone">The <see cref="Bfres.Bone"/> instance to save.</param>
        /// <param name="fileName">The name of the file to save the data into.</param>
        internal ResFileSaver(Bone bone, ResFile resFile, string fileName)
            : this(bone, resFile, new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read), false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResFileSaver"/> class for the file with the given
        /// <paramref name="fileName"/>.
        /// </summary>
        /// <param name="skeleton">The <see cref="Bfres.Skeleton"/> instance to save.</param>
        /// <param name="fileName">The name of the file to save the data into.</param>
        internal ResFileSaver(Skeleton skeleton, ResFile resFile, string fileName)
            : this(skeleton, resFile, new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read), false)
        {
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the saved <see cref="Bfres.ResFile"/> instance.
        /// </summary>
        internal ResFile ResFile { get; }

        /// <summary>
        /// Gets the saved <see cref="Bfres.Model"/> instance.
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
        /// Gets the saved <see cref="Bfres.Material"/> instance.
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
        /// Gets the saved <see cref="Bfres.Shape"/> instance.
        /// </summary>
        internal Shape Shape { get; }

        /// <summary>
        /// Gets the current index when writing lists or dicts.
        /// </summary>
        internal int CurrentIndex { get; private set; }

        // ---- METHODS (INTERNAL) -------------------------------------------------------------------------------------

        internal void ExportSkeletonAnimation()
        {
            SetupLists();
            //Write the header
            WriteHeader("fresSUB", "FSKA\0\0\0\0");
            ((IResData)SkeletalAnim).Save(this);
            WriteSkeletonAnimations(SkeletalAnim);

            WriteStrings();
        }
        internal void ExportMaterialAnimation()
        {
            SetupLists();
            //Write the header
            WriteHeader("fresSUB", "FMAA\0\0\0\0");
            ((IResData)MaterialAnim).Save(this);
            WriteMaterialAnimations(MaterialAnim);

            WriteStrings();
        }
        internal void ExportBoneVisualAnimation()
        {
            SetupLists();
            //Write the header
            WriteHeader("fresSUB", "FVBN\0\0\0\0");
            ((IResData)VisibilityAnim).Save(this);
            WriteBoneVisabiltyAnimations(VisibilityAnim);

            WriteStrings();
        }
        internal void ExportShapeAnimation()
        {
            SetupLists();
            //Write the header
            WriteHeader("fresSUB", "FSHA\0\0\0\0");
            ((IResData)ShapeAnim).Save(this);
            WriteShapeAnimations(ShapeAnim);

            WriteStrings();
        }
        internal void ExportSceneAnimation()
        {
            SetupLists();
            //Write the header
            WriteHeader("fresSUB", "FSCN\0\0\0\0");
            ((IResData)SceneAnim).Save(this);
            WriteSceneAnimations(SceneAnim);

            WriteStrings();
        }
        internal void ExportBone()
        {
            SetupLists();
            //Write the header
            WriteHeader("fmdlSUB", "FSKL\0\0\0\0");
            WriteSignature("BONE");
            ((IResData)Bone).Save(this);

            WriteStrings();
        }
        internal void ExportSkeleton()
        {
            SetupLists();
            //Write the header
            WriteHeader("fmdlSUB", "FSKL\0\0\0\0");
            ((IResData)Skeleton).Save(this);
            WriteSkeleton(Skeleton);

            WriteStrings();
        }
        internal void ExportMaterials()
        {
            SetupLists();

            WriteHeader("fmdlSUB", "FMAT\0\0\0\0");
            ((IResData)Material).Save(this);
            WriteMaterials(Material);

            WriteStrings();
            Flush();
        }

        internal void WriteHeader(string SubSection, string Magic, int Offset = 32)
        {
            ByteOrder = ByteOrder.BigEndian;

            // Create queues fetching the names for the string pool and data blocks to store behind the headers.
            _savedItems = new List<ItemEntry>();
            _savedStrings = new SortedDictionary<string, StringEntry>(ResStringComparer.Instance);
            _savedBlocks = new Dictionary<object, BlockEntry>();

            //Write the header
            Write((byte)127);
            Write(Encoding.ASCII.GetBytes(SubSection));
            Write(ResFile.Version);
            WriteSignature(Magic);
            Write((int)(Offset - Position));
            Write(1); //Detects platform. 0 = Wii U, 1 == Switch
            Write(0);

            Seek(0x30, SeekOrigin.Begin);

            ByteOrder = ByteOrder.LittleEndian;
        }

        internal void ExportModel()
        {
            SetupLists();

            //Write the header
            WriteHeader("fresSUB", "FMDL\0\0\0\0", 48);

            long _ofsBufferInfoPosition = SaveOffset(); //Reserve buffer info pointer

            // Update Shape instances.
            foreach (Shape shape in Model.Shapes)
            {
                shape.VertexBuffer = Model.VertexBuffers[shape.VertexBufferIndex];
            }


            ((IResData)Model).Save(this);
            WriteModel(Model);
            WriteModelBlock(Model);
     
            WriteStrings();

            WriteOffset(_ofsBufferInfoPosition);
            ((IResData)ResFile.BufferInfo).Save(this);

            WriteIndexBuffer();
            WriteVertexBuffer();

            WriteMemoryPool();
        }
        internal void ExportShape()
        {
            SetupLists();

            //Write the header
            WriteHeader("fmdlSUB", "FSHP\0\0\0\0");
            long _ofsVertexPosition = SaveOffset(); //Reserve vertex buffer pointer
            long _ofsBufferInfoPosition = SaveOffset(); //Reserve buffer info pointer

            // Update Shape instances.
    //        Shape.VertexBuffer = Model.VertexBuffers[Shape.VertexBufferIndex];


            ((IResData)Shape).Save(this);
            WriteShapes(Shape);

            WriteOffset(_ofsVertexPosition);
            ((IResData)Shape.VertexBuffer).Save(this);
            WriteVertexBuffers(Shape.VertexBuffer);

            WriteStrings();

            WriteOffset(_ofsBufferInfoPosition);
            ((IResData)ResFile.BufferInfo).Save(this);

            WriteIndexBuffer();
            WriteVertexBuffer();

            WriteMemoryPool();

            Flush();
        }

        internal void SetupLists()
        {
            // Create queues fetching the names for the string pool and data blocks to store behind the headers.
            _savedItems = new List<ItemEntry>();
            _savedStrings = new SortedDictionary<string, StringEntry>(ResStringComparer.Instance);
            _savedBlocks = new Dictionary<object, BlockEntry>();
            _savedHeaderBlockPositions = new List<long>();
            _savedMemoryPoolPointers = new List<long>();

            _savedRelocatedSections = new List<RelocationSection>();
            _savedSection1Entries = new List<RelocationEntry>();
            _savedSection2Entries = new List<RelocationEntry>();
            _savedSection3Entries = new List<RelocationEntry>();
            _savedSection4Entries = new List<RelocationEntry>();
            _savedSection5Entries = new List<RelocationEntry>();
        }

        /// <summary>
        /// Starts serializing the data from the <see cref="ResFile"/> root.
        /// </summary>
        internal void Execute()
        {
            SetupLists();

            // Store the headers recursively and satisfy offsets to them, then the string pool and data blocks.
            ((IResData)ResFile).Save(this);

            //Setup subfiles first
            if (ResFile.Models.Count > 0)
            {
                WriteOffset(ResFile.ModelOffset);
                foreach (Model mdl in ResFile.Models)
                    ((IResData)mdl).Save(this);
            }
            if (ResFile.SkeletalAnims.Count > 0)
            {
                if (ResFile.VersionMajor2 == 9)
                {
                    SaveRelocateEntryToSection(Position + 8, 2, (uint)ResFile.SkeletalAnims.Count, 8, Section1, "Skeleton Animation"); //      <------------ Entry Set
                    SaveRelocateEntryToSection(Position + 32, 4, (uint)ResFile.SkeletalAnims.Count, 6, Section1, "Skeleton Animation"); //      <------------ Entry Set
                }
                else
                {
                    SaveRelocateEntryToSection(Position + 16, 2, (uint)ResFile.SkeletalAnims.Count, 10, Section1, "Skeleton Animation"); //      <------------ Entry Set
                    SaveRelocateEntryToSection(Position + 40, 4, (uint)ResFile.SkeletalAnims.Count, 8, Section1, "Skeleton Animation"); //      <------------ Entry Set
                }

                WriteOffset(ResFile.SkeletonAnimationOffset);
                foreach (SkeletalAnim ska in ResFile.SkeletalAnims)
                    ((IResData)ska).Save(this);
            }
            if (ResFile.MaterialAnims.Count > 0)
            {
          //      SaveRelocateEntryToSection(Position + 16, 2, (uint)ResFile.MaterialAnims.Count, 13, Section1, "Material Animation"); //      <------------ Entry Set
        //        SaveRelocateEntryToSection(Position + 40, 6, (uint)ResFile.MaterialAnims.Count, 9, Section1, "Material Animation"); //      <------------ Entry Set
                WriteOffset(ResFile.MaterialAnimationOffset);
                foreach (MaterialAnim matanim in ResFile.MaterialAnims)
                    ((IResData)matanim).Save(this);
            }
            if (ResFile.BoneVisibilityAnims.Count > 0)
            {
                if (ResFile.VersionMajor2 == 9)
                {
                    SaveRelocateEntryToSection(Position + 8, 2, (uint)ResFile.BoneVisibilityAnims.Count, 10, Section1, "Bone Vis Animation"); //      <------------ Entry Set
                    SaveRelocateEntryToSection(Position + 32, 6, (uint)ResFile.BoneVisibilityAnims.Count, 6, Section1, "Bone Vis Animation"); //      <------------ Entry Set
                }
                else
                {
                    SaveRelocateEntryToSection(Position + 16, 2, (uint)ResFile.BoneVisibilityAnims.Count, 11, Section1, "Bone Vis Animation"); //      <------------ Entry Set
                    SaveRelocateEntryToSection(Position + 40, 6, (uint)ResFile.BoneVisibilityAnims.Count, 7, Section1, "Bone Vis Animation"); //      <------------ Entry Set
                }

                WriteOffset(ResFile.BoneVisAnimationOffset);
                foreach (VisibilityAnim bnanim in ResFile.BoneVisibilityAnims)  
                    ((IResData)bnanim).Save(this);
            }
            if (ResFile.ShapeAnims.Count > 0)
            {
                if (ResFile.VersionMajor2 == 9)
                {
                    SaveRelocateEntryToSection(Position + 8, 2, (uint)ResFile.ShapeAnims.Count, 10, Section1, "Shape Animation"); //      <------------ Entry Set
                    SaveRelocateEntryToSection(Position + 32, 6, (uint)ResFile.ShapeAnims.Count, 6, Section1, "Shape Animation"); //      <------------ Entry Set
                }
                else
                {
                    SaveRelocateEntryToSection(Position + 16, 2, (uint)ResFile.ShapeAnims.Count, 11, Section1, "Shape Animation"); //      <------------ Entry Set
                    SaveRelocateEntryToSection(Position + 40, 6, (uint)ResFile.ShapeAnims.Count, 7, Section1, "Shape Animation"); //      <------------ Entry Set
                }

                WriteOffset(ResFile.ShapeAnimationOffset);
                foreach (ShapeAnim shpanim in ResFile.ShapeAnims)
                    ((IResData)shpanim).Save(this);
            }
            if (ResFile.SceneAnims.Count > 0)
            {
                if (ResFile.VersionMajor2 == 9)
                {
                    SaveRelocateEntryToSection(Position + 8, 2, (uint)ResFile.SceneAnims.Count, 10, Section1, "Scene Animation"); //      <------------ Entry Set
                    SaveRelocateEntryToSection(Position + 32, 6, (uint)ResFile.SceneAnims.Count, 6, Section1, "Scene Animation"); //      <------------ Entry Set
                }
                else
                {
                    SaveRelocateEntryToSection(Position + 16, 10, (uint)ResFile.SceneAnims.Count, 3, Section1, "Scene Animation"); //      <------------ Entry Set
                }

                WriteOffset(ResFile.SceneAnimationOffset);
                foreach (SceneAnim scnanim in ResFile.SceneAnims)
                    ((IResData)scnanim).Save(this);
            }
            if (ResFile.BufferInfo != null)
            {
                WriteOffset(ResFile.BufferInfoOffset);
                ((IResData)ResFile.BufferInfo).Save(this);
            }
            else
            {
                if (ResFile.VersionMajor2 != 9)
                {
                    //Save a default buffer info with no pointer to it
                    //Idk why but this section always saves even without a pointer
                    Write(0x22);
                    Seek(28);
                }
            }
            if (ResFile.ExternalFiles.Count > 0)
            {
                WriteOffset(ResFile.ExternalFileOffset);
                foreach (ExternalFile ext in ResFile.ExternalFiles)
                    ((IResData)ext).Save(this);
            }

            if (ResFile.Models.Count > 0)
            {
                WriteOffset(ResFile.ModelDictOffset);
                ((IResData)ResFile.ModelDict).Save(this);
            }
            if (ResFile.SkeletalAnims.Count > 0)
            {
                WriteOffset(ResFile.SkeletonAnimationDictOffset);
                ((IResData)ResFile.SkeletalAnimDict).Save(this);
            }
            if (ResFile.MaterialAnimDict.Count > 0)
            {
                WriteOffset(ResFile.MaterialAnimationnDictOffset);
                ((IResData)ResFile.MaterialAnimDict).Save(this);
            }
            if (ResFile.BoneVisibilityAnimDict.Count > 0)
            {
                WriteOffset(ResFile.BoneVisAnimationDictOffset);
                ((IResData)ResFile.BoneVisibilityAnimDict).Save(this);
            }
            if (ResFile.ShapeAnimDict.Count > 0)
            {
                WriteOffset(ResFile.ShapeAnimationDictOffset);
                ((IResData)ResFile.ShapeAnimDict).Save(this);
            }
            if (ResFile.SceneAnimDict.Count > 0)
            {
                WriteOffset(ResFile.SceneAnimationDictOffset);
                ((IResData)ResFile.SceneAnimDict).Save(this);
            }
            if (ResFile.ExternalFiles.Count > 0)
            {
                WriteOffset(ResFile.ExternalFileDictOffset);
                ((IResData)ResFile.ExternalFileDict).Save(this);
            }

            //save headers
            foreach (Model mdl in ResFile.Models)
                WriteModel(mdl);

            //Now save data blocks
            foreach (Model mdl in ResFile.Models)
            {
                WriteModelBlock(mdl);
            }

            foreach (SkeletalAnim ska in ResFile.SkeletalAnims)
                WriteSkeletonAnimations(ska);
            foreach (MaterialAnim matanim in ResFile.MaterialAnims)
                WriteMaterialAnimations(matanim);
            foreach (VisibilityAnim bnanim in ResFile.BoneVisibilityAnims)
                WriteBoneVisabiltyAnimations(bnanim);
            foreach (ShapeAnim shpanim in ResFile.ShapeAnims)        
                WriteShapeAnimations(shpanim);
            foreach (SceneAnim scnanim in ResFile.SceneAnims)
                WriteSceneAnimations(scnanim);
            


            WriteStrings();

            if (ResFile.BufferInfo != null)
            {
                WriteIndexBuffer();
                WriteVertexBuffer();
            }
            if (ResFile.MemoryPool != null)
                WriteMemoryPool();
            WriteBlocks(); //External files

            //First setup the values for RLT
            SetupRelocationTable();
            //Now write
            Align(8);
            WriteRelocationTable();

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

            //Save the file name. Goes directly to name instead of size
            Position = _ofsFileName;
            Write(_ofsFileNameString + 2);

            // Save final file size into root header at the provided offset.
            Position = _ofsFileSize;
            Write((uint)BaseStream.Length);
            Flush();
        }

        private void WriteModel(Model mdl)
        {
            if (mdl.VertexBuffers.Count > 0)
            {
                int VtxIndex = 0;

                WriteOffset(mdl.VertexBufferOffset);
                foreach (VertexBuffer vtx in mdl.VertexBuffers)
                {
                    CurrentIndex = VtxIndex++;
                    ((IResData)vtx).Save(this);
                }
            }
            if (mdl.Materials.Count > 0)
            {
                int MatIndex = 0;

                WriteOffset(mdl.MaterialsOffset);
                foreach (Material mat in mdl.Materials)
                {
                    CurrentIndex = MatIndex++;
                    ((IResData)mat).Save(this);
                }
            }
            if (mdl.Shapes.Count > 0)
            {
                int ShpIndex = 0;

                WriteOffset(mdl.ShapeOffset);
                foreach (Shape shp in mdl.Shapes)
                {
                    CurrentIndex = ShpIndex++;
                    ((IResData)shp).Save(this);
                }
            }
            if (mdl.UserData.Count > 0)
            {
                SaveUserData(mdl.UserData, mdl.PosUserDataOffset);
            }
            if (mdl.Skeleton != null)
            {
                WriteOffset(mdl.SkeletonOffset);
                if (mdl.Skeleton != null)
                    ((IResData)mdl.Skeleton).Save(this);
            }
        }

        private void WriteModelBlock(Model mdl)
        {
            if (mdl.Skeleton.Bones.Count > 0)
            {
                if (ResFile.VersionMajor2 >= 8)
                    SaveRelocateEntryToSection(Position, 3, (uint)mdl.Skeleton.Bones.Count, 9, Section1, "Bone array"); //      <------------ Entry Set
                else
                    SaveRelocateEntryToSection(Position, 3, (uint)mdl.Skeleton.Bones.Count, 7, Section1, "Bone array"); //      <------------ Entry Set
            }

            WriteSkeleton(mdl.Skeleton);

            if (mdl.ShapeDict.Count > 0)
            {
                WriteOffset(mdl.ShapeDictOffset);
                ((IResData)mdl.ShapeDict).Save(this);
            }
            if (mdl.MaterialDict.Count > 0)
            {
                WriteOffset(mdl.MaterialsDictOffset);
                ((IResData)mdl.MaterialDict).Save(this);
            }
            if (mdl.UserData.Count > 0)
            {
                WriteOffset(mdl.PosUserDataDictOffset);
                ((IResData)mdl.UserDataDict).Save(this);
                SaveUserDataData(mdl.UserData);
                Align(8);
            }

            foreach (Shape shp in mdl.Shapes)
                WriteShapes(shp);
            foreach (VertexBuffer vtx in mdl.VertexBuffers)
                WriteVertexBuffers(vtx);
            foreach (Material mat in mdl.Materials)
                WriteMaterials(mat);
        }

        private void WriteSkeleton(Skeleton skl)
        {
            if (skl.Bones.Count > 0)
            {
                int BoneIndex = 0;
                WriteOffset(skl.PosBoneArrayOffset);
                foreach (Bone bn in skl.Bones)
                {
                    CurrentIndex = BoneIndex++;
                    ((IResData)bn).Save(this);
                }
            }
            if (skl.MatrixToBoneList.Count > 0)
            {
                Align(8);
                WriteOffset(skl.PosMatrixToBoneListOffset);
                Write(skl.MatrixToBoneList);
            }
            if (skl.InverseModelMatrices.Count > 0)
            {
                Align(8);
                WriteOffset(skl.PosInverseModelMatricesOffset);
                skl.WriteMatrices(this);
            }

            if (skl.BoneDict.Count > 0)
            {
                WriteOffset(skl.PosBoneDictOffset);
                ((IResData)skl.BoneDict).Save(this);
            }
            if (skl.userIndices?.Length > 0)
            {
                WriteOffset(skl.PosUserPointer);
                Write(skl.userIndices);
            }
            if (skl.Bones.Count > 0)
            {
                foreach (Bone bn in skl.Bones)
                {
                    if (bn.UserData.Count > 0)
                    {
                        SaveUserData(bn.UserData, bn.PosUserDataOffset);
                    }
                    if (bn.UserData.Count > 0)
                    {
                        WriteOffset(bn.PosUserDataDictOffset);
                        ((IResData)bn.UserDataDict).Save(this);
                        SaveUserDataData(bn.UserData);
                        Align(8);
                    }
                }
            }
        }

        private void WriteShapes(Shape shp)
        {
            WriteOffset(shp.PosMeshArrayOffset);
            foreach (Mesh msh in shp.Meshes)
                ((IResData)msh).Save(this);

            if (shp.SkinBoneIndices.Count > 0)
            {
                WriteOffset(shp.PosSkinBoneIndicesOffset);
                Write(shp.SkinBoneIndices);
            }

            if (shp.SubMeshBoundings.Count > 0)
            {
                Align(8);
                WriteOffset(shp.PosSubMeshBoundingsOffset);
                shp.WriteBoudnings(this);
            }
            if (shp.KeyShapes.Count > 0)
            {
                WriteOffset(shp.PosKeyShapesOffset);
                foreach (KeyShape key in shp.KeyShapes)
                    ((IResData)key).Save(this);
            }
            if (shp.RadiusArray.Count > 0)
            {
                WriteOffset(shp.PosRadiusArrayOffset);
                Write(shp.RadiusArray);
            }
            foreach (Mesh msh in shp.Meshes)
            {
                Align(8);
                WriteOffset(msh.PosSubMeshesOffset);
                foreach (SubMesh sub in msh.SubMeshes)
                    ((IResData)sub).Save(this);
                WriteOffset(msh.PosBufferUnkOffset);
                Write(new long[9]);
                WriteOffset(msh.PosBufferSizeOffset);
                ((IResData)msh.bufferSize).Save(this);
            }
        }

        private void WriteVertexBuffers(VertexBuffer vtx)
        {
            long[] unk = new long[vtx.Buffers.Count];
            long[] unkArray = new long[vtx.Buffers.Count * 9]; //Each area covers 56 bytes for each buffer

            if (vtx.Attributes.Count > 0)
            {
                SaveRelocateEntryToSection(Position, 1, (uint)vtx.Attributes.Count, 1, Section1, "Vertex Attributes"); //      <------------ Entry Set
                WriteOffset(vtx.AttributeOffset);
                foreach (VertexAttrib att in vtx.Attributes)
                    ((IResData)att).Save(this);
            }
            if (vtx.Buffers.Count > 0)
            {
                WriteOffset(vtx.UnkBufferOffset);
                Write(unkArray);

                WriteOffset(vtx.BufferSizeArrayOffset);
                foreach (VertexBufferSize size in vtx.VertexBufferSizeArray)
                    ((IResData)size).Save(this);

                WriteOffset(vtx.StideArrayOffset);
                foreach (VertexBufferStride st in vtx.StrideArray)
                    ((IResData)st).Save(this);

                if (SaveVertexBufferRuntimeData)
                    Seek(0x20);//Todo WTF is this

                WriteOffset(vtx.UnkBuffer2Offset);
                Write(unk);
            }
            if (vtx.AttributeDict.Count > 0)
            {
                WriteOffset(vtx.AttributeDictOffset);
                ((IResData)vtx.AttributeDict).Save(this);
            }
        }

        private void WriteSkeletonAnimations(SkeletalAnim ska)
        {
            if (ska.BindIndices.Length > 0)
            {
                Align(8);
                WriteOffset(ska.BindIndicesOffset);
                Write(ska.BindIndices);
            }
            if (ska.BoneAnims.Count > 0)
            {
                Align(8);
                if (ResFile.VersionMajor2 == 9)
                    SaveRelocateEntryToSection(Position, 3, (uint)ska.BoneAnims.Count, 4, Section1, "Bone Animation"); //      <------------ Entry Set
                else
                    SaveRelocateEntryToSection(Position, 3, (uint)ska.BoneAnims.Count, 2, Section1, "Bone Animation"); //      <------------ Entry Set
                WriteOffset(ska.BoneAnimsOffset);
                foreach (BoneAnim bn in ska.BoneAnims)
                    ((IResData)bn).Save(this);
            }
            foreach (BoneAnim bn in ska.BoneAnims)
            {
                object baseData = bn.BaseData;
                if (baseData != null)
                {
                    Align(8);
                    WriteOffset(bn.PosBaseDataOffset);
                    bn.BaseData.Save(this, bn.FlagsBase);

                }

                if (bn.Curves.Count > 0)
                {
                    Align(8);
                    SaveRelocateEntryToSection(Position, 2, (uint)bn.Curves.Count, 4, Section1, "Animation Curve"); //      <------------ Entry Set
                    WriteOffset(bn.PosCurvesOffset);
                    foreach (AnimCurve cr in bn.Curves)
                        ((IResData)cr).Save(this);

                    foreach (AnimCurve cr in bn.Curves)
                    {
                        WriteOffset(cr.PosFrameOffset);
                        cr.SaveFrames(this);
                        Align(8);

                        WriteOffset(cr.PosKeyDataOffset);
                        cr.SaveKeyData(this);
                        Align(8);
                    }
                }
            }
            if (ska.UserDatas.Count > 0)
            {
                SaveUserData(ska.UserDatas, ska.UserDataOffset);
            }
            if (ska.UserDataDict.Count > 0)
            {
                WriteOffset(ska.UserDataDictOffset);
                ((IResData)ska.UserDataDict).Save(this);
                SaveUserDataData(ska.UserDatas);
                Align(8);
            }
        }

        private void WriteMaterialAnimations(MaterialAnim matanim)
        {
            if (matanim.BindIndices != null && matanim.BindIndices.Length > 0)
            {
                WriteOffset(matanim.PosBindIndicesOffset);
                Write(matanim.BindIndices);
                Align(8);
            }
            if (matanim.MaterialAnimDataList.Count > 0)
            {
                //    SaveRelocateEntryToSection(Position, 5, (uint)matanim.MaterialAnimDataList.Count, 3, Section1, "Material Animation Data"); //      <------------ Entry Set
                WriteOffset(matanim.PosMatAnimDataOffset);
                foreach (MaterialAnimData mat in matanim.MaterialAnimDataList)
                    ((IResData)mat).Save(this);
            }
            foreach (MaterialAnimData mat in matanim.MaterialAnimDataList)
            {
                if (mat.ParamAnimInfos.Count > 0)
                {
                    SaveRelocateEntryToSection(Position, 1, (uint)mat.ParamAnimInfos.Count, 2, Section1, "Param Anim Info"); //      <------------ Entry Set
                    WriteOffset(mat.PosParamInfoOffset);
                    foreach (ParamAnimInfo prm in mat.ParamAnimInfos)
                        ((IResData)prm).Save(this);
                }
                if (mat.TexturePatternAnimInfos.Count > 0)
                {
                    SaveRelocateEntryToSection(Position, 1, (uint)mat.TexturePatternAnimInfos.Count, 1, Section1, "Tex Pat Anim Info"); //      <------------ Entry Set
                    WriteOffset(mat.PosTexPatInfoOffset);
                    foreach (TexturePatternAnimInfo tex in mat.TexturePatternAnimInfos)
                        ((IResData)tex).Save(this);
                }
                if (mat.Constants != null && mat.Constants.Count > 0)
                {
                    WriteOffset(mat.PosConstansOffset);
                    mat.WriteConstansts(this);
                }
                if (mat.Curves.Count > 0)
                {
                    SaveRelocateEntryToSection(Position, 2, (uint)mat.Curves.Count, 4, Section1, "Animation Curve"); //      <------------ Entry Set
                    WriteOffset(mat.PosCurvesOffset);
                    foreach (AnimCurve cr in mat.Curves)
                        ((IResData)cr).Save(this);

                    foreach (AnimCurve cr in mat.Curves)
                    {
                        WriteOffset(cr.PosFrameOffset);
                        cr.SaveFrames(this);
                        Align(8);

                        WriteOffset(cr.PosKeyDataOffset);
                        cr.SaveKeyData(this);
                        Align(8);
                    }
                }
            }
            if (matanim.TextureNames != null)
            {
                if (matanim.TextureNames.Count > 0)
                {
                    WriteOffset(matanim.PosTextureNamesUnkOffset);
                    Write(new long[matanim.TextureNames.Count]); //Set at runtime i think

                    SaveRelocateEntryToSection(Position, (uint)matanim.TextureNames.Count, 1, 0, Section1, "Texture Pat List"); //      <------------ Entry Set
                    WriteOffset(matanim.PosTextureNamesOffset);
                    SaveStrings(matanim.TextureNames);

                    WriteOffset(matanim.PosTextureBindArrayOffset);
                    Write(matanim.TextureBindArray);

                }
            }
            if (matanim.UserData.Count > 0)
            {
                SaveUserData(matanim.UserData, matanim.PosUserDataOffset);
            }
            if (matanim.UserDataDict.Count > 0)
            {
                WriteOffset(matanim.PosUserDataDictOffset);
                ((IResData)matanim.UserDataDict).Save(this);
                SaveUserDataData(matanim.UserData);
                Align(8);
            }
        }

        private void WriteBoneVisabiltyAnimations(VisibilityAnim bnanim)
        {
            if (bnanim.BindIndices != null)
            {
                WriteOffset(bnanim.PosBindIndicesOffset);
                Write(bnanim.BindIndices);
                Align(8);
            }
            if (bnanim.Names != null)
            {
                if (bnanim.Names.Count > 0)
                {
                    SaveRelocateEntryToSection(Position, (uint)bnanim.Names.Count, 1, 0, Section1, "Bone Vis List"); //      <------------ Entry Set
                    WriteOffset(bnanim.PosNamesOffset);
                    SaveStrings(bnanim.Names);
                    Align(8);
                }
            }
            if (bnanim.Curves.Count > 0)
            {
                SaveRelocateEntryToSection(Position, 2, (uint)bnanim.Curves.Count, 4, Section1, "Animation Curve"); //      <------------ Entry Set
                WriteOffset(bnanim.PosCurvesOffset);
                foreach (AnimCurve cr in bnanim.Curves)
                    ((IResData)cr).Save(this);

                foreach (AnimCurve cr in bnanim.Curves)
                {
                    WriteOffset(cr.PosFrameOffset);
                    cr.SaveFrames(this);
                    Align(8);

                    WriteOffset(cr.PosKeyDataOffset);
                    cr.SaveKeyData(this);
                    Align(8);
                }
            }
            if (bnanim.BaseSDataList != null)
            {
                WriteOffset(bnanim.PosBaseDataOffset);
                bnanim.WriteBaseData(this);
                Align(8);
            }
        }

        private void WriteShapeAnimations(ShapeAnim shpanim)
        {
            if (shpanim.BindIndices.Length > 0)
            {
                WriteOffset(shpanim.PosBindIndicesOffset);
                Write(shpanim.BindIndices);
            }
            if (shpanim.VertexShapeAnims.Count > 0)
            {
                WriteOffset(shpanim.PosVertexShapeAnimsOffset);
                foreach (VertexShapeAnim vtxanim in shpanim.VertexShapeAnims)
                    ((IResData)vtxanim).Save(this);

                foreach (VertexShapeAnim vtxanim in shpanim.VertexShapeAnims)
                {
                    if (vtxanim.BaseDataList?.Length > 0)
                    {
                        WriteOffset(vtxanim.PosBaseDataOffset);
                        Write(vtxanim.BaseDataList);
                        Align(8);
                    }
                    if (vtxanim.KeyShapeAnimInfos?.Count > 0)
                    {
                        WriteOffset(vtxanim.PosKeyShapeAnimInfosOffset);
                        foreach (var cr in vtxanim.KeyShapeAnimInfos)
                            ((IResData)cr).Save(this);
                    }
                    if (vtxanim.Curves.Count > 0)
                    {
                        Align(8);
                        SaveRelocateEntryToSection(Position, 2, (uint)vtxanim.Curves.Count, 4, Section1, "Animation Curve"); //      <------------ Entry Set
                        WriteOffset(vtxanim.PosCurvesOffset);
                        foreach (AnimCurve cr in vtxanim.Curves)
                            ((IResData)cr).Save(this);

                        foreach (AnimCurve cr in vtxanim.Curves)
                        {
                            WriteOffset(cr.PosFrameOffset);
                            cr.SaveFrames(this);
                            Align(8);

                            WriteOffset(cr.PosKeyDataOffset);
                            cr.SaveKeyData(this);
                            Align(8);
                        }
                    }
                }
            }
            if (shpanim.UserData.Count > 0)
            {
                WriteOffset(shpanim.PosUserDataOffset);
                foreach (UserData data in shpanim.UserData)
                    ((IResData)data).Save(this);
            }
            if (shpanim.UserDataDict.Count > 0)
            {
                WriteOffset(shpanim.PosUserDataDictOffset);
                ((IResData)shpanim.UserDataDict).Save(this);
            }
        }

        private void WriteSceneAnimations(SceneAnim scnanim)
        {
            Seek(104);
            if (scnanim.CameraAnims.Count > 0)
            {
                int CurCam = 0;
                foreach (CameraAnim camanim in scnanim.CameraAnims)
                {
                    if (CurCam == 0)
                        WriteOffset(scnanim.PosCameraAnimArrayOffset);

                    ((IResData)camanim).Save(this);
                    CurCam++;
                }
            }

            if (scnanim.CameraAnims.Count > 0)
            {
                WriteOffset(scnanim.PosCameraAnimDictOffset);
                ((IResData)scnanim.CameraAnimDict).Save(this);
            }

            foreach (CameraAnim camanim in scnanim.CameraAnims)
            {
                if (camanim.Curves.Count > 0)
                {
                    Align(8);
                    SaveRelocateEntryToSection(Position, 2, (uint)camanim.Curves.Count, 4, Section1, "Animation Curve"); //      <------------ Entry Set
                    WriteOffset(camanim.PosCurveArrayOffset);
                    foreach (AnimCurve cr in camanim.Curves)
                        ((IResData)cr).Save(this);
                }

                WriteOffset(camanim.PosBaseDataOffset);
                camanim.BaseData.Save(this);
                Align(8);

                foreach (AnimCurve cr in camanim.Curves)
                {
                    WriteOffset(cr.PosFrameOffset);
                    cr.SaveFrames(this);
                    Align(8);

                    WriteOffset(cr.PosKeyDataOffset);
                    cr.SaveKeyData(this);
                    Align(8);
                }

                if (camanim.UserData.Count > 0)
                {
                    SaveUserData(camanim.UserData, camanim.PosUserDataOffset);

                    WriteOffset(camanim.PosUserDataDictOffset);
                    ((IResData)camanim.UserDataDict).Save(this);
                    SaveUserDataData(camanim.UserData);
                    Align(8);
                }
            }
            foreach (LightAnim lightanim in scnanim.LightAnims)
            {
                WriteOffset(lightanim.PosBaseDataOffset);
                lightanim.BaseData.Save(this, lightanim.AnimatedFields);

                if (lightanim.Curves.Count > 0)
                {
                    SaveRelocateEntryToSection(Position, 2, (uint)lightanim.Curves.Count, 4, Section1, "Animation Curve"); //      <------------ Entry Set
                    WriteOffset(lightanim.PosCurveArrayOffset);
                    foreach (AnimCurve cr in lightanim.Curves)
                        ((IResData)cr).Save(this);

                    foreach (AnimCurve cr in lightanim.Curves)
                    {
                        WriteOffset(cr.PosFrameOffset);
                        cr.SaveFrames(this);
                        Align(8);

                        WriteOffset(cr.PosKeyDataOffset);
                        cr.SaveKeyData(this);
                        Align(8);
                    }
                }

                if (lightanim.UserData.Count > 0)
                {
                    SaveUserData(lightanim.UserData, lightanim.PosUserDataOffset);

                    WriteOffset(lightanim.PosUserDataDictOffset);
                    ((IResData)lightanim.UserDataDict).Save(this);
                    SaveUserDataData(lightanim.UserData);
                    Align(8);
                }
            }
            foreach (FogAnim foganim in scnanim.FogAnims)
            {
                WriteOffset(foganim.PosBaseDataOffset);
                foganim.BaseData.Save(this);

                if (foganim.Curves.Count > 0)
                {
                    SaveRelocateEntryToSection(Position, 2, (uint)foganim.Curves.Count, 4, Section1, "Animation Curve"); //      <------------ Entry Set
                    WriteOffset(foganim.PosCurveArrayOffset);
                    foreach (AnimCurve cr in foganim.Curves)
                        ((IResData)cr).Save(this);

                    foreach (AnimCurve cr in foganim.Curves)
                    {
                        WriteOffset(cr.PosFrameOffset);
                        cr.SaveFrames(this);
                        Align(8);

                        WriteOffset(cr.PosKeyDataOffset);
                        cr.SaveKeyData(this);
                        Align(8);
                    }
                }

                if (foganim.UserData.Count > 0)
                {
                    SaveUserData(foganim.UserData, foganim.PosUserDataOffset);

                    WriteOffset(foganim.PosUserDataDictOffset);
                    ((IResData)foganim.UserDataDict).Save(this);
                    SaveUserDataData(foganim.UserData);
                    Align(8);
                }
            }


            if (scnanim.LightAnims.Count > 0)
            {
                WriteOffset(scnanim.PosLightAnimDictOffset);
                ((IResData)scnanim.LightAnimDict).Save(this);
            }
            if (scnanim.FogAnims.Count > 0)
            {
                WriteOffset(scnanim.PosFogAnimDictOffset);
                ((IResData)scnanim.FogAnimDict).Save(this);
            }
            if (scnanim.UserData.Count > 0)
            {
                WriteOffset(scnanim.PosUserDataDictOffset);
                ((IResData)scnanim.UserDataDict).Save(this);
            }
        }

        private void WriteMaterials(Material mat)
        {
            if (this.ResFile.VersionMajor2 >= 10)
            {
                SaveEntries();
                return;
            }

            if (mat.RenderInfos == null)
                mat.RenderInfos = new List<RenderInfo>();
            if (mat.ShaderParams == null)
                mat.ShaderParams = new List<ShaderParam>();
            if (mat.UserDatas == null)
                mat.UserDatas = new List<UserData>();
            if (mat.ShaderAssign == null)
                mat.ShaderAssign = new ShaderAssign();
            if (mat.TextureRefs == null)
                mat.TextureRefs = new List<string>();
            if (mat.VolatileFlags == null)
                mat.VolatileFlags = new byte[0];

            if (mat.RenderInfos.Count > 0)
            {
                SaveRelocateEntryToSection(Position, 2, (uint)mat.RenderInfos.Count, 1, Section1, "Render infos"); //      <------------ Entry Set
                WriteOffset(mat.PosRenderInfoOffset);
                foreach (RenderInfo rnd in mat.RenderInfos)
                    ((IResData)rnd).Save(this);

                int numStrings = mat.RenderInfos.Sum(item => item.GetValueStrings().Length);

                if (numStrings != 0)
                    SaveRelocateEntryToSection(Position, (uint)numStrings, 1, 0, Section1, "Render info strings"); //      <------------ Entry Set

                foreach (RenderInfo rnd in mat.RenderInfos)
                {
                    if (rnd.Type == RenderInfoType.String && rnd.GetValueStrings().Length > 0)
                    {
                        WriteOffset(rnd.DataOffset);
                        rnd.SaveStrings(this);
                    }
                }
                foreach (RenderInfo rnd in mat.RenderInfos)
                {
                    if (rnd.Type == RenderInfoType.Int32 && rnd.GetValueInt32s().Length > 0)
                    {
                        WriteOffset(rnd.DataOffset);
                        rnd.SaveInts(this);
                    }
                }
                foreach (RenderInfo rnd in mat.RenderInfos)
                {
                    if (rnd.Type == RenderInfoType.Single && rnd.GetValueSingles().Length > 0)
                    {
                        WriteOffset(rnd.DataOffset);
                        rnd.SaveFloats(this);
                    }
                }
            }
            if (mat.TextureRefs.Count > 0)
            {
                long[] unkown2 = new long[mat.TextureRefs.Count];//Set at runtime????
                Align(8);
                WriteOffset(mat.PosTextureUnk1Offset);
                Write(unkown2);
            }
            if (mat.Samplers.Count > 0)
            {
                long[] unkown1 = new long[mat.TextureRefs.Count * 15]; //Set at runtime????
                Align(8);
                WriteOffset(mat.PosSamplersOffset);
                foreach (Sampler smp in mat.Samplers)
                    ((IResData)smp).Save(this);

                WriteOffset(mat.PosTextureUnk2Offset);
                Write(unkown1);
            }
            if (mat.ShaderParams.Count > 0)
            {
                SaveRelocateEntryToSection(Position + 8, 1, (uint)mat.ShaderParams.Count, 3, Section1, "Shader Param"); //      <------------ Entry Set
                WriteOffset(mat.PosShaderParamsOffset);
                foreach (ShaderParam prm in mat.ShaderParams)
                    ((IResData)prm).Save(this);
                WriteOffset(mat.PosShaderParamDataOffset);
                Write(mat.ShaderParamData);
            }
            if (mat.VolatileFlags.Length > 0)
            {
                Align(8);
                WriteOffset(mat.PosVolatileFlagsOffset);
                Write(mat.VolatileFlags);
                Align(8);
            }
            if (mat.UserDatas.Count > 0)
                SaveUserData(mat.UserDatas, mat.PosUserDataMaterialOffset);

            if (mat.Samplers.Count > 0)
            {
                WriteOffset(mat.PosSamplerSlotArrayOffset);
                Write(mat.SamplerSlotArray);
            }
            if (mat.TextureRefs.Count > 0)
            {
                WriteOffset(mat.PosTextureSlotArrayOffset);
                Write(mat.TextureSlotArray);
            }
            if (mat.TextureRefs.Count > 0)
            {
                SaveRelocateEntryToSection(Position, (uint)mat.TextureRefs.Count, 1, 0, Section1, "Texture Refs"); //      <------------ Entry Set
                WriteOffset(mat.PosTextureRefsOffset);
                SaveStrings(mat.TextureRefs);
            }
            if (mat.ShaderAssign != null)
            {
                ShaderAssign shd = mat.ShaderAssign;

                if (shd.ShaderOptions == null)
                    shd.ShaderOptions = new List<string>();

                SaveRelocateEntryToSection(Position, 8, 1, 0, Section1, "Shader assign"); //      <------------ Entry Set
                WriteOffset(mat.PosShaderAssignOffset);
                ((IResData)shd).Save(this);

                uint ShaderAssignDataCount = (uint)(mat.ShaderAssign.AttribAssignDict.Count +
                    mat.ShaderAssign.SamplerAssignDict.Count +
                    mat.ShaderAssign.ShaderOptionDict.Count);

                SaveRelocateEntryToSection(Position, (uint)(ShaderAssignDataCount), 1, 0, ResFileSaver.Section1, "Shader assign data"); //      <------------ Entry Set

                if (shd.AttribAssigns.Count > 0)
                {
                    WriteOffset(shd.PosAttribAssigns);
                    SaveStrings(shd.AttribAssigns);
                }
                if (shd.SamplerAssigns.Count > 0)
                {
                    WriteOffset(shd.PosSamplerAssigns);
                    SaveStrings(shd.SamplerAssigns);
                }
                if (shd.ShaderOptions.Count > 0)
                {
                    WriteOffset(shd.PosShaderOptions);
                    SaveStrings(shd.ShaderOptions);
                }
                if (shd.AttribAssigns.Count > 0)
                {
                    WriteOffset(shd.PosAttribAssignDict);
                    ((IResData)shd.AttribAssignDict).Save(this);
                }
                if (shd.SamplerAssigns.Count > 0)
                {
                    WriteOffset(shd.PosSamplerAssignDict);
                    ((IResData)shd.SamplerAssignDict).Save(this);
                }
                if (shd.ShaderOptions.Count > 0)
                {
                    WriteOffset(shd.PosShaderOptionsDict);
                    ((IResData)shd.ShaderOptionDict).Save(this);
                }
            }
            if (mat.RenderInfoDict.Count > 0)
            {
                WriteOffset(mat.PosRenderInfoDictOffset);
                ((IResData)mat.RenderInfoDict).Save(this);
            }
            if (mat.SamplerDict.Count > 0)
            {
                WriteOffset(mat.PosSamplerDictOffset);
                ((IResData)mat.SamplerDict).Save(this);
            }
            if (mat.ShaderParamDict.Count > 0)
            {
                WriteOffset(mat.PosShaderParamDictOffset);
                ((IResData)mat.ShaderParamDict).Save(this);
            }
            if (mat.UserDataDict.Count > 0)
            {
                WriteOffset(mat.PosUserDataDictMaterialOffset);
                ((IResData)mat.UserDataDict).Save(this);
                SaveUserDataData(mat.UserDatas);
                Align(8);
            }
        }

        private void SaveUserData(IList<UserData> userData, long Target)
        {
            Align(8);
            SaveRelocateEntryToSection(Position, 2, (uint)userData.Count, 6, Section1, "user Data"); //      <------------ Entry Set
            WriteOffset(Target);
            foreach (UserData data in userData)
                ((IResData)data).Save(this);
        }

        private void SaveUserDataData(IList<UserData> userData)
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

        internal void WriteOffset(long offset)
        {
            long pos = Position;
            using (TemporarySeek(offset, SeekOrigin.Begin))
            {
                Write(pos);
            }
        }

        internal long SaveOffset()
        {
            long pos = Position;
            Write(0L);
            return pos;
        }

        internal void Write(sbyte[] value)
        {
            for (int i = 0; i < value.Length; i++)
                Write(value[i]);
        }

        // Has 5 sections which branch off to multiple entries
        // If a section is unused it'll use the same offset and data from previous section
        // First section stores all the data from start of file to the end string table.
        // Index buffer gets stored in second section
        // Vertex buffer gets stored in third section
        // Memory Pool gets stored in fourth section
        // All Embedded files stored in last section (points to RLT itself if not used)
        // Entries sometimes will reference structs like the FMDL and will skip the block and magic data
        // Entries sometimes will reference dicts aswell
        internal void SetupRelocationTable()
        {
            RelocationSection ResFileSect;
            RelocationSection ResIndexSect;
            RelocationSection ResVertexSect;
            RelocationSection MemPoolSect;
            RelocationSection ExtFileSect;

            //Setup alignment
            if (ResFile.ExternalFiles.Count > 0)
            {
                if (ResFile.ExternalFiles[0].Data.Length <= 3)
                    Align((int)256);
                else
                    Align((int)ResFile.DataAlignment);
            }

            long RelocationTableStart = Position;


            int EntryIndex = 0;
            uint EntryPos = 0;
            uint SectionSize = _ofsEndOfStringTable;

            int cr = 0;
            int bn = 0;

            _savedSection2Entries = _savedSection2Entries.OrderBy(o => o.Position).ToList();
            _savedSection3Entries = _savedSection3Entries.OrderBy(o => o.Position).ToList();
            _savedSection4Entries = _savedSection4Entries.OrderBy(o => o.Position).ToList();
            _savedSection5Entries = _savedSection5Entries.OrderBy(o => o.Position).ToList();

            bool PrintDebug = true;

            if (PrintDebug)
            {
                foreach (RelocationEntry entry in _savedSection1Entries)
                {
                    Console.WriteLine("Pos = " + entry.Position + " " + entry.StructCount + " " + entry.OffsetCount + " " + entry.PadingCount + " " + entry.Hint);
                }
                foreach (RelocationEntry entry in _savedSection2Entries)
                {
                    Console.WriteLine("Pos = " + entry.Position + " " + entry.StructCount + " " + entry.OffsetCount + " " + entry.PadingCount + " " + entry.Hint);
                }
                foreach (RelocationEntry entry in _savedSection4Entries)
                {
                    Console.WriteLine("Pos = " + entry.Position + " " + entry.StructCount + " " + entry.OffsetCount + " " + entry.PadingCount + " " + entry.Hint);
                }
                foreach (RelocationEntry entry in _savedSection5Entries)
                {
                    Console.WriteLine("Pos = " + entry.Position + " " + entry.StructCount + " " + entry.OffsetCount + " " + entry.PadingCount + " " + entry.Hint);
                }
            }

            //Setup Section 1
            ResFileSect = new RelocationSection(EntryPos, EntryIndex, SectionSize, _savedSection1Entries);
            EntryIndex += _savedSection1Entries.Count;

            if (ResFile.BufferInfo != null) //Setup Section 2 & 3
            {
                EntryPos = (uint)bufferInfoOffset;
                ResIndexSect = new RelocationSection(EntryPos, EntryIndex, ResFile.BufferInfo.getIndexBufferSize(this), _savedSection2Entries);

                EntryIndex += _savedSection2Entries.Count;

                EntryPos = _ofsVertexBuffer;
                ResVertexSect = new RelocationSection(EntryPos, EntryIndex, ResFile.BufferInfo.GetVertexBufferSize(this), _savedSection3Entries);

                EntryIndex += _savedSection3Entries.Count;
            }
            else
            {
                EntryPos = SectionSize;
                ResIndexSect = new RelocationSection(EntryPos, EntryIndex, 0, _savedSection2Entries);
                ResVertexSect = new RelocationSection(EntryPos, EntryIndex, 0, _savedSection3Entries);
            }
            if (ResFile.MemoryPool != null) //Setup Section 4
            {
                EntryPos = (uint)memoryPoolOffset;
                MemPoolSect = new RelocationSection(EntryPos, EntryIndex, 288, _savedSection4Entries);

                EntryIndex += _savedSection4Entries.Count;
            }
            else
            {
                EntryPos = SectionSize;
                MemPoolSect = new RelocationSection(EntryPos, EntryIndex, 0, _savedSection4Entries);
            }
            EntryIndex += _savedSection3Entries.Count;
            if (ResFile.ExternalFiles.Count > 0)  //Setup Section 5
            {
                EntryPos = _ofsExternalFileBlock;
                ExtFileSect = new RelocationSection(EntryPos, EntryIndex, (uint)(RelocationTableStart - _ofsExternalFileBlock), _savedSection5Entries);

                EntryIndex += _savedSection5Entries.Count;
            }
            else
                ExtFileSect = new RelocationSection(EntryPos, EntryIndex, 0, _savedSection5Entries);
            EntryIndex += _savedSection4Entries.Count;

            _savedRelocatedSections.Add(ResFileSect);
            _savedRelocatedSections.Add(ResIndexSect);
            _savedRelocatedSections.Add(ResVertexSect);
            _savedRelocatedSections.Add(MemPoolSect);
            _savedRelocatedSections.Add(ExtFileSect);
        }

        /// <summary>
        /// Gets the block size of all external files
        /// </summary>
        internal uint GetExternalFilesBlockSize()
        {
            uint TotalSize = 0;



            return TotalSize;
        }

        /// <summary>
        /// Reserves space for an offset to the <paramref name="resData"/> written later.
        /// </summary>
        /// <param name="resData">The <see cref="IResData"/> to save.</param>
        /// <param name="index">The index of the element, used for instances referenced by a <see cref="ResDict"/>.
        /// <param name="ShiftPos">The position the offset is saved to.</param>
        /// </param>
        [DebuggerStepThrough]
        internal void Save(IResData resData, int index = -1, long ShiftPos = 0)
        {
            long NewPos = Position;

            if (ShiftPos != 0)
            {
                NewPos = ShiftPos;
            }

            if (resData == null && ShiftPos == 0)
            {
                Write(0L);
                return;
            }
            if (TryGetItemEntry(resData, ItemEntryType.ResData, out ItemEntry entry))
            {
                entry.Offsets.Add((uint)(NewPos));
                entry.Index = index;
            }
            else
            {
                _savedItems.Add(new ItemEntry(resData, ItemEntryType.ResData, (uint)(NewPos), index: index));
            }

            if (ShiftPos == 0)
            {
                Write(UInt32.MaxValue);
                Write(0);
            }
        }

        /// <summary>
        /// Reserves space for the <see cref="Bfres.ResFile"/> file size field which is automatically filled later.
        /// </summary>
        [DebuggerStepThrough]
        internal void SaveFieldFileSize()
        {
            _ofsFileSize = (uint)Position;
            Write(0);
        }

        /// <summary>
        /// Save pointer array to be relocated in section 1
        /// </summary>
        [DebuggerStepThrough]
        internal void SaveRelocateEntryToSection(long pos, uint OffsetCount, uint StructCount, uint PaddingCount, int SectionNumber, string Hint)
        {
            if (StructCount <= 0)
                throw new Exception("Invalid struct count. Should be greater than 0! " + StructCount);


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
                if (SectionNumber == Section3)
                    _savedSection3Entries.Add(new RelocationEntry((uint)pos, OffsetCount, StructCount, PaddingCount, Hint));
                if (SectionNumber == Section4)
                    _savedSection4Entries.Add(new RelocationEntry((uint)pos, OffsetCount, StructCount, PaddingCount, Hint));
                if (SectionNumber == Section5)
                    _savedSection5Entries.Add(new RelocationEntry((uint)pos, OffsetCount, StructCount, PaddingCount, Hint));
            }
        }

        internal void SaveFileNameString(string Name, bool Relocate = false)
        {
            _fileName = Name;
            _ofsFileName = (uint)Position;
            Write(0);
        }

        /// <summary>
        /// Reserves space for the <see cref="Bfres.ResFile"/> string pool size and offset fields which are automatically
        /// filled later.
        /// </summary>
        [DebuggerStepThrough]
        internal void SaveFieldStringPool(bool Relocate = false)
        {
            _ofsStringPool = (uint)Position;
            Write(0L); //Offset
            Write(0); //Size
        }

        /// <summary>
        /// Reserves space for the <see cref="Bfres.ResFile"/> memory pool field which is automatically filled later.
        /// </summary>
        [DebuggerStepThrough]
        internal void SaveMemoryPoolPointer()
        {
            _savedMemoryPoolPointers.Add(Position);
            Write(0L);
        }

        /// <summary>
        /// Reserves space for the <see cref="Bfres.ResFile"/> memory pool field which is automatically filled later.
        /// </summary>
        [DebuggerStepThrough]
        internal void SaveBufferTotalSize()
        {
            _ofsTotalBufferSize = Position;
            Write(0);
        }

        

        /// <summary>
        /// Saves the Index buffer pointer to be used later in the relocation table
        /// </summary>
        [DebuggerStepThrough]
        internal void SaveIndexBufferPointer()
        {
            _ofsIndexBuffer = (uint)Position;
            Write(0L);
        }

        /// <summary>
        /// Saves the Vertex buffer pointer to be used later in the relocation table
        /// </summary>
        [DebuggerStepThrough]
        internal void SaveVertexBufferPointer()
        {
            _ofsVertexBuffer = (uint)Position;
        }

        /// <summary>
        /// Reserves space for the <see cref="Bfres.ResFile"/> memory pool field which is automatically filled later.
        /// </summary>
        [DebuggerStepThrough]
        internal void SaveRelocationTablePointerPointer()
        {
            _ofsRelocationTable = (uint)Position;
            Write(0);
        }

        /// <summary>
        /// Reserves space for an offset to the <paramref name="list"/> written later.
        /// </summary>
        /// <typeparam name="T">The type of the <see cref="IResData"/> elements.</typeparam>
        /// <param name="list">The <see cref="IList{T}"/> to save.</param>
        [DebuggerStepThrough]
        internal void SaveList<T>(IEnumerable<T> list, long ShiftPos = 0)
            where T : IResData, new()
        {
            long NewPos = Position;
            if (ShiftPos != 0)
            {
                NewPos = ShiftPos;
            }
            if (list?.Count() == 0 && ShiftPos != 0)
            {
                return;
            }
            if (list?.Count() == 0 && ShiftPos == 0)
            {
                Write(0L);
                return;
            }
            // The offset to the list is the offset to the first element.
            if (TryGetItemEntry(list.First(), ItemEntryType.ResData, out ItemEntry entry))
            {
                entry.Offsets.Add((uint)NewPos);
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
                        _savedItems.Add(new ItemEntry(element, ItemEntryType.ResData, (uint)NewPos, index: index));
                    }
                    else
                    {
                        // Add without offsets existing yet.
                        _savedItems.Add(new ItemEntry(element, ItemEntryType.ResData, index: index));
                    }
                    index++;
                }
            }

            if (ShiftPos == 0)
            {
                Write(UInt32.MaxValue);
                Write(0);
            }
        }

        /// <summary>
        /// Reserves space for an offset to the <paramref name="dict"/> written later.
        /// </summary>
        /// <typeparam name="T">The type of the <see cref="IResData"/> element values.</typeparam>
        /// <param name="dict">The <see cref="ResDict{T}"/> to save.</param>
        [DebuggerStepThrough]
        internal void SaveDict(ResDict dict, long ShiftPos = 0)
        {
            long NewPos = Position;
            if (ShiftPos != 0)
            {
                NewPos = ShiftPos;
            }

            if (dict?.Count == 0 && ShiftPos == 0)
            {
                Write(0L);
                return;
            }
            if (TryGetItemEntry(dict, ItemEntryType.Dict, out ItemEntry entry))
            {
                entry.Offsets.Add((uint)NewPos);
            }
            else
            {
                _savedItems.Add(new ItemEntry(dict, ItemEntryType.Dict, (uint)NewPos));
            }

            if (ShiftPos == 0)
            {
                Write(UInt32.MaxValue);
                Write(0);
            }
        }

        /// <summary>
        /// Reserves space for an offset to the <paramref name="data"/> written later with the
        /// <paramref name="callback"/>.
        /// </summary>
        /// <param name="data">The data to save.</param>
        /// <param name="callback">The <see cref="Action"/> to invoke to write the data.</param>
        [DebuggerStepThrough]
        internal void SaveCustom(object data, Action callback, long ShiftOffset = 0)
        {
            long NewPos = Position;
            if (ShiftOffset != 0)
            {
                NewPos = ShiftOffset;
            }

            if (data == null && ShiftOffset == 0)
            {
                Write(0L);
                return;
            }
            if (TryGetItemEntry(data, ItemEntryType.Custom, out ItemEntry entry))
            {
                entry.Offsets.Add((uint)NewPos);
            }
            else
            {
                _savedItems.Add(new ItemEntry(data, ItemEntryType.Custom, (uint)NewPos, callback: callback));
            }
            if (ShiftOffset == 0)
            {
                Write(UInt64.MaxValue);
            }
        }

        /// <summary>
        /// Reserves space for an offset to the <paramref name="str"/> written later in the string pool with the
        /// specified <paramref name="encoding"/>.
        /// </summary>
        /// <param name="str">The name to save.</param>
        /// <param name="encoding">The <see cref="Encoding"/> in which the name will be stored.</param>
        [DebuggerStepThrough]
        internal void SaveString(string str, Encoding encoding = null, bool Relocate = false)
        {
            if(str == null)
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

        /// <summary>
        /// Reserves space for offsets to the <paramref name="strings"/> written later in the string pool with the
        /// specified <paramref name="encoding"/>
        /// </summary>
        /// <param name="strings">The names to save.</param>
        /// <param name="encoding">The <see cref="Encoding"/> in which the names will be stored.</param>
        [DebuggerStepThrough]
        internal void SaveStrings(IEnumerable<string> strings, bool relocate = false, Encoding encoding = null)
        {
            if (relocate)
                SaveRelocateEntryToSection(Position, (uint)strings.ToList().Count, 1, 0, ResFileSaver.Section1, "SaveStrings");

            foreach (string str in strings)
            {
                SaveString(str, encoding);
            }
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
        /// Reserves space for an offset to the <paramref name="data"/> written later in the data block pool.
        /// </summary>
        /// <param name="data">The data to save.</param>
        /// <param name="alignment">The alignment to seek to before invoking the callback.</param>
        /// <param name="callback">The <see cref="Action"/> to invoke to write the data.</param>
        [DebuggerStepThrough]
        internal void SaveBlock(object data, int alignment, Action callback)
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
        /// Writes a BFRES signature consisting of 4 ASCII characters encoded as an <see cref="UInt32"/>.
        /// </summary>
        /// <param name="value">A valid signature.</param>
        internal void WriteSignature(string value)
        {
            Write(Encoding.ASCII.GetBytes(value));
        }

        // ---- METHODS (PRIVATE) --------------------------------------------------------------------------------------

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

        public void SaveEntries()
        {
            // Store all queued items. Iterate via index as subsequent calls append to the list.
            for (int i = 0; i < _savedItems.Count; i++)
            {
                if (_savedItems[i].Target != null)
                {
                    // Ignore if it has already been written (list or dict elements).
                    continue;
                }

                ItemEntry entry = _savedItems[i];

                Align(8);
                switch (entry.Type)
                {
                    case ItemEntryType.List:
                        IEnumerable<IResData> list = (IEnumerable<IResData>)entry.Data;
                        // Check if the first item has already been written by a previous dict.
                        if (TryGetItemEntry(list.First(), ItemEntryType.ResData, out ItemEntry firstElement))
                        {
                            entry.Target = firstElement.Target;
                        }
                        else
                        {
                            entry.Target = (uint)Position;
                            CurrentIndex = 0;
                            foreach (IResData element in list)
                            {
                                _savedItems.Add(new ItemEntry(element, ItemEntryType.ResData, target: (uint)Position,
                                    index: CurrentIndex));
                                element.Save(this);
                                CurrentIndex++;
                            }
                        }
                        break;

                    case ItemEntryType.Dict:
                    case ItemEntryType.ResData:
                        entry.Target = (uint)Position;
                        CurrentIndex = entry.Index;
                        ((IResData)entry.Data).Save(this);
                        break;

                    case ItemEntryType.Custom:
                        entry.Target = (uint)Position;
                        entry.Callback.Invoke();
                        break;
                }
            }
            WriteOffsets();

            _savedItems.Clear();
        }

        private void WriteMemoryPool()
        {
            long oldPos = Position;
            Align(ResFile.DataAlignment);

            WriteTotalBufferSize(Position); //Write the total buffer size after memory pool is aligned

            memoryPoolOffset = Position;
            byte[] memoryPool = new byte[288];
            Write(memoryPool);
            foreach (long ptr in _savedMemoryPoolPointers)
            {
                using (TemporarySeek(ptr, SeekOrigin.Begin))
                {
                    Write(memoryPoolOffset);
                }
            }
        }

        private void WriteTotalBufferSize(long BufferEndPos)
        {
            uint BufferSize = (uint)(BufferEndPos - bufferInfoOffset);
            using (TemporarySeek(_ofsTotalBufferSize, SeekOrigin.Begin))
            {
                Write(BufferSize);
            }        
        }

        uint SectionCount = 5;
        private void WriteRelocationTable()
        {
            uint relocationTableOffset = (uint)Position;
            WriteSignature("_RLT");
            _ofsEndOfBlock = (uint)Position;
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

            using (TemporarySeek(_ofsRelocationTable, SeekOrigin.Begin))
            {
                Write(relocationTableOffset);
            }
        }

        private void WriteIndexBuffer()
        {
            Align(ResFile.DataAlignment);
            bufferInfoOffset = Position;
            for (int i = 0; i < ResFile.BufferInfo.IndexBufferData.Length; i++)
            {
                if (Position % 8 != 0) Position = Position + (8 - (Position % 8));
                Write(ResFile.BufferInfo.IndexBufferData[i]);
            }

            //Goto the buffer info section that sets the index buffer offset
            using (TemporarySeek(_ofsIndexBuffer, SeekOrigin.Begin))
            {
                Write(bufferInfoOffset);
            }
        }
        private void WriteVertexBuffer()
        {
            SaveVertexBufferPointer();
            for (int i = 0; i < ResFile.BufferInfo.VertexBufferData.Length; i++)
            {
                if (Position % 8 != 0) Position = Position + (8 - (Position % 8));
                Write(ResFile.BufferInfo.VertexBufferData[i]);
            }
        }

        private void WriteStrings()
        {
            // Sort the strings ordinally.
            Dictionary<string, StringEntry> sorted = new Dictionary<string, StringEntry>();
            foreach (var str in ResFile.StringTable.Strings)
            {
                if (_savedStrings.ContainsKey(str) && !sorted.ContainsKey(str))
                    sorted.Add(str, _savedStrings[str]);
            }

            foreach (KeyValuePair<string, StringEntry> entry in _savedStrings)
            {
                if (!sorted.ContainsKey(entry.Key))
                    sorted.Add(entry.Key, entry.Value); //Add new strings to the end
            }

            Align(4);

            uint stringPoolStart = (uint)Position;

            WriteSignature("_STR");
            SaveHeaderBlock();
            Write(sorted.Count);

            uint stringPoolOffset = (uint)Position;

            foreach (KeyValuePair<string, StringEntry> entry in sorted)
            {
                if (entry.Key == _fileName)
                    _ofsFileNameString = (uint)Position;

                // Align and satisfy offsets.
                using (TemporarySeek())
                {
                    SatisfyOffsets(entry.Value.Offsets, (uint)Position);
                }
                Write((short)entry.Key.Length);

                // Write the name.
                Write(entry.Key, BinaryStringFormat.ZeroTerminated, entry.Value.Encoding ?? Encoding);
                Align(2);
            }

            BaseStream.SetLength(Position); // Workaround to make last alignment expand the file if nothing follows.
            Align(2);

            _ofsEndOfStringTable = (uint)Position;

            // Save string pool offset and size in main file header.
            uint stringPoolSize = (uint)(_ofsEndOfStringTable - stringPoolStart);
            if (_ofsStringPool != 0)
            {
                using (TemporarySeek(_ofsStringPool, SeekOrigin.Begin))
                {
                    Write(stringPoolOffset);
                    Write(0);
                    Write(stringPoolSize);
                }
            }
        
        }
        private void WriteHeaderBlock(uint Size, long Offset)
        {
            Write(Size);
            Write(Offset);
        }
        private void WriteBlocks()
        {
            int i = 0;
            foreach (KeyValuePair<object, BlockEntry> entry in _savedBlocks)
            {
                // Align and satisfy offsets.
                if (entry.Value.Alignment != 0) Align((int)entry.Value.Alignment);

                if (i == 0)
                    _ofsExternalFileBlock = (uint)Position;

                using (TemporarySeek())
                {
                    SatisfyOffsets(entry.Value.Offsets, (uint)Position);
                }

                // Write the data.
                entry.Value.Callback.Invoke();
                i++;
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
                Write((ulong)(target));
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
            internal int Alignment;
            internal Action Callback;

            internal BlockEntry(uint offset, int alignment, Action callback)
            {
                Offsets = new List<uint> { offset };
                Alignment = alignment;
                Callback = callback;
            }
        }
    }
}