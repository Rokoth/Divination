using Divination.Db.Model;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Divination.Services
{
    public class CorrectionHistoryDataService : DataGetService<Db.Model.CorrectionHistory, Contract.Model.CorrectionHistory,
       Contract.Model.CorrectionHistoryFilter>
    {
        public CorrectionHistoryDataService(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }

        protected override string DefaultSort => "Name";

        protected override Func<Db.Model.Filter<Db.Model.CorrectionHistory>, CancellationToken,
            Task<Contract.Model.PagedResult<Db.Model.CorrectionHistory>>> GetListFunc(Db.Interface.IRepository<Db.Model.CorrectionHistory> repo)
        {
            return repo.GetAsyncDeleted;
        }

        protected override Expression<Func<Db.Model.CorrectionHistory, bool>> GetFilter(Contract.Model.CorrectionHistoryFilter filter, Guid userId)
        {
            return s => (filter.Id == null || s.Id == filter.Id);
        }

        protected override async Task<bool> CheckUser(CorrectionHistory entity, Guid userId)
        {
            await Task.CompletedTask;
            return entity.UserId == userId;
        }
    }
}
