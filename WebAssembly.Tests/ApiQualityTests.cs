using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace WebAssembly
{
    /// <summary>
    /// Various checks to ensure that the API is at a high level of quality.
    /// </summary>
    [TestClass]
    public class ApiQualityTests
    {
        private static readonly RegeneratingWeakReference<(Type type, MemberInfo[] members, FieldInfo[] fields, PropertyInfo[] properties, MethodInfo[] methods)[]> typeInfo =
            new RegeneratingWeakReference<(Type type, MemberInfo[] members, FieldInfo[] fields, PropertyInfo[] properties, MethodInfo[] methods)[]>(() =>
                typeof(Module)
                .Assembly
                .GetTypes()
                .Where(type => type.GetCustomAttribute<CompilerGeneratedAttribute>() == null)
                .Select(type => (
                    type,
                    members: type.GetMembers().Where(member => member.DeclaringType?.Assembly == type.Assembly).ToArray(),
                    fields: type.GetFields().Where(field => field.DeclaringType?.Assembly == type.Assembly).ToArray(),
                    properties: type.GetProperties().Where(property => property.DeclaringType?.Assembly == type.Assembly).ToArray(),
                    methbods: type.GetMethods().Where(method => method.DeclaringType?.Assembly == type.Assembly).ToArray())
                )
                .ToArray()
            );

        /// <summary>
        /// With few exceptions, public overrides should be sealed to limit users' ability to hurt themselves.
        /// </summary>
        [TestMethod]
        public void PublicOverridesAreMostlySealed()
        {
            IEnumerable<string> GatherViolations()
            {
                foreach (var info in typeInfo.Reference)
                {
                    var type = info.type;

                    if (type.IsAbstract)
                        continue;
                    if (type.IsSealed)
                        continue;

                    foreach (var method in info.methods)
                    {
                        switch (method.Name)
                        {
                            case nameof(object.ToString):
                            case nameof(object.GetHashCode):
                                if (method.GetParameters().Length == 0)
                                    continue;
                                break;
                            case nameof(object.Equals):
                                if (method.GetParameters().Length == 1)
                                    continue;
                                break;
                        }

                        if (method != null && method.IsVirtual && !method.IsFinal)
                            yield return $"{method.DeclaringType}: {method.Name} override must be sealed.";
                    }
                }
            }

            Assert.AreEqual("", string.Join(Environment.NewLine, GatherViolations()));
        }
    }
}
