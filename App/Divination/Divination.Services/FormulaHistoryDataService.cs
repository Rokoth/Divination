using Divination.Db.Model;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Divination.Services
{
    public class FormulaHistoryDataService : DataGetService<Db.Model.FormulaHistory, Contract.Model.FormulaHistory,
        Contract.Model.FormulaHistoryFilter>
    {
        public FormulaHistoryDataService(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }

        protected override string DefaultSort => "Name";

        protected override Func<Db.Model.Filter<Db.Model.FormulaHistory>, CancellationToken,
            Task<Contract.Model.PagedResult<Db.Model.FormulaHistory>>> GetListFunc(Db.Interface.IRepository<Db.Model.FormulaHistory> repo)
        {
            return repo.GetAsyncDeleted;
        }

        protected override Expression<Func<Db.Model.FormulaHistory, bool>> GetFilter(Contract.Model.FormulaHistoryFilter filter, Guid userId)
        {
            return s => (filter.Name == null || s.Name.Contains(filter.Name))
                && (filter.Id == null || s.Id == filter.Id);
        }

        protected override async Task<bool> CheckUser(FormulaHistory entity, Guid userId)
        {
            await Task.CompletedTask;
            return true;
        }
    }
}
