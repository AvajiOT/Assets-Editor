using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Assets_Editor.OTB;
using Tibia.Protobuf.Appearances;

namespace Assets_Editor
{
    //TODO: check what difference there is between versions
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

        public enum ServerItemCorpseType : byte
        {
            None = 0,
            Blood = 1,
            Venom = 2,
            Fire = 3,
            Undead = 4,
            Energy = 5,
            Ink = 6,
        }

        public enum ServerItemFluidType : byte
        {
            None = 0,
            Water = 1,
            Blood = 2,
            Beer = 3,
            Slime = 4,
            Lemonade = 5,
            Milk = 6,
            Mana = 7,
            Life = 8,
            Oil = 9,
            Urine = 10,
            Coconut = 11,
            Wine = 12,
            Mud = 13,
            FruitJuice = 14,
            Lava = 15,
            Rum = 16,
            Swamp = 17,
            Tea = 18,
            Mead = 19,
            Ink = 20,
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
                    GroundSpeed = (long)appearance.Flags.Bank.Waypoints;
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
                    MinimapColor = (long)appearance.Flags.Automap.Color;

                if (appearance.Flags.Write != null && appearance.Flags.Write.HasMaxTextLength)
                {
                    MaxReadWriteChars = (long)appearance.Flags.Write.MaxTextLength;
                    Readable = true;
                }

                if (appearance.Flags.WriteOnce != null && appearance.Flags.WriteOnce.HasMaxTextLengthOnce)
                {
                    MaxReadChars = (long)appearance.Flags.WriteOnce.MaxTextLengthOnce;
                    Readable = true;
                }
                if (appearance.Flags.Lenshelp != null && appearance.Flags.Lenshelp.Id == 1112)
                    Readable = true;

                if (appearance.Flags.Light != null)
                {
                    LightColor = (long)appearance.Flags.Light.Color;
                    LightLevel = (long)appearance.Flags.Light.Brightness;
                }

