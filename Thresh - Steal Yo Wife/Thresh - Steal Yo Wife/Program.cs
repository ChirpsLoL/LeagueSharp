using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace Thresh___Steal_Yo_Wife
{
    class Program
    {
        private static Obj_AI_Hero player = ObjectManager.Player;
        private static Orbwalking.Orbwalker Orbwalker;
        private static readonly Spell Q = new Spell(SpellSlot.Q, 1075);
        private static readonly Spell W = new Spell(SpellSlot.W, 950);
        private static readonly Spell E = new Spell(SpellSlot.E, 500);
        private static readonly Spell R = new Spell(SpellSlot.R, 400);
        private static Menu menu;
        private const String ver = "1.0.0.0";
        private static int qTick;
        private static Obj_AI_Base hookedUnit;
        private static int hookTick;

        private static List<Vector3> escapeSpots = new List<Vector3>();
        private static List<GameObject> soulList = new List<GameObject>();

        private static Obj_AI_Hero currentTarget
        {
            get
            {
                if (Hud.SelectedUnit != null && Hud.SelectedUnit is Obj_AI_Hero && Hud.SelectedUnit.Team != player.Team)
                    return (Obj_AI_Hero) Hud.SelectedUnit;
                if (TargetSelector.GetSelectedTarget() != null)
                    return TargetSelector.GetSelectedTarget();
                return TargetSelector.GetTarget(qRange + 175, TargetSelector.DamageType.Physical);
            }
        }

        private static float qRange
        {
            get { return menu.SubMenu("misc").Item("QRange").GetValue<Slider>().Value;  }
        }

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnLoad;
        }

        private static void OnLoad(EventArgs args)
        {
            if (player.ChampionName != "Thresh")
                return;

            Q.SetSkillshot(0.5f,70,1900,true,SkillshotType.SkillshotLine);
            W.SetSkillshot(0f,200,1750,false,SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.3f,60, float.MaxValue,false,SkillshotType.SkillshotLine);

            menu = new Menu("Thresh - Steal Yo Wife", "thresh", true);
            Menu orbwalkerMenu = menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            xSLxOrbwalker.AddToMenu(orbwalkerMenu);
            Menu targetMenu = menu.AddSubMenu(new Menu("Target Selector", "TargetSel"));
            TargetSelector.AddToMenu(targetMenu);
            menu.AddSubMenu(new Menu("Combo Settings", "combo"));
            menu.SubMenu("combo").AddItem(new MenuItem("QC", "Use Q Combo").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("QFC", "Q Follow Combo").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("WC", "Use W Combo").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("EC", "Use E Combo").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("EPushC", "Push away targets if HP Low").SetValue(new Slider(20,1,100)));
            menu.SubMenu("combo").AddItem(new MenuItem("RC", "Use R Combo").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("MaxRNum", "Use W Combo").SetValue(new Slider(2,1,5)));
            menu.AddSubMenu(new Menu("Flay Settings", "flay"));
            menu.SubMenu("flay").AddSubMenu(new Menu("Flay Per-Enemy", "whichFlay"));
            foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>().Where(unit => unit.Team != player.Team))
            {
                menu.SubMenu("flay")
                    .SubMenu("whichFlay")
                    .AddItem(
                        new MenuItem(enemy.ChampionName, enemy.ChampionName).SetValue(
                            new StringList(new[] {"Pull", "Pull"})));
            }
            menu.AddSubMenu("Lantern Settings", "lantern");
            menu.SubMenu("lantern").AddItem(new MenuItem("UseW", "Throw to Ally").SetValue(true));
            menu.SubMenu("lantern").AddItem(new MenuItem("NumWEnm", "Throw if # of Enemies").SetValue(new Slider(2, 1, 5)));
            menu.SubMenu("lantern").AddItem(new MenuItem("UseWCC", "Throw to CC'd Ally").SetValue(true));
            menu.AddSubMenu(new Menu("MadLife", "madlife"));
            menu.SubMenu("madlife").AddItem(new MenuItem("QWQPlay", "Jungle QWQE Gank Setup").SetValue(true));
            menu.SubMenu("madlife").AddItem(new MenuItem("QQREPlay", "Jungle QQRE Play Setup").SetValue(true));
            menu.AddSubMenu(new Menu("Misc", "misc"));
            menu.SubMenu("misc")
                .AddItem(new MenuItem("QRange", "Q Attempt Range").SetValue(new Slider(1075, 700, 1075)));
            menu.SubMenu("misc")
                .AddItem(
                    new MenuItem("QHitChance", "Q Hit Chance").SetValue(
                        new StringList(new[] {"Very High", "High", "Medium", "Low"}, 1)));
            menu.SubMenu("misc").AddItem(new MenuItem("packetCasting", "Use Packets").SetValue(true));
            menu.SubMenu("misc").AddSubMenu(new Menu("Gapclosers", "Gapclosers"));
            if (ObjectManager.Get<Obj_AI_Hero>().Any(unit => unit.Team != player.Team && unit.ChampionName == "Rengar"))
            {
                menu.SubMenu("misc")
                    .SubMenu("Gapclosers")
                    .AddItem(new MenuItem("rengarleap", "Rengar - Unseen Predator").SetValue(true));
            }
            foreach (Gapcloser spell in AntiGapcloser.Spells)
            {
                foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>().Where(unit => unit.Team != player.Team))
                {
                    if (spell.ChampionName == enemy.ChampionName)
                    {
                        menu.SubMenu("misc")
                            .SubMenu("Gapclosers")
                            .AddItem(
                                new MenuItem(spell.SpellName, spell.ChampionName + " - " + spell.SpellName).SetValue(
                                    true));
                    }
                }
            }
            menu.SubMenu("misc").AddSubMenu(new Menu("Interruptble Spells", "InterruptSpells"));
            foreach (InterruptableSpell spell in Interrupter.Spells)
            {
                foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>().Where(unit => unit.Team != player.Team))
                {
                    if (spell.ChampionName == enemy.ChampionName)
                    {
                        menu.SubMenu("misc")
                            .SubMenu("InterruptSpells")
                            .AddSubMenu(new Menu(enemy.ChampionName + " - " + spell.SpellName, spell.SpellName));
                        menu.SubMenu("misc")
                            .SubMenu("InterruptSpells")
                            .SubMenu(spell.SpellName)
                            .AddItem(new MenuItem("enabled", "Enabled").SetValue(true));
                        menu.SubMenu("misc")
                            .SubMenu("InterruptSpells")
                            .SubMenu(spell.SpellName)
                            .AddItem(new MenuItem("useEInt", "Interrupt with Flay").SetValue(true));
                        menu.SubMenu("misc")
                            .SubMenu("InterruptSpells")
                            .SubMenu(spell.SpellName)
                            .AddItem(new MenuItem("useQInt", "Interrupt with Hook").SetValue(true));
                    }
                }
            }
            menu.AddSubMenu(new Menu("Draw Settings", "Draw"));
            menu.SubMenu("Draw")
                .AddItem(new MenuItem("drawQMax", "Draw Q Max Range").SetValue(new Circle(true, Color.Red)));
            menu.SubMenu("Draw")
                .AddItem(new MenuItem("drawQEffective", "Draw Q Effective").SetValue(new Circle(true, Color.Blue)));
            menu.SubMenu("Draw")
                .AddItem(new MenuItem("drawW", "Draw W Range").SetValue(new Circle(false, Color.Green)));
            menu.SubMenu("Draw")
                .AddItem(new MenuItem("drawE", "Draw E Range").SetValue(new Circle(false, Color.Aqua)));
            menu.SubMenu("Draw").AddItem(new MenuItem("drawQCol", "Draw Q Line").SetValue(true));
            menu.SubMenu("Draw").AddItem(new MenuItem("drawTargetC", "Draw Target (Circle)").SetValue(true));
            menu.SubMenu("Draw").AddItem(new MenuItem("drawTargetT", "Draw Target (Text)").SetValue(true));
            menu.SubMenu("Draw")
                .AddItem(new MenuItem("drawSouls", "Draw Circle on Souls").SetValue(new Circle(true, Color.DeepSkyBlue)));
            menu.AddToMainMenu();

            //Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;
            //Obj_AI_Base.OnPlayAnimation += OnAnimation;
            Drawing.OnDraw += OnDraw;
            Game.OnGameUpdate += OnGameUpdate;
            GameObject.OnCreate += OnCreateObj;
            AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
            Interrupter.OnPossibleToInterrupt += OnPossibleToInterrupt;

            Game.PrintChat("<font color=\"#FF00FF\"> Thresh - Steal Yo Wife v" + ver + " loaded!</font>");
        }

        public static void OnGameUpdate(EventArgs args)
        {
            //AutoBox();
            //KS();
            //Lantern();
            UpdateSouls();
            UpdateBuffs();

            //if (xSLxOrbwalker.CurrentMode == xSLxOrbwalker.Mode.Combo)
                //Combo();

            //if (xSLxOrbwalker.CurrentMode == xSLxOrbwalker.Mode.Harass)
                //Harass();
        }

        public static void OnDraw(EventArgs args)
        {
            if (menu.SubMenu("Draw").Item("drawQMax").GetValue<Circle>().Active && !player.IsDead)
            {
                Utility.DrawCircle(player.Position, Q.Range,
                    menu.SubMenu("Draw").Item("drawQMax").GetValue<Circle>().Color);
            }

            if (menu.SubMenu("Draw").Item("drawQEffective").GetValue<Circle>().Active && !player.IsDead)
            {
                Utility.DrawCircle(player.Position, qRange,
                    menu.SubMenu("Draw").Item("drawQEffective").GetValue<Circle>().Color);
            }

            if (menu.SubMenu("Draw").Item("drawW").GetValue<Circle>().Active && !player.IsDead)
            {
                Utility.DrawCircle(player.Position, W.Range,
                    menu.SubMenu("Draw").Item("drawW").GetValue<Circle>().Color);
            }

            if (menu.SubMenu("Draw").Item("drawE").GetValue<Circle>().Active && !player.IsDead)
            {
                Utility.DrawCircle(player.Position, E.Range,
                    menu.SubMenu("Draw").Item("drawE").GetValue<Circle>().Color);
            }

            if (menu.SubMenu("Draw").Item("drawQCol").GetValue<bool>() && !player.IsDead)
            {
                if (player.Distance(currentTarget) < qRange + 200)
                {
                    Vector2 playerPos = Drawing.WorldToScreen(player.Position);
                    Vector2 targetPos = Drawing.WorldToScreen(currentTarget.Position);
                    Drawing.DrawLine(playerPos, targetPos, 4,
                        Q.GetPrediction(currentTarget, overrideRange: qRange).Hitchance < GetSelectedHitChance()
                            ? Color.Red
                            : Color.Green);
                }
            }

            if (menu.SubMenu("Draw").Item("drawTargetC").GetValue<bool>() && currentTarget.IsVisible &&
                !currentTarget.IsDead)
            {
                Utility.DrawCircle(currentTarget.Position, currentTarget.BoundingRadius + 10, Color.Red);
                Utility.DrawCircle(currentTarget.Position, currentTarget.BoundingRadius + 25, Color.Red);
                Utility.DrawCircle(currentTarget.Position, currentTarget.BoundingRadius + 45, Color.Red);
            }

            if (menu.SubMenu("Draw").Item("drawTargetT").GetValue<bool>() && !currentTarget.IsDead)
            {
                Drawing.DrawText(100, 150, Color.Red, "Current Target: " + currentTarget.ChampionName);
            }

            if (menu.SubMenu("Draw").Item("drawSouls").GetValue<Circle>().Active && !player.IsDead)
            {
                foreach (GameObject soul in soulList.Where(s => s.IsValid))
                {
                    Utility.DrawCircle(soul.Position, 50,
                        menu.SubMenu("Draw").Item("drawSouls").GetValue<Circle>().Color);
                }
            }
        }

        public static void OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (
                menu.SubMenu("misc")
                    .SubMenu("InterruptSpells")
                    .SubMenu(spell.SpellName)
                    .Item("enabled")
                    .GetValue<bool>())
            {
                if (
                    menu.SubMenu("misc")
                        .SubMenu("InterruptSpells")
                        .SubMenu(spell.SpellName)
                        .Item("useEInt")
                        .GetValue<bool>() && E.IsReady() && player.Distance(unit) < E.Range)
                {
                    /*if (ShouldPull((Obj_AI_Hero) unit))
                        PullFlay(unit);*/
                    //else
                        PushFlay(unit);
                }
                else if (
                    menu.SubMenu("misc")
                        .SubMenu("InterruptSpells")
                        .SubMenu(spell.SpellName)
                        .Item("useQInt")
                        .GetValue<bool>() && Q.IsReady() && !Q.GetPrediction(unit).CollisionObjects.Any())
                {
                    Q.Cast(unit, PacketCasting());
                }
            }
        }

        public static void OnCreateObj(GameObject obj, EventArgs args)
        {
            if (obj.Name.Contains("ChaosMinion") && obj.Team == player.Team)
            {
                soulList.Add(obj);
            }
        }

        public static void UpdateSouls()
        {
            foreach (GameObject soul in soulList.Where(soul => !soul.IsValid))
            {
                soulList.Remove(soul);
            }
        }

        public static void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var GapSender = (Obj_AI_Hero) gapcloser.Sender;
            var GapSpell = GapSender.LastCastedspell().Name.ToLower();
            if (E.IsReady() &&
                menu.SubMenu("misc")
                    .SubMenu("Gapclosers")
                    .Item(GapSender.LastCastedspell().Name.ToLower())
                    .GetValue<bool>() && player.Distance(GapSender) < E.Range + 100)
            {
                if (player.Distance(gapcloser.Start) < player.Distance(gapcloser.End))
                    PullFlay(GapSender);
                else
                    PushFlay(GapSender);
            }
        }

        public static void UpdateBuffs()
        {
            if (hookedUnit == null)
            {
                foreach (Obj_AI_Base obj in ObjectManager.Get<Obj_AI_Base>().Where(unit => unit.Team != player.Team))
                {
                    if (obj.HasBuff("threshqfakeknockup"))
                    {
                        hookedUnit = obj;
                        hookTick = Environment.TickCount + 1500;
                        return;
                    }
                }
            }
            hookTick = 0;
            hookedUnit = null;
        }

        public static bool isQ1()
        {
            return Q.Instance.Name == "ThreshQ";
        }

        public static bool isQ2()
        {
            return Q.Instance.Name == "threshqleap";
        }
        public static bool IsImmune(Obj_AI_Base unit)
        {
            return unit.HasBuff("BlackShield") || unit.HasBuff("SivirE") || unit.HasBuff("NocturneShroudofDarkness") ||
                   unit.HasBuff("deathdefiedbuff");
        }
        public static bool PacketCasting()
        {
            return menu.SubMenu("misc").Item("packetCasting").GetValue<bool>();
        }
        private static HitChance GetSelectedHitChance()
        {
            switch (menu.SubMenu("misc").Item("QHitChance").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.VeryHigh;
                case 1:
                    return HitChance.High;
                case 2:
                    return HitChance.Medium;
                case 3:
                    return HitChance.Low;
            }
            return HitChance.Medium;
        }

        public static void PushFlay(Obj_AI_Base unit)
        {
            if (player.Distance(unit) <= E.Range)
            {
                E.Cast(unit.ServerPosition, PacketCasting());
            }
        }
        public static void PullFlay(Obj_AI_Base unit)
        {
            if (player.Distance(unit) <= E.Range)
            {
                float pX = player.Position.X + (player.Position.X - unit.Position.X);
                float pY = player.Position.Y + (player.Position.Y - unit.Position.Y);
                E.Cast(new Vector2(pX, pY), PacketCasting());
            }
        }

        /*public static void Flay(Obj_AI_Hero unit)
        {
            if (ShouldPull(unit))
            {
                PullFlay();
            }
            else
            {
                PushFlay();
            }
        }*/

        public static Obj_AI_Hero GetNearAlly()
        {
            if (Hud.SelectedUnit != null && Hud.SelectedUnit is Obj_AI_Hero && Hud.SelectedUnit.Team == player.Team &&
                player.Distance(Hud.SelectedUnit.Position) <= W.Range + 200)
            {
                return (Obj_AI_Hero) Hud.SelectedUnit;
            }

            Obj_AI_Hero nearAlly = null;
            foreach (
                Obj_AI_Hero ally in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(
                            unit => unit.Team == player.Team && !unit.IsDead && player.Distance(unit) <= W.Range + 200))
            {
                if (nearAlly == null)
                    nearAlly = ally;
                else if (!nearAlly.IsDead && player.Distance(ally) < player.Distance(nearAlly))
                    nearAlly = ally;
            }
            return nearAlly;
        }

        public static Obj_AI_Hero GetLowHero()
        {
            Obj_AI_Hero lowAlly = null;
            foreach (
                Obj_AI_Hero ally in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(
                            unit => unit.Team == player.Team && !unit.IsDead && player.Distance(unit) <= W.Range + 200))
            {
                if (lowAlly == null)
                    lowAlly = ally;
                else if (!lowAlly.IsDead && ally.Health/ally.MaxHealth < lowAlly.Health/lowAlly.MaxHealth)
                    lowAlly = ally;
            }
            return lowAlly;
        }
        private static Obj_AI_Hero GetCCAlly()
        {
            return
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(
                        unit =>
                            !unit.IsMe && unit.Team == player.Team && !unit.IsDead &&
                            player.Distance(unit) <= W.Range + 200)
                    .FirstOrDefault(
                        ally =>
                            ally.HasBuffOfType(BuffType.Charm) || ally.HasBuffOfType(BuffType.CombatDehancer) ||
                            ally.HasBuffOfType(BuffType.Fear) || ally.HasBuffOfType(BuffType.Knockback) ||
                            ally.HasBuffOfType(BuffType.Knockup) || ally.HasBuffOfType(BuffType.Polymorph) ||
                            ally.HasBuffOfType(BuffType.Snare) || ally.HasBuffOfType(BuffType.Stun) ||
                            ally.HasBuffOfType(BuffType.Suppression) || ally.HasBuffOfType(BuffType.Taunt));
        }
    }
}
