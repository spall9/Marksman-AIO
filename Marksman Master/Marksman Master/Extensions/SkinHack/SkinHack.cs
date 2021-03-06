﻿

namespace Marksman_Master.Extensions.SkinHack
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using EloBuddy;
    using EloBuddy.SDK.Menu;
    using EloBuddy.SDK.Menu.Values;
    using EloBuddy.SDK.Utils;

    internal sealed class SkinHack : ExtensionBase
    {
        private Menu SkinHackMenu { get; set; }

        public override bool IsEnabled { get; set; }

        public static bool EnabledByDefault { get; set; } = true;

        public override string Name { get; } = "SkinHack";

        public Dictionary<string, byte> Skins { get; private set; }
        public Dictionary<KeyValuePair<Champion, byte>, Dictionary<string, byte>> Chromas { get; private set; }
        public Dictionary<Champion, string> BaseSkinNames { get; private set; }

        public ComboBox SkinId { get; set; }
        public Slider ChromaId { get; set; }

        public byte LoadSkinId { get; private set; }

        public byte CurrentSkin { get; set; }

        public override void Load()
        {
            LoadSkinId = (byte) Player.Instance.SkinId;

            IsEnabled = true;

            BaseSkinNames = new Dictionary<Champion, string>
            {
                [Champion.Ashe] = "Ashe",
                [Champion.Caitlyn] = "Caitlyn",
                [Champion.Corki] = "Corki",
                [Champion.Draven] = "Draven",
                [Champion.Ezreal] = "Ezreal",
                [Champion.Graves] = "Graves",
                [Champion.Jhin] = "Jhin",
                [Champion.Jinx] = "Jinx",
                [Champion.Kalista] = "Kalista",
                [Champion.KogMaw] = "KogMaw",
                [Champion.Lucian] = "Lucian",
                [Champion.MissFortune] = "MissFortune",
                [Champion.Quinn] = "Quinn",
                [Champion.Sivir] = "Sivir",
                [Champion.Tristana] = "Tristana",
                [Champion.Twitch] = "Twitch",
                [Champion.Urgot] = "Urgot",
                [Champion.Varus] = "Varus",
                [Champion.Vayne] = "Vayne"
            };

            Chromas = new Dictionary<KeyValuePair<Champion, byte>, Dictionary<string, byte>>
            {
                {new KeyValuePair<Champion, byte>(Champion.Ezreal, 7), new Dictionary<string, byte>
                    {
                        {"Amethyst", 7},
                        {"Meteorite", 10},
                        {"Obsidian", 11},
                        {"Pearl", 12},
                        {"Rose", 13},
                        {"Quartz", 14},
                        {"Ruby", 15},
                        {"Sandstone", 16},
                        {"Striped", 17}
                    }
                },
                {new KeyValuePair<Champion, byte>(Champion.Caitlyn, 0), new Dictionary<string, byte>
                    {
                        {"Default", 0},
                        {"Pink", 7},
                        {"Green", 8},
                        {"Blue", 9}
                    }
                },
                {new KeyValuePair<Champion, byte>(Champion.Lucian, 0), new Dictionary<string, byte>
                    {
                        {"Default", 0},
                        {"Yellow", 3},
                        {"Red", 4},
                        {"Blue", 5}
                    }
                },
                {new KeyValuePair<Champion, byte>(Champion.MissFortune, 7), new Dictionary<string, byte>
                    {
                        {"Amethyst", 7},
                        {"Aquamarine", 11},
                        {"Citrine", 12},
                        {"Peridot", 13},
                        {"Ruby", 14}
                    }
                },
                {new KeyValuePair<Champion, byte>(Champion.Vayne, 3), new Dictionary<string, byte>
                    {
                        {"Default", 3},
                        {"Green", 7},
                        {"Red", 8},
                        {"Silver", 9}
                    }
                },
                {new KeyValuePair<Champion, byte>(Champion.Tristana, 6), new Dictionary<string, byte>
                    {
                        {"Default", 6},
                        {"Navy", 7},
                        {"Purple", 8},
                        {"Orange", 9}
                    }
                }
            };

            var skin = new SkinData(Player.Instance.ChampionName);

            Skins = skin.ToDictionary();
            
            if (!MenuManager.ExtensionsMenu.SubMenus.Any(x => x.UniqueMenuId.Contains("Extension.SkinHack")))
            {
                if (!MainMenu.IsOpen)
                {
                    SkinHackMenu = MenuManager.ExtensionsMenu.AddSubMenu("Skin Hack", "Extension.SkinHack");
                    BuildMenu();
                }
                else MainMenu.OnClose += MainMenu_OnClose;
            }
            else
            {
                var subMenu =
                    MenuManager.ExtensionsMenu.SubMenus.Find(x => x.UniqueMenuId.Contains("Extension.SkinHack"));

                if (subMenu?["SkinId." + Player.Instance.ChampionName] == null)
                    return;

                SkinId = subMenu["SkinId." + Player.Instance.ChampionName].Cast<ComboBox>();
                ChromaId = subMenu["ChromaId." + Player.Instance.ChampionName].Cast<Slider>();

                subMenu["SkinId." + Player.Instance.ChampionName].Cast<ComboBox>().OnValueChange += SkinId_OnValueChange;
                subMenu["ChromaId." + Player.Instance.ChampionName].Cast<Slider>().OnValueChange += ChromaId_OnValueChange;

                UpdateChromaSlider(SkinId.CurrentValue);

                if (HasChromaPack(SkinId.CurrentValue))
                {
                    ChangeSkin(SkinId.CurrentValue, ChromaId.CurrentValue);
                } else ChangeSkin(SkinId.CurrentValue);
            }

            Obj_AI_Base.OnUpdateModel += Obj_AI_Base_OnUpdateModel;
        }

        private void Obj_AI_Base_OnUpdateModel(Obj_AI_Base sender, UpdateModelEventArgs args)
        {
            if (!sender.IsMe || !IsEnabled)
                return;
            

            if (args.Model != BaseSkinNames[Player.Instance.Hero] || args.SkinId != SkinId.CurrentValue)
                args.Process = false;
        }

        private void MainMenu_OnClose(object sender, EventArgs args)
        {
            if (MenuManager.ExtensionsMenu.SubMenus.Any(x => x.UniqueMenuId.Contains("Extension.SkinHack")))
                return;

            SkinHackMenu = MenuManager.ExtensionsMenu.AddSubMenu("Skin Hack", "Extension.SkinHack");
            BuildMenu();

            MainMenu.OnClose -= MainMenu_OnClose;
        }

        private void BuildMenu()
        {
            var skins =
                Skins.Select(x => x.Key)
                    .ToList();

            if (!skins.Any())
                return;

            SkinHackMenu.AddGroupLabel("Skin hack settings : ");

            SkinId = SkinHackMenu.Add("SkinId." + Player.Instance.ChampionName, new ComboBox("Skin : ", skins));
            SkinHackMenu.AddSeparator(5);

            BuildChroma();
        }

        private void BuildChroma()
        {
            ChromaId = SkinHackMenu.Add("ChromaId." + Player.Instance.ChampionName, new Slider("Chroma : "));
            ChromaId.IsVisible = false;
            ChromaId.OnValueChange += ChromaId_OnValueChange;
            SkinId.OnValueChange += SkinId_OnValueChange;

            if (HasChromaPack(SkinId.CurrentValue))
            {
                var dictionary = GetChromaList(SkinId.CurrentValue);

                if (dictionary == null)
                {
                    ChangeSkin(SkinId.CurrentValue);

                    return;
                }
                var maxValue = dictionary.Select(x => x.Key).Count();

                ChromaId.MaxValue = maxValue - 1;

                ChromaId.DisplayName = GetChromaName(SkinId.CurrentValue, ChromaId.CurrentValue);

                ChromaId.IsVisible = true;

                if (Player.Instance.SkinId == 0)
                    ChangeSkin(SkinId.CurrentValue, ChromaId.CurrentValue);
            }
            else if(Player.Instance.SkinId == 0)
                ChangeSkin(SkinId.CurrentValue);
        }

        private void ChromaId_OnValueChange(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args)
        {
            var currentId = SkinId.CurrentValue;

            ChromaId.DisplayName = GetChromaName(SkinId.CurrentValue, ChromaId.CurrentValue);
            
            ChangeSkin(currentId, args.NewValue);
        }

        private void UpdateChromaSlider(int id)
        {
            var dictionary = GetChromaList(id);

            if (dictionary == null)
            {
                ChromaId.IsVisible = false;
                return;
            }

            var maxValue = dictionary.Select(x => x.Key).Count();

            ChromaId.MaxValue = maxValue - 1;

            ChromaId.DisplayName = GetChromaName(SkinId.CurrentValue, ChromaId.CurrentValue);

            ChromaId.IsVisible = true;
        }
        
        private void SkinId_OnValueChange(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args)
        {
            if (HasChromaPack(args.NewValue))
            {
                UpdateChromaSlider(args.NewValue);

                ChangeSkin(args.NewValue, ChromaId.CurrentValue);
                return;
            }

            ChromaId.IsVisible = false;

            ChangeSkin(args.NewValue);
        }

        private bool HasChromaPack(int id)
            => Chromas != null && Chromas.ContainsKey(new KeyValuePair<Champion, byte>(Player.Instance.Hero, (byte) id));

        private string GetChromaName(int id, int chromaId)
        {
            if (Chromas == null || !Chromas.ContainsKey(new KeyValuePair<Champion, byte>(Player.Instance.Hero, (byte) id)))
                return string.Empty;

            var dictionary = GetChromaList(id);
            var baseSkinName = Skins.FirstOrDefault(x => x.Value == id).Key;

            if (dictionary == null)
                return baseSkinName;

            var chromaIdT = dictionary.ElementAtOrDefault(chromaId).Key;

            return chromaIdT != default(string) ? $"{baseSkinName} : {chromaIdT} chroma" : baseSkinName;
        }

        private Dictionary<string, byte> GetChromaList(int id)
            =>
                !HasChromaPack(id)
                    ? null
                    : Chromas.FirstOrDefault(x => x.Key.Key == Player.Instance.Hero && x.Key.Value == id).Value;

        private void ChangeSkin(int id, int? chromaId = null)
        {
            if (!IsEnabled)
                return;

            var skins = Skins;

            if (skins == null)
            {
                return;
            }

            var skinId = skins.ElementAtOrDefault(id).Value;

            if (chromaId.HasValue && HasChromaPack(id))
            {
                var dictionary = GetChromaList(id);

                if (dictionary != null)
                {
                    var chromaIdT = dictionary.ElementAtOrDefault(chromaId.Value).Value;

                    if (chromaIdT != 0)
                    {
                        Player.Instance.SetSkin(BaseSkinNames[Player.Instance.Hero], chromaIdT);
                        return;
                    }
                }
            }

            Player.Instance.SetSkin(BaseSkinNames[Player.Instance.Hero], skinId);

            CurrentSkin = skinId;
        }

        public override void Dispose()
        {
            IsEnabled = false;
            
            SkinId.OnValueChange -= SkinId_OnValueChange;
            ChromaId.OnValueChange -= ChromaId_OnValueChange;

            MainMenu.OnClose -= MainMenu_OnClose;

            Obj_AI_Base.OnUpdateModel -= Obj_AI_Base_OnUpdateModel;

            Player.Instance.SetSkin(BaseSkinNames[Player.Instance.Hero], LoadSkinId);
        }

        public class SkinData
        {
            public string DDragonVersion { get; }
            public Skins SkinsData { get; }

            public SkinData(string championName)
            {
                try
                {
                    var realm =
                        new WebClient().DownloadString(new Uri("http://ddragon.leagueoflegends.com/realms/na.json"));

                    DDragonVersion = (string) JObject.Parse(realm).Property("dd");
                    
                    var output =
                        new WebClient().DownloadString(
                            new Uri(
                                $"http://ddragon.leagueoflegends.com/cdn/{DDragonVersion}/data/en_US/champion/{championName}.json"));

                    var parsedObject = JObject.Parse(output);
                    var data = parsedObject["data"][championName];

                    SkinsData = data.ToObject<Skins>();
                }
                catch (Exception exception)
                {
                    Logger.Info($"Couldn't load skinhack an exception occured\n{exception}");
                }
            }

            public Dictionary<string, byte> ToDictionary()
            {
                var output = new Dictionary<string, byte>();
                try
                {
                    foreach (var skin in SkinsData.SkinsInfos)
                    {
                        output[skin.SkinName] = (byte)skin.SkinId;
                    }
                }
                catch (Exception exception)
                {
                    Logger.Error($"Couldn't load skinhack an exception occured\n{exception}");
                }
                return output;
            }

            public class SkinInfo
            {
                [JsonProperty(PropertyName = "id")]
                public string GameSkinId { get; set; }

                [JsonProperty(PropertyName = "num")]
                public int SkinId { get; set; }

                [JsonProperty(PropertyName = "name")]
                public string SkinName { get; set; }

                [JsonProperty(PropertyName = "chromas")]
                public bool HasChromas { get; set; }
            }

            public class Skins
            {
                [JsonProperty(PropertyName = "skins")]
                public SkinInfo[] SkinsInfos { get; set; }
            }
        }
    }
}