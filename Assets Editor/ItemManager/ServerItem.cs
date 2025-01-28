using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Assets_Editor.OTB;
using Tibia.Protobuf.Appearances;

namespace Assets_Editor
{
    public static class ItemManager
    {
        public enum ServerItemAttribute : byte
        {
            ServerID = 0x10,
            ClientID = 0x11,
            Name = 0x12,
            Description = 0x13,
            GroundSpeed = 0x14,
            SpriteHash = 0x20,
            MinimapColor = 0x21,
            MaxReadWriteChars = 0x22,
            MaxReadChars = 0x23,
            Light = 0x2A,
            StackOrder = 0x2B,
            TradeAs = 0x2D,
            Article = 0x2F,

        }
        public enum ServerItemType : byte
        {
            None = 0,
            Ground = 1,
            Container = 2,
            Fluid = 3,
            Splash = 4,
            Deprecated = 5,
            Podium = 6,
            Key = 7,
            MagicField = 8,
            Depot = 9,
            Mailbox = 10,
            Trashholder = 11,
            Teleport = 12,
            Door = 13,
            Bed = 14,
            Rune = 15
        }

        public enum ServerItemGroup : byte
        {
            None = 0,
            Ground = 1,
            Container = 2,
            Weapon = 3,
            Ammunition = 4,
            Armor = 5,
            Changes = 6,
            Teleport = 7,
            MagicField = 8,
            Writable = 9,
            Key = 10,
            Splash = 11,
            Fluid = 12,
            Door = 13,
            Deprecated = 14,
            Podium = 15
        }

        public enum ServerItemSlot : byte
        {
            None = 0,
            Head = 1,
            Body = 2,
            Legs = 3,
            Feet = 4,
            Backpack = 5,
            TwoHanded = 6,
            RightHand = 7,
            LeftHand = 8,
            Ring = 9,
            Necklace = 10,
            Ammo = 11,
            Hand = 12
        }

        public enum ServerItemWeaponType : byte
        {
            None = 0,
            Sword = 1,
            Club = 2,
            Axe = 3,
            Shield = 4,
            Distance = 5,
            Wand = 6,
            Ammunition = 7,
            Quiver = 8
        }
        public enum ServerItemFlag
        {
            None = 0,
            Unpassable = 1 << 0,
            BlockMissiles = 1 << 1,
            BlockPathfinder = 1 << 2,
            HasElevation = 1 << 3,
            MultiUse = 1 << 4,
            Pickupable = 1 << 5,
            Movable = 1 << 6,
            Stackable = 1 << 7,
            FloorChangeDown = 1 << 8,
            FloorChangeNorth = 1 << 9,
            FloorChangeEast = 1 << 10,
            FloorChangeSouth = 1 << 11,
            FloorChangeWest = 1 << 12,
            StackOrder = 1 << 13,
            Readable = 1 << 14,
            Rotatable = 1 << 15,
            Hangable = 1 << 16,
            HookEast = 1 << 17,
            HookSouth = 1 << 18,
            CanNotDecay = 1 << 19,
            AllowDistanceRead = 1 << 20,
            Unused = 1 << 21,
            ClientCharges = 1 << 22,
            IgnoreLook = 1 << 23,
            IsAnimation = 1 << 24,
            FullGround = 1 << 25,
            ForceUse = 1 << 26
        }

        public class ServerItem
        {
            protected byte[] spriteHash = null;
            public ServerItem()
            {
                Type = ServerItemType.None;
                StackOrder = TileStackOrder.None;
                Movable = true;
                Name = string.Empty;
            }

