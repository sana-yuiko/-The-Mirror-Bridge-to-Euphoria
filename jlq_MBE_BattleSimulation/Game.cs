using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Runtime.InteropServices;
using System.Windows.Media.TextFormatting;

namespace JLQ_MBE_BattleSimulation
{
    /// <summary>游戏类</summary>
    class Game
    {
        /// <summary>随机数对象</summary>
        private Random random;

        /// <summary>当前行动者</summary>
        private Character currentCharacter = null;

        /// <summary>游戏中所有角色列表</summary>
        public List<Character> Characters { get; private set; } 

        /// <summary>Game类的构造函数</summary>
        /// <param name="random">随机数对象</param>
        public Game(Random random)
        {
            this.random = random;
            Characters = new List<Character>();
        }

        //当前行动者属性

        /// <summary>当前行动者是否已移动</summary>
        public bool IsMoved => currentCharacter.IsMoved;
        /// <summary>当前行动者是否已攻击</summary>
        public bool IsAttacked => currentCharacter.IsAttacked;
        /// <summary>当前行动者的位置</summary>
        public Point CurrentPosotion => currentCharacter.Posotion;
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
                Characters.Where(
                    c =>
                        c.Group != currentCharacter.Group &&
                        Calculate.Distance(currentCharacter.Posotion, c.Posotion) <= currentCharacter.AttackRange);

        /// <summary>对当前行动者的敌人列表</summary>
        public IEnumerable<Character> EnemyAsCurrent
            =>
                Characters.Where(
                    c => /*非IgnoreEnemy*/
                        (!(currentCharacter is CharacterMovingIgnoreEnemy)) && /*判断逻辑*/
                        ( /*当前行动者中立且c非中立*/
                            (currentCharacter.Group == Group.Middle && c.Group != Group.Middle) ||
                            /*当前行动者非中立且c与之敌对*/(currentCharacter.Group != Group.Middle &&
                             c.Group == (Group)(-(int)currentCharacter.Group))));

        /// <summary>更新下个行动的角色,取currentTime最小的角色中Interval最大的角色中的随机一个</summary>
        public void NextRoundCharacter()
        {
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
        }


        /// <summary>每个格子能否被到达</summary>
        public bool[,] CanReachPoint = new bool[MainWindow.Column, MainWindow.Row];

        /// <summary>格子的文字显示</summary>
        /// <param name="position">格子</param>
        /// <returns>文字显示</returns>
        public string StringShow(Point position)
        {
            return Characters.FirstOrDefault(c => c.Posotion == position)?.ToString() ?? String.Empty;
        }

        /// <summary>格子的信息提示</summary>
        /// <param name="position">格子</param>
        /// <returns>信息提示</returns>
        public string TipShow(Point position)
        {
            return Characters.FirstOrDefault(c => c.Posotion == position)?.Tip(currentCharacter) ?? String.Empty;
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
            AssignPointCanReach(currentCharacter.Posotion, currentCharacter.MoveAbility);
            var positionList = Characters.Where(c => c.Posotion != currentCharacter.Posotion).Select(c => c.Posotion);
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
            var enm = this.EnemyAsCurrent.Select(c => c.Posotion);
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

        //TODO SC

        //TODO save&load

        //播放声音
        [DllImport("user32.dll")]
        public static extern bool MessageBeep(uint uType);

    }
}
