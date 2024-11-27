using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NCalc2;
using Newtonsoft.Json.Linq;

namespace Divination.Services
{
    public class CalculatorNCalc : ICalculator
    {

        public IEnumerable<CalcResult> Calculate(CalcRequest request)
        {
            List<CalcResult> result = new List<CalcResult>();

            for (int i = 0; i < request.Count; i++)
            {
                var res = CalculateInternal(request);
                if(res!=null)
                    request.ChangeOnSelect?.Invoke(res.Id);
                result.Add(res);
            }

            return result;
        }

        private CalcResult CalculateInternal(CalcRequest request)
        {          
            Guid? id = null;            
            Expression expression = new Expression(request.Formula);
            expression.EvaluateFunction += NCalcExtensionFunctions;
            expression.EvaluateFunction += (name, args)=> NCalcExtensionFunctionGroup(name, args, request.Items);
            id = (Guid)expression.Evaluate();
            if (id.HasValue)
            {
                return new CalcResult(id.Value);
            }
            return null;
        }
                
        private void NCalcExtensionFunctionGroup(string name, FunctionArgs functionArgs, IEnumerable<CalcRequestItem> requestItems)
        {
            if (name == "MaxFrom")
            {
                if (functionArgs.Parameters.Count() != 1)
                    throw new CalculateException("Неверное количество параметров в вызове MaxFrom");
                
                double? maxValue = null;               
                foreach (var item in requestItems)
                {                    
                    if (!string.IsNullOrEmpty(item.Fields))
                    {
                        var fields = JObject.Parse(item.Fields)
                            .ToObject<Dictionary<string, object>>();
                        foreach (var field in fields)
                            functionArgs.Parameters[0].Parameters[field.Key] = field.Value;
                    }
                    var res = (double)functionArgs.Parameters[0].Evaluate();
                    if (!maxValue.HasValue || res > maxValue)
                    {
                        maxValue = res;
                    }
                }
                functionArgs.Result = maxValue;
            }
            if (name == "MinFrom")
            {
                if (functionArgs.Parameters.Count() != 1)
                    throw new CalculateException("Неверное количество параметров в вызове MinFrom");
                double? minValue = null;
                foreach (var item in requestItems)
                {                   
                    if (!string.IsNullOrEmpty(item.Fields))
                    {
                        var fields = JObject.Parse(item.Fields)
                            .ToObject<Dictionary<string, object>>();
                        foreach (var field in fields)
                            functionArgs.Parameters[0].Parameters[field.Key] = field.Value;
                    }
                    var res = (double)functionArgs.Parameters[0].Evaluate();
                    if (!minValue.HasValue || res < minValue)
                    {
                        minValue = res;
                    }
                }
                functionArgs.Result = minValue;
            }
            if (name == "AvgFrom")
            {
                if (functionArgs.Parameters.Count() != 1)
                    throw new CalculateException("Неверное количество параметров в вызове AvgFrom");
                double? sum = 0;
                Dictionary<Guid, double> values = new Dictionary<Guid, double>();
                foreach (var item in requestItems)
                {                   
                    if (!string.IsNullOrEmpty(item.Fields))
                    {
                        var fields = JObject.Parse(item.Fields)
                            .ToObject<Dictionary<string, object>>();
                        foreach (var field in fields)
                            functionArgs.Parameters[0].Parameters[field.Key] = field.Value;
                    }
                    var res = (double)functionArgs.Parameters[0].Evaluate();
                    values[item.Id] = res;
                    sum += res;
                }
                var avg = sum / requestItems.Count();                
                functionArgs.Result = avg;
            }

            if (name == "MaxIdFrom")
            {
                if (functionArgs.Parameters.Count() != 1)
                    throw new CalculateException("Неверное количество параметров в вызове MaxIdFrom");
                double? maxValue = null;
                Guid? id = null;
                foreach (var item in requestItems)
                {                    
                    if (!string.IsNullOrEmpty(item.Fields))
                    {
                        var fields = JObject.Parse(item.Fields)
                            .ToObject<Dictionary<string, object>>();
                        foreach (var field in fields)
                            functionArgs.Parameters[0].Parameters[field.Key] = field.Value;
                    }
                    var res = (double)functionArgs.Parameters[0].Evaluate();
                    if (!maxValue.HasValue || res > maxValue)
                    {
                        id = item.Id;
                        maxValue = res;
                    }
                }
                functionArgs.Result = id;
            }
            if (name == "MinIdFrom")
            {
                if (functionArgs.Parameters.Count() != 1)
                    throw new CalculateException("Неверное количество параметров в вызове MinIdFrom");
                double? minValue = null;
                Guid? id = null;
                foreach (var item in requestItems)
                {                   
                    if (!string.IsNullOrEmpty(item.Fields))
                    {
                        var fields = JObject.Parse(item.Fields)
                            .ToObject<Dictionary<string, object>>();
                        foreach (var field in fields)
                            functionArgs.Parameters[0].Parameters[field.Key] = field.Value;
                    }
                    var res = (double)functionArgs.Parameters[0].Evaluate();
                    if (!minValue.HasValue || res < minValue)
                    {
                        minValue = res;
                        id = item.Id;
                    }
                }
                functionArgs.Result = id;
            }
            if (name == "AvgIdFrom")
            {
                if (functionArgs.Parameters.Count() != 1)
                    throw new CalculateException("Неверное количество параметров в вызове AvgIdFrom");
                double? sum = 0;
                Dictionary<Guid, double> values = new Dictionary<Guid, double>();
                foreach (var item in requestItems)
                {                   
                    if (!string.IsNullOrEmpty(item.Fields))
                    {
                        var fields = JObject.Parse(item.Fields)
                            .ToObject<Dictionary<string, object>>();
                        foreach (var field in fields)
                            functionArgs.Parameters[0].Parameters[field.Key] = field.Value;
                    }
                    var res = (double)functionArgs.Parameters[0].Evaluate();
                    values[item.Id] = res;
                    sum += res;
                }
                var avg = sum / requestItems.Count();
                double? minDelta = null;
                Guid? result = null;
                foreach (var item in values)
                {
                    var delta = item.Value - avg;
                    if (!minDelta.HasValue || delta < minDelta)
                    {
                        minDelta = delta;
                        result = item.Key;
                    }
                }
                functionArgs.Result = result;
            }
        }

