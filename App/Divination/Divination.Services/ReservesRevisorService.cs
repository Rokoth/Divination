using Divination.Contract.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Divination.Services
{
    public class ReservesRevisorService : IReservesRevisorService
    {       
        
        private readonly ILogger<AllocateReservesHostedService> _logger;
        private readonly IServiceProvider serviceProvider;

        public ReservesRevisorService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;           
            _logger = serviceProvider.GetRequiredService<ILogger<AllocateReservesHostedService>>();
        }

        public async Task CheckReserveValues(CancellationToken token)
        {
            try
            {
                var _userDataService = serviceProvider.GetRequiredService<IGetDataService<User, UserFilter>>();
                var _reserveAddDataService = serviceProvider.GetRequiredService<IAddDataService<Reserve, ReserveCreator>>();
                var _reserveDataService = serviceProvider.GetRequiredService<IGetDataService<Reserve, ReserveFilter>>();
                var _productDataService = serviceProvider.GetRequiredService<IGetDataService<Product, ProductFilter>>();
                var _incomingDataService = serviceProvider.GetRequiredService<IGetDataService<Incoming, IncomingFilter>>();
                var _outgoingDataService = serviceProvider.GetRequiredService<IGetDataService<Outgoing, OutgoingFilter>>();
                var _correctionDataService = serviceProvider.GetRequiredService<IGetDataService<Correction, CorrectionFilter>>();
                var allUsers = await _userDataService.GetAsync(new UserFilter(null, null, null, null), Guid.NewGuid(), token);
                foreach (var user in allUsers.Data)
                {
                    var reserves = await _reserveDataService.GetAsync(new ReserveFilter(null, null, null, null), user.Id, token);
                    var products = await _productDataService.GetAsync(new ProductFilter(null, null, null, null, null, false, null, null), user.Id, token);
                    foreach (var reserve in reserves.Data)
                    {
                        var product = products.Data.Single(s => s.Id == reserve.ProductId);
                        if (reserve.Value > product.MaxValue)
                        {
                            await _reserveAddDataService.AddAsync(new ReserveCreator()
                            {
                                ProductId = product.Id,                               
                                Value = product.MaxValue - reserve.Value
                            }, user.Id, token);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при ревизии резервов в методе CheckReserveValues: {ex.Message} {ex.StackTrace}");
            }
        }

        public async Task CheckSum(CancellationToken token)
        {
            try
            {
                var _userDataService = serviceProvider.GetRequiredService<IGetDataService<User, UserFilter>>();
                var _reserveAddDataService = serviceProvider.GetRequiredService<IAddDataService<Reserve, ReserveCreator>>();
                var _reserveDataService = serviceProvider.GetRequiredService<IGetDataService<Reserve, ReserveFilter>>();
                var _productDataService = serviceProvider.GetRequiredService<IGetDataService<Product, ProductFilter>>();
                var _incomingDataService = serviceProvider.GetRequiredService<IGetDataService<Incoming, IncomingFilter>>();
                var _outgoingDataService = serviceProvider.GetRequiredService<IGetDataService<Outgoing, OutgoingFilter>>();
                var _correctionDataService = serviceProvider.GetRequiredService<IGetDataService<Correction, CorrectionFilter>>();
                var allUsers = await _userDataService.GetAsync(new UserFilter(null, null, null, null), Guid.NewGuid(), token);
                foreach (var user in allUsers.Data)
                {
                    var reserves = await _reserveDataService.GetAsync(new ReserveFilter(null, null, null, null), user.Id, token);                    
                    var incomings = await _incomingDataService.GetAsync(new IncomingFilter(null, null, null, null, null, null), user.Id, token);
                    var outgoings = await _outgoingDataService.GetAsync(new OutgoingFilter(null, null, null,  null, null, null, null), user.Id, token);
                    var corrections = await _correctionDataService.GetAsync(new CorrectionFilter(null, null, null, null, null, null), user.Id, token);

                    var reserveSum = reserves.Data.Sum(s => s.Value);
                    var correctionSum = corrections.Data.Sum(s => s.Value);
                    var outgoingsSum = outgoings.Data.Sum(s => s.Value);
                    var incomingsSum = incomings.Data.Sum(s => s.Value);

                    var allSum = incomingsSum
                            + correctionSum
                            - reserveSum
                            - outgoingsSum;

                    var reservesToCorrect = reserves.Data.Where(s => s.Value > 0).ToList();

                    while (allSum < 0)
                    {                        
                        var minReserve = reservesToCorrect.Min(s => s.Value);
                        var avgSum = -allSum / reservesToCorrect.Count;

                        if (minReserve < avgSum)
                        {
                            allSum += minReserve * reservesToCorrect.Count;
                            foreach (var reserve in reservesToCorrect)
                            {
                                reserve.Value -= minReserve;
                                await _reserveAddDataService.AddAsync(new ReserveCreator()
                                {
                                    ProductId = reserve.ProductId,                                  
                                    Value = -minReserve
                                }, user.Id, token);
                            }
                            reservesToCorrect = reservesToCorrect.Where(s => s.Value > 0).ToList();
                        }
                        else
                        {
                            allSum = 0;
                            foreach (var reserve in reservesToCorrect)
                            {
                                reserve.Value -= avgSum;
                                await _reserveAddDataService.AddAsync(new ReserveCreator() { 
                                  ProductId = reserve.ProductId,                                   
                                  Value = -avgSum
                                }, user.Id, token);
                            }
                        }                        
                    }                    
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при ревизии резервов в методе CheckSum: {ex.Message} {ex.StackTrace}");
            }
        }
    }
}