            public ServerItem(Appearance appearance)
            {
                if (appearance.Flags.Bank != null)
                {
                    Type = ServerItemType.Ground;
                    GroundSpeed = (ushort)appearance.Flags.Bank.Waypoints;
                }
                else if (appearance.Flags.Container)
                    Type = ServerItemType.Container;
                else if (appearance.Flags.Liquidcontainer)
                    Type = ServerItemType.Fluid;
                else if (appearance.Flags.Liquidpool)
                    Type = ServerItemType.Splash;
                else
                    Type = ServerItemType.None;

                if (appearance.Flags.Clip)
                    StackOrder = TileStackOrder.Border;
                else if (appearance.Flags.Bottom)
                    StackOrder = TileStackOrder.Bottom;
                else if (appearance.Flags.Top)
                    StackOrder = TileStackOrder.Top;
                else
                    StackOrder = TileStackOrder.None;

                if (appearance.Flags.Automap != null && appearance.Flags.Automap.HasColor)
                    MinimapColor = (ushort)appearance.Flags.Automap.Color;

                if (appearance.Flags.Write != null && appearance.Flags.Write.HasMaxTextLength)
                {
                    MaxReadWriteChars = (ushort)appearance.Flags.Write.MaxTextLength;
                    Readable = true;
                }

                if (appearance.Flags.WriteOnce != null && appearance.Flags.WriteOnce.HasMaxTextLengthOnce)
                {
                    MaxReadChars = (ushort)appearance.Flags.WriteOnce.MaxTextLengthOnce;
                    Readable = true;
                }
                if (appearance.Flags.Lenshelp != null && appearance.Flags.Lenshelp.Id == 1112)
                    Readable = true;

                if (appearance.Flags.Light != null)
                {
                    LightColor = (ushort)appearance.Flags.Light.Color;
                    LightLevel = (ushort)appearance.Flags.Light.Brightness;
                }

                if (appearance.Flags.Market != null && appearance.Flags.Market.HasTradeAsObjectId)
                    TradeAs = (ushort)appearance.Flags.Market.TradeAsObjectId;

                if (appearance.HasName && !string.IsNullOrEmpty(appearance.Name))
                    Name = appearance.Name;

                if (appearance.HasDescription && !string.IsNullOrEmpty(appearance.Description))
                    Description = appearance.Description;

                spriteHash = GenerateItemSpriteHash(appearance);
                Unpassable = appearance.Flags.Unpass;
                BlockMissiles = appearance.Flags.Unsight;
                BlockPathfinder = appearance.Flags.Avoid;
                HasElevation = appearance.Flags.Height != null;
                ForceUse = appearance.Flags.Forceuse;
                MultiUse = appearance.Flags.Multiuse;
                Pickupable = appearance.Flags.Take;
                Movable = !appearance.Flags.Unmove;
                Stackable = appearance.Flags.Cumulative;
                Rotatable = appearance.Flags.Rotate;
                Hangable = appearance.Flags.Hang;
                HookSouth = appearance.Flags.HookSouth;
                HookEast = appearance.Flags.HookEast;
                IgnoreLook = appearance.Flags.IgnoreLook;
                FullGround = appearance.Flags.Fullbank;

            }

