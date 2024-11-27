
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Divination.Services
{
    public abstract class DataService<TEntity, Tdto, TFilter, TCreator, TUpdater> :
        DataGetService<TEntity, Tdto, TFilter>, IAddDataService<Tdto, TCreator>, IUpdateDataService<Tdto, TUpdater>, IDeleteDataService<Tdto>
          where TEntity : Db.Model.IEntity
          where TUpdater : Contract.Model.IEntity
          where Tdto : Contract.Model.Entity
          where TFilter : Contract.Model.Filter<Tdto>
    {

        public DataService(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }

        protected virtual TEntity MapToEntityAdd(TCreator creator, Guid userId)
        {
            var result = _mapper.Map<TEntity>(creator);
            result.Id = Guid.NewGuid();
            result.IsDeleted = false;
            result.VersionDate = DateTimeOffset.Now;
            result = AdditionalMapForAdd(result, creator, userId);
            return result;
        }

        protected abstract TEntity AdditionalMapForAdd(TEntity entity, TCreator creator, Guid userId);

        protected virtual async Task PrepareBeforeAdd(Db.Interface.IRepository<TEntity> repository, TCreator creator, Guid userId, CancellationToken token)
        {
            await Task.CompletedTask;
        }

        protected virtual async Task PrepareBeforeUpdate(Db.Interface.IRepository<TEntity> repository, TUpdater entity, Guid userId, CancellationToken token)
        {
            await Task.CompletedTask;
        }

        protected virtual async Task PrepareBeforeDelete(Db.Interface.IRepository<TEntity> repository, TEntity entity, Guid userId, CancellationToken token)
        {
            await Task.CompletedTask;
        }

        protected virtual async Task ActionAfterAdd(Db.Interface.IRepository<TEntity> repository, TCreator creator, TEntity entity, Guid userId, CancellationToken token)
        {
            await Task.CompletedTask;
        }

        protected virtual async Task ActionAfterUpdate(Db.Interface.IRepository<TEntity> repository, TUpdater updater, TEntity entity, Guid userId, CancellationToken token)
        {
            await Task.CompletedTask;
        }

        protected virtual async Task ActionAfterDelete(Db.Interface.IRepository<TEntity> repository, TEntity entity, Guid userId, CancellationToken token)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// add item method
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public virtual async Task<Tdto> AddAsync(TCreator creator, Guid userId, CancellationToken token)
        {
            return await ExecuteAsync(async (repo) =>
            {
                var entity = MapToEntityAdd(creator, userId);
                await PrepareBeforeAdd(repo, creator, userId, token);
                var result = await repo.AddAsync(entity, false, token);
                await ActionAfterAdd(repo, creator, result, userId, token);
                await repo.SaveChangesAsync();
                var prepare = _mapper.Map<Tdto>(result);
                prepare = await Enrich(prepare, token);
                return prepare;
            });
        }

        protected abstract TEntity UpdateFillFields(TUpdater entity, TEntity entry);

        public virtual async Task<Tdto> UpdateAsync(TUpdater entity, Guid userId, CancellationToken token)
        {
            return await ExecuteAsync(async (repo) =>
            {
                var entry = await repo.GetAsync(entity.Id, token);
                if (entity == null) throw new DataServiceException($"Entity with id = {entity.Id} not found in DB");
                if (!(await CheckUser(entry, userId))) throw new DataServiceException($"Entity with id = {entity.Id} not found in DB");
                entry = UpdateFillFields(entity, entry);
                await PrepareBeforeUpdate(repo, entity, userId, token);
                TEntity result = await repo.UpdateAsync(entry, false, token);
                await ActionAfterUpdate(repo, entity, result, userId, token);
                await repo.SaveChangesAsync();
                var prepare = _mapper.Map<Tdto>(result);
                prepare = await Enrich(prepare, token);
                return prepare;
            });
        }

        public virtual async Task<Tdto> DeleteAsync(Guid id, Guid userId, CancellationToken token)
        {
            return await ExecuteAsync(async (repo) =>
            {
                var entity = await repo.GetAsync(id, token);
                if (entity == null) throw new DataServiceException($"Entity with id = {id} not found in DB");
                if (!(await CheckUser(entity, userId))) throw new DataServiceException($"Entity with id = {id} not found in DB");
                await PrepareBeforeDelete(repo, entity, userId, token);
                entity = await repo.DeleteAsync(entity, false, token);
                await ActionAfterDelete(repo, entity, userId, token);
                await repo.SaveChangesAsync();
                var prepare = _mapper.Map<Tdto>(entity);
                prepare = await Enrich(prepare, token);
                return prepare;
            });
        }
    }

    public interface IGetDataService<Tdto, TFilter>
        where Tdto : Contract.Model.Entity
        where TFilter : Contract.Model.Filter<Tdto>
    {
        Task<Tdto> GetAsync(Guid id, Guid userId, CancellationToken token);
        Task<Contract.Model.PagedResult<Tdto>> GetAsync(TFilter filter, Guid userId, CancellationToken token);
    }

    public interface IAddDataService<Tdto, TCreator> where Tdto : Contract.Model.Entity
    {
        Task<Tdto> AddAsync(TCreator entity, Guid userId, CancellationToken token);
    }

    public interface IUpdateDataService<Tdto, TUpdater> where Tdto : Contract.Model.Entity
    {
        Task<Tdto> UpdateAsync(TUpdater entity, Guid userId, CancellationToken token);
    }

    public interface IDeleteDataService<Tdto> where Tdto : Contract.Model.Entity
    {
        Task<Tdto> DeleteAsync(Guid id, Guid userId, CancellationToken token);
    }

    public static class DataServiceExtension
    {
        public static IServiceCollection AddDataServices(this IServiceCollection services)
        {
            services.AddDataService<UserDataService, Db.Model.User, Contract.Model.User,
                Contract.Model.UserFilter, Contract.Model.UserCreator, Contract.Model.UserUpdater>();
            services.AddDataService<FormulaDataService, Db.Model.Formula, Contract.Model.Formula,
                Contract.Model.FormulaFilter, Contract.Model.FormulaCreator, Contract.Model.FormulaUpdater>();
            services.AddDataService<ProductDataService, Db.Model.Product, Contract.Model.Product,
                Contract.Model.ProductFilter, Contract.Model.ProductCreator, Contract.Model.ProductUpdater>();
            services.AddDataService<IncomingDataService, Db.Model.Incoming, Contract.Model.Incoming,
                Contract.Model.IncomingFilter, Contract.Model.IncomingCreator, Contract.Model.IncomingUpdater>();
            services.AddDataService<OutgoingDataService, Db.Model.Outgoing, Contract.Model.Outgoing,
                Contract.Model.OutgoingFilter, Contract.Model.OutgoingCreator, Contract.Model.OutgoingUpdater>();
            services.AddDataService<ReserveDataService, Db.Model.Reserve, Contract.Model.Reserve,
               Contract.Model.ReserveFilter, Contract.Model.ReserveCreator, Contract.Model.ReserveUpdater>();
            services.AddDataService<CorrectionDataService, Db.Model.Correction, Contract.Model.Correction,
               Contract.Model.CorrectionFilter, Contract.Model.CorrectionCreator, Contract.Model.CorrectionUpdater>();

            services.AddScoped<IGetDataService<Contract.Model.UserHistory, Contract.Model.UserHistoryFilter>, UserHistoryDataService>();
            services.AddScoped<IGetDataService<Contract.Model.FormulaHistory, Contract.Model.FormulaHistoryFilter>, FormulaHistoryDataService>();
            services.AddScoped<IGetDataService<Contract.Model.ProductHistory, Contract.Model.ProductHistoryFilter>, ProductHistoryDataService>();
            services.AddScoped<IGetDataService<Contract.Model.IncomingHistory, Contract.Model.IncomingHistoryFilter>, IncomingHistoryDataService>();
            services.AddScoped<IGetDataService<Contract.Model.OutgoingHistory, Contract.Model.OutgoingHistoryFilter>, OutgoingHistoryDataService>();
            services.AddScoped<IGetDataService<Contract.Model.ReserveHistory, Contract.Model.ReserveHistoryFilter>, ReserveHistoryDataService>();
            services.AddScoped<IGetDataService<Contract.Model.CorrectionHistory, Contract.Model.CorrectionHistoryFilter>, CorrectionHistoryDataService>();
            services.AddScoped<IAuthService, AuthService>();

            return services;
        }

        private static IServiceCollection AddDataService<TService, TEntity, Tdto, TFilter, TCreator, TUpdater>(this IServiceCollection services)
            where TEntity : Db.Model.Entity
            where TUpdater : Contract.Model.IEntity
            where TService : DataService<TEntity, Tdto, TFilter, TCreator, TUpdater>
            where Tdto : Contract.Model.Entity
            where TFilter : Contract.Model.Filter<Tdto>
        {
            services.AddScoped<IGetDataService<Tdto, TFilter>, TService>();
            services.AddScoped<IAddDataService<Tdto, TCreator>, TService>();
            services.AddScoped<IUpdateDataService<Tdto, TUpdater>, TService>();
            services.AddScoped<IDeleteDataService<Tdto>, TService>();
            return services;
        }
    }

    [Serializable]
    internal class DataServiceException : Exception
    {
        public DataServiceException()
        {
        }

        public DataServiceException(string message) : base(message)
        {
        }

        public DataServiceException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DataServiceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
