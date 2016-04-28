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

        public override void MpGain(int mp)
        {
            base.MpGain((int)Math.Floor(1.2 * mp));
        }

        /// <summary>符卡01：梦想封印，对所有4格内的敌人造成1.0倍率的弹幕攻击</summary>
        public override void SC01()
        {
            game.SCAttack((SCer, SCee) => Calculate.Distance(SCer.Position, SCee.Position) <= 4,
                (SCer, SCee) => SCer.DoAttack(SCee, 1.0f, true, true));
        }

        /// <summary>符卡02：封魔阵，</summary>
        public override void SC02()
        {
            //测试，对4格内一点周围1格的敌人造成1.0倍率的弹幕攻击
            /*game.SCAttack(new DelegateEvent.StructChoosePoint(
                (SCer, p) => Calculate.Distance(p, SCer.Position) <= 4,
                (SCer, SCee, p) => Calculate.Distance(p, SCee.Position) <= 1 && SCee.Group != SCer.Group,
                (SCer, SCee, p) => SCer.DoAttack(SCee, 1.0f, true, true)));*/
            //测试，对4格内一点1个自己人施加buff，buff效果：反伤100%真实伤害，时间：1回合
            DelegateEvent.BeAttack b = (attacker, damage) => attacker.beAttack(buffee, damage);
            game.SCAttack(new DelegateEvent.StructChoosePoint(
                (SCer, p) => Calculate.Distance(p, SCer.Position) <= 4,
                (SCer, SCee, p) => SCee.Position == p && SCee.Group == SCer.Group,
                (SCer, SCee, p) => SCee.BuffList.Add(
                    new Buff(SCer, SCee, this.Interval,
                    (buffer, buffee) => buffee.beAttack +=
                    (attacker, damage) => attacker.beAttack(buffee, damage), 
                    (buffer, buffee) => buffee.beAttack -=
                    (attacker, damage) => attacker.beAttack(buffee, damage)))));
        }

        public override void SC03()
        {
            
        }
    }
}
