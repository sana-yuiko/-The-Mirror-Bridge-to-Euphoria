using System;
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
		    enter02 = (s, ev) =>
		    {
		        this.game.DefaultButtonAndLabels();
		        foreach (
		            var l in
		                game.EnemyAsCurrent.Where(c => Calculate.Distance(c.Position, this.Position) <= SC02Range)
		                    .Select(c => c.LabelDisplay))
		        {
		            l.Background = Brushes.LightBlue;
		        }
		    };
            leave02 = (s, ev) =>
            {
                this.game.ResetPadButtons();
                game.PaintButton();
                game.UpdateLabelBackground();
            };
		    enter03 = (s, ev) =>
		    {
		        this.game.DefaultButtonAndLabels();
		        foreach (
		            var l in
		                game.EnemyAsCurrent.Where(c => Calculate.Distance(c.Position, game.MousePoint) <= SC03Range)
		                    .Select(c => c.LabelDisplay))
		        {
		            l.Background = Brushes.LightBlue;
		        }
		    };
		    leave03 = (s, ev) =>
		    {
		        this.game.ResetPadButtons();
		        game.PaintButton();
		        game.UpdateLabelBackground();
		    };
		}

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

        private MouseEventHandler enter02;
        private MouseEventHandler leave02;
        private MouseEventHandler enter03;
        private MouseEventHandler leave03;

        //TODO 天赋

        //符卡
        /// <summary>符卡01：乘着风，瞬移到3格内一名敌方角色面前，并释放旋风对自身2格内所有敌方单位造成0.5倍率的伤害</summary>
        public override void SC01()
        {
            //TODO 面前的定义？
            game.IsLegalClick = point =>
            {
                var c = game[point];
                return c != null && Calculate.Distance(point, this.Position) <= SC01Range &&
                       game.EnemyAsCurrent.Contains(c);
            };
            game.IsTargetLegal = (SCee, point) =>
            {
                var c = game[point];
                if (pointTemp01 == new Point(-1, -1))
                {
                    pointTemp01 = c.Position.Y == this.Position.Y
                        ? new Point(c.Position.X + (c.Position.X > this.Position.X ? -1 : 1), this.Position.Y)
                        : new Point(c.Position.X, c.Position.Y + (c.Position.Y > this.Position.Y ? -1 : 1));
                }
                return Calculate.Distance(SCee.Position, pointTemp01) <= SC01Range2 && game.EnemyAsCurrent.Contains(SCee);
            };
            game.HandleTarget = SCee =>
            {
                if (this.Position != pointTemp01) this.Move(pointTemp01);
                DoAttack(SCee, SC01DamageGain);
            };
        }

        /// <summary>结束符卡01</summary>
        public override void EndSC01()
        {
            base.EndSC01();
        }

        /// <summary>符卡02：孤独绽放的，对4格内一名敌方单位造成2.0倍率的伤害</summary>
        public override void SC02()
        {
            game.IsLegalClick = point =>
            {
                var c = game[point];
                return c != null && Calculate.Distance(point, this.Position) <= SC02Range &&
                       game.EnemyAsCurrent.Contains(c);
            };
            game.IsTargetLegal = (SCee, point) => SCee.Position == point;
            game.HandleTarget = SCee => DoAttack(SCee, SC02DamageGain);


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
                Calculate.Distance(SCee.Position, point) <= SC03Range && game.EnemyAsCurrent.Contains(SCee);
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
