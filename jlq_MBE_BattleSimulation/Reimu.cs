using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace JLQ_MBE_BattleSimulation
{
    class Reimu:CharacterMovingIgnoreEnemy
    {
        public Reimu(int id, Point position, Group group, Random random, Game game)
            : base(id, position, group, random, game)
        {
            
        }

        /// <summary>1.2倍灵力获取</summary>
        /// <param name="mp">获得的灵力量</param>
        public override void MpGain(int mp)
        {
            base.MpGain((int) Math.Floor(1.2*mp));
        }

        /// <summary>符卡01：梦想封印，对所有4格内的敌人造成1.0倍率的弹幕攻击</summary>
        public override void SC01()
        {
            game.GetTarget += (SCer, SCee) => Calculate.Distance(SCer.Position, SCee.Position) <= 4;
            //TODO handle target
        }

        public override void SC02()
        {
            
        }

        public override void SC03()
        {
            
        }
    }
}
