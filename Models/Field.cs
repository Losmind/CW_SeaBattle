using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Course_work.Models
{
    public partial class Field
    {
        private readonly HashSet<Ship> _ships = new HashSet<Ship>();
        private readonly HashSet<Point> _shots = new HashSet<Point>();

        public Field(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public event Action Updated;

        public int Width { get; }
        public int Height { get; }

        public void AddShip(Ship ship)
        {
            _ships.Add(ship);
            Updated?.Invoke();
        }

        public List<Ship> GetShips()
        {
            return _ships.ToList();
        }

        public Ship GetShipToPutOrNull()
        {
            return _ships
                .Where(ship => !ship.Position.HasValue)
                .OrderByDescending(ship => ship.Size)
                .FirstOrDefault();
        }

        public void RemoveShip(Ship ship)
        {
            ship.Position = null;
        }

        public bool PutShip(Ship ship, Point point)
        {
            if (!_ships.Contains(ship))
                throw new InvalidOperationException();
            var actualShip = ship as Ship;

            var dx = 1;
            var dy = 1;
            if (ship.Direction == Direction.Horizontal)
                dx = actualShip.Size;
            else
                dy = actualShip.Size;

            if (0 <= point.X && point.X + dx <= Width
                && 0 <= point.Y && point.Y + dy <= Height)
            {
                actualShip.Position = point;
                Updated?.Invoke();
                return true;
            }
            actualShip.Position = null;
            Updated?.Invoke();
            return false;
        }

        public List<Ship> GetShipsAt(Point point)
        {
            var result = _ships
                .Where(ship => ship.GetPositionPoints().Contains(point))
                .OrderBy(ship => ship.Size)
                .ToList();
            return result;
        }

        public bool ChangeShipDirection(Ship ship)
        {
            var actualShip = ship as Ship;

            if (!actualShip.Position.HasValue)
                return false;

            var position = actualShip.Position.Value;
            if (actualShip.Direction == Direction.Horizontal)
            {
                var overflow = position.Y + ship.Size - Height;
                if (overflow > 0)
                {
                    var newPosition = new Point(position.X, position.Y - overflow);
                    if (newPosition.Y < 0)
                    {
                        actualShip.Position = null;
                        Updated?.Invoke();
                        return false;
                    }

                    actualShip.Position = newPosition;
                }
                actualShip.Direction = Direction.Vertical;
            }
            else
            {
                var overflow = position.X + ship.Size - Width;
                if (overflow > 0)
                {
                    var newPosition = new Point(position.X - overflow, position.Y);
                    if (newPosition.X < 0)
                    {
                        actualShip.Position = null;
                        Updated?.Invoke();
                        return false;
                    }

                    actualShip.Position = newPosition;
                }
                actualShip.Direction = Direction.Horizontal;
            }
            Updated?.Invoke();
            return true;
        }

        public HashSet<Point> GetConflictingPoints()
        {
            var shipToRoundMap = _ships.ToDictionary(ship => ship, GetShipRoundPoints);

            var result = new HashSet<Point>();
            foreach (var ship in _ships)
            {
                var positionPoints = ship.GetPositionPoints();
                foreach (var point in positionPoints)
                {
                    var isPointInOtherShipRound = shipToRoundMap
                        .Any(pair => !pair.Key.Equals(ship) && pair.Value.Contains(point));
                    if (isPointInOtherShipRound)
                        result.Add(point);
                }
            }
            return result;
        }

        public ShotResult ShootTo(Point point)
        {
            if (_shots.Contains(point))
                return ShotResult.Cancel;

            _shots.Add(point);

            var ship = GetShipsAt(point).FirstOrDefault();
            if (ship == null)
            {
                Updated?.Invoke();
                return ShotResult.Miss;
            }

            var willBlow = ship.GetPositionPoints()
                .All(p => _shots.Contains(p));

            if (willBlow)
                _shots.UnionWith(GetShipRoundPoints(ship));

            Updated?.Invoke();
            return ShotResult.Hit;
        }

        private HashSet<Point> GetShipRoundPoints(Ship ship)
        {
            var result = ship.GetPositionPoints()
                .SelectMany(p => GetRoundPoints(p))
                .Where(p => 0 <= p.X && p.X < Width && 0 <= p.Y && p.Y < Height)
                .ToHashSet();
            return result;
        }
        private IEnumerable<Point> GetRoundPoints(Point point)
        {
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                    yield return new Point(point.X + dx, point.Y + dy);
        }
        public HashSet<Point> GetShots()
        {
            return _shots.ToHashSet();
        }

        public bool IsAlive(Ship ship)
        {
            return ship.GetPositionPoints().Any(p => !_shots.Contains(p));
        }

        public bool HasAliveShips()
        {
            return _ships.Any(ship => IsAlive(ship));
        }
    }
}