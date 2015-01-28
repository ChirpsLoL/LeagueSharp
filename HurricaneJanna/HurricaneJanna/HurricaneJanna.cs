using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace HurricaneJanna
{
    public class HurricaneJanna
    {
        private static Obj_AI_Hero Player = ObjectManager.Player;
        private static Orbwalking.Orbwalker Orbwalker;
        private static readonly Spell Q = new Spell(SpellSlot.Q, 1100);
        private static readonly Spell W = new Spell(SpellSlot.W, 600);
        private static readonly Spell E = new Spell(SpellSlot.E, 800);
        private static readonly Spell R = new Spell(SpellSlot.R, 725);
        private static Menu Menu;
        private const String version = "0.0.0.1";
        private static int[] QWE = { 1, 3, 2, 1, 1, 4, 1, 2, 1, 2, 4, 2, 2, 3, 3, 4, 3, 3 };
        private static int[] QEW = { 1, 2, 3, 1, 1, 4, 1, 3, 1, 3, 4, 3, 3, 2, 2, 4, 2, 2 };
        private static int[] WQE = { 2, 1, 3, 2, 2, 4, 2, 1, 2, 1, 4, 1, 1, 3, 3, 4, 3, 3 };
        private static int[] WEQ = { 2, 3, 1, 2, 2, 4, 2, 3, 2, 3, 4, 3, 3, 1, 1, 4, 1, 1 };
        private static int[] EQW = { 3, 1, 2, 3, 3, 4, 3, 1, 3, 1, 4, 1, 1, 2, 2, 4, 2, 2 };
        private static int[] EWQ = { 3, 2, 1, 3, 3, 4, 3, 2, 3, 2, 4, 2, 2, 1, 1, 4, 1, 1 };

        private static StringList Orders = new StringList(new[] { "QWE", "QEW", "WQE", "WEQ", "EQW", "EWQ" }, 2);

        public HurricaneJanna()
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        public Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Janna") return;

            Menu = new Menu("Hurricane Janna", "hj");
            Menu orbwalkermMenu = Menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            Orbwalking.Orbwalker(orbwalkermMenu);
            Menu targetSelectorMenu = Menu.AddSubMenu(new Menu("Target Selector", "targetsel"));
            TargetSelector.AddToMenu(targetSelectorMenu);

            Menu.AddSubMenu(new Menu("Combo", "combo"));

            Menu.AddSubMenu(new Menu("Harass", "harass"));

            Menu.AddSubMenu(new Menu("Misc", "misc"));

            Menu.AddSubMenu(new Menu("Items", "items"));

            Menu.AddSubMenu(new Menu("QSS", "qss"));

            Menu.AddSubMenu(new Menu("AutoLeveler", "autolvl"));
            Menu.SubMenu("AutoLevel").AddItem(new MenuItem("ALSeq", "AutoLevel Seq").SetValue(Orders));
            Menu.SubMenu("AutoLevel").AddItem(new MenuItem("ALAct", "AutoLevel Active").SetValue(false));

            Menu.AddToMainMenu();

            Game.PrintChat("<font color='#FF0000'>HurricaneJanna</font> <font color='#FFFFFF'>"+ version +" loaded!</font>");

            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Drawing.OnDraw += Drawing_OnDraw;

            Menu.Item("ALAct").ValueChanged += AutoLevel_ValueChanged;
            if (isMenuEnabled("ALAct") && !aLInit)
            {
                var AutoLevel_I =
                    new AutoLevel(
                        getSequence(
                            Menu.Item("ALSeq").GetValue<StringList>().SList[
                                Menu.Item("ALSeq").GetValue<StringList>().SelectedIndex]));
                aLInit = true;
            }
        }
    }
}