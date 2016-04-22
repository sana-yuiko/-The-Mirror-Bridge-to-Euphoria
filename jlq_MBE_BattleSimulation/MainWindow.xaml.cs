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
        //棋盘列数与行数
        public const int Column = 9;//列数
        public const int Row = 9;//行数

        private DispatcherTimer timer = new DispatcherTimer { Interval = new TimeSpan(0 /*TODO Set Interval*/) };//Timer对象
        private Border[,] borders = new Border[Column, Row];//棋盘网格线
        private Button[,] buttons = new Button[Column, Row];//感知单击

        //构造函数
        public MainWindow()
        {
            InitializeComponent();

            XmlDocument data = new XmlDocument();
            XmlReader reader = XmlReader.Create("data.xml", new XmlReaderSettings {IgnoreComments = true /*忽略注释*/});
            data.Load(reader);
            XmlNodeList xnl = data.SelectSingleNode("/data/datas").ChildNodes;
            XmlNodeList xnscl = data.SelectSingleNode("/data/sc").ChildNodes;
            for (int i = 0, count = xnl.Count; i < count; i++)
            {
                CharacterData cd = new CharacterData();
                XmlElement xe = (XmlElement)xnl.Item(i);
                XmlElement xesc = (XmlElement)xnscl.Item(i);
                cd.name = xe.GetAttribute("id");
                XmlNodeList xnll = xe.ChildNodes;
                XmlNodeList xnscll = xesc.ChildNodes;
                cd.display = xnll.Item(0).InnerText;
                cd.maxHp = Convert.ToInt32(xnll.Item(1).InnerText);
                cd.attack = Convert.ToInt32(xnll.Item(2).InnerText);
                cd.defence = Convert.ToInt32(xnll.Item(3).InnerText);
                cd.hitRate = Convert.ToInt32(xnll.Item(4).InnerText);
                cd.dodgeRate = Convert.ToInt32(xnll.Item(5).InnerText);
                cd.closeAmendment = Convert.ToSingle(xnll.Item(6).InnerText);
                cd.interval = Convert.ToInt32(xnll.Item(7).InnerText);
                cd.moveAbility = Convert.ToInt32(xnll.Item(8).InnerText);
                cd.attackRange = Convert.ToInt32(xnll.Item(9).InnerText);
                //读取符卡描述
                cd.scName[0] = xnscll.Item(0).InnerText;
                cd.scName[1] = xnscll.Item(1).InnerText;
                cd.scName[2] = xnscll.Item(2).InnerText;
                cd.scName[3] = xnscll.Item(3).InnerText;
                cd.scDisc[0] = xnscll.Item(4).InnerText;
                cd.scDisc[1] = xnscll.Item(5).InnerText;
                cd.scDisc[2] = xnscll.Item(6).InnerText;
                cd.scDisc[3] = xnscll.Item(7).InnerText;
                Calculate.characterDataList.Add(cd);
                comboBoxDisplay.Items.Add(cd.display);
            }
            reader.Close();
        }

        private void GridPadMouseDown(int column, int row)
        {
            //TODO Pad Mouse Down
        }
        
        //帮助菜单
        private void menuHelp_Click(object sender, RoutedEventArgs e)
        {
            //TODO Pause
            MessageBox.Show("鼠标中键可以添加中立单位\n按住shift微移鼠标显示单位信息\n若已移动完毕，点击自身可跳过攻击阶段\n若暴击则会beep", "帮助", MessageBoxButton.OK,
                MessageBoxImage.Question);
            //TODO Continue
        }

        //询问是否关闭窗口
        private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("是否退出？", "退出", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
        }

        //生成棋盘网格线
        private void gridPad_Loaded(object sender, RoutedEventArgs e)
        {
            for (var i = 0; i < Column; i++)
            {
                for (var j = 0; j < Row; j++)
                {
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
            foreach (var button in buttons)
            {
                button.Click +=
                    (s, ev) =>
                        GridPadMouseDown((int) button.GetValue(Grid.ColumnProperty),
                            (int) button.GetValue(Grid.RowProperty));
            }
        }
    }
}
