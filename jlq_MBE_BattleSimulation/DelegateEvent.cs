﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace JLQ_MBE_BattleSimulation
{
    //委托声明

    /// <summary>被攻击的委托</summary>
    /// <param name="damage">伤害值</param>
    /// <param name="attacker">伤害来源</param>
    public delegate void DBeAttacked(int damage, Character attacker);

    /// <summary>buff效果的委托</summary>
    /// <param name="buffee">buff承受者</param>
    /// <param name="buffer">buff发出者</param>
    public delegate void DBuffAffect(Character buffee, Character buffer);

    /// <summary>取消buff的委托</summary>
    public delegate void DBuffCancel();

    /// <summary>如何处理目标的委托</summary>
    /// <param name="SCee">被使用符卡者</param>
    public delegate void DHandleTarget(Character SCee);

    /// <summary>如何选择目标的委托</summary>
    /// <param name="SCee">符卡目标</param>
    /// <param name="clickPoint">单击位置</param>
    /// <returns>该目标是否是符卡的合法目标</returns>
    public delegate bool DIsTargetLegal(Character SCee, Point clickPoint);

    /// <summary>所选位置是否为合法目标的委托</summary>
    /// <param name="clickPoint">单击位置</param>
    /// <returns>单击位置是否合法</returns>
    public delegate bool DIsLegalClick(Point clickPoint);




}
