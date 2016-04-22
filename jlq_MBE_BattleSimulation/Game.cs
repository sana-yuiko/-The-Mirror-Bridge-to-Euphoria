using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Runtime.InteropServices;

namespace JLQ_MBE_BattleSimulation
{
    class Game
    {
        private Random random;//随机数对象

        private const int Column = 9;//列数
        private const int Row = 9;//行数

        public List<Character> Characters { get; private set; }//所有角色 
        private Character currentCharacter = null;//当前行动者

        //构造函数
        public Game(Random random)
        {
            this.random = random;
            Characters = new List<Character>();
        }

        //当前行动者属性
        //当前行动者是否移动/攻击
        public bool IsMoved => currentCharacter.IsMoved;
        public bool IsAttacked => currentCharacter.IsAttacked;
        //当前行动者的位置
        public Point CurrentPosotion => currentCharacter.Posotion;
        //当前行动者的scName与scDisc
        public string[] ScName => currentCharacter.data.scName;
        public string[] ScDisc => currentCharacter.data.scDisc;


        //分别为友军，中立，敌军
        public IEnumerable<Character> FriendCharacters => Characters.Where(c => c.group == Group.Friend);
        public IEnumerable<Character> MiddleCharacters => Characters.Where(c => c.group == Group.Middle);
        public IEnumerable<Character> EnemyCharacters => Characters.Where(c => c.group == Group.Enemy);

        //攻击范围内的可攻击角色
        public IEnumerable<Character> EnemyCanAttack
            =>
                Characters.Where(
                    c =>
                        c.group != currentCharacter.group &&
                        Calculate.Distance(currentCharacter.Posotion, c.Posotion) <= currentCharacter.AttackRange);

        //可行动的角色列表
        public IEnumerable<Character> CharactersCanMove
            => Characters.Where(c => c.CurrentTime == 0 && (!c.IsRounded)).OrderByDescending(c => c.Interval);

        //对当前行动者的敌人列表
        public IEnumerable<Character> EnemyAsCurrent
            =>
                Characters.Where(
                    c => /*非IgnoreEnemy*/
                        (!(currentCharacter is CharacterMovingIgnoreEnemy)) && /*判断逻辑*/
                        ( /*当前行动者中立且c非中立*/
                            (currentCharacter.group == Group.Middle && c.group != Group.Middle) ||
                            /*当前行动者非中立且c与之敌对*/(currentCharacter.group != Group.Middle &&
                             c.group == (Group)(-(int)currentCharacter.group))));

        //每个格子能否被到达
        public bool[,] CanReachPoint = new bool[Column, Row];

        //格子的文字显示
        public string StringShow(Point position)
        {
            return Characters.FirstOrDefault(c => c.Posotion == position)?.ToString() ?? String.Empty;
        }

        //格子的提示
        public string TipShow(Point position)
        {
            return Characters.FirstOrDefault(c => c.Posotion == position)?.Tip(currentCharacter) ?? String.Empty;
        }

        //生成bool二维数组
        public void Generate_CanReachPoint()
        {
            for (var i = 0; i < Column; i++)
            {
                for (var j = 0; j < Row; j++)
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

        //将所有可以到达的点在bool二维数组中置为true
        public void AssignPointCanReach(Point origin, int step)
        {
            CanReachPoint[(int)origin.X, (int)origin.Y] = true;
            if (step == 0) return;
            var enm = this.EnemyAsCurrent.Select(c => c.Posotion);
            if (origin.Y < Row - 1 && !enm.Contains(new Point(origin.X, origin.Y + 1)))
            {
                AssignPointCanReach(new Point(origin.X, origin.Y + 1), step - 1);
            }
            if (origin.Y > 0 && !enm.Contains(new Point(origin.X, origin.Y - 1)))
            {
                AssignPointCanReach(new Point(origin.X, origin.Y - 1), step - 1);
            }
            if (origin.X < Column - 1 && !enm.Contains(new Point(origin.X + 1, origin.Y)))
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
