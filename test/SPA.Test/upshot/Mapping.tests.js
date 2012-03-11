/// <reference path="../Scripts/References.js" />

(function (upshot, $, ko, undefined) {

    var obsOld = upshot.observability.configuration;
    module("mapping.tests.js", {
        teardown: function () {
            upshot.observability.configuration = obsOld;
            testHelper.unmockAjax();
        }
    });

    function createRemoteDataSource(options) {
        options = $.extend({ providerParameters: { url: "unused", operationName: ""} }, options);
        return new upshot.RemoteDataSource(options);
    }

    function createCustomersResult() {
        return [
            {
                ID: 1,
                Name: "Joe",
                Orders: [
                    { ID: 97, ProductName: "Smarties", CustomerID: 1 }
                ]
            },
            {
                ID: 2,
                Name: "Stan",
                Orders: [
                    { ID: 99, ProductName: "Shreddies", CustomerID: 2 }
                ]
            },
            {
                ID: 3,
                Name: "Fred",
                Orders: [
                    { ID: 98, ProductName: "Wheatabix", CustomerID: 3 },
                    { ID: 96, ProductName: "Red Rose Tea", CustomerID: 3 }
                ]
            }
        ];
    }

    var customersMetadata = {
        Customer_Mapping: {
            key: ["ID"],
            fields: {
                ID: { type: "Int32:#System" },
                Name: { type: "String:#System" },
                Orders: {
                    type: "Order_Mapping",
                    array: true,
                    association: {
                        thisKey: ["ID"],
                        otherKey: ["CustomerID"]
                    }
                }
            }
        },
        Order_Mapping: {
            key: ["ID"],
            fields: {
                ID: { type: "Int32:#System" },
                ProductName: { type: "String:#System" },
                CustomerID: { type: "Int32:#System" },
                Customer: {
                    type: "Customer_Mapping",
                    association: {
                        isForeignKey: true,
                        thisKey: ["CustomerID"],
                        otherKey: ["ID"]
                    }
                }
            }
        },
        Entity_Mapping: {
            key: ["Id"],
            fields: {
                Id: { type: "Int32:#System" },
                String: { type: "String:#System" },
                Number: { type: "Int32:#System" }
            }
        },
        CT_Mapping: {
            fields: {
                String: { type: "String:#System" },
                Number: { type: "Int32:#System" },
                CT: { type: "CT_Mapping" }
            }
        }
    };

    function happyMapper(entityType) {
        return function (data) {
            var mapped = upshot.map(data, entityType);
            mapped.Happy = true;
            return mapped;
        }
    }

    (function () {
        var mappingOptions = [
            {
                entityType: "Customer_Mapping",
                mapping: happyMapper("Customer_Mapping")
            },
            {
                entityType: "Customer_Mapping",
                mapping: {
                    map: happyMapper("Customer_Mapping"),
                    unmap: function () { throw "Not reached"; }
                }
            },
            {
                entityType: "Customer_Mapping",
                mapping: {
                    Customer_Mapping: happyMapper("Customer_Mapping")
                }
            },
            {
                entityType: "Customer_Mapping",
                mapping: {
                    Customer_Mapping: {
                        map: happyMapper("Customer_Mapping"),
                        unmap: function () { throw "Not reached"; }
                    }
                }
            }
        ];
        for (var i = 0; i < mappingOptions.length; i++) {
            (function (options) {
                test("Verify customer parent mapping, default child mapping", 3, function () {
                    stop();

                    upshot.observability.configuration = upshot.observability.knockout;
                    upshot.metadata(customersMetadata);
                    dsTestDriver.simulateSuccessService(createCustomersResult());

                    var rds = createRemoteDataSource(options);
                    rds.refresh(function (entities) {
                        equal(entities[1].Orders()[0].ProductName(), "Shreddies", "Mapping and associations applied");
                        equal($.grep(entities, function (entity) { return entity.Happy; }).length, 3, "Customers mapped with custom mapping");
                        equal($.grep(rds.getDataContext().getEntitySet("Order_Mapping").getEntities()(), function (entity) { return entity.Happy; }).length, 0, "Orders mapped with default mapping");

                        start();
                    });
                });
            })(mappingOptions[i]);
        }
    })();

    (function () {
        var mappingOptions = [
            {
                entityType: "Customer_Mapping",
                mapping: {
                    Order_Mapping: happyMapper("Order_Mapping")
                }
            },
            {
                entityType: "Customer_Mapping",
                mapping: {
                    Order_Mapping: {
                        map: happyMapper("Order_Mapping"),
                        unmap: function () { throw "Not reached"; }
                    }
                }
            }
        ];
        for (var i = 0; i < mappingOptions.length; i++) {
            (function (options) {
                test("Verify default parent mapping, custom child mapping", 3, function () {
                    stop();

                    upshot.observability.configuration = upshot.observability.knockout;
                    upshot.metadata(customersMetadata);
                    dsTestDriver.simulateSuccessService(createCustomersResult());

                    var rds = createRemoteDataSource(options);
                    rds.refresh(function (entities) {
                        equal(entities[1].Orders()[0].ProductName(), "Shreddies", "Mapping and associations applied");
                        equal($.grep(entities, function (entity) { return entity.Happy; }).length, 0, "Customers mapped with default mapping");
                        equal($.grep(rds.getDataContext().getEntitySet("Order_Mapping").getEntities()(), function (entity) { return entity.Happy; }).length, 4, "Orders mapped with custom mapping");

                        start();
                    });
                });
            })(mappingOptions[i]);
        }
    })();

    function Customer(data) {
        this.ID = ko.observable(data.ID);
        this.Orders = upshot.map(data.Orders, "Order_Mapping");

        this.Happy = true;
    }

    test("Verify use of ctor as map", 3, function () {
        stop();

        upshot.observability.configuration = upshot.observability.knockout;
        upshot.metadata(customersMetadata);
        dsTestDriver.simulateSuccessService(createCustomersResult());

        var rds = createRemoteDataSource({
            entityType: "Customer_Mapping",
            mapping: Customer
        });
        rds.refresh(function (entities) {
            equal(entities[1].Orders()[0].ProductName(), "Shreddies", "Mapping and associations applied");
            equal($.grep(entities, function (entity) { return entity.Happy; }).length, 3, "Customers mapped with custom mapping");
            equal($.grep(rds.getDataContext().getEntitySet("Order_Mapping").getEntities()(), function (entity) { return entity.Happy; }).length, 0, "Orders mapped with default mapping");

            start();
        });
    });

    // TODO: Factor our KO test setup elsewhere and move this test to a better home.
    test("LDS over ASEV produces correct filtered result", 2, function () {
        stop();

        upshot.observability.configuration = upshot.observability.knockout;
        upshot.metadata(customersMetadata);
        dsTestDriver.simulateSuccessService(createCustomersResult());

        var rds = createRemoteDataSource({
            entityType: "Customer_Mapping",
            mapping: Customer
        });

        rds.refresh(function (entities) {
            var lds = new upshot.LocalDataSource({
                source: entities[2].Orders,
                filter: { property: "ProductName", value: "Wheatabix" }
            });
            lds.refresh(function (entities2) {
                ok(entities2.length === 1 && entities2[0].ProductName() === "Wheatabix", "Correct LDS refresh result");

                var lds2 = new upshot.LocalDataSource({
                    source: upshot.EntitySource.as(entities[2].Orders),
                    filter: { property: "ProductName", value: "Wheatabix" }
                });
                lds2.refresh(function (entities3) {
                    ok(entities3.length === 1 && entities3[0].ProductName() === "Wheatabix", "Correct LDS refresh result");

                    start();
                });
            });
        });
    });

    test("Default knockout mapping adds entity and updated properties", 2, function () {
        upshot.observability.configuration = upshot.observability.knockout;
        upshot.metadata(customersMetadata);

        var entity = upshot.map({
            Id: 1,
            String: "String",
            Number: 1
        }, "Entity_Mapping");

        equal(entity.hasOwnProperty("EntityState"), true, "Entity should have 'EntityState' property");
        equal(entity.String.hasOwnProperty("IsUpdated"), true, "String should have 'IsUpdated' property");
    });

    test("Default knockout mapping for a complex type adds updated properties", 4, function () {
        upshot.observability.configuration = upshot.observability.knockout;
        upshot.metadata(customersMetadata);

        var ct = upshot.map({
            String: "String",
            Number: 1,
            CT: {
                String: "String2",
                Number: 2,
                CT: null
            }
        }, "CT_Mapping");

        equal(ct.hasOwnProperty("EntityState"), false, "CT should not have 'EntityState' property");
        equal(ct.String.hasOwnProperty("IsUpdated"), true, "String should have 'IsUpdated' property");
        equal(ct.CT().hasOwnProperty("EntityState"), false, "Nested CT should not have 'EntityState' property");
        equal(ct.CT().String.hasOwnProperty("IsUpdated"), true, "Nested String should have 'IsUpdated' property");
    });

    test("upshot.addEntityProperties for knockout adds entity properties", 12, function () {
        upshot.observability.configuration = upshot.observability.knockout;
        upshot.metadata(customersMetadata);

        var entity = upshot.map({
            Id: 1,
            String: "String",
            Number: 1
        }, "Entity_Mapping");

        equal(entity.hasOwnProperty("EntityState"), true, "Entity should have 'EntityState' property");
        equal(entity.EntityState(), upshot.EntityState.Unmodified, "EntityState should be unmodified");
        equal(entity.hasOwnProperty("EntityError"), true, "Entity should have 'EntityError' property");
        equal(entity.EntityError(), null, "EntityError should be null");
        equal(entity.hasOwnProperty("IsUpdated"), true, "Entity should have 'IsUpdated' property");
        equal(entity.IsUpdated(), false, "IsUpdated should be false");
        equal(entity.hasOwnProperty("IsAdded"), true, "Entity should have 'IsAdded' property");
        equal(entity.IsAdded(), false, "IsAdded should be false");
        equal(entity.hasOwnProperty("IsDeleted"), true, "Entity should have 'IsDeleted' property");
        equal(entity.IsDeleted(), false, "IsDeleted should be false");
        equal(entity.hasOwnProperty("IsChanged"), true, "Entity should have 'IsChanged' property");
        equal(entity.IsChanged(), false, "IsChanged should be false");
    });

    test("upshot.addUpdatedProperties for knockout adds updated properties", 6, function () {
        upshot.observability.configuration = upshot.observability.knockout;
        upshot.metadata(customersMetadata);

        var entity = upshot.map({
            Id: 1,
            String: "String",
            Number: 1
        }, "Entity_Mapping");

        equal(entity.Id.hasOwnProperty("IsUpdated"), true, "Id should have 'IsUpdated' property");
        equal(entity.Id.IsUpdated(), false, "Id.IsUpdated should be false");
        equal(entity.String.hasOwnProperty("IsUpdated"), true, "String should have 'IsUpdated' property");
        equal(entity.String.IsUpdated(), false, "String.IsUpdated should be false");
        equal(entity.Number.hasOwnProperty("IsUpdated"), true, "Number should have 'IsUpdated' property");
        equal(entity.Number.IsUpdated(), false, "Number.IsUpdated should be false");
    });

    function getEntityStates() {
        var states = {};
        $.each(upshot.EntityState, function (key, value) {
            if (typeof value === "string") {
                states[value] = false;
            }
        });
        return states;
    }

    test("knockout entity.IsUpdated should reflect EntityState", 8, function () {
        upshot.observability.configuration = upshot.observability.knockout;
        upshot.metadata(customersMetadata);

        var entity = upshot.map({
            Id: 1,
            String: "String",
            Number: 1
        }, "Entity_Mapping");

        var states = getEntityStates();
        states[upshot.EntityState.ClientUpdated] = true;
        states[upshot.EntityState.ServerUpdating] = true;
        $.each(states, function (key, value) {
            entity.EntityState(key);
            equal(entity.IsUpdated(), value, "IsUpdated should be " + value + " for state " + key);
        });
    });

    test("knockout entity.IsAdded should reflect EntityState", 8, function () {
        upshot.observability.configuration = upshot.observability.knockout;
        upshot.metadata(customersMetadata);

        var entity = upshot.map({
            Id: 1,
            String: "String",
            Number: 1
        }, "Entity_Mapping");

        var states = getEntityStates();
        states[upshot.EntityState.ClientAdded] = true;
        states[upshot.EntityState.ServerAdding] = true;
        $.each(states, function (key, value) {
            entity.EntityState(key);
            equal(entity.IsAdded(), value, "IsAdded should be " + value + " for state " + key);
        });
    });

    test("knockout entity.IsDeleted should reflect EntityState", 8, function () {
        upshot.observability.configuration = upshot.observability.knockout;
        upshot.metadata(customersMetadata);

        var entity = upshot.map({
            Id: 1,
            String: "String",
            Number: 1
        }, "Entity_Mapping");

        var states = getEntityStates();
        states[upshot.EntityState.ClientDeleted] = true;
        states[upshot.EntityState.ServerDeleting] = true;
        states[upshot.EntityState.Deleted] = true;
        $.each(states, function (key, value) {
            entity.EntityState(key);
            equal(entity.IsDeleted(), value, "IsDeleted should be " + value + " for state " + key);
        });
    });

    test("knockout entity.IsChanged should reflect EntityState", 8, function () {
        upshot.observability.configuration = upshot.observability.knockout;
        upshot.metadata(customersMetadata);

        var entity = upshot.map({
            Id: 1,
            String: "String",
            Number: 1
        }, "Entity_Mapping");

        var states = getEntityStates();
        states[upshot.EntityState.Unmodified] = true;
        states[upshot.EntityState.Deleted] = true;
        $.each(states, function (key, value) {
            entity.EntityState(key);
            equal(entity.IsChanged(), !value, "IsChanged should be " + !value + " for state " + key);
        });
    });

    test("upshot.registerType and upshot.type use", 4, function () {
        upshot.registerType("FooType", function () { return Foo; });
        function Foo() {};
        equal(upshot.type(Foo), "FooType", "upshot.registerType works before key declaration");

        function Foo2() {};
        upshot.registerType("FooType", function () { return Foo2; });
        equal(upshot.type(Foo2), "FooType", "upshot.registerType works after key declaration");

        upshot.registerType({ Bar1Type: function () { return Bar1; }, Bar2Type: function () { return Bar2; } });
        function Bar1() {};
        function Bar2() {};
        ok(upshot.type(Bar1) === "Bar1Type" && upshot.type(Bar2) === "Bar2Type", "upshot.registerType supports multiple registrations");

        var exception;
        try {
            function Zip() {};
            upshot.type(Zip);
        } catch (ex) {
            exception = true;
        }
        ok(!!exception, "upshot.type with no preceding upshot.registerType throws exception");
    });

})(upshot, jQuery, ko);
