using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace JLQ_MBE_BattleSimulation
{
    static class DelegateEvent
    {
        /// <summary>声明如何选择目标的委托，不需要玩家点击即可返回目标</summary>
        /// <param name="SCer">使用符卡者</param>
        /// <param name="SCee">被使用符卡者</param>
        /// <returns>可否选择此目标</returns>
        public delegate bool DelegateGetTarget(Character SCer, Character SCee);
        /// <summary>声明如何处理目标的委托，不需要玩家点击即可返回目标</summary>
        /// <param name="SCer">使用符卡者</param>
        /// <param name="SCee">被使用符卡者</param>
        public delegate void DelegateHandleTarget(Character SCer, Character SCee);
        /// <summary>声明如何显示可选方格的委托，若为null则不显示，不需要玩家点击即可返回目标</summary>
        /// <param name="PSCer">使用符卡者所在方格</param>
        /// <param name="PSCee">被使用符卡者所在方格</param>
        /// <returns>可否选择此方格</returns>
        public delegate bool DelegateGetPosition(Point PSCer, Point PSCee);
        
        /// <summary>声明如何选择目标的委托，需要玩家点击返回目标</summary>
        /// <param name="SCer">使用符卡者</param>
        /// <param name="SCee">被使用符卡者</param>
        /// <param name="point">点击方格</param>
        /// <returns>可否选择此目标</returns>
        public delegate bool DelegateGetTargetNeedChoose(Character SCer, Character SCee, Point point);
        /// <summary>声明如何处理目标的委托，需要玩家点击返回目标</summary>
        /// <param name="SCer">使用符卡者</param>
        /// <param name="SCee">被使用符卡者</param>
        /// <param name="point">点击方格</param>
        public delegate void DelegateHandleTargetNeedChoose(Character SCer, Character SCee, Point point);
        /// <summary>声明如何选择方格的委托，需要玩家点击返回目标</summary>
        /// <param name="SCer">使用符卡者</param>
        /// <param name="point">点击方格</param>
        /// <returns>可否选择此方格</returns>
        public delegate bool DelegateChoosePoint(Character SCer, Point point);
        /// <summary>声明如何显示可选方格的委托，若为null则不显示，需要玩家点击返回目标</summary>
        /// <param name="PSCer">使用符卡者所在方格</param>
        /// <param name="PSCee">被使用符卡者所在方格</param>
        /// <param name="point">点击方格</param>
        /// <returns>可否选择此方格</returns>
        public delegate bool DelegateGetPositionNeedChoose(Point PSCer, Point PSCee, Point point);

        /// <summary>声明buff效果以及buff取消效果的委托</summary>
        /// <param name="buffee">buff承受者</param>
        /// <param name="buffer">buff发出者</param>
        public delegate void BuffAffect(Character buffer, Character buffee);
        

        public delegate void BeAttack(Character attacker, int damage);

        /// <summary>声明储存选择点方法的结构体</summary>
        public struct StructChoosePoint
        {
            public DelegateChoosePoint ChoosePoint;
            public DelegateGetTargetNeedChoose GetTarget;
            public DelegateHandleTargetNeedChoose HandleTarget;
            public StructChoosePoint(DelegateChoosePoint ChoosePoint, DelegateGetTargetNeedChoose GetTarget, DelegateHandleTargetNeedChoose HandleTarget)
            {
                this.ChoosePoint = ChoosePoint;
                this.GetTarget = GetTarget;
                this.HandleTarget = HandleTarget;
            }
        }
    }
}
