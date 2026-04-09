using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ERP.DEMO.Components.MVVM
{
    public class GenericService<TDbContext> where TDbContext : DbContext
    {
        private readonly IDbContextFactory<TDbContext> _dbContextFactory;

        public GenericService(IDbContextFactory<TDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public TDbContext GetDbContext() => _dbContextFactory.CreateDbContext();

        public IQueryable<T> GetQueryable<T>() where T : class
        {
            var dbContext = _dbContextFactory.CreateDbContext();
            return dbContext.Set<T>().AsQueryable();  // Retourne IQueryable pour l'entité
        }

        // Méthode pour exécuter une transaction
        public async Task ExecuteInTransactionAsync(Func<Task> action)
        {
            var dbContext = _dbContextFactory.CreateDbContext();
            using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                // Exécution de l'action dans la transaction
                await action();

                // Commit si tout se passe bien
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                // Rollback en cas d'exception
                await transaction.RollbackAsync();
                throw;
            }
        }

        public void ExecuteInTransaction(Action action)
        {
            var dbContext = _dbContextFactory.CreateDbContext();
            using var transaction = dbContext.Database.BeginTransaction();

            try
            {
                // Exécution de l'action dans la transaction
                action();

                // Commit si tout se passe bien
                transaction.Commit();
            }
            catch (Exception)
            {
                // Rollback en cas d'exception
                transaction.Rollback();
                throw;
            }
        }

    }



    //  public class GenericService<T, TDbContext> where T : class where TDbContext : DbContext //GenericService pour accès à un modèle d'un DbContext
    //  {
    //      private readonly IDbContextResolver _dbContextResolver;

    //      public GenericService(IDbContextResolver dbContextResolver)
    //      {
    //          _dbContextResolver = dbContextResolver;
    //      }

    //      // Récupérer le DbContext spécifique via la résolution
    //      private TDbContext GetDbContext() => _dbContextResolver.Resolve<TDbContext>() as TDbContext;

    //      public async Task<T> FindAsync(int id)
    //      {
    //          var dbContext = GetDbContext();
    //          var dbSet = dbContext.Set<T>();
    //          return await dbSet.FindAsync(id);
    //      }

    //      public TDbContext GetDb()
    //      {
    //	var dbContext = GetDbContext();
    //          return dbContext;
    //}

    //public async Task<List<T>> ToListAsync()
    //      {
    //          var dbContext = GetDbContext();
    //          var dbSet = dbContext.Set<T>();
    //          return await dbSet.ToListAsync();
    //      }

    //      public IQueryable<T> AsQueryable()
    //      {
    //          var dbContext = GetDbContext();
    //          var dbSet = dbContext.Set<T>();
    //          return dbSet.AsQueryable();
    //      }

    //      public async Task UpdateASync(T entity)
    //      {
    //          var dbContext = GetDbContext();
    //          var dbSet = dbContext.Set<T>();
    //          dbSet.Update(entity);
    //          await dbContext.SaveChangesAsync();
    //      }

    //      public async Task AddAsync(T entity)
    //      {
    //          var dbContext = GetDbContext();
    //          var dbSet = dbContext.Set<T>();
    //          await dbSet.AddAsync(entity);
    //          await dbContext.SaveChangesAsync();
    //      }

    //      public async Task DeleteAsync(int id)
    //      {
    //          var dbContext = GetDbContext();
    //          var dbSet = dbContext.Set<T>();
    //          var entity = await dbSet.FindAsync(id);
    //          if (entity != null)
    //          {
    //              dbSet.Remove(entity);
    //              await dbContext.SaveChangesAsync();
    //          }
    //      }
    //  }
}