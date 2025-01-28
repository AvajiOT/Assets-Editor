using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using static Assets_Editor.OTB;
using static Assets_Editor.ItemManager;

namespace Assets_Editor
{
    [XmlRoot("items")]
    public class Items
    {
        [XmlElement("item")]
        public List<Item> ItemList { get; set; }
    }

    public class Item
    {
        [XmlAttribute("id")]
        public int? Id { get; set; }

        [XmlAttribute("fromid")]
        public int? FromId { get; set; }

        [XmlAttribute("toid")]
        public int? ToId { get; set; }

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

    public class Attribute
    {
        [XmlAttribute("key")]
        public string Key { get; set; }

        [XmlAttribute("value")]
        public string Value { get; set; }
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
                XmlSerializer serializer = new XmlSerializer(typeof(Items));
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    Items items = (Items)serializer.Deserialize(fs);
                    foreach (var item in items.ItemList)
                    {
                        if (item.FromId.HasValue && item.ToId.HasValue)
                        {
                            for (int id = item.FromId.Value; id <= item.ToId.Value; id++)
                            {
                                ServerItem serverItem = CreateServerItem(item, (ushort)id);
                                Items.Add(serverItem);
                            }
                        }
                        else if (item.Id.HasValue)
                        {
                            ServerItem serverItem = CreateServerItem(item, (ushort)item.Id.Value);
                            Items.Add(serverItem);
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
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
                            serverItem.Weight = ushort.Parse(attribute.Value);
                            break;
                        case "showcount":
                            serverItem.ShowCount = bool.Parse(attribute.Value);
                            break;
                        case "armor":
                            serverItem.Armor = ushort.Parse(attribute.Value);
                            break;
                        case "defense":
                            serverItem.Defense = ushort.Parse(attribute.Value);
                            break;
                        case "extradef":
                            serverItem.ExtraDefense = ushort.Parse(attribute.Value);
                            break;
                        case "attack":
                            serverItem.Attack = ushort.Parse(attribute.Value);
                            break;
                        case "attackspeed":
                            serverItem.AttackSpeed = ushort.Parse(attribute.Value);
                            break;
                        case "rotateto":
                            serverItem.RotateTo = ushort.Parse(attribute.Value);
                            break;
                        case "moveable":
                            serverItem.Movable = bool.Parse(attribute.Value);
                            break;
                        case "movable":
                            serverItem.Movable = bool.Parse(attribute.Value);
                            break;
                        case "blockprojectile":
                            serverItem.BlockMissiles = bool.Parse(attribute.Value);
                            break;
                        case "allowpickupable":
                            serverItem.Pickupable = bool.Parse(attribute.Value);
                            break;
                        case "pickupable":
                            serverItem.Pickupable = bool.Parse(attribute.Value);
                            break;
                        case "forceserialize":
                            serverItem.ForceSerialize = bool.Parse(attribute.Value);
                            break;
                        case "forcesave":
                            serverItem.ForceSerialize = bool.Parse(attribute.Value);
                            break;
                        case "floorchange":
                            serverItem.FloorChange = attribute.Value;
                            break;
                        case "corpsetype":
                            serverItem.CorpseType = attribute.Value;
                            break;
                        case "containersize":
                            serverItem.ContainerSize = ushort.Parse(attribute.Value);
                            break;
                        case "fluidSource":
                            serverItem.FluidSource = attribute.Value;
                            break;
                        case "decayTo":
                            serverItem.DecayTo = ushort.Parse(attribute.Value);
                            break;
                        case "duration":
                            serverItem.Duration = ushort.Parse(attribute.Value);
                            break;
                    }
                }
            }

            return serverItem;
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
                null => ServerItemType.None,
                _ => ServerItemType.None,
            };
        }
    }
}
