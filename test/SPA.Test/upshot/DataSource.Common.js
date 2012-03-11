/// <reference path="../Scripts/References.js" />


var dsTestDriver;

(function (global, upshot, undefined) {

    if ($.isPlainObject(dsTestDriver)) {
        return;
    }

    function createProductsResult () {
        return {
            GetProductsResult: {
                TotalCount: 3,
                RootResults: [
                    { ID: 1, Manufacturer: "Canon", Price: 200 },
                    { ID: 2, Manufacturer: "Nikon", Price: 400 },
                    { ID: 3, Manufacturer: "Pentax", Price: 500 }
                ],
                Metadata: [
                    {
                        type: "Product:#Sample.Models",
                        key: ["ID"],
                        fields: {
                            ID: { type: "Int32:#System" },
                            Manufacturer: { type: "String:#System" },
                            Price: { type: "Decimal:#System" }
                        },
                        rules: {
                            ID: { required: true },
                            Price: { range: [0, 1000] }
                        }
                    }
                ]
            }
        };
    }

    dsTestDriver = {
        ds: null,
        simulatedSuccess: true,
        errorStatus: "ErrorStatus",
        errorValue: "ErrorValue",
        validationError: {
            SubmitChangesResult: [{
                ValidationErrors: [{
                    Message: "The ID field is required!"
                }]
            }]   
        },

        simulateSuccessService: function (results) {
            testHelper.mockAjaxOnce("unused", this.productsResult = results || createProductsResult());
            this.simulatedSuccess = true;
        },
        simulatePostSuccessService: function (results) {
            testHelper.mockAjaxOnce("unused", results);
            this.simulatedSuccess = true;
        },
        simulateErrorService: function () {
            testHelper.mockAjaxOnce("unused", null, this.errorStatus, this.errorValue);
            this.simulatedSuccess = false;
        },
        simulateValidationErrorService: function () {
            testHelper.mockAjaxOnce("unused", this.validationError);
            this.simulatedSuccess = false;
        },

        onRefreshStartEvent: function (event) {
            equal(event.type, "refreshStart", "Event triggered");
            var args = Array.prototype.slice.call(arguments);
            args.shift();
            dsTestDriver.onRefreshStart.apply(this, args);
        },
        onRefreshStart: function () {
            ok(true, "Callback called");
            equal(arguments.length, 0, "Argument checked");
            ok(this === dsTestDriver.ds, "Context checked");
        },
        onRefreshSuccessEvent: function (event, entities, totalCount) {
            equal(event.type, "refreshSuccess", "Event triggered");
            var args = Array.prototype.slice.call(arguments);
            args.shift();
            dsTestDriver.onRefreshSuccess.apply(this, args);
        },
        onRefreshSuccess: function (entities, totalCount) {
            ok(true, "Callback called");
            ok(dsTestDriver.simulatedSuccess, "Simulation checked");
            equal(arguments.length, 2, "Argument checked");
            ok(this === dsTestDriver.ds, "Context checked");
            equal(totalCount, dsTestDriver.productsResult.GetProductsResult.TotalCount, "Count checked");
            var lastIndex = dsTestDriver.ds._take || (dsTestDriver.ds.dataSource && dsTestDriver.ds.dataSource._take) || totalCount;
            equal(entities[lastIndex - 1].Price, dsTestDriver.productsResult.GetProductsResult.RootResults[lastIndex - 1].Price, "Price checked");
            start();
        },
        onRefreshErrorEvent: function (event) {
            equal(event.type, "refreshError", "Event triggered");
            var args = Array.prototype.slice.call(arguments);
            args.shift();
            dsTestDriver.onRefreshError.apply(this, args);
        },
        onRefreshError: function (httpStatus, errorText, jqXHR) {
            ok(true, "Callback called");
            ok(!dsTestDriver.simulatedSuccess, "Simulation checked");
            equal(arguments.length, 3, "Argument checked");
            ok(this === dsTestDriver.ds, "Context checked");
            equal(errorText, dsTestDriver.errorValue, "error checked");
            start();
        },
        onCommitStartEvent: function (event) {
            equal(event.type, "commitStart", "Event triggered");
            var args = Array.prototype.slice.call(arguments);
            args.shift();
            dsTestDriver.onCommitStart.apply(this, args);
        },
        onCommitStart: function () {
            ok(true, "Callback called");
            equal(arguments.length, 0, "Argument checked");
            ok(this === dsTestDriver.ds, "Context checked");
        },
        onCommitSuccessEvent: function (event) {
            equal(event.type, "commitSuccess", "Event triggered");
            var args = Array.prototype.slice.call(arguments);
            args.shift();
            dsTestDriver.onCommitSuccess.apply(this, args);
        },
        onCommitSuccess: function () {
            ok(true, "Callback called");
            ok(dsTestDriver.simulatedSuccess, "Simulation checked");
            equal(arguments.length, 1, "Argument checked");
            var submitResults = arguments[0]; 
            equal(submitResults.length, 1, "submitResults checked");
            ok(this === dsTestDriver.ds, "Context checked");
            start();
        },
        onCommitErrorEvent: function (event) {
            equal(event.type, "commitError", "Event triggered");
            var args = Array.prototype.slice.call(arguments);
            args.shift();
            dsTestDriver.onCommitError.apply(this, args);
        },
        onCommitError: function (httpStatus, errorText, jqXHR, submitResult) {
            ok(true, "Callback called");
            ok(!dsTestDriver.simulatedSuccess, "Simulation checked");
            equal(arguments.length, 4, "Argument checked");
            ok(this === dsTestDriver.ds, "Context checked");
            equal(errorText, dsTestDriver.errorValue, "errorText checked");
            start();
        },
        onCommitValidationError: function (httpStatus, errorText, jqXHR, submitResult) {
            ok(true, "Callback called");
            ok(!dsTestDriver.simulatedSuccess, "Simulation checked");
            equal(arguments.length, 4, "Argument checked");
            ok(this === dsTestDriver.ds, "Context checked");
            equal(errorText, dsTestDriver.validationError.SubmitChangesResult[0].ValidationErrors[0].Message, "Validation text checked");
            start();
        }
    };

})(this, upshot);
