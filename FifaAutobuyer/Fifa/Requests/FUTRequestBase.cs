using FifaAutobuyer.Database;
using FifaAutobuyer.Fifa.Database;
using FifaAutobuyer.Fifa.Http;
using FifaAutobuyer.Fifa.Models;
using FifaAutobuyer.Fifa.Responses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FifaAutobuyer.Fifa.Requests
{
    public abstract class FUTRequestBase
    {
        public FUTAccount FUTAccount { get; set; }
        public HttpClientEx HttpClient { get; set; }
        public LoginMethod CurrentLoginMethod { get; set; }

        public RequestPerMinuteManager RequestPerMinuteManager { get; set; }
        public RequestPerMinuteManager RequestPerMinuteManagerSearch { get; set; }

        public FUTRequestBase(FUTAccount account, RequestPerMinuteManager rpmManager, RequestPerMinuteManager rpmManagerSearch, LoginMethod loginMethod)
        {
            FUTAccount = account;
            RequestPerMinuteManager = rpmManager;
            RequestPerMinuteManagerSearch = rpmManagerSearch;
            CurrentLoginMethod = loginMethod;
        }
        
        protected void AddMethodOverrideHeader(HttpMethod httpMethod)
        {
            HttpClient.AddRequestHeader(NonStandardHttpHeaders.MethodOverride, httpMethod.Method);
        }

        protected void RemoveMethodOverrideHeader(HttpMethod httpMethod)
        {
            HttpClient.RemoveRequestHeader(NonStandardHttpHeaders.MethodOverride);
        }

        protected static async Task<T> Deserialize<T>(HttpResponseMessage message) where T : FUTError, new()
        {
            var messageContent = await message.Content.ReadAsStringAsync().ConfigureAwait(false);
            T deserializedObject = null;

            try
            {
                deserializedObject = JsonConvert.DeserializeObject<T>(messageContent);
            }
            catch (JsonSerializationException serializationException)
            {
                try
                {
                    var futError = JsonConvert.DeserializeObject<FUTError>(messageContent);
                    deserializedObject = (T)futError;
                }
#pragma warning disable CS0168
                catch (JsonSerializationException e)
                {
                    var msg = string.IsNullOrEmpty(messageContent) ? "null" : messageContent;
                    deserializedObject = new T();
                    deserializedObject.Message = "Data: " + msg + "\r\n\r\nInner Exception:" + e.ToString();
                    deserializedObject.Code = Models.FUTErrorCode.JsonSerializationException;
                    return deserializedObject;
                }
                var msg2 = string.IsNullOrEmpty(messageContent) ? "null" : messageContent;
                deserializedObject = new T();
                deserializedObject.Message = "Data: " + msg2 + "\r\n\r\nInner Exception:" + serializationException.ToString();
                deserializedObject.Code = Models.FUTErrorCode.JsonSerializationException;
                return deserializedObject;
#pragma warning restore CS0168
            }
            if (deserializedObject == null)
            {
                var msg = string.IsNullOrEmpty(messageContent) ? "null" : messageContent;
                deserializedObject = new T();
                deserializedObject.Code = Models.FUTErrorCode.JsonSerializationException;
                deserializedObject.Message = "Response was null -> " + msg;
                return deserializedObject;
            }
            return deserializedObject;
        }
#pragma warning disable 1998
        protected static async Task<T> Deserialize<T>(string messageContent) where T : FUTError, new()
        {
            T deserializedObject = null;

            try
            {
                deserializedObject = JsonConvert.DeserializeObject<T>(messageContent);
            }
            catch (JsonSerializationException serializationException)
            {
                try
                {
                    var futError = JsonConvert.DeserializeObject<FUTError>(messageContent);
                    deserializedObject = (T)futError;
                }
#pragma warning disable CS0168
                catch (JsonSerializationException e)
                {
                    var message = string.IsNullOrEmpty(messageContent) ? "null" : messageContent;
                    deserializedObject = new T();
                    deserializedObject.Message = "Data: " + message + serializationException.ToString() + "\r\n\r\nInner Exception:" + serializationException.ToString();
                    deserializedObject.Code = Models.FUTErrorCode.JsonSerializationException;
                }
#pragma warning restore CS0168
            }
            if (deserializedObject == null)
            {
                deserializedObject = new T();
                deserializedObject.Code = Models.FUTErrorCode.JsonSerializationException;
                deserializedObject.Message = "Response was null";
            }
            return deserializedObject;
        }
    }
}
