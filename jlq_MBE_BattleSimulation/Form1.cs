using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Runtime.InteropServices;

namespace jlq_MBE_BattleSimulation
{
    public partial class Form1 : Form
    {
        private const int length = 9;                           //棋盘长度
        private const int height = 9;                           //棋盘宽度
        private Graphics pag;
        private const float upSpace = 1.0f / 5;                 //画布：上方空白大小，比例
        private const float downSpace = 1.0f / 5;               //画布：下方空白大小，比例
        private const float leftSpace = 2.0f / 5;               //画布：左方空白大小，比例
        private const float rightSpace = 1.0f / 10;             //画布：右方空白大小，比例
        private const float leftSpell = 1.0f / 15;              //画布：符卡左方空白大小，比例
        private const float upSpell = 1.0f / 5;                 //画布：符卡上方空白大小，比例
        private const float spellHeight = 1.0f / 10;            //画布：符卡高度，比例
        private const float spellWidth = 1.0f / 5;              //画布：符卡宽度，比例
        private const float spellSpace = 1.0f / 15;             //画布：符卡垂直间距，比例
        private bool mouseMoving = true;                        //显示tip的中间变量，鼠标是否移走
        private pad pad = new pad();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //读取data.xml文件中储存的角色原始数据与角色符卡描述
            XmlDocument data = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;          //忽略文档里面的注释
            XmlReader reader = XmlReader.Create("data.xml", settings);
            data.Load(reader);
            XmlNode xn = data.SelectSingleNode("/data/datas");
            XmlNodeList xnl = xn.ChildNodes;
            XmlNode xnsc = data.SelectSingleNode("/data/sc");
            XmlNodeList xnscl = xnsc.ChildNodes;
            for (int i = 0; i < xnl.Count; i++)
            {
                chctdata cd = new chctdata();
                XmlElement xe = (XmlElement)xnl.Item(i);
                XmlElement xesc = (XmlElement)xnscl.Item(i);
                cd.who = (charc)Enum.Parse(typeof(charc), xe.GetAttribute("id").ToString());
                XmlNodeList xnll = xe.ChildNodes;
                XmlNodeList xnscll = xesc.ChildNodes;
                cd.display = xnll.Item(0).InnerText;
                cd.maxHp = Convert.ToInt16(xnll.Item(1).InnerText);
                cd.att = Convert.ToInt16(xnll.Item(2).InnerText);
                cd.def = Convert.ToInt16(xnll.Item(3).InnerText);
                cd.hit = Convert.ToInt16(xnll.Item(4).InnerText);
                cd.dod = Convert.ToInt16(xnll.Item(5).InnerText);
                cd.clo = Convert.ToSingle(xnll.Item(6).InnerText);
                cd.spd = Convert.ToInt16(xnll.Item(7).InnerText);
                cd.mov = Convert.ToInt16(xnll.Item(8).InnerText);
                cd.rang = Convert.ToInt16(xnll.Item(9).InnerText);
                //读取符卡描述
                cd.scName[0] = xnscll.Item(0).InnerText;
                cd.scName[1] = xnscll.Item(1).InnerText;
                cd.scName[2] = xnscll.Item(2).InnerText;
                cd.scName[3] = xnscll.Item(3).InnerText;
                cd.scDisc[0] = xnscll.Item(4).InnerText;
                cd.scDisc[1] = xnscll.Item(5).InnerText;
                cd.scDisc[2] = xnscll.Item(6).InnerText;
                cd.scDisc[3] = xnscll.Item(7).InnerText;
                cal.chctdataList.Add(cd);
                //加入备选的comboBox里
                comboBox1.Items.Add(cd.display);
            }
            reader.Close();
            comboBox1.SelectedIndex = 0;
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            pag = CreateGraphics();
            rePaint();
        }

        public void rePaint()
        {
            Pen pLines = new Pen(Color.Blue, 1);            //画笔：网格线
            Brush bOwnChrc = new SolidBrush(Color.Red);     //画笔：己方单位
            Brush bEnmChrc = new SolidBrush(Color.Green);   //画笔：敌方单位
            Brush bMidChrc = new SolidBrush(Color.Black);   //画笔：中立单位
            Brush bWho = new SolidBrush(Color.Gray);        //画笔：下一个要行动的单位
            Brush bRange = new SolidBrush(Color.LightPink); //画笔：标明射程
            Brush bMov = new SolidBrush(Color.LightBlue);   //画笔：标明移动
            Brush bEnm = new SolidBrush(Color.LightPink);   //画笔：可以攻击的敌方单位
            Brush bSpace = new SolidBrush(Color.LightBlue); //画笔：可以移动到的空格子
            Brush bBack = new SolidBrush(Color.White);      //画笔：血条与行动条背景
            Brush bHp = new SolidBrush(Color.Red);          //画笔：血条
            Brush bMp = new SolidBrush(Color.Blue);         //画笔：蓝条
            Brush bTime = new SolidBrush(Color.Green);      //画笔：时间条
            Size s = new Size((int)Math.Floor((1 - leftSpace - rightSpace) / length * Width) - 1, (int)Math.Floor((1 - upSpace - downSpace) / height * Height) - 1);
            //清理画布
            pag.Clear(Color.White);
            //绘制网格
            for (int i = 0; i <= length; i++)
                pag.DrawLine(pLines, (leftSpace + i * (1 - leftSpace - rightSpace) / length) * Width, upSpace * Height, (leftSpace + i * (1 - leftSpace - rightSpace) / length) * Width, (1 - downSpace) * Height);
            for (int i = 0; i <= height; i++)
                pag.DrawLine(pLines, leftSpace * Width, (upSpace + i * (1 - upSpace - downSpace) / height) * Height, (1 - rightSpace) * Width, (upSpace + i * (1 - upSpace - downSpace) / height) * Height);
            //绘制符卡框
            pag.DrawRectangle(pLines, leftSpell * Width, upSpell * Height, spellWidth * Width, spellHeight * Height);
            pag.DrawRectangle(pLines, leftSpell * Width, (upSpell + spellHeight + spellSpace) * Height, spellWidth * Width, spellHeight * Height);
            pag.DrawRectangle(pLines, leftSpell * Width, (upSpell + 2 * spellHeight + 2 * spellSpace) * Height, spellWidth * Width, spellHeight * Height);
            //如果游戏开始
            if (pad.isBattle)
            {
                if (!pad.IsAttacked)
                    //绘制可以攻击到的敌方单位
                    foreach (pos p in pad.canEnm)
                        pag.FillRectangle(bEnm, turnStB(p).x + 2, turnStB(p).y + 2, s.Width, s.Height);
                //MessageBox.Show("123");
                if (!pad.IsMoved)
                    //绘制可以到达的空格子
                    foreach (pos p in pad.CanReachPos)
                        pag.FillRectangle(bSpace, turnStB(p).x + 2, turnStB(p).y + 2, s.Width, s.Height);
                //绘制该谁行动了
                pag.FillRectangle(bWho, turnStB(pad.hangPos).x + 2, turnStB(pad.hangPos).y + 2, s.Width, s.Height);
                //绘制符卡文字
                pag.DrawString(pad.scName[1], new Font("宋体", 15), bMidChrc, leftSpell * Width, upSpell * Height);
                pag.DrawString(pad.scName[2], new Font("宋体", 15), bMidChrc, leftSpell * Width, (upSpell + spellHeight + spellSpace) * Height);
                pag.DrawString(pad.scName[3], new Font("宋体", 15), bMidChrc, leftSpell * Width, (upSpell + 2 * spellHeight + 2 * spellSpace) * Height);
            }
            foreach (chct c in pad.Chcts)
            {
                //绘制场上单位
                pag.DrawString(c.display, new Font("宋体", 25), c.where == 0 ? bMidChrc : (c.where == 1 ? bEnmChrc : bOwnChrc), turnX(c.pos.x) + 1, turnY(c.pos.y) + 1);
                //绘制血条背景
                pag.FillRectangle(bBack, turnX(c.pos.x) + (1 - leftSpace - rightSpace) / length * Width / 10 + 1, turnY(c.pos.y) + (1 - upSpace - downSpace) / height * Height * 4 / 5, (1 - leftSpace - rightSpace) / length * Width * 4 / 5 - 2, 6);
                //绘制血条
                pag.FillRectangle(bHp, turnX(c.pos.x) + (1 - leftSpace - rightSpace) / length * Width / 10 + 1, turnY(c.pos.y) + (1 - upSpace - downSpace) / height * Height * 4 / 5, (1 - leftSpace - rightSpace) / length * Width * 4 / 5 * ((float)c.hp / c.maxHp) - 2, 2);
                //绘制蓝条
                pag.FillRectangle(bMp, turnX(c.pos.x) + (1 - leftSpace - rightSpace) / length * Width / 10 + 1, turnY(c.pos.y) + (1 - upSpace - downSpace) / height * Height * 4 / 5 + 2, (1 - leftSpace - rightSpace) / length * Width * 4 / 5 * ((float)c.hp / c.maxHp) - 2, 2);
                //绘制时间条
                pag.FillRectangle(bTime, turnX(c.pos.x) + (1 - leftSpace - rightSpace) / length * Width / 10 + 1, turnY(c.pos.y) + (1 - upSpace - downSpace) / height * Height * 4 / 5 + 4, (1 - leftSpace - rightSpace) / length * Width * 4 / 5 * ((float)c.curTime / c.Spd) - 2, 2);
            }
        }

