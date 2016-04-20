using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JLQ_MBE_BattleSimulation
{
    class CharacterData
    {
        public string name;       //角色是谁
        public string display;  //屏幕上显示什么文字
        public int maxHp;       //最大血量
        public int attack;         //攻击   
        public int defence;         //防御
        public int hitRate;         //命中
        public int dodgeRate;         //闪避
        public float closeAmendment;       //近战补正
        public int interval;         //行动间隔
        public int moveAbility;         //机动
        public int attackRange;        //普攻范围
        public string[] scName = { "", "", "", "" };    //符卡名
        public string[] scDisc = { "", "", "", "" };    //符卡描述
    }
}
