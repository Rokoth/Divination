using System.Collections.Generic;

namespace Divination.Services
{
    /// <summary>
    /// Библиотека выбора элемента по формуле
    /// </summary>
    public interface ICalculator
    {
        /// <summary>
        /// Метод расчета по формуле
        /// </summary>
        /// <param name="request">Запрос расчета</param>
        /// <returns></returns>
        IEnumerable<CalcResult> Calculate(CalcRequest request);
    }
}