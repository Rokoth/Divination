using Divination.Db.Model;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Divination.Services
{
    public class UserHistoryDataService : DataGetService<Db.Model.UserHistory, Contract.Model.UserHistory,
        Contract.Model.UserHistoryFilter>
    {
        public UserHistoryDataService(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }

        protected override string DefaultSort => "Name";

        protected override Func<Db.Model.Filter<Db.Model.UserHistory>, CancellationToken, 
            Task<Contract.Model.PagedResult<Db.Model.UserHistory>>> GetListFunc(Db.Interface.IRepository<Db.Model.UserHistory> repo)
        {
            return repo.GetAsyncDeleted;
        }

        protected override Expression<Func<Db.Model.UserHistory, bool>> GetFilter(Contract.Model.UserHistoryFilter filter, Guid userId)
        {
            return s => (filter.Name == null || s.Name.Contains(filter.Name)) 
                && (filter.Id == null || s.Id == filter.Id);
        }

        protected override async Task<bool> CheckUser(UserHistory entity, Guid userId)
        {
            await Task.CompletedTask;
            return entity.Id == userId;
        }
    }
}
