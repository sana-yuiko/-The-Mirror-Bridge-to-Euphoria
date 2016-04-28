using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Media.TextFormatting;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Data;

namespace JLQ_MBE_BattleSimulation
{
    /// <summary>游戏类</summary>
    class Game
    {
        /// <summary>随机数对象</summary>
        private Random random;

        /// <summary>当前行动者</summary>
        public Character CurrentCharacter = null;

        /// <summary>是否为战斗模式</summary>
        public bool IsBattle { get; private set; }

        private Section? _section ;
        /// <summary>当前回合所在的阶段</summary>
        public Section? Section
        {
            get { return _section; }
            set
            {
                _section = value;
                LabelSection.Content = Calculate.Convert(value);
            }
        }


        /// <summary>当前行动者是否已移动</summary>
        public bool HasMoved
        {
            get { return CurrentCharacter?.HasMoved ?? false; }
            set
            {
                if (CurrentCharacter == null) return;
                CurrentCharacter.HasMoved = value;
                LabelMove.Content = value ? "已移动" : "还未移动";
            }
        }

        /// <summary>当前行动者是否已攻击</summary>
        public bool HasAttacked
        {
            get { return CurrentCharacter?.HasAttacked ?? false; }
            set
            {
                if (CurrentCharacter == null) return;
                CurrentCharacter.HasAttacked = value;
                LabelAttack.Content = value ? "已攻击" : "还未攻击";
            }
        }


        /// <summary>游戏中所有角色列表</summary>
        public List<Character> Characters { get; }

        //窗体显示
        /// <summary>当前阶段</summary>
        public Label LabelSection { get; set; }
        /// <summary>是否已攻击</summary>
        public Label LabelAttack { get; set; }
        /// <summary>是否已移动</summary>
        public Label LabelMove { get; set; }
        /// <summary>用以响应鼠标事件的按钮</summary>
        public Button[,] Buttons { get; set; }

        //符卡相关
        /// <summary>储存选择点方法的结构体</summary>
        public DelegateEvent.StructChoosePoint ChoosePoint;
        /// <summary>是否处于符卡选择目标状态中</summary>
        public bool isSCChoosingTarget = false;

        /// <summary>Game类的构造函数</summary>
        public Game()
        {
            Characters = new List<Character>();
            this.random = new Random();
            this.IsBattle = false;

            //LabelSection
            LabelSection = new Label
            {
                Content = "游戏还未开始",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(197, 47, 0, 0),
                Width = 115,
                FontWeight = FontWeights.SemiBold,
                Height = 25
            };
            LabelSection.SetValue(Grid.RowProperty, 2);
            var binding = new Binding
            {
                Source = LabelSection,
                Path = new PropertyPath("Content"),
                Converter = new ConverterContentToColor()
            };
            LabelSection.SetBinding(Label.ForegroundProperty, binding);
            //LabelMove
            LabelMove = new Label
            {
                Content = "还未移动",
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(82, 2, 0, 0),
                VerticalAlignment = VerticalAlignment.Top,
                Width = 115,
                FontWeight = FontWeights.SemiBold,
                Height = 25,
            };
            var binding2 = new Binding
            {
                Source = LabelMove,
                Path = new PropertyPath("Content"),
                Converter = new ConverterHasMovedToColor()
            };
            LabelMove.SetBinding(Label.ForegroundProperty, binding2);
            //LabelAttack
            LabelAttack = new Label
            {
                Content = "还未攻击",
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(259, 2, 0, 0),
                VerticalAlignment = VerticalAlignment.Top,
                Width = 115,
                FontWeight = FontWeights.SemiBold,
                Height = 25,
            };
            var binding3 = new Binding
            {
                Source = LabelAttack,
                Path = new PropertyPath("Content"),
                Converter = new ConverterHasAttackedToColor()
            };
            LabelAttack.SetBinding(Label.ForegroundProperty, binding3);
            //Buttons
            Buttons = new Button[MainWindow.Column, MainWindow.Row];
            for (var i = 0; i < MainWindow.Column; i++)
            {
                for (var j = 0; j < MainWindow.Row; j++)
                {
                    Buttons[i, j] = new Button
                    {
                        Margin = new Thickness(1),
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Background = Brushes.LightYellow,
                        Opacity = 0
                    };
                    Buttons[i, j].SetValue(Grid.ColumnProperty, i);
                    Buttons[i, j].SetValue(Grid.RowProperty, j);
                    Buttons[i, j].SetValue(Grid.ColumnSpanProperty, 1);
                    Buttons[i, j].SetValue(Grid.RowSpanProperty, 1);
                    Buttons[i, j].SetValue(Panel.ZIndexProperty, 1);
                }
            }

            this.Section = null;
        }

