/// <reference path="../Scripts/References.js" />

(function (upshot, ko, undefined) {

    module("ChangeTracking");

    var observability = upshot.observability;

    test("Explicit commit multiple property edits", 5, function () {
        var submitCount = 0;
        var propertyChangedCount = 0;

        var dc = createTestContext(function () {
            submitCount++;
        }, true);

        var entitySet = dc.getEntitySet("Product");
        var prod = entitySet.getEntities()[0];
        var state = entitySet.getEntityState(prod);
        equal(state, upshot.EntityState.Unmodified);

        entitySet.bind("propertyChanged", function () {
            propertyChangedCount++;
        });

        $.observable(prod).property("Name", "xyz");
        $.observable(prod).property("Name", "foo");
        $.observable(prod).property("Price", 9.99);

        equal(propertyChangedCount, 3);

        state = entitySet.getEntityState(prod);
        equal(state, upshot.EntityState.ClientUpdated);

        equal(submitCount, 0);

        dc.commitChanges();

        equal(submitCount, 1);
    });

    asyncTest("Implicit commit multiple property edits", 5, function () {
        var submitCount = 0;
        var propertyChangedCount = 0;

        var dc = createTestContext(function () {
            submitCount++;
        }, false);

        var entitySet = dc.getEntitySet("Product");
        var prod = entitySet.getEntities()[0];
        var state = entitySet.getEntityState(prod);
        equal(state, upshot.EntityState.Unmodified);

        entitySet.bind("propertyChanged", function () {
            propertyChangedCount++;
        });

        $.observable(prod).property("Name", "xyz");
        $.observable(prod).property("Name", "foo");
        $.observable(prod).property("Price", 9.99);

        // before the timeout has expired we shouldn't have committed anything
        equal(submitCount, 0);

        equal(propertyChangedCount, 3);

        state = entitySet.getEntityState(prod);
        equal(state, upshot.EntityState.ClientUpdated);

        // Here we queue the test verification and start so that it runs
        // AFTER the queued commit
        setTimeout(function () {
            equal(submitCount, 1);
            start();
        }, 0);

    });

    asyncTest("Implicit commit multiple array operations", 5, function () {
        var submitCount = 0;

        var dc = createTestContext(function (options, editedEntities) {
            submitCount++;
            equal(editedEntities.length, 2);
        }, false);

        var entitySet = dc.getEntitySet("Product");
        var prod = entitySet.getEntities()[0];

        // do an insert
        var newProd = {
            ID: 2,
            Name: "Frish Gnarbles",
            Category: "Snacks",
            Price: 7.99
        };
        $.observable(entitySet.getEntities()).insert(newProd);
        var state = entitySet.getEntityState(newProd);
        equal(state, upshot.EntityState.ClientAdded);

        // do a delete
        entitySet.deleteEntity(prod);
        state = entitySet.getEntityState(prod);
        equal(state, upshot.EntityState.ClientDeleted);

        // before the timeout has expired we shouldn't have committed anything
        equal(submitCount, 0);

        // Here we queue the test verification and start so that it runs
        // AFTER the queued commit
        setTimeout(function () {
            equal(submitCount, 1);
            start();
        }, 0);
    });

/* TODO: We forced managed associations on for Dev11 beta, since unmanaged associations are broken.
    test("Nested entities can be added to an entity, and navigation properties are untracked", 16, function () {
        try {
            upshot.observability.configuration = observability.knockout;

            var manageAssociations = false;
            var dc = createKoTestContext(function () { }, false, manageAssociations);

            var orders = dc.getEntitySet("Order"),
                orderDetails = dc.getEntitySet("OrderDetail"),
                order = orders.getEntities()()[0],
                order2 = orders.getEntities()()[1],
                orderDetail = orderDetails.getEntities()()[0];

            order.OrderDetails.remove(orderDetail);
            equal(1, order.OrderDetails().length, "There should only be a single order detail");
            equal(upshot.EntityState.Unmodified, orders.getEntityState(order), "The order should not be modified");
            equal(upshot.EntityState.Unmodified, orderDetails.getEntityState(orderDetail), "The order detail should not be modified");

            order.OrderDetails.push(orderDetail);
            equal(2, order.OrderDetails().length, "There should be two order details");
            equal(upshot.EntityState.Unmodified, orders.getEntityState(order), "The order should not be modified");
            equal(upshot.EntityState.Unmodified, orderDetails.getEntityState(orderDetail), "The order detail should not be modified");

            orderDetail.Order(order2);
            equal(orderDetail.OrderId(), order2.Id(), "Ids should be equal");
            equal(upshot.EntityState.Unmodified, orders.getEntityState(order2), "The order should not be modified");
            equal(upshot.EntityState.ClientUpdated, orderDetails.getEntityState(orderDetail), "The order detail should not be modified");
            equal(true, orderDetails.isUpdated(orderDetail, "OrderId"), "The OrderId should be modified");
            equal(false, orderDetails.isUpdated(orderDetail, "Order"), "The Order should not be tracked");

            var properties = [];
            $.each(observability.knockout.unmap(orderDetail, "OrderDetail"), function (key, value) {
                properties.push(key);
                equal(ko.utils.unwrapObservable(orderDetail[key]), value, "Properties should be equal");
            });
            equal(properties.length, 4, "The should be 4 serialized properties");

        } finally {
            upshot.observability.configuration = observability.jquery;
        }
    });
*/

    test("Nested entities can be added to an entity, and navigation properties are computed", 39, function () {
        try {
            upshot.observability.configuration = observability.knockout;

            var dc = createKoTestContext(function () { }, false);

            var orders = dc.getEntitySet("Order"),
                orderDetails = dc.getEntitySet("OrderDetail"),
                order = orders.getEntities()()[0],
                order2 = orders.getEntities()()[1],
                orderDetail = orderDetails.getEntities()()[0];

            ok(order.OrderDetails.indexOf(orderDetail) >= 0 && order2.OrderDetails.indexOf(orderDetail) < 0 && orderDetail.Order() === order, "The order detail is a child of order1");
            equal(orders.getEntityState(order), upshot.EntityState.Unmodified, "order1 should not be modified");
            equal(orders.getEntityState(order2), upshot.EntityState.Unmodified, "order2 should not be modified");
            equal(orderDetails.getEntityState(orderDetail), upshot.EntityState.Unmodified, "The order detail should not be modified");
            equal(orderDetails.isUpdated(orderDetail, "OrderId"), false, "The OrderId should not be modified");
            equal(orderDetails.isUpdated(orderDetail, "Order"), false, "The Order should not be tracked");

            orderDetail.OrderId(order2.Id());
            ok(order2.OrderDetails.indexOf(orderDetail) >= 0 && order.OrderDetails.indexOf(orderDetail) < 0 && orderDetail.Order() === order2, "The order detail is a child of order2");
            equal(orders.getEntityState(order), upshot.EntityState.Unmodified, "order1 should not be modified");
            equal(orders.getEntityState(order2), upshot.EntityState.Unmodified, "order2 should not be modified");
            equal(orderDetails.getEntityState(orderDetail), upshot.EntityState.ClientUpdated, "The order detail should be ClientUpdated");
            equal(orderDetails.isUpdated(orderDetail, "OrderId"), true, "The OrderId should be modified");
            equal(orderDetails.isUpdated(orderDetail, "Order"), false, "The Order should not be tracked");

            orderDetails.revertUpdates(orderDetail);
            ok(order.OrderDetails.indexOf(orderDetail) >= 0 && order2.OrderDetails.indexOf(orderDetail) < 0 && orderDetail.Order() === order, "The order detail is a child of order1");
            equal(orders.getEntityState(order), upshot.EntityState.Unmodified, "order1 should not be modified");
            equal(orders.getEntityState(order2), upshot.EntityState.Unmodified, "order2 should not be modified");
            equal(orderDetails.getEntityState(orderDetail), upshot.EntityState.Unmodified, "The order detail should not be modified");
            equal(orderDetails.isUpdated(orderDetail, "OrderId"), false, "The OrderId should not be modified");
            equal(orderDetails.isUpdated(orderDetail, "Order"), false, "The Order should not be tracked");

            orderDetail.Order(order2);
            ok(orderDetail.OrderId() === order2.Id(), "Ids should be equal");
            equal(orders.getEntityState(order2), upshot.EntityState.Unmodified, "The order should not be modified");
            equal(orderDetails.getEntityState(orderDetail), upshot.EntityState.ClientUpdated, "The order detail should be ClientUpdated");
            equal(orderDetails.isUpdated(orderDetail, "OrderId"), true, "The OrderId should be modified");
            equal(orderDetails.isUpdated(orderDetail, "Order"), false, "The Order should not be tracked");

            orderDetails.revertUpdates(orderDetail);
            ok(order.OrderDetails.indexOf(orderDetail) >= 0 && order2.OrderDetails.indexOf(orderDetail) < 0 && orderDetail.Order() === order, "The order detail is a child of order1");
            equal(orders.getEntityState(order), upshot.EntityState.Unmodified, "order1 should not be modified");
            equal(orders.getEntityState(order2), upshot.EntityState.Unmodified, "order2 should not be modified");
            equal(orderDetails.getEntityState(orderDetail), upshot.EntityState.Unmodified, "The order detail should not be modified");
            equal(orderDetails.isUpdated(orderDetail, "OrderId"), false, "The OrderId should not be modified");
            equal(orderDetails.isUpdated(orderDetail, "Order"), false, "The Order should not be tracked");

            orderDetail.Order(null);
            equal(orderDetail.OrderId(), null, "FK should be null");
            equal(orders.getEntityState(order), upshot.EntityState.Unmodified, "The old order should not be modified");
            equal(orderDetails.getEntityState(orderDetail), upshot.EntityState.ClientUpdated, "The order detail should be ClientUpdated");
            equal(orderDetails.isUpdated(orderDetail, "OrderId"), true, "The OrderId should be modified");
            equal(orderDetails.isUpdated(orderDetail, "Order"), false, "The Order should not be tracked");

            var properties = [];
            $.each(observability.knockout.unmap(orderDetail, "OrderDetail"), function (key, value) {
                properties.push(key);
                equal(ko.utils.unwrapObservable(orderDetail[key]), value, "Properties should be equal");
            });
            equal(properties.length, 4, "The should be 4 serialized properties");

        } finally {
            upshot.observability.configuration = observability.jquery;
        }
    });

    test("Nested entities can be added to an entity, and property changes do not bubble to the parent", 2, function () {
        try {
            upshot.observability.configuration = observability.knockout;

            var dc = createKoTestContext(function () { }, false);

            var orders = dc.getEntitySet("Order"),
                orderDetails = dc.getEntitySet("OrderDetail"),
                order = orders.getEntities()()[0],
                orderDetail = orderDetails.getEntities()()[0];

            orderDetail.Name("asdf");
            equal(orders.getEntityState(order), upshot.EntityState.Unmodified, "The order should not be modified");
            equal(orderDetails.getEntityState(orderDetail), upshot.EntityState.ClientUpdated, "The order detail should not be modified");
        } finally {
            upshot.observability.configuration = observability.jquery;
        }
    });

    // Create and return a context using the specified submitChanges mock
    function createTestContext(submitChangesMock, bufferChanges) {
        var dataProvider = new upshot.DataProvider();
        var implicitCommitHandler;
        if (!bufferChanges) {
            implicitCommitHandler = function () {
                dc._commitChanges({ providerParameters: {} });
            }
        }
        var dc = new upshot.DataContext(dataProvider, implicitCommitHandler);
        dc._submitChanges = submitChangesMock;

        // add a single product to the context
        var type = "Product";
        var products = [];
        products.push({
            ID: 1,
            Name: "Crispy Snarfs",
            Category: "Snacks",
            Price: 12.99
        });
        products.push({
            ID: 2,
            Name: "Cheezy Snax",
            Category: "Snacks",
            Price: 1.99
        });

        // mock out enough metadata to do the attach
        upshot.metadata(type, { key: ["ID"] });

        dc.merge(products, type, null);

        return dc;
    }

    function createKoTestContext(submitChangesMock, bufferChanges) {
        var dataProvider = new upshot.DataProvider();
        var implicitCommitHandler;
        if (!bufferChanges) {
            implicitCommitHandler = function () {
                dc._commitChanges({ providerParameters: {} });
            }
        }
        var dc = new upshot.DataContext(dataProvider, implicitCommitHandler);
        dc._submitChanges = submitChangesMock;

        // add a single product to the context
        var manageAssociations = true;  // TODO: Lift this to a createKoTestContext parameter when unmanaged associations is supported.
        var OrderDetail = function (data) {
            this.Id = ko.observable(data.Id);
            this.Name = ko.observable(data.Name);
            this.Order = ko.observable();
            this.OrderId = manageAssociations ? ko.observable(data.OrderId) : ko.computed(function () {
                return this.Order() ? this.Order().Id() : data.OrderId;
            }, this);
            this.Extra = ko.observable("extra");
        };
        var Order = function (data) {
            this.Id = ko.observable(data.Id);
            this.Name = ko.observable(data.Name);
            this.OrderDetails = ko.observableArray(ko.utils.arrayMap(data.OrderDetails, function (od) { return new OrderDetail(od); }));
            this.Extra = ko.observable("extra");
        };

        var orders = [],
            order = {
                Id: 1,
                Name: "Order 1",
                OrderDetails: []
            };
        orders.push(new Order(order));
        orders.push(new Order({
            Id: 2,
            Name: "Order 2",
            OrderDetails: []
        }));
        var orderDetails = [];
        orderDetails.push(new OrderDetail({
            Id: 1,
            Name: "Order Detail 1",
            OrderId: order.Id
        }));
        orderDetails.push(new OrderDetail({
            Id: 2,
            Name: "Order Detail 2",
            OrderId: order.Id
        }));
        orders[0].OrderDetails(orderDetails);

        // mock out enough metadata to do the attach
        upshot.metadata("Order", { key: ["Id"], fields: { Id: { type: "Int32:#System" }, Name: { type: "String:#System" }, OrderDetails: { type: "OrderDetail", association: { Name: "O_OD", isForeignKey: false, thisKey: ["Id"], otherKey: ["OrderId"] }, array: true } } });
        upshot.metadata("OrderDetail", { key: ["Id"], fields: { Id: { type: "Int32:#System" }, Name: { type: "String:#System" }, Order: { type: "Order", association: { Name: "O_OD", isForeignKey: true, thisKey: ["OrderId"], otherKey: ["Id"]} }, OrderId: { type: "Int32:#System"}} });

        dc.merge(orders, "Order", null);

        return dc;
    }
})(upshot, ko);
