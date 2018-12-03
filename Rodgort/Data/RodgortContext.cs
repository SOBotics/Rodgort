using Microsoft.EntityFrameworkCore;

namespace Rodgort.Data
{
    public class RodgortContext : DbContext
    {
        public RodgortContext(DbContextOptions<RodgortContext> options) : base(options)
        {
        }
    }
}
