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
    public class Game
    {
        /// <summary>随机数对象</summary>
        public Random Random;

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

        /// <summary>每个格子能否被到达</summary>
        public bool[,] CanReachPoint = new bool[MainWindow.Column, MainWindow.Row];


        //窗体显示
        /// <summary>当前阶段</summary>
        public Label LabelSection { get; set; }
        /// <summary>是否已攻击</summary>
        public Label LabelAttack { get; set; }
        /// <summary>是否已移动</summary>
        public Label LabelMove { get; set; }
        /// <summary>用以响应鼠标事件的按钮</summary>
        public Button[,] Buttons { get; set; }
        /// <summary>符卡按钮</summary>
        public Button[] ButtonSC { get; set; }

        //符卡相关
        /// <summary>传递参数，如何获取目标以及所需参数列表</summary>
        public DIsTargetLegal IsTargetLegal;
        /// <summary>传递参数，如何处理目标</summary>
        public DHandleTarget HandleTarget;
        /// <summary>传递参数，判断单击位置是否有效</summary>
        public DIsLegalClick IsLegalClick;
        /// <summary>当前符卡序号，0为不处于符卡状态</summary>
        public int ScSelect;

        /// <summary>Game类的构造函数</summary>
        public Game()
        {
            Characters = new List<Character>();
            this.Random = new Random();
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
                Margin = new Thickness(0, 2, 0, 0),
                VerticalAlignment = VerticalAlignment.Top,
                Width = 115,
                FontWeight = FontWeights.SemiBold,
                Height = 25,
            };
            LabelMove.SetValue(Grid.ColumnProperty, 1);
            LabelMove.SetValue(Grid.ColumnSpanProperty, 2);
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
                Margin = new Thickness(0, 2, 0, 0),
                VerticalAlignment = VerticalAlignment.Top,
                Width = 115,
                FontWeight = FontWeights.SemiBold,
                Height = 25,
            };
            LabelAttack.SetValue(Grid.ColumnProperty, 4);
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
            //ButtonSC
            ButtonSC = new Button[3];
            for (var i = 0; i < 3; i++)
            {
                ButtonSC[i] = new Button
                {
                    Content = "SC0" + (i + 1),
                    IsEnabled = false,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };
            }

            this.Section = null;
        }

        /// <summary>确定在某位置的角色，若没有则返回null</summary>
        /// <param name="position">需要搜索角色的位置</param>
        /// <returns>在该位置的角色</returns>
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
        public virtual IEnumerable<Character> EnemyBlock => CurrentCharacter.EnemyBlock;

        /// <summary>对当前行动者的敌人列表</summary>
        public IEnumerable<Character> EnemyAsCurrent => CurrentCharacter.Enemy;


        /// <summary>更新下个行动的角色,取currentTime最小的角色中Interval最大的角色中的随机一个</summary>
        public void GetNextRoundCharacter()
        {
            CurrentCharacter?.ResetSCShow();
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
            var i = Random.Next(stack.Count);
            CurrentCharacter = stack.ElementAt(i);
            UpdateLabelBackground();

            var ct = CurrentCharacter.CurrentTime;
            foreach (var character in Characters)
            {
                character.CurrentTime -= ct;
            }
            CurrentCharacter.CurrentTime = CurrentCharacter.Interval;

            Generate_CanReachPoint();

            CurrentCharacter.SCShow();
        }

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
        private void AssignPointCanReach(Point origin, int step)
        {
            CanReachPoint[(int)origin.X, (int)origin.Y] = true;
            if (step == 0) return;
            var enm = this.EnemyBlock.Select(c => c.Position).ToList();
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

        /// <summary>结算buff</summary>
        /// <param name="section">当前结算的阶段</param>
        public void BuffSettle(Section section)
        {
            var buffLists = Characters.Select(c => c.BuffList);
            IEnumerable<Buff> buffs = new List<Buff>();
            buffs = buffLists.Select(
                buffList => buffList.Where(b => b.Buffer == CurrentCharacter && b.ExecuteSection == section))
                    .Aggregate(buffs, (current, buffsToOne) => current.Concat(buffsToOne));
            foreach (var buff in buffs)
            {
                buff.BuffAffect(buff.Buffee, buff.Buffer);
                if (buff.Round(CurrentCharacter.Interval))
                {
                    buff.BuffCancels();
                }
                Thread.Sleep(200);
            }
        }

        /// <summary>改为战斗模式</summary>
        public void TurnToBattle()
        {
            IsBattle = true;
        }

        //棋盘绘制相关
        /// <summary>恢复棋盘和角色正常颜色</summary>
        public void DefaultButtonAndLabels()
        {
            ResetPadButtons();
            foreach (var c in Characters.Where(c => c != CurrentCharacter))
            {
                c.LabelDisplay.Background = Brushes.White;
            }
            SetCurrentLabel();
        }

        /// <summary>生成与起始点距离小于等于范围的按钮的颜色</summary>
        /// <param name="origin">起始点</param>
        /// <param name="range">范围</param>
        public void SetButtonBackground(Point origin, int range)
        {
            for (var i = 0; i < MainWindow.Column; i++)
            {
                for (var j = 0; j < MainWindow.Row; j++)
                {
                    var point1 = new Point(i, j);
                    if (Calculate.Distance(point1, origin) <= range && this[point1] == null)
                    {
                        Buttons[i, j].Opacity = 1;
                    }
                }
            }
        }

        /// <summary>生成可到达点的按钮颜色</summary>
        public void PaintButton()
        {
            if (HasMoved) return;
            for (var i = 0; i < MainWindow.Column; i++)
            {
                for (var j = 0; j < MainWindow.Row; j++)
                {
                    if (!CanReachPoint[i, j]) continue;
                    if (new Point(i, j) != CurrentPosition)
                    {
                        Buttons[i, j].Opacity = 1;
                    }
                }
            }
        }

        /// <summary>将全部棋盘按钮置透明</summary>
        public void ResetPadButtons()
        {
            foreach (var b in Buttons)
            {
                b.Opacity = 0;
            }
        }

        /// <summary>更新角色标签颜色</summary>
        public void UpdateLabelBackground()
        {
            foreach (var c in Characters)
            {
                c.LabelDisplay.Background = Brushes.White;
            }
            SetCurrentLabel();
            if (HasAttacked) return;
            foreach (var c in EnemyCanAttack)
            {
                c.LabelDisplay.Background = Brushes.LightBlue;
            }
        }

        /// <summary>将当前行动角色标签颜色设为淡粉色</summary>
        public void SetCurrentLabel()
        {
            CurrentCharacter.LabelDisplay.Background = Brushes.LightPink;
        }

        //SC
        /// <summary>SC</summary>
        public void SC(int index)
        {
            ScSelect = index;
            switch (index)
            {
                case 1:
                    CurrentCharacter.SC01();
                    return;
                case 2:
                    CurrentCharacter.SC02();
                    return;
                case 3:
                    CurrentCharacter.SC03();
                    return;
            }
        }

        /// <summary>结束符卡结算</summary>
        public void EndSC()
        {
            var index = ScSelect;
            ScSelect = 0;
            IsTargetLegal = null;
            HandleTarget = null;
            IsLegalClick = null;
            switch (index)
            {
                case 1:
                    CurrentCharacter.EndSC01();
                    return;
                case 2:
                    CurrentCharacter.EndSC02();
                    return;
                case 3:
                    CurrentCharacter.EndSC03();
                    return;
            }
        }
        
        /// <summary>执行符卡效果</summary>
        public void DoSc()
        {
            
        }


        //TODO save&load

        /// <summary>播放声音</summary>
        /// <param name="uType"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool MessageBeep(uint uType);

    }
}
