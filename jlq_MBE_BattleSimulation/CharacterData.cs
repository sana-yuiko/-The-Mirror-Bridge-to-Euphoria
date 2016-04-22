using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JLQ_MBE_BattleSimulation
{
    class CharacterData
    {
        public string name;//角色是谁
        public string display = "";//屏幕上显示什么文字
        public int maxHp = 120;//最大血量
        public int attack = 100;//攻击   
        public int defence = 80;//防御
        public int hitRate = 80;//命中
        public int dodgeRate = 60;//闪避
        public float closeAmendment = 1.0f;//近战补正
        public int interval = 30;//行动间隔
        public int moveAbility = 3;//机动
        public int attackRange = 3;//普攻范围
        public string[] scName = { "", "", "", "" };//符卡名
        public string[] scDisc = { "", "", "", "" };//符卡描述
    }
}
