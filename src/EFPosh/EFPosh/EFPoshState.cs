using EFPosh.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFPosh
{
    internal static class EFPoshState
    {
        public static DbContextInteractions LatestDbContext { get; set; }
    }
}
