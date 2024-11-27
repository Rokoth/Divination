using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Divination.Services
{
    public class ProductDataService : DataService<Db.Model.Product, Contract.Model.Product,
       Contract.Model.ProductFilter, Contract.Model.ProductCreator, Contract.Model.ProductUpdater>
    {
        public ProductDataService(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }

        protected override Expression<Func<Db.Model.Product, bool>> GetFilter(Contract.Model.ProductFilter filter, Guid userId)
        {
            return s => (filter.Name == null || s.Name.ToLower().Contains(filter.Name.ToLower())) 
                     && (filter.LastAddDateFrom == null || s.LastAddDate >= filter.LastAddDateFrom)
                      && (filter.LastAddDateTo == null || s.LastAddDate < filter.LastAddDateTo)
                      && (!filter.LeafOnly || s.IsLeaf )
                      && (filter.ParentId == null || s.ParentId == filter.ParentId)
                      && (userId == s.UserId);
        }

        protected override async Task<Contract.Model.Product> Enrich(Contract.Model.Product entity, CancellationToken token)
        {
            entity.FullName = await GetFullName(entity.Id, token);
            entity.Reserve = await GetReserve(entity.Id, token);
            return entity;
        }

        protected override async Task<IEnumerable<Contract.Model.Product>> Enrich(IEnumerable<Contract.Model.Product> entities, CancellationToken token)
        {
            if (entities.Any())
            {
                List<Contract.Model.Product> result = new List<Contract.Model.Product>();                            
                foreach (var item in entities)
                {                    
                    item.FullName = await GetFullName(item.Id, token);
                    item.Reserve = await GetReserve(item.Id, token);
                    result.Add(item);
                }
                return result;
            }
            return entities;
        }

        private async Task<decimal> GetReserve(Guid productId, CancellationToken token)
        {
            var repo = _serviceProvider.GetRequiredService<Db.Interface.IRepository<Db.Model.Reserve>>();
            var reserve = (await repo.GetAsync(new Db.Model.Filter<Db.Model.Reserve>() { 
              Page = 0, Selector = s=>s.ProductId == productId, Size = 10, Sort = null
            }, token)).Data.FirstOrDefault();
            return reserve?.Value ?? 0;
        }

        private async Task<string> GetFullName(Guid productId, CancellationToken token)
        {
            var repo = _serviceProvider.GetRequiredService<Db.Interface.IRepository<Db.Model.Product>>();
            var product = await repo.GetAsync(productId, token);
            if (product.ParentId == null) return product.Name;
            return (await GetFullName(product.ParentId.Value, token)) + "/" +  product.Name;
        }

        protected override Db.Model.Product UpdateFillFields(Contract.Model.ProductUpdater entity, Db.Model.Product entry)
        {           
            entry.AddPeriod = entity.AddPeriod;
            entry.Name = entity.Name;
            entry.Description = entity.Description;            
            entry.MaxValue = entity.MaxValue;
            entry.MinValue = entity.MinValue;
            entry.ParentId = entity.ParentId;        
            return entry;
        }

        protected override string DefaultSort => "Name";       

        protected override async Task PrepareBeforeUpdate(Db.Interface.IRepository<Db.Model.Product> repository, Contract.Model.ProductUpdater entity, Guid userId, CancellationToken token)
        {
            var currentEntity = await repository.GetAsync(entity.Id, token);
            if (currentEntity.ParentId!= entity.ParentId && currentEntity.ParentId.HasValue)
            {
                var otherChilds = await repository.GetAsync(new Db.Model.Filter<Db.Model.Product>() {
                  Selector = s=>s.ParentId == currentEntity.ParentId && s.Id != currentEntity.Id
                }, token);

                if (!otherChilds.Data.Any())
                {
                    var currentParent = await repository.GetAsync(currentEntity.ParentId.Value, token);
                    currentParent.IsLeaf = true;
                    currentParent.VersionDate = DateTime.Now;
                    await repository.UpdateAsync(currentParent, false, token);
                }
            }
        }

        protected override async Task PrepareBeforeDelete(Db.Interface.IRepository<Db.Model.Product> repository, Db.Model.Product entity, Guid userId, CancellationToken token)
        {
            var currentEntity = await repository.GetAsync(entity.Id, token);
            if (currentEntity.ParentId.HasValue)
            {
                var otherChilds = await repository.GetAsync(new Db.Model.Filter<Db.Model.Product>()
                {
                    Selector = s => s.ParentId == currentEntity.ParentId && s.Id != currentEntity.Id
                }, token);

                if (!otherChilds.Data.Any())
                {
                    var currentParent = await repository.GetAsync(currentEntity.ParentId.Value, token);
                    currentParent.IsLeaf = true;
                    currentParent.VersionDate = DateTime.Now;
                    await repository.UpdateAsync(currentParent, false, token);
                }
            }
        }

        protected override async Task ActionAfterAdd(Db.Interface.IRepository<Db.Model.Product> repository, 
            Contract.Model.ProductCreator creator, Db.Model.Product entity, Guid userId, CancellationToken token)
        {
            if (entity.ParentId.HasValue)
            {
                var parent = await repository.GetAsync(entity.ParentId.Value, token);
                if (parent.IsLeaf)
                {
                    parent.IsLeaf = false;
                    parent.VersionDate = DateTime.Now;
                    await repository.UpdateAsync(parent, false, token);
                }
            }
        }

        protected override async Task ActionAfterUpdate(Db.Interface.IRepository<Db.Model.Product> repository, 
            Contract.Model.ProductUpdater updater, Db.Model.Product entity, Guid userId, CancellationToken token)
        {
            if (entity.ParentId.HasValue)
            {
                var parent = await repository.GetAsync(entity.ParentId.Value, token);
                if (parent.IsLeaf)
                {
                    parent.IsLeaf = false;
                    parent.VersionDate = DateTime.Now;
                    await repository.UpdateAsync(parent, false, token);
                }
            }
        }

        protected override Db.Model.Product AdditionalMapForAdd(Db.Model.Product entity, Contract.Model.ProductCreator creator, Guid userId)
        {
            entity.UserId = userId;
            entity.IsLeaf = true;
            entity.LastAddDate = DateTimeOffset.Now;
            return entity;
        }

        protected override async Task<bool> CheckUser(Db.Model.Product entity, Guid userId)
        {
            await Task.CompletedTask;
            return entity.UserId == userId;
        }
    }
}
