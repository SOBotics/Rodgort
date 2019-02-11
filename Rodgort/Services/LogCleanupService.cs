using System;
using System.Linq;
using Rodgort.Data;

namespace Rodgort.Services
{
    public class LogCleanupService
    {
        public const string SERVICE_NAME = "Delete old log entries";
        
        private readonly RodgortContext _context;
        
        public LogCleanupService(RodgortContext context)
        {
            _context = context;
        }

        public void Execute()
        {
            var logsToDelete = _context
                .Logs
                .Where(l => l.Level == "Trace" && l.TimeLogged < DateTime.UtcNow.AddDays(2)
                            || l.Level == "Debug" && l.TimeLogged < DateTime.UtcNow.AddDays(4)
                            || l.Level == "Info" && l.TimeLogged < DateTime.UtcNow.AddDays(7)
                            || l.Level == "Warn" && l.TimeLogged < DateTime.UtcNow.AddDays(30)
                            || l.Level == "Error" && l.TimeLogged < DateTime.UtcNow.AddDays(90)
                            || l.Level == "Error" && l.TimeLogged < DateTime.UtcNow.AddDays(90)
                );

            foreach (var logToDelete in logsToDelete)
                _context.Logs.Remove(logToDelete);

            _context.SaveChanges();
        }
    }
}
