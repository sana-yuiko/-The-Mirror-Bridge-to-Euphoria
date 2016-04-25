using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JLQ_MBE_BattleSimulation
{
    /// <summary>阵营</summary>
    enum Group { Middle, Friend, Enemy = -1 }
    /// <summary>游戏阶段</summary>
    enum Section { Preparing, Round, End }
}
