using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using Assets_Editor;
using Efundies;
using Tibia.Protobuf.Appearances;
using static Assets_Editor.ItemManager;

namespace Assets_Editor
{
    public static class OTB
    {
        public enum SpecialChar : byte
        {
            NodeStart = 0xFE,
            NodeEnd = 0xFF,
            EscapeChar = 0xFD,
        }
        public enum RootAttribute
        {
            Version = 0x01
        }
        
        public enum TileStackOrder : byte
        {
            None = 0,
            Border = 1,
            Bottom = 2,
            Top = 3
        }
        public class OtbVersionInfo
        {
            #region Public Properties

            public uint MajorVersion { get; set; }

            public uint MinorVersion { get; set; }

            public uint BuildNumber { get; set; }

            public string CSDVersion { get; set; }

            #endregion
        }

        public static bool GenerateOTB(OtbVersionInfo version, Appearances appearances, string path)
        {
            try
            {
                using (BinaryTreeWriter writer = new BinaryTreeWriter(path))
                {
                    writer.WriteUInt32(0, false); // version, always 0

                    writer.CreateNode(0); // root node
                    writer.WriteUInt32(0, true); // flags, unused for root node


                    MemoryStream ms = new MemoryStream();
                    BinaryWriter property = new BinaryWriter(ms);
                    property.Write(version.MajorVersion);
                    property.Write(version.MinorVersion);
                    property.Write(version.BuildNumber);
                    byte[] CSDVersion = Encoding.ASCII.GetBytes(version.CSDVersion);
                    Array.Resize(ref CSDVersion, 128);
                    property.Write(CSDVersion);

                    writer.WriteProp(RootAttribute.Version, property);
                    int CurrentId = 0;
                    //foreach (var item in appearances.Object)
                    for (int i = 100; i <= appearances.Object[^1].Id; i++)
                    {
                        Appearance item;
                        if (i == appearances.Object[CurrentId].Id)
                        {
                            item = appearances.Object[CurrentId];
                            CurrentId++;
                        }
                        else
                        {
                            item = new Appearance();
                            item.Flags = new AppearanceFlags();
                        }

                        List<ServerItemAttribute> saveAttributeList = new List<ServerItemAttribute>();
                        saveAttributeList.Add(ServerItemAttribute.ServerID);
                        saveAttributeList.Add(ServerItemAttribute.ClientID);
                        saveAttributeList.Add(ServerItemAttribute.SpriteHash);

                        if (item.Flags.Automap != null && item.Flags.Automap.HasColor)
                        {
                            saveAttributeList.Add(ServerItemAttribute.MinimapColor);
                        }

                        if (item.Flags.WriteOnce != null && item.Flags.WriteOnce.HasMaxTextLengthOnce)
                        {
                            saveAttributeList.Add(ServerItemAttribute.MaxReadWriteChars);
                        }

                        if (item.Flags.Write != null && item.Flags.Write.HasMaxTextLength)
                        {
                            saveAttributeList.Add(ServerItemAttribute.MaxReadChars);
                        }

                        if (item.Flags.Light != null && item.Flags.Light.HasColor)
                        {
                            saveAttributeList.Add(ServerItemAttribute.Light);
                        }

                        if (item.Flags.Bank != null && item.Flags.Bank.HasWaypoints)
                        {
                            saveAttributeList.Add(ServerItemAttribute.GroundSpeed);
                        }

                        if (item.Flags.HasClip || item.Flags.HasBottom || item.Flags.HasTop)
                        {
                            saveAttributeList.Add(ServerItemAttribute.StackOrder);
                        }

                        if (item.Flags.Market != null && item.Flags.Market.HasTradeAsObjectId)
                        {
                            saveAttributeList.Add(ServerItemAttribute.TradeAs);
                        }

                        if (item.HasName && !string.IsNullOrEmpty(item.Name))
                        {
                            saveAttributeList.Add(ServerItemAttribute.Name);
                        }

                        if (item.Flags.HasContainer)
                            writer.CreateNode((byte)ServerItemGroup.Container);
                        else if (item.Flags.HasLiquidcontainer)
                            writer.CreateNode((byte)ServerItemGroup.Fluid);
                        else if (item.Flags.Bank != null && item.Flags.Bank.HasWaypoints)
                            writer.CreateNode((byte)ServerItemGroup.Ground);
                        else if (item.Flags.HasLiquidpool)
                            writer.CreateNode((byte)ServerItemGroup.Splash);
                        else
                            writer.CreateNode((byte)ServerItemGroup.None);

                        uint flags = 0;

                        if (item.Flags.HasUnpass)
                        {
                            flags |= (uint)ServerItemFlag.Unpassable;
                        }

                        if (item.Flags.Unsight)
                        {
                            flags |= (uint)ServerItemFlag.BlockMissiles;
                        }

                        if (item.Flags.HasAvoid)
                        {
                            flags |= (uint)ServerItemFlag.BlockPathfinder;
                        }

                        if (item.Flags.Height != null && item.Flags.Height.HasElevation)
                        {
                            flags |= (uint)ServerItemFlag.HasElevation;
                        }

                        if (item.Flags.HasForceuse)
                        {
                            flags |= (uint)ServerItemFlag.ForceUse;
                        }

                        if (item.Flags.HasMultiuse)
                        {
                            flags |= (uint)ServerItemFlag.MultiUse;
                        }

                        if (item.Flags.HasTake)
                        {
                            flags |= (uint)ServerItemFlag.Pickupable;
                        }

                        if (!item.Flags.HasUnmove)
                        {
                            flags |= (uint)ServerItemFlag.Movable;
                        }

                        if (item.Flags.HasCumulative)
                        {
                            flags |= (uint)ServerItemFlag.Stackable;
                        }

                        if (item.Flags.HasClip || item.Flags.HasBottom || item.Flags.HasTop)
                        {
                            flags |= (uint)ServerItemFlag.StackOrder;
                        }

                        if (item.Flags.Write != null || item.Flags.WriteOnce != null)
                        {
                            flags |= (uint)ServerItemFlag.Readable;
                        }

                        if (item.Flags.HasRotate)
                        {
                            flags |= (uint)ServerItemFlag.Rotatable;
                        }

                        if (item.Flags.HasHang)
                        {
                            flags |= (uint)ServerItemFlag.Hangable;
                        }

                        if (item.Flags.HasHookSouth)
                        {
                            flags |= (uint)ServerItemFlag.HookSouth;
                        }

                        if (item.Flags.HasHookEast)
                        {
                            flags |= (uint)ServerItemFlag.HookEast;
                        }

                        if (item.Flags.IgnoreLook)
                        {
                            flags |= (uint)ServerItemFlag.IgnoreLook;
                        }

                        if (item.Flags.HasAnimateAlways)
                        {
                            flags |= (uint)ServerItemFlag.IsAnimation;
                        }

                        if (item.Flags.Fullbank)
                        {
                            flags |= (uint)ServerItemFlag.FullGround;
                        }

                        writer.WriteUInt32(flags, true);


                        foreach (ServerItemAttribute attribute in saveAttributeList)
                        {
                            switch (attribute)
                            {
                                case ServerItemAttribute.ServerID:
                                    property.Write((ushort)i);
                                    writer.WriteProp(ServerItemAttribute.ServerID, property);
                                    break;

                                case ServerItemAttribute.TradeAs:
                                    property.Write((ushort)item.Flags.Market.TradeAsObjectId);
                                    writer.WriteProp(ServerItemAttribute.TradeAs, property);
                                    break;

                                case ServerItemAttribute.ClientID:
                                    property.Write((ushort)item.Id);
                                    writer.WriteProp(ServerItemAttribute.ClientID, property);
                                    break;

                                case ServerItemAttribute.GroundSpeed:
                                    property.Write((ushort)item.Flags.Bank.Waypoints);
                                    writer.WriteProp(ServerItemAttribute.GroundSpeed, property);
                                    break;

                                case ServerItemAttribute.Name:
                                    property.Write(item.Name.ToCharArray());
                                    writer.WriteProp(ServerItemAttribute.Name, property);
                                    break;

                                case ServerItemAttribute.SpriteHash:
                                {
                                    if (item.FrameGroup.Count > 0)
                                        property.Write(GenerateItemSpriteHash(item));
                                    else
                                        property.Write(new byte[16]);

                                    writer.WriteProp(ServerItemAttribute.SpriteHash, property);
                                    break;
                                }
                                case ServerItemAttribute.MinimapColor:
                                    property.Write((ushort)item.Flags.Automap.Color);
                                    writer.WriteProp(ServerItemAttribute.MinimapColor, property);
                                    break;

                                case ServerItemAttribute.MaxReadWriteChars:
                                    property.Write((ushort)item.Flags.WriteOnce.MaxTextLengthOnce);
                                    writer.WriteProp(ServerItemAttribute.MaxReadWriteChars, property);
                                    break;

                                case ServerItemAttribute.MaxReadChars:
                                    property.Write((ushort)item.Flags.Write.MaxTextLength);
                                    writer.WriteProp(ServerItemAttribute.MaxReadChars, property);
                                    break;

                                case ServerItemAttribute.Light:
                                    property.Write((ushort)item.Flags.Light.Brightness);
                                    property.Write((ushort)item.Flags.Light.Color);
                                    writer.WriteProp(ServerItemAttribute.Light, property);
                                    break;

                                case ServerItemAttribute.StackOrder:
                                {
                                    if (item.Flags.HasClip)
                                        property.Write((byte)TileStackOrder.Border);
                                    else if (item.Flags.HasBottom)
                                        property.Write((byte)TileStackOrder.Bottom);
                                    else if (item.Flags.HasTop)
                                        property.Write((byte)TileStackOrder.Top);

                                    writer.WriteProp(ServerItemAttribute.StackOrder, property);
                                    break;
                                }

                            }
                        }

                        writer.CloseNode();
                    }

                    writer.CloseNode();
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static byte[] GenerateItemSpriteHash(Appearance appearance)
        {
            MD5 md5 = MD5.Create();
            MemoryStream stream = new MemoryStream();
            byte[] rgbaData = new byte[4096];
            FrameGroup frameGroup = appearance.FrameGroup[0];

            for (int l = 0; l < frameGroup.SpriteInfo.PatternLayers; l++)
            {
                for (int h =  0; h < (int)(frameGroup.SpriteInfo.PatternHeight); h++)
                {
                    for (int w =  0; w < (int)(frameGroup.SpriteInfo.PatternWidth); w++)
                    {
                        int index = (int)(w + h * frameGroup.SpriteInfo.PatternWidth + l * frameGroup.SpriteInfo.PatternWidth * frameGroup.SpriteInfo.PatternHeight);
                        int spriteId = (int)frameGroup.SpriteInfo.SpriteId[index];
                        using Bitmap target = new Bitmap(MainWindow.MainSprStorage.getSpriteStream((uint)spriteId));
                        target.RotateFlip(RotateFlipType.RotateNoneFlipY);
                        var lockedBitmap = new LockBitmap(target);
                        lockedBitmap.LockBits();
                        for (int y = 0; y < target.Height; y++)
                        {
                            for (int x = 0; x < target.Width; x++)
                            {
                                Color c = lockedBitmap.GetPixel(x, y);
                                if (c == Color.FromArgb(0, 0, 0, 0))
                                    c = Color.FromArgb(0, 17, 17, 17);
                                rgbaData[128 * y + x * 4 + 0] = c.B;
                                rgbaData[128 * y + x * 4 + 1] = c.G;
                                rgbaData[128 * y + x * 4 + 2] = c.R;
                                rgbaData[128 * y + x * 4 + 3] = 0;
                            }
                        }
                        lockedBitmap.UnlockBits();
                        stream.Write(rgbaData, 0, 4096);
                    }
                }
            }


            stream.Position = 0;
            byte[] spriteHash = md5.ComputeHash(stream);
            return spriteHash;
        }
    }
}
