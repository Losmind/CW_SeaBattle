using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace Course_work.Models
{
    internal class SaveLoadParser
    {
        // ФОРМАТ СОХРАНЕНИЯ
        // 1 СТРОЧКА - КОЛИЧЕСТВО КОРАБЛЕЙ ИГРОКА
        // ДАЛЬШЕ ПЕРЕЧИСЛЯЮТСЯ КОРАБЛИ ИГРОКА В ФОРМАТЕ ДЛИНАКОРАБЛЯ;X-КООРДИНАТА;Y-КООРДИНАТА;НАПРАВЛЕНИЕ
        // СТРОЧКА ПОПАДАНИЙ В ПОЛЕ ИГРОКА В ФОРМАТЕ X-КООРДИНАТА-ПОПАДАНИЯ,Y-КООРДИНАТА-ПОПАДАНИЯ;X-КООРДИНАТА-ДРУГОГО-ПОПАДАНИЯ,Y-КООРДИНАТА-ДРУГОГО-ПОПАДАНИЯ;...,...;...,...
        // СТРОЧКА - КОЛИЧЕСТВО КОРАБЛЕЙ КОМПЬЮТЕРА
        // ДАЛЬШЕ ПЕРЕЧИСЛЯЮТСЯ КОРАБЛИ КОМПЬЮТЕРА В ФОРМАТЕ ДЛИНАКОРАБЛЯ;X-КООРДИНАТА;Y-КООРДИНАТА;НАПРАВЛЕНИЕ
        // СТРОЧКА ПОПАДАНИЙ В ПОЛЕ КОМПЬЮТЕРА В ФОРМАТЕ X-КООРДИНАТА-ПОПАДАНИЯ,Y-КООРДИНАТА-ПОПАДАНИЯ;X-КООРДИНАТА-ДРУГОГО-ПОПАДАНИЯ,Y-КООРДИНАТА-ДРУГОГО-ПОПАДАНИЯ;...,...;...,...
        // ЧЕЙ ХОД? 1 - ЕСЛИ ИГРОКА, 0 -ЕСЛИ КОМПЬЮТЕРА
        // РАЗМЕРНОСТЬ ПОЛЯ (ДЛИНА ИЛИ ШИРИНА, ТАК КАК ПОЛЕ КВАДРАТНОЕ)


        //Сохраняет всю игру в файл
        public void SaveGame(Field playerField, Field aiField, bool isPlayerMove, string path)
        {
            try
            {
                var lines = new string[playerField.GetShips().Where(s=>s.Position.HasValue).Count() + aiField.GetShips().Where(s => s.Position.HasValue).Count() + 6]; // Ships for each field + 2 (shots for each) + 2 (count of ships for each) + currentMove + size
                var emptySpace = FillFieldData(lines, playerField, 0);
                FillFieldData(lines, aiField, emptySpace);
                lines[lines.Length - 2] = isPlayerMove ? "1" : "0";
                lines[lines.Length - 1] = "10";
                File.WriteAllLines(path, lines, Encoding.UTF8);
            }
            catch { }
        }

        // Парсит игру из файла
        public ParsingResult ParseGame(string path)
        {
            if (!File.Exists(path)) throw new InvalidOperationException();

            var lines = File.ReadAllLines(path);
            var size = int.Parse(lines[lines.Length - 1]);
            var parsingResult = ParseFieldFromString(lines, size, 0);
            var playerField = parsingResult.Item1;
            var emptySpace = parsingResult.Item2;
            var aiField = ParseFieldFromString(lines, size, emptySpace).Item1;
            var currentMove = lines[lines.Length - 2];

            return new ParsingResult(playerField, aiField, size, currentMove);
        }

        // Парсит корабли из строки
        private Tuple<Field, int> ParseFieldFromString(string[] lines, int size, int startIndex)
        {
            var field = new Field(size, size);

            var shipsCount = int.Parse(lines[startIndex]);
            var ships = new List<Ship>();
            var shots = new HashSet<Point>();

            for (var i = 0; i < shipsCount; i++)
                ships.Add(ParseShipFromString(lines[i +startIndex+ 1]));

            shots = ParseShotsFromString(lines[startIndex + shipsCount+1]);

            foreach(var ship in ships)
                field.AddShip(ship);

            foreach (var shot in shots)
                field.ShootTo(shot);

            return Tuple.Create(field, shipsCount+startIndex+2);
        }

        // Парсит конкреттный корабль из строки
        private Ship ParseShipFromString(string s)
        {
            var parts = s.Split(';');
            var size = int.Parse(parts[0]);
            var position = new Point(int.Parse(parts[1]), int.Parse(parts[2]));
            var direction = parts[3] == "Vertical" || parts[3] == "vertical" ? Direction.Vertical : Direction.Horizontal;

            var ship = new Ship(size);
            ship.Position = position;
            ship.Direction = direction;

            return ship;
        }

        // Парсит попадания из строки
        private HashSet<Point> ParseShotsFromString(string s)
        {
            if (string.IsNullOrEmpty(s)) return new HashSet<Point>();
            return s.Split(';')
                .Select(p =>
                    new Point(int.Parse(p.Split(',')[0]),
                                int.Parse(p.Split(',')[1])
                             ))
                    .ToHashSet();
        }

        // Заполняет поле в файл
        private int FillFieldData(string[] lines, Field field, int startIndex)
        {
            var ships = field.GetShips().Where(s => s.Position.HasValue).ToList();
            var shots = field.GetShots();
            lines[startIndex] = ships.Count.ToString();

            for (var i = 0; i < ships.Count; i++)
                lines[i +startIndex + 1] = ShipToString(ships[i]);

            lines[startIndex + ships.Count+1] = CreateShotsString(shots);
            return startIndex + ships.Count + 2;
        }

        // Форматирует корабль в текстовое представление
        private string ShipToString(Ship ship)
        {
            return $"{ship.Size};{ship.Position.Value.X};{ship.Position.Value.Y};{ship.Direction}";
        }

        // Форматирует попадания в текстовое представление
        private string CreateShotsString(IEnumerable<Point> shots)
        {
            return string.Join(";", shots.Select(shot => $"{shot.X},{shot.Y}"));
        }
    }
}
