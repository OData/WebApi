// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WebPages.Resources;
using Microsoft.WebPages.TestUtils;

namespace Microsoft.WebPages.Test {
    /// <summary>
    ///This is a test class for WebPageSurrogateControlBuilderTest and is intended
    ///to contain all WebPageSurrogateControlBuilderTest Unit Tests
    ///</summary>
    [TestClass()]
    public class WebPageSurrogateControlBuilderTest {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext {
            get {
                return testContextInstance;
            }
            set {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        /// <summary>
        ///A test for IsAspx
        ///</summary>
        [TestMethod()]
        public void IsAspxTest() {
            Dictionary<string, bool> testCases = new Dictionary<string, bool>() {
                { "test.abc", false },
                { "test", false },
                { "test.aspx", true },
                { "TEST.AspX", true },
                { "TEST.xyzabc.AspX", true},
            };
            foreach (var kvp in testCases) {
                string virtualPath = kvp.Key;
                bool expected = kvp.Value;
                bool actual;
                actual = WebPageSurrogateControlBuilder.IsAspx(virtualPath);
                Assert.AreEqual(expected, actual);
            }
        }

        /// <summary>
        ///A test for IsAspq
        ///</summary>
        [TestMethod()]
        public void IsAspqTest() {
            Dictionary<string, bool> testCases = new Dictionary<string, bool>() {
                { "test.abc", false },
                { "test", false },
                { "test.aspq", true },
                { "TEST.AspQ", true },
                { "TEST.xyzabc.AsPq", true},
            };
            foreach (var kvp in testCases) {
                string virtualPath = kvp.Key;
                bool expected = kvp.Value;
                bool actual;
                actual = WebPageSurrogateControlBuilder.IsAspq(virtualPath);
                Assert.AreEqual(expected, actual);
            }
        }


        /// <summary>
        ///A test for IsAscq
        ///</summary>
        [TestMethod()]
        public void IsAscqTest() {
            Dictionary<string, bool> testCases = new Dictionary<string, bool>() {
                { "test.abc", false },
                { "test", false },
                { "test.ascq", true },
                { "TEST.AscQ", true },
                { "TEST.xyzabc.AsCQ", true},
            };
            foreach (var kvp in testCases) {
                string virtualPath = kvp.Key;
                bool expected = kvp.Value;
                bool actual;
                actual = WebPageSurrogateControlBuilder.IsAscq(virtualPath);
                Assert.AreEqual(expected, actual);
            }
        }

        /// <summary>
        ///A test for AddImports
        ///</summary>
        [TestMethod]
        public void AddImportsTest() {
            CodeCompileUnit ccu = new CodeCompileUnit();
            ccu.Namespaces.Add(new CodeNamespace());
            WebPageSurrogateControlBuilder.AddImports(ccu);
            VerifyDefaultNameSpaces(ccu);

            // Temporarily set the static field to something else
            var originalNamespaces = new List<string>(WebPageSurrogateControlBuilder.Namespaces);
            WebPageSurrogateControlBuilder.Namespaces.Clear();
            WebPageSurrogateControlBuilder.Namespaces.Add("System.ABC");
            ccu = new CodeCompileUnit();
            ccu.Namespaces.Add(new CodeNamespace());
            WebPageSurrogateControlBuilder.AddImports(ccu);
            Assert.AreEqual(1, ccu.Namespaces[0].Imports.Count);
            Assert.AreEqual("System.ABC", ccu.Namespaces[0].Imports[0].Namespace);

            // Restore the static field
            WebPageSurrogateControlBuilder.Namespaces.Clear();
            foreach (var ns in originalNamespaces) {
                WebPageSurrogateControlBuilder.Namespaces.Add(ns);
            }
        }

        [TestMethod]
        public void ProcessGeneratedCodeTest() {
            VerifyProcessGeneratedCode(new WebPageSurrogateControlBuilder());
            VerifyProcessGeneratedCode(new WebUserControlSurrogateControlBuilder());
        }

        public void VerifyProcessGeneratedCode(ControlBuilder builder) {
            CodeCompileUnit ccu = new CodeCompileUnit();
            ccu.Namespaces.Add(new CodeNamespace());
            builder.ProcessGeneratedCode(ccu, null, null, null, null);
            VerifyDefaultNameSpaces(ccu);
        }

        public void VerifyDefaultNameSpaces(CodeCompileUnit ccu) {
            Assert.AreEqual(13, ccu.Namespaces[0].Imports.Count);
            Assert.AreEqual(WebPageSurrogateControlBuilder.Namespaces.Count, ccu.Namespaces[0].Imports.Count);
        }
                
        /// <summary>
        ///A test for GetLanguageAttributeFromText
        ///</summary>
        [TestMethod()]
        public void GetLanguageAttributeFromTextTest() {
            Assert.AreEqual("abcd", WebPageSurrogateControlBuilder.GetLanguageAttributeFromText("<%@  Page  Language=\"  abcd\" %>  csharp c# cs  "));
            Assert.AreEqual("xxx", WebPageSurrogateControlBuilder.GetLanguageAttributeFromText("<%@  Page  lanGUAGE='xxx' %>  csharp c# cs  "));
            Assert.AreEqual("", WebPageSurrogateControlBuilder.GetLanguageAttributeFromText("<%@  Page  languagE='' %>  csharp c# cs  "));
            Assert.AreEqual(null, WebPageSurrogateControlBuilder.GetLanguageAttributeFromText("<%@  Page  hello world %>  csharp c# cs  "));
        }

        /// <summary>
        ///A test for GetLanguageFromText
        ///</summary>
        [TestMethod()]
        public void GetLanguageFromTextPositiveTest() {
            Assert.AreEqual(Language.VisualBasic, WebPageSurrogateControlBuilder.GetLanguageFromText("<%@  Page   %>  csharp c# cs  "));
            Assert.AreEqual(Language.VisualBasic, WebPageSurrogateControlBuilder.GetLanguageFromText("<%@   Page   Language='  vB    '   %>  csharp c# cs  "));
            Assert.AreEqual(Language.VisualBasic, WebPageSurrogateControlBuilder.GetLanguageFromText("<%@   Page   Language=\"  vb    \"   %>  csharp c# cs  "));
            Assert.AreEqual(Language.VisualBasic, WebPageSurrogateControlBuilder.GetLanguageFromText("<%@   Page   Language='  Vb    '   %>  csharp c# cs  "));
            Assert.AreEqual(Language.VisualBasic, WebPageSurrogateControlBuilder.GetLanguageFromText("<%@   Page   Language='  vB    '   %>  csharp c# cs  "));
            Assert.AreEqual(Language.VisualBasic, WebPageSurrogateControlBuilder.GetLanguageFromText("<%@   Page   Language='  vBs    '   %>  csharp c# cs  "));
            Assert.AreEqual(Language.VisualBasic, WebPageSurrogateControlBuilder.GetLanguageFromText("<%@   Page   Language='  vBscript    '   %>  csharp c# cs  "));
            Assert.AreEqual(Language.VisualBasic, WebPageSurrogateControlBuilder.GetLanguageFromText("<%@   Page   Language='  vBscript    '   %>  csharp c# cs  "));
            Assert.AreEqual(Language.VisualBasic, WebPageSurrogateControlBuilder.GetLanguageFromText("<%@   Page   Language='  vIsuaLBasic    '   %>  csharp c# cs  "));

            Assert.AreEqual(Language.CSharp, WebPageSurrogateControlBuilder.GetLanguageFromText("<%@   Page   Language='  c#    '   %>  vb vbs visualbasic vbscript  "));
            Assert.AreEqual(Language.CSharp, WebPageSurrogateControlBuilder.GetLanguageFromText("<%@   Page   Language='  cS    '   %>  vb vbs visualbasic vbscript  "));
            Assert.AreEqual(Language.CSharp, WebPageSurrogateControlBuilder.GetLanguageFromText("<%@   Page   Language='  cShaRP    '   %>  vb vbs visualbasic vbscript  "));
        }

        /// <summary>
        ///A test for GetLanguageFromText
        ///</summary>
        [TestMethod()]
        public void GetLanguageFromTextNegativeTest() {
            ExceptionAssert.Throws<HttpException>(() =>
                WebPageSurrogateControlBuilder.GetLanguageFromText("<%@   Page   Language='  zxc'   %>  vb vbs visualbasic vbscript  "),
                String.Format(WebPageResources.WebPage_InvalidLanguage, "zxc"));
        }

        /// <summary>
        ///A test for FixUpWriteSnippetStatement
        ///</summary>
        [TestMethod()]
        public void FixUpWriteSnippetStatementTest() {
            var code = " abc zyx";
            var stmt = new CodeSnippetStatement(code);
            WebPageSurrogateControlBuilder.FixUpWriteSnippetStatement(stmt);
            Assert.AreEqual(code, stmt.Value);

            code = " this.Write(\"hello\"); ";
            stmt = new CodeSnippetStatement(code);
            WebPageSurrogateControlBuilder.FixUpWriteSnippetStatement(stmt);
            Assert.AreEqual(code, stmt.Value);

            // @__w.Write case
            code = " @__w.Write(\"hello\"); ";
            stmt = new CodeSnippetStatement(code);
            WebPageSurrogateControlBuilder.FixUpWriteSnippetStatement(stmt);
            Assert.AreEqual(" WriteLiteral(\"hello\"); ", stmt.Value);

            // __w.Write case
            code = " __w.Write(\"hello\"); ";
            stmt = new CodeSnippetStatement(code);
            WebPageSurrogateControlBuilder.FixUpWriteSnippetStatement(stmt);
            Assert.AreEqual(" WriteLiteral(\"hello\"); ", stmt.Value);
        }

        /// <summary>
        ///A test for FixUpWriteCodeExpressionStatement
        ///</summary>
        [TestMethod()]
        public void FixUpWriteCodeExpressionStatementTest() {
            // Null test
            WebPageSurrogateControlBuilder.FixUpWriteCodeExpressionStatement(null);

            // Should fix up the statement
            var invoke = new CodeMethodInvokeExpression(new CodeArgumentReferenceExpression("__w"), "Write");
            CodeExpressionStatement exprStmt = new CodeExpressionStatement(invoke);
            WebPageSurrogateControlBuilder.FixUpWriteCodeExpressionStatement(exprStmt);
            Assert.AreEqual(invoke.Method.MethodName, "WriteLiteral");
            Assert.AreEqual(invoke.Method.TargetObject.GetType(), typeof(CodeThisReferenceExpression));

            // Should NOT fix up the statement
            invoke = new CodeMethodInvokeExpression(new CodeArgumentReferenceExpression("xyz"), "Write");
            exprStmt = new CodeExpressionStatement(invoke);
            WebPageSurrogateControlBuilder.FixUpWriteCodeExpressionStatement(exprStmt);
            Assert.AreEqual(invoke.Method.MethodName, "Write");
            Assert.IsInstanceOfType(invoke.Method.TargetObject, typeof(CodeArgumentReferenceExpression));
        }

        /// <summary>
        ///A test for FixUpWriteStatement
        ///</summary>
        [TestMethod()]
        public void FixUpWriteStatementTest() {
            
            var invoke = new CodeMethodInvokeExpression(new CodeArgumentReferenceExpression("__w"), "Write");
            CodeExpressionStatement exprStmt = new CodeExpressionStatement(invoke);
            WebPageSurrogateControlBuilder.FixUpWriteStatement(exprStmt);
            Assert.AreEqual(invoke.Method.MethodName, "WriteLiteral");
            Assert.IsInstanceOfType(invoke.Method.TargetObject, typeof(CodeThisReferenceExpression));

            // @__w.Write case
            var code = " @__w.Write(\"hello\"); ";
            var stmt = new CodeSnippetStatement(code);
            WebPageSurrogateControlBuilder.FixUpWriteStatement(stmt);
            Assert.AreEqual(" WriteLiteral(\"hello\"); ", stmt.Value);

            // __w.Write case
            code = " __w.Write(\"hello\"); ";
            stmt = new CodeSnippetStatement(code);
            WebPageSurrogateControlBuilder.FixUpWriteStatement(stmt);
            Assert.AreEqual(" WriteLiteral(\"hello\"); ", stmt.Value);
        }

        [TestMethod]
        public void FixUpClassNull() {
            WebPageSurrogateControlBuilder.FixUpClass(null, null);
        }

        [TestMethod]
        public void OnCodeGenerationCompleteTest() {
            Utils.RunInSeparateAppDomain(() => {
                var vpath = "/WebSite1/index.aspq";
                var contents = "<%@ Page Language=C# %>";
                Utils.SetupVirtualPathInAppDomain(vpath, contents);
                new WebPageSurrogateControlBuilderTest().FixUpClassTest(type => {
                    var ccu = new CodeCompileUnit();
                    ccu.Namespaces.Add(new CodeNamespace());

                    var builder = new MockControlBuilder() { VPath = vpath };
                    builder.ProcessGeneratedCode(ccu, null, type, null, null);
                    builder.CallOnCodeGenerationComplete();
                });
            });
        }

        [TestMethod]
        public void OnCodeGenerationCompleteControlBuilderTest() {
            Utils.RunInSeparateAppDomain(() => {
                var vpath = "/WebSite1/index.ascq";
                var contents = "<%@ Page Language=C# %>";
                Utils.SetupVirtualPathInAppDomain(vpath, contents);
                new WebPageSurrogateControlBuilderTest().FixUpClassTest(type => {
                    var ccu = new CodeCompileUnit();
                    ccu.Namespaces.Add(new CodeNamespace());

                    var builder = new MockUserControlBuilder() { VPath = vpath };
                    builder.ProcessGeneratedCode(ccu, null, type, null, null);
                    builder.CallOnCodeGenerationComplete();
                });
            });
        }

        public class MockControlBuilder : WebPageSurrogateControlBuilder {
            public string VPath { get; set; }
            public void CallOnCodeGenerationComplete() {
                base.OnCodeGenerationComplete();
            }

            internal override string GetPageVirtualPath() {
                return VPath;
            }
        }

        public class MockUserControlBuilder : WebUserControlSurrogateControlBuilder {
            public string VPath { get; set; }
            public void CallOnCodeGenerationComplete() {
                base.OnCodeGenerationComplete();
            }

            internal override string GetPageVirtualPath() {
                return VPath;
            }
        }

        [TestMethod]
        public void FixUpClassVirtualPathTest() {
            Utils.RunInSeparateAppDomain(() => {
                var vpath = "/WebSite1/index.aspq";
                var contents = "<%@ Page Language=C# %>";
                Utils.SetupVirtualPathInAppDomain(vpath, contents);

                new WebPageSurrogateControlBuilderTest().FixUpClassTest(type =>
                    WebPageSurrogateControlBuilder.FixUpClass(type, vpath));
            });
        }

        /// <summary>
        ///A test for FixUpClass
        ///</summary>
        [TestMethod()]
        public void FixUpClassLanguageTest() {
            FixUpClassTest(type =>
                WebPageSurrogateControlBuilder.FixUpClass(type, Language.CSharp));
        }

        [TestMethod]
        public void FixUpClassDefaultApplicationBaseTypeTest() {
            Utils.RunInSeparateAppDomain(() => {
                var baseTypeField = typeof(PageParser).GetField("s_defaultApplicationBaseType", BindingFlags.Static | BindingFlags.NonPublic);
                baseTypeField.SetValue(null, typeof(WebPageHttpApplication));
                var pageType = new WebPageSurrogateControlBuilderTest().FixUpClassTest(type =>
                    WebPageSurrogateControlBuilder.FixUpClass(type, Language.CSharp));
                var properties = pageType.Members.OfType<CodeMemberProperty>().ToList();
                var prop = properties[0];
                Assert.AreEqual(typeof(WebPageHttpApplication).FullName, prop.Type.BaseType);
            });
        }

        public CodeTypeDeclaration FixUpClassTest(Action<CodeTypeDeclaration> fixUpClassMethod) {
            var type = new CodeTypeDeclaration();
            // Add some dummy base types which should get removed
            type.BaseTypes.Add("basetype1");
            type.BaseTypes.Add("basetype2");
            type.BaseTypes.Add("basetype3");

            // Add a property which should get retained
            var appInstance = new CodeMemberProperty() { Name = "ApplicationInstance" };
            var returnStatement = new CodeMethodReturnStatement(new CodeCastExpression(typeof(HttpApplication), null));
            appInstance.GetStatements.Add(returnStatement);
            type.Members.Add(appInstance);

            // Add a render method which should get retained but modified
            var renderMethod = new CodeMemberMethod();
            renderMethod.Name = "__Render__control1";

            // Add a code snippet statement that should not be modified
            var stmt1 = new CodeSnippetStatement("MyCode.DoSomething");
            renderMethod.Statements.Add(stmt1);

            // Add a code snippet statement that should be modified
            var code2 = " @__w.Write(\"hello\"); ";
            var stmt2 = new CodeSnippetStatement(code2);
            renderMethod.Statements.Add(stmt2);

            // Add a method invoke statement that should be modified
            var invoke3 = new CodeMethodInvokeExpression(new CodeArgumentReferenceExpression("__w"), "Write");
            CodeExpressionStatement stmt3 = new CodeExpressionStatement(invoke3);
            WebPageSurrogateControlBuilder.FixUpWriteStatement(stmt3);
            renderMethod.Statements.Add(stmt3);

            type.Members.Add(renderMethod);

            // Snippets should get retained
            var snippet1 = "public void Test1() { }";
            var snippet2 = "public void Test2() { }";
            type.Members.Add(new CodeSnippetTypeMember(snippet1));
            type.Members.Add(new CodeSnippetTypeMember(snippet2));

            // Add dummy members which should get removed
            type.Members.Add(new CodeMemberProperty() { Name = "DummyProperty1" });
            type.Members.Add(new CodeMemberProperty() { Name = "DummyProperty2" });
            type.Members.Add(new CodeMemberMethod() { Name = "DummyMethod1" });
            type.Members.Add(new CodeMemberMethod() { Name = "DummyMethod2" });

            // Run the method we are testing
            fixUpClassMethod(type);

            // Basic verification
            Assert.AreEqual(1, type.BaseTypes.Count);
            Assert.AreEqual(4, type.Members.Count);

            // Verify properties
            var properties = type.Members.OfType<CodeMemberProperty>().ToList();
            Assert.AreEqual(1, properties.Count);
            Assert.AreEqual("ApplicationInstance", properties[0].Name);

            // Verify snippets
            var snippets = type.Members.OfType<CodeSnippetTypeMember>().ToList();
            Assert.AreEqual(2, snippets.Count);
            Assert.IsNotNull(snippets.Find(s => s.Text == snippet1));
            Assert.IsNotNull(snippets.Find(s => s.Text == snippet2));

            // Verify methods
            var methods = type.Members.OfType<CodeMemberMethod>().ToList();
            Assert.AreEqual(1, methods.Count);
            Assert.AreEqual("Execute", methods[0].Name);

            // Verify statements in the method
            var statements = methods[0].Statements;
            Assert.AreEqual(4, statements.Count); // The fourth statement is a snippet generated for use as helper.

            // First statement should be unchanged
            Assert.AreEqual(stmt1, statements[0]);

            // Second statement should be fixed to use WriteLiteral
            Assert.IsInstanceOfType(statements[1], typeof(CodeSnippetStatement));
            Assert.AreEqual(" WriteLiteral(\"hello\"); ", ((CodeSnippetStatement)statements[1]).Value);

            // Third statement should be fixed to use WriteLiteral
            Assert.IsInstanceOfType(statements[2], typeof(CodeExpressionStatement));
            var invokeExpr = ((CodeExpressionStatement)statements[2]).Expression;
            Assert.IsInstanceOfType(invokeExpr, typeof(CodeMethodInvokeExpression));
            var invoke = invokeExpr as CodeMethodInvokeExpression;
            Assert.AreEqual(invoke.Method.MethodName, "WriteLiteral");
            Assert.IsInstanceOfType(invoke.Method.TargetObject, typeof(CodeThisReferenceExpression));

            // Fourth statement should be a generated code snippet
            Assert.IsInstanceOfType(statements[3], typeof(CodeSnippetStatement));
            return type;
        }

        [TestMethod]
        public void FixUpAspxClassUserControlTest() {
            CodeTypeDeclaration type;
            CodeCastExpression cast;
            GetAspxClass(out type, out cast, typeof(WebUserControlSurrogate));
            WebPageSurrogateControlBuilder.FixUpAspxClass(type, "/test/test.ascx");
            Assert.AreEqual(typeof(UserControl).FullName, cast.TargetType.BaseType);
        }

        [TestMethod]
        public void FixUpClassUserControlTest() {
            CodeTypeDeclaration type;
            CodeCastExpression cast;
            GetAspxClass(out type, out cast, typeof(WebUserControlSurrogate));
            WebPageSurrogateControlBuilder.FixUpClass(type, "/test/test.ascx");
            Assert.AreEqual(typeof(UserControl).FullName, cast.TargetType.BaseType);
        }

        private static void GetAspxClass(out CodeTypeDeclaration type, out CodeCastExpression cast, Type surrogateType) {
            type = new CodeTypeDeclaration();
            type.BaseTypes.Add(new CodeTypeReference(surrogateType));
            var ctor = new CodeConstructor();
            cast = new CodeCastExpression();
            cast.TargetType = new CodeTypeReference(surrogateType);
            var prop = new CodePropertyReferenceExpression();
            prop.TargetObject = cast;
            var assign = new CodeAssignStatement();
            assign.Left = prop;
            ctor.Statements.Add(assign);
            type.Members.Add(ctor);
        }

        [TestMethod]
        public void ProcessScriptBlocksCSTest() {
            var pageType = new CodeTypeDeclaration("MyControl");
            var snippet = new CodeSnippetTypeMember("public int foo;");
            pageType.Members.Add(snippet);
            var renderMethod = new CodeMemberMethod();
            WebPageSurrogateControlBuilder.ProcessScriptBlocks(pageType, renderMethod, Language.CSharp);
            var snippets = renderMethod.Statements.OfType<CodeSnippetStatement>().ToList();
            Assert.AreEqual(1, snippets.Count);
            var snip = snippets[0];
            Assert.IsTrue(snip.Value.Contains("public static class MyControlExtensions"));
            Assert.IsTrue(snip.Value.Contains("public static HelperResult MyControl(this System.Web.Mvc.HtmlHelper htmlHelper, int foo = default(int))"));
            Assert.IsTrue(snip.Value.Contains("uc.foo = foo;"));
        }

        [TestMethod]
        public void ProcessScriptBlocksVBTest() {
            var pageType = new CodeTypeDeclaration("MyControl");
            var snippet = new CodeSnippetTypeMember("public foo as int");
            pageType.Members.Add(snippet);
            var renderMethod = new CodeMemberMethod();
            WebPageSurrogateControlBuilder.ProcessScriptBlocks(pageType, renderMethod, Language.VisualBasic);
            var snippets = renderMethod.Statements.OfType<CodeSnippetStatement>().ToList();
            Assert.AreEqual(1, snippets.Count);
            var snip = snippets[0];
            Assert.IsTrue(snip.Value.Contains("Public Module MyControlExtensions"));
            Assert.IsTrue(snip.Value.Contains("Public Function MyControl(htmlHelper As System.Web.Mvc.HtmlHelper, optional foo as int = Nothing) As HelperResult"));
            Assert.IsTrue(snip.Value.Contains("uc.foo = foo"));
        }

        [TestMethod]
        public void HasAspCodeTest() {
            Assert.IsTrue(new WebPageSurrogateControlBuilder().HasAspCode);
            Assert.IsTrue(new WebUserControlSurrogateControlBuilder().HasAspCode);
        }
    }

}

