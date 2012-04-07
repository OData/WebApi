// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.WebPages.Test {
    /// <summary>
    ///This is a test class for SectionControlBuilderTestand is intended
    ///to contain all SectionControlBuilder Unit Tests
    ///</summary>
    [TestClass()]
    public class SectionControlBuilderTest {

        private const int defaultIndex = 2 ;
        private const int defaultIndexAfterOffset = 4;

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

        [TestMethod]
        public void GetSectionRenderMethodTest() {
            var members = GetRenderMembers();
            var result = SectionControlBuilder.GetSectionRenderMethod(members, defaultIndex);
            Assert.AreEqual(members[0], result);
        }

        [TestMethod]
        public void GetSectionNameTest() {
            var sectionName = "MySectionName";
            var result = SectionControlBuilder.GetSectionName(GetBuildMembers(), defaultIndex);
            Assert.AreEqual(sectionName, result);
        }

        [TestMethod]
        public void GetSectionNameNullTest() {
            var index = defaultIndexAfterOffset;
            var methodName = "__BuildControl__control";
            Assert.IsNull(SectionControlBuilder.GetSectionName(new CodeTypeMember[] { }, defaultIndex));
            
            var method = new CodeMemberMethod();
            method = new CodeMemberMethod() { Name = methodName + index.ToString() };
            method.Statements.Add(new CodeSnippetStatement("test"));
            Assert.IsNull(SectionControlBuilder.GetSectionName(new CodeTypeMember[] { method }, defaultIndex));
            
            var statement = new CodeAssignStatement(null, null);
            method.Statements.Clear();
            method.Statements.Add(statement);
            Assert.IsNull(SectionControlBuilder.GetSectionName(new CodeTypeMember[] { method }, defaultIndex));

            var left = new CodePropertyReferenceExpression(null, "test");
            statement = new CodeAssignStatement(left, null);
            method.Statements.Clear();
            method.Statements.Add(statement);
            Assert.IsNull(SectionControlBuilder.GetSectionName(new CodeTypeMember[] { method }, defaultIndex));

            left = new CodePropertyReferenceExpression(null, "Name");
            statement = new CodeAssignStatement(left, null);
            method.Statements.Clear();
            method.Statements.Add(statement);
            Assert.IsNull(SectionControlBuilder.GetSectionName(new CodeTypeMember[] { method }, defaultIndex));

            left = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("test"), "Name");
            statement = new CodeAssignStatement(left, null);
            method.Statements.Clear();
            method.Statements.Add(statement);
            Assert.IsNull(SectionControlBuilder.GetSectionName(new CodeTypeMember[] { method }, defaultIndex));

        }

        [TestMethod]
        public void GetDefineSectionStatementsTest() {
            var renderMembers = GetRenderMembers();
            var buildMembers = GetBuildMembers();
            var members = new List<CodeTypeMember>(renderMembers);
            foreach (var m in buildMembers) {
                members.Add(m);
            }
            var result = SectionControlBuilder.GetDefineSectionStatements(members, defaultIndex, Language.CSharp);
            VerifyDefineSection(((CodeMemberMethod)renderMembers[0]).Statements[0], result);
        }

        private static void VerifyDefineSection(CodeStatement renderStatement, IList<CodeStatement> result) {
            Assert.AreEqual(3, result.Count);
            Assert.IsInstanceOfType(result[0], typeof(CodeSnippetStatement));
            var snippet = result[0] as CodeSnippetStatement;
            Assert.AreEqual("DefineSection(\"MySectionName\", delegate () {", snippet.Value);
            Assert.IsInstanceOfType(result[1], typeof(CodeExpressionStatement));
            Assert.AreEqual(renderStatement, result[1]);
            Assert.IsInstanceOfType(result[2], typeof(CodeSnippetStatement));
            snippet = result[2] as CodeSnippetStatement;
            Assert.AreEqual("});", snippet.Value);
        }

        public IList<CodeTypeMember> GetRenderMembers() {
            // Create a method of the following form:
            // void _Render__control4() {
            //   this.Write("Hello");
            // }
            var sectionName = "MySectionName";
            var methodName = "__Render__control";
            var expr = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "Write", new CodeExpression[] { new CodePrimitiveExpression("Hello") });
            var statement = new CodeExpressionStatement(expr);
            return GetBuildMembers(methodName, sectionName, defaultIndexAfterOffset, statement);
        }

        public IList<CodeTypeMember> GetBuildMembers() {
            // Create a method of the following form:
            // void __BuildControl__control4() {
            //   __ctrl.Name = "my_section_name";
            // }
            var sectionName = "MySectionName";
            var methodName = "__BuildControl__control";
            var left = new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("__ctrl"), "Name");
            var right = new CodePrimitiveExpression(sectionName);
            var statement = new CodeAssignStatement(left, right);
            return GetBuildMembers(methodName, sectionName, defaultIndexAfterOffset, statement);
        }

        public IList<CodeTypeMember> GetBuildMembers(string methodName, string sectionName, int index, CodeStatement statement = null) {

            var method = new CodeMemberMethod() { Name = methodName + index.ToString() };
            if (statement != null) {
                method.Statements.Add(statement);
            }
            return new CodeTypeMember[] { method };
        }

        [TestMethod]
        public void ProcessRenderControlMethodTest() {
            // Create a statement of the following form:
            // parameterContainer.Controls[0].RenderControl(@__w);
            // where parameterContainer is a parameter.
            var container = new CodeArgumentReferenceExpression("parameterContainer");
            var controls = new CodePropertyReferenceExpression(container, "Controls");
            var indexer = new CodeIndexerExpression(controls, new CodeExpression[] { new CodePrimitiveExpression(defaultIndex) });
            var invoke = new CodeMethodInvokeExpression(indexer, "RenderControl", new CodeExpression[] { new CodeArgumentReferenceExpression("__w") });
            var stmt = new CodeExpressionStatement(invoke);
            var renderMethod = new CodeMemberMethod();
            var renderMembers = GetRenderMembers();
            var buildMembers = GetBuildMembers();
            var members = new List<CodeTypeMember>(renderMembers);
            foreach (var m in buildMembers) {
                members.Add(m);
            }
            SectionControlBuilder.ProcessRenderControlMethod(members, renderMethod, stmt, Language.CSharp);
            VerifyDefineSection(((CodeMemberMethod)renderMembers[0]).Statements[0], renderMethod.Statements.OfType<CodeStatement>().ToList());
        }

        [TestMethod]
        public void ProcessRenderControlMethodFalseTest() {
            Assert.IsFalse(SectionControlBuilder.ProcessRenderControlMethod(null, null, null, Language.CSharp));
            Assert.IsFalse(SectionControlBuilder.ProcessRenderControlMethod(null, null, new CodeExpressionStatement(new CodeSnippetExpression("test")), Language.CSharp));

            var invoke = new CodeMethodInvokeExpression(null, "RenderControlX", new CodeExpression[] { new CodeArgumentReferenceExpression("__w") });
            var stmt = new CodeExpressionStatement(invoke);
            Assert.IsFalse(SectionControlBuilder.ProcessRenderControlMethod(null, null, stmt, Language.CSharp));

            invoke = new CodeMethodInvokeExpression(new CodeSnippetExpression(""), "RenderControl", new CodeExpression[] { new CodeArgumentReferenceExpression("__w") });
            stmt = new CodeExpressionStatement(invoke);
            Assert.IsFalse(SectionControlBuilder.ProcessRenderControlMethod(null, null, stmt, Language.CSharp));

            var indexer = new CodeIndexerExpression(new CodeSnippetExpression(""), new CodeExpression[] { new CodePrimitiveExpression(defaultIndex) });
            invoke = new CodeMethodInvokeExpression(indexer, "RenderControl", new CodeExpression[] { new CodeArgumentReferenceExpression("__w") });
            stmt = new CodeExpressionStatement(invoke);
            Assert.IsFalse(SectionControlBuilder.ProcessRenderControlMethod(null, null, stmt, Language.CSharp));

            var container = new CodeArgumentReferenceExpression("parameterContainer");
            var controls = new CodePropertyReferenceExpression(container, "Controls");
            indexer = new CodeIndexerExpression(controls, new CodeExpression[] { new CodeSnippetExpression("") });
            invoke = new CodeMethodInvokeExpression(indexer, "RenderControl", new CodeExpression[] { new CodeArgumentReferenceExpression("__w") });
            stmt = new CodeExpressionStatement(invoke);
            Assert.IsFalse(SectionControlBuilder.ProcessRenderControlMethod(null, null, stmt, Language.CSharp));

            controls = new CodePropertyReferenceExpression(container, "ControlsX");
            indexer = new CodeIndexerExpression(controls, new CodeExpression[] { new CodePrimitiveExpression(defaultIndex) });
            invoke = new CodeMethodInvokeExpression(indexer, "RenderControl", new CodeExpression[] { new CodeArgumentReferenceExpression("__w") });
            stmt = new CodeExpressionStatement(invoke);
            Assert.IsFalse(SectionControlBuilder.ProcessRenderControlMethod(null, null, stmt, Language.CSharp));

            controls = new CodePropertyReferenceExpression(new CodeSnippetExpression("test"), "Controls");
            indexer = new CodeIndexerExpression(controls, new CodeExpression[] { new CodePrimitiveExpression(defaultIndex) });
            invoke = new CodeMethodInvokeExpression(indexer, "RenderControl", new CodeExpression[] { new CodeArgumentReferenceExpression("__w") });
            stmt = new CodeExpressionStatement(invoke);
            Assert.IsFalse(SectionControlBuilder.ProcessRenderControlMethod(null, null, stmt, Language.CSharp));

            container = new CodeArgumentReferenceExpression("parameterContainerX");
            controls = new CodePropertyReferenceExpression(container, "Controls");
            indexer = new CodeIndexerExpression(controls, new CodeExpression[] { new CodePrimitiveExpression(defaultIndex) });
            invoke = new CodeMethodInvokeExpression(indexer, "RenderControl", new CodeExpression[] { new CodeArgumentReferenceExpression("__w") });
            stmt = new CodeExpressionStatement(invoke);
            Assert.IsFalse(SectionControlBuilder.ProcessRenderControlMethod(null, null, stmt, Language.CSharp));
        }

        [TestMethod]
        public void GetDefineSectionStartSnippetTest() {
            var snippetStmt = SectionControlBuilder.GetDefineSectionStartSnippet("HelloWorld", Language.CSharp) as CodeSnippetStatement;
            Assert.AreEqual("DefineSection(\"HelloWorld\", delegate () {", snippetStmt.Value);
            snippetStmt = SectionControlBuilder.GetDefineSectionStartSnippet("HelloWorld", Language.VisualBasic) as CodeSnippetStatement;
            Assert.AreEqual("DefineSection(\"HelloWorld\", Sub()", snippetStmt.Value);
        }

        [TestMethod]
        public void HasAspCodeTest() {
            Assert.IsTrue(new SectionControlBuilder().HasAspCode);
        }
   }
}