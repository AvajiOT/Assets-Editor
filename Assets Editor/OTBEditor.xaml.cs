using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Tibia.Protobuf.Appearances;
using static Assets_Editor.DatEditor;
using static Assets_Editor.OTB;
using static Assets_Editor.ItemsXMLReader;
using static Assets_Editor.ItemManager;

namespace Assets_Editor
{
    /// <summary>
    /// Interaction logic for OTBEditor.xaml
    /// </summary>
    public partial class OTBEditor : Window
    {
        private dynamic _editor;
        private bool _legacy = false;
        private List<CatalogTransparency> transparentSheets = new List<CatalogTransparency>();
        private string OTBFilePath;
        private string XMLFilePath;
        private Storyboard itemsXMLStoryboard;
        public OTBEditor(dynamic editor, bool legacy)
        {
            InitializeComponent();
            _editor = editor;
            _legacy = legacy;
        }
        private Dictionary<uint, Appearance> appearanceByClientId = new Dictionary<uint, Appearance>();
        private List<ServerItem> OTBItems = new List<ServerItem>();
        private bool OTBLoaded = false;
        private uint MajorVersion;
        private uint MinorVersion;
        private uint BuildNumber;
        private uint ClientVersion;
        private void OpenOTBButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openOTBFileDialog = new OpenFileDialog();
            openOTBFileDialog.Filter = "OTB Files (*.otb)|*.otb";
            if (openOTBFileDialog.ShowDialog() == true)
            {
                appearanceByClientId.Clear();
                ItemListView.ItemsSource = null;
                OtbPathText.Text = openOTBFileDialog.FileName;
                OTBFilePath = openOTBFileDialog.FileName;
            }
        }

