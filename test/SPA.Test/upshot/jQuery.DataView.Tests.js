/// <reference path="../Scripts/References.js" />
(function (global, upshot, undefined) {

    module("jQuery.dataview.Tests.js", {
        teardown: function () {
            testHelper.unmockAjax();
        }
    });

    // refreshStart
    test("refreshStart ctor", 3, function () {
        stop();
        dsTestDriver.simulateSuccessService();
        dsTestDriver.ds = $.upshot.remoteDataview({
            providerParameters: { url: "unused" },
            provider: upshot.riaDataProvider,
            refreshStart: dsTestDriver.onRefreshStart
        });
        dsTestDriver.ds.refresh(function () { start(); });
    });

    test("refreshStart bind", 4, function () {
        stop();
        dsTestDriver.simulateSuccessService();
        dsTestDriver.ds = $.upshot.remoteDataview({ providerParameters: { url: "unused"}, provider: upshot.riaDataProvider });
        $(dsTestDriver.ds).bind("refreshStart", dsTestDriver.onRefreshStartEvent);
        dsTestDriver.ds.refresh(function () { start(); });
    });

    // refreshSuccess
    test("refreshSuccess ctor", 6, function () {
        stop();
        dsTestDriver.simulateSuccessService();
        dsTestDriver.ds = $.upshot.remoteDataview({
            providerParameters: { url: "unused" },
            provider: upshot.riaDataProvider,
            refreshSuccess: dsTestDriver.onRefreshSuccess
        });
        dsTestDriver.ds.refresh();
    });

    test("refreshSuccess refresh simplest option", 6, function () {
        stop();
        dsTestDriver.simulateSuccessService();
        dsTestDriver.ds = $.upshot.remoteDataview({ providerParameters: { url: "unused"}, provider: upshot.riaDataProvider, })
            .refresh(dsTestDriver.onRefreshSuccess);
    });

    test("refreshSuccess bind", 7, function () {
        stop();
        dsTestDriver.simulateSuccessService();
        dsTestDriver.ds = $.upshot.remoteDataview({ providerParameters: { url: "unused"}, provider: upshot.riaDataProvider, });
        $(dsTestDriver.ds).one("refreshSuccess", dsTestDriver.onRefreshSuccessEvent);
        dsTestDriver.ds.refresh();
    });

    test("refreshSuccess localDataview ctor", 6, function () {
        stop();
        dsTestDriver.simulateSuccessService();
        dsTestDriver.ds = $.upshot.remoteDataview({
            providerParameters: { url: "unused" },
            provider: upshot.riaDataProvider,
            refreshSuccess: function () {
                dsTestDriver.ds = $.upshot.localDataview({
                    input: this,
                    paging: { limit: 2 },
                    refreshSuccess: dsTestDriver.onRefreshSuccess
                });
                setTimeout(function () { dsTestDriver.ds.refresh(); }, 10);
            }
        });
        dsTestDriver.ds.refresh();
    });

    test("refreshSuccess localDataview refresh simplest option", 6, function () {
        stop();
        dsTestDriver.simulateSuccessService();
        dsTestDriver.ds = $.upshot.remoteDataview({ providerParameters: { url: "unused"}, provider: upshot.riaDataProvider })
            .refresh(function () {
                dsTestDriver.ds = $.upshot.localDataview({
                    input: this,
                    paging: { limit: 2 }
                });
                setTimeout(function () { dsTestDriver.ds.refresh(dsTestDriver.onRefreshSuccess); }, 10);
            });
    });

    test("refreshSuccess localDataview bind", 7, function () {
        stop();
        dsTestDriver.simulateSuccessService();
        dsTestDriver.ds = $.upshot.remoteDataview({ providerParameters: { url: "unused"}, provider: upshot.riaDataProvider, });
        $(dsTestDriver.ds).one("refreshSuccess", function () {
            dsTestDriver.ds = $.upshot.localDataview({
                input: this,
                paging: { limit: 1 }
            });
            $(dsTestDriver.ds).one("refreshSuccess", dsTestDriver.onRefreshSuccessEvent);
            setTimeout(function () { dsTestDriver.ds.refresh(); }, 10);
        });
        dsTestDriver.ds.refresh();
    });

    // refreshError
    test("refreshError ctor", 5, function () {
        stop();
        dsTestDriver.simulateErrorService();
        dsTestDriver.ds = $.upshot.remoteDataview({
            providerParameters: { url: "unused" },
            provider: upshot.riaDataProvider,
            refreshError: dsTestDriver.onRefreshError
        }).refresh();
    });

    test("refreshError refresh simplest option", 5, function () {
        stop();
        dsTestDriver.simulateErrorService();
        dsTestDriver.ds = $.upshot.remoteDataview({ providerParameters: { url: "unused"}, provider: upshot.riaDataProvider, });
        dsTestDriver.ds.refresh(null, dsTestDriver.onRefreshError);
    });

    test("refreshError bind", 6, function () {
        stop();
        dsTestDriver.simulateErrorService();
        dsTestDriver.ds = $.upshot.remoteDataview({ providerParameters: { url: "unused"}, provider: upshot.riaDataProvider });
        $(dsTestDriver.ds).one("refreshError", dsTestDriver.onRefreshErrorEvent);
        dsTestDriver.ds.refresh();
    });

    // commitStart
    test("commitStart ctor", 4, function () {
        stop();
        dsTestDriver.simulateSuccessService();
        var products = [];
        dsTestDriver.ds = $.upshot.remoteDataview({
            providerParameters: { url: "unused" },
            provider: upshot.riaDataProvider,
            commitStart: dsTestDriver.onCommitStart,
            result: products,
            commitSuccess: function () { start(); }
        }).refresh(function () {
            equal(products.length, dsTestDriver.productsResult.GetProductsResult.TotalCount, "Count checked");
            dsTestDriver.simulatePostSuccessService();
            $.observable(products[0]).property("Price", products[0].Price + 100);
        });
    });

    test("commitStart bind ctor", 5, function () {
        stop();
        dsTestDriver.simulateSuccessService();
        var products = [];
        dsTestDriver.ds = $.upshot.remoteDataview({
            providerParameters: { url: "unused" },
            provider: upshot.riaDataProvider,
            result: products,
            commitSuccess: function () { start(); }
        });
        $(dsTestDriver.ds).bind("commitStart", dsTestDriver.onCommitStartEvent);
        dsTestDriver.ds.refresh(function () {
            equal(products.length, dsTestDriver.productsResult.GetProductsResult.TotalCount, "Count checked");
            dsTestDriver.simulatePostSuccessService();
            $.observable(products[0]).property("Price", products[0].Price + 100);
        });
    });

    // commitSuccess
    test("commitSuccess ctor", 6, function () {
        stop();
        dsTestDriver.simulateSuccessService();
        var products = [];
        dsTestDriver.ds = $.upshot.remoteDataview({
            providerParameters: { url: "unused" },
            provider: upshot.riaDataProvider,
            result: products,
            commitSuccess: dsTestDriver.onCommitSuccess
        }).refresh(function () {
            equal(products.length, dsTestDriver.productsResult.GetProductsResult.TotalCount, "Count checked");
            dsTestDriver.simulatePostSuccessService();
            $.observable(products[0]).property("Price", products[0].Price + 100);
        });
    });

    test("commitSuccess commit callback", 6, function () {
        stop();
        dsTestDriver.simulateSuccessService();
        var products = [];
        dsTestDriver.ds = $.upshot.remoteDataview({
            providerParameters: { url: "unused" },
            provider: upshot.riaDataProvider,
            result: products,
            bufferChanges: true
        });
        dsTestDriver.ds.refresh(function () {
            equal(products.length, dsTestDriver.productsResult.GetProductsResult.TotalCount, "Count checked");
            dsTestDriver.simulatePostSuccessService();
            $.observable(products[0]).property("Price", products[0].Price + 100);
            this.commitChanges(dsTestDriver.onCommitSuccess, dsTestDriver.onCommitError);
        });
    });

    test("commitSuccess bind", 7, function () {
        stop();
        dsTestDriver.simulateSuccessService();
        var products = [];
        dsTestDriver.ds = $.upshot.remoteDataview({
            providerParameters: { url: "unused" },
            provider: upshot.riaDataProvider,
            result: products
        });
        $(dsTestDriver.ds).bind("commitSuccess", dsTestDriver.onCommitSuccessEvent);
        dsTestDriver.ds.refresh(function () {
            equal(products.length, dsTestDriver.productsResult.GetProductsResult.TotalCount, "Count checked");
            dsTestDriver.simulatePostSuccessService();
            $.observable(products[0]).property("Price", products[0].Price + 100);
        });
    });

    // commitError
    test("commitError ctor", 6, function () {
        stop();
        dsTestDriver.simulateSuccessService();
        var products = [];
        dsTestDriver.ds = $.upshot.remoteDataview({
            providerParameters: { url: "unused" },
            provider: upshot.riaDataProvider,
            result: products,
            commitError: dsTestDriver.onCommitError
        }).refresh(function () {
            equal(products.length, dsTestDriver.productsResult.GetProductsResult.TotalCount, "Count checked");
            dsTestDriver.simulateErrorService();
            $.observable(products[0]).property("Price", products[0].Price + 100);
        });
    });

    test("commitError commit callback", 6, function () {
        stop();
        dsTestDriver.simulateSuccessService();
        var products = [];
        dsTestDriver.ds = $.upshot.remoteDataview({
            providerParameters: { url: "unused" },
            provider: upshot.riaDataProvider,
            result: products,
            bufferChanges: true
        });
        dsTestDriver.ds.refresh(function () {
            equal(products.length, dsTestDriver.productsResult.GetProductsResult.TotalCount, "Count checked");
            dsTestDriver.simulateErrorService();
            $.observable(products[0]).property("Price", products[0].Price + 100);
            this.commitChanges(dsTestDriver.onCommitSuccess, dsTestDriver.onCommitError);
        });
    });

    test("commitError bind", 7, function () {
        stop();
        dsTestDriver.simulateSuccessService();
        var products = [];
        dsTestDriver.ds = $.upshot.remoteDataview({
            providerParameters: { url: "unused" },
            provider: upshot.riaDataProvider,
            result: products
        });
        $(dsTestDriver.ds).bind("commitError", dsTestDriver.onCommitErrorEvent);
        dsTestDriver.ds.refresh(function () {
            equal(products.length, dsTestDriver.productsResult.GetProductsResult.TotalCount, "Count checked");
            dsTestDriver.simulateErrorService();
            $.observable(products[0]).property("Price", products[0].Price + 100);
        });
    });

    test("commitValidationError commit callback", 6, function () {
        stop();
        dsTestDriver.simulateSuccessService();
        var products = [];
        dsTestDriver.ds = $.upshot.remoteDataview({
            providerParameters: { url: "unused" },
            provider: upshot.riaDataProvider,
            result: products,
            bufferChanges: true
        });
        dsTestDriver.ds.refresh(function () {
            equal(products.length, dsTestDriver.productsResult.GetProductsResult.TotalCount, "Count checked");
            dsTestDriver.simulateValidationErrorService();
            $.observable(products).insert(0, { ID: 4, Manufacturer: "Kodak", Price: 800 });
            this.commitChanges(dsTestDriver.onCommitSuccess, dsTestDriver.onCommitValidationError);
        });
    });

})(this, upshot);
