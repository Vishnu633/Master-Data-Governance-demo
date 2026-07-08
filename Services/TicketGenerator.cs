using System;
using System.Linq;
using System.Threading;
using Hofinsoft.Mdg.Data;

namespace Hofinsoft.Mdg.Services
{
    /// <summary>
    /// Generates auto-incrementing ticket references in NMSR/YYYY/MM/XXXX format.
    /// </summary>
    public class TicketGenerator
    {
        private static int _counter = 0;

        public string GenerateTicketRef(NomcatDbContext db)
        {
            var seq = Interlocked.Increment(ref _counter);
            var now = DateTime.UtcNow;

            while (true)
            {
                var refNo = $"NMSR/{now.Year}/{now.Month:D2}/{seq:D4}";
                var exists = db.ItemRequests.Any(r => r.RequestRefNo == refNo);
                if (!exists)
                {
                    return refNo;
                }
                seq = Interlocked.Increment(ref _counter);
            }
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
