using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace JLQ_MBE_BattleSimulation
{
    class Reimu:CharacterMovingIgnoreEnemy
    {
        static AutoResetEvent are = new AutoResetEvent(false);

        public Reimu(int id, Point position, Group group, Random random, Game game)
            : base(id, position, group, random, game)
        {
            
        }

        /// <summary>灵力回收增加20%</summary>
        public new void mpGain(int mp)
        {
            ((Character)this).mpGain((int)Math.Floor(1.2f * mp));
        }

        /// <summary>符卡01：梦想封印，对所有4格内的敌人造成1.0倍率的弹幕攻击</summary>
        public override void SC01()
        {
            //向game类传递参数，如何选择目标
            game.HowToChoose = ((SCer, SCee) => Calculate.Distance(SCer.Position, SCee.Position) <= 4);
            //向game类传递参数，如何处理目标
            //game.HowToHandle=((SCer,SCee)=>)
        }


    }
}
