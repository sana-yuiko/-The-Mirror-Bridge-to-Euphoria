using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace JLQ_MBE_BattleSimulation
{
    /// <summary>冴月麟</summary>
	class Rin : Character
	{
		public Rin(int id, Point position, Group group, Random random, Game game)
			: base(id, position, group, random, game)
		{
            //符卡01
            //显示有效单击点
		    enterButton[0] = (s, ev) =>
		    {
		        this.game.DefaultButtonAndLabels();
		        foreach (var point in Game.PadPoints.Where(SC01IsLegalClick))
		        {
		            game[point].LabelDisplay.Background = Brushes.LightBlue;
		        }
		        pointTemp1 = Game.DefaultPoint;
		    };
            SetDefaultLeaveSCButtonDelegate(0);
            //显示将瞬移到的点和将被攻击的目标
		    enterPad[0] = (s, ev) =>
		    {
		        if (!this.game.IsLegalClick(this.game.MousePoint)) return;
		        this.game.DefaultButtonAndLabels();
		        if (this.Position != pointTemp1)
		        {
		            game.Buttons[(int) pointTemp1.X, (int) pointTemp1.Y].Opacity = 1;
		        }
		        foreach (
		            var l in
		                Enemy.Where(
		                    SCee => Calculate.Distance(SCee.Position, pointTemp1) <= SC01Range2 && Enemy.Contains(SCee))
		                    .Select(c => c.LabelDisplay))
		        {
		            l.Background = Brushes.LightBlue;
		        }
                pointTemp1 = Game.DefaultPoint;
            };
            SetDefaultLeavePadButtonDelegate(0);
            //符卡02
            //显示有效单击点
            enterButton[1] = (s, ev) =>
		    {
		        this.game.DefaultButtonAndLabels();
		        foreach (
		            var l in
		                this.Enemy.Where(c => Calculate.Distance(c.Position, this.Position) <= SC02Range)
		                    .Select(c => c.LabelDisplay))
		        {
		            l.Background = Brushes.LightBlue;
		        }
		    };
            SetDefaultLeaveSCButtonDelegate(1);
            //显示将被攻击的目标
		    enterPad[1] = (s, ev) =>
		    {
		        if (!this.game.IsLegalClick(game.MousePoint)) return;
		        this.game.DefaultButtonAndLabels();
		        game[game.MousePoint].LabelDisplay.Background = Brushes.LightBlue;
		    };
            SetDefaultLeavePadButtonDelegate(1);
            //符卡03
            //显示将被攻击的目标
            enterPad[2] = (s, ev) =>
		    {
		        this.game.DefaultButtonAndLabels();
		        foreach (
		            var l in
		                Enemy.Where(c => Calculate.Distance(c.Position, game.MousePoint) <= SC03Range)
		                    .Select(c => c.LabelDisplay))
		        {
		            l.Background = Brushes.LightBlue;
		        }
		    };
            SetDefaultLeavePadButtonDelegate(2);
		}

        /// <summary>天赋范围</summary>
        private const int skillRange = 2;
        /// <summary>符卡01的参数</summary>
	    private const int SC01Range = 3;
	    private const int SC01Range2 = 2;
        private const float SC01DamageGain = 0.5f;
        /// <summary>符卡02的参数</summary>
        private const int SC02Range = 4;
        private const float SC02DamageGain = 2.0f;
        /// <summary>符卡03的参数</summary>
        private const int SC03Range = 1;
        private const float SC03DamageGain = 0.7f;

	    private Point pointTemp1 = Game.DefaultPoint;

        //TODO 天赋
        /// <summary>天赋：当你受到攻击时，对2格内随机一名敌方单位造成所受伤害30%的真实伤害</summary>
        /// <param name="damage">伤害值</param>
        /// <param name="attacker">攻击者</param>
        public override void BeAttacked(int damage, Character attacker)
        {
            base.BeAttacked(damage, attacker);
            var legalTarget =
                this.Enemy.Where(c => Calculate.Distance(c.Position, this.Position) <= skillRange).ToArray();
            if (legalTarget.Length == 0) return;
            var index = random.Next(legalTarget.Length);
            var target = legalTarget[index];
            //判断是否命中
            if (HandleIsHit(target)) return;
            //造成无来源伤害
            var damageNew = (int) (damage*0.3*FloatDamage);
            target.BeAttacked(damageNew, null);
        }

        //符卡
        /// <summary>符卡01：乘着风，瞬移到3格内一名敌方角色面前，并释放旋风对自身2格内所有敌方单位造成0.5倍率的伤害</summary>
        public override void SC01()
        {
            game.IsLegalClick = SC01IsLegalClick;
            game.IsTargetLegal = (SCee, point) =>
            {
                if (this.Position != pointTemp1) Move(pointTemp1);
                return Calculate.Distance(SCee.Position, pointTemp1) <= SC01Range2 &&
                       Enemy.Contains(SCee);
            };
            game.HandleTarget = SCee => DoAttack(SCee, SC01DamageGain);
            AddPadButtonEvent(0);
        }

        /// <summary>结束符卡01</summary>
        public override void EndSC01()
        {
            base.EndSC01();
            RemovePadButtonEvent(0);
        }

        /// <summary>符卡02：孤独绽放的，对4格内一名敌方单位造成2.0倍率的伤害</summary>
        public override void SC02()
        {
            game.IsLegalClick = point =>
            {
                var c = game[point];
                return c != null && Calculate.Distance(point, this.Position) <= SC02Range &&
                       Enemy.Contains(c);
            };
            game.IsTargetLegal = (SCee, point) => SCee.Position == point;
            game.HandleTarget = SCee => DoAttack(SCee, SC02DamageGain);
            enterButton[1](null, null);

        }

        /// <summary>结束符卡02</summary>
        public override void EndSC02()
        {
            base.EndSC02();
        }
        /// <summary>符卡03：对一点周围1格内所有敌方角色造成0.7倍率的伤害</summary>
        public override void SC03()
        {
            game.IsLegalClick = point => true;
            game.IsTargetLegal = (SCee, point) =>
                Calculate.Distance(SCee.Position, point) <= SC03Range && Enemy.Contains(SCee);
            game.HandleTarget = SCee => DoAttack(SCee, SC03DamageGain);
            AddPadButtonEvent(2);
        }
        /// <summary>结束符卡03</summary>
        public override void EndSC03()
        {
            base.EndSC03();
            RemovePadButtonEvent(2);
        }

        public override void SCShow()
        {
            for (var i = 0; i < 2; i++)
            {
                AddSCButtonEvent(i);
            }
        }

        public override void ResetSCShow()
        {
            for (var i = 0; i < 2; i++)
            {
                RemoveSCButtonEvent(i);
            }
        }

        private bool SC01IsLegalClick(Point point)
        {
            var c = game[point];
            if (c == null || Calculate.Distance(point, this.Position) > SC01Range ||
                (!Enemy.Contains(c)))
            {
                return false;
            }
            pointTemp1 = c.Position.Y == this.Position.Y
                ? new Point(c.Position.X + (c.Position.X > this.Position.X ? -1 : 1), this.Position.Y)
                : new Point(c.Position.X, c.Position.Y + (c.Position.Y > this.Position.Y ? -1 : 1));
            if (this.Position == pointTemp1 || game[pointTemp1] == null) return true;
            pointTemp1 = Game.DefaultPoint;
            return false;
        }
    }
}
