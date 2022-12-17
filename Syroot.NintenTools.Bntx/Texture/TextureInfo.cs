using System.IO;
using System.Diagnostics;
using Syroot.NintenTools.NSW.Bntx.Core;
using Syroot.NintenTools.NSW.Bntx.GFX;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Runtime.InteropServices;

namespace Syroot.NintenTools.NSW.Bntx
{
    /// <summary>
    /// Represents an FMDL subfile in a <see cref="BntxFile"/>, storing multi-dimensional texture data.
    /// </summary>
    [DebuggerDisplay(nameof(Texture) + " {" + nameof(Name) + "}")]
    public class Texture : IResData
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const string _signature = "BRTI";


        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="Material"/> class from the given <paramref name="stream"/> which
        /// is optionally left open.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to load the data from.</param>
        /// <param name="leaveOpen"><c>true</c> to leave the stream open after reading, otherwise <c>false</c>.</param>
        public void Import(Stream stream, bool leaveOpen = false)
        {
            using (BntxFileLoader loader = new BntxFileLoader(this, stream, leaveOpen))
            {
                loader.ImportTexture();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResFile"/> class from the file with the given
        /// <paramref name="fileName"/>.
        /// </summary>
        /// <param name="fileName">The name of the file to load the data from.</param>
        public void Import(string fileName)
        {
            using (BntxFileLoader loader = new BntxFileLoader(this, fileName))
            {
                loader.ImportTexture();
            }
        }

        /// <summary>
        /// Saves the contents in the given <paramref name="stream"/> and optionally leaves it open
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to save the contents into.</param>
        /// <param name="leaveOpen"><c>true</c> to leave the stream open after writing, otherwise <c>false</c>.</param>
        public void Export(Stream stream, BntxFile BntxFile, bool leaveOpen = false)
        {
            using (BntxFileSaver saver = new BntxFileSaver(this, BntxFile, stream, leaveOpen))
            {
                saver.ExportTexture();
            }
        }

        /// <summary>
        /// Saves the contents in the file with the given <paramref name="fileName"/>.
        /// </summary>
        /// <param name="fileName">The name of the file to save the contents into.</param>
        public void Export(string fileName, BntxFile BntxFile)
        {
            using (BntxFileSaver saver = new BntxFileSaver(this, BntxFile, fileName))
            {
                saver.ExportTexture();
            }
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the source channel to map to the R (red) channel.
        /// </summary>
        [Browsable(true)]
        [Description("The source channel to map to the R (red) channel.")]
        [Category("Channels")]
        [DisplayName("Red Channel")]
        public ChannelType ChannelRed { get; set; }

        /// <summary>
        /// Gets or sets the source channel to map to the G (green) channel.
        /// </summary>
        [Browsable(true)]
        [Description("The source channel to map to the G (green) channel.")]
        [Category("Channels")]
        [DisplayName("Green Channel")]
        public ChannelType ChannelGreen { get; set; }

        /// <summary>
        /// Gets or sets the source channel to map to the B (blue) channel.
        /// </summary>
        [Browsable(true)]
        [Description("The source channel to map to the B (blue) channel.")]
        [Category("Channels")]
        [DisplayName("Blue Channel")]
        public ChannelType ChannelBlue { get; set; }

        /// <summary>
        /// Gets or sets the source channel to map to the A (alpha) channel.
        /// </summary>
        [Browsable(true)]
        [Description("The source channel to map to the A (alpha) channel.")]
        [Category("Channels")]
        [DisplayName("Alpha Channel")]
        public ChannelType ChannelAlpha { get; set; }

        /// <summary>
        /// Gets or sets the width of the texture.
        /// </summary>
        [Browsable(true)]
        [ReadOnly(true)]
        [Description("Width of the image")]
        [Category("Image Info")]
        [DisplayName("Width")]
        public uint Width { get; set; }

        /// <summary>
        /// Gets or sets the height of the texture.
        /// </summary>
        [Browsable(true)]
        [ReadOnly(true)]
        [Description("Height of the image")]
        [Category("Image Info")]
        [DisplayName("Height")]
        public uint Height { get; set; }

        /// <summary>
        /// Gets or sets the number of mipmaps stored in the <see cref="MipData"/>.
        /// </summary>
        [Browsable(true)]
        [ReadOnly(true)]
        [Description("Number of mip maps")]
        [Category("Image Info")]
        [DisplayName("Mip Count")]
        public uint MipCount { get; set; }

        /// <summary>
        /// Gets or sets the desired texture data buffer format.
        /// </summary>
        [Browsable(true)]
        [ReadOnly(true)]
        [Description("Format")]
        [Category("Image Info")]
        [DisplayName("Format")]
        public SurfaceFormat Format { get; set; }



        [Browsable(true)]
        [Description("Change the format to enable or disable SRGB")]
        [Category("Image Info")]
        [DisplayName("Use SRGB")]
        public bool UseSRGB
        {
            get
            {
                byte DataType = (byte)((int)Format >> 0 & 0xff);
                if (DataType == 0x06)
                    return true;
                else
                    return false;
            }
            set
            {
                var format = (SurfaceFormat)(((int)Format >> 8) & 0xff);

                if (value == true)
                {
                    Format = (SurfaceFormat)((int)format << 8 | (int)0x06 << 0);
                }
                else
                {
                    Format = (SurfaceFormat)((int)format << 8 | (int)0x01 << 0);
                }
            }
        }

        /// <summary>
        /// Gets or sets the name with which the instance can be referenced uniquely in <see cref="ResDict{Texture}"/>
        /// instances.
        /// </summary>
        [Browsable(true)]
        [Description("Name")]
        [Category("Image Info")]
        [DisplayName("Name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the path of the file which originally supplied the data of this instance.
        /// </summary>
        [Browsable(true)]
        [Description("The path the file was originally located.")]
        [Category("Image Info")]
        [DisplayName("Path")]
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the depth of the texture.
        /// </summary>
        [Browsable(true)]
        [ReadOnly(true)]
        [Description("Depth")]
        [DisplayName("Depth")]
        public uint Depth { get; set; }

        /// <summary>
        /// Gets or sets the tiling mode.
        /// </summary>
        [Browsable(true)]
        [ReadOnly(true)]
        [Description("Tiling mode")]
        [DisplayName("Tile Mode")]
        public TileMode TileMode { get; set; }


        /// <summary>
        /// Gets or sets the swizzling value.
        /// </summary>
        [Browsable(true)]
        [Description("Swizzle")]
        [DisplayName("Swizzle")]
        public uint Swizzle { get; set; }

        /// <summary>
        /// Gets or sets the swizzling alignment.
        /// </summary>
        [Browsable(true)]
        [ReadOnly(true)]
        [Description("Alignment")]
        [DisplayName("Alignment")]
        public int Alignment { get; set; }

        /// <summary>
        /// Gets or sets the pixel swizzling stride.
        /// </summary>
        [Browsable(true)]
        [ReadOnly(true)]
        [Description("The pixel swizzling stride")]
        [DisplayName("Pitch")]
        public uint Pitch { get; set; }

        /// <summary>
        /// Gets or sets the dims of the texture.
        /// </summary>
        [Browsable(true)]
        [ReadOnly(true)]
        [Description("Dims of the texture")]
        [DisplayName("Dims")]
        public Dim Dim { get; set; }

        /// <summary>
        /// Gets or sets the shape of the texture.
        /// </summary>
        [Browsable(true)]
        [ReadOnly(true)]
        [Description("Shape of texture")]
        [DisplayName("Surface Shape")]
        public SurfaceDim SurfaceDim { get; set; }

        /// <summary>
        /// Gets or sets the offsets in the <see cref="MipData"/> array to the data of the mipmap level corresponding
        /// to the array index.
        /// </summary>
        [Browsable(false)]
        public long[] MipOffsets { get; set; }

        /// <summary>
        /// The raw bytes of texture data stored for each mip map
        /// </summary>
        [Browsable(false)]
        public List<List<byte[]>> TextureData { get; set; }

        [Browsable(false)]
        public uint textureLayout { get; set; }

        [Browsable(false)]
        public uint textureLayout2 { get; set; }

        [Description("GPU access flags")]
        [Category("Image Info")]
        [DisplayName("Access Flags")]
        public AccessFlags AccessFlags { get; set; }

        [Browsable(false)]
        public uint[] Regs { get; set; }

        [Browsable(false)]
        public uint ArrayLength { get; set; }

        /// <summary>
        /// Gets or sets info flags
        /// </summary>
        [Browsable(true)]
        [ReadOnly(true)]
        [Description("Flags")]
        [DisplayName("Flags")]
        public byte Flags { get; set; }


        /// <summary>
        /// Gets or sets the image size
        /// </summary>
        [Browsable(true)]
        [ReadOnly(true)]
        [Description("size of image")]
        [DisplayName("Image Size")]
        public uint ImageSize { get; set; }

        /// <summary>
        /// Gets or sets sample amount
        /// </summary>
        [Browsable(true)]
        [ReadOnly(true)]
        [Description("Sample count")]
        [DisplayName("Sample Count")]
        public uint SampleCount { get; set; }

        [Browsable(false)]
        public ResDict UserDataDict { get; set; }

        [Browsable(false)]
        public IList<UserData> UserData { get; set; }

        [Browsable(false)]
        public int ReadTextureLayout { get; set; }
        [Browsable(false)]
        public int sparseBinding { get; set; }
        [Browsable(false)]
        public int sparseResidency { get; set; }
        [Browsable(false)]
        public uint BlockHeightLog2 { get; set; }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        int GetTotalSize()
        {
            return TextureData.Sum(o => o[0].Length);
        }

        void IResData.Load(BntxFileLoader loader)
        {
            loader.CheckSignature(_signature);
            loader.LoadHeaderBlock();
            Flags = loader.ReadByte();
            Dim = loader.ReadEnum<Dim>(true);
            TileMode = loader.ReadEnum<TileMode>(true);
            Swizzle = loader.ReadUInt16();
            MipCount = loader.ReadUInt16();
            SampleCount = loader.ReadUInt32();
            Format = loader.ReadEnum<SurfaceFormat>(true);

            AccessFlags = loader.ReadEnum<AccessFlags>(false);
            Width = loader.ReadUInt32();
            Height = loader.ReadUInt32();
            Depth = loader.ReadUInt32();
            ArrayLength = loader.ReadUInt32();
            textureLayout = loader.ReadUInt32();
            textureLayout2 = loader.ReadUInt32();
            byte[] reserved = loader.ReadBytes(20);
            ImageSize = loader.ReadUInt32();

            if (ImageSize == 0)
                throw new System.Exception("Empty image size!");

            Alignment = loader.ReadInt32();
            uint ChannelType = loader.ReadUInt32();
            SurfaceDim = loader.ReadEnum<SurfaceDim>(true);
            Name = loader.LoadString();
            long ParentOffset = loader.ReadInt64();
            long PtrOffset = loader.ReadInt64();
            long UserDataOffset = loader.ReadInt64();
            long TexPtr = loader.ReadInt64();
            long TexView = loader.ReadInt64();
            long descSlotDataOffset = loader.ReadInt64();
            UserDataDict = loader.LoadDict();

            UserData = loader.LoadList<UserData>(UserDataDict.Count, UserDataOffset);
            MipOffsets = loader.LoadCustom(() => loader.ReadInt64s((int)MipCount), PtrOffset);

            ChannelRed = (ChannelType)((ChannelType >> 0) & 0xff);
            ChannelGreen = (ChannelType)((ChannelType >> 8) & 0xff);
            ChannelBlue = (ChannelType)((ChannelType >> 16) & 0xff);
            ChannelAlpha = (ChannelType)((ChannelType >> 24) & 0xff);
            TextureData = new List<List<byte[]>>();

            ReadTextureLayout = (int)Flags & 1;
            sparseBinding = (int)Flags >> 1;
            sparseResidency = (int)Flags >> 2;
            BlockHeightLog2 = textureLayout & 7;

            int ArrayOffset = 0;
            for (int a = 0; a < ArrayLength; a++)
            {
                List<byte[]> mips = new List<byte[]>();
                for (int i = 0; i < MipCount; i++)
                {
                    int size = (int)((MipOffsets[0] + ImageSize - MipOffsets[i]) / ArrayLength);
                    using (loader.TemporarySeek(ArrayOffset + MipOffsets[i], System.IO.SeekOrigin.Begin))
                    {
                        mips.Add(loader.ReadBytes(size));
                    }
                    if (mips[i].Length == 0)
                        throw new System.Exception($"Empty mip size! Texture {Name} ImageSize {ImageSize} mips level {i} sizee {size} ArrayLength {ArrayLength}");
                }
                TextureData.Add(mips);

                ArrayOffset += mips[0].Length;
            }

            int mip = 0;
            long StartMip = MipOffsets[0];
            foreach (long offset in MipOffsets)
                MipOffsets[mip++] = offset - StartMip;
        }
        internal long PosUserDataOffset;
        internal long PosUserDataDictOffset;

        void IResData.Save(BntxFileSaver saver)
        {
            int Channels = (int)ChannelAlpha << 24 | (int)ChannelBlue << 16 | (int)ChannelGreen << 8 | (int)ChannelRed;

            if (ReadTextureLayout != 1)
                textureLayout = 0;
            else if (saver.BntxFile.VersionMajor2 == 4 && saver.BntxFile.VersionMinor >= 1)
                textureLayout = (uint)BlockHeightLog2;
            else
                textureLayout = (uint)(sparseResidency << 5 | sparseBinding << 4 | (int)BlockHeightLog2);

            Console.WriteLine($"sparseResidency {sparseResidency} sparseBinding {sparseBinding} BlockHeightLog2 {BlockHeightLog2}");

            Flags = (byte)(sparseResidency << 2 | sparseBinding << 1 | (int)ReadTextureLayout);

            saver.WriteSignature(_signature);
            saver.SaveHeaderBlock();
            long TexturePos = saver.Position;
            saver.Write((byte)Flags);
            saver.Write(Dim, true);
            saver.Write(TileMode, true);
            saver.Write((ushort)Swizzle);
            saver.Write((ushort)TextureData[0].Count);
            saver.Write(SampleCount);
            saver.Write(Format, true);
            saver.Write(AccessFlags, false);
            saver.Write(Width);
            saver.Write(Height);
            saver.Write(Depth);
            saver.Write(TextureData.Count);
            saver.Write(textureLayout);
            saver.Write(textureLayout2);
            saver.Seek(20); //reserved
            saver.Write(GetTotalSize());
            saver.Write(Alignment);
            saver.Write((uint)Channels);
            saver.Write(SurfaceDim, true);
            saver.SaveRelocateEntryToSection(saver.Position, 3, 1, 0, BntxFileSaver.Section1, "Texture Info"); //      <------------ Entry Set
            saver.SaveString(Name);
            saver.Write((long)0x20); //ParentOffset
            long PosDataOffsets = saver.SaveOffset();
            saver.SaveRelocateEntryToSection(saver.Position, 1, 1, 0, BntxFileSaver.Section1, "User Data"); //      <------------ Entry Set
            PosUserDataOffset = saver.SaveOffset();
            saver.SaveRelocateEntryToSection(saver.Position, 2, 1, 0, BntxFileSaver.Section1, "Texture Info"); //      <------------ Entry Set
            saver.Write(TexturePos + 0x90); //TexPtr
            saver.Write(TexturePos + 0x190); //TexView
            saver.Write(0L); //descSlotDataOffset
            saver.SaveRelocateEntryToSection(saver.Position, 1, 1, 0, BntxFileSaver.Section1, "User Data"); //      <------------ Entry Set
            PosUserDataDictOffset = saver.SaveOffset();// userDictOffset

            saver.Write(new byte[512]);

            saver.Align(8);
            long _ofsDataOffsets = saver.Position;
            saver.SaveRelocateEntryToSection(_ofsDataOffsets, (uint)TextureData[0].Count, 1, 0, BntxFileSaver.Section2, "TextureBlocks");

            foreach (long mipmap in MipOffsets)
            {
                saver.SaveMipMapOffsets();
            }

            if (UserData.Count > 0) {
                saver.SaveUserData(UserData, PosUserDataOffset);
            }
            if (UserData.Count > 0)
            {
                saver.SaveUserDataData(UserData);
                saver.Align(8);
                saver.WriteOffset(PosUserDataDictOffset);
                ((IResData)UserDataDict).Save(saver);
                saver.Align(8);
            }

            using (saver.TemporarySeek(PosDataOffsets, System.IO.SeekOrigin.Begin))
            {
                saver.Write(_ofsDataOffsets);
            }
        }
    }
}