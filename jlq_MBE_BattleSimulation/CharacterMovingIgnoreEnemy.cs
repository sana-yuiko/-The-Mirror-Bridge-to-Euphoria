using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace JLQ_MBE_BattleSimulation
{
    abstract class CharacterMovingIgnoreEnemy:Character
    {
        protected CharacterMovingIgnoreEnemy(int id, Point position, Group group, Random random, Game game)
            : base(id, position, group, random, game)
        {
            
        }
    }
}