                if (appearance.Flags.Market != null && appearance.Flags.Market.HasTradeAsObjectId)
                    TradeAs = (long)appearance.Flags.Market.TradeAsObjectId;

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
            public ServerItemWeaponType WeaponType { get; set; }
            public ServerItemCorpseType CorpseType { get; set; }
            public ServerItemFluidType FluidSource { get; set; }
            public bool HasStackOrder { get; set; }
            public TileStackOrder StackOrder { get; set; }
            public bool Unpassable { get; set; }
            //public bool Blocking { get; set; } // "Blocking" in items.xml will be the same as "unpassable"
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
            public bool SuppressDrunk { get; set; }
            public bool SuppressFire { get; set; }
            public bool SuppressEnergy { get; set; }
            public bool SuppressDrown { get; set; }
            public bool SuppressPhysical { get; set; }
            public bool SuppressFreeze { get; set; }
            public bool SuppressDazzle { get; set; }
            public bool SuppressCurse { get; set; }
            public bool SuppressPoison { get; set; }
            public bool Replaceable { get; set; }
            public long GroundSpeed { get; set; }
            public long LightLevel { get; set; }
            public long LightColor { get; set; }
            public long MaxReadChars { get; set; }
            public long MaxReadWriteChars { get; set; }
            public long MinimapColor { get; set; }
            public long TradeAs { get; set; }
            public long Weight { get; set; }
            public long Worth { get; set; }
            public long Duration { get; set; }
            public long DurationMin { get; set; }
            public long DurationMax { get; set; }
            public long DecayTo { get; set; }
            public long EquipTo { get; set; }
            public long DeEquipTo { get; set; }
            public long WriteOnceItemID { get; set; }
            public long Charges { get; set; }
            public long RotateTo { get; set; }
            public long ShootType { get; set; }
            public long EffectType { get; set; }
            public long Armor { get; set; }
            public long Attack { get; set; }
            public long HitChance { get; set; }
            public long MaxHitChance { get; set; }
            public long Defense { get; set; }
            public long ExtraDefense { get; set; }
            public long AttackSpeed { get; set; }
            public long Range { get; set; }
            public long Speed { get; set; }
            public long IceAttack { get; set; }
            public long EarthAttack { get; set; }
            public long EnergyAttack { get; set; }
            public long FireAttack { get; set; }
            public long DeathAttack { get; set; }
            public long HolyAttack { get; set; }
            public long MaxHP { get; set; }
            public long MaxHPPercent { get; set; }
            public long MaxMP { get; set; }
            public long MaxMPPercent { get; set; }
            public long HealthGain { get; set; }
            public long HealthGainTicks { get; set; }
            public long ManaGain { get; set; }
            public long ManaGainTicks { get; set; }
            public long CriticalChance { get; set; }
            public long CriticalAmount { get; set; }
            public long LifeLeechChance { get; set; }
            public long LifeLeechAmount { get; set; }
            public long ManaLeechChance { get; set; }
            public long ManaLeechAmount { get; set; }
            public long ExtraMagicLevels { get; set; }
            public long ExtraMagicLevelsPercent { get; set; }
            public long ExtraSwordLevels { get; set; }
            public long ExtraSwordLevelsPercent { get; set; }
            public long ExtraAxeLevels { get; set; }
            public long ExtraAxeLevelsPercent { get; set; }
            public long ExtraClubLevels { get; set; }
            public long ExtraClubLevelsPercent { get; set; }
            public long ExtraDistanceLevels { get; set; }
            public long ExtraDistanceLevelsPercent { get; set; }
            public long ExtraShieldLevels { get; set; }
            public long ExtraShieldLevelsPercent { get; set; }
            public long ExtraFishingLevels { get; set; }
            public long ExtraFishingLevelsPercent { get; set; }
            public long ExtraFistLevels { get; set; }
            public long ExtraFistLevelsPercent { get; set; }
            public long AbsorbAllPercent { get; set; }
            public long AbsorbPhysicalPercent { get; set; }
            public long AbsorbMagicPercent { get; set; }
            public long AbsorbElemenetPercent { get; set; }
            public long AbsorbFirePercent { get; set; }
            public long AbsorbIcePercent { get; set; }
            public long AbsorbEnergyPercent { get; set; }
            public long AbsorbPoisonPercent { get; set; }
            public long AbsorbDeathPercent { get; set; }
            public long AbsorbHolyPercent { get; set; }
            public long AbsorbEarthPercent { get; set; }
            public long AbsorbHealingPercent { get; set; }
            public long AbsorbLifeDrainPercent { get; set; }
            public long AbsorbManaDrainPercent { get; set; }
            public long AbsorbDrownPercent { get; set; }
            public long FieldAbsorbPercentEnergy { get; set; }
            public long FieldAbsorbPercentFire { get; set; }
            public long FieldAbsorbPercentEarth { get; set; }
            public long FieldAbsorbPercentPoison { get; set; }
            public long ContainerSize { get; set; }
            public long LevelDoor { get; set; }
            public long DestroyTo { get; set; }
            public long FieldTicks { get; set; }
            public long FieldStart { get; set; }
            public long FieldInitDamage { get; set; }
            public long FieldDamage { get; set; }
            public long FieldCount { get; set; }
            public string Name { get; set; }
            public string PluralName { get; set; }
            public string EditorSuffix { get; set; }
            public string Article { get; set; }
            public string Description { get; set; }
            public string SlotType { get; set; }
            public string AmmoType { get; set; }
            public string FloorChange { get; set; }
            public string RuneSpellName { get; set; }
            public string FieldType { get; set; }



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
        public static class ServerItemTypeConverter
        {
            public static ServerItemType FromString(string value)
            {
                return value.ToLower() switch
                {
                    "key" => ServerItemType.Key,
                    "magicfield" => ServerItemType.MagicField,
                    "container" => ServerItemType.Container,
                    "depot" => ServerItemType.Depot,
                    "mailbox" => ServerItemType.Mailbox,
                    "trashholder" => ServerItemType.Trashholder,
                    "teleport" => ServerItemType.Teleport,
                    "door" => ServerItemType.Door,
                    "bed" => ServerItemType.Bed,
                    "rune" => ServerItemType.Rune,
                    "podium" => ServerItemType.Podium,
                    "" => ServerItemType.None,
                    _ => ServerItemType.None,
                };
            }
            public static string ToString(ServerItemType value)
            {
                return value switch
                {
                    ServerItemType.Key => "key",
                    ServerItemType.MagicField => "magicfield",
                    ServerItemType.Container => "container",
                    ServerItemType.Depot => "depot",
                    ServerItemType.Mailbox => "mailbox",
                    ServerItemType.Trashholder => "trashholder",
                    ServerItemType.Teleport => "teleport",
                    ServerItemType.Door => "door",
                    ServerItemType.Bed => "bed",
                    ServerItemType.Rune => "rune",
                    ServerItemType.Podium => "podium",
                    ServerItemType.None => "",
                    _ => "",
                };
            }
        }
        public static class ServerItemWeaponTypeConverter
        {
            public static ServerItemWeaponType FromString(string value)
            {
                return value.ToLower() switch
                {
                    "sword" => ServerItemWeaponType.Sword,
                    "axe" => ServerItemWeaponType.Axe,
                    "club" => ServerItemWeaponType.Club,
                    "distance" => ServerItemWeaponType.Distance,
                    "shield" => ServerItemWeaponType.Shield,
                    "wand" => ServerItemWeaponType.Wand,
                    "" => ServerItemWeaponType.None,
                    _ => ServerItemWeaponType.None,
                };
            }
            public static string ToString(ServerItemWeaponType value)
            {
                return value switch
                {
                    ServerItemWeaponType.Sword => "sword",
                    ServerItemWeaponType.Axe => "axe",
                    ServerItemWeaponType.Club => "club",
                    ServerItemWeaponType.Distance => "distance",
                    ServerItemWeaponType.Shield => "shield",
                    ServerItemWeaponType.Wand => "wand",
                    ServerItemWeaponType.None => "",
                    _ => "",
                };
            }
        }

