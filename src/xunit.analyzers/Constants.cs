using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers
{
    internal static class Constants
    {
        static class Categories
        {
            internal static string Usage { get; } = "Usage";
            internal static string Assertions { get; } = "Assertions";
            internal static string Extensibility { get; } = "Extensibility";
        }

        internal static class Descriptors
        {
            internal static DiagnosticDescriptor X1000_TestClassMustBePublic { get; } = new DiagnosticDescriptor("xUnit1000",
                "Test classes must be public",
                "Test classes must be public",
                Categories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true);

            internal static DiagnosticDescriptor X1001_FactMethodMustNotHaveParameters { get; } = new DiagnosticDescriptor("xUnit1001",
                "Fact methods cannot have parameters",
                "Fact methods cannot have parameters",
                Categories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true,
                description: "Remove the parameters from the method or convert it into a Theory.");

            internal static DiagnosticDescriptor X1002_TestMethodMustNotHaveMultipleFactAttributes { get; } = new DiagnosticDescriptor("xUnit1002",
                "Test methods cannot have multiple Fact or Theory attributes",
                "Test methods cannot have multiple Fact or Theory attributes",
                Categories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true);

            internal static DiagnosticDescriptor X1003_TheoryMethodMustHaveTestData { get; } = new DiagnosticDescriptor("xUnit1003",
                "Theory methods must have test data",
                "Theory methods must have test data",
                Categories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true,
                description: "Use InlineData, MemberData, or ClassData to provide test data for the Theory");

            internal static DiagnosticDescriptor X1004_TestMethodShouldNotBeSkipped { get; } = new DiagnosticDescriptor("xUnit1004",
                "Test methods should not be skipped",
                "Test methods should not be skipped",
                Categories.Usage, DiagnosticSeverity.Warning, isEnabledByDefault: true);

            internal static DiagnosticDescriptor X1005_FactMethodShouldNotHaveTestData { get; } = new DiagnosticDescriptor("xUnit1005",
                "Fact methods should not have test data",
                "Fact methods should not have test data",
                Categories.Usage, DiagnosticSeverity.Warning, isEnabledByDefault: true);

            internal static DiagnosticDescriptor X1006_TheoryMethodShouldHaveParameters { get; } = new DiagnosticDescriptor("xUnit1006",
                "Theory methods should have parameters",
                "Theory methods should have parameters",
                Categories.Usage, DiagnosticSeverity.Warning, isEnabledByDefault: true);

            internal static DiagnosticDescriptor X1007_ClassDataAttributeMustPointAtValidClass { get; } = new DiagnosticDescriptor("xUnit1007",
                "ClassData must point at a valid class",
                "ClassData must point at a valid class",
                Categories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true,
                description: "The class {0} must be public, not sealed, with an empty constructor, and implement IEnumerable<object[]>.");

            internal static DiagnosticDescriptor X1008_DataAttributeShouldBeUsedOnATheory { get; } = new DiagnosticDescriptor("xUnit1008",
                "Test data attribute should only be used on a Theory",
                "Test data attribute should only be used on a Theory",
                Categories.Usage, DiagnosticSeverity.Warning, isEnabledByDefault: true);

            internal static DiagnosticDescriptor X1009_InlineDataMustMatchTheoryParameters_TooFewValues { get; } = new DiagnosticDescriptor("xUnit1009",
                "InlineData values must match the number of method parameters",
                "InlineData values must match the number of method parameters",
                Categories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true);

            internal static DiagnosticDescriptor X1010_InlineDataMustMatchTheoryParameters_IncompatibleValueType { get; } = new DiagnosticDescriptor("xUnit1010",
                "The value is not convertible to the method parameter type",
                "The value is not convertible to the method parameter '{0}' of type '{1}'.",
                Categories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true);

            internal static DiagnosticDescriptor X1011_InlineDataMustMatchTheoryParameters_ExtraValue { get; } = new DiagnosticDescriptor("xUnit1011",
                "There is no matching method parameter",
                "There is no matching method parameter for value: {0}.",
                Categories.Usage, DiagnosticSeverity.Error, isEnabledByDefault: true);

            internal static DiagnosticDescriptor X1012_InlineDataMustMatchTheoryParameters_NullShouldNotBeUsedForIncompatibleParameter { get; } = new DiagnosticDescriptor("xUnit1012",
                "Null should not be used for value type parameters",
                "Null should not be used for value type parameter '{0}' of type '{1}'.",
                Categories.Usage, DiagnosticSeverity.Warning, isEnabledByDefault: true,
                description: "XUnit will execute the theory initializing the parameter to the default value of the type, which might not be the desired behavior");

            internal static DiagnosticDescriptor X1013_PublicMethodShouldBeMarkedAsTest { get; } = new DiagnosticDescriptor("xUnit1013",
                "Public method should be marked as test",
                "Public method '{0}' on test class '{1}' should be marked as a {2}.",
                Categories.Usage, DiagnosticSeverity.Warning, isEnabledByDefault: true,
                description: "Public methods on a test class that return void or Task should be marked as tests or have their accessibility reduced. While test methods do not have to be public "
                + " having public non-test methods might indicate that a method was intended to be a test but the annotation was not applied.");

            internal static DiagnosticDescriptor X1014_MemberDataShouldUseNameOfOperator { get; } = new DiagnosticDescriptor("xUnit1014",
                "MemberData should use nameof operator for member name",
                "MemberData should use nameof operator to reference member '{0}' on type '{1}'.",
                Categories.Usage, DiagnosticSeverity.Warning, isEnabledByDefault: true);

            internal static DiagnosticDescriptor X1015_TestBaseClassShouldBeAbstract { get; } = new DiagnosticDescriptor("xUnit1015",
                "Base classes with test methods should be abstract",
                "Base class '{0}' has test methods and should be abstract",
                Categories.Usage, DiagnosticSeverity.Warning, isEnabledByDefault: true,
                description: "Base classes with test methods should be abstract. Otherwise the test method will be executed multiple times, once for each concrete type in the class hierarchy.");

            internal static DiagnosticDescriptor X2000_AssertEqualLiteralValueShouldBeFirst { get; } = new DiagnosticDescriptor("xUnit2000",
                "Expected value should be first",
                "The literal or constant value {0} should be the first argument in the call to '{1}' in method '{2}' on type '{3}'.",
                Categories.Assertions, DiagnosticSeverity.Warning, isEnabledByDefault: true,
                description: "The xUnit Assertion library produces the best error messages if the expected value is passed in as the first argument.");

            internal static DiagnosticDescriptor X2001_AssertEqualsShouldNotBeUsed { get; } = new DiagnosticDescriptor("xUnit2001",
                "Do not use invalid equality check",
                "Do not use {0}.",
                Categories.Assertions, DiagnosticSeverity.Hidden, isEnabledByDefault: true);

            internal static DiagnosticDescriptor X2002_AssertNullShouldNotBeCalledOnValueTypes { get; } = new DiagnosticDescriptor("xUnit2002",
                "Do not use null check on value type",
                "Do not use {0} on value type '{1}'.",
                Categories.Assertions, DiagnosticSeverity.Warning, isEnabledByDefault: true);

            internal static DiagnosticDescriptor X2003_AssertEqualShouldNotUsedForNullCheck { get; } = new DiagnosticDescriptor("xUnit2003",
                "Do not use equality check to test for null value",
                "Do not use {0} to check for null value.",
                Categories.Assertions, DiagnosticSeverity.Warning, isEnabledByDefault: true);

            internal static DiagnosticDescriptor X2004_AssertEqualShouldNotUsedForBoolLiteralCheck { get; } = new DiagnosticDescriptor("xUnit2004",
                "Do not use equality check to test for boolean conditions",
                "Do not use {0} to check for boolean conditions.",
                Categories.Assertions, DiagnosticSeverity.Warning, isEnabledByDefault: true);

            internal static DiagnosticDescriptor X2005_AssertSameShouldNotBeCalledOnValueTypes { get; } = new DiagnosticDescriptor("xUnit2005",
                "Do not use identity check on value type",
                "Do not use {0} on value type '{1}'.",
                Categories.Assertions, DiagnosticSeverity.Warning, isEnabledByDefault: true,
                description: "The value type will be boxed which means its identity will always be different.");

        }

        internal static class Types
        {
            internal static readonly string XunitClassDataAttribute = "Xunit.ClassDataAttribute";
            internal static readonly string XunitIAsyncLifetime = "Xunit.IAsyncLifetime";
            internal static readonly string XunitInlineDataAttribute = "Xunit.InlineDataAttribute";
            internal static readonly string XunitMemberDataAttribute = "Xunit.MemberDataAttribute";
            internal static readonly string XunitFactAttribute = "Xunit.FactAttribute";
            internal static readonly string XunitTheoryAttribute = "Xunit.TheoryAttribute";

            internal static readonly string XunitSdkDataAttribute = "Xunit.Sdk.DataAttribute";

            internal static readonly string XunitAssert = "Xunit.Assert";

            internal static readonly string SystemThreadingTasksTask = "System.Threading.Tasks.Task";
        }
    }
}
