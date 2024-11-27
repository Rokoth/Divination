using Divination.Db.Model;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Divination.Services
{
    public class ReserveHistoryDataService : DataGetService<Db.Model.ReserveHistory, Contract.Model.ReserveHistory,
        Contract.Model.ReserveHistoryFilter>
    {
        public ReserveHistoryDataService(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }

        protected override string DefaultSort => "Name";

        protected override Func<Db.Model.Filter<Db.Model.ReserveHistory>, CancellationToken,
            Task<Contract.Model.PagedResult<Db.Model.ReserveHistory>>> GetListFunc(Db.Interface.IRepository<Db.Model.ReserveHistory> repo)
        {
            return repo.GetAsyncDeleted;
        }

        protected override Expression<Func<Db.Model.ReserveHistory, bool>> GetFilter(Contract.Model.ReserveHistoryFilter filter, Guid userId)
        {
            return s => (filter.Id == null || s.Id == filter.Id);
        }

        protected override async Task<bool> CheckUser(ReserveHistory entity, Guid userId)
        {
            await Task.CompletedTask;
            return entity.UserId == userId;
        }
    }
}
