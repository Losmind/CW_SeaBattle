using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Course_work.Models;
using System.Windows.Forms;

namespace Course_work.Views
{
    public partial class Arrangement : Form
    {
        private readonly Field _field;
        private readonly Image _shipImage;
        private readonly Image _cellImage;
        private readonly Size _cellSize = new Size(32, 32);
        private Point _startPoint;
        private Ship _selectedShip;

        // Конструктор по умолчанию выполняет базовую конфигурацию данного представления
        public Arrangement()
        {
            DoubleBuffered = true;
            _field = ConfigureField(10);
            _shipImage = Image.FromFile("ShipCell.png");
            _cellImage = Image.FromFile("EmptyCell.png");

            MouseClick += HandleClick;

            ConfigureControls();
            ConfigureTimer(60);
            InitializeComponent();
        }


        // Добавляет кнопки случайной игры и новой игры и подписывает на их события клика соотвествующие методы
        private void ConfigureControls()
        {
            var button = new Button() { Text = "Начать игру", Location = new Point(600, 175), Size = new Size(150, 50)};
            button.Click += StartGame;
            var button2 = new Button() { Text = "Случайная игра", Location = new Point(600, button.Bottom), Size = button.Size };
            button2.Click += StartRandomGame;
            Controls.Add(button2);
            Controls.Add(button);
        }

        // Генерирует случайную игру
        private void StartRandomGame(object sender, EventArgs eventArgs)
        {
            var battle = new Views.Battle(10);
            battle.Show();
            Close();
        }

        // Запускает игру
        private void StartGame(object sender, EventArgs eventArgs)
        {
            if (_field.GetShipToPutOrNull() == null)
            {
                var battle = new Views.Battle(_field);
                battle.Show();
                Close();
            }
            else
            {
                var result = MessageBox.Show($"Вы не расположили все корабли!", "Игра не может быть начата", MessageBoxButtons.OK);
            }
        }

        // Обрабатывает нажатие кнопки мыши
        private void HandleClick(object sender, MouseEventArgs eventArgs)
        {
            if (eventArgs.Button == MouseButtons.Right)
            {
                if (_selectedShip == null) return;
                _field.ChangeShipDirection(_selectedShip);

                if (_field.GetConflictingPoints().Count > 0)
                    _field.ChangeShipDirection(_selectedShip);

                Invalidate();
            }

            if (eventArgs.Button == MouseButtons.Left)
            {
                var point = GetCellPositionByMousePosition(eventArgs.Location);
                var ship = _field.GetShipsAt(point).FirstOrDefault();
                if (ship == null)
                {
                    var shipToPull = _field.GetShipToPutOrNull();
                    if (shipToPull != null)
                    {
                        _field.PutShip(shipToPull, point);

                        if (_field.GetConflictingPoints().Count > 0)
                        {
                            _field.RemoveShip(shipToPull);
                            MessageBox.Show("Корабли нельзя расставлять рядом", "Предупреждение");
                        }
                        Invalidate();
                        return;
                    }
                }
                else
                {
                    _selectedShip = ship;
                    Invalidate();
                    return;
                }
            }

            if (eventArgs.Button == MouseButtons.Middle)
            {
                if (_selectedShip == null) return;
                _field.RemoveShip(_selectedShip);
                Invalidate();
            }
        }

        // Получает коодинаты ячейки взависимости от координат мыши
        private Point GetCellPositionByMousePosition(Point mousePosition)
        {
            var xOffset = mousePosition.X - _startPoint.X;
            var yOffset = mousePosition.Y - _startPoint.Y;

            var x = xOffset / _cellSize.Width;
            var y = yOffset / _cellSize.Height;

            return new Point(x, y);
        }

        // Конфигурирует класс Field (создает и добавялет корабли, но не расставляет их)
        private Field ConfigureField(int size)
        {
            var field = new Models.Field(size, size);
            var startShips = CreateStartShips();

            foreach (var ship in startShips)
                field.AddShip(ship);

            return field;
        }

        // Включает таймер который постоянно вызывает Invalidate
        private void ConfigureTimer(int intervalInMilliseconds)
        {
            var timer = new Timer();
            timer.Interval = intervalInMilliseconds;
            timer.Tick += (sender, e) => Invalidate();
        }

