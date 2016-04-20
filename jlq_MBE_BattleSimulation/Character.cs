using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace JLQ_MBE_BattleSimulation
{
    enum Group { Middle,Own,Enemy }
    class Character
    {
        //以下为字段
        //只读字段
        private readonly int id;//ID
        private readonly int maxHp;//最大血量
        private readonly int maxMp;//最大灵力
        private readonly int attack;//攻击
        private readonly int defence;//防御
        private readonly int hitRate;//命中率
        private readonly int dodgeRate;//闪避率
        private readonly float closeAmendment;//近战补正
        private readonly int interval;//间隔
        private readonly int moveAbility;//机动
        private readonly int attackRange;//攻击范围

        //可变字段
        //增益
        private float attackX;
        private float defenceX;
        private float hitRateX;
        private float dodgeRateX;
        private float closeAmendmentX;
        private float intervalX;
        private int moveAbilityX;
        private int attackRangeX;

        private Random random;

        //属性
        //自动属性
        public string Display { get; protected set; }//屏幕显示
        public int Hp { get; protected set; }//血量
        public int Mp { get; protected set; }//灵力
        public int CurrentTime { get; protected set; }//当前行动间隔
        public Point Posotion { get; protected set; }//位置
        public Group Group { get; protected set; }//阵营
        public bool IsRounded { get; private set; }//是否已行动
        public bool IsMoved { get; private set; }//是否已移动
        public bool IsAttacked { get; private set; }//是否已攻击
        //TODO get a list of buff
        //只读属性
        public int Attack => (int) Math.Floor(attackX*attack);//攻击
        public int Defence => (int) Math.Floor(defence*defenceX);//防御
        public int HitRate => (int) Math.Floor(hitRate*hitRateX);//命中率
        public int DodgeRate => (int) Math.Floor(dodgeRate*dodgeRateX);//闪避率
        public int CloseAmendment => (int) Math.Floor(closeAmendment*closeAmendmentX);//近战补正
        public int Interval => (int) Math.Floor(interval*intervalX);//行动间隔
        public int MoveAbility => moveAbility + moveAbilityX;//机动
        public int AttackRange => attackRange + attackRangeX;//攻击范围
        private float CriticalHitGain => 1.5f;//暴击增益
        private float CriticalHitRate => 0.2f;//暴击率
        private float DamageFloat => 0.1f;//伤害浮动
        private bool IsDead => Hp <= 0;//是否死亡

        //构造函数
        public Character(int id, Point position, Group group, Random random)
        {
            this.id = id;
            this.Posotion = position;
            this.Group = group;
            IsRounded = false;
            IsMoved = false;
            IsAttacked = false;
            var data = Calculate.characterDataList.Where(cd => cd.name == this.GetType().ToString()).ElementAt(0);
            this.maxHp = data.maxHp;
            this.Hp = maxHp;
            this.maxMp = 1000;
            this.Mp = maxMp;
            Display = data.display;
            this.attack = data.attack;
            this.defence = data.defence;
            this.hitRate = data.hitRate;
            this.dodgeRate = data.dodgeRate;
            this.closeAmendment = data.closeAmendment;
            this.interval = data.interval;
            this.moveAbility = data.moveAbility;
            this.attackRange = data.attackRange;
            this.CurrentTime = interval;
            this.random = random;
        }

        //被攻击
        public void BeAttacked(int damage, Character attacker)
        {
            Damage(damage);
        }

        //攻击，返回是否暴击
        public bool DoAttack(Character target)
        {
            bool isCriticalHit = false;
            int distance = Calculate.Distance(this.Posotion, target.Posotion);
            //判断是否命中
            if (random.NextDouble() <=Calculate.HitRate(this.HitRate - target.DodgeRate, distance))
            {
                float CloseGain;
                //判断是否近战
                if (distance == 1)
                {
                    CloseGain = this.CloseAmendment;
                }
                else
                {
                    CloseGain = 1.0f;
                }
                double damage;
                //判断是否暴击
                isCriticalHit = random.NextDouble() <= this.CriticalHitRate;
                damage =/*基础伤害*/Calculate.Damage(this.Attack, target.Defence)*
                        /*近战补正*/CloseGain*/*伤害浮动*/((2*random.NextDouble() - 1)*this.DamageFloat + 1);
                if (isCriticalHit)
                {
                    damage *= this.CriticalHitGain;
                }
                target.Damage((int)damage);
            }
            return isCriticalHit;
        }

        //行动
        public void Rounded()
        {
            this.IsRounded = true;
        }

        //计时
        public void SetTime()
        {
            if (CurrentTime == 0)
            {
                CurrentTime = this.Interval - 1;
            }
            else
            {
                CurrentTime--;
            }
            IsRounded = false;
            IsMoved = false;
            IsAttacked = false;
        }

        //行动
        public void Round(Point clickPosition)
        {
            //TODO Round
        }

        //重写object类的ToString方法
        public override string ToString()
            =>
                String.Format("HP: {0} / {1}\nAttack: {2}\nDefence: {3}\n" +
                              "Hit Rate: {4}\nDodge Rate: {5}\nClose Gain: {6}{7}\n" +
                              "Interval: {8}\nMove Ability: {9}\nAttack Range: {10}\nCurrent Time: {11}", Hp, maxHp,
                    Attack, Defence, HitRate, DodgeRate, CloseAmendment, (CloseAmendment%1 == 0) ? ".0" : "", Interval,
                    MoveAbility, AttackRange, CurrentTime);

        public string Tip(Character attacker)
        {
            return String.Format("Rate: {0}%\nDamage: {1}",
                Math.Floor(Calculate.HitRate(attacker.HitRate - this.DodgeRate,
                    Calculate.Distance(this.Posotion, attacker.Posotion))),
                Calculate.Damage(attacker.Attack, this.Defence));
        }

        public void Reset()
        {
            CurrentTime = Interval;
            Hp = maxHp;
            IsRounded = false;
        }


        //以下为私有函数
        //受伤
        private void Damage(int damage)
        {
            Hp -= damage;
            //TODO whether dead
        }

        //移动
        private void Move(Point end)
        {
            this.Posotion = end;
        }

    }
}
