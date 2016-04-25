using System;
using System.Collections.Generic;
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
    class Buff
    {
        /// <summary>buff剩余轮数</summary>
        public int RoundNum { get; protected set; }

        /// <summary>buff效果的委托</summary>
        /// <param name="buffee">buff承受者</param>
        /// <param name="buffer">buff发出者</param>
        public delegate void BuffAffect(Character buffee, Character buffer);

        /// <summary>取消buff的委托</summary>
        public delegate void BuffCancel();

        /// <summary>buff效果的委托对象</summary>
        public BuffAffect buffAffect;

        /// <summary>取消buff的委托对象</summary>
        public BuffCancel buffCancels;

        /// <summary>buff发出者</summary>
        public Character buffer;

        /// <summary>buff承受者</summary>
        public Character buffee;

        /// <summary>Buff类的构造函数</summary>
        /// <param name="buffee">buff承受者</param>
        /// <param name="buffer">buff发出者</param>
        /// <param name="roundNum">buff持续回合数</param>
        /// <param name="affect">buff效果委托</param>
        public Buff(Character buffee, Character buffer, int roundNum, BuffAffect affect)
        {
            this.buffer = buffer;
            this.buffee = buffee;
            this.RoundNum = roundNum;
            this.buffAffect = affect;
        }

        /// <summary>buff引发</summary>
        public void BuffTrigger()
        {
            buffAffect(this.buffer, this.buffee);
        }

        /// <summary>轮数减少</summary>
        /// <returns></returns>
        public bool Round() => (--RoundNum) == 0;

        /// <summary>buff结束</summary>
        public void BuffEnd()
        {
            buffCancels();
        }

    }
}