        public Character this[Point position] => Characters.FirstOrDefault(c => c.Position == position);

        //当前行动者属性

        /// <summary>当前行动者的位置</summary>
        public Point CurrentPosition => CurrentCharacter.Position;
        /// <summary>当前行动者的scName</summary>
        public string[] ScName => CurrentCharacter.Data.ScName;
        /// <summary>当前行动者的scDisc</summary>
        public string[] ScDisc => CurrentCharacter.Data.ScDisc;

        /// <summary>友军列表</summary>
        public IEnumerable<Character> FriendCharacters => Characters.Where(c => c.Group == Group.Friend);
        /// <summary>中立列表</summary>
        public IEnumerable<Character> MiddleCharacters => Characters.Where(c => c.Group == Group.Middle);
        /// <summary>敌军列表</summary>
        public IEnumerable<Character> EnemyCharacters => Characters.Where(c => c.Group == Group.Enemy);

        /// <summary>攻击范围内的可攻击角色</summary>
        public IEnumerable<Character> EnemyCanAttack
            =>
                EnemyAsCurrent.Where(
                    c =>
                        c.Group != CurrentCharacter.Group &&
                        Calculate.Distance(CurrentCharacter.Position, c.Position) <= CurrentCharacter.AttackRange);

        /// <summary>对当前行动者的阻挡列表</summary>
        public IEnumerable<Character> EnemyBlock
            => (CurrentCharacter is CharacterMovingIgnoreEnemy) ? new List<Character>() : EnemyAsCurrent;

        /// <summary>
        /// 对当前行动者的移动列表
        /// </summary>
        public IEnumerable<Character> EnemyAsCurrent =>
            Characters.Where(c => /*当前行动者中立且c非中立*/
                (CurrentCharacter.Group == Group.Middle && c.Group != Group.Middle) ||
                    /*当前行动者非中立且c与之敌对*/ (CurrentCharacter.Group != Group.Middle &&
                        c.Group == (Group) (-(int) CurrentCharacter.Group)));


        /// <summary>
        /// 更新下个行动的角色,取currentTime最小的角色中Interval最大的角色中的随机一个
        /// 依次结算buff的结束
        /// </summary>
        public void GetNextRoundCharacter()
        {
            foreach(var c in Characters)
            {
                c.LabelDisplay.BorderThickness = new Thickness(0);
            }
            //currentCharacter?.Reset();
            HasMoved = false;
            HasAttacked = false;
            var stack = new Stack<Character>();
            stack.Push(Characters.ElementAt(0));
            foreach (var character in Characters)
            {
                var temp = stack.Peek();
                if (character.CurrentTime < temp.CurrentTime)
                {
                    stack.Clear();
                    stack.Push(character);
                }
                else if (character.CurrentTime == temp.CurrentTime)
                {
                    if (character.Interval > temp.Interval)
                    {
                        stack.Clear();
                        stack.Push(character);
                    }
                    else if (character.Interval == temp.Interval)
                    {
                        stack.Push(character);
                    }
                }
            }
            var i = random.Next(stack.Count);
            CurrentCharacter = stack.ElementAt(i);
            UpdateLabelBackground();

            var ct = CurrentCharacter.CurrentTime;
            foreach (var character in Characters)
            {
                character.CurrentTime -= ct;
            }
            CurrentCharacter.CurrentTime = CurrentCharacter.Interval;

            Generate_CanReachPoint();
        }

        /// <summary>更新角色显示的边框颜色</summary>
        public void UpdateLabelBackground()
        {
            foreach (var c in Characters)
            {
                c.LabelDisplay.Background = Brushes.White;
            }
            CurrentCharacter.LabelDisplay.Background = Brushes.LightPink;
            foreach (var c in EnemyCanAttack)
            {
                c.LabelDisplay.Background = Brushes.LightBlue;
            }

        }

        /// <summary>每个格子能否被到达</summary>
        public bool[,] CanReachPoint = new bool[MainWindow.Column, MainWindow.Row];

        /// <summary>格子的文字显示</summary>
        /// <param name="position">格子</param>
        /// <returns>文字显示</returns>
        public string StringShow(Point position)
        {
            return Characters.FirstOrDefault(c => c.Position == position)?.ToString() ?? null;
        }

