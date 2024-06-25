using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;



namespace MoveTable_Server.Controllers
{
    // 套用 CORS 策略
    [EnableCors("AllowAny")]
    [Route("api/[controller]")]
    [ApiController]
    public class MemberManagementController : ControllerBase
    {
        private MoveTablesDbContext _context;

        public MemberManagementController(MoveTablesDbContext context)
        {
            _context = context;
        }
        
    }
}
