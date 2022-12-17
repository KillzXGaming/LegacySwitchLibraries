using System.IO;
using System.ComponentModel;
using System.Collections.Generic;
using Syroot.Maths;
using Syroot.NintenTools.NSW.Bfres.Core;

namespace Syroot.NintenTools.NSW.Bfres
{
    /// <summary>
    /// Represents an FSKL section in a <see cref="Model"/> subfile, storing armature data.
    /// </summary>
    public class Skeleton : IResData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Skeleton"/> class.
        /// </summary>
        public Skeleton()
        {
            MatrixToBoneList = new List<ushort>();
            InverseModelMatrices = new List<Matrix3x4>();
            Bones = new List<Bone>();
            BoneDict = new ResDict();
            FlagsRotation = SkeletonFlagsRotation.EulerXYZ;
            FlagsScaling = SkeletonFlagsScaling.Maya;
        }

        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const string _signature = "FSKL";

        private const uint _flagsScalingMask = 0b00000000_00000000_00000011_00000000;
        private const uint _flagsRotationMask = 0b00000000_00000000_01110000_00000000;

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="Material"/> class from the given <paramref name="stream"/> which
        /// is optionally left open.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to load the data from.</param>
        /// <param name="leaveOpen"><c>true</c> to leave the stream open after reading, otherwise <c>false</c>.</param>
        public void Import(Stream stream, bool leaveOpen = false)
        {
            using (ResFileLoader loader = new ResFileLoader(this, stream, leaveOpen))
            {
                loader.ImportSkeleton();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResFile"/> class from the file with the given
        /// <paramref name="fileName"/>.
        /// </summary>
        /// <param name="fileName">The name of the file to load the data from.</param>
        public void Import(string fileName)
        {
            using (ResFileLoader loader = new ResFileLoader(this, fileName))
            {
                loader.ImportSkeleton();
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
                saver.ExportSkeleton();
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
                saver.ExportSkeleton();
            }
        }

        // ---- FIELDS -------------------------------------------------------------------------------------------------

        private uint _flags;

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        [Browsable(true)]
        [Category("Skeleton")]
        [DisplayName("Scaling")]
        public SkeletonFlagsScaling FlagsScaling
        {
            get { return (SkeletonFlagsScaling)(_flags & _flagsScalingMask); }
            set { _flags = _flags & ~_flagsScalingMask | (uint)value; }
        }

        /// <summary>
        /// Gets or sets the rotation method used to store bone rotations.
        /// </summary>
        [Browsable(true)]
        [Category("Skeleton")]
        [DisplayName("Rotation")]
        public SkeletonFlagsRotation FlagsRotation
        {
            get { return (SkeletonFlagsRotation)(_flags & _flagsRotationMask); }
            set { _flags = _flags & ~_flagsRotationMask | (uint)value; }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="Bone"/> names.
        /// </summary>
        [Browsable(false)]
        public ResDict BoneDict { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="Bone"/> instances forming the skeleton.
        /// </summary>
        [Browsable(false)]
        public IList<Bone> Bones { get; set; }

        [Browsable(false)]
        public IList<ushort> MatrixToBoneList { get; set; }

        public IList<ushort> GetSmoothIndices()
        {
            List<ushort> indices = new List<ushort>();
            foreach (Bone bone in Bones)
            {
                if (bone.SmoothMatrixIndex != -1)
                    indices.Add((ushort)bone.SmoothMatrixIndex);
            }
            return indices;
        }

        public IList<ushort> GetRigidIndices()
        {
            List<ushort> indices = new List<ushort>();
            foreach (Bone bone in Bones)
            {
                if (bone.RigidMatrixIndex != -1)
                    indices.Add((ushort)bone.RigidMatrixIndex);
            }
            return indices;
        }


        [Browsable(false)]
        public IList<Matrix3x4> InverseModelMatrices { get; set; }

        public ushort[] userIndices;

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
             loader.CheckSignature(_signature);
            if (loader.ResFile.VersionMajor2 >= 9)
                _flags = loader.ReadUInt32();
            else
                loader.LoadHeaderBlock();
            BoneDict                        = loader.LoadDict();           
            long BoneArrayOffset            = loader.ReadOffset();
            long MatrixToBoneListOffset     = loader.ReadOffset();
            long InverseModelMatricesOffset = loader.ReadOffset();

            if (loader.ResFile.VersionMajor2 == 8)
            {
                loader.Seek(16);
            }
            if (loader.ResFile.VersionMajor2 >= 9)
            {
                //Less padding?
                loader.Seek(8);
            }

            long userPointer                = loader.ReadInt64();
            if (loader.ResFile.VersionMajor2 < 9)
                _flags = loader.ReadUInt32();
            ushort numBone                  = loader.ReadUInt16();
            ushort numSmoothMatrix          = loader.ReadUInt16();
            ushort numRigidMatrix           = loader.ReadUInt16();
            loader.Seek(6);

            userIndices = loader.LoadCustom(() => loader.ReadUInt16s(numBone), userPointer);

            MatrixToBoneList = loader.LoadCustom(()     => loader.ReadUInt16s((numSmoothMatrix + numRigidMatrix)), MatrixToBoneListOffset);
            InverseModelMatrices = loader.LoadCustom(() => loader.ReadMatrix3x4s(numSmoothMatrix), InverseModelMatricesOffset);

            Bones = loader.LoadList<Bone>(numBone, BoneArrayOffset);
        }

        internal void WriteMatrices(ResFileSaver saver)
        {
            saver.Write(InverseModelMatrices);
        }


        internal long PosBoneDictOffset;
        internal long PosBoneArrayOffset;
        internal long PosMatrixToBoneListOffset;
        internal long PosInverseModelMatricesOffset;
        internal long PosUserPointer;

        void IResData.Save(ResFileSaver saver)
        {
            if (InverseModelMatrices == null)
                InverseModelMatrices = new List<Matrix3x4>();
            if (MatrixToBoneList == null)
                MatrixToBoneList = new List<ushort>(); 

            saver.WriteSignature(_signature);
            if (saver.ResFile.VersionMajor2 >= 9)
                saver.Write(_flags);
            else
                saver.SaveHeaderBlock();

            saver.SaveRelocateEntryToSection(saver.Position, 4, 1, 0, ResFileSaver.Section1, "FSKL"); //      <------------ Entry Set
            PosBoneDictOffset = saver.SaveOffset();
            PosBoneArrayOffset = saver.SaveOffset();
            PosMatrixToBoneListOffset = saver.SaveOffset();
            PosInverseModelMatricesOffset = saver.SaveOffset();
            if (saver.ResFile.VersionMajor2 == 8)
            {
                saver.Seek(16);
            }
            if (saver.ResFile.VersionMajor2 >= 9)
            {
                saver.Seek(8);
            }
            PosUserPointer = saver.SaveOffset();// UserPointer

            if (saver.ResFile.VersionMajor2 < 9)
                saver.Write(_flags);

            saver.Write((ushort)BoneDict.Count);
            saver.Write((ushort)InverseModelMatrices.Count); // NumSmoothMatrix
            saver.Write((ushort)(MatrixToBoneList.Count - InverseModelMatrices.Count)); // NumRigidMatrix

            if (saver.ResFile.VersionMajor2 >= 9)
                saver.Seek(2);
            else
                saver.Seek(6);
        }
    }

    public enum SkeletonFlagsScaling : uint
    {
        None,
        Standard = 1 << 8,
        Maya = 2 << 8,
        Softimage = 3 << 8
    }

    /// <summary>
    /// Represents the rotation method used to store bone rotations.
    /// </summary>
    public enum SkeletonFlagsRotation : uint
    {
        /// <summary>
        /// A quaternion represents the rotation.
        /// </summary>
        Quaternion,

        /// <summary>
        /// A <see cref="Vector3F"/> represents the Euler rotation in XYZ order.
        /// </summary>
        EulerXYZ = 1 << 12
    }
}
