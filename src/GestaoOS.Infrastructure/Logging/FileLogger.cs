using System;
using System.IO;
using GestaoOS.Application.Abstractions;

namespace GestaoOS.Infrastructure.Logging
{
    public class FileLogger : ILogger
    {
        private readonly string _logPath;

        public FileLogger()
        {
            var directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            Directory.CreateDirectory(directory);
            _logPath = Path.Combine(directory, "gestao-os.log");
        }

        public void Error(Exception exception)
        {
            Write("ERROR", exception.ToString());
        }

        public void Info(string message)
        {
            Write("INFO", message);
        }

        private void Write(string level, string message)
        {
            File.AppendAllText(_logPath, string.Format("{0:yyyy-MM-dd HH:mm:ss} [{1}] {2}{3}", DateTime.Now, level, message, Environment.NewLine));
        }
    }
}
