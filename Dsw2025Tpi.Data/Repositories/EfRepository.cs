using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;


namespace Dsw2025Tpi.Data.Repositories;


public class EfRepository : IRepository
{
    // Inyección del contexto de base de datos (DbContext)
    private readonly Dsw2025TpiContext _context;

    // Constructor que recibe el contexto y lo guarda en la variable privada
    public EfRepository(Dsw2025TpiContext context)
    {
        _context = context;
    }

    // Método genérico para agregar una entidad a la base
    public async Task<T> Add<T>(T entity) where T : EntityBase
    {
        await _context.AddAsync(entity);       // Agrega la entidad al contexto
        await _context.SaveChangesAsync();     // Guarda los cambios en la base
        return entity;                         // Devuelve la entidad agregada
    }

    // Método genérico para eliminar una entidad
    public async Task<T> Delete<T>(T entity) where T : EntityBase
    {
        _context.Remove(entity);               // Marca la entidad para eliminarla
        await _context.SaveChangesAsync();     // Aplica los cambios
        return entity;                         // Devuelve la entidad eliminada
    }

    // Devuelve la primera entidad que cumpla una condición (o null si no existe)
    //el where T : EntityBase Significa que el método solo puede trabajar con tipos que hereden de EntityBase.
    public async Task<T?> First<T>(Expression<Func<T, bool>> predicate, params string[] include) where T : EntityBase
    {
        return await Include(_context.Set<T>(), include) // Aplica los includes
                     .FirstOrDefaultAsync(predicate);     // Devuelve el primero que cumpla la condición
    }

    // Devuelve todas las entidades de un tipo
    //Los includes sirven para traer datos relacionados en una sola consulta.
    public async Task<IEnumerable<T>?> GetAll<T>(params string[] include) where T : EntityBase
    {
        return await Include(_context.Set<T>(), include) // Aplica includes si los hay
                     .ToListAsync();                      // Devuelve todas las entidades como lista
    }

    // Devuelve una entidad por su ID
    public async Task<T?> GetById<T>(Guid id, params string[] include) where T : EntityBase
    {
        return await Include(_context.Set<T>(), include) // Aplica includes
                     .FirstOrDefaultAsync(e => e.Id == id); // Busca la entidad con ese ID
    }

    // Devuelve una lista filtrada por una condición
    public async Task<IEnumerable<T>?> GetFiltered<T>(Expression<Func<T, bool>> predicate, params string[] include) where T : EntityBase
    {
        return await Include(_context.Set<T>(), include) // Aplica includes
                     .Where(predicate)                   // Aplica filtro
                     .ToListAsync();                     // Devuelve como lista
    }

    // Actualiza una entidad existente
    public async Task<T> Update<T>(T entity) where T : EntityBase
    {
        _context.Update(entity);            // Le avisa al contexto que la entidad cambió
        await _context.SaveChangesAsync();  // Aplica los cambios
        return entity;                      // Devuelve la entidad actualizada
    }

    // Método genérico que agrega Includes a una consulta de Entity Framework
    // Esto sirve para traer datos relacionados (como Order con sus OrderItems)
    private static IQueryable<T> Include<T>(IQueryable<T> query, string[] includes) where T : EntityBase
    {
        // Se parte de la consulta original (por ejemplo: context.Orders)
        var includedQuery = query;

        // Recorre cada string que representa el nombre de una propiedad de navegación
        // Por ejemplo: "OrderItems", "Customer"
        foreach (var include in includes)
        {
            // A la consulta se le agrega ese include usando el método .Include()
            // Esto le dice a EF: "Traé también esta relación"
            includedQuery = includedQuery.Include(include);
        }

        // Devuelve la consulta completa, lista para ejecutarse, con los includes aplicados
        return includedQuery;
    }
}