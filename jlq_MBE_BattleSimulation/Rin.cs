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
		    enter01 = (s, ev) =>
		    {
		        if (this.game.IsLegalClick == null || (!this.game.IsLegalClick(this.game.MousePoint))) return;
		        this.game.DefaultButtonAndLabels();
		        game.Buttons[(int) pointTemp01.X, (int) pointTemp01.Y].Opacity = 1;
		        foreach (
		            var l in
		                Enemy.Where(
		                    SCee => Calculate.Distance(SCee.Position, pointTemp01) <= SC01Range2 && Enemy.Contains(SCee))
		                    .Select(c => c.LabelDisplay))
		        {
		            l.Background = Brushes.LightBlue;
		        }
		    };
		    leave01 = (s, ev) =>
		    {
                pointTemp01 = new Point(-1, -1);
		        this.game.ResetPadButtons();
		        this.game.PaintButton();
		        this.game.UpdateLabelBackground();
		    };
            enter02 = (s, ev) =>
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
            leave02 = (s, ev) =>
            {
                this.game.ResetPadButtons();
                this.game.PaintButton();
                this.game.UpdateLabelBackground();
            };
		    enter03 = (s, ev) =>
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
		    leave03 = (s, ev) =>
		    {
		        this.game.ResetPadButtons();
		        this.game.PaintButton();
		        this.game.UpdateLabelBackground();
		    };
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

	    private Point pointTemp01 = new Point(-1, -1);

        private MouseEventHandler enter01;
        private MouseEventHandler leave01;
        private MouseEventHandler enter02;
        private MouseEventHandler leave02;
        private MouseEventHandler enter03;
        private MouseEventHandler leave03;

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
            game.IsLegalClick = point =>
            {
                var c = game[point];
                if (c == null || Calculate.Distance(point, this.Position) > SC01Range ||
                    (!Enemy.Contains(c)))
                {
                    return false;
                }
                pointTemp01 = c.Position.Y == this.Position.Y
                    ? new Point(c.Position.X + (c.Position.X > this.Position.X ? -1 : 1), this.Position.Y)
                    : new Point(c.Position.X, c.Position.Y + (c.Position.Y > this.Position.Y ? -1 : 1));
                if (game[pointTemp01] == null) return true;
                pointTemp01 = new Point(-1, -1);
                return false;
            };
            game.IsTargetLegal = (SCee, point) =>
            {
                if (this.Position != pointTemp01) Move(pointTemp01);
                return Calculate.Distance(SCee.Position, pointTemp01) <= SC01Range2 &&
                       Enemy.Contains(SCee);
            };
            game.HandleTarget = SCee => DoAttack(SCee, SC01DamageGain);
            foreach (var b in game.Buttons)
            {
                b.MouseEnter += enter01;
                b.MouseLeave += leave01;
            }
        }

        /// <summary>结束符卡01</summary>
        public override void EndSC01()
        {
            base.EndSC01();
            foreach (var b in game.Buttons)
            {
                b.MouseEnter -= enter01;
                b.MouseLeave -= leave01;
            }
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
            enter02(null, null);

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
            foreach (var b in game.Buttons)
            {
                b.MouseEnter += enter03;
                b.MouseLeave += leave03;
            }
        }
        /// <summary>结束符卡03</summary>
        public override void EndSC03()
        {
            base.EndSC03();
            foreach (var b in game.Buttons)
            {
                b.MouseEnter -= enter03;
                b.MouseLeave -= leave03;
            }
        }

        public override void SCShow()
        {
            game.ButtonSC[1].MouseEnter += enter02;
            game.ButtonSC[1].MouseLeave += leave02;
        }

        public override void ResetSCShow()
        {
            game.ButtonSC[1].MouseEnter -= enter02;
            game.ButtonSC[1].MouseLeave -= leave02;
        }
    }
}
