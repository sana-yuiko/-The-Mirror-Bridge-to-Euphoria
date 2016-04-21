using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JLQ_MBE_BattleSimulation
{
    class Game
    {
        private Random random;
        public List<Character> Characters { get; private set; }//所有角色 
        
        //构造函数
        public Game(Random random)
        {
            this.random = random;
            Characters = new List<Character>();
        }  
    }
}
