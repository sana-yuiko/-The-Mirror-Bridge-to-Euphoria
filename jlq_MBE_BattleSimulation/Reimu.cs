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
    /// <summary>灵梦</summary>
    class Reimu : CharacterMovingIgnoreEnemy
    {
        public Reimu(int id, Point position, Group group, Random random, Game game)
            : base(id, position, group, random, game)
        {
            enter01 = (s, ev) =>
            {
                game.DefaultButtonAndLabels();
                game.SetButtonBackground(this.Position, SC01Range);
            };
            leave01 = (s, ev) =>
            {
                this.game.ResetPadButtons();
                game.PaintButton();
                game.UpdateLabelBackground();
            };
        }

        /// <summary>符卡01的范围</summary>
        private const int SC01Range = 4;

        private MouseEventHandler enter01;
        private MouseEventHandler leave01;


        /// <summary>天赋：1.2倍灵力获取</summary>
        /// <param name="mp">获得的灵力量</param>
        public override void MpGain(int mp)
        {
            base.MpGain(Calculate.Floor(1.2*mp));
        }

        /// <summary>符卡01：梦想封印，对所有4格内的敌人造成1.0倍率的弹幕攻击</summary>
        public override void SC01()
        {
            game.IsTargetLegal =
                (SCee, point) =>
                    Calculate.Distance(this.Position, SCee.Position) <= SC01Range && Enemy.Contains(SCee);
            game.HandleTarget = t => DoAttack(t);
        }

        /// <summary>结束符卡01</summary>
        public override void EndSC01()
        {
            base.EndSC01();
        }

        /// <summary>符卡02</summary>
        public override void SC02()
        {
            //TODO SC02
        }

        /// <summary>结束符卡02</summary>
        public override void EndSC02()
        {

        }

        /// <summary>符卡03</summary>
        public override void SC03()
        {
            //TODO SC03
        }

        /// <summary>结束符卡03</summary>
        public override void EndSC03()
        {

        }

        public override void SCShow()
        {
            game.ButtonSC[0].MouseEnter += enter01;
            game.ButtonSC[0].MouseLeave += leave01;
        }

        public override void ResetSCShow()
        {
            game.ButtonSC[0].MouseEnter -= enter01;
            game.ButtonSC[0].MouseLeave -= leave01;

        }
    }
}
