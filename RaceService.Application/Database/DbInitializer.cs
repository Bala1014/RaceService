using System;
using System.Collections.Generic;
using System.Text;

namespace RaceService.Application.Database
{
    public class DbInitializer
    {
        private readonly ApplicationDbContext _context;
        public DbInitializer(ApplicationDbContext context)
        {
            _context = context;
        }


        public void Init()
        {
               
        }
    }
}
