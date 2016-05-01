using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace JLQ_MBE_BattleSimulation
{
    /// <summary>大妖精</summary>
	class Daiyousei : Character
	{
		public Daiyousei(int id, Point position, Group group, Random random, Game game)
			: base(id, position, group, random, game)
		{

		}

        private const int skillRange = 2;
        private const float skillGain = 0.05f;
        private const int SC01Range = 4;
        private const int SC02Range = 2;
        private const float SC02Gain = 1.5f;

        /// <summary>天赋：雾之湖的恩惠</summary>
        public override void PreparingSection()
        {
            foreach (
                var c in
                    game.Characters.Where(
                        c =>
                            c.Group == this.Group && c != this &&
                            Calculate.Distance(c.Position, this.Position) <= skillRange))
            {
                c.Hp = Math.Min(c.Data.MaxHp, c.Hp + (int) (skillGain*c.Data.MaxHp));
            }
        }

        //符卡
        /// <summary>符卡01</summary>
        public override void SC01()
        {
            //TODO SC01
        }

        /// <summary>结束符卡01</summary>
        public override void EndSC01()
        {
            
        }

        /// <summary>符卡02</summary>
        public override void SC02()
        {
            game.IsLegalClick = point =>
            {
                if (Calculate.Distance(point, this.Position) > SC01Range) return false;
                var c = game[point];
                return c != null && Enemy.Contains(c);
            };
            game.IsTargetLegal = (SCee, point) => SCee.Position == point;
            game.HandleTarget = SCee => DoAttack(SCee, 1.5f);
        }

        /// <summary>结束符卡02</summary>
        public override void EndSC02()
        {
            base.EndSC02();
        }
        /// <summary>符卡03</summary>
        public override void SC03()
        {
            game.IsTargetLegal =
                (SCee, point) =>
                    Calculate.Distance(SCee.Position, this.Position) <= SC02Range && SCee.Group == this.Group;
            game.HandleTarget = SCee => SCee.Hp = Math.Min(SCee.Data.MaxHp, SCee.Hp + (int) (SCee.Attack*SC02Gain));
        }
        /// <summary>结束符卡03</summary>
        public override void EndSC03()
        {
            base.EndSC03();
        }

    }
}
