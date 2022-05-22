using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Course_work.Models;
using static Course_work.Models.Field;

namespace Course_work.Views
{
    public partial class Battle : Form
    {
        public Logger _logger = new Logger("log.txt");

        private readonly Size _cellSize = new Size(32, 32);
        private readonly Image _shipImage;
        private readonly Image _crossImage;
        private readonly Image _cellImage;
        private readonly Image _missImage;
        private readonly Field _playerField;
        private readonly Field _AIField;
        private readonly Random _random = new Random();

        private double _AIDifficult = 50;
        private Point _playerStartPoint = new Point(32, 32);
        private Point _playerMarkupStartPoint = new Point(0, 0);
        private Point _AIStartPoint = new Point(32 + 10 * 32 + 100 + 32, 32);
        private Point _AIMarkupStartPoint = new Point(32 + 10 * 32 + 100, 0);
        private bool _isPlayerMove = true;

        // Создает случайную игру (Новые поля для компьютера и игрока с размером size) и конфигурирует базовые настройки данного представления
        public Battle(int size)
        {
            DoubleBuffered = true;
            MouseClick += HandleClick;
            _shipImage = Image.FromFile("ShipCell.png");
            _crossImage = Image.FromFile("CrossCell.png");
            _cellImage = Image.FromFile("EmptyCell.png");
            _missImage = Image.FromFile("MissCell.png");
            _playerField = new Field(size, size);
            foreach (var ship in CreateStartShips())
                _playerField.AddShip(ship);
            _AIField = new Field(size, size);
            foreach (var ship in CreateStartShips())
                _AIField.AddShip(ship);
            _random = new Random();
            _logger.Clear();
            ConfigureControls();
            ArrangeShipsAutomaticly(_AIField);
            ArrangeShipsAutomaticly(_playerField);
            InitializeComponent();
        }

        // Создает случайное поле для компьютера и конфигурирует базовые настройки данного представления
        public Battle(Field playerField)
        {
            DoubleBuffered = true;
            MouseClick += HandleClick;
            _shipImage = Image.FromFile("ShipCell.png");
            _crossImage = Image.FromFile("CrossCell.png");
            _cellImage = Image.FromFile("EmptyCell.png");
            _missImage = Image.FromFile("MissCell.png");
            _playerField = playerField;
            _AIField = new Field(_playerField.Width, _playerField.Height);
            foreach (var ship in CreateStartShips())
                _AIField.AddShip(ship);
            _random = new Random();
            _logger.Clear();
            ConfigureControls();
            ArrangeShipsAutomaticly(_AIField);
            InitializeComponent();
        }

        // Создает игру из файла сохранения
        public Battle(ParsingResult parsingResult)
        {
            DoubleBuffered = true;
            MouseClick += HandleClick;
            _shipImage = Image.FromFile("ShipCell.png");
            _crossImage = Image.FromFile("CrossCell.png");
            _cellImage = Image.FromFile("EmptyCell.png");
            _missImage = Image.FromFile("MissCell.png");
            _playerField = parsingResult.PlayerField;
            _AIField = parsingResult.AiField;
            _isPlayerMove = parsingResult.CurrentMove == "1" ? true : false;
            _random = new Random();
            _logger.Clear();
            ConfigureControls();
            InitializeComponent();
        }

        // Метод добавляющий кнопку сохранения игры
        private void ConfigureControls()
        {
            var button = new Button() { Text = "Сохранить игру" };
            button.Location = new Point(500, 500);
            button.Width = 100;
            button.Height = 30;
            button.Click += SaveGame;
            Controls.Add(button);
        }

        // Метод отвечающий за сохранение игры в файл
        private void SaveGame(object sender, EventArgs eventArgs)
        {
            // Комменатрии по работе парсера в классе парсера
            var parser = new SaveLoadParser();
            parser.SaveGame(_playerField, _AIField, true, "save.txt");
        }