        public static class ServerItemCorpseTypeConverter
        {
            public static ServerItemCorpseType FromString(string value)
            {
                return value.ToLower() switch
                {
                    "blood" => ServerItemCorpseType.Blood,
                    "venom" => ServerItemCorpseType.Venom,
                    "fire" => ServerItemCorpseType.Fire,
                    "undead" => ServerItemCorpseType.Undead,
                    "energy" => ServerItemCorpseType.Energy,
                    "ink" => ServerItemCorpseType.Ink,
                    "" => ServerItemCorpseType.None,
                    _ => ServerItemCorpseType.None,
                };
            }
            public static string ToString(ServerItemCorpseType value)
            {
                return value switch
                {
                    ServerItemCorpseType.Blood => "blood",
                    ServerItemCorpseType.Venom => "venom",
                    ServerItemCorpseType.Fire => "fire",
                    ServerItemCorpseType.Undead => "undead",
                    ServerItemCorpseType.Energy => "energy",
                    ServerItemCorpseType.Ink => "ink",
                    ServerItemCorpseType.None => "",
                    _ => "",
                };
            }
        }
        public static class ServerItemFluidTypeConverter
        {
            public static ServerItemFluidType FromString(string value)
            {
                return value.ToLower() switch
                {
                    "water" => ServerItemFluidType.Water,
                    "blood" => ServerItemFluidType.Blood,
                    "beer" => ServerItemFluidType.Beer,
                    "slime" => ServerItemFluidType.Slime,
                    "lemonade" => ServerItemFluidType.Lemonade,
                    "milk" => ServerItemFluidType.Milk,
                    "mana" => ServerItemFluidType.Mana,
                    "life" => ServerItemFluidType.Life,
                    "oil" => ServerItemFluidType.Oil,
                    "urine" => ServerItemFluidType.Urine,
                    "coconut" => ServerItemFluidType.Coconut,
                    "wine" => ServerItemFluidType.Wine,
                    "mud" => ServerItemFluidType.Mud,
                    "fruitjuice" => ServerItemFluidType.FruitJuice,
                    "lava" => ServerItemFluidType.Lava,
                    "rum" => ServerItemFluidType.Rum,
                    "swamp" => ServerItemFluidType.Swamp,
                    "tea" => ServerItemFluidType.Tea,
                    "mead" => ServerItemFluidType.Mead,
                    "ink" => ServerItemFluidType.Ink,
                    "" => ServerItemFluidType.None,
                    _ => ServerItemFluidType.None,
                };
            }
            public static string ToString(ServerItemFluidType value)
            {
                return value switch
                {
                    ServerItemFluidType.Water => "water",
                    ServerItemFluidType.Blood => "blood",
                    ServerItemFluidType.Beer => "beer",
                    ServerItemFluidType.Slime => "slime",
                    ServerItemFluidType.Lemonade => "lemonade",
                    ServerItemFluidType.Milk => "milk",
                    ServerItemFluidType.Mana => "mana",
                    ServerItemFluidType.Life => "life",
                    ServerItemFluidType.Oil => "oil",
                    ServerItemFluidType.Urine => "urine",
                    ServerItemFluidType.Coconut => "coconut",
                    ServerItemFluidType.Wine => "wine",
                    ServerItemFluidType.Mud => "mud",
                    ServerItemFluidType.FruitJuice => "fruitjuice",
                    ServerItemFluidType.Lava => "lava",
                    ServerItemFluidType.Rum => "rum",
                    ServerItemFluidType.Swamp => "swamp",
                    ServerItemFluidType.Tea => "tea",
                    ServerItemFluidType.Mead => "mead",
                    ServerItemFluidType.Ink => "ink",
                    ServerItemFluidType.None => "",
                    _ => "",
                };
            }
        }
    }
}
