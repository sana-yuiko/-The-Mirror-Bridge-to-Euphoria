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
        /// <summary>用于感知单击的按钮二维数组</summary>
        private Button[,] buttons = new Button[Column, Row];

        /// <summary>game对象</summary>
        private readonly Game game;

        /// <summary>加人模式的当前ID</summary>
        private int ID = 1;
        /// <summary>加人模式上一个添加的角色</summary>
        private Character characterLastAdd = null;

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

            //初始化game对象
            game = new Game();
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
                //以下你肯定凌乱了不过就是调用对应的构造函数创建角色对象而已
                var characterData = Calculate.CharacterDataList.First(cd => cd.Display == comboBoxDisplay.Text);
                Type[] constructorTypes =
                {
                    typeof (int), typeof (Point), typeof (Group), typeof (Random), typeof (Game)
                };
                var group = (leftButton == MouseButtonState.Pressed)
                    ? Group.Friend
                    : ((middleButton == MouseButtonState.Pressed) ? Group.Middle : Group.Enemy);
                object[] parameters = {ID, new Point(column, row), group, new Random(), game};
                characterLastAdd =
                    (Character)
                        Type.GetType("JLQ_MBE_BattleSimulation." + characterData.Name).GetConstructors()[0].Invoke(
                            parameters);
                //各种加入列表
                gridPad.Children.Add(characterLastAdd.LabelDisplay);
                game.Characters.Add(characterLastAdd);
                labelID.Content = (++ID).ToString();
                menuBackout.IsEnabled = true;
            }
            //战斗模式
            else
            {
                //如果不是行动阶段则操作非法
                if (game.Section != Section.Round) return;
                //如果单击的位置是合法移动点
                if (game.CanReachPoint[column, row])
                {
                    //如果已经移动过则操作非法
                    if (game.HasMoved)
                    {
                        MessageBox.Show("已移动过", "操作非法", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    //移动
                    game.currentCharacter.Move(new Point(column, row));
                    game.currentCharacter.HasMoved = true;
                    foreach (var b in buttons)
                    {
                        b.Opacity = 0;
                    }
                    labelMove.Content = "已移动";
                    labelMove.Foreground = Brushes.Gray;
                    game.UpdateLabelBorder();
                    //如果同时已经攻击过则进入结束阶段
                    if (!game.HasAttacked && game.EnemyCanAttack.Any()) return;
                    Thread.Sleep(500);
                    EndSection();
                }
                //如果单击的位置是合法攻击点
                else if (game.EnemyCanAttack.Select(c => c.Position).Contains(new Point(column, row)))
                {
                    //获取目标
                    var target = game.EnemyCanAttack.First(c => c.Position == new Point(column, row));
                    //如果已经攻击过则操作非法
                    if (game.HasAttacked)
                    {
                        MessageBox.Show("已攻击过", "操作非法", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    //攻击
                    game.currentCharacter.DoAttack(target);
                    game.currentCharacter.HasAttacked = false;
                    foreach (var c in game.EnemyCanAttack)
                    {
                        c.LabelDisplay.Background = Brushes.White;
                    }

                    //死人提示
                    if (target.IsDead)
                    {
                        gridPad.Children.Remove(target.LabelDisplay);
                        MessageBox.Show(String.Format("{0}号{1}已死亡", ID, target.Data.Name), "死亡",
                            MessageBoxButton.OK, MessageBoxImage.Hand);
                        game.Characters.Remove(target);
                    }

                    labelAttack.Content = "已攻击";
                    labelAttack.Foreground = Brushes.Gray;
                    //如果同时已经移动过则进入结束阶段
                    if (!game.HasMoved) return;
                    Thread.Sleep(500);
                    EndSection();
                }
                //单击位置非法，操作非法
                else
                {
                    MessageBox.Show("位置非法", "操作非法", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>更新label的文字提示</summary>
        private void UpdateSection()
        {
            labelSection.Content = Calculate.Convert(game.Section);
        }

        //游戏流程
        /// <summary>准备阶段</summary>
        public void PreparingSection()
        {
            //重置提示
            labelMove.Content = "还未移动";
            labelMove.Foreground = Brushes.Red;
            labelAttack.Content = "还未攻击";
            labelAttack.Foreground = Brushes.Red;
            //获取下个行动的角色
            game.GetNextRoundCharacter();
            for(var i = 0; i < Column; i++)
            {
                for (var j = 0; j < Row; j++)
                {
                    if (!game.CanReachPoint[i, j]) continue;
                    if (new Point(i, j) != game.CurrentPosotion)
                    {
                        buttons[i, j].Opacity = 1;
                    }
                }
            }

            //跳转阶段
            game.Section = Section.Preparing;
            UpdateSection();
            game.BuffSettle(JLQ_MBE_BattleSimulation.Section.Preparing);
            Thread.Sleep(500);
            game.Section = Section.Round;
            UpdateSection();
        }
        /// <summary>结束阶段</summary>
        public void EndSection()
        {
            game.currentCharacter.CurrentTime = game.currentCharacter.Interval;
            game.Section = Section.End;
            UpdateSection();
            game.BuffSettle(Section.End);
            Thread.Sleep(1000);

            PreparingSection();
        }


        /// <summary>帮助菜单</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuHelp_Click(object sender, RoutedEventArgs e)
        {
            //TODO Pause
            MessageBox.Show("鼠标中键可以添加中立单位\n按住shift微移鼠标显示单位信息\n若已移动完毕，点击自身可跳过攻击阶段\n若暴击则会beep", "帮助", MessageBoxButton.OK,
                MessageBoxImage.Question);
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
                        BorderBrush = new SolidColorBrush(Colors.Blue),
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
                    //生成网格内用来响应事件的按钮
                    buttons[i, j] = new Button
                    {
                        Margin = new Thickness(1),
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Background = Brushes.LightYellow,
                        Opacity = 0
                    };
                    buttons[i, j].SetValue(Grid.ColumnProperty, i);
                    buttons[i, j].SetValue(Grid.RowProperty, j);
                    buttons[i, j].SetValue(Grid.ColumnSpanProperty, 1);
                    buttons[i, j].SetValue(Grid.RowSpanProperty, 1);
                    buttons[i, j].SetValue(Panel.ZIndexProperty, 1);
                    gridPad.Children.Add(buttons[i, j]);
                }
            }
            //生成按钮事件
            foreach (var button in buttons)
            {
                int column = (int) button.GetValue(Grid.ColumnProperty);
                int row = (int) button.GetValue(Grid.RowProperty);
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
                        button.ToolTip = game.TipShow(new Point(column, row));
                    }
                    else
                    {
                        button.ToolTip = game.StringShow(new Point(column, row));
                    }
                };
            }
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
            game.Characters.Clear();
            characterLastAdd = null;
            menuBackout.IsEnabled = false;
            ID = 1;
            labelID.Content = "1";
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
            label2.Visibility = Visibility.Hidden;
            labelID.Visibility = Visibility.Hidden;
            comboBoxDisplay.Text = "";
            comboBoxDisplay.IsEnabled = false;
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
            gridPad.Children.Remove(characterLastAdd.LabelDisplay);
            game.Characters.Remove(characterLastAdd);
            labelID.Content = (--ID).ToString();
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

        private void SC01_Click(object sender, RoutedEventArgs e)
        {
            game.SC01();
        }
    }
}
