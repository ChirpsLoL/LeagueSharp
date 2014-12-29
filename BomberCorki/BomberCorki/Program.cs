using System;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace BomberCorki
{
    class BomberCorki
    {
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        private static Orbwalking.Orbwalker Orbwalker;
        private static Spell Q, W, E, R;
        private static Items.Item Botrk, bilge;
        private static Menu Menu;

        private static int[] QWE = new[] { 1, 2, 3, 1, 1, 4, 1, 2, 1, 2, 4, 2, 2, 3, 3, 4, 3, 3 };
        private static int[] QEW = new[] { 1, 3, 2, 1, 1, 4, 1, 3, 1, 3, 4, 3, 3, 2, 2, 4, 2, 2 };
        private static int[] WQE = new[] { 2, 1, 3, 2, 2, 4, 2, 1, 2, 1, 4, 1, 1, 3, 3, 4, 3, 3 };
        private static int[] WEQ = new[] { 2, 3, 1, 2, 2, 4, 2, 3, 2, 3, 4, 3, 3, 1, 1, 4, 1, 1 };
        private static int[] EQW = new[] { 3, 1, 2, 3, 3, 4, 3, 1, 3, 1, 4, 1, 1, 2, 2, 4, 2, 2 };
        private static int[] EWQ = new[] { 3, 2, 1, 3, 3, 4, 3, 2, 3, 2, 4, 2, 2, 1, 1, 4, 1, 1 };

        private static StringList Orders = new StringList(new[] { "QWE", "QEW", "WQE", "WEQ", "EQW", "EWQ" }, 3);

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Corki")
                return;
            Q = new Spell(SpellSlot.Q, 825);
            W = new Spell(SpellSlot.W, 800);
            E = new Spell(SpellSlot.E, 600);
            R = new Spell(SpellSlot.R, 1225);

            Q.SetSkillshot(0.30f, 250f, 1000f, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.20f, 40f, 2000f, true, SkillshotType.SkillshotLine);

            bilge = new Items.Item((int)ItemId.Bilgewater_Cutlass, 450);
            Botrk = new Items.Item((int)ItemId.Blade_of_the_Ruined_King, 450);

            Menu = new Menu("BomberCorki", "corki", true);
            Menu orbwalkerMenu = Menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            var ts = new Menu("Target Selector", "TargetSel");
            TargetSelector.AddToMenu(ts);
            Menu.AddSubMenu(ts);
            Menu.AddSubMenu(new Menu("[BC] Combo", "Combo"));
            Menu.SubMenu("Combo").AddItem((new MenuItem("UseQC", "Use Q in Combo")).SetValue(true));
            Menu.SubMenu("Combo").AddItem((new MenuItem("UseEC", "Use E in Combo")).SetValue(true));
            Menu.SubMenu("Combo").AddItem((new MenuItem("UseRC", "Use R in Combo")).SetValue(true));
            Menu.SubMenu("Combo").AddItem((new MenuItem("RManaC", "Min R Mana %")).SetValue(new Slider(30, 1, 100)));
            Menu.AddSubMenu(new Menu("[BC] Harrass", "Harrass"));
            Menu.SubMenu("Harrass").AddItem((new MenuItem("UseQH", "Use Q in Harrass")).SetValue(true));
            Menu.SubMenu("Harrass").AddItem((new MenuItem("UseEH", "Use E in Harrass")).SetValue(true));
            Menu.SubMenu("Harrass").AddItem((new MenuItem("UseRH", "Use R in Harrass")).SetValue(true));
            Menu.SubMenu("Harrass").AddItem((new MenuItem("QManaH", "Min Q Mana %")).SetValue(new Slider(50, 1, 100)));
            Menu.SubMenu("Harrass").AddItem((new MenuItem("RManaH", "Min R Mana %")).SetValue(new Slider(50, 1, 100)));
            Menu.AddSubMenu(new Menu("[BC] Farm", "Farm"));
            Menu.SubMenu("Farm").AddItem((new MenuItem("UseQF", "Use Q in Harrass")).SetValue(true));
            Menu.SubMenu("Farm").AddItem((new MenuItem("UseEF", "Use E in Harrass")).SetValue(true));
            Menu.SubMenu("Farm").AddItem((new MenuItem("UseRF", "Use R in Harrass")).SetValue(true));
            Menu.SubMenu("Farm").AddItem((new MenuItem("QManaF", "Min Q Mana %")).SetValue(new Slider(50, 1, 100)));
            Menu.SubMenu("Farm").AddItem((new MenuItem("RManaF", "Min R Mana %")).SetValue(new Slider(50, 1, 100)));
            Menu.AddSubMenu(new Menu("[BC] Settings", "Settings"));
            Menu.SubMenu("Settings").AddItem(new MenuItem("ThreshLantern", "Grab Thresh Lantern").SetValue(new KeyBind("S".ToCharArray()[0], KeyBindType.Press)));
            Menu.AddSubMenu(new Menu("Items", "Items"));
            Menu.SubMenu("Items").AddItem((new MenuItem("Botrk", "Use Botrk")).SetValue(true));
            Menu.SubMenu("Items").AddItem((new MenuItem("bilge", "Use Bilgewater")).SetValue(true));
            Menu.SubMenu("Items").AddItem((new MenuItem("OwnHPBotrk", "Min own HP % Botrk")).SetValue(new Slider(50, 1, 100)));
            Menu.SubMenu("Items").AddItem((new MenuItem("EnmHPBotrk", "Min Enemy HP % Botrk")).SetValue(new Slider(20, 1, 100)));
            Menu.AddSubMenu(new Menu("Drawings", "Drawings"));
            Menu.SubMenu("Drawings").AddItem((new MenuItem("DrawAA", "Show AA Range")).SetValue(true));
            Menu.SubMenu("Drawings").AddItem((new MenuItem("DrawQ", "Show Q Range")).SetValue(true));
            Menu.SubMenu("Drawings").AddItem((new MenuItem("DrawR", "Show R Range")).SetValue(true));
            Menu.AddSubMenu(new Menu("[BC] AutoLeveler", "AutoLevel"));
            Menu.SubMenu("AutoLevel").AddItem(new MenuItem("ALSeq", "AutoLevel Seq").SetValue(Orders));
            Menu.SubMenu("AutoLevel").AddItem(new MenuItem("ALAct", "AutoLevel Active").SetValue(false));
            Menu.AddToMainMenu();
            Menu.Item("ALAct").ValueChanged += AutoLevel_ValueChanged;
            if (isMenuEnabled("ALAct"))
            {
                var AutoLevel = new AutoLevel(getSequence(Menu.Item("ALSeq").GetValue<StringList>().SList[Menu.Item("ALSeq").GetValue<StringList>().SelectedIndex]));

            }

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;

            Game.PrintChat("Bomber Corki by Chirps Loaded!");
        }

        private static void AutoLevel_ValueChanged(object sender, OnValueChangeEventArgs ev)
        {
            AutoLevel.Enabled(ev.GetNewValue<bool>());
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                Bomb("combo");
                Gun("combo");
                Rocket("combo");
                UseItems();
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                Bomb("mixed");
                Rocket("mixed");
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                Bomb("clear");
                Gun("clear");
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;
            if (isMenuEnabled("DrawAA"))
            {
                Utility.DrawCircle(Player.Position, Player.AttackRange, Color.CornflowerBlue);
            }
            if (isMenuEnabled("DrawQ"))
            {
                if (Q.IsReady())
                {
                    Utility.DrawCircle(Player.Position, Q.Range, Color.CornflowerBlue);
                }
            }
            if (isMenuEnabled("DrawR"))
            {
                if (R.IsReady())
                {
                    Utility.DrawCircle(Player.Position, R.Range, Color.CornflowerBlue);
                }
            }
        }

        private static void Bomb(String val)
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(825, TargetSelector.DamageType.Physical);
            if (val == "combo")
            {
                if (isMenuEnabled("UseQC"))
                {
                    if (Q.IsReady())
                    {
                        if (target.IsValidTarget(Q.Range))
                            Q.Cast(target, false, true);
                    }
                }
            }
            if (val == "mixed")
            {
                if (isMenuEnabled("UseQH"))
                {
                    if (Q.IsReady())
                    {
                        if (target.IsValidTarget(Q.Range))
                            Q.Cast(target, false, true);
                    }
                }
            }
            if (val == "clear")
            {
                if (isMenuEnabled("UseQF"))
                {
                    if (Q.IsReady())
                    {
                        var Minions = MinionManager.GetMinions(Player.Position, Q.Range, MinionTypes.All,
                            MinionTeam.NotAlly);
                        if (Minions.Count == 0) return;
                        if (Player.Mana/Player.MaxMana*100 >= GetSlider("QManaF"))
                        {
                            if (Minions.Count >= 4)
                            {
                                var castPostion = MinionManager.GetBestCircularFarmLocation(Minions.Select(minion => minion.ServerPosition.To2D()).ToList(), Q.Width, Q.Range);
                                Q.Cast(castPostion.Position, false);
                            }
                        }
                    }
                }
            }
        }

        private static void Gun(string val)
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(600, TargetSelector.DamageType.Physical);
            if (val == "combo")
            {
                if (isMenuEnabled("UseEC"))
                {
                    if (E.IsReady())
                    {
                        if (target.IsValidTarget(R.Range))
                            E.Cast(target, false);
                    }
                }
            }
            
            if (val == "clear")
            {
                if (E.IsReady())
                    {
                        var Minions = MinionManager.GetMinions(Player.Position, E.Range, MinionTypes.All,
                            MinionTeam.NotAlly);
                        if (Minions.Count == 0) return;
                        if (isMenuEnabled("UseEF"))
                        {
                            if (Minions.Count >= 8)
                            {
                                E.Cast(Player.Position, false);
                            }
                        }
                    }
                }
        }

        private static void Rocket(String val)
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(1225, TargetSelector.DamageType.Physical);
            if (val == "combo")
            {
                if (isMenuEnabled("UseRC"))
                {
                    if (R.IsReady())
                    {
                        if (Player.Mana / Player.MaxMana * 100 >= GetSlider("RManaC"))
                        {
                            if (target.IsValidTarget(Q.Range))
                                R.Cast(target, false, true);
                        }
                    }
                }
            }
            if (val == "mixed")
            {
                if (isMenuEnabled("UseRH"))
                {
                    if (R.IsReady())
                    {
                        if (Player.Mana/Player.MaxMana*100 >= GetSlider("RManaH"))
                        {
                            if (target.IsValidTarget(Q.Range))
                                R.Cast(target, false, true);
                        }
                    }
                }
            }
        }

        private static void UseItems()
        {
            Obj_AI_Hero target = TargetSelector.GetSelectedTarget();

            if (isMenuEnabled("bilge"))
            {
                if (bilge.IsReady())
                {
                    if (target.IsValidTarget((bilge.Range)))
                    {
                        bilge.Cast(target);
                    }
                }
            }

            if (isMenuEnabled("Botrk"))
            {
                if (Botrk.IsReady())
                {
                    if (Player.Health/Player.MaxHealth*100 >= GetSlider("OwnHPBortk"))
                    {
                        if (target.Health/target.MaxHealth*100 >= GetSlider("EnmHPBortk"))
                        {
                            if (target.IsValidTarget(Botrk.Range))
                            {
                                Botrk.Cast(target);
                            }
                        }
                    }
                }
            }
        }

        static int[] getSequence(String Order)
        {
            switch (Order)
            {
                case "QWE":
                    return QWE;
                case "QEW":
                    return QEW;
                case "WQE":
                    return WQE;
                case "EQW":
                    return EQW;
                case "WEQ":
                    return WEQ;
                case "EWQ":
                    return EWQ;
                default:
                    return null;
            }
        }

        public static bool isMenuEnabled(String val)
        {
            return Menu.Item(val).GetValue<bool>();
        }
        private static int GetSlider(string s)
        {
            return Menu.Item(s).GetValue<Slider>().Value;
        }
    }
}
