using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Divination.Services
{
    public class OutgoingDataService : DataService<Db.Model.Outgoing, Contract.Model.Outgoing,
       Contract.Model.OutgoingFilter, Contract.Model.OutgoingCreator, Contract.Model.OutgoingUpdater>
    {
        public OutgoingDataService(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }

        protected override Expression<Func<Db.Model.Outgoing, bool>> GetFilter(Contract.Model.OutgoingFilter filter, Guid userId)
        {
            return s => userId == s.UserId
               && (string.IsNullOrEmpty(filter.Description) || s.Description.Contains(filter.Description))
               && (filter.DateFrom == null || s.OutgoingDate >= filter.DateFrom.Value)
               && (filter.DateTo == null || s.OutgoingDate <= filter.DateTo.Value)
                && (filter.ProductId == null || s.ProductId == filter.ProductId.Value);
        }

        protected override async Task<Contract.Model.Outgoing> Enrich(Contract.Model.Outgoing entity, CancellationToken token)
        {
            var productDataService = _serviceProvider.GetRequiredService<IGetDataService<Contract.Model.Product, Contract.Model.ProductFilter>>();
            var product = await productDataService.GetAsync(entity.ProductId, entity.UserId, token);
            entity.Product = product.FullName;
            return entity;
        }

        protected override async Task<IEnumerable<Contract.Model.Outgoing>> Enrich(IEnumerable<Contract.Model.Outgoing> entities, CancellationToken token)
        {
            if (entities.Any())
            {
                List<Contract.Model.Outgoing> result = new List<Contract.Model.Outgoing>();
                var productDataService = _serviceProvider.GetRequiredService<IGetDataService<Contract.Model.Product, Contract.Model.ProductFilter>>();
                var products = await productDataService.GetAsync(new Contract.Model.ProductFilter(null, null, null, null, null, false, null, null), entities.First().UserId, token);
                foreach (var item in entities)
                {
                    var product = products.Data.First(s=>s.Id == item.ProductId);
                    item.Product = product.FullName;
                    result.Add(item);
                }
                return result;
            }
            return entities;
        }

        protected override async Task PrepareBeforeAdd(Db.Interface.IRepository<Db.Model.Outgoing> repository,
            Contract.Model.OutgoingCreator creator, Guid userId, CancellationToken token)
        {
            var reserveDataService = _serviceProvider.GetRequiredService<IAddDataService<Contract.Model.Reserve, Contract.Model.ReserveCreator>>();
            await reserveDataService.AddAsync(new Contract.Model.ReserveCreator() { 
               ProductId = creator.ProductId,              
               Value = -creator.Value
            }, userId, token);

            var productDataService = _serviceProvider.GetRequiredService<IGetDataService<Contract.Model.Product, Contract.Model.ProductFilter>>();
            var product = await productDataService.GetAsync(creator.ProductId, userId, token);
            if (product.MaxValue < creator.Value)
            {
                var productUpdateDataService = _serviceProvider.GetRequiredService<IUpdateDataService<Contract.Model.Product, Contract.Model.ProductUpdater>>();                
                await productUpdateDataService.UpdateAsync(new Contract.Model.ProductUpdater() {
                  AddPeriod = product.AddPeriod,
                  MaxValue = creator.Value,
                  Description = product.Description,
                  Id = product.Id,
                  MinValue = product.MinValue,
                  Name = product.Name,
                  ParentId = product.ParentId
                }, userId, token);
            }
        }

        protected override async Task PrepareBeforeUpdate(Db.Interface.IRepository<Db.Model.Outgoing> repository,
            Contract.Model.OutgoingUpdater entity, Guid userId, CancellationToken token)
        {
            var reserveDataService = _serviceProvider.GetRequiredService<IAddDataService<Contract.Model.Reserve, Contract.Model.ReserveCreator>>();
            var currentEntity = await repository.GetAsync(entity.Id, token);
            if (currentEntity.ProductId != entity.ProductId)
            {
                await reserveDataService.AddAsync(new Contract.Model.ReserveCreator()
                {
                    ProductId = currentEntity.ProductId,                   
                    Value = currentEntity.Value
                }, userId, token);
                await reserveDataService.AddAsync(new Contract.Model.ReserveCreator()
                {
                    ProductId = entity.ProductId,                   
                    Value = -entity.Value
                }, userId, token);
                return;
            }

            if (currentEntity.Value != entity.Value)
            {
                await reserveDataService.AddAsync(new Contract.Model.ReserveCreator()
                {
                    ProductId = currentEntity.ProductId,                    
                    Value = currentEntity.Value - entity.Value
                }, userId, token);               
            }

            var productDataService = _serviceProvider.GetRequiredService<IGetDataService<Contract.Model.Product, Contract.Model.ProductFilter>>();
            var product = await productDataService.GetAsync(entity.ProductId, userId, token);
            if (product.MaxValue < entity.Value)
            {
                var productUpdateDataService = _serviceProvider.GetRequiredService<IUpdateDataService<Contract.Model.Product, Contract.Model.ProductUpdater>>();
                await productUpdateDataService.UpdateAsync(new Contract.Model.ProductUpdater()
                {
                    AddPeriod = product.AddPeriod,
                    MaxValue = entity.Value,
                    Description = product.Description,
                    Id = product.Id,
                    MinValue = product.MinValue,
                    Name = product.Name,
                    ParentId = product.ParentId
                }, userId, token);
            }
        }

        protected override async Task PrepareBeforeDelete(Db.Interface.IRepository<Db.Model.Outgoing> repository,
            Db.Model.Outgoing entity, Guid userId, CancellationToken token)
        {
            var reserveDataService = _serviceProvider.GetRequiredService<IAddDataService<Contract.Model.Reserve, Contract.Model.ReserveCreator>>();
            var currentEntity = await repository.GetAsync(entity.Id, token);
            await reserveDataService.AddAsync(new Contract.Model.ReserveCreator()
            {
                ProductId = currentEntity.ProductId,               
                Value = currentEntity.Value
            }, userId, token);
        }

        protected override Db.Model.Outgoing UpdateFillFields(Contract.Model.OutgoingUpdater entity, Db.Model.Outgoing entry)
        {
            entry.Description = entity.Description;
            entry.OutgoingDate = entity.OutgoingDate;
            entry.ProductId = entity.ProductId;
            entry.Value = entity.Value;
            return entry;
        }

        protected override Db.Model.Outgoing AdditionalMapForAdd(Db.Model.Outgoing entity, Contract.Model.OutgoingCreator creator, Guid userId)
        {
            entity.UserId = userId;
            return entity;
        }

        protected override async Task<bool> CheckUser(Db.Model.Outgoing entity, Guid userId)
        {
            await Task.CompletedTask;
            return entity.UserId == userId;
        }

        protected override string DefaultSort => "OutgoingDate";

    }
}