        public void rePaint(pos center, pos movrang)
        {
            Pen pLines = new Pen(Color.Blue, 1);            //画笔：网格线
            Brush bOwnChrc = new SolidBrush(Color.Red);     //画笔：己方单位
            Brush bEnmChrc = new SolidBrush(Color.Green);   //画笔：敌方单位
            Brush bMidChrc = new SolidBrush(Color.Black);   //画笔：中立单位
            Brush bRange = new SolidBrush(Color.LightPink); //画笔：标明射程
            Brush bMov = new SolidBrush(Color.LightBlue);   //画笔：标明移动
            //用于SHIFT菜单的射程显示
            //清理画布
            pag.Clear(Color.White);
            //绘制网格
            for (int i = 0; i <= length; i++)
                pag.DrawLine(pLines, (leftSpace + i * (1 - leftSpace - rightSpace) / length) * Width, upSpace * Height, (leftSpace + i * (1 - leftSpace - rightSpace) / length) * Width, (1 - downSpace) * Height);
            for (int i = 0; i <= height; i++)
                pag.DrawLine(pLines, leftSpace * Width, (upSpace + i * (1 - upSpace - downSpace) / height) * Height, (1 - rightSpace) * Width, (upSpace + i * (1 - upSpace - downSpace) / height) * Height);
            int min = movrang.y < movrang.x ? movrang.y : movrang.x;
            int max = movrang.y < movrang.x ? movrang.x : movrang.y;
            Brush bmin = movrang.y < movrang.x ? bRange : bMov;
            Brush bmax = movrang.y < movrang.x ? bMov : bRange;
            //显示射程，移动中大的
            for (int i = 1; i <= length; i++)
                for (int j = 1; j <= height; j++)
                    if (new pos(i, j) - center <= max)
                        pag.FillRectangle(bmax, turnX(i) + 1, turnY(j) + 1, (1 - leftSpace - rightSpace) / length * Width - 2, (1 - upSpace - downSpace) / height * Height - 2);
            //显示射程，移动中小的
            for (int i = 1; i <= length; i++)
                for (int j = 1; j <= height; j++)
                    if (new pos(i, j) - center <= min)
                        pag.FillRectangle(bmin, turnX(i) + 1, turnY(j) + 1, (1 - leftSpace - rightSpace) / length * Width - 2, (1 - upSpace - downSpace) / height * Height - 2);
            //绘制场上单位
            foreach (chct c in pad.Chcts)
                pag.DrawString(c.display, new Font("宋体", 25), c.where == 0 ? bMidChrc : (c.where == 1 ? bEnmChrc : bOwnChrc), turnX(c.pos.x) + 1, turnY(c.pos.y) + 1);
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            pos p0 = new pos(e.X, e.Y);
            pos p = turnBtS(p0);
            if (p != new pos(-1, -1)) 
            {
                if (!pad.isBattle)
                {
                    //如果在放人模式，则鼠标点击处添加一个单位
                    pad.addChct(p, e.Button, comboBox1.SelectedIndex);
                    //自动跳转到下一个角色
                    comboBox1.SelectedIndex = (comboBox1.SelectedIndex == 33 ? 33 : (comboBox1.SelectedIndex + 1));
                }
                else
                    pad.round(p);
                rePaint();
            }
            else if (pad.isBattle)
            {
                if (p0 > new pos((int)(leftSpell * Width), (int)(upSpell * Height))
                        && p0 < new pos((int)((leftSpell + spellWidth) * Width), (int)((upSpell + spellHeight) * Height)))
                {
                    //提示消息显示符卡描述

                }
                else if (p0 > new pos((int)(leftSpell * Width), (int)((upSpell + spellHeight + spellSpace) * Height))
                    && p0 < new pos((int)((leftSpell + spellWidth) * Width), (int)((upSpell + 2 * spellHeight + spellSpace) * Height)))
                {

                }
                else if (p0 > new pos((int)(leftSpell * Width), (int)((upSpell + 2 * spellHeight + 2 * spellSpace) * Height))
                    && p0 < new pos((int)((leftSpell + spellWidth) * Width), (int)((upSpell + 3 * spellHeight + 2 * spellSpace) * Height)))
                {

                }
            }
        }

        private int turnY(float y)
        {
            //从屏幕像素转化成网格
            if (y >= upSpace * Height && y <= (1 - downSpace) * Height)
                return 1 + (int)Math.Floor((y / Height - upSpace) * height / (1 - upSpace - downSpace));
            return -1;
        }

        private int turnX(float x)
        {
            //从屏幕像素转化成网格
            if (x >= leftSpace * Width && x <= (1 - rightSpace) * Width)
                return 1 + (int)Math.Floor((x / Width - leftSpace) * length / (1 - leftSpace - rightSpace));
            return -1;
        }

        private float turnY(int y)
        {
            //从网格转化成屏幕像素
            return ((y - 1.0f) * (1 - upSpace - downSpace) / height + upSpace) * Height;
        }

        private float turnX(int x)
        {
            //从网格转化成屏幕像素
            return ((x - 1.0f) * (1 - leftSpace - rightSpace) / length + leftSpace) * Width;
        }

        private pos turnStB(pos s)
        {
            //从网格转化成屏幕像素
            return new pos((int)Math.Floor(turnX(s.x)), (int)Math.Floor(turnY(s.y)));
        }

