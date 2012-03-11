/// <reference path="../Scripts/References.js" />
(function (global, upshot, undefined) {

    module("DataProvider tests");

    function getTestProvider(verifyGet, verifySubmit) {
        return {
            get: function (getParameters, queryParameters, success, error) {
                var queryResult = {
                    entities: [{ ID: 1, Name: "Mathew" }, { ID: 2, Name: "Amy"}],
                    totalCount: 2
                };
                if (verifyGet) {
                    verifyGet(getParameters, queryParameters);
                }
                success(queryResult);
            },
            submit: function (submitParameters, changeSet, success, error) {
                if (verifySubmit) {
                    verifySubmit(submitParameters, changeSet);
                }
                success(changeSet);
            }
        };
    }

    function getTestContext() {
        var dc = new upshot.DataContext(getTestProvider());
        upshot.metadata(getTestMetadata());
        return dc;
    }

    function getTestMetadata() {
        return { Contact: { key: ["ID"] } };
    }

    // Verify an offline type scenario where a DataProvider is used directly and results
    // are "offlined" and rehydrated back into the context (metadata and entities)
    test("Offline scenario", 4, function () {
        // execute a direct query using the dataprovider and simulate
        // caching of the results
        var provider = getTestProvider();
        var cachedEntities;
        provider.get({ operationName: "contacts" }, null, function (result) {
            cachedEntities = result.entities;
        });
        equal(2, cachedEntities.length);

        // create a new context and load the cached entities into it
        var dataContext = getTestContext();
        var mergedEntities = dataContext.merge(cachedEntities, "Contact", null);

        // verify that after merge, entities are annotated with their type
        var entitySet = dataContext.getEntitySet("Contact");
        var contact = entitySet.getEntities()[0];
        equal(contact.__type, "Contact");

        // verify that the "rehydrated" context is fully functional
        equal(entitySet.getEntityState(contact), upshot.EntityState.Unmodified);
        $.observable(contact).property("Name", "xyz");
        equal(entitySet.getEntityState(contact), upshot.EntityState.ClientUpdated);
    });

    test("Custom data provider", 4, function () {
        var verifySubmit = function (submitParameters, changeSet) {
            // verify we got the expected changeset
            equal(changeSet.length, 1);
            equal(changeSet[0].Entity.Name, "foo");
        };
        var provider = getTestProvider(null, verifySubmit);

        // create a datasource using the provider and verify an E2E query + update cycle
        var ds = upshot.RemoteDataSource({
            providerParameters: { operationName: "contacts" },
            entityType: "Contact",
            provider: provider,
            bufferChanges: true,
            refreshSuccess: function (entities) {
                equal(entities.length, 2);

                // modify an entity
                $.observable(entities[0]).property("Name", "foo");

                ds.commitChanges(function () {
                    ok(true);
                });
            }
        });
        upshot.metadata(getTestMetadata());
        ds.refresh();
    });

    test("Verify get/submit parameter handling", 15, function () {
        var verifySubmit = function (submitParameters, changeSet) {
            // verify "outer params" are pushed in
            equal(submitParameters.outerA, "outerA");
            equal(submitParameters.outerB, "outerB");

            // verify submit only params
            equal(submitParameters.submitA, "submitA");
            equal(submitParameters.submitB, "submitB");

            equal(submitParameters.getA, undefined);
            equal(submitParameters.getB, undefined);
        };
        var verifyGet = function (getParameters) {
            // verify "outer params" are pushed in
            equal(getParameters.outerA, "outerA");
            equal(getParameters.outerB, "outerB");

            // verify get only params
            equal(getParameters.getA, "getA");
            equal(getParameters.getB, "getB");

            equal(getParameters.submitA, undefined);
            equal(getParameters.submitB, undefined);

            // verify original provider parameter objects weren't modified
            equal(providerParameters.get.url, undefined);
        };
        var provider = getTestProvider(verifyGet, verifySubmit);

        var providerParameters = {
            outerA: "outerA",
            outerB: "outerB",
            get: {
                getA: "getA",
                getB: "getB"
            },
            submit: {
                submitA: "submitA",
                submitB: "submitB"
            }
        };

        var ds = upshot.RemoteDataSource({
            providerParameters: providerParameters,
            entityType: "Contact",
            provider: provider,
            bufferChanges: true,
            refreshSuccess: function (entities) {
                equal(entities.length, 2);

                // modify an entity
                $.observable(entities[0]).property("Name", "foo");

                ds.commitChanges(function () {
                    ok(true);
                });
            }
        });
        upshot.metadata(getTestMetadata());
        ds.refresh();
    });

})(this, upshot);