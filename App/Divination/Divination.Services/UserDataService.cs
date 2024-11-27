using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Divination.Services
{
    public class UserDataService : DataService<Db.Model.User, Contract.Model.User,
        Contract.Model.UserFilter, Contract.Model.UserCreator, Contract.Model.UserUpdater>
    {
        public UserDataService(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }

        /// <summary>
        /// function for enrichment data item
        /// </summary>
        protected override async Task<Contract.Model.User> Enrich(Contract.Model.User entity, CancellationToken token)
        {
            var userSettingsRepo = _serviceProvider.GetRequiredService<Db.Interface.IRepository<Db.Model.UserSettings>>();
            var formulaRepo = _serviceProvider.GetRequiredService<Db.Interface.IRepository<Db.Model.Formula>>();
            var userSettings = (await userSettingsRepo.GetAsync(new Db.Model.Filter<Db.Model.UserSettings>()
            {
                Selector = s => s.UserId == entity.Id
            }, token)).Data.Single();
            var formula = (await formulaRepo.GetAsync(new Db.Model.Filter<Db.Model.Formula>()
            {
                Selector = s => s.Id == entity.FormulaId
            }, token)).Data.Single();
            entity.Formula = formula.Name;           
            entity.LeafOnly = userSettings.LeafOnly;
            entity.DefaultReserveValue = userSettings.DefaultReserveValue;
            entity.AddPeriod = userSettings.AddPeriod;

            return entity;
        }

        /// <summary>
        /// function for enrichment data item
        /// </summary>
        protected override async Task<IEnumerable<Contract.Model.User>> Enrich(IEnumerable<Contract.Model.User> entities, CancellationToken token)
        {
            var result = new List<Contract.Model.User>();
            var userSettingsRepo = _serviceProvider.GetRequiredService<Db.Interface.IRepository<Db.Model.UserSettings>>();
            var formulaRepo = _serviceProvider.GetRequiredService<Db.Interface.IRepository<Db.Model.Formula>>();
            var userIds = entities.Select(s => s.Id).ToList();
            var formulaIds = entities.Select(s => s.FormulaId).Distinct().ToList();
            var userSettingss = (await userSettingsRepo.GetAsync(new Db.Model.Filter<Db.Model.UserSettings>()
            {
                Selector = s => userIds.Contains(s.UserId)
            }, token)).Data.ToList();
            var formulas = (await formulaRepo.GetAsync(new Db.Model.Filter<Db.Model.Formula>()
            {
                Selector = s => formulaIds.Contains(s.Id)
            }, token)).Data.ToList();

            foreach (var entity in entities)
            {
                var formula = formulas.Where(s=>s.Id == entity.FormulaId).Single();
                var userSettings = userSettingss.Where(s => s.UserId == entity.Id).Single();
                entity.Formula = formula.Name;                
                entity.LeafOnly = userSettings.LeafOnly;
                entity.DefaultReserveValue = userSettings.DefaultReserveValue;
                entity.AddPeriod = userSettings.AddPeriod;
                result.Add(entity);
            }

            return result;
        }

        protected override Expression<Func<Db.Model.User, bool>> GetFilter(Contract.Model.UserFilter filter, Guid userId)
        {
            return s => filter.Name == null || s.Name.Contains(filter.Name);
        }

        protected override async Task ActionAfterAdd(Db.Interface.IRepository<Db.Model.User> repository,
            Contract.Model.UserCreator creator, Db.Model.User entity, Guid userId, CancellationToken token)
        {
            var userSettingsRepo = _serviceProvider.GetRequiredService<Db.Interface.IRepository<Db.Model.UserSettings>>();
            await userSettingsRepo.AddAsync(new Db.Model.UserSettings() {               
               Id = Guid.NewGuid(),
               IsDeleted = false,
               LeafOnly = creator.LeafOnly,              
               UserId = entity.Id,
               VersionDate = DateTimeOffset.Now
            }, false, token);
        }

        protected override async Task ActionAfterUpdate(Db.Interface.IRepository<Db.Model.User> repository,
            Contract.Model.UserUpdater updater, Db.Model.User entity, Guid userId, CancellationToken token)
        {
            var userSettingsRepo = _serviceProvider.GetRequiredService<Db.Interface.IRepository<Db.Model.UserSettings>>();
            var userSettings = (await userSettingsRepo.GetAsync(new Db.Model.Filter<Db.Model.UserSettings>()
            { 
               Selector = s=>s.UserId == entity.Id
            }, token)).Data.Single();

            userSettings.DefaultReserveValue = updater.DefaultReserveValue;
            userSettings.AddPeriod = updater.AddPeriod;
            userSettings.LeafOnly = updater.LeafOnly;           

            await userSettingsRepo.UpdateAsync(userSettings, false, token);
        }

        protected override async Task ActionAfterDelete(Db.Interface.IRepository<Db.Model.User> repository,
            Db.Model.User entity, Guid userId, CancellationToken token)
        {
            var userSettingsRepo = _serviceProvider.GetRequiredService<Db.Interface.IRepository<Db.Model.UserSettings>>();
            var userSettings = (await userSettingsRepo.GetAsync(new Db.Model.Filter<Db.Model.UserSettings>()
            {
                Selector = s => s.UserId == entity.Id
            }, token)).Data.Single();                       

            await userSettingsRepo.DeleteAsync(userSettings, false, token);
        }

        protected override Db.Model.User UpdateFillFields(Contract.Model.UserUpdater entity, Db.Model.User entry)
        {
            entry.Description = entity.Description;
            entry.Login = entity.Login;
            entry.Name = entity.Name;
            entry.Email = entity.Email;
            if (entity.LastAddedDate.HasValue) entry.LastAddedDate = entity.LastAddedDate.Value;
            if (entity.PasswordChanged)
            {
                entry.Password = SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes(entity.Password));
            }
            return entry;
        }

        protected override Db.Model.User AdditionalMapForAdd(Db.Model.User entity, Contract.Model.UserCreator creator, Guid userId)
        {
            entity.Password = SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes(creator.Password));
            return entity;
        }

        protected override async Task<bool> CheckUser(Db.Model.User entity, Guid userId)
        {
            await Task.CompletedTask;
            return entity.Id == userId;
        }

        protected override string DefaultSort => "Name";
        
    }
}
