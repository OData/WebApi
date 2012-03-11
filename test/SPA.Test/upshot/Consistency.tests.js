/// <reference path="../Scripts/References.js" />
(function (global, upshot, undefined) {

    module("Consistency.tests.js");

    function getEntitySet() {
        upshot.metadata("Employee", {
            key: ["Id"],
            fields: {
                Name: {
                    type: "String:#System"
                },
                Manager: {
                    type: "Employee",
                    association: {
                        name: "Employee_Employee",
                        thisKey: ["ManagerId"],
                        otherKey: ["Id"],
                        isForeignKey: true
                    }
                },
                Reports: {
                    type: "Employee",
                    array: true,
                    association: {
                        name: "Employee_Employee2",
                        thisKey: ["Id"],
                        otherKey: ["ManagerId"],
                        isForeignKey: false
                    }
                },
                Id: {
                    type: "Int32:#System"
                },
                ManagerId: {
                    type: "Int32:#System"
                }
            }
        });
       
        var context = new upshot.DataContext();
        context.merge([
            { Id:1, Name: "Fred", ManagerId: 2 },
            { Id:2, Name: "Bob" },
            { Id:3, Name: "Jane" }
        ], "Employee");

        return context.getEntitySet("Employee");
    }

    test("AssociatedEntitiesView wrt FK update and revert", 6, function () {
        var entitySet = getEntitySet(),
            entities = entitySet.getEntities();

        ok(entities[1].Reports.length === 1, "Bob should have Fred as a report");
        ok(entities[2].Reports.length === 0, "Jane has no reports");

        $.observable(entities[0]).property("ManagerId", 3);

        ok(entities[1].Reports.length === 0, "Bob should have no reports");
        ok(entities[2].Reports.length === 1, "Jane should have Fred as a report");

        entitySet.revertChanges();
        
        ok(entities[1].Reports.length === 1, "Bob should have Fred as a report");
        ok(entities[2].Reports.length === 0, "Jane has no reports");
    });

    test("AssociatedEntitiesView wrt insert and revert", 3, function () {
        var entitySet = getEntitySet(),
            entities = entitySet.getEntities();

        ok(entities[1].Reports.length === 1, "Bob should have Fred as a report");

        $.observable(entities[1].Reports).insert(entities[2]);

        ok(entities[1].Reports.length === 2, "Bob should have Fred and Jane as reports");

        entitySet.revertChanges();
        
        ok(entities[1].Reports.length === 1, "Bob should have Fred as a report");
    });

    test("LocalDataSource auto-refresh over AssociatedEntitiesView", 9, function () {
        stop();
        
        var entitySet = getEntitySet(),
            entities = entitySet.getEntities();

        ok(entities[1].Reports.length === 1, "Bob should have Fred as a report");
        ok(entities[2].Reports.length === 0, "Jane has no reports");

        var localDataSource = new upshot.LocalDataSource({
            source: entities[1].Reports,
            autoRefresh: true,
            filter: { property: "Name", operator: "!=", value: "Joan" }
        });
        localDataSource.refresh(function () {
            ok(localDataSource.getEntities().length === 1, "Bob should have Fred as a report");

            $.observable(entities[0]).property("ManagerId", 3);

            ok(entities[1].Reports.length === 0, "Bob should have no reports");
            ok(entities[2].Reports.length === 1, "Jane should have Fred as a report");
            ok(localDataSource.getEntities().length === 0, "Bob should have no reports");

            entitySet.revertChanges();
        
            ok(entities[1].Reports.length === 1, "Bob should have Fred as a report");
            ok(entities[2].Reports.length === 0, "Jane has no reports");
            ok(localDataSource.getEntities().length === 1, "Bob should have Fred as a report");

            start();
        });
    });

    test("LocalDataSource auto-refresh over EntitySet wrt property updates and revert", 3, function () {
        stop();

        var entitySet = getEntitySet();

        var localDataSource = new upshot.LocalDataSource({
            source: entitySet,
            autoRefresh: true,
            filter: { property: "Name", value: "Fred" }
        });
        localDataSource.refresh(function () {
            ok(localDataSource.getEntities().length === 1, "We have an employee named Fred");

            $.observable(localDataSource.getEntities()[0]).property("Name", "Fredrick");
        
            ok(localDataSource.getEntities().length === 0, "We have no employees named Fred");

            entitySet.revertChanges();

            ok(localDataSource.getEntities().length === 1, "We have an employee named Fred");

            start();
        });
    });

    test("EntitySet wrt insert and revert", 4, function () {
        var entitySet = getEntitySet();

        ok(entitySet.getEntities().length === 3, "Have 3 entities");

        var newEmployee = { Name: "Barb" };
        $.observable(entitySet.getEntities()).insert(newEmployee);

        ok(entitySet.getEntities().length === 4, "Now 4 entities");

        entitySet.revertChanges();

        ok(entitySet.getEntities().length === 3, "Have 3 entities");
        ok((entitySet.getEntityState(newEmployee) || upshot.EntityState.Deleted) === upshot.EntityState.Deleted);
    });

})(this, upshot);