            public ushort ServerId { get; set; }
            public ushort ClientId { get; set; }
            public ServerItemType Type { get; set; }
            public bool HasStackOrder { get; set; }
            public TileStackOrder StackOrder { get; set; }
            public bool Unpassable { get; set; }
            public bool BlockMissiles { get; set; }
            public bool BlockPathfinder { get; set; }
            public bool HasElevation { get; set; }
            public bool ForceUse { get; set; }
            public bool MultiUse { get; set; }
            public bool Pickupable { get; set; }
            public bool Movable { get; set; }
            public bool Stackable { get; set; }
            public bool Readable { get; set; }
            public bool Rotatable { get; set; }
            public bool Hangable { get; set; }
            public bool HookSouth { get; set; }
            public bool HookEast { get; set; }
            public bool HasCharges { get; set; }
            public bool IgnoreLook { get; set; }
            public bool FullGround { get; set; }
            public bool AllowDistanceRead { get; set; }
            public bool IsAnimation { get; set; }
            public bool ShowCount { get; set; }
            public bool ShowCharges { get; set; }
            public bool ShowDuration { get; set; }
            public bool StopDuration { get; set; }
            public bool Writeable { get; set; }
            public bool ShowAttributes { get; set; }
            public bool SetPlayerInvisible { get; set; }
            public bool MagicShield { get; set; }
            public bool ForceSerialize { get; set; }
            public ushort GroundSpeed { get; set; }
            public ushort LightLevel { get; set; }
            public ushort LightColor { get; set; }
            public ushort MaxReadChars { get; set; }
            public ushort MaxReadWriteChars { get; set; }
            public ushort MinimapColor { get; set; }
            public ushort TradeAs { get; set; }
            public ushort Weight { get; set; }
            public ushort Worth { get; set; }
            public ushort Duration { get; set; }
            public ushort DecayTo { get; set; }
            public ushort EquipTo { get; set; }
            public ushort DeEquipTo { get; set; }
            public ushort WriteOnceItemID { get; set; }
            public ushort Charges { get; set; }
            public ushort RotateTo { get; set; }
            public ushort ShootType { get; set; }
            public ushort EffectType { get; set; }
            public ushort Armor { get; set; }
            public ushort Attack { get; set; }
            public ushort HitChance { get; set; }
            public ushort MaxHitChance { get; set; }
            public ushort Defense { get; set; }
            public ushort ExtraDefense { get; set; }
            public ushort AttackSpeed { get; set; }
            public ushort Range { get; set; }
            public ushort Speed { get; set; }
            public ushort IceAttack { get; set; }
            public ushort EarthAttack { get; set; }
            public ushort EnergyAttack { get; set; }
            public ushort FireAttack { get; set; }
            public ushort DeathAttack { get; set; }
            public ushort HolyAttack { get; set; }
            public ushort MaxHP { get; set; }
            public ushort MaxHPPercent { get; set; }
            public ushort MaxMP { get; set; }
            public ushort MaxMPPercent { get; set; }
            public ushort HealthGain { get; set; }
            public ushort HealthGainTicks { get; set; }
            public ushort ManaGain { get; set; }
            public ushort ManaGainTicks { get; set; }
            public ushort CriticalChance { get; set; }
            public ushort CriticalAmount { get; set; }
            public ushort ExtraMagicLevels { get; set; }
            public ushort ExtraMagicLevelsPercent { get; set; }
            public ushort ExtraSwordLevels { get; set; }
            public ushort ExtraSwordLevelsPercent { get; set; }
            public ushort ExtraAxeLevels { get; set; }
            public ushort ExtraAxeLevelsPercent { get; set; }
            public ushort ExtraClubLevels { get; set; }
            public ushort ExtraClubLevelsPercent { get; set; }
            public ushort ExtraDistanceLevels { get; set; }
            public ushort ExtraDistanceLevelsPercent { get; set; }
            public ushort ExtraShieldLevels { get; set; }
            public ushort ExtraShieldLevelsPercent { get; set; }
            public ushort ExtraFishingLevels { get; set; }
            public ushort ExtraFishingLevelsPercent { get; set; }
            public ushort ExtraFistLevels { get; set; }
            public ushort ExtraFistLevelsPercent { get; set; }
            public ushort AbsorbAllPercent { get; set; }
            public ushort AbsorbPhysicalPercent { get; set; }
            public ushort AbsorbElemenetPercent { get; set; }
            public ushort AbsorbFirePercent { get; set; }
            public ushort AbsorbIcePercent { get; set; }
            public ushort AbsorbEnergyPercent { get; set; }
            public ushort AbsorbDeathPercent { get; set; }
            public ushort AbsorbHolyPercent { get; set; }
            public ushort AbsorbEarthPercent { get; set; }
            public ushort AbsorbHealingPercent { get; set; }
            public ushort AbsorbLifeDrainPercent { get; set; }
            public ushort AbsorbManaDrainPercent { get; set; }
            public ushort AbsorbDrownPercent { get; set; }
            public ushort ContainerSize { get; set; }
            public string Name { get; set; }
            public string PluralName { get; set; }
            public string EditorSuffix { get; set; }
            public string Article { get; set; }
            public string Description { get; set; }
            public string SlotType { get; set; }
            public string WeaponType { get; set; }
            public string CorpseType { get; set; }
            public string FluidSource { get; set; }
            public string FloorChange { get; set; }
            public string RuneSpellName { get; set; }


            // used to find sprites during updates
            public virtual byte[] SpriteHash
            {
                get
                {
                    return spriteHash;
                }

                set
                {
                    spriteHash = value;
                }
            }
        }
        
    }
}