        private static void NCalcExtensionFunctions(string name, FunctionArgs functionArgs)
        {            
            if (name == "random")
            {
                if (functionArgs.Parameters.Count() == 0)
                {
                    functionArgs.Result = new Random().Next();
                }
                else if (functionArgs.Parameters.Count() == 1)
                {
                    functionArgs.Result = new Random().Next((int)(long)functionArgs.Parameters[0].Evaluate());
                }
                else
                {
                    functionArgs.Result = new Random().Next((int)(long)functionArgs.Parameters[0].Evaluate(), (int)(long)functionArgs.Parameters[1].Evaluate());
                }
            }
            if (name == "randomInt")
            {
                if (functionArgs.Parameters.Count() == 0)
                {
                    functionArgs.Result = new Random().Next();
                }
                else if (functionArgs.Parameters.Count() == 1)
                {
                    functionArgs.Result = new Random().Next((int)functionArgs.Parameters[0].Evaluate());
                }
                else
                {
                    functionArgs.Result = new Random().Next((int)functionArgs.Parameters[0].Evaluate(), (int)functionArgs.Parameters[1].Evaluate());
                }
            }
        }

    }

    public class CalcResult
    {
        public CalcResult(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; private set; }
    }

    public class CalcRequest
    { 
        public IEnumerable<CalcRequestItem> Items { get; set; }
        public string Formula { get; set; }
        public int Count { get; set; }
        public Action<Guid> ChangeOnSelect { get; set; }
    }

    public class CalcRequestItem
    {
        public Guid Id { get; set; }
        public string Fields { get; set; }
    }

    
}

