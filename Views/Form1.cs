using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Course_work
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            timer1.Interval = 1000;
            timer1.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = DateTime.Now.ToString();
        }

        private void button2_Click(object sender, EventArgs e) // кнопка "Закрыть приложение"
        {
            Application.Exit();
        }

        private void обАвтореToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Автор: Семенов Илья Максимович\r\n" + "Группа: 3045\r\n" + "Почта: semenov.ilya9879@yandex.ru\r\n", "Об авторе");
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Данный проект представляет собой программный продукт, реализующий игру в «Морской бой» с компьютером. Программа предназначена для обычных пользователей компьютеров и призвана обеспечить интересное проведение их свободного времени.\r\nПосле нажатия кнопки «Начать игру» пользователь сможет сыграть с компьютером в \"морской бой\".", "О программе");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Во владении каждого игрока есть 10 кораблей, располагающихся на поле размером 10 на 10 клеток: один — четырехпалубный, занимающий на поле четыре клетки; два — трехпалубных, занимающие по три клетки каждый; три — двухпалубных, занимающие две клетки и четыре — однопалубных, занимающие по одной клетке соответственно. Игроку нужно расположить на своём поле корабли так, чтобы они находились на расстоянии не менее одной клетки друг от друга (чтобы они не касались). Корабли должны быть прямыми, они не могут располагаться по диагонали. Корабли разрешено ставить в углы и по краям поля. После расстановки кораблей игра начинается. Игрок, получивший право первым начать игру, производит «выстрел» по одной из клеток на поле противника, например, с координатой «А6». Если на этой клетке есть часть корабля или весь корабль, то это соответственно значит, что корабль «подбит» или «убит». После этого игрок получает право еще на один ход. Ход переходит сопернику, если в выбранной им клетке нет корабля. Игра ведется до тех пор, пока у одного из участников не будут потоплены все корабли.", "Правила игры в морской бой");
        }

        private void button1_Click(object sender, EventArgs e) // кнопка "Начать новую игру"
        {
            var arrangement = new Views.Arrangement();
            arrangement.Show();
        }


        private void button4_Click(object sender, EventArgs e) // кнопка "Загрузить игру"
        {
            var parser = new Models.SaveLoadParser();
            var parsingResult = parser.ParseGame("save.txt");
            var battle = new Views.Battle(parsingResult);
            battle.Show();
        }
    }
}
