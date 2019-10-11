using Microsoft.AspNetCore.Mvc;
using SimpleSmtpInterceptor.Data;
using SimpleSmtpInterceptor.Data.Models;
using System.Linq;

namespace SimpleSmtpInterceptor.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DistinctFiltersController : ControllerBase
    {
        private readonly InterceptorModel _context;

        public DistinctFiltersController(InterceptorModel context)
        {
            _context = context;
        }

        // GET: api/DistinctFilters
        [HttpGet]
        public DistinctFilters GetDistinctFilters()
        {
            var m = new DistinctFilters();
            
            m.ToAddresses = _context.Emails.Select(x => x.To).Distinct().ToList();
            m.FromAddresses = _context.Emails.Select(x => x.From).Distinct().ToList();
            m.Subjects = _context.Emails.Select(x => x.Subject).Distinct().ToList();
            
            return m;
        }
    }
}