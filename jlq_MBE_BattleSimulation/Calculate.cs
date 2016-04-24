using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;

namespace JLQ_MBE_BattleSimulation
{
    static class Calculate
    {
        /// <summary>计算命中率</summary>
        /// <param name="relativeHitRate">攻击者对防御者的相对命中率</param>
        /// <param name="distance">攻击者对防御者的相对距离</param>
        /// <returns>命中率</returns>
        private static double HitRate(int relativeHitRate, int distance)
        {
            double p = 1.0 / (1 + Math.Pow(0.93, relativeHitRate));
            return (p > 0.95 ? 0.95 : (p < 0.05 ? 0.05 : p)) * (1.0f - 0.05f * distance);
        }

        /// <summary>计算命中率</summary>
        /// <param name="attacker">攻击者</param>
        /// <param name="target">攻击目标</param>
        /// <returns>命中率</returns>
        public static double HitRate(Character attacker, Character target)
        {
            return HitRate(attacker.HitRate - target.DodgeRate, Distance(attacker.Posotion, target.Posotion));
        }

        //伤害公式
        /// <summary>
        /// 计算伤害值
        /// </summary>
        /// <param name="attack">攻击者的攻击值</param>
        /// <param name="defence">防御者的防御值</param>
        /// <returns>伤害值</returns>
        public static int Damage(int attack, int defence)
        {
            return attack * attack / (attack + defence);
        }

        /// <summary>储存角色列表中所有角色的原始数据</summary>
        public static List<CharacterData> characterDataList = new List<CharacterData>();

        /// <summary>求两点距离，参数可交换</summary>
        /// <param name="point1">点1</param>
        /// <param name="point2">点2</param>
        /// <returns>距离值</returns>
        public static int Distance(Point point1, Point point2)
        {
            return (int) (Math.Abs(point1.X - point2.X) + Math.Abs(point1.Y - point2.Y));
        }
    }
}
