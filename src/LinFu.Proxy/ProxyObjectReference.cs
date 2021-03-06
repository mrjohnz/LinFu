﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using LinFu.AOP.Interfaces;
using LinFu.Proxy.Interfaces;

namespace LinFu.Proxy
{
    /// <summary>
    /// Represents a helper class that deserializes proxy instances.
    /// </summary>
    [Serializable]
    public class ProxyObjectReference : IObjectReference, ISerializable
    {
        private readonly Type _baseType;
        private readonly IProxy _proxy;

        /// <summary>
        /// Initializes a new instance of the ProxyObjectReference class.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> class that contains the serialized data.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that describes the serialization state.</param>
        protected ProxyObjectReference(SerializationInfo info, StreamingContext context)
        {
            // Deserialize the base type using its assembly qualified name
            var qualifiedName = info.GetString("__baseType");
            _baseType = Type.GetType(qualifiedName, true, false);

            // Rebuild the list of interfaces
            var interfaceList = new List<Type>();
            var interfaceCount = info.GetInt32("__baseInterfaceCount");
            for (var i = 0; i < interfaceCount; i++)
            {
                var keyName = string.Format("__baseInterface{0}", i);
                var currentQualifiedName = info.GetString(keyName);
                var interfaceType = Type.GetType(currentQualifiedName, true, false);

                interfaceList.Add(interfaceType);
            }

            // Reconstruct the proxy
            var factory = new ProxyFactory();
            var proxyType = factory.CreateProxyType(_baseType, interfaceList.ToArray());
            _proxy = (IProxy) Activator.CreateInstance(proxyType);

            var interceptor = (IInterceptor) info.GetValue("__interceptor", typeof (IInterceptor));
            _proxy.Interceptor = interceptor;
        }


        /// <summary>
        /// Returns the deserialized proxy instance.
        /// </summary>
        /// <param name="context">The <see cref="StreamingContext"/> that describes the serialization state.</param>
        /// <returns></returns>
        public object GetRealObject(StreamingContext context)
        {
            return _proxy;
        }


        /// <summary>
        /// Serializes the proxy to a stream. 
        /// </summary>
        /// <remarks>This method override does nothing.</remarks>
        /// <param name="info">The <see cref="SerializationInfo"/> class that contains the serialized data.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that describes the serialization state.</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
        }
    }
}