using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository
{
    public class EfRepository : IRepository
    {
        private readonly ApplicationDbContext dbContext;

        public EfRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public Task<T> GetByIdAsync<T>(Guid id) where T : BaseEntity
        {
            return dbContext.Set<T>().SingleOrDefaultAsync(e => e.Id == id);
        }

        public Task<T> GetByIdWithIncludeAsync<T>(Guid id, string[] includes) where T : BaseEntity
        {
            var query = dbContext.Set<T>().Where(p => p.Id == id).AsQueryable();
            foreach (var include in includes)
                query = query.Include(include);

            return query.FirstOrDefaultAsync();
        }

        public Task<List<T>> ListAsync<T>() where T : BaseEntity
        {
            return dbContext.Set<T>().ToListAsync();
        }

        public async Task<T> AddAsync<T>(T entity) where T : BaseEntity
        {
            await dbContext.Set<T>().AddAsync(entity);
            await dbContext.SaveChangesAsync();

            return entity;
        }

        public async Task UpdateAsync<T>(T entity) where T : BaseEntity
        {
            dbContext.Entry(entity).State = EntityState.Modified;
            await dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync<T>(T entity) where T : BaseEntity
        {
            dbContext.Set<T>().Remove(entity);
            await dbContext.SaveChangesAsync();
        }
    }
}