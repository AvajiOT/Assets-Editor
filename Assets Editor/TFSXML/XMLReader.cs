using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using static Assets_Editor.OTB;
using static Assets_Editor.ItemManager;
using Microsoft.VisualBasic.Logging;
using System.Diagnostics;

namespace Assets_Editor
{
    [Serializable]
    [XmlRoot("items")]
    public class Items
    {
        public Items()
        {
            ItemList = new List<Item>();
        }

        [XmlElement("item")]
        public List<Item> ItemList { get; set; }
    }

    [Serializable]
    public class Item
    {
        private int? id;
        private int? fromId;
        private int? toId;

        public Item()
        {
            Attributes = new List<Attribute>();
        }

        // Change these to use explicit XML serialization for nullable types
        [XmlAttribute("id")]
        public string XmlId
        {
            get => Id.HasValue ? Id.Value.ToString() : null;
            set => Id = value != null ? (int?)int.Parse(value) : null;
        }

        [XmlAttribute("fromid")]
        public string XmlFromId
        {
            get => FromId.HasValue ? FromId.Value.ToString() : null;
            set => FromId = value != null ? (int?)int.Parse(value) : null;
        }

        [XmlAttribute("toid")]
        public string XmlToId
        {
            get => ToId.HasValue ? ToId.Value.ToString() : null;
            set => ToId = value != null ? (int?)int.Parse(value) : null;
        }

        // Add non-serialized properties for the actual nullable values
        [XmlIgnore]
        public int? Id
        {
            get => id;
            set => id = value;
        }

        [XmlIgnore]
        public int? FromId
        {
            get => fromId;
            set => fromId = value;
        }

        [XmlIgnore]
        public int? ToId
        {
            get => toId;
            set => toId = value;
        }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("article")]
        public string Article { get; set; }

        [XmlElement("attribute")]
        public List<Attribute> Attributes { get; set; }

        [XmlElement("plural")]
        public string PluralName { get; set; }

