using System;
using System.ComponentModel.DataAnnotations;

namespace Navigator.Core
{
    [Serializable]
    public class ActionResult<TValue>
    {
        [Required]
        public int ResultCode { get; set; }

        public string ErrorMessage { get; set; }

        [Required]
        public TValue Value { get; set; }

        public ActionResult(TValue value)
        {
            Value = value;
        }

        public ActionResult()
        {
            Value = default;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator ActionResult<TValue>(TValue value)
        {
            return new ActionResult<TValue>(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator ActionResult<TValue>(ActionResult<Empty> value)
        {
            return new ActionResult<TValue>()
            {
                ErrorMessage = value.ErrorMessage,
                ResultCode = value.ResultCode
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator TValue(ActionResult<TValue> value)
        {
            return value.Value;
        }
    }

    public class ActionResult : ActionResult<object>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static ActionResult NotFound()
        {
            return new ActionResult
            {
                ErrorMessage = "Not found",
                ResultCode = 404
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static ActionResult BadRequest()
        {
            return new ActionResult
            {
                ErrorMessage = "Bad request",
                ResultCode = 400
            };
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Empty
    {
    }
}
