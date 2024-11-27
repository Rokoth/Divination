using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Divination.Services
{
    public class CorrectionDataService : DataService<Db.Model.Correction, Contract.Model.Correction,
       Contract.Model.CorrectionFilter, Contract.Model.CorrectionCreator, Contract.Model.CorrectionUpdater>
    {

        public CorrectionDataService(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }

        protected override Expression<Func<Db.Model.Correction, bool>> GetFilter(Contract.Model.CorrectionFilter filter, Guid userId)
        {
            return s => userId == s.UserId
               && (string.IsNullOrEmpty(filter.Description) || s.Description.Contains(filter.Description))
               && (filter.DateFrom == null || s.CorrectionDate >= filter.DateFrom.Value)
               && (filter.DateTo == null || s.CorrectionDate <= filter.DateTo.Value);
        }

        public override async Task<Contract.Model.Correction> AddAsync(Contract.Model.CorrectionCreator creator, Guid userId, CancellationToken token)
        {
            return await ExecuteAsync(async (repo) =>
            {               
                if (creator.Value.HasValue)
                {
                    Db.Model.Correction correction = new Db.Model.Correction() { 
                      CorrectionDate = DateTimeOffset.Now,
                      Description = creator.Description,
                      Id = Guid.NewGuid(),
                      IsDeleted = false,
                      UserId = userId,
                      Value = creator.Value.Value,
                      VersionDate = DateTimeOffset.Now
                    };

                    await repo.AddAsync(correction, true, token);
                    var prepare = _mapper.Map<Contract.Model.Correction>(correction);
                    prepare = await Enrich(prepare, token);
                    return prepare;
                }

                if (creator.TotalValue.HasValue)
                {
                    var _incomingRepo = _serviceProvider.GetRequiredService<Db.Interface.IRepository<Db.Model.Incoming>>();
                    var _outgoingRepo = _serviceProvider.GetRequiredService<Db.Interface.IRepository<Db.Model.Outgoing>>();           

                    var incomings = await _incomingRepo.GetAsync(new Db.Model.Filter<Db.Model.Incoming>() {
                      Selector = s=>s.UserId == userId
                    }, token);
                    var outgoings = await _outgoingRepo.GetAsync(new Db.Model.Filter<Db.Model.Outgoing>() {
                        Selector = s => s.UserId == userId
                    }, token);                 
                    var corrections = await repo.GetAsync(new Db.Model.Filter<Db.Model.Correction>() {
                        Selector = s => s.UserId == userId
                    }, token);
                                        
                    var correctValue = creator.TotalValue.Value - 
                        (   incomings.Data.Sum(s=>s.Value) 
                            + corrections.Data.Sum(s => s.Value)
                            - outgoings.Data.Sum(s => s.Value));

                    if (correctValue != 0)
                    {
                        Db.Model.Correction correction = new Db.Model.Correction()
                        {
                            CorrectionDate = DateTimeOffset.Now,
                            Description = creator.Description,
                            Id = Guid.NewGuid(),
                            IsDeleted = false,
                            UserId = userId,
                            Value = correctValue,
                            VersionDate = DateTimeOffset.Now
                        };

                        await repo.AddAsync(correction, true, token);
                        var prepare = _mapper.Map<Contract.Model.Correction>(correction);
                        prepare = await Enrich(prepare, token);
                        return prepare;
                    }
                }

                return null;

            });
        }

        public override async Task<Contract.Model.Correction> UpdateAsync(Contract.Model.CorrectionUpdater creator, Guid userId, CancellationToken token)
        {
            throw new DataServiceException("Операция Update для корректировок недопустима");
        }

        public override async Task<Contract.Model.Correction> DeleteAsync(Guid id, Guid userId, CancellationToken token)
        {
            throw new DataServiceException("Операция Delete для корректировок недопустима");
        }

        protected override Db.Model.Correction UpdateFillFields(Contract.Model.CorrectionUpdater entity, Db.Model.Correction entry)
        {
            return entry;
        }

        protected override Db.Model.Correction AdditionalMapForAdd(Db.Model.Correction entity, Contract.Model.CorrectionCreator creator, Guid userId)
        {
            entity.UserId = userId;
            return entity;
        }

        protected override async Task<bool> CheckUser(Db.Model.Correction entity, Guid userId)
        {
            await Task.CompletedTask;
            return entity.UserId == userId;
        }

        protected override string DefaultSort => "CorrectionDate";

    }
}
