using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace JLQ_MBE_BattleSimulation
{
    enum Group { Middle,Friend,Enemy=-1 }
    abstract class Character
    {
        //以下为字段
        //只读字段
        private readonly int id;//ID
        public readonly CharacterData data;//角色数据
        private readonly int maxMp;//最大灵力
        public readonly Group group;//阵营

        //可变字段
        //增益
        private float attackX = 1.0f;
        private float defenceX = 1.0f;
        private float hitRateX = 1.0f;
        private float dodgeRateX = 1.0f;
        private float closeAmendmentX = 1.0f;
        private float intervalX = 1.0f;
        private int moveAbilityX = 0;
        private int attackRangeX = 0;

        private Random random;
        private Game game;

        //属性
        //自动属性
        public int Hp { get; protected set; }//血量
        public int Mp { get; protected set; }//灵力
        public int CurrentTime { get; protected set; }//当前行动间隔
        public Point Posotion { get; protected set; }//位置
        public bool IsRounded { get; private set; }//是否已行动
        public bool IsMoved { get; private set; }//是否已移动
        public bool IsAttacked { get; private set; }//是否已攻击
        //TODO get a list of buff
        //只读属性
        public int Attack => (int) Math.Floor(data.attack*attackX);//攻击
        public int Defence => (int) Math.Floor(data.defence*defenceX);//防御
        public int HitRate => (int) Math.Floor(data.hitRate*hitRateX);//命中率
        public int DodgeRate => (int) Math.Floor(data.dodgeRate*dodgeRateX);//闪避率
        public int CloseAmendment => (int) Math.Floor(data.closeAmendment*closeAmendmentX);//近战补正
        public int Interval => (int) Math.Floor(data.interval*intervalX);//行动间隔
        public int MoveAbility => data.moveAbility + moveAbilityX;//机动
        public int AttackRange => data.attackRange + attackRangeX;//攻击范围
        private float CriticalHitGain => 1.5f;//暴击增益
        private float CriticalHitRate => 0.2f;//暴击率
        private float DamageFloat => 0.1f;//伤害浮动
        private bool IsDead => Hp <= 0;//是否死亡

        //构造函数
        protected Character(int id, Point position, Group group, Random random, Game game)
        {
            this.id = id;
            this.Posotion = position;
            this.group = group;
            IsRounded = false;
            IsMoved = false;
            IsAttacked = false;
            this.data = Calculate.characterDataList.Where(cd => cd.name == this.GetType().ToString()).ElementAt(0);
            this.Hp = this.data.maxHp;
            this.maxMp = 1000;
            this.Mp = maxMp;
            this.CurrentTime = this.data.interval;

            this.random = random;
            this.game = game;
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
                target.BeAttacked((int) damage, this);
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
                              "Interval: {8}\nMove Ability: {9}\nAttack Range: {10}\nCurrent Time: {11}", Hp, data.maxHp,
                    Attack, Defence, HitRate, DodgeRate, CloseAmendment, (CloseAmendment%1 == 0) ? ".0" : "", Interval,
                    MoveAbility, AttackRange, CurrentTime);

        //提示
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
            Hp = data.maxHp;
            IsRounded = false;
        }

        //以下为符卡
        public virtual void SC01() {}//符卡01
        public virtual void SC02() {}//符卡02
        public virtual void SC03() {}//符卡03


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
