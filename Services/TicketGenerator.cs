using System;
using System.Threading;

namespace Hofinsoft.Mdg.Services
{
    /// <summary>
    /// Generates auto-incrementing ticket references in NMSR/YYYY/MM/XXXX format.
    /// </summary>
    public class TicketGenerator
    {
        private static int _counter = 0;

        public string GenerateTicketRef()
        {
            var seq = Interlocked.Increment(ref _counter);
            var now = DateTime.UtcNow;
            return $"NMSR/{now.Year}/{now.Month:D2}/{seq:D4}";
        }

        /// <summary>
        /// Reset counter (useful for testing).
        /// </summary>
        public static void Reset(int startFrom = 0)
        {
            _counter = startFrom;
        }
    }
}
