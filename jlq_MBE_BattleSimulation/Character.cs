using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace JLQ_MBE_BattleSimulation
{
    /// <summary>阵营</summary>
    enum Group { Middle,Friend,Enemy=-1 }
    /// <summary>角色类</summary>
    abstract class Character
    {
        //以下为字段
        //只读字段
        /// <summary>ID</summary>
        private readonly int _id;
        /// <summary>角色数据</summary>
        public readonly CharacterData Data;
        /// <summary>最大灵力</summary>
        private readonly int _maxMp;
        /// <summary>阵营</summary>
        public readonly Group Group;

        //可变字段
        //增益
        /// <summary>攻击增益</summary>
        private float _attackX = 1.0f;
        /// <summary>防御增益</summary>
        private float _defenceX = 1.0f;
        /// <summary>命中率增益</summary>
        private float _hitRateX = 1.0f;
        /// <summary>闪避率增益</summary>
        private float _dodgeRateX = 1.0f;
        /// <summary>近战补正增益</summary>
        private float _closeAmendmentX = 1.0f;
        /// <summary>行动间隔增益</summary>
        private float _intervalX = 1.0f;
        /// <summary>机动增量</summary>
        private int _moveAbilityX = 0;
        /// <summary>攻击范围增量</summary>
        private int _attackRangeX = 0;

        /// <summary>随机数对象</summary>
        protected Random random;
        /// <summary>游戏对象</summary>
        protected Game game;

        //属性
        //自动属性
        /// <summary>血量</summary>
        public int Hp { get; protected set; }
        /// <summary>灵力</summary>
        public int Mp { get; protected set; }
        /// <summary>当前剩余冷却时间</summary>
        public int CurrentTime { get; protected set; }
        /// <summary>位置，X为Grid.Column，Y为Grid.Row</summary>
        public Point Position { get; protected set; }
        /// <summary>是否已移动</summary>
        public bool IsMoved { get; private set; }
        /// <summary>是否已攻击</summary>
        public bool IsAttacked { get; private set; }
        /// <summary>显示Display的Label</summary>
        public Label LabelDisplay { get; set; }
        /// <summary>buff列表</summary>
        public List<Buff> BuffList { get; protected set; } 
        /// <summary>用于显示Display的Label</summary>

        //只读属性
        /// <summary>攻击</summary>
        public int Attack => (int) Math.Floor(Data.Attack*_attackX);
        /// <summary>防御</summary>
        public int Defence => (int) Math.Floor(Data.Defence*_defenceX);
        /// <summary>命中率</summary>
        public int HitRate => (int) Math.Floor(Data.HitRate*_hitRateX);
        /// <summary>闪避率</summary>
        public int DodgeRate => (int) Math.Floor(Data.DodgeRate*_dodgeRateX);
        /// <summary>近战补正</summary>
        public int CloseAmendment => (int) Math.Floor(Data.CloseAmendment*_closeAmendmentX);
        /// <summary>行动间隔</summary>
        public int Interval => (int) Math.Floor(Data.Interval*_intervalX);
        /// <summary>机动</summary>
        public int MoveAbility => Data.MoveAbility + _moveAbilityX;
        /// <summary>攻击范围</summary>
        public int AttackRange => Data.AttackRange + _attackRangeX;
        /// <summary>暴击增益</summary>
        private float CriticalHitGain => 1.5f;
        /// <summary>暴击率</summary>
        private float CriticalHitRate => 0.2f;
        /// <summary>伤害浮动</summary>
        private float DamageFloat => 0.1f;
        /// <summary>是否死亡</summary>
        private bool IsDead => Hp <= 0;

        /// <summary>Character类的构造函数</summary>
        /// <param name="id">角色ID</param>
        /// <param name="position">角色位置</param>
        /// <param name="group">角色阵营</param>
        /// <param name="random">随机数对象</param>
        /// <param name="game">游戏对象</param>
        public Character(int id, Point position, Group group, Random random, Game game)
        {
            this._id = id;
            this.Position = position;
            this.Group = group;
            IsMoved = false;
            IsAttacked = false;
            this.Data =
                Calculate.CharacterDataList.Where(cd => cd.Name == this.GetType().ToString().Substring(25)).ElementAt(0);
            this.Hp = this.Data.MaxHp;
            this._maxMp = 1000;
            this.Mp = _maxMp;
            this.CurrentTime = this.Data.Interval;

            this.LabelDisplay = new Label
            {
                Margin = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Content = this.Data.Display,
            };
            switch (this.Group)
            {
                case Group.Friend:
                    LabelDisplay.Foreground = Brushes.Red;
                    break;
                case Group.Middle:
                    LabelDisplay.Foreground = Brushes.Black;
                    break;
                default:
                    LabelDisplay.Foreground = Brushes.Green;
                    break;
            }
            SetLabel();

            this.random = random;
            this.game = game;
        }

        /// <summary>被攻击</summary>
        /// <param name="damage">伤害值</param>
        /// <param name="attacker">攻击者</param>
        public void BeAttacked(int damage, Character attacker)
        {
            Damage(damage);
        }

        /// <summary>攻击</summary>
        /// <param name="target">攻击目标</param>
        /// <returns>是否暴击</returns>
        public bool DoAttack(Character target)
        {
            bool isCriticalHit = false;
            int distance = Calculate.Distance(this.Position, target.Position);
            //判断是否命中
            if (random.NextDouble() <= Calculate.HitRate(this, target))
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

        public void SetTime()
        {
            //TODO SetTime
            //if (CurrentTime == 0)
            //{
            //    CurrentTime = this.Interval - 1;
            //}
            //else
            //{
            //    CurrentTime--;
            //}
            IsMoved = false;
            IsAttacked = false;
        }

        /// <summary>将各数据转化为字符串显示</summary>
        /// <returns>各数据字符串化的结果</returns>
        public override string ToString()
            =>
                String.Format("HP: {0} / {1}\nAttack: {2}\nDefence: {3}\n" +
                              "Hit Rate: {4}\nDodge Rate: {5}\nClose Gain: {6}{7}\n" +
                              "Interval: {8}\nMove Ability: {9}\nAttack Range: {10}\nCurrent Time: {11}", Hp, Data.MaxHp,
                    Attack, Defence, HitRate, DodgeRate, CloseAmendment, (CloseAmendment%1 == 0) ? ".0" : "", Interval,
                    MoveAbility, AttackRange, CurrentTime);

        /// <summary>命中率和伤害值的信息提示</summary>
        /// <param name="target">攻击接受者</param>
        /// <returns></returns>
        public string Tip(Character target)
        {
            return String.Format("Rate: {0}%\nDamage: {1}",
                Math.Floor(Calculate.HitRate(this, target)*100),
                Calculate.Damage(this.Attack, target.Defence));
        }

        /// <summary>重置属性</summary>
        public void Reset()
        {
            CurrentTime = Interval;
            Hp = Data.MaxHp;
        }

        /// <summary>移动至指定坐标</summary>
        /// <param name="end">移动的目标坐标</param>
        public void Move(Point end)
        {
            this.Position = end;
            SetLabel();
        }

        /// <summary>在各方向移动指定的值，若超限则取边界</summary>
        /// <param name="relativeX">移动的列向相对坐标</param>
        /// <param name="relativeY">移动的行向相对坐标</param>
        public void Move(int relativeX, int relativeY)
        {
            this.Position = new Point(GetValidPosition((int)this.Position.X + relativeX, MainWindow.Column),
                GetValidPosition((int)this.Position.Y + relativeY, MainWindow.Row));
            SetLabel();

        }


        //以下为符卡

        /// <summary>符卡01</summary>
        public virtual void SC01() {}
        /// <summary>符卡02</summary>
        public virtual void SC02() {}
        /// <summary>符卡03</summary>
        public virtual void SC03() {}


        //以下为私有函数

        /// <summary>受伤</summary>
        /// <param name="damage">伤害值</param>
        private void Damage(int damage)
        {
            Hp -= damage;
            //TODO whether dead
        }

        /// <summary>将不合法的Position坐标项转化为合法值</summary>
        /// <param name="coordinate">待转化值</param>
        /// <param name="max">最大值</param>
        /// <returns></returns>
        private int GetValidPosition(int coordinate,int max)
        {
            if (coordinate < 0)
            {
                return 0;
            }
            else if (coordinate > max)
            {
                return max;
            }
            else
            {
                return coordinate;
            }
        }

        private void SetLabel()
        {
            LabelDisplay.SetValue(Grid.ColumnProperty, (int) Position.X);
            LabelDisplay.SetValue(Grid.RowProperty, (int) Position.Y);
        }
    }
}