        [XmlElement("editorsuffix")]
        public string EditorSuffix { get; set; }
    }

    [Serializable]
    public class Attribute
    {
        public Attribute()
        {
            NestedAttributes = new List<Attribute>();
        }

        [XmlAttribute("key")]
        public string Key { get; set; }

        [XmlAttribute("value")]
        public string Value { get; set; }

        [XmlAttribute("minvalue")]
        public string MinValue { get; set; }

        [XmlAttribute("maxvalue")]
        public string MaxValue { get; set; }

        [XmlElement("attribute")]
        public List<Attribute> NestedAttributes { get; set; }
    }


    public class ItemsXMLReader
    {
        public ItemsXMLReader()
        {
            Items = new List<ServerItem>();
        }

        public List<ServerItem> Items { get; private set; }

        public bool Read(string path)
        {
            if (!File.Exists(path))
            {
                return false;
            }

            try
            {
                Debug.WriteLine("Attempting to read items.xml");
                // Create XmlSerializer with XmlRoot attribute override
                var overrides = new XmlAttributeOverrides();
                var attributes = new XmlAttributes { XmlRoot = new XmlRootAttribute("items") };
                overrides.Add(typeof(Items), attributes);

                var serializer = new XmlSerializer(typeof(Items), overrides);

                using (var fs = new FileStream(path, FileMode.Open))
                {
                    var items = (Items)serializer.Deserialize(fs);
                    Debug.WriteLine("Opened itemsXML");

                    if (items?.ItemList == null || items.ItemList.Count == 0)
                    {
                        Debug.WriteLine("No items found in itemsXML");
                        return false;
                    }

                    foreach (var item in items.ItemList)
                    {
                        if (item.FromId.HasValue && item.ToId.HasValue)
                        {
                            for (int id = item.FromId.Value; id <= item.ToId.Value; id++)
                            {
                                var serverItem = CreateServerItem(item, (ushort)id);
                                Items.Add(serverItem);
                            }
                        }
                        else if (item.Id.HasValue)
                        {
                            var serverItem = CreateServerItem(item, (ushort)item.Id.Value);
                            Items.Add(serverItem);
                        }
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Log the full exception details
                Debug.WriteLine("Exception occurred: " + ex.Message);
                Debug.WriteLine("Stack Trace:");
                Debug.WriteLine(ex.StackTrace);  // This adds the stack trace

                // Log inner exception details if present
                var currentException = ex;
                int exceptionDepth = 0;
                while (currentException.InnerException != null)
                {
                    exceptionDepth++;
                    currentException = currentException.InnerException;
                    Debug.WriteLine($"Inner Exception Level {exceptionDepth}:");
                    Debug.WriteLine($"Message: {currentException.Message}");
                    Debug.WriteLine($"Stack Trace:");
                    Debug.WriteLine(currentException.StackTrace);
                }

                // Alternatively, you can use Exception.ToString() which includes all nested exceptions
                Debug.WriteLine("Complete Exception Details:");
                Debug.WriteLine(ex.ToString());

                return false;
            }
        }

        private ServerItem CreateServerItem(Item item, ushort id)
        {
            ServerItem serverItem = new ServerItem
            {
                ServerId = id,
                Name = item.Name,
                Article = item.Article ?? string.Empty,
                PluralName = item.PluralName ?? string.Empty,
                EditorSuffix = item.EditorSuffix ?? string.Empty
            };

            if (item.Attributes != null)
            {
                foreach (var attribute in item.Attributes)
                {
                    ProcessAttribute(serverItem, attribute);
                }
            }

            return serverItem;
        }
        private void ProcessAttribute(ServerItem serverItem, Attribute attribute)
        {
            switch (attribute.Key)
            {
                case "type":
                    serverItem.Type = ServerItemTypeConverter.FromString(attribute.Value);
                    break;
                case "description":
                    serverItem.Description = attribute.Value;
                    break;
                case "runespellname":
                    serverItem.RuneSpellName = attribute.Value;
                    break;
                case "weight":
                    serverItem.Weight = long.Parse(attribute.Value);
                    break;
                case "showcount":
                    serverItem.ShowCount = bool.Parse(attribute.Value);
                    break;
                case "armor":
                    serverItem.Armor = long.Parse(attribute.Value);
                    break;
                case "defense":
                    serverItem.Defense = long.Parse(attribute.Value);
                    break;
                case "extradef":
                    serverItem.ExtraDefense = long.Parse(attribute.Value);
                    break;
                case "attack":
                    serverItem.Attack = long.Parse(attribute.Value);
                    break;
                case "attackspeed":
                    serverItem.AttackSpeed = long.Parse(attribute.Value);
                    break;
                case "rotateto":
                    serverItem.RotateTo = long.Parse(attribute.Value);
                    break;
                case "moveable":
                case "movable":
                    serverItem.Movable = bool.Parse(attribute.Value);
                    break;
                case "blockprojectile":
                    serverItem.BlockMissiles = bool.Parse(attribute.Value);
                    break;
                case "blocking":
                    serverItem.Unpassable = bool.Parse(attribute.Value);
                    break;
                case "allowpickupable":
                case "pickupable":
                    serverItem.Pickupable = bool.Parse(attribute.Value);
                    break;
                case "forceserialize":
                case "forcesave":
                    serverItem.ForceSerialize = bool.Parse(attribute.Value);
                    break;
                case "floorchange":
                    serverItem.FloorChange = attribute.Value;
                    break;
                case "corpsetype":
                    serverItem.CorpseType = ServerItemCorpseTypeConverter.FromString(attribute.Value);
                    break;
                case "containersize":
                    serverItem.ContainerSize = long.Parse(attribute.Value);
                    break;
                case "fluidSource":
                    serverItem.FluidSource = ServerItemFluidTypeConverter.FromString(attribute.Value);
                    break;
                case "readable":
                    serverItem.Readable = bool.Parse(attribute.Value);
                    break;
                case "writeable":
                    serverItem.Writeable = bool.Parse(attribute.Value);
                    break;
                case "maxtextlen":
                    serverItem.MaxReadWriteChars = long.Parse(attribute.Value);
                    break;
                case "writeonceitemid":
                    serverItem.WriteOnceItemID = long.Parse(attribute.Value);
                    break;
                case "weapontype":
                    serverItem.WeaponType = ServerItemWeaponTypeConverter.FromString(attribute.Value);
                    break;
                case "slottype":
                    serverItem.SlotType = attribute.Value;
                    break;
                case "ammotype":
                    serverItem.AmmoType = attribute.Value;
                    break;
                //TODO: This will be a string on items.xml however we are using bytes here.
                //Need to convert somewhere to get back to string, based on something provided by the user as every server has different shoottypes or effect
                //case "shoottype":
                //    serverItem.ShootType = attribute.Value; 
                //    break;
                //case "effect":
                //    serverItem.EffectType = attribute.Value;
                //    break;
                case "range":
                    serverItem.Range = long.Parse(attribute.Value);
                    break;
                case "stopduration":
                    serverItem.StopDuration = bool.Parse(attribute.Value);
                    break;
                case "decayTo":
                    serverItem.DecayTo = long.Parse(attribute.Value);
                    break;
                case "transformequipto":
                    serverItem.EquipTo = long.Parse(attribute.Value);
                    break;
                case "transformdeequipto":
                    serverItem.DeEquipTo = long.Parse(attribute.Value);
                    break;
                case "duration":
                    if (!string.IsNullOrEmpty(attribute.Value))
                        serverItem.Duration = long.Parse(attribute.Value);
                    if (!string.IsNullOrEmpty(attribute.MinValue))
                        serverItem.DurationMin = long.Parse(attribute.MinValue);
                    if (!string.IsNullOrEmpty(attribute.MaxValue))
                        serverItem.DurationMax = long.Parse(attribute.MaxValue);
                    break;
                case "showduration":
                    serverItem.ShowDuration = bool.Parse(attribute.Value);
                    break;
                case "charges":
                    serverItem.Charges = long.Parse(attribute.Value);
                    break;
                case "showcharges":
                    serverItem.ShowCharges = bool.Parse(attribute.Value);
                    break;
                case "showattributes":
                    serverItem.ShowAttributes = bool.Parse(attribute.Value);
                    break;
                case "hitchance":
                    serverItem.HitChance = long.Parse(attribute.Value);
                    break;
                case "maxhitchance":
                    serverItem.MaxHitChance = long.Parse(attribute.Value);
                    break;
                case "invisible":
                    serverItem.SetPlayerInvisible = bool.Parse(attribute.Value);
                    break;
                case "speed":
                    serverItem.Speed = long.Parse(attribute.Value);
                    break;
                case "healthgain":
                    serverItem.HealthGain = long.Parse(attribute.Value);
                    break;
                case "healthticks":
                    serverItem.HealthGainTicks = long.Parse(attribute.Value);
                    break;
                case "managain":
                    serverItem.ManaGain = long.Parse(attribute.Value);
                    break;
                case "manaticks":
                    serverItem.ManaGainTicks = long.Parse(attribute.Value);
                    break;
                case "manashield":
                    serverItem.MagicShield = bool.Parse(attribute.Value);
                    break;
                case "skillsword":
                    serverItem.ExtraSwordLevels = long.Parse(attribute.Value);
                    break;
                case "skillaxe":
                    serverItem.ExtraAxeLevels = long.Parse(attribute.Value);
                    break;
                case "skillclub":
                    serverItem.ExtraClubLevels = long.Parse(attribute.Value);
                    break;
                case "skilldistance":
                    serverItem.ExtraDistanceLevels = long.Parse(attribute.Value);
                    break;
                case "skillshield":
                    serverItem.ExtraShieldLevels = long.Parse(attribute.Value);
                    break;
                case "skillfist":
                    serverItem.ExtraFistLevels = long.Parse(attribute.Value);
                    break;
                case "skillfish":
                    serverItem.ExtraFishingLevels = long.Parse(attribute.Value);
                    break;
                case "maxhitpoints":
                    serverItem.MaxHP = long.Parse(attribute.Value);
                    break;
                case "maxhitpointspercent":
                    serverItem.MaxHPPercent = long.Parse(attribute.Value);
                    break;
                case "maxmanapoints":
                    serverItem.MaxMP = long.Parse(attribute.Value);
                    break;
                case "maxmanapointspercent":
                    serverItem.MaxMPPercent = long.Parse(attribute.Value);
                    break;
                case "magicpoints":
                    serverItem.ExtraMagicLevels = long.Parse(attribute.Value);
                    break;
                case "maxlevelpoints":
                    serverItem.ExtraMagicLevels = long.Parse(attribute.Value);
                    break;
                case "magicpointspercent":
                    serverItem.ExtraMagicLevelsPercent = long.Parse(attribute.Value);
                    break;
                case "criticalhitchance":
                    serverItem.CriticalChance = long.Parse(attribute.Value);
                    break;
                case "criticalhitamount":
                    serverItem.CriticalAmount = long.Parse(attribute.Value);
                    break;
                case "lifeleechchance":
                    serverItem.LifeLeechChance = long.Parse(attribute.Value);
                    break;
                case "lifeleechamount":
                    serverItem.LifeLeechAmount = long.Parse(attribute.Value);
                    break;
                case "manaleechchance":
                    serverItem.ManaLeechChance = long.Parse(attribute.Value);
                    break;
                case "manaleechamount":
                    serverItem.ManaLeechAmount = long.Parse(attribute.Value);
                    break;
                case "fieldabsorbpercentenergy":
                    serverItem.FieldAbsorbPercentEnergy = long.Parse(attribute.Value);
                    break;
                case "fieldabsorbpercentfire":
                    serverItem.FieldAbsorbPercentFire = long.Parse(attribute.Value);
                    break;
                case "fieldabsorbpercentpoison":
                    serverItem.FieldAbsorbPercentPoison = long.Parse(attribute.Value);
                    break;
                case "fieldabsorbpercentearth":
                    serverItem.FieldAbsorbPercentEarth = long.Parse(attribute.Value);
                    break;
                case "absorbpercentall":
                    serverItem.AbsorbAllPercent = long.Parse(attribute.Value);
                    break;
                case "absorbpercentallelements":
                    serverItem.AbsorbElemenetPercent = long.Parse(attribute.Value);
                    break;
                case "absorbpercentelements":
                    serverItem.AbsorbElemenetPercent = long.Parse(attribute.Value);
                    break;
                case "absorbpercentmagic":
                    serverItem.AbsorbMagicPercent = long.Parse(attribute.Value);
                    break;
                case "absorbpercentphysical":
                    serverItem.AbsorbPhysicalPercent = long.Parse(attribute.Value);
                    break;
                case "absorbpercentfire":
                    serverItem.AbsorbFirePercent = long.Parse(attribute.Value);
                    break;
                case "absorbpercentenergy":
                    serverItem.AbsorbEnergyPercent = long.Parse(attribute.Value);
                    break;
                case "absorbpercentpoison":
                    serverItem.AbsorbPoisonPercent = long.Parse(attribute.Value);
                    break;
                case "absorbpercentearth":
                    serverItem.AbsorbEarthPercent = long.Parse(attribute.Value);
                    break;
                case "absorbpercentdeath":
                    serverItem.AbsorbDeathPercent = long.Parse(attribute.Value);
                    break;
                case "absorbpercentholy":
                    serverItem.AbsorbHolyPercent = long.Parse(attribute.Value);
                    break;
                case "absorbpercentdrown":
                    serverItem.AbsorbDrownPercent = long.Parse(attribute.Value);
                    break;
                case "absorbpercentlifedrain":
                    serverItem.AbsorbLifeDrainPercent = long.Parse(attribute.Value);
                    break;
                case "absorbpercentmanadrain":
                    serverItem.AbsorbManaDrainPercent = long.Parse(attribute.Value);
                    break;
                case "absorbpercenthealing":
                    serverItem.AbsorbHealingPercent = long.Parse(attribute.Value);
                    break;
                case "elementice":
                    serverItem.IceAttack = long.Parse(attribute.Value);
                    break;
                case "elementfire":
                    serverItem.FireAttack = long.Parse(attribute.Value);
                    break;
                case "elementenergy":
                    serverItem.EnergyAttack = long.Parse(attribute.Value);
                    break;
                case "elementearth":
                    serverItem.EarthAttack = long.Parse(attribute.Value);
                    break;
                case "elementdeath":
                    serverItem.DeathAttack = long.Parse(attribute.Value);
                    break;
                case "elementholy":
                    serverItem.HolyAttack = long.Parse(attribute.Value);
                    break;
                case "worth":
                    serverItem.Worth = long.Parse(attribute.Value);
                    break;

                // field damage specific
                case "field":
                    serverItem.FieldType = attribute.Value;
                    break;
                case "ticks":
                    serverItem.FieldTicks = long.Parse(attribute.Value);
                    break;
                case "initdamage":
                    serverItem.FieldInitDamage = long.Parse(attribute.Value);
                    break;
                case "count":
                    serverItem.FieldCount = long.Parse(attribute.Value);
                    break;
                case "damage":
                    serverItem.FieldDamage = long.Parse(attribute.Value);
                    break;
                case "start":
                    serverItem.FieldStart = long.Parse(attribute.Value);
                    break;

            }

            if (attribute.NestedAttributes != null)
            {
                foreach (var nestedAttribute in attribute.NestedAttributes)
                {
                    ProcessAttribute(serverItem, nestedAttribute);
                }
            }
        }
    } 
}
