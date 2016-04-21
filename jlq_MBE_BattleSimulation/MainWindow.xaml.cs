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
using System.Xml;

namespace JLQ_MBE_BattleSimulation
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
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
                //加入备选的comboBox里
            }
            reader.Close();
        }
        
        private void menuHelp_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("鼠标中键可以添加中立单位\n按住shift微移鼠标显示单位信息\n若已移动完毕，点击自身可跳过攻击阶段\n若暴击则会beep", "帮助", MessageBoxButton.OK,
                MessageBoxImage.Question);
        }

        private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("是否退出？", "退出", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
        }
    }
}
