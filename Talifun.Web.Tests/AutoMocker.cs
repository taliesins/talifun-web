using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Talifun.Web.Tests
{
    public class AutoMocker<T>
    {
        private readonly MockManager mockManager;
        private T constructedObject;

        public AutoMocker(MockManager mockManager)
        {
            this.mockManager = mockManager;
        }

        public T ConstructedObject()
        {
            if (constructedObject == null)
                constructedObject = GenerateObject();

            return constructedObject;
        }

        private T GenerateObject()
        {
            var constructor = GreediestConstructor();
            var constructorParameters = new List<object>();

            foreach (var parameterInfo in constructor.GetParameters())
            {
                constructorParameters.Add(mockManager.Mock(parameterInfo.ParameterType));
            }

            return (T)constructor.Invoke(constructorParameters.ToArray());
        }

        private static ConstructorInfo GreediestConstructor()
        {
            return typeof(T).GetConstructors()
                .OrderByDescending(x => x.GetParameters().Length)
                .First();
        }
    }
}
