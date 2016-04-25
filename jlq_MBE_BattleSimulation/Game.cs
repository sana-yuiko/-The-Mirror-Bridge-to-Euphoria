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

namespace JLQ_MBE_BattleSimulation
{
    /// <summary>游戏类</summary>
    class Game
    {
        /// <summary>随机数对象</summary>
        private Random random;

        /// <summary>当前行动者</summary>
        public Character currentCharacter = null;

        /// <summary>是否为战斗模式</summary>
        public bool IsBattle { get; private set; }

        /// <summary>当前回合所在的阶段</summary>
        public Section? Section { get; set; }
        /// <summary>当前行动者是否已移动</summary>
        public bool HasMoved => currentCharacter.HasMoved;
        /// <summary>当前行动者是否已攻击</summary>
        public bool HasAttacked => currentCharacter.HasAttacked;


        /// <summary>游戏中所有角色列表</summary>
        public List<Character> Characters { get; } 

        /// <summary>Game类的构造函数</summary>
        public Game()
        {
            Characters = new List<Character>();
            this.Section = null;
            this.random = new Random();
            this.IsBattle = false;
        }

        //当前行动者属性

        /// <summary>当前行动者的位置</summary>
        public Point CurrentPosotion => currentCharacter.Position;
        /// <summary>当前行动者的scName</summary>
        public string[] ScName => currentCharacter.Data.ScName;
        /// <summary>当前行动者的scDisc</summary>
        public string[] ScDisc => currentCharacter.Data.ScDisc;

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
                        c.Group != currentCharacter.Group &&
                        Calculate.Distance(currentCharacter.Position, c.Position) <= currentCharacter.AttackRange);

        /// <summary>对当前行动者的阻挡列表</summary>
        public IEnumerable<Character> EnemyBlock
            => (currentCharacter is CharacterMovingIgnoreEnemy) ? new List<Character>() : EnemyAsCurrent;

        /// <summary>
        /// 对当前行动者的移动列表
        /// </summary>
        public IEnumerable<Character> EnemyAsCurrent =>
            Characters.Where(c => /*当前行动者中立且c非中立*/
                (currentCharacter.Group == Group.Middle && c.Group != Group.Middle) ||
                    /*当前行动者非中立且c与之敌对*/ (currentCharacter.Group != Group.Middle &&
                        c.Group == (Group) (-(int) currentCharacter.Group)));


        /// <summary>更新下个行动的角色,取currentTime最小的角色中Interval最大的角色中的随机一个</summary>
        public void GetNextRoundCharacter()
        {
            foreach(var c in Characters)
            {
                c.LabelDisplay.BorderThickness = new Thickness(0);
            }
            currentCharacter?.Reset();
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
            currentCharacter = stack.ElementAt(i);
            UpdateLabelBackground();

            var ct = currentCharacter.CurrentTime;
            foreach (var character in Characters)
            {
                character.CurrentTime -= ct;
                character.UpdateBarTime();
            }


            Generate_CanReachPoint();
        }

        /// <summary>更新角色显示的边框颜色</summary>
        public void UpdateLabelBackground()
        {
            foreach (var c in Characters)
            {
                c.LabelDisplay.Background = Brushes.White;
            }
            currentCharacter.LabelDisplay.Background = Brushes.LightPink;
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
            if (currentCharacter == null) return null;
            return Characters.FirstOrDefault(c => c.Position == position)?.Tip(currentCharacter) ?? null;
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
            AssignPointCanReach(currentCharacter.Position, currentCharacter.MoveAbility);
            var positionList = Characters.Where(c => c.Position != currentCharacter.Position).Select(c => c.Position);
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

        /// <summary>结算buff</summary>
        /// <param name="section">当前结算的阶段</param>
        public void BuffSettle(Section section)
        {
            var buffLists = Characters.Select(c => c.BuffList);
            IEnumerable<Buff> buffs = new List<Buff>();
            buffs = buffLists.Select(
                buffList => buffList.Where(b => b.buffer == currentCharacter && b.ExecuteSection == section))
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
        }

        /// <summary>改为战斗模式</summary>
        public void TurnToBattle()
        {
            IsBattle = true;
        }

        //TODO SC

        //TODO save&load

        //播放声音
        [DllImport("user32.dll")]
        public static extern bool MessageBeep(uint uType);

    }
}
