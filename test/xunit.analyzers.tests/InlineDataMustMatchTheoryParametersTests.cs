﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public abstract class InlineDataMustMatchTheoryParametersTests
    {
        public abstract class Analyzer
        {
            readonly DiagnosticAnalyzer analyzer = new InlineDataMustMatchTheoryParameters();

            public class ForFactMethod : Analyzer
            {

                [Fact]
                public async void DoesNotFindError_WhenNoDataAttributes()
                {
                    var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, "public class TestClass { [Xunit.Fact] public void TestMethod() { } }");

                    Assert.Empty(diagnostics);
                }

                [Fact]
                public async void DoesNotFindError_WithAttribute()
                {
                    var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                        "public class TestClass { [Xunit.Fact, Xunit.InlineData] public void TestMethod(string a) { } }");

                    Assert.Empty(diagnostics);
                }
            }

            public class ForTheoryWithArgumentMatch : Analyzer
            {
                [Fact]
                public async void DoesNotFindErrorFor_UsingParamsArray()
                {
                    var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                        "public class TestClass { [Xunit.Theory, Xunit.InlineData(\"abc\", 1, null)] public void TestMethod(string a, int b, object c) { } }");

                    Assert.Empty(diagnostics);
                }

                [Fact]
                public async void DoesNotFindError_UsingExplicitArray()
                {
                    var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                        "public class TestClass { [Xunit.Theory, Xunit.InlineData(new object[] {\"abc\", 1, null})] public void TestMethod(string a, int b, object c) { } }");

                    Assert.Empty(diagnostics);
                }

                [Fact]
                public async void DoesNotFindError_UsingExplicitNamedArray()
                {
                    var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                        "public class TestClass { [Xunit.Theory, Xunit.InlineData(data: new object[] {\"abc\", 1, null})] public void TestMethod(string a, int b, object c) { } }");

                    Assert.Empty(diagnostics);
                }

                [Fact]
                public async void DoesNotFindError_UsingImplicitArray()
                {
                    var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                        "public class TestClass { [Xunit.Theory, Xunit.InlineData(new [] {(object)\"abc\", 1, null})] public void TestMethod(string a, int b, object c) { } }");

                    Assert.Empty(diagnostics);
                }

                [Fact]
                public async void DoesNotFindError_UsingImplicitNamedArray()
                {
                    var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                        "public class TestClass { [Xunit.Theory, Xunit.InlineData(data: new [] {(object)\"abc\", 1, null})] public void TestMethod(string a, int b, object c) { } }");

                    Assert.Empty(diagnostics);
                }
            }

            public class ForAttributeWithTooFewArguments : Analyzer
            {
                [Fact]
                public async void FindsError()
                {
                    var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                        "public class TestClass { [Xunit.Theory, Xunit.InlineData(1)] public void TestMethod(int a, int b, string c) { } }");

                    Assert.Collection(diagnostics,
                      d =>
                      {
                          Assert.Equal("InlineData values must match the number of method parameters", d.GetMessage());
                          Assert.Equal("xUnit1009", d.Descriptor.Id);
                          Assert.Equal(DiagnosticSeverity.Error, d.Severity);
                      });
                }
            }

            public class ForAttributeWithTooManyArguments : Analyzer
            {
                [Fact]
                public async void FindsError()
                {
                    var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                        "public class TestClass { [Xunit.Theory, Xunit.InlineData(1, 2, \"abc\")] public void TestMethod(int a) { } }");

                    Assert.Collection(diagnostics,
                        d =>
                        {
                            Assert.Equal("There is no matching method parameter for value: 2.", d.GetMessage());
                            Assert.Equal("xUnit1011", d.Descriptor.Id);
                            Assert.Equal(DiagnosticSeverity.Error, d.Severity);
                        },
                        d =>
                        {
                            Assert.Equal("There is no matching method parameter for value: \"abc\".", d.GetMessage());
                            Assert.Equal("xUnit1011", d.Descriptor.Id);
                            Assert.Equal(DiagnosticSeverity.Error, d.Severity);
                        });
                }
            }

            public class ForAttributeNullValue : Analyzer
            {
                public static TheoryData<string> ValueTypes { get; } = new TheoryData<string>
                {
                    "bool",
                    "int",
                    "byte",
                    "short",
                    "long",
                    "decimal",
                    "double",
                    "float",
                    "char",
                    "ulong",
                    "uint",
                    "ushort",
                    "sbyte",
                    "System.StringComparison",
                    "System.Guid",
                };

                [Fact]
                public async void FindsWarning_ForSingleNullValue()
                {
                    var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                        "public class TestClass { [Xunit.Theory, Xunit.InlineData(null)] public void TestMethod(int a) { } }");

                    Assert.Collection(diagnostics,
                       d =>
                       {
                           Assert.Equal("Null should not be used for value type parameter 'a' of type 'int'.", d.GetMessage());
                           Assert.Equal("xUnit1012", d.Descriptor.Id);
                           Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
                       });
                }

                [Theory]
                [MemberData(nameof(ValueTypes))]
                public async void FindsWarning_ForValueTypeParameter(string type)
                {
                    var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                        "public class TestClass { [Xunit.Theory, Xunit.InlineData(1, null)] public void TestMethod(int a, " + type + " b) { } }");

                    Assert.Collection(diagnostics,
                       d =>
                       {
                           Assert.Equal("Null should not be used for value type parameter 'b' of type '" + type + "'.", d.GetMessage());
                           Assert.Equal("xUnit1012", d.Descriptor.Id);
                           Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
                       });
                }

                [Theory]
                [MemberData(nameof(ValueTypes))]
                public async void DoesNotFindWarning_ForNullableValueType(string type)
                {
                    var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                        "public class TestClass { [Xunit.Theory, Xunit.InlineData(1, null)] public void TestMethod(int a, " + type + "? b) { } }");

                    Assert.Empty(diagnostics);
                }

                [Theory]
                [InlineData("object")]
                [InlineData("string")]
                [InlineData("System.Exception")]
                public async void DoesNotFindWarning_ForReferenceParameterType(string type)
                {
                    var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                        "public class TestClass { [Xunit.Theory, Xunit.InlineData(1, null)] public void TestMethod(int a, " + type + " b) { } }");

                    Assert.Empty(diagnostics);
                }
            }

            public class ForConversionToNumericValue : Analyzer
            {
                public static IEnumerable<Tuple<string>> NumericTypes = new[] { "int", "long", "short", "byte", "float", "double", "decimal", "uint", "ulong", "ushort", "sbyte", }.Select(t => Tuple.Create(t));

                public static IEnumerable<Tuple<string, string>> NumericValuesAndNumericTypes { get; } = from value in NumericValues
                                                                                                         from type in NumericTypes
                                                                                                         select Tuple.Create(value.Item1, type.Item1);

                public static IEnumerable<Tuple<string, string>> BoolValuesAndNumericTypes { get; } = from value in BoolValues
                                                                                                      from type in NumericTypes
                                                                                                      select Tuple.Create(value.Item1, type.Item1);

                [Theory]
                [TupleMemberData(nameof(NumericValuesAndNumericTypes))]
                public async void DoesNotFindError_FromAnyOtherNumericType(string value, string type)
                {
                    var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                        "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(" + type + " a) { } }");

                    Assert.Empty(diagnostics);
                }

                [Theory]
                [TupleMemberData(nameof(NumericValuesAndNumericTypes))]
                public async void DoesNotFindError_FromAnyOtherNumericType_ToNullable(string value, string type)
                {
                    var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                        "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(" + type + "? a) { } }");

                    Assert.Empty(diagnostics);
                }

                [Theory]
                [TupleMemberData(nameof(BoolValuesAndNumericTypes))]
                public async void FindsError_ForBoolArgument(string value, string type)
                {
                    var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                        "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(" + type + " a) { } }");

                    Assert.Collection(diagnostics,
                      d =>
                      {
                          Assert.Equal("The value is not convertible to the method parameter 'a' of type '" + type + "'.", d.GetMessage());
                          Assert.Equal("xUnit1010", d.Descriptor.Id);
                      });
                }

                [Theory]
                [TupleMemberData(nameof(NumericTypes))]
                public async void DoesNotFindError_ForCharArgument(string type)
                {
                    var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                        "public class TestClass { [Xunit.Theory, Xunit.InlineData('a')] public void TestMethod(" + type + " a) { } }");

                    Assert.Empty(diagnostics);
                }

                [Theory]
                [TupleMemberData(nameof(NumericTypes))]
                public async void FindsError_ForEnumArgument(string type)
                {
                    var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                        "public class TestClass { [Xunit.Theory, Xunit.InlineData(System.StringComparison.InvariantCulture)] public void TestMethod(" + type + " a) { } }");


                    Assert.Collection(diagnostics,
                      d =>
                      {
                          Assert.Equal("The value is not convertible to the method parameter 'a' of type '" + type + "'.", d.GetMessage());
                          Assert.Equal("xUnit1010", d.Descriptor.Id);
                      });
                }
            }

            public class ForConversionToBoolValue : Analyzer
            {
                [Theory]
                [TupleMemberData(nameof(BoolValues))]
                public async void DoesNotFindError_FromBoolType(string value)
                {
                    var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                        "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(bool a) { } }");

                    Assert.Empty(diagnostics);
                }

                [Theory]
                [TupleMemberData(nameof(BoolValues))]
                public async void DoesNotFindError_FromBoolType_ToNullable(string value)
                {
                    var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                        "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(bool? a) { } }");

                    Assert.Empty(diagnostics);
                }

                [Theory]
                [TupleMemberData(nameof(NumericValues))]
                [InlineData("System.StringComparison.Ordinal")]
                [InlineData("'a'")]
                [InlineData("\"abc\"")]
                [InlineData("typeof(string)")]
                public async void FindsError_ForOtherArguments(string value)
                {
                    var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                        "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(bool a) { } }");

                    Assert.Collection(diagnostics,
                      d =>
                      {
                          Assert.Equal("The value is not convertible to the method parameter 'a' of type 'bool'.", d.GetMessage());
                          Assert.Equal("xUnit1010", d.Descriptor.Id);
                      });
                }
            }

            public class ForConversionToCharValue : Analyzer
            {
                [Theory]
                [InlineData("'a'")]
                [TupleMemberData(nameof(IntegerValues))]
                public async void DoesNotFindError_FromCharOrIntegerType(string value)
                {
                    var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                        "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(char a) { } }");

                    Assert.Empty(diagnostics);
                }

                [Theory]
                [InlineData("'a'")]
                [TupleMemberData(nameof(IntegerValues))]
                public async void DoesNotFindError_FromCharType_ToNullable(string value)
                {
                    var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                        "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(char? a) { } }");

                    Assert.Empty(diagnostics);
                }

                [Theory]
                [TupleMemberData(nameof(FloatingPointValues))]
                [InlineData("System.StringComparison.Ordinal")]
                [TupleMemberData(nameof(BoolValues))]
                [InlineData("\"abc\"")]
                [InlineData("typeof(string)")]
                public async void FindsError_ForOtherArguments(string value)
                {
                    var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                        "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(char a) { } }");

                    Assert.Collection(diagnostics,
                      d =>
                      {
                          Assert.Equal("The value is not convertible to the method parameter 'a' of type 'char'.", d.GetMessage());
                          Assert.Equal("xUnit1010", d.Descriptor.Id);
                      });
                }
            }

            public class ForConversionToEnum : Analyzer
            {
                [Fact]
                public async void DoesNotFindError_FromEnum()
                {
                    var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                        "public class TestClass { [Xunit.Theory, Xunit.InlineData(System.StringComparison.Ordinal)] public void TestMethod(System.StringComparison a) { } }");

                    Assert.Empty(diagnostics);
                }

                [Fact]
                public async void DoesNotFindError_FromEnum_ToNullable()
                {
                    var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                        "public class TestClass { [Xunit.Theory, Xunit.InlineData(System.StringComparison.Ordinal)] public void TestMethod(System.StringComparison? a) { } }");

                    Assert.Empty(diagnostics);
                }

                [Theory]
                [TupleMemberData(nameof(NumericValues))]
                [TupleMemberData(nameof(BoolValues))]
                [InlineData("'a'")]
                [InlineData("\"abc\"")]
                [InlineData("typeof(string)")]
                public async void FindsError_ForOtherArguments(string value)
                {
                    var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                        "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(System.StringComparison a) { } }");

                    Assert.Collection(diagnostics,
                      d =>
                      {
                          Assert.Equal("The value is not convertible to the method parameter 'a' of type 'System.StringComparison'.", d.GetMessage());
                          Assert.Equal("xUnit1010", d.Descriptor.Id);
                      });
                }
            }

            public class ForConversionToType : Analyzer
            {
                [Theory]
                [InlineData("typeof(string)")]
                [InlineData("null")]
                public async void DoesNotFindError_FromType(string value)
                {
                    var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                        "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(System.Type a) { } }");

                    Assert.Empty(diagnostics);
                }

                [Theory]
                [TupleMemberData(nameof(NumericValues))]
                [TupleMemberData(nameof(BoolValues))]
                [InlineData("'a'")]
                [InlineData("\"abc\"")]
                [InlineData("System.StringComparison.Ordinal")]
                public async void FindsError_ForOtherArguments(string value)
                {
                    var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                        "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(System.Type a) { } }");

                    Assert.Collection(diagnostics,
                      d =>
                      {
                          Assert.Equal("The value is not convertible to the method parameter 'a' of type 'System.Type'.", d.GetMessage());
                          Assert.Equal("xUnit1010", d.Descriptor.Id);
                      });
                }
            }

            public class ForConversionToString : Analyzer
            {
                [Theory]
                [InlineData("\"abc\"")]
                [InlineData("null")]
                public async void DoesNotFindError_FromBoolType(string value)
                {
                    var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                        "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(string a) { } }");

                    Assert.Empty(diagnostics);
                }

                [Theory]
                [TupleMemberData(nameof(NumericValues))]
                [TupleMemberData(nameof(BoolValues))]
                [InlineData("System.StringComparison.Ordinal")]
                [InlineData("'a'")]
                [InlineData("typeof(string)")]
                public async void FindsError_ForOtherArguments(string value)
                {
                    var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                        "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(string a) { } }");

                    Assert.Collection(diagnostics,
                      d =>
                      {
                          Assert.Equal("The value is not convertible to the method parameter 'a' of type 'string'.", d.GetMessage());
                          Assert.Equal("xUnit1010", d.Descriptor.Id);
                      });
                }
            }

            public class ForConversionToInterface : Analyzer
            {
                [Theory]
                [TupleMemberData(nameof(NumericValues))]
                [InlineData("System.StringComparison.Ordinal")]
                [InlineData("null")]
                public async void DoesNotFindError_FromTypesImplementingInterface(string value)
                {
                    var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                        "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(System.IFormattable a) { } }");

                    Assert.Empty(diagnostics);
                }

                [Theory]
                [TupleMemberData(nameof(BoolValues))]
                [InlineData("'a'")]
                [InlineData("\"abc\"")]
                [InlineData("typeof(string)")]
                public async void FindsError_ForOtherArguments(string value)
                {
                    var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                        "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(System.IFormattable a) { } }");

                    Assert.Collection(diagnostics,
                      d =>
                      {
                          Assert.Equal("The value is not convertible to the method parameter 'a' of type 'System.IFormattable'.", d.GetMessage());
                          Assert.Equal("xUnit1010", d.Descriptor.Id);
                      });
                }
            }

            public class ForConversionToObject : Analyzer
            {
                [Theory]
                [TupleMemberData(nameof(BoolValues))]
                [TupleMemberData(nameof(NumericValues))]
                [InlineData("System.StringComparison.Ordinal")]
                [InlineData("'a'")]
                [InlineData("\"abc\"")]
                [InlineData("null")]
                [InlineData("typeof(string)")]
                public async void DoesNotFindError_FromAnyValue(string value)
                {
                    var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                        "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(object a) { } }");

                    Assert.Empty(diagnostics);
                }
            }

            // Note: decimal literal 42M is not valid as an attribute argument
            public static IEnumerable<Tuple<string>> IntegerValues { get; } = new[] { "42", "42L", "42u", "42ul", "(short)42", "(byte)42", "(ushort)42", "(sbyte)42", }.Select(v => Tuple.Create(v)).ToArray();

            public static IEnumerable<Tuple<string>> FloatingPointValues { get; } = new[] { "42f", "42d" }.Select(v => Tuple.Create(v)).ToArray();

            public static IEnumerable<Tuple<string>> NumericValues { get; } = IntegerValues.Concat(FloatingPointValues).ToArray();

            public static IEnumerable<Tuple<string>> BoolValues { get; } = new[] { "true", "false" }.Select(v => Tuple.Create(v)).ToArray();
        }
    }
}

