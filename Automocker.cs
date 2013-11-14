using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TypeMock.ArrangeActAssert;

namespace yads.Automocker
{
    public class Automocker
    {
        private readonly IDictionary<Type, object> _typeRegistry = new Dictionary<Type, object>();
        private static readonly MethodInfo TypeMockInstance;

        static Automocker()
        {
            TypeMockInstance = Isolate.Fake.GetType()
                                      .GetMethods()
                                      .Where(m => m.Name == "Instance")
                                      .Select(m => new
                                                       {
                                                           Method = m,
                                                           Params = m.GetParameters(),
                                                           Args = m.GetGenericArguments()
                                                       })
                                      .Where(x => x.Params.Length == 0
                                                  && x.Args.Length == 1)
                                      .Select(x => x.Method)
                                      .First();
        }

        public T CreateSut<T>()
        {
            var type = typeof (T);
            var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            ConstructorInfo largestConstructor = null;
            foreach (var constructor in constructors)
            {
                if (largestConstructor == null ||
                    largestConstructor.GetParameters().Length < constructor.GetParameters().Length)
                {
                    largestConstructor = constructor;
                }
            }
            var parameters = new List<object>();
            foreach (var parameterInfo in largestConstructor.GetParameters())
            {
                if (!_typeRegistry.ContainsKey(parameterInfo.ParameterType))
                {
                    _typeRegistry[parameterInfo.ParameterType] = GetMockInstance(parameterInfo.ParameterType);
                }
                parameters.Add(_typeRegistry[parameterInfo.ParameterType]);
            }
            return (T) largestConstructor.Invoke(parameters.ToArray());
        }

        public T GetMock<T>()
        {
            var type = typeof (T);
            if (_typeRegistry.ContainsKey(type))
            {
                return (T) _typeRegistry[type];
            }
            var mockInstance = (T) GetMockInstance(type);
            _typeRegistry[type] = mockInstance;
            return mockInstance;
        }

        private static object GetMockInstance(Type type)
        {
            return TypeMockInstance.MakeGenericMethod(type).Invoke(Isolate.Fake, null);
        }
    }
}