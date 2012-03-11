/// <reference path="../Scripts/References.js" />
(function (global, upshot, undefined) {

    module("DataSource.tests.js", {
        teardown: function () {
            testHelper.unmockAjax();
        }
    });

    function createRemoteDataSource(options) {
        options = $.extend({ providerParameters: { url: "unused", operationName: "" }, provider: upshot.riaDataProvider }, options || {});
        return new upshot.RemoteDataSource(options);
    }

    function createTestDataContext() {
        return new upshot.DataContext(new upshot.riaDataProvider());
    }

    // refreshStart
    test("refreshStart RemoteDataSource", 3, function () {
        stop();
        dsTestDriver.simulateSuccessService();
        dsTestDriver.ds = upshot.RemoteDataSource({ providerParameters: { url: "unused", operationName: "" }, provider: upshot.riaDataProvider });
        dsTestDriver.ds.bind("refreshStart", dsTestDriver.onRefreshStart);
        dsTestDriver.ds.refresh(function () { start(); });
    });

    test("refreshStart observer LocalDataSource over RemoteDataSource", 3, function () {
        stop();
        dsTestDriver.simulateSuccessService();
        dsTestDriver.ds = upshot.LocalDataSource({ source: createRemoteDataSource() });
        dsTestDriver.ds.bind({
            refreshStart: dsTestDriver.onRefreshStart,
            refreshSuccess: function () { start(); }
        });
        dsTestDriver.ds.refresh({ all: true });
    });

    test("refreshStart observer LocalDataSource over EntitySet", 3, function () {
        stop();
        dsTestDriver.simulateSuccessService();
        var dataContext = upshot.DataContext(new upshot.riaDataProvider());
        dataContext.__load({ providerParameters: { url: "unused", operationName: ""} }, function (entitySet) {
            dsTestDriver.ds = new upshot.LocalDataSource({ source: entitySet });
            dsTestDriver.ds.bind("refreshStart", dsTestDriver.onRefreshStart);
            dsTestDriver.ds.bind("refreshSuccess", function () { start(); });
            dsTestDriver.ds.refresh();
        });
    });

    // refreshSuccess
    test("refreshSuccess RemoteDataSource", 6, function () {
        stop();
        dsTestDriver.simulateSuccessService();
        dsTestDriver.ds = createRemoteDataSource();
        dsTestDriver.ds.refresh(dsTestDriver.onRefreshSuccess, dsTestDriver.onRefreshError);
    });

    test("refreshSuccess observer RemoteDataSource", 6, function () {
        stop();
        dsTestDriver.simulateSuccessService();
        dsTestDriver.ds = createRemoteDataSource();
        dsTestDriver.ds.bind("refreshSuccess", dsTestDriver.onRefreshSuccess);
        dsTestDriver.ds.bind("refreshError", dsTestDriver.onRefreshError);
        dsTestDriver.ds.refresh();
    });

    test("refreshSuccess LocalDataSource over RemoteDataSource", 6, function () {
        stop();
        dsTestDriver.simulateSuccessService();
        dsTestDriver.ds = new upshot.LocalDataSource({ source: createRemoteDataSource() });
        dsTestDriver.ds.refresh({ all: true }, dsTestDriver.onRefreshSuccess, dsTestDriver.onRefreshError);
    });

    test("refreshSuccess observer LocalDataSource over RemoteDataSource", 6, function () {
        stop();
        dsTestDriver.simulateSuccessService();
        dsTestDriver.ds = new upshot.LocalDataSource({ source: createRemoteDataSource() })
                                .bind("refreshSuccess", dsTestDriver.onRefreshSuccess)
                                .bind("refreshError", dsTestDriver.onRefreshError);
        dsTestDriver.ds.refresh({ all: true });
    });

    test("refreshSuccess LocalDataSource over EntitySet", 6, function () {
        stop();
        dsTestDriver.simulateSuccessService();
        var dataContext = createTestDataContext();
        dataContext.__load({ providerParameters: { url: "unused"} }, function (entitySet) {
            dsTestDriver.ds = new upshot.LocalDataSource({ source: entitySet });
            dsTestDriver.ds.refresh(dsTestDriver.onRefreshSuccess, dsTestDriver.onRefreshError);
        });
    });

    test("refreshSuccess observer LocalDataSource over EntitySet", 6, function () {
        stop();
        dsTestDriver.simulateSuccessService();
        var dataContext = createTestDataContext();
        dataContext.__load({ providerParameters: { url: "unused"} }, function (entitySet) {
            dsTestDriver.ds = new upshot.LocalDataSource({ source: entitySet })
                                    .bind({
                                        refreshSuccess: dsTestDriver.onRefreshSuccess,
                                        refreshError: dsTestDriver.onRefreshError
                                    });
            dsTestDriver.ds.refresh();
        });
    });

    // refreshError
    test("refreshError RemoteDataSource", 5, function () {
        stop();
        dsTestDriver.simulateErrorService();
        dsTestDriver.ds = createRemoteDataSource();
        dsTestDriver.ds.refresh(dsTestDriver.onRefreshSuccess, dsTestDriver.onRefreshError);
    });

    test("refreshError observer RemoteDataSource", 5, function () {
        stop();
        dsTestDriver.simulateErrorService();
        dsTestDriver.ds = createRemoteDataSource();
        dsTestDriver.ds.bind("refreshSuccess", dsTestDriver.onRefreshSuccess);
        dsTestDriver.ds.bind("refreshError", dsTestDriver.onRefreshError);
        dsTestDriver.ds.refresh();
    });

    test("refreshError LocalDataSource over RemoteDataSource", 5, function () {
        stop();
        dsTestDriver.simulateErrorService();
        dsTestDriver.ds = new upshot.LocalDataSource({ source: createRemoteDataSource() });
        dsTestDriver.ds.refresh({ all: true }, dsTestDriver.onRefreshSuccess, dsTestDriver.onRefreshError);
    });

    test("refreshError observer LocalDataSource over RemoteDataSource", 5, function () {
        stop();
        dsTestDriver.simulateErrorService();
        dsTestDriver.ds = new upshot.LocalDataSource({ source: createRemoteDataSource() });
        dsTestDriver.ds.bind("refreshSuccess", dsTestDriver.onRefreshSuccess);
        dsTestDriver.ds.bind("refreshError", dsTestDriver.onRefreshError);
        dsTestDriver.ds.refresh({ all: true });
    });

    // entityChanged
    asyncTest("entityChanged event is raised on a successful update", 12, function () {
        var listener = {
            events: [],
            onEntityUpdated: function (entity, path, eventArgs) {
                listener.events.push({ entity: entity, path: path, eventArgs: eventArgs });
            }
        };
        dsTestDriver.ds = new upshot.RemoteDataSource({ providerParameters: { url: "unused", operationName: "" }, provider: upshot.riaDataProvider, entityType: "Product:#Sample.Models", bufferChanges: true });
        dsTestDriver.ds.bind("entityUpdated", listener.onEntityUpdated);

        dsTestDriver.simulateSuccessService();
        dsTestDriver.ds.refresh(function (entities) {
            var entity = entities[0];

            equal(dsTestDriver.ds.isUpdated(entity), false, "The entity should not have changes");
            equal(listener.events.length, 0, "There should have been one event");

            $.observable(entity).property("Price", 700);

            equal(dsTestDriver.ds.isUpdated(entity), true, "The entity should have changes");
            equal(listener.events.length, 1, "There should have been one event");
            equal(listener.events[0].entity, entity, "The event should have been for the entity");
            equal(listener.events[0].path, "", "The event should have been directly on the entity");
            equal(listener.events[0].eventArgs.newValues.Price, 700, "The event should have been a property change");

            listener.events.length = 0;

            dsTestDriver.simulatePostSuccessService();
            dsTestDriver.ds.commitChanges(function () {
                equal(dsTestDriver.ds.isUpdated(entity), false, "The entity should no longer have changes");
                equal(listener.events.length, 1, "There should have been one event");
                equal(listener.events[0].entity, entity, "The event should have been for the entity");
                equal(listener.events[0].path, undefined, "The event should not include a path");
                equal(listener.events[0].eventArgs, undefined, "The event should not include args");
                start();
            });
        });
    });

    test("insert without initial refresh over RemoteDataSource", 5, function () {
        stop();

        dsTestDriver.ds = new upshot.RemoteDataSource({
            providerParameters: { url: "unused" },
            provider: upshot.riaDataProvider,
            entityType: "Foo"
        });
        dsTestDriver.ds.bind("commitSuccess", dsTestDriver.onCommitSuccess);
        upshot.metadata("Foo", { key: ["FooId"] });

        dsTestDriver.simulatePostSuccessService();
        $.observable(dsTestDriver.ds.getEntities()).insert({ FooId: 1 });
    });

    test("revertChanges without initial refresh over RemoteDataSource", 2, function () {
        stop();

        var entities = [],
            ds = new upshot.RemoteDataSource({
                entityType: "Foo",
                result: entities,
                bufferChanges: true,
                provider: upshot.riaDataProvider
            });
        upshot.metadata("Foo", { key: ["FooId"] });

        $.observable(entities).insert({});
        equal(entities.length, 1);

        ds.revertChanges();
        equal(entities.length, 0);

        start();
    });

    test("DataSource reset", 23, function () {
        stop();
        dsTestDriver.simulateSuccessService();

        var rds = dsTestDriver.ds = createRemoteDataSource();
        rds.refresh(function () {
            stop();
            dsTestDriver.onRefreshSuccess.apply(this, arguments);

            var lds = upshot.LocalDataSource({ source: rds });
            lds.refresh(function () {

                // RDS and LDS should be loaded and in sync.
                equal(lds.refreshNeeded(), false, "LocalDataSource should not need refresh");
                ok(upshot.sameArrayContents(rds.getEntities(), lds.getEntities()), "LocalDataSource refreshed properly");

                rds.reset();
                // RDS should have been reset.  LDS should need a refresh.
                equal(rds.getEntities().length, 0, "RemoteDataSource has no entities");
                equal(rds.getTotalEntityCount(), undefined, "RemoteDataSource has no total count");
                equal(lds.refreshNeeded(), true, "LocalDataSource should need a refresh");

                lds.reset();
                // LDS should have been reset.
                equal(lds.getEntities().length, 0, "LocalDataSource has no entities");
                equal(lds.getTotalEntityCount(), undefined, "LocalDataSource has no total count");

                // Put the RDS back in its loaded state.
                dsTestDriver.simulateSuccessService();
                rds.refresh(function () {
                    stop();
                    dsTestDriver.onRefreshSuccess.apply(this, arguments);

                    // Put the LDS back in sync with the RDS.
                    equal(lds.refreshNeeded(), true, "LocalDataSource should need a refresh");
                    lds.refresh(function () {
                        ok(upshot.sameArrayContents(rds.getEntities(), lds.getEntities()), "LocalDataSource refreshed properly");

                        lds.reset();
                        // LDS should have been reset.
                        equal(lds.getEntities().length, 0, "LocalDataSource has no entities");
                        equal(lds.getTotalEntityCount(), undefined, "LocalDataSource has no total count");

                        start();
                    });
                });
            });
        });
    });

    test("refresh with edits", 5, function () {
        stop();

        var rds = dsTestDriver.ds = createRemoteDataSource();

        dsTestDriver.simulateSuccessService();
        rds.refresh(function (entities) {
            ok(entities.length === 3, "Successful initial refresh");

            var entity = entities[0];
            $.observable(entity).property("Manufacturer", "Foo");

            var exception;
            try {
                rds.refresh();
            } catch (ex) {
                exception = true;
            }
            ok(!!exception, "Can't load with edits to DataSource entities");
            rds.revertChanges();

            setTimeout(function () {
                var emptyProductsResult = {
                    GetProductsResult: {
                        TotalCount: 3,
                        RootResults: [],
                        Metadata: [ $.extend({}, { type: "Product:#Sample.Models" }, upshot.metadata()["Product:#Sample.Models"]) ]
                    }
                };
                dsTestDriver.simulateSuccessService(emptyProductsResult);
                rds.refresh(function (entities) {
                    ok(entities.length === 0, "'Canon' isn't in filtered result");

                    $.observable(entity).property("Manufacturer", "Foo");

                    setTimeout(function () {
                        var exception;
                        try {
                            dsTestDriver.simulateSuccessService(emptyProductsResult);
                            rds.refresh(function(entities) {
                                ok(entities.length === 0, "Can load with edits not included in DataSource entities");
                                start();
                            });
                        } catch (ex) {
                            exception = true;
                        }
                        ok(!exception, "Can load with edits not included in DataSource entities");
                    }, 0);
                });
            }, 0);
        });
    });

    test("refresh with edits and 'allowRefreshWithEdits' option", 3, function () {
        stop();

        var rds = dsTestDriver.ds = createRemoteDataSource({ allowRefreshWithEdits: true });

        dsTestDriver.simulateSuccessService();
        rds.refresh(function (entities) {
            ok(entities.length === 3, "Successful initial refresh");

            var entity = entities[0];
            $.observable(entity).property("Manufacturer", "Foo");

            setTimeout(function () {
                var exception;
                try {
                    dsTestDriver.simulateSuccessService();
                    rds.refresh(function (entities) {
                        ok(entities.length === 3, "Can load with edits to DataSource entities (using allowRefreshWithEdits)");
                        start();
                    });
                } catch (ex) {
                    exception = true;
                }

                ok(!exception, "Can load with edits to DataSource entities (using allowRefreshWithEdits)");
            }, 0);
        });
    });

})(this, upshot);
