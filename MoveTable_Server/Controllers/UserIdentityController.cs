using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MoveTable_Server.Controllers
{
    // 套用 CORS 策略
    [EnableCors("AllowAny")]
    [Route("api/[controller]")]
    [ApiController]
    public class UserIdentityController : ControllerBase
    {
        private MoveTablesDbContext _context;

        public IConfiguration _configuration; //引用JWT

        public UserIdentityController(MoveTablesDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
    }
}
