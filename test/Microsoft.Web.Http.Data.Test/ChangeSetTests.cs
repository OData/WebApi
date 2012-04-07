// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Web.Http.Data;
using Microsoft.Web.Http.Data.Test.Models;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.ServiceModel.DomainServices.Server.Test
{
    public class ChangeSetTests
    {
        /// <summary>
        /// Verify ChangeSet validation when specifying/requesting original for Insert operations.
        /// </summary>
        [Fact]
        public void Changeset_OriginalInvalidForInserts()
        {
            // can't specify an original for an insert operation
            Product curr = new Product { ProductID = 1 };
            Product orig = new Product { ProductID = 1 };
            ChangeSetEntry entry = new ChangeSetEntry { Id = 1, Entity = curr, OriginalEntity = orig, Operation = ChangeOperation.Insert };
            ChangeSet cs = null;
            Assert.Throws<InvalidOperationException>(delegate
            {
                cs = new ChangeSet(new ChangeSetEntry[] { entry });
            },
            String.Format(Resource.InvalidChangeSet, Resource.InvalidChangeSet_InsertsCantHaveOriginal));

            // get original should throw for insert operations
            entry = new ChangeSetEntry { Id = 1, Entity = curr, OriginalEntity = null, Operation = ChangeOperation.Insert };
            cs = new ChangeSet(new ChangeSetEntry[] { entry });
            Assert.Throws<InvalidOperationException>(delegate
            {
                cs.GetOriginal(curr);
            },
            String.Format(Resource.ChangeSet_OriginalNotValidForInsert));
        }

        [Fact]
        public void Constructor_HasErrors()
        {
            ChangeSet changeSet = this.GenerateChangeSet();
            Assert.False(changeSet.HasError);

            changeSet = this.GenerateChangeSet();
            changeSet.ChangeSetEntries.First().ValidationErrors = new List<ValidationResultInfo>() { new ValidationResultInfo("Error", new[] { "Error" }) };
            Assert.True(changeSet.HasError, "Expected ChangeSet to have errors");
        }

        [Fact]
        public void GetOriginal()
        {
            ChangeSet changeSet = this.GenerateChangeSet();
            ChangeSetEntry op = changeSet.ChangeSetEntries.First();

            Product currentEntity = new Product();
            Product originalEntity = new Product();

            op.Entity = currentEntity;
            op.OriginalEntity = originalEntity;

            Product changeSetOriginalEntity = changeSet.GetOriginal(currentEntity);

            // Verify we returned the original
            Assert.Same(originalEntity, changeSetOriginalEntity);
        }

        [Fact]
        public void GetOriginal_EntityExistsMoreThanOnce()
        {
            ChangeSet changeSet = this.GenerateChangeSet();
            ChangeSetEntry op1 = changeSet.ChangeSetEntries.Skip(0).First();
            ChangeSetEntry op2 = changeSet.ChangeSetEntries.Skip(1).First();
            ChangeSetEntry op3 = changeSet.ChangeSetEntries.Skip(2).First();

            Product currentEntity = new Product(), originalEntity = new Product();

            op1.Entity = currentEntity;
            op1.OriginalEntity = originalEntity;

            op2.Entity = currentEntity;
            op2.OriginalEntity = originalEntity;

            op3.Entity = currentEntity;
            op3.OriginalEntity = null;

            Product changeSetOriginalEntity = changeSet.GetOriginal(currentEntity);

            // Verify we returned the original
            Assert.Same(originalEntity, changeSetOriginalEntity);
        }

        [Fact]
        public void GetOriginal_InvalidArgs()
        {
            ChangeSet changeSet = this.GenerateChangeSet();

            Assert.ThrowsArgumentNull(
                () => changeSet.GetOriginal<Product>(null),
                "clientEntity");
        }

        [Fact]
        public void GetOriginal_EntityOperationNotFound()
        {
            ChangeSet changeSet = this.GenerateChangeSet();
            Assert.Throws<ArgumentException>(
                () => changeSet.GetOriginal(new Product()),
                Resource.ChangeSet_ChangeSetEntryNotFound);
        }

        private ChangeSet GenerateChangeSet()
        {
            return new ChangeSet(this.GenerateEntityOperations(false));
        }

        private IEnumerable<ChangeSetEntry> GenerateEntityOperations(bool alternateTypes)
        {
            List<ChangeSetEntry> ops = new List<ChangeSetEntry>(10);

            int id = 1;
            for (int i = 0; i < ops.Capacity; ++i)
            {
                object entity, originalEntity;

                if (!alternateTypes || i % 2 == 0)
                {
                    entity = new MockEntity1() { FullName = String.Format("FName{0} LName{0}", i) };
                    originalEntity = new MockEntity1() { FullName = String.Format("OriginalFName{0} OriginalLName{0}", i) };
                }
                else
                {
                    entity = new MockEntity2() { FullNameAndID = String.Format("FName{0} LName{0} ID{0}", i) };
                    originalEntity = new MockEntity2() { FullNameAndID = String.Format("OriginalFName{0} OriginalLName{0} OriginalID{0}", i) };
                }

                ops.Add(new ChangeSetEntry { Id = id++, Entity = entity, OriginalEntity = originalEntity, Operation = ChangeOperation.Update });
            }

            return ops;
        }

        public class MockStoreEntity
        {
            public int ID { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        public class MockEntity1
        {
            public string FullName { get; set; }
        }

        public class MockEntity2
        {
            public string FullNameAndID { get; set; }
        }

        public class MockDerivedEntity : MockEntity1
        {
        }
    }
}
