using System.IO;

namespace Course_work.Models
{
    public class Logger
    {
        private readonly string _path;

        // Создаёт файл log.txt если он не существует
        public Logger(string path)
        {
            _path = path;

            if (!File.Exists(_path))
                File.Create(_path).Dispose();
        }

        // Добавляет в файл строку с текстом
        public void Log(string message)
        {
            File.AppendAllText(_path, message);
        }

        // Очищает файл
        public void Clear()
        {
            File.WriteAllText(_path, string.Empty);
        }
    }
}