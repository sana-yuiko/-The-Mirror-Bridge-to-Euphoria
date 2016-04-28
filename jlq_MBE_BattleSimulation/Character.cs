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
    abstract class Character
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
        /// <summary>作为buffee的buff列表</summary>
        public List<Buff> BuffList { get; protected set; }

        //显示
        /// <summary>显示Display的Label</summary>
        public Label LabelDisplay { get; set; }
        public ProgressBar BarHp { get; set; }
        public ProgressBar BarMp { get; set; }
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
        public bool IsDead { get { if (Hp <= 0) game.Characters.Remove(this);
                return Hp <= 0; } }

        //Event相关
        //public event DelegateEvent.DamageHandler Damage;
        public DelegateEvent.BeAttack beAttack;
        
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
                Margin = new Thickness(2, 0, 2, 5),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Bottom,
                Height = 2,
                Foreground = Brushes.Green,
                Maximum = this.Interval,
                Value = this.CurrentTime
            };
            this.BarMp = new ProgressBar
            {
                Margin = new Thickness(2, 0, 2, 2),
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

            beAttack = new DelegateEvent.BeAttack(this.BeAttack);
        }

        public void BeAttack(Character attacker, int damage)
        {
            OnDamage(damage);
        }

        /// <summary>普通攻击</summary>
        /// <param name="target">攻击目标</param>
        /// <returns>是否暴击</returns>
        public bool DoAttack(Character target)
        {
            var distance = Calculate.Distance(this.Position, target.Position);
            return DoAttack(target, 1.0f, distance != 1, false);
        }

        /// <summary>攻击，符卡用</summary>
        /// <param name="target">攻击目标</param>
        /// <param name="times">伤害值增益</param>
        /// <param name="isDanmaku">是否为远程攻击</param>
        /// <param name="MustHit">是否必中</param>
        /// <returns>是否暴击</returns>
        public bool DoAttack(Character target, float times, bool isDanmaku, bool MustHit)
        {
            //判断是否命中
            if (!MustHit && random.NextDouble() > Calculate.HitRate(this, target)) return false;
            //判断是否近战
            float CloseGain = isDanmaku ? 1.0f : this.CloseAmendment;
            //判断是否暴击
            bool isCriticalHit = random.NextDouble() <= this.CriticalHitRate;
            float CriticalGain = isCriticalHit ? this.CriticalHitGain : 1.0f;
            //伤害浮动
            double DamageFloat = (2 * random.NextDouble() - 1) * this.DamageFloat + 1;
            double damage =/*基础伤害*/Calculate.Damage(this.Attack, target.Defence) *
                    /*近战补正*/CloseGain */*伤害浮动*/DamageFloat */*暴击伤害*/CriticalGain * times;
            target.beAttack(this, (int)damage);
            return isCriticalHit;
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

        /// <summary>时间流逝</summary>
        /// <param name="time">流逝时间</param>
        public void Time(int time)
        {
            CurrentTime -= time;
            List<Buff> BuffCancelList = new List<Buff>();
            foreach(Buff b in BuffList)
            {
                if (b.Time(time))
                {
                    //取消显示
                }
            }
        }

        //以下为符卡

        /// <summary>符卡01</summary>
        public abstract void SC01();
        /// <summary>符卡02</summary>
        public abstract void SC02();
        /// <summary>符卡03</summary>
        public abstract void SC03();


        //以下为私有函数

        /// <summary>受伤</summary>
        /// <param name="damage">伤害值</param>
        private void OnDamage(int damage)
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

    }
}
