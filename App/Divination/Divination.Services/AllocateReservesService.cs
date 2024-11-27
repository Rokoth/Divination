using Divination.Contract.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Divination.Services
{
    public class AllocateReservesService : IAllocateReservesService
    {
        private ILogger<AllocateReservesHostedService> _logger;
        private IServiceProvider _serviceProvider;



        public AllocateReservesService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = serviceProvider.GetRequiredService<ILogger<AllocateReservesHostedService>>();            
        }

        public async Task Execute(CancellationToken token)
        {
            try
            {
                var _userDataService = _serviceProvider.GetRequiredService<IGetDataService<User, UserFilter>>();
                var _userUpdateService = _serviceProvider.GetRequiredService<IUpdateDataService<User, UserUpdater>>();
                var _reserveDataService = _serviceProvider.GetRequiredService<IAddDataService<Reserve, ReserveCreator>>();
                var allUsers = await _userDataService.GetAsync(new UserFilter(null, null, null, null), Guid.NewGuid(), token);
                foreach (var user in allUsers.Data)
                {
                    if (!user.LastAddedDate.HasValue || (user.LastAddedDate.Value.AddMinutes(user.AddPeriod) < DateTimeOffset.Now))
                    {
                        await _reserveDataService.AddAsync(new ReserveCreator(){}, user.Id, token);                        
                        await _userUpdateService.UpdateAsync(new UserUpdater() { 
                             Description = user.Description,
                             FormulaId = user.FormulaId,
                             Id = user.Id,
                             LastAddedDate = user.LastAddedDate.Value.AddMinutes(user.AddPeriod),
                             LeafOnly = user.LeafOnly,
                             Login = user.Login,
                             Name = user.Name,
                             PasswordChanged = false,
                             AddPeriod = user.AddPeriod,
                             DefaultReserveValue = user.DefaultReserveValue,
                             Email = user.Email                             
                        }, user.Id, token);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при резервировании : {ex.Message} {ex.StackTrace}");
            }
        }
    }
}