        // Получает координаты ячейки по координатам мыши
        private Point GetCellPositionByMousePosition(Point mousePosition)
        {
            var xOffset = mousePosition.X - _AIStartPoint.X;
            var yOffset = mousePosition.Y - _AIStartPoint.Y;

            var x = xOffset / _cellSize.Width;
            var y = yOffset / _cellSize.Height;

            if (xOffset < 0 || yOffset < 0) return new Point(-100, -100);

            return new Point(x, y);
        }

        // Проверяет находится ли точка point(x, y) внутри поля (size, size)
        private bool InBounds(Field field, Point point)
        {
            return point.X >= 0 && point.X < field.Width && point.Y >= 0 && point.Y < field.Height;
        }

        // Обратаывает нажатие кнопки мыши
        private void HandleClick(object sender, MouseEventArgs eventArgs)
        {
            var point = GetCellPositionByMousePosition(eventArgs.Location);
            if (_isPlayerMove)
            {
                var shotResult = _AIField.ShootTo(point);
                _logger.Log($"Игрок выстрелил в клетку {point.X},{point.Y} результат: {shotResult.ToString()}\n");
                if (shotResult == Field.ShotResult.Miss && InBounds(_AIField, point))
                {
                    _isPlayerMove = false;
                    AIMove();
                }
                if (!_AIField.HasAliveShips() || !_playerField.HasAliveShips())
                {
                    Invalidate();
                    EndGame();
                }
            }
            else
            {
                AIMove();
            }
            Invalidate();
        }
         
        // Завершает игру и выводит сообщение
        private void EndGame()
        {
            var message = _playerField.HasAliveShips() ? "вы победили, поздравляем!" : _AIField.HasAliveShips() ? "вы проиграли." : "вы победили, поздравляем!";
            var winner = _playerField.HasAliveShips() ? "игрок" : _AIField.HasAliveShips() ? "компьютер" : "игрок";
            var result = MessageBox.Show($"Игра завершена, {message}", "Игра завершена", MessageBoxButtons.OK);
            _logger.Log($"Игра завершена, победил {winner}.");
            if (result == DialogResult.OK)
            {
                Close();
            }
        }

        // Ход компьютера
        private void AIMove(Point? position = null)
        {
            
            Invalidate();
            if (_isPlayerMove) return;

            var freeCells = new List<Point>();
            var shots = _playerField.GetShots();

            for (var y = 0; y < _playerField.Height; y++)
            {
                for (var x = 0; x < _playerField.Width; x++)
                {
                    var point = new Point(x, y);
                    if (shots.Contains(point)) continue;
                    freeCells.Add(point);
                }
            }
            if (freeCells.Count <= 0)
            {
                EndGame();
                return;
            }
            ShotResult shotResult = ShotResult.Miss;
            Point? target = null;
            if (position == null)
            {
                var random = _random.Next(freeCells.Count);
                target = freeCells[random];
                shotResult = _playerField.ShootTo((Point)target);
                _logger.Log($"ИИ выстрелил в клетку {target.Value.X},{target.Value.Y} результат: {shotResult.ToString()}\n");
            }
            else
            {
                var ship = _playerField.GetShipsAt(position.Value).FirstOrDefault();
                target = ship.GetPositionPoints().Where(p => !shots.Contains(p)).FirstOrDefault();
                shotResult = _playerField.ShootTo((Point)target);
                _logger.Log($"ИИ выстрелил в клетку {target.Value.X},{target.Value.Y} результат: {shotResult.ToString()}\n");
                if (_playerField.IsAlive(ship))
                {

                    var random = _random.Next(0, 100);
                    if (random < _AIDifficult)
                    {
                        AIMove(target);
                        return;
                    }
                    else
                    {
                        AIMove();
                        return;
                    }

                }
                else
                {
                    AIMove();
                    return;
                }
            }
            if (shotResult == ShotResult.Hit)
            {
                AIMove(target);
            }
            else
            {
                _isPlayerMove = true;
                return;
            }
        }

