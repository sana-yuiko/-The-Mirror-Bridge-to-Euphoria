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
        //命中率公式
        public static double HitRate(int relativeHitRate, int distance)
        {
            double p = 1.0 / (1 + Math.Pow(0.93, relativeHitRate));
            return (p > 0.95 ? 0.95 : (p < 0.05 ? 0.05 : p)) * (1.0f - 0.05f * distance);
        }

        //伤害公式
        public static int Damage(int attack, int defence)
        {
            return attack * attack / (attack + defence);
        }

        public static List<CharacterData> characterDataList = new List<CharacterData>();//储存所有角色的原始数据

        //求两点距离
        public static int Distance(Point point1, Point point2)
        {
            return (int) (Math.Abs(point1.X - point2.X) + Math.Abs(point1.Y - point2.Y));
        }
    }
}
