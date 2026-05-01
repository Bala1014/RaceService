using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceService.Application.Database;
using RaceService.Application.Domain.Entities;

namespace RaceService.Api.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class RaceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RaceController> _logger;


        public RaceController(ApplicationDbContext DbContext, ILogger<RaceController> logger)
        {
            _logger = logger;
            _context = DbContext;
        }

        [HttpGet]
        public async Task<IActionResult> getAllRaces()
        {
            var result = await _context.Race.ToListAsync();

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> getRaceById([FromRoute] string id)
        {
            Guid requestId = Guid.Parse(id); 
            var result = await _context.Race.SingleAsync(r => r.Id == requestId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> createRace(int laps, string name,
            DateTime startTimeUTC, Guid trackId)
        {
            var newEntity = new Race
            {
                Id = new Guid(),
                Name = name,
                StartTimeUTC = startTimeUTC,
                Laps         = laps,
                Status       = RaceStatus.Upcoming,
                CreatedAt    = DateTime.Now,
                TrackId      = trackId
            } ;

            try
            {
                _context.Race.AddRange(newEntity);
                _context.SaveChanges();
            }catch(Exception e)
            {
                _logger.LogCritical(e, "error in creating new Race");
            }

            return Created();
        }
        
    }


}
