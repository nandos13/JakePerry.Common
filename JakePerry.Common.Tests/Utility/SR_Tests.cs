using System.Reflection;

namespace JakePerry.Tests
{
    [TestClass]
    public class SR_Tests
    {
        public static string GetTestDisplayName(MethodInfo method, object?[]? data)
            => UnitTestingUtility.GetTestDisplayName(method, data);

        private static IDynamicTestData ReflectionMembers
        {
            get
            {
                var actions = new Func<MemberInfo>[]
                {
                    () => SR.SRClass,
                    () => SR.Format1Method,
                    () => SR.Format2Method,
                    () => SR.Format3Method,
                    () => SR.GetResourceStringMethod
                };

                return actions.Select(a => new object[1] { a });
            }
        }

        private static IDynamicTestData ExceptionResources
        {
            get
            {
                var flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
                var fields = typeof(SR.Strings).GetFields(flags);

                var strings = fields.Select(f => (string)f.GetValue(null)!);

                return strings.Select(s => new object[] { s });
            }
        }

        [TestMethod]
        [DynamicData(nameof(ReflectionMembers),
            DynamicDataDisplayName = nameof(GetTestDisplayName))]
        public void SR_GetInternalMembersViaReflection_DoesNotThrowAndIsNotNull(Func<MemberInfo> action)
        {
            var member = action.Invoke();

            Assert.IsNotNull(member);

            Console.WriteLine(member);
        }

        [TestMethod]
        [DynamicData(nameof(ExceptionResources),
            DynamicDataDisplayName = nameof(GetTestDisplayName))]
        public void GetResourceString_PredefinedResourceString_IsFound(string resource)
        {
            var resourceString = SR.GetResourceString(resource);

            int length = resourceString?.Length ?? 0;

            Assert.IsTrue(length > 0);
            Assert.IsNotNull(resourceString);
            Assert.IsFalse(StringComparer.Ordinal.Equals(resource, resourceString));

            Console.WriteLine(resource);
            Console.WriteLine(resourceString);
        }
    }
}
