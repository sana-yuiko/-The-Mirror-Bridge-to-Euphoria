using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;

namespace JLQ_MBE_BattleSimulation
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>棋盘列数</summary>
        public const int Column = 9;
        /// <summary>棋盘行数</summary>
        public const int Row = 9;

        //private DispatcherTimer timer = new DispatcherTimer { Interval = new TimeSpan(0 /*TODO Set Interval*/) };//Timer对象
        /// <summary>棋盘网格线</summary>
        private Border[,] borders = new Border[Column, Row];

        /// <summary>game对象</summary>
        private readonly Game game;

        private int _id = 1;

        /// <summary>加人模式的当前ID</summary>
        private int ID
        {
            get { return _id; }
            set
            {
                _id = value;
                labelID.Content = value.ToString();
            }
        }
        /// <summary>加人模式上一个添加的角色</summary>
        private Character characterLastAdd = null;
        /// <summary>鼠标的网格位置</summary>
        private Point mousePoint = new Point(-1, -1);
        /// <summary>符卡按钮</summary>
        private Button[] scButtons = new Button[3];
        /// <summary>角色数标签</summary>
        private Label[] labels = new Label[3];

        /// <summary>构造函数</summary>
        public MainWindow()
        {
            InitializeComponent();

            //读取角色各数据
            XmlDocument data = new XmlDocument();
            XmlReader reader = XmlReader.Create("data.xml", new XmlReaderSettings {IgnoreComments = true /*忽略注释*/});
            data.Load(reader);
            XmlNodeList xnl = data.SelectSingleNode("/data/datas").ChildNodes;
            XmlNodeList xnscl = data.SelectSingleNode("/data/sc").ChildNodes;
            for (int i = 0, count = xnl.Count; i < count; i++)
            {
                //读取角色数据
                var cd = new CharacterData();
                var xe = (XmlElement)xnl.Item(i);
                var xesc = (XmlElement)xnscl.Item(i);
                cd.Name = xe.GetAttribute("id");
                var xnll = xe.ChildNodes;
                var xnscll = xesc.ChildNodes;
                cd.Display = xnll.Item(0).InnerText;
                cd.MaxHp = Convert.ToInt32(xnll.Item(1).InnerText);
                cd.Attack = Convert.ToInt32(xnll.Item(2).InnerText);
                cd.Defence = Convert.ToInt32(xnll.Item(3).InnerText);
                cd.HitRate = Convert.ToInt32(xnll.Item(4).InnerText);
                cd.DodgeRate = Convert.ToInt32(xnll.Item(5).InnerText);
                cd.CloseAmendment = Convert.ToSingle(xnll.Item(6).InnerText);
                cd.Interval = Convert.ToInt32(xnll.Item(7).InnerText);
                cd.MoveAbility = Convert.ToInt32(xnll.Item(8).InnerText);
                cd.AttackRange = Convert.ToInt32(xnll.Item(9).InnerText);
                //读取符卡描述
                cd.ScName[0] = xnscll.Item(0).InnerText;
                cd.ScName[1] = xnscll.Item(1).InnerText;
                cd.ScName[2] = xnscll.Item(2).InnerText;
                cd.ScName[3] = xnscll.Item(3).InnerText;
                cd.ScDisc[0] = xnscll.Item(4).InnerText;
                cd.ScDisc[1] = xnscll.Item(5).InnerText;
                cd.ScDisc[2] = xnscll.Item(6).InnerText;
                cd.ScDisc[3] = xnscll.Item(7).InnerText;

                Calculate.CharacterDataList.Add(cd);
                comboBoxDisplay.Items.Add(cd.Display);
            }
            reader.Close();
            //加入数组
            scButtons[0] = buttonSC01;
            scButtons[1] = buttonSC02;
            scButtons[2] = buttonSC03;
            labels[0] = labelEnemy;
            labels[1] = labelMiddle;
            labels[2] = labelFriend;
            //初始化game对象
            game = new Game();
        }

        /// <summary>当前行动角色</summary>
        private Character currentCharacter => game.CurrentCharacter;
        /// <summary>当前游戏阶段</summary>
        private Section? section
        {
            get { return game.Section; }
            set { game.Section = value; }
        }

        /// <summary>添加角色</summary>
        /// <param name="point">添加的位置</param>
        /// <param name="group">角色的阵营</param>
        /// <param name="display">显示的字符串</param>
        private void AddCharacter(Point point, Group group, string display)
        {
            //以下你肯定凌乱了不过就是调用对应的构造函数创建角色对象而已
            var characterData = Calculate.CharacterDataList.First(cd => cd.Display == display);
            Type[] constructorTypes =
            {
                    typeof (int), typeof (Point), typeof (Group), typeof (Random), typeof (Game)
                };
            object[] parameters = { ID, point, group, game.Random, game };
            characterLastAdd =
                (Character)
                    Type.GetType("JLQ_MBE_BattleSimulation." + characterData.Name).GetConstructors()[0].Invoke(
                        parameters);
            //各种加入列表
            gridPad.Children.Add(characterLastAdd.LabelDisplay);
            gridPad.Children.Add(characterLastAdd.BarHp);
            gridPad.Children.Add(characterLastAdd.BarTime);
            gridPad.Children.Add(characterLastAdd.BarMp);
            game.Characters.Add(characterLastAdd);
            ID++;
            menuBackout.IsEnabled = true;
            var labelTemp = labels[(int) group + 1];
            labelTemp.Content = Convert.ToInt32(labelTemp.Content) + 1;
        }

        private void RemoveCharacter(Character target)
        {
            var labelTemp = labels[(int) target.Group + 1];
            labelTemp.Content = Convert.ToInt32(labelTemp.Content) - 1;
            gridPad.Children.Remove(target.LabelDisplay);
            gridPad.Children.Remove(target.BarHp);
            gridPad.Children.Remove(target.BarTime);
            gridPad.Children.Remove(target.BarMp);
            game.Characters.Remove(target);

        }

        /// <summary>网格单击事件</summary>
        /// <param name="column">单击位置的列向坐标</param>
        /// <param name="row">单击位置的横向坐标</param>
        /// <param name="leftButton">鼠标左键状态</param>
        /// <param name="middleButton">鼠标中键状态</param>
        /// <param name="rightButton">鼠标右键状态</param>
        private void GridPadMouseDown(int column, int row, MouseButtonState leftButton, MouseButtonState middleButton,
            MouseButtonState rightButton)
        {
            //TODO Pad Mouse Down
            //加人模式
            if (!game.IsBattle)
            {
                //如果没选角色Display则操作非法
                if (String.IsNullOrEmpty(comboBoxDisplay.Text))
                {
                    MessageBox.Show("请选择角色！", "操作非法", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                //如果这个位置已添加则操作非法
                if (game.Characters.Any(c => c.Position == new Point(column, row)))
                {
                    MessageBox.Show("此位置已有角色！", "操作非法", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                //添加角色
                var group = (leftButton == MouseButtonState.Pressed)
                    ? Group.Friend
                    : ((middleButton == MouseButtonState.Pressed) ? Group.Middle : Group.Enemy);
                AddCharacter(new Point(column, row), group, comboBoxDisplay.Text);
            }
            //战斗模式
            else
            {
                //如果不是行动阶段则操作非法
                if (section != Section.Round) return;
                //如果单击的位置是合法移动点
                if (game.CanReachPoint[column, row])
                {
                    //如果已经移动过则操作非法
                    if (game.HasMoved)
                    {
                        if (currentCharacter.Position != new Point(column, row))
                        {
                            MessageBox.Show("已移动过", "操作非法", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        EndSection();
                        return;
                    }
                    //移动
                    currentCharacter.Move(new Point(column, row));
                    game.HasMoved = true;
                    foreach (var b in game.Buttons)
                    {
                        b.Opacity = 0;
                    }
                    game.UpdateLabelBackground();
                    //如果同时已经攻击过则进入结束阶段
                    if (!game.HasAttacked && game.EnemyCanAttack.Any()) return;
                    //Thread.Sleep(500);
                    EndSection();
                }
                //如果单击的位置是合法攻击点
                else if (game.EnemyCanAttack.Any(c => c.Position == new Point(column, row)))
                {
                    //如果已经攻击过则操作非法
                    if (game.HasAttacked)
                    {
                        MessageBox.Show("已攻击过", "操作非法", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    //获取目标
                    var target = game[new Point(column, row)];
                    //攻击
                    currentCharacter.DoAttack(target);
                    game.HasAttacked = true;
                    foreach (var c in game.EnemyCanAttack)
                    {
                        c.LabelDisplay.Background = Brushes.White;
                    }

                    //死人提示
                    IsDead(target);
                    game.Generate_CanReachPoint();
                    Paint();

                    //如果同时已经移动过则进入结束阶段
                    if (!game.HasMoved) return;
                    //Thread.Sleep(500);
                    EndSection();
                }
                //单击位置非法，操作非法
                else
                {
                    MessageBox.Show("位置非法", "操作非法", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        //游戏流程
        /// <summary>准备阶段</summary>
        private void PreparingSection()
        {
            //重置提示
            //获取下个行动的角色
            game.GetNextRoundCharacter();
            for (var i = 0; i < 3; i++)
            {
                scButtons[i].Content = currentCharacter.Data.ScName[i + 1];
                scButtons[i].ToolTip = currentCharacter.Data.ScDisc[i + 1];
            }
            Paint();

            //跳转阶段
            section = Section.Preparing;
            game.BuffSettle(Section.Preparing);
            //Thread.Sleep(500);
            section = Section.Round;
        }
        /// <summary>结束阶段</summary>
        private void EndSection()
        {
            section = Section.End;
            game.BuffSettle(Section.End);
            //Thread.Sleep(1000);
            //游戏是否结束
            if (!game.FriendCharacters.Any())
            {
                MessageBox.Show("敌方获胜！", "游戏结束", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            if (!game.EnemyCharacters.Any())
            {
                MessageBox.Show("己方获胜！", "游戏结束", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            PreparingSection();
        }

        /// <summary>生成正确的网格颜色</summary>
        private void Paint()
        {
            for (var i = 0; i < Column; i++)
            {
                for (var j = 0; j < Row; j++)
                {
                    if (!game.CanReachPoint[i, j]) continue;
                    if (new Point(i, j) != game.CurrentPosition)
                    {
                        game.Buttons[i, j].Opacity = 1;
                    }
                }
            }
        }

        private void DefaultButtonBackground()
        {
            foreach (var b in game.Buttons)
            {
                b.Opacity = 0;
            }
            foreach (var c in game.Characters)
            {
                c.LabelDisplay.Background = Brushes.White;
            }
        }

        /// <summary>
        /// 将与起始点距离小于等于范围的点设为淡黄色
        /// </summary>
        /// <param name="origin">起始点</param>
        /// <param name="range">范围</param>
        private void SetBackground(Point origin, int range)
        {
            for (var i = 0; i < Column; i++)
            {
                for (var j = 0; j < Row; j++)
                {
                    var point1 = new Point(i, j);
                    if (point1 != origin && Calculate.Distance(point1, origin) <= range &&
                        point1 != game.CurrentPosition)
                    {
                        game.Buttons[i, j].Opacity = 1;
                    }
                }
            }
        }

        /// <summary>死亡结算</summary>
        /// <param name="target"></param>
        private void IsDead(Character target)
        {
            if (!target.IsDead) return;
            MessageBox.Show(
                String.Format("{0}号{1}{2}被{3}号{4}{5}杀死", target.ID, Calculate.Convert(target.Group), target.Name,
                    currentCharacter.ID, Calculate.Convert(currentCharacter.Group), currentCharacter.Name), "死亡",
                MessageBoxButton.OK, MessageBoxImage.Hand);
            RemoveCharacter(target);
        }

        /// <summary>随机添加角色</summary>
        /// <param name="group">角色阵营</param>
        /// <param name="number">添加数量</param>
        private void RandomlyAddCharacters(Group group, int number)
        {
            var points = new List<Point>();
            for (var i = 0; i < Column; i++)
            {
                for (var j = 0; j < Row; j++)
                {
                    points.Add(new Point(i, j));
                }
            }
            var pointsCanAdd = points.Where(p => game[p] == null);
            if (pointsCanAdd.Count() < number)
            {
                MessageBox.Show("空格不足！", "添加失败", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var count = Calculate.CharacterDataList.Count;
            for (var i = 0; i < number; i++)
            {
                var index = game.Random.Next(pointsCanAdd.Count());
                var displayIndex = game.Random.Next(count);
                AddCharacter(pointsCanAdd.ElementAt(index), group, Calculate.CharacterDataList.ElementAt(displayIndex).Display);
            }
            if (!(bool) checkBox.IsChecked) return;
            MessageBox.Show("生成成功", "成功", MessageBoxButton.OK, MessageBoxImage.Information);

        }


        /// <summary>帮助-操作菜单</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuHelp_Click(object sender, RoutedEventArgs e)
        {
            //TODO Pause
            MessageBox.Show(
                "加人模式左键添加己方单位，中键添加中立单位，右键单击敌方单位；\n鼠标悬停在单位上方显示数据，按住shift微移鼠标显示单位详细信息；\n点击自身可跳过行动阶段；\n若暴击则会beep。", "操作",
                MessageBoxButton.OK, MessageBoxImage.Question);
            //TODO Continue
        }

        /// <summary>关闭时询问是否关闭窗口</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("是否退出？", "退出", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
        }

        /// <summary>生成棋盘网格内的控件</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gridPad_Loaded(object sender, RoutedEventArgs e)
        {
            for (var i = 0; i < Column; i++)
            {
                for (var j = 0; j < Row; j++)
                {
                    //生成网格线
                    borders[i, j] = new Border
                    {
                        BorderBrush = new SolidColorBrush(Colors.Black),
                        BorderThickness = new Thickness(1),
                        Margin = new Thickness(0),
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                    };
                    borders[i, j].SetValue(Grid.ColumnProperty, i);
                    borders[i, j].SetValue(Grid.RowProperty, j);
                    borders[i, j].SetValue(Grid.ColumnSpanProperty, 1);
                    borders[i, j].SetValue(Grid.RowSpanProperty, 1);
                    borders[i, j].SetValue(Panel.ZIndexProperty, 0);
                    gridPad.Children.Add(borders[i, j]);
                    
                    gridPad.Children.Add(game.Buttons[i, j]);
                }
            }
            //生成按钮事件
            foreach (var button in game.Buttons)
            {
                var column = (int) button.GetValue(Grid.ColumnProperty);
                var row = (int) button.GetValue(Grid.RowProperty);
                button.MouseDown += (s, ev) =>
                {
                    if (ev.LeftButton == MouseButtonState.Released)
                    {
                        GridPadMouseDown(column, row, MouseButtonState.Released, ev.MiddleButton, ev.RightButton);
                    }
                };
                button.Click +=
                    (s, ev) =>
                        GridPadMouseDown(column, row, MouseButtonState.Pressed, MouseButtonState.Released,
                            MouseButtonState.Released);
                button.MouseMove += (s, ev) =>
                {
                    if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                    {
                        button.ToolTip = game.StringShow(new Point(column, row));
                    }
                    else
                    {
                        button.ToolTip = game.TipShow(new Point(column, row));
                    }
                };
                button.MouseEnter += (s, ev) =>
                {
                    mousePoint = new Point(column, row);
                };
                button.MouseLeave += (s, ev) =>
                {
                    mousePoint = new Point(-1, -1);
                };
                button.KeyDown += (s, ev) =>
                {
                    //如果shift和ctrl都没被按下或不在行动阶段或不在棋盘内或该点无角色或该点角色为当前角色则无效
                    if ((!(Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift) ||
                           Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))) ||
                        section != Section.Round || mousePoint == new Point(-1, -1) ||
                        game.Characters.All(c => c.Position != mousePoint) ||
                        mousePoint == game.CurrentPosition) return;
                    //如果shift被按下
                    if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                    {
                        var character = game[mousePoint];
                        if (character != null)
                        {
                            //清屏
                            DefaultButtonBackground();

                            SetBackground(mousePoint, character.AttackRange);
                            currentCharacter.LabelDisplay.Background = Brushes.LightPink;
                            character.LabelDisplay.Background = Brushes.LightBlue;
                        }
                    }
                    //如果ctrl被按下
                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        var character = game[mousePoint];
                        if (character != null)
                        {
                            //清屏
                            DefaultButtonBackground();

                            SetBackground(mousePoint, character.MoveAbility);
                            currentCharacter.LabelDisplay.Background = Brushes.LightPink;
                            character.LabelDisplay.Background = Brushes.LightBlue;
                        }
                    }

                };
                button.KeyUp += (s, ev) =>
                {
                    //如果不在行动阶段或仍有shift或ctrl在棋盘内则无效
                    if (section != Section.Round) return;
                    if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift) ||
                        Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) return;
                    //恢复原本显示
                    foreach (var b in game.Buttons)
                    {
                        b.Opacity = 0;
                    }
                    Paint();
                    game.UpdateLabelBackground();
                };
            }

            gridWindow.Children.Add(game.LabelSection);
            gridGame.Children.Add(game.LabelAttack);
            gridGame.Children.Add(game.LabelMove);
        }

        /// <summary>退出菜单</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>清除已添加的所有单位</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuClear_Click(object sender, RoutedEventArgs e)
        {
            var labels = game.Characters.Select(c => c.LabelDisplay);
            foreach (var l in labels)
            {
                gridPad.Children.Remove(l);
            }
            var progressBars = game.Characters.Select(c => c.BarHp);
            progressBars = progressBars.Concat(game.Characters.Select(c => c.BarTime));
            progressBars = progressBars.Concat(game.Characters.Select(c => c.BarMp));
            foreach (var p in progressBars)
            {
                gridPad.Children.Remove(p);
            }
            game.Characters.Clear();
            characterLastAdd = null;
            menuBackout.IsEnabled = false;
            ID = 1;
            labelID.Content = "1";
            foreach (var label in labels)
            {
                label.Content = "0";
            }
        }

        /// <summary>模式切换</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuPattern_Click(object sender, RoutedEventArgs e)
        {
            if (game.Characters.Count == 0)
            {
                MessageBox.Show("还未加人！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            menuPattern.IsEnabled = false;
            menuBackout.IsEnabled = false;
            menuClear.IsEnabled = false;
            labelShow.Content = "战斗模式";
            //控件操作
            label2.Visibility = Visibility.Hidden;
            labelID.Visibility = Visibility.Hidden;
            comboBoxDisplay.Text = "";
            comboBoxDisplay.IsEnabled = false;
            comboBoxEnemy.IsEnabled = false;
            comboBoxEnemy.Text = "";
            comboBoxFriend.IsEnabled = false;
            comboBoxFriend.Text = "";
            comboBoxMiddle.IsEnabled = false;
            comboBoxMiddle.Text = "";
            buttonGenerateFriend.IsEnabled = false;
            buttonGenerateEnemy.IsEnabled = false;
            buttonGenerateMiddle.IsEnabled = false;
            labelShow.Foreground = Brushes.Black;
            game.TurnToBattle();
            PreparingSection();

        }

        /// <summary>撤销上一次添加的角色</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuBackout_Click(object sender, RoutedEventArgs e)
        {
            if (characterLastAdd == null) return;
            RemoveCharacter(characterLastAdd);
            ID--;
            if (game.Characters.Count == 0)
            {
                characterLastAdd = null;
                menuBackout.IsEnabled = false;
            }
            else
            {
                characterLastAdd = game.Characters.Last();
            }
        }

        private void menuShow_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "红色字体为己方单位，黑色字体为中立单位，绿色字体为敌方单位；\n" + "战斗模式下：淡粉色为当前行动单位；\n淡蓝色为当前行动单位可攻击的单位；\n淡黄色为可以移动至的位置\n" +
                "鼠标悬停在单位上方：\n按下Shift显示该角色的攻击范围；\n按下ctrl显示该角色的移动范围。", "显示", MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void buttonGenerateFriend_Click(object sender, RoutedEventArgs e)
        {
            RandomlyAddCharacters(Group.Friend, Int32.Parse(comboBoxFriend.Text));
        }

        private void buttonGenerateEnemy_Click(object sender, RoutedEventArgs e)
        {
            RandomlyAddCharacters(Group.Enemy, Int32.Parse(comboBoxEnemy.Text));
        }

        private void buttonGenerateMiddle_Click(object sender, RoutedEventArgs e)
        {
            RandomlyAddCharacters(Group.Middle, Int32.Parse(comboBoxMiddle.Text));
        }
    }
}
