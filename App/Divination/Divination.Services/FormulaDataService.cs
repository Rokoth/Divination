using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Divination.Services
{
    public class FormulaDataService : DataService<Db.Model.Formula, Contract.Model.Formula,
       Contract.Model.FormulaFilter, Contract.Model.FormulaCreator, Contract.Model.FormulaUpdater>
    {
        public FormulaDataService(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }

        protected override Expression<Func<Db.Model.Formula, bool>> GetFilter(Contract.Model.FormulaFilter filter, Guid userId)
        {
            return s => (filter.Name == null || s.Name.Contains(filter.Name)) 
                     && (filter.IsDefault == null || s.IsDefault == filter.IsDefault);
        }

        protected override async Task PrepareBeforeAdd(Db.Interface.IRepository<Db.Model.Formula> repository, 
            Contract.Model.FormulaCreator creator, Guid userId, CancellationToken token)
        {
            if (creator.IsDefault)
            {
                var currentDefaults = await repository.GetAsync(new Db.Model.Filter<Db.Model.Formula>()
                {
                    Page = 0,
                    Size = 10,
                    Selector = s => s.IsDefault
                }, token);
                foreach (var item in currentDefaults.Data)
                {
                    item.IsDefault = false;
                    await repository.UpdateAsync(item, false,  token);
                }
            }
        }

        protected override async Task PrepareBeforeUpdate(Db.Interface.IRepository<Db.Model.Formula> repository, 
            Contract.Model.FormulaUpdater entity, Guid userId, CancellationToken token)
        {
            if (entity.IsDefault)
            {
                var currentDefaults = await repository.GetAsync(new Db.Model.Filter<Db.Model.Formula>()
                {
                    Page = 0,
                    Size = 10,
                    Selector = s => s.IsDefault && s.Id != entity.Id
                }, token);
                foreach (var item in currentDefaults.Data)
                {
                    item.IsDefault = false;
                    await repository.UpdateAsync(item, false, token);
                }
            }
        }

        protected override async Task PrepareBeforeDelete(Db.Interface.IRepository<Db.Model.Formula> repository,
            Db.Model.Formula entity, Guid userId, CancellationToken token)
        {
            if (entity.IsDefault)
            {
                var currentDefault = (await repository.GetAsync(new Db.Model.Filter<Db.Model.Formula>()
                {
                    Page = 0,
                    Size = 10,
                    Selector = s => true
                }, token)).Data.FirstOrDefault();
                currentDefault.IsDefault = true;
                await repository.UpdateAsync(currentDefault, false, token);
            }
        }

        protected override Db.Model.Formula UpdateFillFields(Contract.Model.FormulaUpdater entity, Db.Model.Formula entry)
        {
            entry.Text = entity.Text;
            entry.Name = entity.Name;
            entry.IsDefault = entity.IsDefault;
            return entry;
        }

        protected override Db.Model.Formula AdditionalMapForAdd(Db.Model.Formula entity, Contract.Model.FormulaCreator creator, Guid userId)
        {
            return entity;
        }

        protected override async Task<bool> CheckUser(Db.Model.Formula entity, Guid userId)
        {
            await Task.CompletedTask;
            return true;
        }

        protected override string DefaultSort => "Name";

    }
}
