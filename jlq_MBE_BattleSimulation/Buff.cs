using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JLQ_MBE_BattleSimulation
{
    class Buff
    {
        //没有参数的buff
        //默认的buff类可能会利用buff者自身与施加buff者的相关数据，可以对施加buff者造成影响
        //更复杂的buff需要自建子类
        //每个角色的buff效果自己编写，会有一些静态的buff供调用
        //开发者备注：静态的buff需要有一系列参数，每人使用lambda表达式代入参数的具体值
        public int RoundNum { get; protected set; }                 //剩余几回合结束buff
        public delegate void BuffAffect(Character buffee, Character buffer);//buff效果的delegate
        public delegate void BuffCancel();//取消buff
        public BuffAffect buffAffect;
        public BuffCancel buffCancels;
        public Character buffer;//buff发出者
        public Character buffee;//bugg承受者

        //构造函数
        public Buff(Character buffee, Character buffer, int roundNum, BuffAffect affect)
        {
            this.buffer = buffer;
            this.buffee = buffee;
            this.RoundNum = roundNum;
            this.buffAffect = affect;
        }

        //buff引发
        public void BuffTrigger()
        {
            buffAffect(this.buffer, this.buffee);
        }

        //轮数减少
        public bool Round() => (--RoundNum) == 0;

        //buff结束
        public void BuffEnd()
        {
            buffCancels();
        }

    }
}
