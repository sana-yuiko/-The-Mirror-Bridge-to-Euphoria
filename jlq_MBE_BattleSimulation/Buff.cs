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
        /// <summary>buff剩余时间</summary>
        public int TimeRemain { get; protected set; }

        ///// <summary>buff效果的委托对象</summary>
        //public DelegateEvent.BuffAffect buffAffect;
        /// <summary>buff效果取消的委托对象</summary>
        public DelegateEvent.BuffAffect buffCancel;

        /// <summary>buff发出者</summary>
        public Character buffer;
        /// <summary>buff承受者</summary>
        public Character buffee;

        /// <summary>Buff类的构造函数</summary>
        /// <param name="buffee">buff承受者</param>
        /// <param name="buffer">buff发出者</param>
        /// <param name="timeRemain">buff持续时间</param>
        /// <param name="affect">buff效果委托</param>
        /// <param name="buffCancel">buff效果取消委托</param>
        public Buff(Character buffer, Character buffee, int timeRemain, DelegateEvent.BuffAffect affect, DelegateEvent.BuffAffect buffCancel)
        {
            this.buffer = buffer;
            this.buffee = buffee;
            this.TimeRemain = timeRemain;
            affect(this.buffer, this.buffee);
            this.buffCancel = buffCancel;
        }

        /// <summary>buff时间流逝</summary>
        /// <param name="time">经过时间</param>
        /// <returns>是否结束buff</returns>
        public bool Time(int time)
        {
            TimeRemain -= time;
            if (TimeRemain <= 0)
            {
                EndBuff();
                buffee.BuffList.Remove(this);
            }
            return TimeRemain <= 0;
        }

        public void EndBuff()
        {
            buffCancel(buffee, buffer);
        }
    }
}