        private void OpenXMLButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openXMLFileDialog = new OpenFileDialog();
            openXMLFileDialog.Filter = "XML Files (*.xml)|*.xml";
            if (openXMLFileDialog.ShowDialog() == true)
            {
                XmlPathText.Text = openXMLFileDialog.FileName;
                XMLFilePath = openXMLFileDialog.FileName;
            }
        }

        private void LoadItemsButton_Click(object sender, RoutedEventArgs e)
        {
            if ((OTBFilePath == null && XMLFilePath == null) || (OTBFilePath == null && XMLFilePath != null))
            {
                MessageBox.Show("You need to select an items.otb file.");
                return;
            }

            // Load items from items.otb
            OTBReader otbReader = new OTBReader();
            OTBItems = new List<ServerItem>();
            bool stats = otbReader.Read(OTBFilePath);
            if (stats == true)
            {
                OTBLoaded = true;
                MajorVersion = otbReader.MajorVersion;
                MinorVersion = otbReader.MinorVersion;
                BuildNumber = otbReader.BuildNumber;
                ClientVersion = otbReader.ClientVersion;
                foreach (Appearance appearance in MainWindow.appearances.Object)
                {
                    appearanceByClientId.Add(appearance.Id, appearance); // Replace '0' with the desired value for each Appearance key
                }

                foreach (var item in otbReader.Items)
                {
                    OTBItems.Add(item);
                    ItemListView.Items.Add(new ShowList() { Id = item.ServerId, Cid = item.ClientId });
                }
            }

            if (XMLFilePath != null)
            {
                // Load items from items.xml
                ItemsXMLReader xmlReader = new ItemsXMLReader(); //TODO: implement reading items.xml
                xmlReader.Read(XMLFilePath);
                foreach (var XMLItem in xmlReader.Items)
                {
                    ServerItem item = OTBItems[XMLItem.ServerId];
                    item.Name = XMLItem.Name; // note; this will change currently saved name
                    item.Description = XMLItem.Description;
                    item.SlotType = XMLItem.SlotType;
                    item.WeaponType = XMLItem.WeaponType;
                    item.CorpseType = XMLItem.CorpseType;
                    item.FluidSource = XMLItem.FluidSource;
                    item.FloorChange = XMLItem.FloorChange;
                    item.ShowCount = XMLItem.ShowCount;
                    item.ShowCharges = XMLItem.ShowCharges;
                    item.ShowAttributes = XMLItem.ShowAttributes;
                    item.ShowDuration = XMLItem.ShowDuration;
                    item.StopDuration = XMLItem.StopDuration;
                    item.Speed = XMLItem.Speed;
                    item.Writeable = XMLItem.Writeable;
                    item.SetPlayerInvisible = XMLItem.SetPlayerInvisible;
                    item.MagicShield = XMLItem.MagicShield;
                    item.Weight = XMLItem.Weight;
                    item.Worth = XMLItem.Worth;
                    item.Duration = XMLItem.Duration;
                    item.DecayTo = XMLItem.DecayTo;
                    item.EquipTo = XMLItem.EquipTo;
                    item.DeEquipTo = XMLItem.DeEquipTo;
                    item.WriteOnceItemID = XMLItem.WriteOnceItemID;
                    item.Charges = XMLItem.Charges;
                    item.RotateTo = XMLItem.RotateTo;
                    item.ShootType = XMLItem.ShootType;
                    item.EffectType = XMLItem.EffectType;
                    item.Armor = XMLItem.Armor;
                    item.Attack = XMLItem.Attack;
                    item.Defense = XMLItem.Defense;
                    item.ExtraDefense = XMLItem.ExtraDefense;
                    item.AttackSpeed = XMLItem.AttackSpeed;
                    item.Range = XMLItem.Range;
                    item.Speed = XMLItem.Speed;
                    item.IceAttack = XMLItem.IceAttack;
                    item.EarthAttack = XMLItem.EarthAttack;
                    item.FireAttack = XMLItem.FireAttack;
                    item.EnergyAttack = XMLItem.EnergyAttack;
                    item.DeathAttack = XMLItem.DeathAttack;
                    item.HolyAttack = XMLItem.HolyAttack;
                    item.MaxHP = XMLItem.MaxHP;
                    item.MaxHPPercent = XMLItem.MaxHPPercent;
                    item.MaxMP = XMLItem.MaxMP;
                    item.MaxMPPercent = XMLItem.MaxMPPercent;
                    item.HealthGain = XMLItem.HealthGain;
                    item.ManaGain = XMLItem.ManaGain;
                    item.HealthGainTicks = XMLItem.HealthGainTicks;
                    item.ManaGainTicks = XMLItem.ManaGainTicks;
                    item.CriticalChance = XMLItem.CriticalChance;
                    item.CriticalAmount = XMLItem.CriticalAmount;
                    item.ExtraMagicLevels = XMLItem.ExtraMagicLevels;
                    item.ExtraMagicLevelsPercent = XMLItem.ExtraMagicLevelsPercent;
                    item.ExtraSwordLevels = XMLItem.ExtraSwordLevels;
                    item.ExtraSwordLevelsPercent = XMLItem.ExtraSwordLevelsPercent;
                    item.ExtraAxeLevels = XMLItem.ExtraAxeLevels;
                    item.ExtraAxeLevelsPercent = XMLItem.ExtraAxeLevelsPercent;
                    item.ExtraClubLevels = XMLItem.ExtraClubLevels;
                    item.ExtraClubLevelsPercent = XMLItem.ExtraClubLevelsPercent;
                    item.ExtraDistanceLevels = XMLItem.ExtraDistanceLevels;
                    item.ExtraDistanceLevelsPercent = XMLItem.ExtraDistanceLevelsPercent;
                    item.ExtraShieldLevels = XMLItem.ExtraShieldLevels;
                    item.ExtraShieldLevelsPercent = XMLItem.ExtraShieldLevelsPercent;
                    item.ExtraFishingLevels = XMLItem.ExtraFishingLevels;
                    item.ExtraFishingLevelsPercent = XMLItem.ExtraFishingLevelsPercent;
                    item.ExtraFistLevels = XMLItem.ExtraFistLevels;
                    item.ExtraFistLevelsPercent = XMLItem.ExtraFistLevelsPercent;
                    item.AbsorbAllPercent = XMLItem.AbsorbAllPercent;
                    item.AbsorbPhysicalPercent = XMLItem.AbsorbPhysicalPercent;
                    item.AbsorbElemenetPercent = XMLItem.AbsorbElemenetPercent;
                    item.AbsorbFirePercent = XMLItem.AbsorbFirePercent;
                    item.AbsorbIcePercent = XMLItem.AbsorbIcePercent;
                    item.AbsorbEarthPercent = XMLItem.AbsorbEarthPercent;
                    item.AbsorbEnergyPercent = XMLItem.AbsorbEnergyPercent;
                    item.AbsorbDeathPercent = XMLItem.AbsorbDeathPercent;
                    item.AbsorbHolyPercent = XMLItem.AbsorbHolyPercent;
                    item.AbsorbHealingPercent = XMLItem.AbsorbHealingPercent;
                    item.AbsorbLifeDrainPercent = XMLItem.AbsorbLifeDrainPercent;
                    item.AbsorbManaDrainPercent = XMLItem.AbsorbManaDrainPercent;
                    item.EditorSuffix = XMLItem.EditorSuffix;
                    item.PluralName = XMLItem.PluralName;
                    item.ForceSerialize = XMLItem.ForceSerialize;
                    item.HitChance = XMLItem.HitChance;
                    item.MaxHitChance = XMLItem.MaxHitChance;
                    item.RuneSpellName = XMLItem.RuneSpellName;
                }
            }
        }

        private void ItemListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ServerItem item = OTBItems[ItemListView.SelectedIndex];
            I_ServerId.Value = item.ServerId;
            I_ClientId.Value = item.ClientId;
            I_Name.Text = item.Name;
            I_StackOrder.SelectedIndex = (int)item.StackOrder;
            I_Type.SelectedIndex = (int)item.Type;
            I_GroundSpeed.Value = item.GroundSpeed;
            I_Unpassable.IsChecked = item.Unpassable;
            I_Movable.IsChecked = item.Movable;
            I_BlockMissiles.IsChecked = item.BlockMissiles;
            I_BlockPath.IsChecked = item.BlockPathfinder;
            I_Pickable.IsChecked = item.Pickupable;
            I_Stackable.IsChecked = item.Stackable;
            I_ForceUse.IsChecked = item.ForceUse;
            I_MultiUse.IsChecked = item.MultiUse;
            I_Rotateable.IsChecked = item.Rotatable;
            I_Hangable.IsChecked = item.Hangable;
            I_HookSouth.IsChecked = item.HookSouth;
            I_HookEast.IsChecked = item.HookEast;
            I_Elevation.IsChecked = item.HasElevation;
            I_IgnoreLook.IsChecked = item.IgnoreLook;
            I_Readable.IsChecked = item.Readable;
            I_FullGround.IsChecked = item.FullGround;
            I_MiniMapColor.Value = item.MinimapColor;
            I_ShowAs.Value = item.TradeAs;
            I_LightLevel.Value = item.LightLevel;
            I_LightColor.Value = item.LightColor;
            I_MaxReadWriteChars.Value = item.MaxReadWriteChars;
            I_MaxReadChars.Value = item.MaxReadChars;

            _editor.ObjectMenu.SelectedIndex = 1;

            foreach (ShowList showlist in _editor.ObjListView.Items)
            {
                if (showlist.Id == item.ClientId)
                {
                    _editor.ObjListView.SelectedItem = showlist;
                }
            }
        }

        private void ItemListView_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            VirtualizingStackPanel panel = Utils.FindVisualChild<VirtualizingStackPanel>(ItemListView);
            if (ItemListView.Items.Count > 0 && panel != null)
            {
                int offset = (int)panel.VerticalOffset;
                for (int i = 0; i < ItemListView.Items.Count; i++)
                {
                    if (i >= offset && i < Math.Min(offset + 20, ItemListView.Items.Count))
                    {
                        if (_legacy)
                        {
                            ShowList item = (ShowList)ItemListView.Items[i];
                            if (appearanceByClientId.ContainsKey(item.Cid))
                            {
                                item.Image = Utils.BitmapToBitmapImage(LegacyAppearance.GetObjectImage(appearanceByClientId[item.Cid], MainWindow.MainSprStorage));
                            }
                        }
                        else
                        {
                            ShowList item = (ShowList)ItemListView.Items[i];
                            if (appearanceByClientId.ContainsKey(item.Cid))
                            {
                                item.Image = Utils.BitmapToBitmapImage(MainWindow.getSpriteStream((int)appearanceByClientId[item.Cid].FrameGroup[0].SpriteInfo.SpriteId[0]));
                            }
                        }

                    }
                    else
                    {
                        ShowList item = (ShowList)ItemListView.Items[i];
                        item.Image = null;
                    }
                }
            }
        }

        private void CreateMissingItems(object sender, RoutedEventArgs e)
        {
            if (!OTBLoaded)
                return;
            ushort lastCid = 0;
            int lastServerId = 0;
            uint maxCid = MainWindow.appearances.Object.Last().Id;

            foreach (ServerItem item in OTBItems)
            {
                if (item.ClientId > lastCid)
                {
                    lastCid = item.ClientId;
                }
                if (item.ServerId > lastServerId)
                {
                    lastServerId = item.ServerId;
                }
            }
            lastServerId++;
            ushort newItemCounter = 0;

            for (ushort i = 100; i <= maxCid; i++)
            {
                ServerItem itemExist = OTBItems.FirstOrDefault(x => x.ClientId == i);
                if (itemExist == null && appearanceByClientId.ContainsKey(i))
                {
                    ServerItem item = new ServerItem(appearanceByClientId[i]);
                    if (BitConverter.ToString(item.SpriteHash) != "4B-1B-1C-88-FF-2F-AF-29-0E-BC-39-2B-11-6D-10-1C" || i > lastCid)
                    {
                        item.ClientId = i;
                        item.ServerId = (ushort)lastServerId;
                        OTBItems.Add(item);
                        ItemListView.Items.Add(new ShowList() { Id = item.ServerId, Cid = item.ClientId });
                        if (_legacy)
                            NewItemListView.Items.Add(new ShowList() { Id = item.ServerId, Cid = item.ClientId, Image = Utils.BitmapToBitmapImage(LegacyAppearance.GetObjectImage(appearanceByClientId[item.ClientId], MainWindow.MainSprStorage)) });
                        else
                            NewItemListView.Items.Add(new ShowList() { Id = item.ServerId, Cid = item.ClientId, Image = Utils.BitmapToBitmapImage(MainWindow.getSpriteStream((int)appearanceByClientId[item.ClientId].FrameGroup[0].SpriteInfo.SpriteId[0])) });
                        lastServerId++;
                        newItemCounter++;
                    }
                }
            }
        }
        private void CheckMisMatcheditems(object sender, RoutedEventArgs e)
        {
            if (!OTBLoaded)
                return;
            NewItemListView.Items.Clear();
            foreach (ServerItem item in OTBItems)
            {
                if (!appearanceByClientId.ContainsKey(item.ClientId))
                {
                    continue;
                }

                ServerItem citem = new ServerItem(appearanceByClientId[item.ClientId]);
                if (!Utils.ByteArrayCompare(item.SpriteHash,citem.SpriteHash))
                {
                    if (_legacy)
                        NewItemListView.Items.Add(new ShowList() { Id = item.ServerId, Cid = item.ClientId, Image = Utils.BitmapToBitmapImage(LegacyAppearance.GetObjectImage(appearanceByClientId[item.ClientId], MainWindow.MainSprStorage)) });
                    else
                        NewItemListView.Items.Add(new ShowList() { Id = item.ServerId, Cid = item.ClientId, Image = Utils.BitmapToBitmapImage(MainWindow.getSpriteStream((int)appearanceByClientId[item.ClientId].FrameGroup[0].SpriteInfo.SpriteId[0])) });
                }
            }

        }
        private void NewItemListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            if (!OTBLoaded)
                return;
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "OTB files (*.otb)|*.otb";
            dialog.Title = "Save OTB File";

            if (dialog.ShowDialog() == true)
            {
                if (dialog.FileName.Length == 0)
                {
                    return;
                }

                try
                {
                    OTBWriter writer = new OTBWriter(OTBItems);
                    writer.MajorVersion = MajorVersion;
                    writer.MinorVersion = MinorVersion;
                    writer.BuildNumber = BuildNumber;
                    writer.ClientVersion = ClientVersion;
                    if (writer.Write(dialog.FileName))
                    {
                        Debug.WriteLine("OTB saved");
                    }
                }
                catch (UnauthorizedAccessException exception)
                {
                    MessageBox.Show(exception.Message);
                }
            }
        }

        private void ReloadItemsAttr(object sender, RoutedEventArgs e)
        {
            foreach (ShowList selectedItem in ItemListView.SelectedItems)
            {
                ServerItem item = OTBItems[ItemListView.Items.IndexOf(selectedItem)];
                ServerItem itemAttr = new ServerItem(appearanceByClientId[item.ClientId]);
                itemAttr.ServerId = item.ServerId;
                itemAttr.ClientId = item.ClientId;
                OTBItems[ItemListView.Items.IndexOf(selectedItem)] = itemAttr;
            }
        }

        private void SaveItemButton_Click(object sender, RoutedEventArgs e)
        {
            ServerItem item = OTBItems[ItemListView.SelectedIndex];
            item.ServerId = (ushort)I_ServerId.Value;
            item.ClientId = (ushort)I_ClientId.Value;
            item.Name = I_Name.Text;
            item.StackOrder = (TileStackOrder)I_StackOrder.SelectedIndex;
            item.Type = (ServerItemType)I_Type.SelectedIndex;
            item.GroundSpeed = (ushort)I_GroundSpeed.Value;
            item.Unpassable = (bool)I_Unpassable.IsChecked;
            item.Movable = (bool)I_Movable.IsChecked;
            item.BlockMissiles = (bool)I_BlockMissiles.IsChecked;
            item.BlockPathfinder = (bool)I_BlockPath.IsChecked;
            item.Pickupable = (bool)I_Pickable.IsChecked;
            item.Stackable = (bool)I_Stackable.IsChecked;
            item.ForceUse = (bool)I_ForceUse.IsChecked;
            item.MultiUse = (bool)I_MultiUse.IsChecked;
            item.Rotatable = (bool)I_Rotateable.IsChecked;
            item.Hangable = (bool)I_Hangable.IsChecked;
            item.HookSouth = (bool)I_HookSouth.IsChecked;
            item.HookEast = (bool)I_HookEast.IsChecked;
            item.HasElevation = (bool)I_Elevation.IsChecked;
            item.IgnoreLook = (bool)I_IgnoreLook.IsChecked;
            item.Readable = (bool)I_Readable.IsChecked;
            item.FullGround = (bool)I_FullGround.IsChecked;
            item.MinimapColor = (ushort)I_MiniMapColor.Value;
            item.TradeAs = (ushort)I_ShowAs.Value;
            item.LightLevel = (ushort)I_LightLevel.Value;
            item.LightColor = (ushort)I_LightColor.Value;
            item.MaxReadWriteChars = (ushort)I_MaxReadWriteChars.Value;
            item.MaxReadChars = (ushort)I_MaxReadChars.Value;
        }

        private int GetSpriteIndex(FrameGroup frameGroup, int layers, int patternX, int patternY, int patternZ, int frames)
        {
            var spriteInfo = frameGroup.SpriteInfo;
            int index = 0;

            if (spriteInfo.Animation != null)
                index = (int)(frames % spriteInfo.Animation.SpritePhase.Count);
            index = index * (int)spriteInfo.PatternDepth + patternZ;
            index = index * (int)spriteInfo.PatternHeight + patternY;
            index = index * (int)spriteInfo.PatternWidth + patternX;
            index = index * (int)spriteInfo.Layers + layers;
            return index;
        }

        private void EnableItemsXMLElements()
        {
            // Adding things to I_Type combobox
            I_Type.Items.Add(new ComboBoxItem() { Content = "Key" });
            I_Type.Items.Add(new ComboBoxItem() { Content = "Magic Field" });
            I_Type.Items.Add(new ComboBoxItem() { Content = "Depot" });
            I_Type.Items.Add(new ComboBoxItem() { Content = "Mailbox" });
            I_Type.Items.Add(new ComboBoxItem() { Content = "Trashholder" });
            I_Type.Items.Add(new ComboBoxItem() { Content = "Teleport" });
            I_Type.Items.Add(new ComboBoxItem() { Content = "Door" });
            I_Type.Items.Add(new ComboBoxItem() { Content = "Bed" });
            I_Type.Items.Add(new ComboBoxItem() { Content = "Rune" });
        }

        private Storyboard StartSingleSpriteAnimation(Image imageControl, Appearance appearance)
        {
            TimeSpan frameRate = TimeSpan.FromMilliseconds(200);
            List<BitmapImage> imageFrames = new List<BitmapImage>();
            try
            {
                for (int i = 0; i < appearance.FrameGroup[0].SpriteInfo.SpriteId.Count; i++)
                {
                    int index = GetSpriteIndex(appearance.FrameGroup[0], 0, (int)Math.Min(2, appearance.FrameGroup[0].SpriteInfo.PatternWidth - 1), (int)Math.Min(1, appearance.FrameGroup[0].SpriteInfo.PatternHeight - 1), 0, i);
                    BitmapImage imageFrame = Utils.BitmapToBitmapImage(MainWindow.getSpriteStream((int)appearance.FrameGroup[0].SpriteInfo.SpriteId[index]));
                    imageFrames.Add(imageFrame);
                }
            }
            catch
            {
                MainWindow.Log("Error animation for sprite " + appearance.Id + ", crash prevented.");
            }

            if (imageControl == null) throw new ArgumentNullException(nameof(imageControl));
            var animation = new ObjectAnimationUsingKeyFrames();
            TimeSpan currentTime = TimeSpan.Zero;

            foreach (BitmapImage imageFrame in imageFrames)
            {
                var keyFrame = new DiscreteObjectKeyFrame(imageFrame, currentTime);
                animation.KeyFrames.Add(keyFrame);
                currentTime += frameRate;
            }

            Storyboard.SetTarget(animation, imageControl);
            Storyboard.SetTargetProperty(animation, new PropertyPath(Image.SourceProperty));

            var storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            storyboard.RepeatBehavior = RepeatBehavior.Forever;
            storyboard.Begin();

            return storyboard;
        }
        private void ItemShootType_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            int shootTypeId = (int)I_ShootType.Value;
            try
            {
                if (shootTypeId > 0 && MainWindow.appearances.Missile.Any(a => a.Id == shootTypeId))
                {
                    var missile = MainWindow.appearances.Missile.FirstOrDefault(a => a.Id == shootTypeId);
                    if (missile != null && missile.FrameGroup.Count > 0 && missile.FrameGroup[0].SpriteInfo.SpriteId.Count > 0)
                        ShootTypeImage.Source = Utils.BitmapToBitmapImage(MainWindow.getSpriteStream((int)missile.FrameGroup[0].SpriteInfo.SpriteId[0]));
                    else if (ShootTypeImage != null)
                        ShootTypeImage.Source = null;
                }
                else if (ShootTypeImage != null)
                    ShootTypeImage.Source = null;
            }
            catch (Exception)
            {
                MainWindow.Log("Invalid appearance properties for id " + shootTypeId + ", crash prevented.");
            }
        }
        private void ItemEffectType_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            int effectTypeId = (int)I_EffectType.Value;
            try
            {
                if (effectTypeId > 0 && MainWindow.appearances.Effect.Any(a => a.Id == effectTypeId))
                {
                    var effect = MainWindow.appearances.Effect.FirstOrDefault(a => a.Id == effectTypeId);
                    if (effect != null)
                        itemsXMLStoryboard = StartSingleSpriteAnimation(EffectTypeImage, effect);
                }
                else
                {
                    itemsXMLStoryboard?.Stop();
                }
            }
            catch
            {
                MainWindow.Log("Invalid appearance properties for id " + effectTypeId + ", crash prevented.");
            }
        }

        private void ItemRotateTo_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // This will change the visible item based on the server ID
        }

        private void I_Type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            if (comboBox != null)
            {
                ComboBoxItem selectedItem = comboBox.SelectedValue as ComboBoxItem;
                if (selectedItem != null && (string)selectedItem.Content == "Container")
                {
                    if (I_ContainerSize != null)
                        I_ContainerSize.Visibility = Visibility.Visible;
                    if (I_ContainerSizeLabel != null)
                        I_ContainerSizeLabel.Visibility = Visibility.Visible;

                }
                else if (selectedItem != null && (string)selectedItem.Content == "Rune")
                {
                    if (I_RuneName != null)
                        I_RuneName.Visibility = Visibility.Visible;
                    if (I_RuneNameLabel != null)
                        I_RuneNameLabel.Visibility = Visibility.Visible;
                }
                else
                {
                    if (I_ContainerSize != null)
                        I_ContainerSize.Visibility = Visibility.Collapsed;
                    if (I_ContainerSizeLabel != null)
                        I_ContainerSizeLabel.Visibility = Visibility.Collapsed;
                    if (I_RuneName != null)
                        I_RuneName.Visibility = Visibility.Collapsed;
                    if (I_RuneNameLabel != null)
                        I_RuneNameLabel.Visibility = Visibility.Collapsed;
                }
            }
        }
    }
}
