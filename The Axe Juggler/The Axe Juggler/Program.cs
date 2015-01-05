using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DZDraven;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;


namespace The_Axe_Juggler
{
    class Program
    {
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        private static Orbwalking.Orbwalker Orbwalker;
        private static Spell Q, W, E, R;
        private static Menu Menu;
        public static String verision = "1.0.1.13";
        public static List<Reticle> reticleList = new List<Reticle>();
        public static List<Obj_AI_Turret> towerPos = new List<Obj_AI_Turret>();
        public static bool isCatching = false;
        

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
            Menu.SubMenu("Drawings").AddItem(new MenuItem("DrawRet", "Draw Reticles").SetValue(new Circle(true, Color.Yellow)));
            Menu.AddToMainMenu();

            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 1100);
            R = new Spell(SpellSlot.R, 20000);
            E.SetSkillshot(250f,130f,1400f,false,SkillshotType.SkillshotLine);
            R.SetSkillshot(400f,160f,2000f,false,SkillshotType.SkillshotLine);

            compileTowerArray();
            GameObject.OnCreate += OnCreateObject;
            GameObject.OnDelete += OnDeleteObject;
            AntiGapcloser.OnEnemyGapcloser += OnGapcloser;
            Interrupter.OnPossibleToInterrupt += OnInterruptCreate;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            var QRadius = Menu.Item("QRadius").GetValue<Slider>().Value;
            var drawCatch = Menu.Item("DrawCRange").GetValue<Circle>();
            var drawRet = Menu.Item("DrawRet").GetValue<Circle>();
            if (drawCatch.Active)
            {
                Drawing.DrawCircle(Game.CursorPos, QRadius, drawCatch.Color);
            }
            if (drawRet.Active)
            {
                foreach (Reticle r in reticleList)
                {
                    if (r.getObj().IsValid)
                    {
                        Drawing.DrawCircle(r.getPosition(), 100, drawRet.Color);
                    }
                }
            }
        }

        static void Orbwalking_AfterAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
            if (!unit.IsMe) return;
            var tar = (Obj_AI_Hero)target;
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    if (isEn("QC")) { CastQ(); }
                    if (isEn("WC") &&
                        (ObjectManager.Player.Buffs.FirstOrDefault(
                            buff => buff.Name == "dravenfurybuff" || buff.Name == "DravenFury") == null))
                    {
                        var WManaCombo = Menu.Item("WManaC").GetValue<Slider>().Value;
                        if (getManaPer() >= WManaCombo) {W.Cast();}
                    }
                    if (isEn("BOTRK"))
                    {
                        useItem(3153, (Obj_AI_Hero) target);
                    }
                    if (isEn("Youmuu"))
                    {
                        useItem(3142);
                    }
                    if (isEn("SOTD"))
                    {
                        var hasIE = Items.HasItem(3031);
                        var coeff = hasIE ? 2.5 : 2.0;
                        if ((Player.GetAutoAttackDamage(target)*coeff*3 >= target.Health))
                        {
                            useItem(3131);
                            Orbwalker.ForceTarget(target);
                        }
                    }
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    if (isEn("QM")) {CastQ();}
                    if (isEn("WM") &&
                        (ObjectManager.Player.Buffs.FirstOrDefault(buff => buff.Name == "dravenfurybuff" || buff.Name == "DravenFury") == null))
                    {
                        var WManaMix = Menu.Item("WManaM").GetValue<Slider>().Value;
                        if (getManaPer() >= WManaMix) {W.Cast();}
                    }
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    if (isEn("QLH")) {CastQ();}
                    if (isEn("WLH") && (ObjectManager.Player.Buffs.FirstOrDefault(buff => buff.Name == "dravenfurybuff" || buff.Name = "DravenFury") == null)) {W.Cast();}
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    if (isEn("WLC") && (ObjectManager.Player.Buffs.FirstOrDefault(buff => buff.Name == "dravenfurybuff" || buff.Name == "DravenFury") == null)) {W.Cast();}
                    if (isEn("QLC")) {CastQ();}
                    break;
                default:
                    return;
            }
        }

        private static bool PlayerInTurretRange()
        {
            foreach (var val in towerPos)
            {
                if (val.Health == 0)
                {
                    towerPos.Remove(val);
                }
            }
            foreach (var val in towerPos)
            {
                if (Player.Distance(val) < 975f)
                {
                    return true;
                }
            }
        }

        private static bool RetInTurretRange(Vector3 retPosition)
        {
            foreach (var val in towerPos)
            {
                if (val.Health == 0)
                {
                    towerPos.Remove(val);
                }
            }
            foreach (var val in towerPos)
            {
                if (Vector3.Distance(retPosition, val.Position) < 975f)
                {
                    return true;
                }
            }
            return false;
        }

        private static void compileTowerArray()
        {
            foreach (var tower in ObjectManager.Get<Obj_AI_Turret>().Where(tower => tower.IsEnemy))
            {
                towerPos.Add(tower);
            }
        }

        private static bool IsZoneSafe(Vector3 v, float dist)
        {
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy))
            {
                if (Vector3.Distance(enemy.Position, v) < dist && !enemy.IsDead && enemy != null)
                {
                    return false;
                }
            }
        }

        private static Obj_AI_Hero ClosestHero(float range)
        {
            Obj_AI_Hero clhero = null;
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
            {
                if (!hero.IsDead && hero.IsVisible && Player.Distance(hero) < Player.Distance(clhero))
                {
                    clhero = hero;
                }
            }
            return clhero;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            var safeZone = Menu.Item("SafeZone").GetValue<Slider>().Value;
            var target = TargetSelector.GetTarget(550f, TargetSelector.DamageType.Physical);
            var ETarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            var RTarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);

            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
            {
                if (isEn("EKs"))
                {
                    var ePred = E.GetPrediction(hero);
                    if (E.GetHealthPrediction(hero) <= 0)
                    {
                        E.Cast(hero);
                        break;
                    }
                }
                if (isEn("QKs"))
                {
                    if (Q.GetDamage(hero) + Player.GetAutoAttackDamage(hero) >= hero.Health)
                    {
                        if (GetQNumber() < 1) {Q.Cast();}
                        Orbwalker.SetAttack(true);
                        Orbwalker.ForceTarget(hero);
                        break;
                    }
                }
                if (isEn("RKs"))
                {
                    var RPred = R.GetPrediction(hero);
                    if (R.GetHealthPrediction(hero) <= 0 && Player.Distance(hero) <= 2000f)
                    {
                        R.Cast(hero);
                        break;
                    }
                }
            }
            if (Menu.Item("QRefresh").GetValue<KeyBind>().Active)
            {
                reticleList.Clear();
            }
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    if (isEn("EC")) {CastE(ETarget);}
                    if (isEn("RC")) {CastR(RTarget);}
                    if (isEn("ACC")) {OrbwalkToReticle(safeZone, 100);}
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    if (isEn("EM")) {CastE(ETarget);}
                    if (isEn("RM")) {CastR(RTarget);}
                    if (isEn("ACM")) {OrbwalkToReticle(safeZone, 100);}
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    if (isEn("ACLH")) {OrbwalkToReticle(safeZone, 100);}
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    if (isEn("ACLC")) {OrbwalkToReticle(safeZone, 100);}
                    break;
                default:
                    break;
            }
        }

        private static void useItem(int id, Obj_AI_Hero target = null)
        {
            if (Items.HasItem(id) && Items.CanUseItem(id))
            {
                Items.UseItem(id, target);
            }
        }

        private static void OnCreateObject(GameObject sender, EventArgs args)
        {
            if (!sender.Name.Contains("Q_reticle_self")) { return; }
            reticleList.Add(new Reticle(sender, Game.Time, sender.Position, Game.Time + 1.20, sender.NetworkId));
        }

        private static void OnDeleteObject(GameObject sender, EventArgs args)
        {
            if (!sender.Name.Contains("Q_reticle_self")) { return; }
            foreach (Reticle ret in reticleList)
            {
                if (Player.ServerPosition.Distance(ret.getPosition()) <= 100 && ret.getNetworkId() == sender.NetworkId)
                {
                    isCatching = false;
                }
                if (ret.getNetworkId() == sender.NetworkId) {reticleList.Remove(ret);}
            }
        }

        private static void OnGapcloser(ActiveGapcloser gapcloser)
        {
            if (!isEn("EGapCloser")) {return;}
            if (gapcloser.End.Distance(Player.ServerPosition) <= 50f)
            {
                var EPred = E.GetPrediction(gapcloser.Sender);
                if (EPred.Hitchance >= HitChance.Medium)
                {
                    E.Cast(EPred.CastPosition);
                }
            }
        }

        private static void OnInterruptCreate(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!isEn("EInterrupt")) {return;}
            var EPred = E.GetPrediction(unit);
            if (EPred.Hitchance >= HitChance.Medium)
            {
                E.Cast(EPred.CastPosition);
            }
        }

        private static bool IsInStandRange()
        {
            return (Vector3.Distance(Game.CursorPos, Player.Position) < 220);
        }

        private static void OrbwalkToReticle(int SafeZone, int RetSafeZone)
        {
            bool toggle = isEn("UseAARet");
            var target = ClosestHero(900f);
            Reticle ClosestRet = null;
            var QRadius = Menu.Item("QRadius").GetValue<Slider>().Value;
            foreach (Reticle r in reticleList)
            {
                if (!r.getObj().IsValid) { reticleList.Remove(r); }
            }
            if (reticleList.Count > 0)
            {
                float closestDist = float.MaxValue;
                foreach (Reticle r in reticleList)
                {
                    if (r.getPosition().Distance(Game.CursorPos) <= QRadius &&
                        Player.Distance(r.getPosition()) < closestDist)
                    {
                        if (IsZoneSafe(r.getPosition(), RetSafeZone) && IsZoneSafe(Player.Position, SafeZone))
                        {
                            ClosestRet = r;
                            closestDist = Player.Distance(r.getPosition());
                        }
                    }
                }
            }
            if (ClosestRet != null && !RetInTurretRange(ClosestRet.getPosition()))
            {
                float myHitbox = 65;
                float QDist = Vector2.Distance(ClosestRet.getPosition().To2D(), Player.ServerPosition.To2D()) - myHitbox;
                float QDist1 = Player.GetPath(ClosestRet.getPosition()).ToList().To2D().PathLength();
                bool CanReachRet = (QDist1 / Player.MoveSpeed+Game.Time)<(ClosestRet.getEndTime());
                bool CanReachRetWBonus = (QDist1 / (Player.MoveSpeed + (Player.MoveSpeed * (getMoveSpeedBonusW() / 100))) + Game.Time) < (ClosestRet.getEndTime());
                bool WNeeded = false;
                if (CanReachRetWBonus && !CanReachRet)
                {
                    W.Cast();
                    WNeeded = true;
                }
                if ((CanReachRet || WNeeded))
                {
                    WNeeded = false;
                    if (Player.Distance(ClosestRet.getPosition()) >= 100)
                    {
                        if (ClosestRet.getPosition() != Game.CursorPos)
                        {
                            Orbwalker.SetOrbwalkingPoint(ClosestRet.getPosition());
                        }
                        else
                        {
                            Orbwalker.SetOrbwalkingPoint(Game.CursorPos);
                        }
                    }
                    Console.WriteLine("Orbwalking to " + ClosestRet.getPosition().ToString());
                }
            }
        }

        private static int getMoveSpeedBonusW()
        {
            switch (W.Level)
            {
                case 1:
                    return 40;
                case 2:
                    return 45;
                case 3:
                    return 50;
                case 4:
                    return 55;
                case 5:
                    return 60;
                default:
                    return 0;
            }
        }
        private static void CastQ()
        {
            var qNumberOnPlayer = GetQNumber();
            if (reticleList.Count + 1 > Menu.Item("MaxQNum").GetValue<Slider>().Value) { return; }
            if (qNumberOnPlayer > 2) {return;}
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    var QManaCombo = Menu.Item("QManaC").GetValue<Slider>().Value;
                    if (getManaPer() >= QManaCombo)
                    {
                        Q.Cast();
                    }
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    var QManaMix = Menu.Item("QManaM").GetValue<Slider>().Value;
                    if (getManaPer() >= QManaMix)
                    {
                        Q.Cast();
                    }
                    break;
                default:
                    Q.Cast();
                    break;
            }
        }

        private static void CastE(Obj_AI_Base unit)
        {
            var EPrediction = E.GetPrediction(unit);
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    var EManaCombo = Menu.Item("EManaC").GetValue<Slider>().Value;
                    if ((getManaPer() >= EManaCombo)) {E.Cast(unit);}
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    var EManaMix = Menu.Item("EManaM").GetValue<Slider>().Value;
                    if ((getManaPer() >= EManaMix)) {E.Cast(unit);}
                    break;
                default:
                    break;
            }
        }

        private static void CastR(Obj_AI_Base unit)
        {
            var RPrediction = R.GetPrediction(unit);
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    var RManaCombo = Menu.Item("RManaC").GetValue<Slider>().Value;
                    if ((getManaPer() >= RManaCombo) && Player.Distance(unit) < 2000f) { R.Cast(unit); }
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    var RManaMix = Menu.Item("RManaM").GetValue<Slider>().Value;
                    if ((getManaPer() >= RManaMix) && Player.Distance(unit) < 2000f) { R.Cast(unit); }
                    break;
                default:
                    break;
            }
        }
        private static int GetQNumber()
        {
            var buff = ObjectManager.Player.Buffs.FirstOrDefault(buff1 => buff1.Name.Equals("dravenspinningattack"));
            return buff != null ? buff.Count : 0;
        }

        static bool isEn(String opt)
        {
            return Menu.Item(opt).GetValue<bool>();
        }

        private static float getManaPer()
        {
            float mana = (Player.Mana/Player.MaxMana)*100;
            return mana;
        }
    }
}
