using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;

namespace clinicaMedica.Helpers
{
    public class PaginatedList<T> : List<T>
    {
        // Variables Globales
        public int PageIndex { get; set; }
        public int TotalPages { get; set; }

        // Constructor
        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count/(double)pageSize);
            this.AddRange(items);
        }

        // Propiedades para Habilitar / Deshabilitar botones de navegación

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;


        // Se hace factoring por motivo de mantener constructor de manera async
        public static async Task<PaginatedList<T>> CreateAsync(
            IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }

        
    }
    
}