        private pos turnBtS(pos b)
        {
            //从屏幕像素转化成网格
            return new pos(turnX((float)b.x), turnY((float)b.y));
        }

        private void 加人模式ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pad.modesChange();
            加人模式ToolStripMenuItem.Text = pad.isBattle ? "战斗模式" : "加人模式";
            rePaint();
        }

        private void 清除所有单位ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pad.clearAll();
            rePaint();
        }

        private void 帮助ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(@"鼠标中键可以添加中立单位
按住shift微移鼠标显示单位信息
若已移动完毕，点击自身可跳过攻击阶段
若暴击则会beep
bugs:加人模式里可以把几个人放到同一个格子里，
而且可以移动到己方单位的格子里，但是100%会出错
如果同一时刻俩人都可以行动然后一个人死了，会出现那个人的幽灵，
此时不要乱攻击移动，都点击那个幽灵自己就不会出错");
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey)
                toolTip1.Active = false;
            rePaint();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            pos p0 = new pos(MousePosition.X - Left, MousePosition.Y - Top) + new pos(-8, -31);
            pos p = turnBtS(p0);
            if (!toolTip1.Active && e.KeyCode == Keys.ShiftKey)
            {
                if (p != new pos(-1, -1))
                {
                    //提示消息显示单位参数
                    toolTip1.SetToolTip(this, pad.show(p));
                    toolTip1.Active = pad.show(p) != "";
                    rePaint(p, pad.movRangAt(p));
                }
                else if(pad.isBattle)
                {
                    if (p0 > new pos((int)(leftSpell * Width), (int)(upSpell * Height))
                        && p0 < new pos((int)((leftSpell + spellWidth) * Width), (int)((upSpell + spellHeight) * Height)))
                    {
                        //提示消息显示符卡描述
                        toolTip1.SetToolTip(this, pad.scDisc[1]);
                        toolTip1.Active = true;
                    }
                    else if (p0 > new pos((int)(leftSpell * Width), (int)((upSpell + spellHeight + spellSpace) * Height))
                        && p0 < new pos((int)((leftSpell + spellWidth) * Width), (int)((upSpell + 2 * spellHeight + spellSpace) * Height)))
                    {
                        toolTip1.SetToolTip(this, pad.scDisc[2]);
                        toolTip1.Active = true;
                    }
                    else if (p0 > new pos((int)(leftSpell * Width), (int)((upSpell + 2 * spellHeight + 2 * spellSpace) * Height))
                        && p0 < new pos((int)((leftSpell + spellWidth) * Width), (int)((upSpell + 3 * spellHeight + 2 * spellSpace) * Height)))
                    {
                        toolTip1.SetToolTip(this, pad.scDisc[3]);
                        toolTip1.Active = true;
                    }
                }
            }
        }

        private void comboBox1_KeyUp(object sender, KeyEventArgs e)
        {
            Form1_KeyUp(sender, e);
        }

        private void comboBox1_KeyDown(object sender, KeyEventArgs e)
        {
            Form1_KeyDown(sender, e);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            pad.save();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            pad.load();
        }

        private void button1_KeyUp(object sender, KeyEventArgs e)
        {
            Form1_KeyUp(sender, e);
        }

        private void button1_KeyDown(object sender, KeyEventArgs e)
        {
            Form1_KeyDown(sender, e);
        }

        private void button2_KeyDown(object sender, KeyEventArgs e)
        {
            Form1_KeyDown(sender, e);
        }

        private void button2_KeyUp(object sender, KeyEventArgs e)
        {
            Form1_KeyUp(sender, e);
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {

            if (pad.isBattle)
            {
                pos p = turnBtS(new pos(e.X, e.Y));
                if (p != new pos(-1, -1) && pad.canEnm.Contains(p))
                {
                    if (mouseMoving)
                    {
                        toolTip2.SetToolTip(this, pad.tip(p));
                        toolTip2.Active = pad.tip(p) != "";
                        mouseMoving = false;
                    }
                    else
                        mouseMoving = true;
                }
                else
                    toolTip2.Active = false;
            }
        }
    }

    public struct pos
    {
        //坐标结构，可以储存像素坐标或者网格坐标
        public int x;
        public int y;

        public pos(int x,int y)
        {
            this.x = x;
            this.y = y;
        }

        public static int operator -(pos posa, pos posb)
        {
            //-运算返回两单位之间的距离。
            return Math.Abs(posa.x - posb.x) + Math.Abs(posa.y - posb.y);
        }

        public static pos operator +(pos posa, pos posb)
        {
            //+运算返回两坐标值的叠加
            return new pos(posa.x + posb.x, posa.y + posb.y);
        }

        public override string ToString()
        {
            return "x = " + x + ", y = " + y;
        }

        public static bool operator <(pos posa,pos posb)
        {
            //x与y均小于才算小于
            return posa.x < posb.x && posa.y < posb.y;
        }

        public static bool operator >(pos posa, pos posb)
        {
            //x与y均大于才算大于
            return posa.x > posb.x && posa.y > posb.y;
        }

        public static bool operator !=(pos posa,pos posb)
        {
            //x与y均不等才算不等
            return posa.x != posb.x && posa.y != posb.y;
        }

        public static bool operator ==(pos posa, pos posb)
        {
            //x与y均相等才算相等
            return posa.x == posb.x && posa.y == posb.y;
        }
    }
    
    public enum charc
    {
        Reimu,
        Marisa,
        Rin,
        Rumia,
        Daiyousei,
        Cirno,
        Meirin,
        Koakuma,
        Patchouli,
        Sakuya,
        Reimiria,
        Flandre,
        Letty,
        Chen,
        Alice,
        LilyWhite,
        Lunasa,
        Merlin,
        Lyrica,
        Leira,
        Youmu,
        Yuyuko,
        Ran,
        Yukari,
        Suika,
        Wriggle,
        Mystia,
        Keine,
        Tewi,
        Reisen,
        Eirin,
        Kaguya,
        Mokou,
        Gedama
    }

    public class cal
    {
        public static double hitRate(int delta, int d)
        {
            //命中率公式
            double p = 1.0 / (1 + Math.Pow(0.93, delta));
            return (p > 0.95 ? 0.95 : (p < 0.05 ? 0.05 : p)) * (1.0f - 0.05f * d);
        }

        public static int damage(int att, int def)
        {
            //伤害公式
            return att * att / (att + def);
        }

        public static List<chctdata> chctdataList = new List<chctdata>();     //储存所有角色的原始数据
    }

    public class pad
    {
        private const int length = 9;                           //棋盘长度
        private const int height = 9;                           //棋盘宽度
        public bool isBattle = false;                           //false为放人，true为战斗
        private List<chct> chcts = new List<chct>();            //储存场上所有单位
        public List<chct> Chcts { get { return chcts; } }
        private List<chct> nextMoves                            //返回该谁行动了
        { get { return new List<chct>(from c in chcts
                                      where c.curTime == 0 && !c.isRounded
                                      orderby c.Spd descending
                                      select c); } }
        public List<pos> canEnm                                 //返回所有能打到的敌方单位
        { get { return new List<pos>(from c in chcts
                                     where c.pos - hang.pos <= hang.Rang && c.@where != hang.@where
                                     select c.pos); } }
        public bool[,] canReach = new bool[length, height];    //储存所有能到达的空格子
        private List<pos> Enm                                   //返回敌人的点单，计算所有能到达的空格子中使用的中间变量
        { get { return new List<pos>(from c in chcts
                                     where ((hang.@where != 0 && c.@where == -hang.@where) || (hang.@where == 0 && c.@where != hang.@where)) && hang.OnMovingIgnoreEnm
                                     select c.pos + new pos(-1, -1)); } }
        public bool IsMoved { get { return hang.IsMoved; } }
        public bool IsAttacked { get { return hang.IsAttacked; } }
        private chct hang = null;                               //目前行动着的单位
        private List<chct> chctsSaved = new List<chct>();       //保存场上单位
        [DllImport("user32.dll")]
        public static extern bool MessageBeep(uint uType);
        public pos hangPos { get { return hang.pos; } }
        public string[] scName
        { get { return (from c in cal.chctdataList
                        where c.who == hang.who
                        select c).ElementAt(0).scName; } }
        public string[] scDisc
        { get { return (from c in cal.chctdataList
                        where c.who == hang.who
                        select c).ElementAt(0).scDisc; } }
        private List<chct> scTargets = new List<chct>();        //选取sc的目标的中间变量
        private int scTargetsPara;                              //选取sc的目标的中间变量

        public List<pos> CanReachPos
        { get
            {
                canReachGen();
                List<pos> l = new List<pos>();
                for (int i = 0; i < length; i++)
                    for (int j = 0; j < height; j++)
                        if (canReach[i, j]) l.Add(new pos(i + 1, j + 1));
                return l;
            } }

        public string show(pos p)
        {
            var h = chcts.Find(c => c.pos == p);
            if (h != null) return h.ToString(); else return "";
        }

        public string tip(pos p)
        {
            var h = chcts.Find(c => c.pos == p);
            if (h != null) return h.tip(hang); else return "";
        }

        public void addChct(pos p, MouseButtons e, int index)
        {
            //鼠标点击处添加一个单位
            switch (index)
            {
                case 0:
                    chcts.Add(new reimu(chcts.Count, p, e != MouseButtons.Left ? (e != MouseButtons.Right ? 0 : 1) : -1, this));
                    break;
                case 1:
                    chcts.Add(new marisa(chcts.Count, p, e != MouseButtons.Left ? (e != MouseButtons.Right ? 0 : 1) : -1, this));
                    break;
                case 2:
                    chcts.Add(new rin(chcts.Count, p, e != MouseButtons.Left ? (e != MouseButtons.Right ? 0 : 1) : -1, this));
                    break;
                case 3:
                    chcts.Add(new rumia(chcts.Count, p, e != MouseButtons.Left ? (e != MouseButtons.Right ? 0 : 1) : -1, this));
                    break;
                case 4:
                    chcts.Add(new daiyousei(chcts.Count, p, e != MouseButtons.Left ? (e != MouseButtons.Right ? 0 : 1) : -1, this));
                    break;
                case 5:
                    chcts.Add(new cirno(chcts.Count, p, e != MouseButtons.Left ? (e != MouseButtons.Right ? 0 : 1) : -1, this));
                    break;
                case 6:
                    chcts.Add(new meirin(chcts.Count, p, e != MouseButtons.Left ? (e != MouseButtons.Right ? 0 : 1) : -1, this));
                    break;
                case 7:
                    chcts.Add(new koakuma(chcts.Count, p, e != MouseButtons.Left ? (e != MouseButtons.Right ? 0 : 1) : -1, this));
                    break;
                case 8:
                    chcts.Add(new patchouli(chcts.Count, p, e != MouseButtons.Left ? (e != MouseButtons.Right ? 0 : 1) : -1, this));
                    break;
                case 9:
                    chcts.Add(new sakuya(chcts.Count, p, e != MouseButtons.Left ? (e != MouseButtons.Right ? 0 : 1) : -1, this));
                    break;
                case 10:
                    chcts.Add(new reimiria(chcts.Count, p, e != MouseButtons.Left ? (e != MouseButtons.Right ? 0 : 1) : -1, this));
                    break;
                case 11:
                    chcts.Add(new flandre(chcts.Count, p, e != MouseButtons.Left ? (e != MouseButtons.Right ? 0 : 1) : -1, this));
                    break;
                case 12:
                    chcts.Add(new letty(chcts.Count, p, e != MouseButtons.Left ? (e != MouseButtons.Right ? 0 : 1) : -1, this));
                    break;
                case 13:
                    chcts.Add(new chen(chcts.Count, p, e != MouseButtons.Left ? (e != MouseButtons.Right ? 0 : 1) : -1, this));
                    break;
                case 14:
                    chcts.Add(new alice(chcts.Count, p, e != MouseButtons.Left ? (e != MouseButtons.Right ? 0 : 1) : -1, this));
                    break;
                case 15:
                    chcts.Add(new lilywhite(chcts.Count, p, e != MouseButtons.Left ? (e != MouseButtons.Right ? 0 : 1) : -1, this));
                    break;
                case 16:
                    chcts.Add(new lunasa(chcts.Count, p, e != MouseButtons.Left ? (e != MouseButtons.Right ? 0 : 1) : -1, this));
                    break;
                case 17:
                    chcts.Add(new merlin(chcts.Count, p, e != MouseButtons.Left ? (e != MouseButtons.Right ? 0 : 1) : -1, this));
                    break;
                case 18:
                    chcts.Add(new lyrica(chcts.Count, p, e != MouseButtons.Left ? (e != MouseButtons.Right ? 0 : 1) : -1, this));
                    break;
                case 19:
                    chcts.Add(new leira(chcts.Count, p, e != MouseButtons.Left ? (e != MouseButtons.Right ? 0 : 1) : -1, this));
                    break;
                case 20:
                    chcts.Add(new youmu(chcts.Count, p, e != MouseButtons.Left ? (e != MouseButtons.Right ? 0 : 1) : -1, this));
                    break;
                case 21:
                    chcts.Add(new yuyuko(chcts.Count, p, e != MouseButtons.Left ? (e != MouseButtons.Right ? 0 : 1) : -1, this));
                    break;
                case 22:
                    chcts.Add(new ran(chcts.Count, p, e != MouseButtons.Left ? (e != MouseButtons.Right ? 0 : 1) : -1, this));
                    break;
                case 23:
                    chcts.Add(new yukari(chcts.Count, p, e != MouseButtons.Left ? (e != MouseButtons.Right ? 0 : 1) : -1, this));
                    break;
                case 24:
                    chcts.Add(new suika(chcts.Count, p, e != MouseButtons.Left ? (e != MouseButtons.Right ? 0 : 1) : -1, this));
                    break;
                case 25:
                    chcts.Add(new wriggle(chcts.Count, p, e != MouseButtons.Left ? (e != MouseButtons.Right ? 0 : 1) : -1, this));
                    break;
                case 26:
                    chcts.Add(new mystia(chcts.Count, p, e != MouseButtons.Left ? (e != MouseButtons.Right ? 0 : 1) : -1, this));
                    break;
                case 27:
                    chcts.Add(new keine(chcts.Count, p, e != MouseButtons.Left ? (e != MouseButtons.Right ? 0 : 1) : -1, this));
                    break;
                case 28:
                    chcts.Add(new tewi(chcts.Count, p, e != MouseButtons.Left ? (e != MouseButtons.Right ? 0 : 1) : -1, this));
                    break;
                case 29:
                    chcts.Add(new reisen(chcts.Count, p, e != MouseButtons.Left ? (e != MouseButtons.Right ? 0 : 1) : -1, this));
                    break;
                case 30:
                    chcts.Add(new eirin(chcts.Count, p, e != MouseButtons.Left ? (e != MouseButtons.Right ? 0 : 1) : -1, this));
                    break;
                case 31:
                    chcts.Add(new kaguya(chcts.Count, p, e != MouseButtons.Left ? (e != MouseButtons.Right ? 0 : 1) : -1, this));
                    break;
                case 32:
                    chcts.Add(new mokou(chcts.Count, p, e != MouseButtons.Left ? (e != MouseButtons.Right ? 0 : 1) : -1, this));
                    break;
                case 33:
                    chcts.Add(new gedama(chcts.Count, p, e != MouseButtons.Left ? (e != MouseButtons.Right ? 0 : 1) : -1, this));
                    break;
            }
        }

        public void round(pos p)
        {
            hang.round(p);
        }

        public void canReachGen()
        {
            //生成所有能到达的空格子
            for (int i = 0; i < length; i++)
                for (int j = 0; j < height; j++)
                    canReach[i, j] = false;
            canReachPoint(hang.pos + new pos(-1, -1), hang.Mov);
            //排除场上单位
            foreach (pos ps in new List<pos>(from c in chcts where !(c.pos == hang.pos) select c.pos))
                canReach[ps.x - 1, ps.y - 1] = false;
        }


        public void canReachPoint(pos center, int mov)
        {
            //返回所有能到达的空格子
            if (mov == 0)
                canReach[center.x, center.y] = true;
            else
            {
                canReach[center.x, center.y] = true;
                if (center.y < height - 1 && !Enm.Contains(center + new pos(0, 1))) canReachPoint(center + new pos(0, 1), mov - 1);
                if (center.y > 0 && !Enm.Contains(center + new pos(0, -1))) canReachPoint(center + new pos(0, -1), mov - 1);
                if (center.x < length - 1 && !Enm.Contains(center + new pos(1, 0))) canReachPoint(center + new pos(1, 0), mov - 1);
                if (center.x > 0 && !Enm.Contains(center + new pos(-1, 0))) canReachPoint(center + new pos(-1, 0), mov - 1);
            }
        }

        public void modesChange()
        {
            isBattle = !isBattle;
            if (isBattle)
                if (chcts.Count() != 0)
                    nextRound();
                else
                    isBattle = false;
            else
            {
                hang = null;
                foreach (chct c in chcts)
                    c.reset();
            }
        }

        public void clearAll()
        {
            chcts.Clear();
        }

        public void nextRound()
        {
            //行动
            if (nextMoves.Count() == 0)
                //返回所有接下来该行动的单位
                do
                {
                    foreach (chct c in chcts)
                        c.time();
                } while (nextMoves.Count() == 0);
            //获取所有time值0的角色按spd从大到小排序，拿出下一个要行动的单位
            hang = nextMoves.ElementAt(0);
            hang.rounded();
        }

        public List<chct> ChoosingScTargets(int a, List<int> parameters)
        {
            //1,2,4,8位为单位，1位为敌方单位，2位为己方单位
            //16*n为选择方式
            //n==1为某范围内单体，n==2为某范围内一个点周围一定范围内所有目标
            scTargets = new List<chct>();
            scTargetsPara = a;

            return scTargets;
        }

        public List<chct> NoChoosingScTargets(int a, List<int> parameters)
        {
            //1,2,4,8位为单位，1位为敌方单位，2位为己方单位
            //16*n为选择方式
            //n==1为自身周围一定范围内所有目标
            scTargets = new List<chct>();
            scTargetsPara = a;

            return scTargets;
        }

        public void save()
        {
            chctsSaved = new List<chct>(chcts);
        }

        public void load()
        {
            chcts = new List<chct>(chctsSaved);
        }

        public pos movRangAt(pos p)
        {
            var a = from c in chcts
                     where c.pos == p
                     select c;
            if (a.Count() != 0)
                return new pos(a.ElementAt(0).Mov, a.ElementAt(0).Rang);
            else
                return new pos(0, 0);
        }
    }

    public class chctdata
    {
        public charc who;       //角色是谁
        public string display;  //屏幕上显示什么文字
        public int maxHp;       //最大血量
        public int att;         //攻击   
        public int def;         //防御
        public int hit;         //命中
        public int dod;         //闪避
        public float clo;       //近战补正
        public int spd;         //行动间隔
        public int mov;         //机动
        public int rang;        //普攻范围
        public string[] scName = { "", "", "", "" };    //符卡名
        public string[] scDisc = { "", "", "", "" };    //符卡描述
    }

    public class chct
    {
        public readonly int id;                           //是第几个被放入的单位
        public readonly int maxHp;                        //最大血量
        public readonly int maxMp;                        //最大灵力
        public string display { get; protected set; }     //屏幕上显示什么文字
        public int hp { get; protected set; }             //血量
        public int mp { get; protected set; }             //灵力
        protected readonly int att;                         //攻击
        private float attx = 1.0f;
        public int Att { get { return (int)Math.Floor(attx * att); } }
        protected readonly int def;                         //防御
        private float defx = 1.0f;
        public int Def { get { return (int)Math.Floor(defx * def); } }
        protected readonly int hit;                         //命中
        private float hitx = 1.0f;
        public int Hit { get { return (int)Math.Floor(hitx * hit); } }
        protected readonly int dod;                         //闪避
        private float dodx = 1.0f;
        public int Dod { get { return (int)Math.Floor(dodx * dod); } }
        protected readonly float clo;                       //近战补正
        private float clox = 1.0f;
        public float Clo { get { return (int)Math.Floor(clox * clo); } }
        protected readonly int spd;                         //行动间隔
        private float spdx = 1.0f;
        public int Spd { get { return (int)Math.Floor(spdx * spd); } }
        protected readonly int mov;                         //机动
        private int movx = 0;
        public int Mov { get { return mov + movx; } }
        protected readonly int rang;                        //普攻范围
        private int rangx = 0;
        public int Rang { get { return rang + rangx; } }
        public int curTime { get; protected set; }        //行动间隔到多少了
        public pos pos { get; protected set; }            //所处坐标
        public int where { get; protected set; }          //阵营，0中立，1己方，-1敌方
        public bool isRounded { get; protected set; }     //是否行动过了
        public charc who { get; }                         //返回是谁
        private bool isMoved = false;                     //是否已经移动
        public bool IsMoved { get { return isMoved; } }
        private bool isAttacked = false;                  //是否已经攻击
        public bool IsAttacked { get { return isAttacked; } }
        public List<buff> buffList { get; protected set; }
        public pad pad;

        public chct(charc whoc, int id, pos pos, int where, pad pad)
        {
            this.id = id;
            this.pos = pos;
            this.where = where;
            isRounded = false;
            this.pad = pad;
            chctdata who = (from c in cal.chctdataList
                            where c.who == whoc
                            select c).ElementAt(0);
            maxHp = who.maxHp;
            hp = maxHp;
            maxMp = 1000;
            mp = maxMp;
            display = who.display;
            att = who.att;
            def = who.def;
            hit = who.hit;
            dod = who.dod;
            clo = who.clo;
            spd = who.spd;
            mov = who.mov;
            rang = who.rang;
            curTime = spd;
        }

        public bool attack(chct attacker)
        {
            //普通攻击，包括伤害浮动，返回是否暴击
            Random r = new Random();
            bool b = false;
            //BeforeBeingAttacked(attacker);
            //attacker.BeforeAttacking(this);
            float clo;
            //bool c = false;
            if (attacker.pos - pos == 1)
            {
                clo = Clo;
                //c = true;
                //BeforeBeingCloseAttacked(attacker);
                //attacker.BeforeCloseAttacking(this);
            }
            else
            {
                clo = 1.0f;
                //BeforeBeingDanmakuAttacked(attacker);
                //attacker.BeforeDanmakuAttacking(this);
            }
            if (r.NextDouble() <= cal.hitRate(attacker.Hit - Dod, attacker.pos - pos))
            {
                //伤害浮动&暴击
                b = r.NextDouble() < CriticalHitRate;
                //BeforeBeingHit(attacker);
                //attacker.BeforeHitting(this);
                if (b)
                {
                    //BeforeBeingCriticalHit(attacker);
                    //attacker.BeforeCriticalHitting(this);
                    damage(attacker, (int)Math.Floor(cal.damage(attacker.Att, Def) * CriticalHitDamage * (1 + (r.NextDouble() - 0.5) * 2 * DamageFloat)));
                    //OnBeingCriticalHit(attacker);
                    //attacker.OnCriticalHitting(this);
                }
                else
                    damage(attacker, (int)Math.Floor(cal.damage(attacker.Att, Def) * (1 + (r.NextDouble() - 0.5) * 2 * DamageFloat)));
                //OnBeingHit(attacker);
                //attacker.OnHitting(this);
            }
            //else
            //{
                //attacker.OnBeingDodged(this);
                //OnDodging(attacker);
            //}
            //if (c)
            //{
                //OnBeingCloseAttacked(attacker);
                //attacker.OnCloseAttacking(this);
            //}
            //else
            //{
                //OnBeingDanmakuAttacked(attacker);
                //attacker.OnDanmakuAttacking(this);
            //}
            //OnBeingAttacked(attacker);
            //attacker.OnAttacking(this);
            return b;
        }

        public void ScAttack(chct attacker, float times, bool isFloating)
        {
            //BeforeBeingScAttacked(attacker);
            //attacker.BeforeScAttacking(this);
            Random r = new Random();
            damage(attacker, (int)Math.Floor(cal.damage(attacker.Att, Def) * (isFloating ? (1 + (r.NextDouble() - 0.5) * 2 * DamageFloat) : 1)));
            //OnBeingScAttacked(attacker);
            //attacker.OnScAttacking(this);
        }

        public void damage(chct attacker, int damage)
        {
            //BeforeBeingDamaged(attacker);
            //attacker.BeforeDamaging(this);
            hp -= damage;
            //OnBeingDamaged(attacker);
            //attacker.OnDamaging(this);
        }

        public void move(pos newPos)
        {
            //移动
            //BeforeMoving();
            pos = newPos;
            //OnMoving();
        }

        public void rounded()
        {
            isRounded = true;
        }

        public void time()
        {
            curTime = (curTime == 0) ? (Spd - 1) : (curTime - 1);
            isRounded = false;
            isMoved = false;
            isAttacked = false;
        }

        public void round(pos p)
        {
            //如果只能攻击并且点了自己，则算做攻击过
            if (!isAttacked && isMoved && p == pos) isAttacked = true;
            //如果点击的格子在敌人列表里，则攻击
            if (!isAttacked && pad.canEnm.Contains(p))
            {
                if (new List<chct>(from c in pad.Chcts
                                   where c.pos == p
                                   select c).ElementAt(0).attack(this))
                    //若暴击，则beep
                    pad.MessageBeep(48);
                isAttacked = true;
            }
            //如果点击的格子在空格子列表里，则移动
            if (!isMoved && pad.canReach[p.x - 1, p.y - 1])
            {
                move(new pos(p.x, p.y));
                isMoved = true;
            }
            //移除死亡单位
            pad.Chcts.RemoveAll(c => c.hp <= 0);
            //重新生成可以到达的空格子
            pad.canReachGen();
            if (isMoved && isAttacked)
            {
                isMoved = false;
                isAttacked = false;
                pad.nextRound();
            }
        }

        public override string ToString()
        {
            return Convert.ToString(hp) + " / " + Convert.ToString(maxHp) + "\natt: " + Convert.ToString(Att)
                + "\ndef: " + Convert.ToString(Def) + "\nhit: " + Convert.ToString(Hit) + "\ndod: " + Convert.ToString(Dod)
                + "\nclo: " + Convert.ToString(Clo) + ((clo % 1 == 0) ? ".0" : "") + "\nspd: " + Convert.ToString(Spd) + "\nmov: " + Convert.ToString(Mov)
                + "\nrang: " + Convert.ToString(rang) + "\ncurruentTime: " + Convert.ToString(curTime);
        }

        public string tip(chct attacker)
        {
            return "Rate: " + Convert.ToString(Math.Floor(cal.hitRate(attacker.Hit - Dod, attacker.pos - pos) * 1000) / 10)
                + "%\nDamage: " + Convert.ToString(Math.Floor((decimal)cal.damage(attacker.Att, Def)));
        }

        public void reset()
        {
            curTime = Spd;
            hp = maxHp;
            isRounded = false;
        }

        //一些静态的buff方法
        public static void buffBleed(chct buffee, chct buffer, int damage)
        {
            //流血。造成一定量的真实伤害。
            buffer.damage(buffer, damage);
        }

        //符卡使用
        public void SC01() { }
        public void SC02() { }
        public void SC03() { }
        //灵梦的移动无视单位体积碰撞接口
        public bool OnMovingIgnoreEnm { get { return false; } }
        //一堆平a接口
        //private void OnBeingAttacked(chct attacker) { }
        //private void OnAttacking(chct defenser) { }
        //private void BeforeBeingAttacked(chct attacker) { }
        //private void BeforeAttacking(chct defenser) { }
        //private void OnBeingHit(chct attacker) { }
        //private void OnHitting(chct defenser) { }
        //private void BeforeBeingHit(chct attacker) { }
        //private void BeforeHitting(chct defenser) { }
        //private void OnBeingDodged(chct defenser) { }
        //private void OnDodging(chct attacker) { }
        //private void OnBeingCriticalHit(chct attacker) { }
        //private void OnCriticalHitting(chct defenser) { }
        //private void BeforeBeingCriticalHit(chct attacker) { }
        //private void BeforeCriticalHitting(chct defenser) { }
        //private void OnBeingDanmakuAttacked(chct attacker) { }
        //private void OnDanmakuAttacking(chct defenser) { }
        //private void BeforeBeingDanmakuAttacked(chct attacker) { }
        //private void BeforeDanmakuAttacking(chct defenser) { }
        //private void OnBeingCloseAttacked(chct attacker) { }
        //private void OnCloseAttacking(chct defenser) { }
        //private void BeforeBeingCloseAttacked(chct attacker) { }
        //private void BeforeCloseAttacking(chct defenser) { }
        //平a相关数值
        private float CriticalHitDamage { get { return 1.5f; } }
        private float CriticalHitRate { get { return 0.2f; } }
        private float DamageFloat { get { return 0.1f; } }
        //移动接口
        //private void OnMoving() { }
        //private void BeforeMoving() { }
        //陷阱等需添加陷阱单位，无法在这里直接声明
        //伤害接口
        //private void OnBeingDamaged(chct attacker) { }
        //private void OnDamaging(chct defenser) { }
        //private void BeforeBeingDamaged(chct attacker) { }
        //private void BeforeDamaging(chct defenser) { }
        //符卡伤害接口
        //private void OnBeingScAttacked(chct attacker) { }
        //private void OnScAttacking(chct defenser) { }
        //private void BeforeBeingScAttacked(chct attacker) { }
        //private void BeforeScAttacking(chct defenser) { }
        //buff类
    }

    public class reimu : chct
    {
        public static new charc who { get { return charc.Reimu; } }

        public reimu(int id, pos pos, int where, pad pad) : base(who, id, pos, where, pad)
        {
            
        }

        //天赋，移动无视单位体积碰撞
        public new bool OnMovingIgnoreEnm { get { return true; } }
        //天赋，灵力回收增加20%

        //符卡01
        public new void SC01()
        {

        }

        //符卡02
        public new void SC02()
        {

        }

        //符卡03
        public new void SC03()
        {

        }
    }

    public class marisa : chct
    {
        public static new charc who { get { return charc.Marisa; } }

        public marisa(int id, pos pos, int where, pad pad) : base(who, id, pos, where, pad) { }

        //天赋

        //符卡01
        public new void SC01()
        {

        }

        //符卡02
        public new void SC02()
        {

        }

        //符卡03
        public new void SC03()
        {

        }
    }

    public class rin : chct
    {
        public static new charc who { get { return charc.Rin; } }

        public rin(int id, pos pos, int where, pad pad) : base(who, id, pos, where, pad) { }

        //天赋

        //符卡01
        public new void SC01()
        {

        }

        //符卡02
        public new void SC02()
        {

        }

        //符卡03
        public new void SC03()
        {

        }
    }

    public class rumia : chct
    {
        public static new charc who { get { return charc.Rumia; } }

        public rumia(int id, pos pos, int where, pad pad) : base(who, id, pos, where, pad) { }

        //天赋

        //符卡01
        public new void SC01()
        {

        }

        //符卡02
        public new void SC02()
        {

        }

        //符卡03
        public new void SC03()
        {

        }
    }

    public class daiyousei : chct
    {
        public static new charc who { get { return charc.Daiyousei; } }

        public daiyousei(int id, pos pos, int where, pad pad) : base(who, id, pos, where, pad) { }

        //天赋

        //符卡01
        public new void SC01()
        {

        }

        //符卡02
        public new void SC02()
        {

        }

        //符卡03
        public new void SC03()
        {

        }
    }

    public class cirno : chct
    {
        public static new charc who { get { return charc.Cirno; } }

        public cirno(int id, pos pos, int where, pad pad) : base(who, id, pos, where, pad) { }

        //天赋

        //符卡01
        public new void SC01()
        {

        }

        //符卡02
        public new void SC02()
        {

        }

        //符卡03
        public new void SC03()
        {

        }
    }

    public class meirin : chct
    {
        public static new charc who { get { return charc.Meirin; } }

        public meirin(int id, pos pos, int where, pad pad) : base(who, id, pos, where, pad) { }

        //天赋

        //符卡01
        public new void SC01()
        {

        }

        //符卡02
        public new void SC02()
        {

        }

        //符卡03
        public new void SC03()
        {

        }
    }

    public class koakuma : chct
    {
        public static new charc who { get { return charc.Koakuma; } }

        public koakuma(int id, pos pos, int where, pad pad) : base(who, id, pos, where, pad) { }

        //天赋

        //符卡01
        public new void SC01()
        {

        }

        //符卡02
        public new void SC02()
        {

        }

        //符卡03
        public new void SC03()
        {

        }
    }

    public class patchouli : chct
    {
        public static new charc who { get { return charc.Patchouli; } }

        public patchouli(int id, pos pos, int where, pad pad) : base(who, id, pos, where, pad) { }

        //天赋

        //符卡01
        public new void SC01()
        {

        }

        //符卡02
        public new void SC02()
        {

        }

        //符卡03
        public new void SC03()
        {

        }
    }

    public class sakuya : chct
    {
        public static new charc who { get { return charc.Sakuya; } }

        public sakuya(int id, pos pos, int where, pad pad) : base(who, id, pos, where, pad) { }

        //天赋

        //符卡01
        public new void SC01()
        {

        }

        //符卡02
        public new void SC02()
        {

        }

        //符卡03
        public new void SC03()
        {

        }
    }

    public class reimiria : chct
    {
        public static new charc who { get { return charc.Reimiria; } }

        public reimiria(int id, pos pos, int where, pad pad) : base(who, id, pos, where, pad) { }

        //天赋

        //符卡01
        public new void SC01()
        {

        }

        //符卡02
        public new void SC02()
        {

        }

        //符卡03
        public new void SC03()
        {

        }
    }

    public class flandre : chct
    {
        public static new charc who { get { return charc.Flandre; } }

        public flandre(int id, pos pos, int where, pad pad) : base(who, id, pos, where, pad) { }

        //天赋

        //符卡01
        public new void SC01()
        {

        }

        //符卡02
        public new void SC02()
        {

        }

        //符卡03
        public new void SC03()
        {

        }
    }

    public class letty : chct
    {
        public static new charc who { get { return charc.Letty; } }

        public letty(int id, pos pos, int where, pad pad) : base(who, id, pos, where, pad) { }

        //天赋

        //符卡01
        public new void SC01()
        {

        }

        //符卡02
        public new void SC02()
        {

        }

        //符卡03
        public new void SC03()
        {

        }
    }

    public class chen : chct
    {
        public static new charc who { get { return charc.Chen; } }

        public chen(int id, pos pos, int where, pad pad) : base(who, id, pos, where, pad) { }

        //天赋

        //符卡01
        public new void SC01()
        {

        }

        //符卡02
        public new void SC02()
        {

        }

        //符卡03
        public new void SC03()
        {

        }
    }

    public class alice : chct
    {
        public static new charc who { get { return charc.Alice; } }

        public alice(int id, pos pos, int where, pad pad) : base(who, id, pos, where, pad) { }

        //天赋

        //符卡01
        public new void SC01()
        {

        }

        //符卡02
        public new void SC02()
        {

        }

        //符卡03
        public new void SC03()
        {

        }
    }

    public class lilywhite : chct
    {
        public static new charc who { get { return charc.LilyWhite; } }

        public lilywhite(int id, pos pos, int where, pad pad) : base(who, id, pos, where, pad) { }

        //天赋

        //符卡01
        public new void SC01()
        {

        }

        //符卡02
        public new void SC02()
        {

        }

        //符卡03
        public new void SC03()
        {

        }
    }

    public class lunasa : chct
    {
        public static new charc who { get { return charc.Lunasa; } }

        public lunasa(int id, pos pos, int where, pad pad) : base(who, id, pos, where, pad) { }

        //天赋

        //符卡01
        public new void SC01()
        {

        }

        //符卡02
        public new void SC02()
        {

        }

        //符卡03
        public new void SC03()
        {

        }
    }

    public class merlin : chct
    {
        public static new charc who { get { return charc.Merlin; } }

        public merlin(int id, pos pos, int where, pad pad) : base(who, id, pos, where, pad) { }

        //天赋

        //符卡01
        public new void SC01()
        {

        }

        //符卡02
        public new void SC02()
        {

        }

        //符卡03
        public new void SC03()
        {

        }
    }

    public class lyrica : chct
    {
        public static new charc who { get { return charc.Lyrica; } }

        public lyrica(int id, pos pos, int where, pad pad) : base(who, id, pos, where, pad) { }

        //天赋

        //符卡01
        public new void SC01()
        {

        }

        //符卡02
        public new void SC02()
        {

        }

        //符卡03
        public new void SC03()
        {

        }
    }

    public class leira : chct
    {
        public static new charc who { get { return charc.Leira; } }

        public leira(int id, pos pos, int where, pad pad) : base(who, id, pos, where, pad) { }

        //天赋

        //符卡01
        public new void SC01()
        {

        }

        //符卡02
        public new void SC02()
        {

        }

        //符卡03
        public new void SC03()
        {

        }
    }

    public class youmu : chct
    {
        public static new charc who { get { return charc.Youmu; } }

        public youmu(int id, pos pos, int where, pad pad) : base(who, id, pos, where, pad) { }

        //天赋

        //符卡01
        public new void SC01()
        {

        }

        //符卡02
        public new void SC02()
        {

        }

        //符卡03
        public new void SC03()
        {

        }
    }

    public class yuyuko : chct
    {
        public static new charc who { get { return charc.Yuyuko; } }

        public yuyuko(int id, pos pos, int where, pad pad) : base(who, id, pos, where, pad) { }

        //天赋

        //符卡01
        public new void SC01()
        {

        }

        //符卡02
        public new void SC02()
        {

        }

        //符卡03
        public new void SC03()
        {

        }
    }

    public class ran : chct
    {
        public static new charc who { get { return charc.Ran; } }

        public ran(int id, pos pos, int where, pad pad) : base(who, id, pos, where, pad) { }

        //天赋

        //符卡01
        public new void SC01()
        {

        }

        //符卡02
        public new void SC02()
        {

        }

        //符卡03
        public new void SC03()
        {

        }
    }

    public class yukari : chct
    {
        public static new charc who { get { return charc.Yukari; } }

        public yukari(int id, pos pos, int where, pad pad) : base(who, id, pos, where, pad) { }

        //天赋

        //符卡01
        public new void SC01()
        {

        }

        //符卡02
        public new void SC02()
        {

        }

        //符卡03
        public new void SC03()
        {

        }
    }

    public class suika : chct
    {
        public static new charc who { get { return charc.Suika; } }

        public suika(int id, pos pos, int where, pad pad) : base(who, id, pos, where, pad) { }

        //天赋

        //符卡01
        public new void SC01()
        {

        }

        //符卡02
        public new void SC02()
        {

        }

        //符卡03
        public new void SC03()
        {

        }
    }

    public class wriggle : chct
    {
        public static new charc who { get { return charc.Wriggle; } }

        public wriggle(int id, pos pos, int where, pad pad) : base(who, id, pos, where, pad) { }

        //天赋

        //符卡01
        public new void SC01()
        {

        }

        //符卡02
        public new void SC02()
        {

        }

        //符卡03
        public new void SC03()
        {

        }
    }

    public class mystia : chct
    {
        public static new charc who { get { return charc.Mystia; } }

        public mystia(int id, pos pos, int where, pad pad) : base(who, id, pos, where, pad) { }

        //天赋

        //符卡01
        public new void SC01()
        {

        }

        //符卡02
        public new void SC02()
        {

        }

        //符卡03
        public new void SC03()
        {

        }
    }

    public class keine : chct
    {
        public static new charc who { get { return charc.Keine; } }

        public keine(int id, pos pos, int where, pad pad) : base(who, id, pos, where, pad) { }

        //天赋

        //符卡01
        public new void SC01()
        {

        }

        //符卡02
        public new void SC02()
        {

        }

        //符卡03
        public new void SC03()
        {

        }
    }

    public class tewi : chct
    {
        public static new charc who { get { return charc.Tewi; } }

        public tewi(int id, pos pos, int where, pad pad) : base(who, id, pos, where, pad) { }

        //天赋

        //符卡01
        public new void SC01()
        {

        }

        //符卡02
        public new void SC02()
        {

        }

        //符卡03
        public new void SC03()
        {

        }
    }

    public class reisen : chct
    {
        public static new charc who { get { return charc.Reisen; } }

        public reisen(int id, pos pos, int where, pad pad) : base(who, id, pos, where, pad) { }

        //天赋

        //符卡01
        public new void SC01()
        {

        }

        //符卡02
        public new void SC02()
        {

        }

        //符卡03
        public new void SC03()
        {

        }
    }

    public class eirin : chct
    {
        public static new charc who { get { return charc.Eirin; } }

        public eirin(int id, pos pos, int where, pad pad) : base(who, id, pos, where, pad) { }

        //天赋

        //符卡01
        public new void SC01()
        {

        }

        //符卡02
        public new void SC02()
        {

        }

        //符卡03
        public new void SC03()
        {

        }
    }

    public class kaguya : chct
    {
        public static new charc who { get { return charc.Kaguya; } }

        public kaguya(int id, pos pos, int where, pad pad) : base(who, id, pos, where, pad) { }

        //天赋

        //符卡01
        public new void SC01()
        {

        }

        //符卡02
        public new void SC02()
        {

        }

        //符卡03
        public new void SC03()
        {

        }
    }

    public class mokou : chct
    {
        public static new charc who { get { return charc.Mokou; } }

        public mokou(int id, pos pos, int where, pad pad) : base(who, id, pos, where, pad) { }

        //天赋

        //符卡01
        public new void SC01()
        {

        }

        //符卡02
        public new void SC02()
        {

        }

        //符卡03
        public new void SC03()
        {

        }
    }

    public class gedama : chct
    {
        public static new charc who { get { return charc.Gedama; } }

        public gedama(int id, pos pos, int where, pad pad) : base(who, id, pos, where, pad) { }
    }

    public class buff
    {
        //没有参数的buff
        //默认的buff类可能会利用buff者自身与施加buff者的相关数据，可以对施加buff者造成影响
        //更复杂的buff需要自建子类
        //每个角色的buff效果自己编写，会有一些静态的buff供调用
        //开发者备注：静态的buff需要有一系列参数，每人使用lambda表达式代入参数的具体值
        public int roundNum { get; protected set; }                 //剩余几回合结束buff
        public delegate void affect(chct buffee, chct buffer);      //buff效果的delegate
        public delegate void buffCancel();                          //取消buff
        public affect buffAffect;
        public buffCancel buffCancels;
        public chct buffer;
        public chct buffee;
        
        public buff(chct buffee, chct buffer, int roundNum, affect buffAffect)
        {
            this.buffAffect = buffAffect;
            this.roundNum = roundNum;
            this.buffee = buffee;
            this.buffer = buffer;
        }

        public void buffTrigger()
        {
            buffAffect(buffee, buffer);
        }

        public bool round()
        {
            //经过一回合。返回是否结束。
            roundNum--;
            return roundNum == 0;
        }

        //取消buff
        public void buffEnd(buffCancel buffCancel)
        {
            buffCancel();
        }
    }
}