        // Возвращает перечисление всех кораблей ( 4-однопалубных 3-двупалубных и т.д. )
        private IEnumerable<Ship> CreateStartShips()
        {
            yield return new Models.Ship(1);
            yield return new Models.Ship(1);
            yield return new Models.Ship(1);
            yield return new Models.Ship(1);

            yield return new Models.Ship(2);
            yield return new Models.Ship(2);
            yield return new Models.Ship(2);

            yield return new Models.Ship(3);
            yield return new Models.Ship(3);

            yield return new Models.Ship(4);
        }

        // При запуске формы вызывает метод SetBackgroundSize
        private void Arrangement_Load(object sender, EventArgs e)
        {
            SetBackgrounAndSize();
        }

        // Устанавливает задний фон и размеры окна
        private void SetBackgrounAndSize()
        {
            var background = Image.FromFile("backgr.png");
            Width = background.Width;
            Height = background.Height;
            BackgroundImage = background;
        }

        // Вызывается при перерисовке (При любом вызове Invalidate())
        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;
            DrawMarkup(graphics, new Point(0, 0), new Size(32, 32), _field);
            DrawField(graphics, new Point(32, 32), new Size(32, 32), _field);
        }

        // Отрисовывает поле одного из игроков
        private void DrawField(Graphics graphics, Point startPoint, Size cellSize, Field field)
        {
            _startPoint = startPoint;
            for (var y = 0; y < field.Width; y++)
            {
                for (var x = 0; x < field.Height; x++)
                {
                    var ship = field.GetShipsAt(new Point(x, y)).FirstOrDefault();

                    if (ship != null && ship.Position == new Point(x, y)) DrawShip(graphics, new Point(startPoint.X + (x * cellSize.Width), startPoint.Y + (y * cellSize.Height)), cellSize, ship);
                    else if (ship != null) continue;
                    else DrawImage(graphics, new Point(startPoint.X + (x * cellSize.Width), startPoint.Y + (y * cellSize.Height)), cellSize, _cellImage);
                }
            }
        }

        // Отрисовывает корабль
        private void DrawShip(Graphics graphics, Point startPoint, Size cellSize, Models.Ship ship)
        {
            if (ship.Position == null) return;
            var offset = ship.Direction == Direction.Horizontal ? new Point(1, 0) : new Point(0, 1);
            for (var i = 0; i < ship.Size; i++)
            {
                var point = new Point(startPoint.X + (i * cellSize.Width * offset.X), startPoint.Y + (i * cellSize.Height * offset.Y));
                DrawImage(graphics, point, cellSize, _shipImage);

            }
        }

        // Отрисовывает размтеку (А Б В Г Д 0 1 2 3...)
        private void DrawMarkup(Graphics graphics, Point startPoint, Size cellSize, Field field)
        {
            var startCharCode = 1072;
            var point = new Point(startPoint.X + cellSize.Width, startPoint.Y);

            for (var x = 1; x <= field.Width; x++)
            {
                DrawCellWithString(graphics, point, cellSize, new Font("Arial", 18), Brushes.Aqua, Brushes.Black, ((char)startCharCode).ToString());
                point = new Point(point.X + cellSize.Width, point.Y);
                startCharCode++;
            }

            point = new Point(startPoint.X, startPoint.Y + cellSize.Height);

            for (var y = 1; y <= field.Height; y++)
            {
                DrawCellWithString(graphics, point, cellSize, new Font("Arial", 18), Brushes.Aqua, Brushes.Black, y.ToString());
                point = new Point(point.X, point.Y + cellSize.Height);
                startCharCode++;
            }
        }

        // Отрисовывает клетку
        private void DrawCell(Graphics graphics, Brush brush, Point point, Size size)
        {
            graphics.FillRectangle(brush, point.X, point.Y, size.Width, size.Height);
        }

        // Отрисовывает ячейку с изображением
        private void DrawImage(Graphics graphics, Point point, Size size, Image image)
        {
            graphics.DrawImage(image, point.X, point.Y, size.Width, size.Height);
        }

        // Отрисовывает ячейку с текстом
        private void DrawCellWithString(Graphics graphics, Point point, Size size, Font font, Brush cellBrush, Brush textBrush, String s)
        {
            DrawCell(graphics, cellBrush, point, size);
            graphics.DrawString(s, font, textBrush, point);
        }
    }
}