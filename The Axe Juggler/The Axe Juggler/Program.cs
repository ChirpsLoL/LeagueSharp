using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;


namespace The_Axe_Juggler
{
    class Program
    {
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        private static Orbwalking.Orbwalker Orbwalker;
        private static Spell Q, W, E, R;
        private static Menu Menu;
        private static string version = 1.0.1.13;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Draven")
                return;

            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 1050);
            R = new Spell(SpellSlot.R, 20000);

            E.SetSkillshot(250f, 130f, 1400f,false, SkillshotType.SkillshotLine);
            R.SetSkillshot(400f, 160f, 2000f, false, SkillshotType.SkillshotLine);

            Menu = new Menu("Draven - The Axe Juggler", "draven");
            Menu orbwalkerMenu = Menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            Menu tsmMenu = Menu.AddSubMenu(new Menu("Target Selector", "TargetSel"));
            TargetSelector.AddToMenu(tsmMenu);
            Menu.AddSubMenu(new Menu("Q Settings", "QMenu"));
            Menu.SubMenu("QMenu").AddItem(new MenuItem("QC", "Use Q Combo").SetValue(true));
            Menu.SubMenu("QMenu").AddItem(new MenuItem("QM", "Use Q Mixed").SetValue(false));
            Menu.SubMenu("QMenu").AddItem(new MenuItem("QLH", "Use Q LastHit").SetValue(false));
            Menu.SubMenu("QMenu").AddItem(new MenuItem("QLC", "Use Q LaneClear").SetValue(false));
            Menu.SubMenu("QMenu").AddItem(new MenuItem("QKs", "Use Q KS").SetValue(true));
            Menu.SubMenu("QMenu").AddItem(new MenuItem("MaxQNum", "Max # of Q").SetValue(new Slider(2, 1, 4)));
            Menu.SubMenu("QMenu").AddItem(new MenuItem("SafeZone", "BETA SafeZone").SetValue(new Slider(100, 0, 400)));
            Menu.SubMenu("QMenu").AddItem(new MenuItem("QRadius", "Catch Radius").SetValue(new Slider(600, 200, 800)));
            Menu.SubMenu("QMenu").AddItem(new MenuItem("QManaC", "Min Q Mana in Combo").SetValue(new Slider(10, 0, 100)));
            Menu.SubMenu("QMenu").AddItem(new MenuItem("QManaM", "Min Q Mana in Mixed").SetValue(new Slider(10, 0, 100)));
            Menu.AddSubMenu(new Menu("W Settings", "WMenu"));
            Menu.SubMenu("WMenu").AddItem(new MenuItem("WC", "Use W Combo").SetValue(true));
            Menu.SubMenu("WMenu").AddItem(new MenuItem("WM", "Use W Mixed").SetValue(true));
            Menu.SubMenu("WMenu").AddItem(new MenuItem("WLH", "Use W LastHit").SetValue(false));
            Menu.SubMenu("WMenu").AddItem(new MenuItem("WLC", "Use W LaneClear").SetValue(false));
            Menu.SubMenu("WMenu").AddItem(new MenuItem("WManaC", "Min W Mana in Combo").SetValue(new Slider(60, 1, 100)));
            Menu.SubMenu("WMenu").AddItem(new MenuItem("WManaM", "Min W Mana in Mixed").SetValue(new Slider(60, 1, 100)));
            Menu.AddSubMenu(new Menu("E Settings", "EMenu"));
            Menu.SubMenu("EMenu").AddItem(new MenuItem("EC", "Use W Combo").SetValue(true));
            Menu.SubMenu("EMenu").AddItem(new MenuItem("EM", "Use W Mixed").SetValue(false));
            Menu.SubMenu("EMenu").AddItem(new MenuItem("EKs", "Use E Ks").SetValue(true));
            Menu.SubMenu("EMenu").AddItem(new MenuItem("EGapCloser", "Use E AntiGapcloser").SetValue(true));
            Menu.SubMenu("EMenu").AddItem(new MenuItem("EInterrupt", "Use E Interrupt").SetValue(true));
            Menu.SubMenu("EMenu").AddItem(new MenuItem("EManaC", "Min E Mana in Combo").SetValue(new Slider(20, 1, 100)));
            Menu.SubMenu("EMenu").AddItem(new MenuItem("EManaM", "Min E Mana in Mixed").SetValue(new Slider(20, 1, 100)));
            Menu.AddSubMenu(new Menu("R Settings", "RMenu"));
            Menu.SubMenu("RMenu").AddItem(new MenuItem("RC", "Use R Combo").SetValue(true));
            Menu.SubMenu("RMenu").AddItem(new MenuItem("RM", "Use R Mixed").SetValue(false));
            Menu.SubMenu("RMenu").AddItem(new MenuItem("RKs", "Use R Ks").SetValue(true));
            Menu.SubMenu("RMenu").AddItem(new MenuItem("RManaC", "Min R Mana in Combo").SetValue(new Slider(5, 1, 100)));
            Menu.SubMenu("RMenu").AddItem(new MenuItem("RManaM", "Min R Mana in Mixed").SetValue(new Slider(5, 1, 100)));
            Menu.AddSubMenu(new Menu("Axe Juggles", "AxeJuggles"));
            Menu.SubMenu("AxeJuggles").AddItem(new MenuItem("ACC", "Axe Catch Combo").SetValue(true));
            Menu.SubMenu("AxeJuggles").AddItem(new MenuItem("ACM", "Axe Catch Mixed").SetValue(true));
            Menu.SubMenu("AxeJuggles").AddItem(new MenuItem("ACLH", "Axe Catch LastHit").SetValue(true));
            Menu.SubMenu("AxeJuggles").AddItem(new MenuItem("ACLC", "Axe Catch LaneClear").SetValue(true));
            Menu.AddSubMenu(new Menu("Items", "Items"));
            Menu.SubMenu("Items").AddItem(new MenuItem("BOTRK", "Use BOTRK").SetValue(true));
            Menu.SubMenu("Items").AddItem(new MenuItem("Youmuu", "Use Youmuu").SetValue(true));
            Menu.SubMenu("Items").AddItem(new MenuItem("SOTD", "Use SOTD if Oneshot").SetValue(true));
            Menu.AddSubMenu(new Menu("Drawings", "Drawings"));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("DrawCRange", "Draw CatchRange").SetValue(new Circle(true,Color.FromArgb(80, 255, 0, 255))));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("DrawRet", "Draw Reticles").SetValue(true, Color.Yellow));
        }
    }
}
