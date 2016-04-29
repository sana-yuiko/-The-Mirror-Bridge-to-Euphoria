using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace JLQ_MBE_BattleSimulation
{
    /// <summary>角色类</summary>
    public abstract class Character
    {
        //以下为字段
        //只读字段
        /// <summary>ID</summary>
        public readonly int ID;
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

        private float __intervalX;

        /// <summary>行动间隔增益</summary>
        private float _intervalX
        {
            get { return __intervalX; }
            set
            {
                __intervalX = value;
                CurrentTime = Math.Min(CurrentTime, Interval);
                BarTime.Maximum = Interval;
            }
        }
        /// <summary>机动增量</summary>
        private int _moveAbilityX = 0;
        /// <summary>攻击范围增量</summary>
        private int _attackRangeX = 0;

        /// <summary>随机数对象</summary>
        protected Random random;
        /// <summary>游戏对象</summary>
        protected Game game;

        //属性
        private int _hp;

        /// <summary>血量</summary>
        public int Hp
        {
            get { return _hp; }
            set
            {
                _hp = value;
                BarHp.Value = value;
            }
        }

        private int _mp;

        /// <summary>灵力</summary>
        public int Mp
        {
            get { return _mp; }
            set
            {
                _mp = value;
                BarMp.Value = value;
            }
        }

        private int _currentTime;

        /// <summary>当前剩余冷却时间</summary>
        public int CurrentTime
        {
            get { return _currentTime; }
            set
            {
                _currentTime = value;
                BarTime.Value = value;
            }
        }
        /// <summary>位置，X为Grid.Column，Y为Grid.Row</summary>
        public Point Position { get; protected set; }
        /// <summary>是否已移动</summary>
        public bool HasMoved { get; set; }
        /// <summary>是否已攻击</summary>
        public bool HasAttacked { get; set; }
        /// <summary>buff列表</summary>
        public List<Buff> BuffList { get; protected set; }

        //显示
        /// <summary>显示Display的Label</summary>
        public Label LabelDisplay { get; set; }
        /// <summary>显示Hp的ProgressBar</summary>
        public ProgressBar BarHp { get; set; }
        /// <summary>显示Mp的ProgressBar</summary>
        public ProgressBar BarMp { get; set; }
        /// <summary>显示剩余时间的ProgressBar</summary>
        public ProgressBar BarTime { get; set; }


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
        public bool IsDead => Hp <= 0;
        /// <summary>名字</summary>
        public string Name => Data.Name;

        /// <summary>被攻击结算的委托对象</summary>
        protected DBeAttacked HandleBeAttacked;
        /// <summary>是否命中的委托对象</summary>
        protected DIsHit HandleIsHit;
        /// <summary>近战增益的委托对象</summary>
        protected DCloseGain HandleCloseGain;
        /// <summary>是否暴击的委托对象</summary>
        protected DIsCriticalHit HandleIsCriticalHit;

        /// <summary>Character类的构造函数</summary>
        /// <param name="id">角色ID</param>
        /// <param name="position">角色位置</param>
        /// <param name="group">角色阵营</param>
        /// <param name="random">随机数对象</param>
        /// <param name="game">游戏对象</param>
        public Character(int id, Point position, Group group, Random random, Game game)
        {
            this.ID = id;
            this.Position = position;
            this.Group = group;
            HasMoved = false;
            HasAttacked = false;
            this.Data =
                Calculate.CharacterDataList.Where(cd => cd.Name == this.GetType().ToString().Substring(25)).ElementAt(0);
            this._maxMp = 1000;
            //初始化显示
            this.LabelDisplay = new Label
            {
                Margin = new Thickness(2, 2, 2, 11),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Top,
                Content = this.Data.Display,
                Padding = new Thickness(0),
                FontSize = 23
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

            this.BarHp = new ProgressBar
            {
                Margin = new Thickness(2, 0, 2, 8),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Bottom,
                Height = 2,
                Foreground = Brushes.Red,
                Maximum = this.Data.MaxHp,
                Value = this.Hp
            };
            this.BarTime = new ProgressBar
            {
                Margin = new Thickness(2, 0, 2, 2),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Bottom,
                Height = 2,
                Foreground = Brushes.Green,
                Maximum = this.Interval,
                Value = this.CurrentTime
            };
            this.BarMp = new ProgressBar
            {
                Margin = new Thickness(2, 0, 2, 5),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Bottom,
                Height = 2,
                Foreground = Brushes.Blue,
                Maximum = this._maxMp,
                Value = this.Mp
            };
            Set();

            this.Hp = this.Data.MaxHp;
            this.Mp = _maxMp;
            _intervalX = 1.0f;
            this.CurrentTime = this.Data.Interval;

            BuffList = new List<Buff>();
            this.random = random;
            this.game = game;
            //初始化委托
            HandleBeAttacked = BeAttacked;
            HandleIsHit = IsHit;
            HandleCloseGain = t => 1.0f;
            HandleIsCriticalHit = IsCriticalHit;
        }

        /// <summary>被攻击</summary>
        /// <param name="damage">伤害值</param>
        /// <param name="attacker">伤害来源</param>
        public void BeAttacked(int damage, Character attacker)
        {
            Damage(damage);
        }

        /// <summary>攻击</summary>
        /// <param name="target">攻击目标</param>
        /// <returns>是否暴击</returns>
        public bool DoAttack(Character target)
        {
            return DoAttack(target, 1.0f);
        }
        /// <summary>
        /// 攻击
        /// </summary>
        /// <param name="target">攻击目标</param>
        /// <param name="times">伤害值增益</param>
        /// <returns></returns>
        public bool DoAttack(Character target, float times)
        {
            //判断是否命中
            if (HandleIsHit(target)) return false;
            //判断是否近战
            var closeGain = HandleCloseGain(target);
            //计算基础伤害
            var damage = /*基础伤害*/ Calculate.Damage(this.Attack, target.Defence)*
                                     /*近战补正*/closeGain* /*伤害浮动*/((2*random.NextDouble() - 1)*this.DamageFloat + 1)*times;
            //判断是否暴击
            var isCriticalHit = HandleIsCriticalHit(target);
            if (isCriticalHit)
            {
                damage *= this.CriticalHitGain;
            }
            target.HandleBeAttacked((int)damage, this);
            return isCriticalHit;
        }

        /// <summary>将各数据转化为字符串显示</summary>
        /// <returns>各数据字符串化的结果</returns>
        public override string ToString()
            =>
                String.Format("HP: {0} / {1}\nMP: {2} / {3}\n攻击: {4}\n防御: {5}\n" +
                              "命中率: {6}\n闪避率: {7}\n近战补正: {8}{9}\n" +
                              "行动间隔: {10}\n机动: {11}\n攻击范围: {12}\n剩余冷却时间: {13}", Hp, Data.MaxHp, Mp, _maxMp,
                    Attack, Defence, HitRate, DodgeRate, CloseAmendment, (CloseAmendment%1 == 0) ? ".0" : "", Interval,
                    MoveAbility, AttackRange, CurrentTime);

        /// <summary>命中率和伤害值的信息提示</summary>
        /// <param name="target">攻击接受者</param>
        /// <returns></returns>
        public string Tip(Character target)
        {
            return String.Format("命中几率: {0}%\n平均伤害值: {1}",
                Math.Floor(Calculate.HitRate(this, target)*100),
                Calculate.Damage(this.Attack, target.Defence));
        }

        /// <summary>移动至指定坐标</summary>
        /// <param name="end">移动的目标坐标</param>
        public void Move(Point end)
        {
            this.Position = end;
            Set();
        }

        /// <summary>在各方向移动指定的值，若超限则取边界</summary>
        /// <param name="relativeX">移动的列向相对坐标</param>
        /// <param name="relativeY">移动的行向相对坐标</param>
        public void Move(int relativeX, int relativeY)
        {
            this.Position = new Point(GetValidPosition((int)this.Position.X + relativeX, MainWindow.Column),
                GetValidPosition((int)this.Position.Y + relativeY, MainWindow.Row));
            Set();

        }

        /// <summary>检测灵力是否足够</summary>
        /// <param name="mp">消耗的灵力量</param>
        /// <returns>灵力是否足够</returns>
        public bool IsMpEnough(int mp)
        {
            return Mp >= mp;
        }

        /// <summary>灵力消耗</summary>
        /// <param name="mp">消耗的灵力量</param>
        /// <returns>灵力是否足够</returns>
        public bool MpUse(int mp)
        {
            if (Mp < mp)
            {
                return false;
            }
            Mp -= mp;
            return true;
        }

        /// <summary>灵力获取</summary>
        /// <param name="mp">获得的灵力量</param>
        public virtual void MpGain(int mp)
        {
            Mp = Math.Min(_maxMp, Mp + mp);
        }


        //以下为符卡

        /// <summary>符卡01</summary>
        public abstract void SC01();
        /// <summary>结束使用符卡01</summary>
        public abstract void EndSC01();
        /// <summary>符卡02</summary>
        public abstract void SC02();
        /// <summary>结束使用符卡02</summary>
        public abstract void EndSC02();
        /// <summary>符卡03</summary>
        public abstract void SC03();
        /// <summary>结束使用符卡03</summary>
        public abstract void EndSC03();


        //以下为私有函数

        /// <summary>受伤</summary>
        /// <param name="damage">伤害值</param>
        private void Damage(int damage)
        {
            Hp -= damage;
        }

        /// <summary>将不合法的Position坐标项转化为合法值</summary>
        /// <param name="coordinate">待转化值</param>
        /// <param name="max">最大值</param>
        /// <returns></returns>
        private static int GetValidPosition(int coordinate, int max)
        {
            if (coordinate < 0)
            {
                return 0;
            }
            return coordinate > max ? max : coordinate;
        }

        //显示更新
        /// <summary>更新显示display的位置</summary>
        private void Set()
        {
            LabelDisplay.SetValue(Grid.ColumnProperty, (int) Position.X);
            LabelDisplay.SetValue(Grid.RowProperty, (int) Position.Y);
            BarHp.SetValue(Grid.ColumnProperty, (int)Position.X);
            BarHp.SetValue(Grid.RowProperty, (int)Position.Y);
            BarTime.SetValue(Grid.ColumnProperty, (int)Position.X);
            BarTime.SetValue(Grid.RowProperty, (int)Position.Y);
            BarMp.SetValue(Grid.ColumnProperty, (int)Position.X);
            BarMp.SetValue(Grid.RowProperty, (int)Position.Y);

        }

        /// <summary>是否命中</summary>
        /// <param name="target">攻击目标</param>
        /// <returns>是否命中</returns>
        protected virtual bool IsHit(Character target)
        {
            return random.NextDouble() > Calculate.HitRate(this, target);
        }
        /// <summary>近战增益</summary>
        /// <param name="target">攻击目标</param>
        /// <returns>近战增益</returns>
        protected virtual float CloseGain(Character target)
        {
            float closeGain;
            var distance = Calculate.Distance(this.Position, target.Position);
            if (distance == 1)
            {
                closeGain = this.CloseAmendment;
            }
            else
            {
                closeGain = 1.0f;
            }
            return closeGain;
        }
        /// <summary>是否暴击</summary>
        /// <param name="target">攻击目标</param>
        /// <returns>是否暴击</returns>
        protected virtual bool IsCriticalHit(Character target)
        {
            return random.NextDouble() <= this.CriticalHitRate;
        }

        //RESET
        /// <summary>恢复是否命中的委托对象</summary>
        protected void ResetHandleIsHit()
        {
            HandleIsHit = IsHit;
        }
        /// <summary>恢复近战增益的委托对象</summary>
        protected void ResetHandleCloseGain()
        {
            HandleCloseGain = t => 1.0f;
        }
        /// <summary>恢复是否暴击的委托对象</summary>
        protected void ResetHandleIsCriticalHit()
        {
            HandleIsCriticalHit = IsCriticalHit;
        }
        /// <summary>恢复被攻击结算的委托对象</summary>
        protected void ResetBeAttacked()
        {
            HandleBeAttacked = BeAttacked;
        }
    }
}
