using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using Syroot.NintenTools.NSW.Bfres.Core;

namespace Syroot.NintenTools.NSW.Bfres
{
    /// <summary>
    /// Represents an FSCN subfile in a <see cref="ResFile"/>, storing scene animations controlling camera, light and
    /// fog settings.
    /// </summary>
    [DebuggerDisplay(nameof(SceneAnim) + " {" + nameof(Name) + "}")]
    public class SceneAnim : IResData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SceneAnim"/> class.
        /// </summary>
        public SceneAnim()
        {
            Name = "";
            Path = "";
            CameraAnims = new List<CameraAnim>();
            LightAnims = new List<LightAnim>();
            FogAnims = new List<FogAnim>();
            CameraAnimDict = new ResDict();
            LightAnimDict = new ResDict();
            FogAnimDict = new ResDict();

            UserData = new List<UserData>();
            UserDataDict = new ResDict();
        }

        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const string _signature = "FSCN";

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
                loader.ImportSceneAnimations();
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
                loader.ImportSceneAnimations();
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
                saver.ExportSceneAnimation();
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
                saver.ExportSceneAnimation();
            }
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the name with which the instance can be referenced uniquely in <see cref="ResDict{SceneAnim}"/>
        /// instances.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the path of the file which originally supplied the data of this instance.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="CameraAnim"/> instances.
        /// </summary>
        public IList<CameraAnim> CameraAnims { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="LightAnim"/> instances.
        /// </summary>
        public IList<LightAnim> LightAnims { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="FogAnim"/> instances.
        /// </summary>
        public IList<FogAnim> FogAnims { get; set; }

        /// <summary>
        /// Gets or sets customly attached <see cref="UserData"/> instances.
        /// </summary>
        public IList<UserData> UserData { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="CameraAnim"/> instances.
        /// </summary>
        public ResDict CameraAnimDict { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="LightAnim"/> instances.
        /// </summary>
        public ResDict LightAnimDict { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="FogAnim"/> instances.
        /// </summary>
        public ResDict FogAnimDict { get; set; }

        /// <summary>
        /// Gets or sets customly attached <see cref="UserData"/> instances.
        /// </summary>
        public ResDict UserDataDict { get; set; }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        private uint UnknownFlags;
        void IResData.Load(ResFileLoader loader)
        {
            loader.CheckSignature(_signature);
            if (loader.ResFile.VersionMajor2 >= 9)
                UnknownFlags = loader.ReadUInt32(); 
            else
                loader.LoadHeaderBlock();

            Name                         = loader.LoadString();
            Path                         = loader.LoadString();
            long CameraAnimArrayOffset   = loader.ReadOffset(true);
            CameraAnimDict               = loader.LoadDict();
            long LightAnimArrayOffset    = loader.ReadOffset(true);
            LightAnimDict                = loader.LoadDict();
            long FogAnimArrayOffset      = loader.ReadOffset(true);
            FogAnimDict                  = loader.LoadDict();
            long UserDataOffset          = loader.ReadOffset(true);
            UserDataDict                 = loader.LoadDict();
            ushort numUserData           = loader.ReadUInt16();
            ushort numCameraAnim         = loader.ReadUInt16();
            ushort numLightAnim          = loader.ReadUInt16();
            ushort numFogAnim            = loader.ReadUInt16();

            CameraAnims = loader.LoadList<CameraAnim>(numCameraAnim, CameraAnimArrayOffset);
            LightAnims = loader.LoadList<LightAnim>(numLightAnim, LightAnimArrayOffset);
            FogAnims = loader.LoadList<FogAnim>(numFogAnim, FogAnimArrayOffset);
            UserData = loader.LoadList<UserData>(numUserData, UserDataOffset);
        }

        internal long PosCameraAnimArrayOffset;
        internal long PosCameraAnimDictOffset;
        internal long PosLightAnimArrayOffset;
        internal long PosLightAnimDictOffset;
        internal long PosFogAnimArrayOffset;
        internal long PosFogAnimDictOffset;
        internal long PosUserDataOffset;
        internal long PosUserDataDictOffset;

        void IResData.Save(ResFileSaver saver)
        {
            saver.WriteSignature(_signature);
            if (saver.ResFile.VersionMajor2 >= 9)
                saver.Write(UnknownFlags);
            else
                saver.SaveHeaderBlock();

            saver.SaveString(Name);
            saver.SaveString(Path);
            PosCameraAnimArrayOffset = saver.SaveOffset();
            PosCameraAnimDictOffset = saver.SaveOffset();
            PosLightAnimArrayOffset = saver.SaveOffset();
            PosLightAnimDictOffset = saver.SaveOffset();
            PosFogAnimArrayOffset = saver.SaveOffset();
            PosFogAnimDictOffset = saver.SaveOffset();
            PosUserDataOffset = saver.SaveOffset();
            PosUserDataDictOffset = saver.SaveOffset();
            saver.Write((ushort)UserData.Count);
            saver.Write((ushort)CameraAnims.Count);
            saver.Write((ushort)LightAnims.Count);
            saver.Write((ushort)FogAnims.Count);
        }
    }
}