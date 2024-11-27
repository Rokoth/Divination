using Divination.Contract.Model;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Divination.Services
{
    public class IncomingDataService : DataService<Db.Model.Incoming, Contract.Model.Incoming,
       Contract.Model.IncomingFilter, Contract.Model.IncomingCreator, Contract.Model.IncomingUpdater>
    {
        public IncomingDataService(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }

        protected override Expression<Func<Db.Model.Incoming, bool>> GetFilter(Contract.Model.IncomingFilter filter, Guid userId)
        {
            return s => userId == s.UserId
                && (string.IsNullOrEmpty(filter.Description) || s.Description.Contains(filter.Description))
                && (filter.DateFrom == null || s.IncomingDate >= filter.DateFrom.Value)
                && (filter.DateTo == null || s.IncomingDate <= filter.DateTo.Value);
        }
              

        protected override Db.Model.Incoming UpdateFillFields(Contract.Model.IncomingUpdater entity, Db.Model.Incoming entry)
        {
            entry.Description = entity.Description;
            entry.IncomingDate = entity.IncomingDate;
            entry.Value = entity.Value;
            return entry;
        }

        protected override Db.Model.Incoming AdditionalMapForAdd(Db.Model.Incoming entity, IncomingCreator creator, Guid userId)
        {
            entity.UserId = userId;
            return entity;
        }

        protected override async Task<bool> CheckUser(Db.Model.Incoming entity, Guid userId)
        {
            await Task.CompletedTask;
            return entity.UserId == userId;
        }

        protected override string DefaultSort => "IncomingDate";

    }
}
