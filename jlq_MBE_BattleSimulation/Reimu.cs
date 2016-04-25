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

        //TODO 灵力回收增加20%

        public override void SC01()
        {
            
        }
    }
}
