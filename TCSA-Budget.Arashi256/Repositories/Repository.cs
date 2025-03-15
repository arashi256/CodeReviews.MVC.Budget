using Microsoft.EntityFrameworkCore;
using TCSA_Budget.Arashi256.Interfaces;
using TCSA_Budget.Arashi256.Models;

namespace TCSA_Budget.Arashi256.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly BudgetDbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(BudgetDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAll() => await _dbSet.AsNoTracking().ToListAsync();

        public async Task<T?> GetById(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            return entity == null ? null : entity;
        }

        public async Task Add(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task Update(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}