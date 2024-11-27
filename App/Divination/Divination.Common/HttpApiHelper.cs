//Copyright 2021 Dmitriy Rokoth
//Licensed under the Apache License, Version 2.0
//
//ref2

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Divination.Common
{

    /// <summary>
    /// Методы сериализации-десериализации для http запросов
    /// </summary>
    public static class HttpApiHelper
    {
        /// <summary>
        /// Сериализация запроса
        /// </summary>
        /// <typeparam name="TReq"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static StringContent SerializeRequest<TReq>(this TReq entity)
        {
            var json = JsonConvert.SerializeObject(entity);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            return data;
        }

        /// <summary>
        /// Десериализация ответа
        /// </summary>
        /// <typeparam name="TResp"></typeparam>
        /// <param name="result"></param>
        /// <returns></returns>
        public static async Task<Response<TResp>> ParseResponse<TResp>(this HttpResponseMessage result) where TResp : class
        {
            if (result != null && result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();
                return new Response<TResp>()
                { 
                    ResponseCode = ResponseEnum.OK,
                    ResponseBody = JObject.Parse(response).ToObject<TResp>()
                };
            }
            if (result != null && result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return new Response<TResp>()
                {
                    ResponseCode = ResponseEnum.NeedAuth
                };
            }
            return new Response<TResp>()
            {
                ResponseCode = ResponseEnum.Error
            };
        }

        /// <summary>
        /// Десериализация ответа (массив)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <returns></returns>
        public static async Task<Response<IEnumerable<T>>> ParseResponseArray<T>(this HttpResponseMessage result) where T : class
        {
            if (result != null && result.IsSuccessStatusCode)
            {
                var ret = new List<T>();
                var response = await result.Content.ReadAsStringAsync();
                foreach (var item in JArray.Parse(response))
                {
                    ret.Add(item.ToObject<T>());
                }
                return new Response<IEnumerable<T>>()
                {
                    ResponseCode = ResponseEnum.OK,
                    ResponseBody = ret
                };
            }
            if (result != null && result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return new Response<IEnumerable<T>>()
                {
                    ResponseCode = ResponseEnum.NeedAuth
                };
            }
            return new Response<IEnumerable<T>>()
            {
                ResponseCode = ResponseEnum.Error
            };
        }
    }
}