        /// <summary>格子的信息提示</summary>
        /// <param name="position">格子</param>
        /// <returns>信息提示</returns>
        public string TipShow(Point position)
        {
            if (CurrentCharacter == null) return null;
            return Characters.FirstOrDefault(c => c.Position == position)?.Tip(CurrentCharacter) ?? null;
        }

        /// <summary>生成bool二维数组</summary>
        public void Generate_CanReachPoint()
        {
            for (var i = 0; i < MainWindow.Column; i++)
            {
                for (var j = 0; j < MainWindow.Row; j++)
                {
                    CanReachPoint[i, j] = false;
                }
            }
            AssignPointCanReach(CurrentCharacter.Position, CurrentCharacter.MoveAbility);
            var positionList = Characters.Where(c => c.Position != CurrentCharacter.Position).Select(c => c.Position);
            foreach (var position in positionList)
            {
                CanReachPoint[(int) position.X, (int) position.Y] = false;
            }
        }

        /// <summary>将所有可以到达的点在bool二维数组中置为true</summary>
        /// <param name="origin">起点</param>
        /// <param name="step">步数</param>
        public void AssignPointCanReach(Point origin, int step)
        {
            CanReachPoint[(int)origin.X, (int)origin.Y] = true;
            if (step == 0) return;
            var enm = this.EnemyBlock.Select(c => c.Position);
            if (origin.Y < MainWindow.Row - 1 && !enm.Contains(new Point(origin.X, origin.Y + 1)))
            {
                AssignPointCanReach(new Point(origin.X, origin.Y + 1), step - 1);
            }
            if (origin.Y > 0 && !enm.Contains(new Point(origin.X, origin.Y - 1)))
            {
                AssignPointCanReach(new Point(origin.X, origin.Y - 1), step - 1);
            }
            if (origin.X < MainWindow.Column - 1 && !enm.Contains(new Point(origin.X + 1, origin.Y)))
            {
                AssignPointCanReach(new Point(origin.X + 1, origin.Y), step - 1);
            }
            if (origin.X > 0 && !enm.Contains(new Point(origin.X - 1, origin.Y)))
            {
                AssignPointCanReach(new Point(origin.X - 1, origin.Y), step - 1);
            }
        }

        /*/// <summary>结算buff</summary>
        /// <param name="section">当前结算的阶段</param>
        public void BuffSettle(Section section)
        {
            var buffLists = Characters.Select(c => c.BuffList);
            IEnumerable<Buff> buffs = new List<Buff>();
            buffs = buffLists.Select(
                buffList => buffList.Where(b => b.buffer == CurrentCharacter && b.ExecuteSection == section))
                    .Aggregate(buffs, (current, buffsToOne) => current.Concat(buffsToOne));
            foreach (var buff in buffs)
            {
                buff.buffAffect(buff.buffee, buff.buffer);
                if (buff.Round())
                {
                    buff.buffCancels();
                }
                Thread.Sleep(200);
            }
        }*/

        /// <summary>改为战斗模式</summary>
        public void TurnToBattle()
        {
            IsBattle = true;
        }

        /// <summary>不用选择目标的符卡攻击</summary>
        /// <param name="GetTarget">获得目标的方法</param>
        /// <param name="HandleTarget">处理目标的方法</param>
        public void SCAttack(DelegateEvent.DelegateGetTarget GetTarget, DelegateEvent.DelegateHandleTarget HandleTarget)
        {
            //如果没有这样的符卡则使用注释内的代码段：
            //符卡选中多个目标，在放出符卡的时候依次结算
            //结算完一个之后有可能会影响到“多个目标”的判定
            //比如说，符卡是，选定距离你3格以内的所有目标
            //依次冲向它们的脸前并依次造成多少伤害
            List<Character> CharactersCanAttack = new List<Character>();
            foreach(Character c in Characters)
            {
                if (GetTarget(CurrentCharacter, c)) CharactersCanAttack.Add(c);
            }
            foreach(Character c in CharactersCanAttack)
            {
                HandleTarget(CurrentCharacter, c);
            }
            /*foreach (Character c in Characters)
            {
                if (GetTarget(CurrentCharacter, c)) HandleTarget(CurrentCharacter, c);
            }*/
        }

        /// <summary>需要选择目标的符卡攻击</summary>
        /// <param name="ChoosePoint"></param>
        public void SCAttack(DelegateEvent.StructChoosePoint ChoosePoint)
        {
            isSCChoosingTarget = true;
            this.ChoosePoint = ChoosePoint;
        }


        //TODO save&load

        //播放声音
        [DllImport("user32.dll")]
        public static extern bool MessageBeep(uint uType);

    }
}
