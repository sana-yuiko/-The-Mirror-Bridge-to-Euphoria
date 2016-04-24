using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        /// <summary>随机数对象</summary>
        private Random random = new Random();
        /// <summary>game对象</summary>
        private Game game;

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
                CharacterData cd = new CharacterData();
                XmlElement xe = (XmlElement)xnl.Item(i);
                XmlElement xesc = (XmlElement)xnscl.Item(i);
                cd.Name = xe.GetAttribute("id");
                XmlNodeList xnll = xe.ChildNodes;
                XmlNodeList xnscll = xesc.ChildNodes;
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

                Calculate.characterDataList.Add(cd);
                comboBoxDisplay.Items.Add(cd.Display);
            }
            reader.Close();
            //初始化game对象
            game = new Game(random);
        }

        private void GridPadMouseDown(int column, int row)
        {
            //TODO Pad Mouse Down
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
                        VerticalAlignment = VerticalAlignment.Stretch
                    };
                    borders[i, j].SetValue(Grid.ColumnProperty, i);
                    borders[i, j].SetValue(Grid.RowProperty, j);
                    borders[i, j].SetValue(Grid.ColumnSpanProperty, 1);
                    borders[i, j].SetValue(Grid.RowSpanProperty, 1);
                    borders[i, j].SetValue(Panel.ZIndexProperty, 1);
                    gridPad.Children.Add(borders[i, j]);
                    //生成网格内用来响应事件的按钮
                    buttons[i, j] = new Button
                    {
                        Margin = new Thickness(1),
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
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
                button.Click +=
                    (s, ev) =>
                        GridPadMouseDown(column, row);
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
    }
}
