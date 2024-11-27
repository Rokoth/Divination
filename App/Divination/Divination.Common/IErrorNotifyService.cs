//Copyright 2021 Dmitriy Rokoth
//Licensed under the Apache License, Version 2.0
//
//ref1
using System.Threading.Tasks;

namespace Divination.Common
{
    /// <summary>
    /// Wrapper интеграции с сервисом сбора уведомлений об ошибках
    /// </summary>
    public interface IErrorNotifyService
    {        
        /// <summary>
        /// Отправить уведомление
        /// </summary>
        /// <param name="message">текст уведомления</param>
        /// <param name="level">Уровень сообщения</param>
        /// <param name="title">Заголовок (не обяз)</param>
        /// <returns></returns>
        Task Send(string message, MessageLevelEnum level = MessageLevelEnum.Error, string title = null);
    }
}