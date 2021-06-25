using System;
using System.Collections.Generic;
using Navigator.Exceptions;

namespace Navigator.Core
{
    /// <summary>
    /// 
    /// </summary>
    internal class NavigatorInterface : Dictionary<string, ControllerAction>
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public NavigatorInterface(string name)
        {
            Name = name;
        }

        /// <summary>
        /// 
        /// </summary>
        public Type ControllerType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public new ControllerAction this[string key]
        {
            get => !ContainsKey(key) ? null : base[key];
            set => Add(key, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        public void AddAction(ControllerAction action)
        {
            if (ContainsKey(action.MessageMethodName))
            {
                throw new IllegalControllerDeclarationException("Duplicate action definition for method " + action.MessageMethodName, action.ControllerType);
            }

            Add(action.MessageMethodName, action);
        }
    }
}
