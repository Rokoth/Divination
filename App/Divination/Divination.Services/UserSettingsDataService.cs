using Divination.Contract.Model;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Divination.Services
{
    public class UserSettingsDataService : DataService<Db.Model.UserSettings, Contract.Model.UserSettings,
       Contract.Model.UserSettingsFilter, Contract.Model.UserSettingsCreator, Contract.Model.UserSettingsUpdater>
    {
        public UserSettingsDataService(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }

        protected override Expression<Func<Db.Model.UserSettings, bool>> GetFilter(Contract.Model.UserSettingsFilter filter, Guid userId)
        {
            return s => s.UserId == userId;
        }        
        protected override Db.Model.UserSettings UpdateFillFields(Contract.Model.UserSettingsUpdater entity, Db.Model.UserSettings entry)
        {           
            entry.DefaultReserveValue = entity.DefaultReserveValue;
            entry.LeafOnly = entity.LeafOnly;                       
            return entry;
        }

        protected override Db.Model.UserSettings AdditionalMapForAdd(Db.Model.UserSettings entity, UserSettingsCreator creator, Guid userId)
        {
            entity.UserId = userId;
            return entity;
        }

        protected override async Task<bool> CheckUser(Db.Model.UserSettings entity, Guid userId)
        {
            await Task.CompletedTask;
            return entity.UserId == userId;
        }

        protected override string DefaultSort => "Name";

    }
}