        // Вызывается каждый раз при перерисовке окна (вызов Invalidate())
        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;
            DrawMarkup(graphics, _playerMarkupStartPoint, new Size(32, 32), _playerField);
            DrawField(graphics, _playerStartPoint, new Size(32, 32), _playerField, false);
            DrawMarkup(graphics, _AIMarkupStartPoint, new Size(32, 32), _playerField);
            DrawField(graphics, _AIStartPoint, new Size(32, 32), _AIField, true);
        }

        // Отрисовывает поле (компьютера или игрока)
        private void DrawField(Graphics graphics, Point startPoint, Size cellSize, Field field, bool useFogOfWar)
        {
            for (var y = 0; y < field.Width; y++)
            {
                for (var x = 0; x < field.Height; x++)
                {
                    var ship = field.GetShipsAt(new Point(x, y)).FirstOrDefault();

                    if (!useFogOfWar)
                    {
                        var shots = field.GetShots();
                        var point = new Point(startPoint.X + (x * cellSize.Width), startPoint.Y + (y * cellSize.Height));

                        if (field.GetShipsAt(new Point(x, y)).Count > 0 && !shots.Any(s => s == new Point(x, y)))
                            DrawImage(graphics, point, cellSize, _shipImage);
                        else if (field.GetShipsAt(new Point(x, y)).Count > 0 && shots.Any(s => s == new Point(x, y)))
                            DrawImage(graphics, point, cellSize, _crossImage);
                        else if (field.GetShipsAt(new Point(x, y)).Count <= 0 && shots.Any(s => s == new Point(x, y)))
                            DrawImage(graphics, point, cellSize, _missImage);
                        else
                            DrawImage(graphics, point, cellSize, _cellImage);
                    }
                    else
                    {
                        var shots = field.GetShots();
                        var point = new Point(startPoint.X + (x * cellSize.Width), startPoint.Y + (y * cellSize.Height));

                        if (field.GetShipsAt(new Point(x, y)).Count > 0 && shots.Any(s => s == new Point(x, y)))
                            DrawImage(graphics, point, cellSize, _crossImage);
                        else if (field.GetShipsAt(new Point(x, y)).Count <= 0 && shots.Any(s => s == new Point(x, y)))
                            DrawImage(graphics, point, cellSize, _missImage);
                        else
                            DrawImage(graphics, point, cellSize, _cellImage);
                    }
                }
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

        // Отрисовывает клетку с изображением
        private void DrawImage(Graphics graphics, Point point, Size size, Image image)
        {
            graphics.DrawImage(image, point.X, point.Y, size.Width, size.Height);
        }

        // Отрисовывает клетку со строкой
        private void DrawCellWithString(Graphics graphics, Point point, Size size, Font font, Brush cellBrush, Brush textBrush, String s)
        {
            DrawCell(graphics, cellBrush, point, size);
            graphics.DrawString(s, font, textBrush, point);
        }

        // Расставялет случайным образом корабли в поле
        private bool ArrangeShipsAutomaticly(Field field)
        {
            for (var i = 0; i < 1000; i++)
            {
                RemoveAllShips(field);
                if (TryArrangeShips(field, 1000))
                    return true;
            }
            return false;
        }

        // Удаляет все корабли с поля
        private void RemoveAllShips(Field field)
        {
            foreach (var ship in field.GetShips().Where(it => it.Position != null))
                field.PutShip(ship, new Point(-1, -1));
        }

        // Пытается расставить корабли (нужно для случайной расстановки)
        private bool TryArrangeShips(Field field, int steps)
        {
            for (var i = 0; i < steps; i++)
            {
                var ship = field.GetShipToPutOrNull();
                if (ship == null)
                    break;
                var x = _random.Next(0, field.Width);
                var y = _random.Next(0, field.Height);
                field.PutShip(ship, new Point(x, y));
                if (_random.Next(0, 2) == 1)
                    field.ChangeShipDirection(ship);

                if (field.GetConflictingPoints().Any())
                    field.PutShip(ship, new Point(-1, -1));
            }

            return field.GetShipToPutOrNull() == null
                && !field.GetConflictingPoints().Any();
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

        // При запуске формы вызывает метод SetBackgrounSize
        private void Battle_Load(object sender, EventArgs e)
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
    }
}