using Divination.Db.Model;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Divination.Services
{
    public class OutgoingHistoryDataService : DataGetService<Db.Model.OutgoingHistory, Contract.Model.OutgoingHistory,
        Contract.Model.OutgoingHistoryFilter>
    {
        public OutgoingHistoryDataService(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }

        protected override string DefaultSort => "Name";

        protected override Func<Db.Model.Filter<Db.Model.OutgoingHistory>, CancellationToken,
            Task<Contract.Model.PagedResult<Db.Model.OutgoingHistory>>> GetListFunc(Db.Interface.IRepository<Db.Model.OutgoingHistory> repo)
        {
            return repo.GetAsyncDeleted;
        }

        protected override Expression<Func<Db.Model.OutgoingHistory, bool>> GetFilter(Contract.Model.OutgoingHistoryFilter filter, Guid userId)
        {
            return s => (filter.Id == null || s.Id == filter.Id);
        }

        protected override async Task<bool> CheckUser(OutgoingHistory entity, Guid userId)
        {
            await Task.CompletedTask;
            return entity.UserId == userId;
        }
    }
}
