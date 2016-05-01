using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JLQ_MBE_BattleSimulation
{
    /// <summary>
    /// 没有参数的buff；
    /// 默认的buff类可能会利用buff者自身与施加buff者的相关数据，可以对施加buff者造成影响；
    /// 更复杂的buff需要自建子类；
    /// 每个角色的buff效果自己编写，会有一些静态的buff供调用；
    /// 开发者备注：静态的buff需要有一系列参数，每人使用lambda表达式代入参数的具体值；
    /// </summary>
    public class Buff
    {
        /// <summary>
        /// 将Interval设为此值，则buff无限剩余时间
        /// </summary>
        public const int Infinite = Int32.MaxValue;
        /// <summary>buff剩余时间</summary>
        public int Interval { get; protected set; }
        /// <summary>buff执行的阶段</summary>
        public readonly Section ExecuteSection;
        /// <summary>buff名称</summary>
        public readonly string Name;

        /// <summary>buff效果的委托对象</summary>
        public DBuffAffect BuffAffect;
        /// <summary>取消buff的委托对象</summary>
        public DBuffCancel BuffCancels;

        /// <summary>buff发出者</summary>
        public Character Buffer;
        /// <summary>buff承受者</summary>
        public Character Buffee;

        /// <summary>Buff类的构造函数</summary>
        /// <param name="buffee">buff承受者</param>
        /// <param name="buffer">buff发出者</param>
        /// <param name="roundNum">buff持续回合数</param>
        /// <param name="executeSection">buff执行的阶段</param>
        /// <param name="name">buff名称</param>
        /// <param name="affect">buff效果委托</param>
        /// <param name="cancel">buff取消委托</param>
        public Buff(Character buffee, Character buffer, int roundNum, Section executeSection, string name,
            DBuffAffect affect, DBuffCancel cancel = null)
        {
            this.Buffer = buffer;
            this.Buffee = buffee;
            this.Interval = roundNum;
            this.ExecuteSection = executeSection;
            this.Name = name;
            this.BuffAffect = affect;
            this.BuffCancels = cancel;
        }

        /// <summary>buff引发</summary>
        public void BuffTrigger()
        {
            BuffAffect(this.Buffer, this.Buffee);
        }

        /// <summary>buff剩余时间减少</summary>
        /// <param name="time">减少的时间</param>
        /// <returns>减少后剩余时间是否小于等于0</returns>
        public bool Round(int time)
        {
            if (Interval <= time)
            {
                Interval = 0;
                return true;
            }
            if (Interval == Infinite) return false;
            Interval -= time;
            return false;
        } 

        /// <summary>buff结束</summary>
        public void BuffEnd()
        {
            BuffCancels();
        }

        /// <summary>重写object类的ToString方法</summary>
        /// <returns>转化为字符串的结果</returns>
        public override string ToString() => String.Format("{0} 剩余时间：{1}", Name, Interval);
    }
}
