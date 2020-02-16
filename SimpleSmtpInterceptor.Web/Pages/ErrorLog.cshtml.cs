using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SimpleSmtpInterceptor.Data;
using SimpleSmtpInterceptor.Data.Entities;

namespace SimpleSmtpInterceptor.Web.Pages
{
    public class ErrorLogModel : PageModel
    {
        private readonly SimpleSmtpInterceptor.Data.InterceptorModel _context;

        public ErrorLogModel(SimpleSmtpInterceptor.Data.InterceptorModel context)
        {
            _context = context;
        }

        public IList<Log> Log { get;set; }

        public async Task OnGetAsync()
        {
            Log = await _context
                .Logs
                .OrderByDescending(x => x.CreatedOnUtc)
                .Take(20)
                .ToListAsync();
        }
    }
}
