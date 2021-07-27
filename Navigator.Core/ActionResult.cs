using System;
using System.ComponentModel.DataAnnotations;

namespace Navigator.DataContracts
{
    [Serializable]
    public class ActionResult<TValue> where TValue : class
    {
        [Required]
        public int ResultCode { get; set; }

        public string ErrorMessage { get; set; }

        [Required]
        public new TValue Value { get; set; }

        public ActionResult(TValue value)
        {
            Value = value;
        }

        public ActionResult()
        {
            Value = default(TValue);
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
            return new ActionResult<TValue>(null)
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
        public static ActionResult Unauthorized()
        {
            return new ActionResult
            {
                ErrorMessage = "Unauthorized",
                ResultCode = 8
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
