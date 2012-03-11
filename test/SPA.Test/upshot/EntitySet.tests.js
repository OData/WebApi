/// <reference path="../Scripts/References.js" />

(function (global, $, upshot, undefined) {

    module("EntitySet.tests.js");

    var datasets = upshot.test.datasets,
        observability = upshot.observability;

    // direct tests against the default identity algorithm
    test("Compute identity tests", 14, function () {
        var metadata = {
            A: { key: ["k1"] },
            B: { key: ["k1", "k2", "k3"] },
            C: {}, // missing key metadata
            D: { key: ["a.b.k1", "a.b.k2"] }
        };
        var dc = new upshot.DataContext(new upshot.riaDataProvider());
        upshot.metadata(metadata);

        // valid single part key scenarios
        equal(upshot.EntitySet.__getIdentity({ k1: "1" }, "A"), "1");
        equal(upshot.EntitySet.__getIdentity({ k1: 1234 }, "A"), "1234");
        equal(upshot.EntitySet.__getIdentity({ k1: 1.234 }, "A"), "1.234");
        equal(upshot.EntitySet.__getIdentity({ k1: "E93CF47D-E7A6-44B2-8E30-0869B187BFB4" }, "A"), "E93CF47D-E7A6-44B2-8E30-0869B187BFB4");

        // valid multipart key scenarios
        equal(upshot.EntitySet.__getIdentity({ k1: "1", k2: "2", k3: "3" }, "B"), "1,2,3");
        equal(upshot.EntitySet.__getIdentity({ k1: 1, k2: 2, k3: 3 }, "B"), "1,2,3");
        equal(upshot.EntitySet.__getIdentity({ k1: true, k2: "E93CF47D-E7A6-44B2-8E30-0869B187BFB4", k3: 5 }, "B"), "true,E93CF47D-E7A6-44B2-8E30-0869B187BFB4,5");
        equal(upshot.EntitySet.__getIdentity({ k1: 1, k2: null, k3: undefined }, "B"), "1,null,undefined");

        // verify pathing scenarios (e.g. used by the OData provider)
        equal(upshot.EntitySet.__getIdentity({ a: { b: { k1: "1", k2: "2"}} }, "D"), "1,2");

        // no key metadata specified
        try {
            upshot.EntitySet.__getIdentity({ k1: "1", k2: "2", k3: "3" }, "C", "C");
        }
        catch (e) {
            equal(e, "No key metadata specified for entity type 'C'");
        }

        // invalid key member specification - missing member
        try {
            upshot.EntitySet.__getIdentity({ k1: "1", k2: "2" /* missing k3 member */ }, "B");
        }
        catch (e) {
            equal(e, "Key member 'k3' doesn't exist on entity type 'B'");
        }

        // invalid path specification - missing member
        try {
            upshot.EntitySet.__getIdentity({ a: { b: { k1: "1" /* k2 member missing */}} }, "D");
        }
        catch (e) {
            equal(e, "Key member 'a.b.k2' doesn't exist on entity type 'D'");
        }

        // invalid path specification - null path part
        try {
            upshot.EntitySet.__getIdentity({ a: { b: null} }, "D");
        }
        catch (e) {
            equal(e, "Key member 'a.b.k1' doesn't exist on entity type 'D'");
        }

        // no metadata registered for type
        try {
            upshot.EntitySet.__getIdentity({}, "E");
        }
        catch (e) {
            equal(e, "No metadata available for type 'E'.  Register metadata using 'upshot.metadata(...)'.");
        }
    });

    test("Load entities with multipart keys", 4, function () {
        var metadata = {
            A: { key: ["k1"] },
            B: { key: ["k1", "k2", "k3"] }
        };
        var dc = new upshot.DataContext(new upshot.riaDataProvider());
        upshot.metadata(metadata);

        var entities = [{ k1: "1" }, { k1: 1234 }, { k1: 1.234 }, { k1: "E93CF47D-E7A6-44B2-8E30-0869B187BFB4"}];
        dc.merge(entities, "A", null);

        entities = [
            { k1: "1", k2: "2", k3: "3" },
            { k1: 1, k2: 2, k3: 3 },
            { k1: true, k2: "E93CF47D-E7A6-44B2-8E30-0869B187BFB4", k3: 5 }
        ];
        dc.merge(entities, "B", null);

        // verify the entities are in the cache
        equal(dc.getEntitySet("A")._entityStates["1"], upshot.EntityState.Unmodified);
        equal(dc.getEntitySet("A")._entityStates["E93CF47D-E7A6-44B2-8E30-0869B187BFB4"], upshot.EntityState.Unmodified);
        equal(dc.getEntitySet("B")._entityStates["1,2,3"], upshot.EntityState.Unmodified);
        equal(dc.getEntitySet("B")._entityStates["true,E93CF47D-E7A6-44B2-8E30-0869B187BFB4,5"], upshot.EntityState.Unmodified);
    });

    test("Raises events when primitive values change", 10, function () {
        var es = createEntitySet(),
            entity = loadEntities(es, datasets.primitives.create(1));

        equal(es.getEntities().length, 1, "There should be a single entity loaded");

        var tracker = createEventTracker(es);

        $.observable(entity).property("B", false);

        equal(tracker.events.length, 2, "There should have been two events");
        equal(tracker.events[0].type, "propertyChanged", "The event should be 'propertyChanged'");
        equal(tracker.events[0].entity, entity, "The entity should have been modified");
        equal(tracker.events[0].path, "B", "Property 'B' should have been set");
        equal(tracker.events[0].value, entity.B, "The new value should be false");
        equal(tracker.events[1].type, "entityUpdated", "The event should be 'entityUpdated'");
        equal(tracker.events[1].entity, entity, "The entity should have been modified");
        equal(tracker.events[1].path, "", "The event should have occured on the entity");
        notEqual(tracker.events[1].eventArgs, null, "The event args should not be null");
    });

    test("Raises events when scalar values change", 10, function () {
        var es = createEntitySet(),
            entity = loadEntities(es, datasets.scalars.create(1));

        equal(es.getEntities().length, 1, "There should be a single entity loaded");

        var tracker = createEventTracker(es);

        $.observable(entity).property("D", new Date());

        equal(tracker.events.length, 2, "There should have been two events");
        equal(tracker.events[0].type, "propertyChanged", "The event should be 'propertyChanged'");
        equal(tracker.events[0].entity, entity, "The entity should have been modified");
        equal(tracker.events[0].path, "D", "Property 'D' should have been set");
        equal(tracker.events[0].value, entity.D, "The new value should be false");
        equal(tracker.events[1].type, "entityUpdated", "The event should be 'entityUpdated'");
        equal(tracker.events[1].entity, entity, "The entity should have been modified");
        equal(tracker.events[1].path, "", "The event should have occured on the entity");
        notEqual(tracker.events[1].eventArgs, null, "The event args should not be null");
    });

    test("Raises event when nested values change", 6, function () {
        var es = createEntitySet(),
            entity = loadEntities(es, datasets.nested.create(1));

        equal(es.getEntities().length, 1, "There should be a single entity loaded");

        var tracker = createEventTracker(es);

        $.observable(entity.O).property("B", false);

        equal(tracker.events.length, 1, "There should have been one event");
        equal(tracker.events[0].type, "entityUpdated", "The event should be 'entityUpdated'");
        equal(tracker.events[0].entity, entity, "The entity should have been modified");
        equal(tracker.events[0].path, "O", "The event should have occured on 'O'");
        notEqual(tracker.events[0].eventArgs, null, "The event args should not be null");
    });

    test("Raises event when tree values change", 6, function () {
        var es = createEntitySet(),
            entity = loadEntities(es, datasets.tree.create(1));

        equal(es.getEntities().length, 1, "There should be a single entity loaded");

        var tracker = createEventTracker(es);

        $.observable(entity.O.O1).property("B", false);

        equal(tracker.events.length, 1, "There should have been one event");
        equal(tracker.events[0].type, "entityUpdated", "The event should be 'entityUpdated'");
        equal(tracker.events[0].entity, entity, "The entity should have been modified");
        equal(tracker.events[0].path, "O.O1", "The event should have occured on 'O.O1'");
        notEqual(tracker.events[0].eventArgs, null, "The event args should not be null");
    });

    test("Raises event when array values change", 6, function () {
        var es = createEntitySet(),
            entity = loadEntities(es, datasets.array.create(1));

        equal(es.getEntities().length, 1, "There should be a single entity loaded");

        var tracker = createEventTracker(es);

        $.observable(entity.A[0]).property("B", false);

        equal(tracker.events.length, 1, "There should have been one event");
        equal(tracker.events[0].type, "entityUpdated", "The event should be 'entityUpdated'");
        equal(tracker.events[0].entity, entity, "The entity should have been modified");
        equal(tracker.events[0].path, "A[0]", "The event should have occured on 'A[0]'");
        notEqual(tracker.events[0].eventArgs, null, "The event args should not be null");
    });

    test("Raises event when nested array values change", 6, function () {
        var es = createEntitySet(),
            entity = loadEntities(es, datasets.nestedArrays.create(1));

        equal(es.getEntities().length, 1, "There should be a single entity loaded");

        var tracker = createEventTracker(es);

        $.observable(entity.A[0].A[0]).property("B", false);

        equal(tracker.events.length, 1, "There should have been one event");
        equal(tracker.events[0].type, "entityUpdated", "The event should be 'entityUpdated'");
        equal(tracker.events[0].entity, entity, "The entity should have been modified");
        equal(tracker.events[0].path, "A[0].A[0]", "The event should have occured on 'A[0].A[0]'");
        notEqual(tracker.events[0].eventArgs, null, "The event args should not be null");
    });

    test("Raises events on revertChanges", 11, function () {
        var es = createEntitySet(),
            entity = loadEntities(es, datasets.primitives.create(1));

        equal(es.getEntities().length, 1, "There should be a single entity loaded");

        var tracker = createEventTracker(es);

        $.observable(entity).property("B", false);

        equal(tracker.events.length, 2, "There should have been two events");

        tracker.events.length = 0;

        es.revertChanges();

        equal(tracker.events.length, 2, "There should have been two events");
        equal(tracker.events[0].type, "propertyChanged", "The event should be 'propertyChanged'");
        equal(tracker.events[0].entity, entity, "The entity should have been modified");
        equal(tracker.events[0].path, "B", "Property 'B' should have been set");
        equal(tracker.events[0].value, entity.B, "The reverted value should be true");
        equal(tracker.events[1].type, "entityUpdated", "The event should be 'entityUpdated'");
        equal(tracker.events[1].entity, entity, "The entity should have been modified");
        equal(tracker.events[1].path, undefined, "The event path should be undefined");
        equal(tracker.events[1].eventArgs, undefined, "The event args should be undefined");
    });

    test("Raises events on revertChanges with entity", 11, function () {
        var es = createEntitySet(),
            entity = loadEntities(es, datasets.primitives.create(1));

        equal(es.getEntities().length, 1, "There should be a single entity loaded");

        var tracker = createEventTracker(es);

        $.observable(entity).property("B", false);

        equal(tracker.events.length, 2, "There should have been two events");

        tracker.events.length = 0;

        es.revertChanges(entity);

        equal(tracker.events.length, 2, "There should have been two events");
        equal(tracker.events[0].type, "propertyChanged", "The event should be 'propertyChanged'");
        equal(tracker.events[0].entity, entity, "The entity should have been modified");
        equal(tracker.events[0].path, "B", "Property 'B' should have been set");
        equal(tracker.events[0].value, entity.B, "The reverted value should be true");
        equal(tracker.events[1].type, "entityUpdated", "The event should be 'entityUpdated'");
        equal(tracker.events[1].entity, entity, "The entity should have been modified");
        equal(tracker.events[1].path, undefined, "The event path should be undefined");
        equal(tracker.events[1].eventArgs, undefined, "The event args should be undefined");
    });

    test("Raises events on revertUpdates", 11, function () {
        var es = createEntitySet(),
            entity = loadEntities(es, datasets.primitives.create(1));

        equal(es.getEntities().length, 1, "There should be a single entity loaded");

        var tracker = createEventTracker(es);

        $.observable(entity).property("B", false);

        equal(tracker.events.length, 2, "There should have been two events");

        tracker.events.length = 0;

        es.revertUpdates(entity);

        equal(tracker.events.length, 2, "There should have been two events");
        equal(tracker.events[0].type, "propertyChanged", "The event should be 'propertyChanged'");
        equal(tracker.events[0].entity, entity, "The entity should have been modified");
        equal(tracker.events[0].path, "B", "Property 'B' should have been set");
        equal(tracker.events[0].value, entity.B, "The reverted value should be true");
        equal(tracker.events[1].type, "entityUpdated", "The event should be 'entityUpdated'");
        equal(tracker.events[1].entity, entity, "The entity should have been modified");
        equal(tracker.events[1].path, undefined, "The event path should be undefined");
        equal(tracker.events[1].eventArgs, undefined, "The event args should be undefined");
    });

    test("Raises events on revertUpdates for property", 11, function () {
        var es = createEntitySet(),
            entity = loadEntities(es, datasets.primitives.create(1));

        equal(es.getEntities().length, 1, "There should be a single entity loaded");

        var tracker = createEventTracker(es);

        $.observable(entity).property("B", false);

        equal(tracker.events.length, 2, "There should have been two events");

        tracker.events.length = 0;

        es.revertUpdates(entity, "B");

        equal(tracker.events.length, 2, "There should have been two events");
        equal(tracker.events[0].type, "propertyChanged", "The event should be 'propertyChanged'");
        equal(tracker.events[0].entity, entity, "The entity should have been modified");
        equal(tracker.events[0].path, "B", "Property 'B' should have been set");
        equal(tracker.events[0].value, entity.B, "The reverted value should be true");
        equal(tracker.events[1].type, "entityUpdated", "The event should be 'entityUpdated'");
        equal(tracker.events[1].entity, entity, "The entity should have been modified");
        equal(tracker.events[1].path, undefined, "The event path should be undefined");
        equal(tracker.events[1].eventArgs, undefined, "The event args should be undefined");
    });

    test("Raises event when knockout array values change", 6, function () {
        try {
            upshot.observability.configuration = observability.knockout;

            var es = createEntitySet(),
            entity = loadEntities(es, datasets.ko_array(1));

            equal(es.getEntities()().length, 1, "There should be a single entity loaded");

            var tracker = createEventTracker(es);

            entity.A()[0].B(false);

            equal(tracker.events.length, 1, "There should have been one event");
            equal(tracker.events[0].type, "entityUpdated", "The event should be 'entityUpdated'");
            equal(tracker.events[0].entity, entity, "The entity should have been modified");
            equal(tracker.events[0].path, "A[0]", "The event should have occured on 'A[0]'");
            notEqual(tracker.events[0].eventArgs, null, "The event args should not be null");
        } finally {
            upshot.observability.configuration = observability.jquery;
        }
    });

    test("Subscriptions are adjusted when nested values change", 5, function () {
        var es = createEntitySet(),
            entity = loadEntities(es, datasets.nested.create(1));

        equal(es.getEntities().length, 1, "There should be a single entity loaded");

        var original = entity.O,
            _new = { B: false };
        var tracker = createEventTracker(es);

        $.observable(original).property("B", false);

        equal(tracker.events.length, 1, "There should have been one event");

        tracker.events.length = 0;

        $.observable(entity).property("O", _new);

        equal(tracker.events.length, 2, "There should have been two events");

        tracker.events.length = 0;

        $.observable(original).property("B", true);

        equal(tracker.events.length, 0, "There should not have been any events");

        $.observable(_new).property("B", true);

        equal(tracker.events.length, 1, "There should have been one event");
    });

    test("Subscriptions are adjusted when tree values change", 5, function () {
        var es = createEntitySet(),
            entity = loadEntities(es, datasets.tree.create(1));

        equal(es.getEntities().length, 1, "There should be a single entity loaded");

        var original = entity.O,
            _new = { O1: { B: false} };
        var tracker = createEventTracker(es);

        $.observable(original.O1).property("B", false);

        equal(tracker.events.length, 1, "There should have been one event");

        tracker.events.length = 0;

        $.observable(entity).property("O", _new);

        equal(tracker.events.length, 2, "There should have been two events");

        tracker.events.length = 0;

        $.observable(original.O1).property("B", true);

        equal(tracker.events.length, 0, "There should not have been any events");

        tracker.events.length = 0;

        $.observable(_new.O1).property("B", true);

        equal(tracker.events.length, 1, "There should have been one event");
    });

    test("Subscriptions are added on array insert", 3, function () {
        var es = createEntitySet(),
            entity = loadEntities(es, datasets.array.create(1));

        equal(es.getEntities().length, 1, "There should be a single entity loaded");

        var _new = { B: false };
        var tracker = createEventTracker(es);

        $.observable(entity.A).insert(entity.A.length, _new);

        equal(tracker.events.length, 1, "There should have been one event");

        tracker.events.length = 0;

        $.observable(_new).property("B", true);

        equal(tracker.events.length, 1, "There should have been one event");
    });

    test("Subscriptions are removed on array remove", 4, function () {
        var es = createEntitySet(),
            entity = loadEntities(es, datasets.array.create(1));

        equal(es.getEntities().length, 1, "There should be a single entity loaded");

        var original = entity.A[0];
        var tracker = createEventTracker(es);

        $.observable(original).property("B", false);

        equal(tracker.events.length, 1, "There should have been one event");

        tracker.events.length = 0;

        $.observable(entity.A).remove(0, 1);

        equal(tracker.events.length, 1, "There should have been one event");

        tracker.events.length = 0;

        $.observable(original).property("B", true);

        equal(tracker.events.length, 0, "There should not have been any events");
    });

    test("Subscriptions are adjusted on array replaceAll", 5, function () {
        var es = createEntitySet(),
            entity = loadEntities(es, datasets.array.create(1));

        equal(es.getEntities().length, 1, "There should be a single entity loaded");

        var original = entity.A[0],
            _new = { B: false };
        var tracker = createEventTracker(es);

        $.observable(original).property("B", false);

        equal(tracker.events.length, 1, "There should have been one event");

        tracker.events.length = 0;

        $.observable(entity.A).replaceAll([_new]);

        equal(tracker.events.length, 1, "There should have been one event");

        tracker.events.length = 0;

        $.observable(original).property("B", true);

        equal(tracker.events.length, 0, "There should not have been any events");

        tracker.events.length = 0;

        $.observable(_new).property("B", true);

        equal(tracker.events.length, 1, "There should have been one event");
    });

    testWithRevertVariations(function (revertFn) {
        test("isUpdated returns true when primitive values change", 17, function () {
            var es = createEntitySet(),
                entity = loadEntities(es, datasets.primitives.create(1));

            equal(es.getEntities().length, 1, "There should be a single entity loaded");

            equal(es.getEntityState(entity), upshot.EntityState.Unmodified, "The entity state should be 'Unmodified'");
            equal(es.isUpdated(entity), false, "The entity should not have changes");
            equal(es.isUpdated(entity, "B"), false, "The entity should not have changes for 'B'");

            $.observable(entity).property("B", false);

            equal(es.getEntityState(entity), upshot.EntityState.ClientUpdated, "The entity state should be 'ClientUpdated'");
            equal(es.isUpdated(entity), true, "The entity should have changes");
            equal(es.isUpdated(entity, "B"), true, "The entity should have changes for 'B'");
            equal(es.isUpdated(entity, "N"), false, "The entity should not have changes for 'N'");

            $.observable(entity).property("B", true);

            equal(es.getEntityState(entity), upshot.EntityState.ClientUpdated, "The entity state should still be 'ClientUpdated'");
            equal(es.isUpdated(entity), true, "The entity should still have changes");
            equal(es.isUpdated(entity, "B"), true, "The entity should still have changes for 'B'");
            equal(es.isUpdated(entity, "N"), false, "The entity should still not have changes for 'N'");

            revertFn(es, entity, "B");

            equal(es.isUpdated(entity, "B"), false, "The entity should no longer have changes for 'B'");
            dataEqual(entity, datasets.primitives.create(1), "The entity should be reverted");
        });
    });

    testWithRevertVariations(function (revertFn) {
        test("isUpdated returns true when scalar values change", 12, function () {
            var es = createEntitySet(),
                entity = loadEntities(es, datasets.scalars.create(1));

            equal(es.getEntities().length, 1, "There should be a single entity loaded");

            equal(es.getEntityState(entity), upshot.EntityState.Unmodified, "The entity state should be 'Unmodified'");
            equal(es.isUpdated(entity), false, "The entity should not have changes");
            equal(es.isUpdated(entity, "D"), false, "The entity should not have changes for 'D'");

            $.observable(entity).property("D", new Date());

            equal(es.getEntityState(entity), upshot.EntityState.ClientUpdated, "The entity state should be 'ClientUpdated'");
            equal(es.isUpdated(entity), true, "The entity should have changes");
            equal(es.isUpdated(entity, "D"), true, "The entity should have changes for 'D'");

            revertFn(es, entity, "D");

            equal(es.isUpdated(entity, "D"), false, "The entity should no longer have changes for 'D'");
            dataEqual(entity, datasets.scalars.create(1), "The entity should be reverted");
        });
    });

    testWithRevertVariations(true, function (revertFn) {
        test("isUpdated returns true when nested values change", 15, function () {
            var es = createEntitySet(),
                entity = loadEntities(es, datasets.nested.create(1));

            equal(es.getEntities().length, 1, "There should be a single entity loaded");

            equal(es.getEntityState(entity), upshot.EntityState.Unmodified, "The entity state should be 'Unmodified'");
            equal(es.isUpdated(entity), false, "The entity should not have changes");
            equal(es.isUpdated(entity, "O"), false, "The nested object 'O' should not have changes");
            equal(es.isUpdated(entity, "O.B"), false, "The nested object 'O' should not have changes for 'B'");

            $.observable(entity.O).property("B", false);

            equal(es.getEntityState(entity), upshot.EntityState.ClientUpdated, "The entity state should be 'ClientUpdated'");
            equal(es.isUpdated(entity), true, "The entity should have changes");
            equal(es.isUpdated(entity, "O"), true, "The nested object 'O' should have changes");
            equal(es.isUpdated(entity, "O.B"), true, "The nested object 'O' should have changes for 'B'");
            equal(es.isUpdated(entity, "O", true), false, "The entity should not have changes for 'O'");

            revertFn(es, entity, "O");

            equal(es.isUpdated(entity, "O"), false, "The nested object 'O' should no longer have changes");
            equal(es.isUpdated(entity, "O.B"), false, "The nested object 'O' should no longer have changes for 'B'");
            dataEqual(entity, datasets.nested.create(1), "The entity should be reverted");
        });
    });

    testWithRevertVariations(true, function (revertFn) {
        test("isUpdated returns true when tree values change", 19, function () {
            var es = createEntitySet(),
                entity = loadEntities(es, datasets.tree.create(1));

            equal(es.getEntities().length, 1, "There should be a single entity loaded");

            equal(es.getEntityState(entity), upshot.EntityState.Unmodified, "The entity state should be 'Unmodified'");
            equal(es.isUpdated(entity), false, "The entity should not have changes");
            equal(es.isUpdated(entity, "O"), false, "The nested object 'O' should not have changes");
            equal(es.isUpdated(entity, "O.O1"), false, "The nested object 'O.O1' should not have changes");
            equal(es.isUpdated(entity, "O.O1.B"), false, "The nested object 'O.O1' should not have changes for 'B'");

            $.observable(entity.O.O1).property("B", false);

            equal(es.getEntityState(entity), upshot.EntityState.ClientUpdated, "The entity state should be 'ClientUpdated'");
            equal(es.isUpdated(entity), true, "The entity should have changes");
            equal(es.isUpdated(entity, "O"), true, "The nested object 'O' should have changes");
            equal(es.isUpdated(entity, "O.O1"), true, "The nested object 'O.O1' should have changes");
            equal(es.isUpdated(entity, "O.O1.B"), true, "The nested object 'O.O1' should have changes for 'B'");
            equal(es.isUpdated(entity, "O", true), false, "The entity should not have changes for 'O'");
            equal(es.isUpdated(entity, "O.O1", true), false, "The nested object 'O' should not have changes for 'O1'");

            revertFn(es, entity, "O");

            equal(es.isUpdated(entity, "O"), false, "The nested object 'O' should no longer have changes");
            equal(es.isUpdated(entity, "O.O1"), false, "The nested object 'O.O1' should no longer have changes");
            equal(es.isUpdated(entity, "O.O1.B"), false, "The nested object 'O.O1' should no longer have changes for 'B'");
            dataEqual(entity, datasets.tree.create(1), "The entity should be reverted");
        });
    });

    testWithRevertVariations(true, function (revertFn) {
        test("isUpdated returns true when array values change", 18, function () {
            var es = createEntitySet(),
                entity = loadEntities(es, datasets.array.create(1));

            equal(es.getEntities().length, 1, "There should be a single entity loaded");

            equal(es.getEntityState(entity), upshot.EntityState.Unmodified, "The entity state should be 'Unmodified'");
            equal(es.isUpdated(entity), false, "The entity should not have changes");
            equal(es.isUpdated(entity, "A"), false, "The array should not have changes");
            equal(es.isUpdated(entity, "A[0]"), false, "The object at index 0 should not have changes");
            equal(es.isUpdated(entity, "A[0].B"), false, "The object at index 0 should not have changes for 'B'");

            $.observable(entity.A[0]).property("B", false);

            equal(es.getEntityState(entity), upshot.EntityState.ClientUpdated, "The entity state should be 'ClientUpdated'");
            equal(es.isUpdated(entity), true, "The entity should have changes");
            equal(es.isUpdated(entity, "A"), true, "The array should have changes");
            equal(es.isUpdated(entity, "A[0]"), true, "The object at index 0 should have changes");
            equal(es.isUpdated(entity, "A[0].B"), true, "The object at index 0 should have changes for 'B'");
            equal(es.isUpdated(entity, "A", true), false, "The entity should not have changes for 'A'");

            revertFn(es, entity, "A");

            equal(es.isUpdated(entity, "A"), false, "The array should no longer have changes");
            equal(es.isUpdated(entity, "A[0]"), false, "The object at index 0 should no longer have changes");
            equal(es.isUpdated(entity, "A[0].B"), false, "The object at index 0 should no longer have changes for 'B'");
            dataEqual(entity, datasets.array.create(1), "The entity should be reverted");
        });
    });

    testWithRevertVariations(true, function (revertFn) {
        test("isUpdated returns true when nested array values change", 25, function () {
            var es = createEntitySet(),
                entity = loadEntities(es, datasets.nestedArrays.create(1));

            equal(es.getEntities().length, 1, "There should be a single entity loaded");

            equal(es.getEntityState(entity), upshot.EntityState.Unmodified, "The entity state should be 'Unmodified'");
            equal(es.isUpdated(entity), false, "The entity should not have changes");
            equal(es.isUpdated(entity, "A"), false, "The array should not have changes");
            equal(es.isUpdated(entity, "A[0]"), false, "The object at index 0 should not have changes");
            equal(es.isUpdated(entity, "A[0].A"), false, "The nested array should not have changes");
            equal(es.isUpdated(entity, "A[0].A[0]"), false, "The nested object at index 0 should not have changes");
            equal(es.isUpdated(entity, "A[0].A[0].B"), false, "The nested object at index 0 should not have changes for 'B'");

            $.observable(entity.A[0].A[0]).property("B", false);

            equal(es.getEntityState(entity), upshot.EntityState.ClientUpdated, "The entity state should be 'ClientUpdated'");
            equal(es.isUpdated(entity), true, "The entity should have changes");
            equal(es.isUpdated(entity, "A"), true, "The array should have changes");
            equal(es.isUpdated(entity, "A[0]"), true, "The object at index 0 should have changes");
            equal(es.isUpdated(entity, "A[0].A"), true, "The nested array should have changes");
            equal(es.isUpdated(entity, "A[0].A[0]"), true, "The nested object at index 0 should have changes");
            equal(es.isUpdated(entity, "A[0].A[0].B"), true, "The nested object at index 0 should have changes for 'B'");
            equal(es.isUpdated(entity, "A", true), false, "The entity should not have changes for 'A'");
            equal(es.isUpdated(entity, "A[0].A", true), false, "The object at index 0 not have changes for 'A'");

            revertFn(es, entity, "A");

            equal(es.isUpdated(entity, "A"), false, "The array should no longer have changes");
            equal(es.isUpdated(entity, "A[0]"), false, "The object at index 0 should no longer have changes");
            equal(es.isUpdated(entity, "A[0].A"), false, "The nested array should no longer have changes");
            equal(es.isUpdated(entity, "A[0].A[0]"), false, "The nested object at index 0 should no longer have changes");
            equal(es.isUpdated(entity, "A[0].A[0].B"), false, "The nested object at index 0 should no longer have changes for 'B'");
            dataEqual(entity, datasets.nestedArrays.create(1), "The entity should be reverted");
        });
    });

    testWithRevertVariations(function (revertFn) {
        test("isUpdated returns true when knockout primitive values change", 27, function () {
            try {
                upshot.observability.configuration = observability.knockout;

                var es = createEntitySet(),
                    entity = loadEntities(es, datasets.ko_primitives(1, true));

                equal(es.getEntities()().length, 1, "There should be a single entity loaded");

                equal(es.getEntityState(entity), upshot.EntityState.Unmodified, "The entity state should be 'Unmodified'");
                equal(es.isUpdated(entity), false, "The entity should not have changes");
                equal(es.isUpdated(entity, "B"), false, "The entity should not have changes for 'B'");
                equal(entity.IsUpdated(), false, "The entity should not be changed");
                equal(entity.B.IsUpdated(), false, "The entity should not be changed for 'B'");

                entity.B(false);

                equal(es.getEntityState(entity), upshot.EntityState.ClientUpdated, "The entity state should be 'ClientUpdated'");
                equal(es.isUpdated(entity), true, "The entity should have changes");
                equal(es.isUpdated(entity, "B"), true, "The entity should have changes for 'B'");
                equal(es.isUpdated(entity, "N"), false, "The entity should not have changes for 'N'");
                equal(entity.IsUpdated(), true, "The entity should be changed");
                equal(entity.B.IsUpdated(), true, "The entity should be changed for 'B'");
                equal(entity.N.IsUpdated(), false, "The entity should not be changed for 'N'");

                entity.B(true);

                equal(es.getEntityState(entity), upshot.EntityState.ClientUpdated, "The entity state should still be 'ClientUpdated'");
                equal(es.isUpdated(entity), true, "The entity should still have changes");
                equal(es.isUpdated(entity, "B"), true, "The entity should still have changes for 'B'");
                equal(es.isUpdated(entity, "N"), false, "The entity should still not have changes for 'N'");
                equal(entity.IsUpdated(), true, "The entity should still be changed");
                equal(entity.B.IsUpdated(), true, "The entity should still be changed for 'B'");
                equal(entity.N.IsUpdated(), false, "The entity should still not be changed for 'N'");

                revertFn(es, entity, "B");

                equal(es.isUpdated(entity, "B"), false, "The entity should no longer have changes for 'B'");
                equal(entity.IsUpdated(), false, "The entity should no longer be changed");
                equal(entity.B.IsUpdated(), false, "The entity should no longer be changed for 'B'");
                dataEqual(entity, datasets.ko_primitives(1), "The entity should be reverted");
            } finally {
                upshot.observability.configuration = observability.jquery;
            }
        });
    });

    testWithRevertVariations(true, function (revertFn) {
        test("isUpdated returns true when knockout tree values change", 29, function () {
            try {
                upshot.observability.configuration = observability.knockout;

                var es = createEntitySet(),
                    entity = loadEntities(es, datasets.ko_tree(1, true));

                equal(es.getEntities()().length, 1, "There should be a single entity loaded");

                equal(es.getEntityState(entity), upshot.EntityState.Unmodified, "The entity state should be 'Unmodified'");
                equal(es.isUpdated(entity), false, "The entity should not have changes");
                equal(es.isUpdated(entity, "O"), false, "The nested object 'O' should not have changes");
                equal(es.isUpdated(entity, "O.O1"), false, "The nested object 'O.O1' should not have changes");
                equal(es.isUpdated(entity, "O.O1.B"), false, "The nested object 'O.O1' should not have changes for 'B'");
                equal(entity.IsUpdated(), false, "The entity should not be changed");
                equal(entity.O.IsUpdated(), false, "The nested object 'O' should not be changed");
                equal(entity.O().O1.IsUpdated(), false, "The nested object 'O.O1' should not be changed");
                equal(entity.O().O1().B.IsUpdated(), false, "The nested object 'O.O1' should not be changed for 'B'");

                entity.O().O1().B(false);

                equal(es.getEntityState(entity), upshot.EntityState.ClientUpdated, "The entity state should be 'ClientUpdated'");
                equal(es.isUpdated(entity), true, "The entity should have changes");
                equal(es.isUpdated(entity, "O"), true, "The nested object 'O' should have changes");
                equal(es.isUpdated(entity, "O.O1"), true, "The nested object 'O.O1' should have changes");
                equal(es.isUpdated(entity, "O.O1.B"), true, "The nested object 'O.O1' should have changes for 'B'");
                equal(es.isUpdated(entity, "O", true), false, "The entity should not have changes for 'O'");
                equal(es.isUpdated(entity, "O.O1", true), false, "The nested object 'O' should not have changes for 'O1'");
                equal(entity.IsUpdated(), true, "The entity should be changed");
                equal(entity.O.IsUpdated(), false, "The nested object 'O' should still not be changed");
                equal(entity.O().O1.IsUpdated(), false, "The nested object 'O.O1' should still not be changed");
                equal(entity.O().O1().B.IsUpdated(), true, "The nested object 'O.O1' should be changed for 'B'");

                revertFn(es, entity, "O");

                equal(es.isUpdated(entity, "O"), false, "The nested object 'O' should no longer have changes");
                equal(es.isUpdated(entity, "O.O1"), false, "The nested object 'O.O1' should no longer have changes");
                equal(es.isUpdated(entity, "O.O1.B"), false, "The nested object 'O.O1' should no longer have changes for 'B'");
                equal(entity.IsUpdated(), false, "The entity should no longer be changed");
                equal(entity.O().O1().B.IsUpdated(), false, "The nested object 'O.O1' should no longer be changed for 'B'");
                dataEqual(entity, datasets.ko_tree(1), "The entity should be reverted");
            } finally {
                upshot.observability.configuration = observability.jquery;
            }
        });
    });

    testWithRevertVariations(true, function (revertFn) {
        test("isUpdated returns true when knockout array values change", 23, function () {
            try {
                upshot.observability.configuration = observability.knockout;

                var es = createEntitySet(),
                    entity = loadEntities(es, datasets.ko_array(1, true));

                equal(es.getEntities()().length, 1, "There should be a single entity loaded");

                equal(es.getEntityState(entity), "Unmodified", "The entity state should be 'Unmodified'");
                equal(es.isUpdated(entity), false, "The entity should not have changes");
                equal(es.isUpdated(entity, "A[0]"), false, "The object at index 0 should not have changes");
                equal(es.isUpdated(entity, "A[0].B"), false, "The object at index 0 should not have changes for 'B'");
                equal(entity.IsUpdated(), false, "The entity should not be changed");
                equal(entity.A.IsUpdated(), false, "The entity should not be changed for 'A'");
                equal(entity.A()[0].B.IsUpdated(), false, "The object at index 0 should not be changed for 'B'");

                entity.A()[0].B(false);

                equal(es.getEntityState(entity), "ClientUpdated", "The entity state should be 'ClientUpdated'");
                equal(es.isUpdated(entity), true, "The entity should have changes");
                equal(es.isUpdated(entity, "A[0]"), true, "The object at index 0 should have changes");
                equal(es.isUpdated(entity, "A[0].B"), true, "The object at index 0 should have changes for 'B'");
                equal(es.isUpdated(entity, "A", true), false, "The entity should not have changes for 'A'");
                equal(entity.IsUpdated(), true, "The entity should be changed");
                equal(entity.A.IsUpdated(), false, "The entity should not be changed for 'A'");
                equal(entity.A()[0].B.IsUpdated(), true, "The object at index 0 should be changed for 'B'");

                revertFn(es, entity, "A");

                equal(es.isUpdated(entity, "A[0]"), false, "The object at index 0 should no longer have changes");
                equal(es.isUpdated(entity, "A[0].B"), false, "The object at index 0 should no longer have changes for 'B'");
                equal(entity.IsUpdated(), false, "The entity should no longer be changed");
                equal(entity.A()[0].B.IsUpdated(), false, "The object at index 0 should no longer be changed for 'B'");
                dataEqual(entity, datasets.ko_array(1), "The entity should be reverted");
            } finally {
                upshot.observability.configuration = observability.jquery;
            }
        });
    });

    testWithRevertVariations(true, function (revertFn) {
        test("isUpdated returns true when knockout nested array values change", 35, function () {
            try {
                upshot.observability.configuration = observability.knockout;

                var es = createEntitySet(),
                    entity = loadEntities(es, datasets.ko_nestedArrays(1, true));

                equal(es.getEntities()().length, 1, "There should be a single entity loaded");

                equal(es.getEntityState(entity), upshot.EntityState.Unmodified, "The entity state should be 'Unmodified'");
                equal(es.isUpdated(entity), false, "The entity should not have changes");
                equal(es.isUpdated(entity, "A"), false, "The array should not have changes");
                equal(es.isUpdated(entity, "A[0]"), false, "The object at index 0 should not have changes");
                equal(es.isUpdated(entity, "A[0].A"), false, "The nested array should not have changes");
                equal(es.isUpdated(entity, "A[0].A[0]"), false, "The nested object at index 0 should not have changes");
                equal(es.isUpdated(entity, "A[0].A[0].B"), false, "The nested object at index 0 should not have changes for 'B'");
                equal(entity.IsUpdated(), false, "The entity should not be changed");
                equal(entity.A.IsUpdated(), false, "The entity should not be changed for 'A'");
                equal(entity.A()[0].A.IsUpdated(), false, "The object at index 0 should not be changed for 'A'");
                equal(entity.A()[0].A()[0].B.IsUpdated(), false, "The nested object at index 0 should not be changed for 'B'");

                entity.A()[0].A()[0].B(false);

                equal(es.getEntityState(entity), upshot.EntityState.ClientUpdated, "The entity state should be 'ClientUpdated'");
                equal(es.isUpdated(entity), true, "The entity should have changes");
                equal(es.isUpdated(entity, "A"), true, "The array should have changes");
                equal(es.isUpdated(entity, "A[0]"), true, "The object at index 0 should have changes");
                equal(es.isUpdated(entity, "A[0].A"), true, "The nested array should have changes");
                equal(es.isUpdated(entity, "A[0].A[0]"), true, "The nested object at index 0 should have changes");
                equal(es.isUpdated(entity, "A[0].A[0].B"), true, "The nested object at index 0 should have changes for 'B'");
                equal(es.isUpdated(entity, "A", true), false, "The entity should not have changes for 'A'");
                equal(es.isUpdated(entity, "A[0].A", true), false, "The object at index 0 not have changes for 'A'");
                equal(entity.IsUpdated(), true, "The entity should not be changed");
                equal(entity.A.IsUpdated(), false, "The entity should still not be changed for 'A'");
                equal(entity.A()[0].A.IsUpdated(), false, "The object at index 0 should still not be changed for 'A'");
                equal(entity.A()[0].A()[0].B.IsUpdated(), true, "The nested object at index 0 should be changed for 'B'");

                revertFn(es, entity, "A");

                equal(es.isUpdated(entity, "A"), false, "The array should no longer have changes");
                equal(es.isUpdated(entity, "A[0]"), false, "The object at index 0 should no longer have changes");
                equal(es.isUpdated(entity, "A[0].A"), false, "The nested array should no longer have changes");
                equal(es.isUpdated(entity, "A[0].A[0]"), false, "The nested object at index 0 should no longer have changes");
                equal(es.isUpdated(entity, "A[0].A[0].B"), false, "The nested object at index 0 should no longer have changes for 'B'");
                equal(entity.IsUpdated(), false, "The entity should no longer be changed");
                equal(entity.A()[0].A()[0].B.IsUpdated(), false, "The nested object at index 0 should no longer be changed for 'B'");
                dataEqual(entity, datasets.ko_nestedArrays(1), "The entity should be reverted");
            } finally {
                upshot.observability.configuration = observability.jquery;
            }
        });
    });

    testWithRevertVariations(function (revertFn) {
        test("isUpdated returns true when multiple properties are updated", 16, function () {
            var es = createEntitySet(),
                entity = loadEntities(es, datasets.primitives.create(1));

            equal(es.getEntities().length, 1, "There should be a single entity loaded");

            equal(es.isUpdated(entity, "B"), false, "The entity should not have changes for 'B'");
            equal(es.isUpdated(entity, "N"), false, "The entity should not have changes for 'N'");

            $.observable(entity).property("B", false);

            equal(es.isUpdated(entity, "B"), true, "The entity should have changes for 'B'");
            equal(es.isUpdated(entity, "N"), false, "The entity should still not have changes for 'N'");

            $.observable(entity).property("N", -1);

            equal(es.isUpdated(entity, "B"), true, "The entity should still have changes for 'B'");
            equal(es.isUpdated(entity, "N"), true, "The entity should have changes for 'N'");

            revertFn(es, entity, "B", true);
            revertFn(es, entity, "N");

            equal(es.isUpdated(entity, "B"), false, "The entity should no longer have changes for 'B'");
            equal(es.isUpdated(entity, "N"), false, "The entity should no longer have changes for 'N'");
            dataEqual(entity, datasets.primitives.create(1), "The entity should be reverted");
        });
    });

    testWithRevertVariations(true, function (revertFn) {
        test("isUpdated returns true when multiple nested properties are updated", 14, function () {
            var es = createEntitySet(),
                entity = loadEntities(es, datasets.nested.create(1));

            equal(es.getEntities().length, 1, "There should be a single entity loaded");

            equal(es.isUpdated(entity, "B"), false, "The entity should not have changes for 'B'");
            equal(es.isUpdated(entity, "O.B"), false, "The nested object 'O' should not have changes for 'B'");

            $.observable(entity).property("B", false);

            equal(es.isUpdated(entity, "B"), true, "The entity should have changes for 'B'");
            equal(es.isUpdated(entity, "O.B"), false, "The nested object 'O' should still not have changes for 'B'");

            $.observable(entity.O).property("B", false);

            equal(es.isUpdated(entity, "B"), true, "The entity should still have changes for 'B'");
            equal(es.isUpdated(entity, "O.B"), true, "The nested object 'O' should have changes for 'B'");

            revertFn(es, entity, "B", true);
            revertFn(es, entity, "O");

            equal(es.isUpdated(entity, "B"), false, "The entity should no longer have changes for 'B'");
            equal(es.isUpdated(entity, "O.B"), false, "The nested object 'O' should no longer have changes for 'B'");
            dataEqual(entity, datasets.nested.create(1), "The entity should be reverted");
        });
    });

    test("_clearChanges resets tracking on multiple nested properties updates", 11, function () {
        var es = createEntitySet(),
            entity = loadEntities(es, datasets.nested.create(1));

        equal(es.getEntities().length, 1, "There should be a single entity loaded");

        equal(es.isUpdated(entity, "B"), false, "The entity should not have changes for 'B'");
        equal(es.isUpdated(entity, "O.B"), false, "The nested object 'O' should not have changes for 'B'");

        $.observable(entity).property("B", false);

        equal(es.isUpdated(entity, "B"), true, "The entity should have changes for 'B'");
        equal(es.isUpdated(entity, "O.B"), false, "The nested object 'O' should still not have changes for 'B'");

        $.observable(entity.O).property("B", false);

        equal(es.isUpdated(entity, "B"), true, "The entity should still have changes for 'B'");
        equal(es.isUpdated(entity, "O.B"), true, "The nested object 'O' should have changes for 'B'");

        es._clearChanges(entity, false);

        equal(es.isUpdated(entity), false, "The entity should no longer have changes");
        equal(es.isUpdated(entity, "O"), false, "The nested object should no longer have changes");
        equal(es.isUpdated(entity, "B"), false, "The entity should no longer have changes for 'B'");
        equal(es.isUpdated(entity, "O.B"), false, "The nested object 'O' should no longer have changes for 'B'");
    });

    testWithRevertVariations(true, function (revertFn) {
        test("isUpdated returns true when multiple knockout nested properties are updated", 23, function () {
            try {
                upshot.observability.configuration = observability.knockout;

                var es = createEntitySet(),
                    entity = loadEntities(es, datasets.ko_tree(1, true));

                equal(es.getEntities()().length, 1, "There should be a single entity loaded");

                equal(es.isUpdated(entity, "B"), false, "The entity should not have changes for 'B'");
                equal(es.isUpdated(entity, "O.B"), false, "The nested object 'O' should not have changes for 'B'");
                equal(entity.B.IsUpdated(), false, "The entity should not be changed for 'B'");
                equal(entity.O().B.IsUpdated(), false, "The nested object 'O' should not be changed for 'B'");

                entity.B(false);

                equal(es.isUpdated(entity, "B"), true, "The entity should have changes for 'B'");
                equal(es.isUpdated(entity, "O.B"), false, "The nested object 'O' should still not have changes for 'B'");
                equal(entity.B.IsUpdated(), true, "The entity should be changed for 'B'");
                equal(entity.O().B.IsUpdated(), false, "The nested object 'O' should still not be changed for 'B'");

                entity.O().B(false);

                equal(es.isUpdated(entity, "B"), true, "The entity should still have changes for 'B'");
                equal(es.isUpdated(entity, "O.B"), true, "The nested object 'O' should have changes for 'B'");
                equal(entity.B.IsUpdated(), true, "The entity should still be changed for 'B'");
                equal(entity.O().B.IsUpdated(), true, "The nested object 'O' should be changed for 'B'");

                revertFn(es, entity, "B", true);
                revertFn(es, entity, "O");

                equal(es.isUpdated(entity, "B"), false, "The entity should no longer have changes for 'B'");
                equal(es.isUpdated(entity, "O.B"), false, "The nested object 'O' should no longer have changes for 'B'");
                equal(entity.IsUpdated(), false, "The entity should no longer be changed");
                equal(entity.B.IsUpdated(), false, "The entity should no longer be changed for 'B'");
                equal(entity.O().B.IsUpdated(), false, "The nested object 'O' should no longer be changed for 'B'");
                dataEqual(entity, datasets.ko_tree(1), "The entity should be reverted");
            } finally {
                upshot.observability.configuration = observability.jquery;
            }
        });
    });

    test("_clearChanges resets tracking on multiple knockout nested properties updates", 19, function () {
        try {
            upshot.observability.configuration = observability.knockout;

            var es = createEntitySet(),
                entity = loadEntities(es, datasets.ko_tree(1, true));

            equal(es.getEntities()().length, 1, "There should be a single entity loaded");

            equal(es.isUpdated(entity, "B"), false, "The entity should not have changes for 'B'");
            equal(es.isUpdated(entity, "O.B"), false, "The nested object 'O' should not have changes for 'B'");
            equal(entity.B.IsUpdated(), false, "The entity should not be changed for 'B'");
            equal(entity.O().B.IsUpdated(), false, "The nested object 'O' should not be changed for 'B'");

            entity.B(false);

            equal(es.isUpdated(entity, "B"), true, "The entity should have changes for 'B'");
            equal(es.isUpdated(entity, "O.B"), false, "The nested object 'O' should still not have changes for 'B'");
            equal(entity.B.IsUpdated(), true, "The entity should be changed for 'B'");
            equal(entity.O().B.IsUpdated(), false, "The nested object 'O' should still not be changed for 'B'");

            entity.O().B(false);

            equal(es.isUpdated(entity, "B"), true, "The entity should still have changes for 'B'");
            equal(es.isUpdated(entity, "O.B"), true, "The nested object 'O' should have changes for 'B'");
            equal(entity.B.IsUpdated(), true, "The entity should still be changed for 'B'");
            equal(entity.O().B.IsUpdated(), true, "The nested object 'O' should be changed for 'B'");

            es._clearChanges(entity, false);

            equal(es.isUpdated(entity), false, "The entity should no longer have changes");
            equal(es.isUpdated(entity, "O"), false, "The nested object should no longer have changes");
            equal(es.isUpdated(entity, "B"), false, "The entity should no longer have changes for 'B'");
            equal(es.isUpdated(entity, "O.B"), false, "The nested object 'O' should no longer have changes for 'B'");
            equal(entity.B.IsUpdated(), false, "The entity should no longer be changed for 'B'");
            equal(entity.O().B.IsUpdated(), false, "The nested object 'O' should no longer be changed for 'B'");
        } finally {
            upshot.observability.configuration = observability.jquery;
        }
    });

    testWithRevertVariations(true, function (revertFn) {
        test("isUpdated returns true on array insert", 13, function () {
            var es = createEntitySet(),
                entity = loadEntities(es, datasets.array.create(1));

            equal(es.getEntities().length, 1, "There should be a single entity loaded");

            equal(es.getEntityState(entity), upshot.EntityState.Unmodified, "The entity state should be 'Unmodified'");
            equal(es.isUpdated(entity), false, "The entity should not have changes");
            equal(es.isUpdated(entity, "A"), false, "The array should not have changes");

            var length = entity.A.length,
                _new = { B: false };

            $.observable(entity.A).insert(length, _new);

            equal(es.getEntityState(entity), upshot.EntityState.ClientUpdated, "The entity state should be 'ClientUpdated'");
            equal(es.isUpdated(entity), true, "The entity should have changes");
            equal(es.isUpdated(entity, "A"), true, "The array should have changes");
            equal(es.isUpdated(entity, "A[1]"), false, "The last object should not have changes");
            equal(entity.A.length, length + 1, "The updated array lengths should be equal");

            revertFn(es, entity, "A");

            equal(es.isUpdated(entity, "A"), false, "The array should no longer have changes");
            dataEqual(entity, datasets.array.create(1), "The entity should be reverted");
        });
    });

    testWithRevertVariations(true, function (revertFn) {
        test("isUpdated returns true on array remove", 12, function () {
            var es = createEntitySet(),
                entity = loadEntities(es, datasets.array.create(1));

            equal(es.getEntities().length, 1, "There should be a single entity loaded");

            equal(es.getEntityState(entity), upshot.EntityState.Unmodified, "The entity state should be 'Unmodified'");
            equal(es.isUpdated(entity), false, "The entity should not have changes");
            equal(es.isUpdated(entity, "A"), false, "The array should not have changes");

            var length = entity.A.length;

            $.observable(entity.A).remove(0, 1);

            equal(es.getEntityState(entity), upshot.EntityState.ClientUpdated, "The entity state should be 'ClientUpdated'");
            equal(es.isUpdated(entity), true, "The entity should have changes");
            equal(es.isUpdated(entity, "A"), true, "The array should have changes");
            equal(entity.A.length, length - 1, "The updated array lengths should be equal");

            revertFn(es, entity, "A");

            equal(es.isUpdated(entity, "A"), false, "The array should no longer have changes");
            dataEqual(entity, datasets.array.create(1), "The entity should be reverted");
        });
    });

    testWithRevertVariations(true, function (revertFn) {
        test("isUpdated returns true on array replaceAll", 13, function () {
            var es = createEntitySet(),
                entity = loadEntities(es, datasets.array.create(1));

            equal(es.getEntities().length, 1, "There should be a single entity loaded");

            equal(es.getEntityState(entity), upshot.EntityState.Unmodified, "The entity state should be 'Unmodified'");
            equal(es.isUpdated(entity), false, "The entity should not have changes");
            equal(es.isUpdated(entity, "A"), false, "The array should not have changes");

            var length = entity.A.length,
                _new = { B: false };

            $.observable(entity.A).replaceAll([_new]);

            equal(es.getEntityState(entity), upshot.EntityState.ClientUpdated, "The entity state should be 'ClientUpdated'");
            equal(es.isUpdated(entity), true, "The entity should have changes");
            equal(es.isUpdated(entity, "A"), true, "The array should have changes");
            equal(es.isUpdated(entity, "A[0]"), false, "The object at index '0' should not have changes");
            equal(entity.A.length, 1, "The updated array lengths should be equal");

            revertFn(es, entity, "A");

            equal(es.isUpdated(entity, "A"), false, "The array should no longer have changes");
            dataEqual(entity, datasets.array.create(1), "The entity should be reverted");
        });
    });

    testWithRevertVariations(true, function (revertFn) {
        test("isUpdated returns true on multiple array updates", 16, function () {
            var es = createEntitySet(),
                entity = loadEntities(es, datasets.array.create(1));

            equal(es.getEntities().length, 1, "There should be a single entity loaded");

            equal(es.getEntityState(entity), upshot.EntityState.Unmodified, "The entity state should be 'Unmodified'");
            equal(es.isUpdated(entity), false, "The entity should not have changes");
            equal(es.isUpdated(entity, "A"), false, "The array should not have changes");

            var length = entity.A.length,
                _new = { B: false };

            $.observable(entity.A).insert(length, _new);

            equal(es.getEntityState(entity), upshot.EntityState.ClientUpdated, "The entity state should be 'ClientUpdated'");
            equal(es.isUpdated(entity), true, "The entity should have changes");
            equal(es.isUpdated(entity, "A"), true, "The array should have changes");
            equal(es.isUpdated(entity, "A[2]"), false, "The last object should not have changes");
            equal(entity.A.length, length + 1, "The updated array lengths should be equal");

            $.observable(entity.A).remove(1, 2);

            equal(es.isUpdated(entity), true, "The entity should still have changes");
            equal(es.isUpdated(entity, "A"), true, "The array should still have changes");
            equal(entity.A.length, length - 1, "The updated array lengths should be equal");

            revertFn(es, entity, "A");

            equal(es.isUpdated(entity, "A"), false, "The array should no longer have changes");
            dataEqual(entity, datasets.array.create(1), "The entity should be reverted");
        });
    });

    testWithRevertVariations(true, function (revertFn) {
        test("isUpdated returns true on knockout array insert", 17, function () {
            try {
                upshot.observability.configuration = observability.knockout;

                var es = createEntitySet(),
                    entity = loadEntities(es, datasets.ko_array(1, true));

                equal(es.getEntities()().length, 1, "There should be a single entity loaded");

                equal(es.getEntityState(entity), "Unmodified", "The entity state should be 'Unmodified'");
                equal(es.isUpdated(entity), false, "The entity should not have changes");
                equal(es.isUpdated(entity, "A"), false, "The array should not have changes");
                equal(entity.A.IsUpdated(), false, "The entity should not be changed for 'A'");

                var length = entity.A().length,
                    _new = { B: ko.observable(false) };

                entity.A.push(_new);

                equal(es.getEntityState(entity), "ClientUpdated", "The entity state should be 'ClientUpdated'");
                equal(es.isUpdated(entity), true, "The entity should have changes");
                equal(es.isUpdated(entity, "A"), true, "The array should have changes");
                equal(es.isUpdated(entity, "A[1]"), false, "The last object should not have changes");
                equal(entity.A().length, length + 1, "The updated array lengths should be equal");
                equal(entity.A.IsUpdated(), true, "The entity should be changed for 'A'");

                revertFn(es, entity, "A");

                equal(es.isUpdated(entity, "A"), false, "The array should no longer have changes");
                equal(entity.IsUpdated(), false, "The entity should no longer be changed");
                equal(entity.A.IsUpdated(), false, "The entity should no longer be changed for 'A'");
                dataEqual(entity, datasets.ko_array(1), "The entity should be reverted");
            } finally {
                upshot.observability.configuration = observability.jquery;
            }
        });
    });

    testWithRevertVariations(true, function (revertFn) {
        test("isUpdated returns true on knockout array remove", 16, function () {
            try {
                upshot.observability.configuration = observability.knockout;

                var es = createEntitySet(),
                    entity = loadEntities(es, datasets.ko_array(1, true));

                equal(es.getEntities()().length, 1, "There should be a single entity loaded");

                equal(es.getEntityState(entity), "Unmodified", "The entity state should be 'Unmodified'");
                equal(es.isUpdated(entity), false, "The entity should not have changes");
                equal(es.isUpdated(entity, "A"), false, "The array should not have changes");
                equal(entity.A.IsUpdated(), false, "The entity should not be changed for 'A'");

                var length = entity.A().length;

                entity.A.shift();

                equal(es.getEntityState(entity), "ClientUpdated", "The entity state should be 'ClientUpdated'");
                equal(es.isUpdated(entity), true, "The entity should have changes");
                equal(es.isUpdated(entity, "A"), true, "The array should have changes");
                equal(entity.A().length, length - 1, "The updated array lengths should be equal");
                equal(entity.A.IsUpdated(), true, "The entity should be changed for 'A'");

                revertFn(es, entity, "A");

                equal(es.isUpdated(entity, "A"), false, "The array should no longer have changes");
                equal(entity.IsUpdated(), false, "The entity should no longer be changed");
                equal(entity.A.IsUpdated(), false, "The entity should no longer be changed for 'A'");
                dataEqual(entity, datasets.ko_array(1), "The entity should be reverted");
            } finally {
                upshot.observability.configuration = observability.jquery;
            }
        });
    });

    testWithRevertVariations(true, function (revertFn) {
        test("isUpdated returns true on multiple knockout array updates", 21, function () {
            try {
                upshot.observability.configuration = observability.knockout;

                var es = createEntitySet(),
                    entity = loadEntities(es, datasets.ko_array(1, true));

                equal(es.getEntities()().length, 1, "There should be a single entity loaded");

                equal(es.getEntityState(entity), upshot.EntityState.Unmodified, "The entity state should be 'Unmodified'");
                equal(es.isUpdated(entity), false, "The entity should not have changes");
                equal(es.isUpdated(entity, "A"), false, "The array should not have changes");
                equal(entity.A.IsUpdated(), false, "The entity should not be changed for 'A'");

                var length = entity.A().length,
                    _new = { B: false };

                entity.A.push(_new);

                equal(es.getEntityState(entity), upshot.EntityState.ClientUpdated, "The entity state should be 'ClientUpdated'");
                equal(es.isUpdated(entity), true, "The entity should have changes");
                equal(es.isUpdated(entity, "A"), true, "The array should have changes");
                equal(es.isUpdated(entity, "A[1]"), false, "The last object should not have changes");
                equal(entity.A().length, length + 1, "The updated array lengths should be equal");
                equal(entity.A.IsUpdated(), true, "The entity should be changed for 'A'");

                entity.A.pop();
                entity.A.pop();

                equal(es.isUpdated(entity), true, "The entity should still have changes");
                equal(es.isUpdated(entity, "A"), true, "The array should still have changes");
                equal(entity.A().length, length - 1, "The updated array lengths should be equal");
                equal(entity.A.IsUpdated(), true, "The entity should still be changed for 'A'");

                revertFn(es, entity, "A");

                equal(es.isUpdated(entity, "A"), false, "The array should no longer have changes");
                equal(entity.IsUpdated(), false, "The entity should no longer be changed");
                equal(entity.A.IsUpdated(), false, "The entity should no longer be changed for 'A'");
                dataEqual(entity, datasets.ko_array(1), "The entity should be reverted");
            } finally {
                upshot.observability.configuration = observability.jquery;
            }
        });
    });

    testWithRevertVariations(true, function (revertFn) {
        test("changes to a nested object are cleared on revert changes", 14, function () {
            var es = createEntitySet(),
                entity = loadEntities(es, datasets.nested.create(1));

            equal(es.getEntities().length, 1, "There should be a single entity loaded");

            equal(es.getEntityState(entity), upshot.EntityState.Unmodified, "The entity state should be 'Unmodified'");
            equal(es.isUpdated(entity), false, "The entity should not have changes");
            equal(es.isUpdated(entity, "O"), false, "The nested object should not have changes");

            $.observable(entity.O).property("B", false);

            equal(es.getEntityState(entity), upshot.EntityState.ClientUpdated, "The entity state should be 'ClientUpdated'");
            equal(es.isUpdated(entity), true, "The entity should have changes");
            equal(es.isUpdated(entity, "O"), true, "The nested object should have changes");
            equal(es.isUpdated(entity, "O.B"), true, "The nested object should have changes for 'B'");

            var original = entity.O;
            $.observable(entity).property("O", null);

            equal(es.isUpdated(entity), true, "The entity should have changes");
            equal(es.isUpdated(entity, "O"), true, "The entity should have changes for 'O'");

            revertFn(es, entity, "O");

            equal(es.isUpdated(entity, "O"), false, "The nested object should no longer have changes");
            dataEqual(entity, datasets.nested.create(1), "The entity should be reverted");
        });
    });

    testWithRevertVariations(true, function (revertFn) {
        test("changes to an array are cleared on revert changes", 20, function () {
            var es = createEntitySet(),
                entity = loadEntities(es, datasets.nestedArrays.create(1));

            equal(es.getEntities().length, 1, "There should be a single entity loaded");

            equal(es.getEntityState(entity), upshot.EntityState.Unmodified, "The entity state should be 'Unmodified'");
            equal(es.isUpdated(entity), false, "The entity should not have changes");
            equal(es.isUpdated(entity, "A"), false, "The array should not have changes");
            equal(es.isUpdated(entity, "A[0]"), false, "The object at index 0 should not have changes");
            equal(es.isUpdated(entity, "A[0].A"), false, "The nested array should not have changes");

            $.observable(entity.A[0].A).remove(0, 1);

            equal(es.getEntityState(entity), upshot.EntityState.ClientUpdated, "The entity state should be 'ClientUpdated'");
            equal(es.isUpdated(entity), true, "The entity should have changes");
            equal(es.isUpdated(entity, "A"), true, "The array should have changes");
            equal(es.isUpdated(entity, "A[0]"), true, "The object at index 0 should have changes");
            equal(es.isUpdated(entity, "A[0].A"), true, "The nested array should have changes");

            var original = entity.A[0];
            $.observable(entity.A).remove(0, 1);

            equal(es.isUpdated(entity), true, "The entity should have changes");
            equal(es.isUpdated(entity, "A"), true, "The array should have changes");
            equal(es.isUpdated(entity, "A[0]"), false, "The object at index 0 should not have changes");

            revertFn(es, entity, "A");

            equal(es.isUpdated(entity, "A"), false, "The array should no longer have changes");
            equal(es.isUpdated(entity, "A[0]"), false, "The object at index 0 should no longer have changes");
            equal(es.isUpdated(entity, "A[0].A"), false, "The nested array should no longer have changes");
            dataEqual(entity, datasets.nestedArrays.create(1), "The entity should be reverted");
        });
    });

    test("_getOriginalValue returns the original value", 4, function () {
        var es = createEntitySet(),
            entity = loadEntities(es, datasets.nestedArrays.create(1));

        equal(es.getEntities().length, 1, "There should be a single entity loaded");

        equal(es.getEntityState(entity), upshot.EntityState.Unmodified, "The entity state should be 'Unmodified'");

        $.observable(entity).property("B", false);
        $.observable(entity).property("S", "S");
        $.observable(entity.A).insert(entity.A.length, [{ B: true}]);
        $.observable(entity.A[1]).property("N", -1);
        $.observable(entity.A[0].A).replaceAll([{ B: true }, { B: true}]);

        equal(es.isUpdated(entity), true, "The entity should have changes");

        dataEqual(es._getOriginalValue(entity), datasets.nestedArrays.create(1), "Original values should be equal.");
    });

    test("_merge sets new values into entity", 2, function () {
        var es = createEntitySet(),
            entity = loadEntities(es, datasets.nestedArrays.create(1));

        equal(es.getEntities().length, 1, "There should be a single entity loaded");

        var _new = datasets.nestedArrays.create(1);

        _new.B = false;
        _new.N = 3;
        _new.A[0].B = false;
        _new.A[1].A.push({ B: true });

        es._merge(entity, _new);

        dataEqual(entity, _new, "The entity should equal new values");
    });

    testWithRevertVariations(function (revertFn) {
        test("_merge does not update a modified entity", 15, function () {
            var es = createEntitySet(),
                entity = loadEntities(es, datasets.nestedArrays.create(1));

            equal(es.getEntities().length, 1, "There should be a single entity loaded");

            equal(es.getEntityState(entity), upshot.EntityState.Unmodified, "The entity state should be 'Unmodified'");

            $.observable(entity).property("N", -1);

            equal(es.isUpdated(entity), true, "The entity should have changes");
            equal(es.isUpdated(entity, "N"), true, "The entity should have changes for 'N'");

            var original = es._getOriginalValue(entity),
                _new = datasets.nestedArrays.create(1);
            _new.B = false;

            es._merge(entity, _new);

            equal(es.isUpdated(entity), true, "The entity should have changes");
            equal(es.isUpdated(entity, "N"), true, "The entity should have changes for 'N'");
            equal(entity.B, original.B, "The value of 'B' should match the original value");
            notEqual(entity.B, _new.B, "The value of 'B' should not match the new value");

            revertFn(es, entity, "N");

            equal(es.isUpdated(entity, "N"), false, "The entity should no longer have changes for 'N'");
            equal(entity.B, original.B, "The value of 'B' should match the original value");
            notEqual(entity.B, _new.B, "The value of 'B' should not match the new value");
            dataEqual(entity, original, "The entity should equal the original values");
        });
    });


    function createEntitySet() {
        var dataContext = new upshot.DataContext(new upshot.riaDataProvider()),
            entitySet = new upshot.EntitySet(dataContext, "entityType");

        upshot.metadata("entityType", {
            key: ["Id"]
        });
        return entitySet;
    }

    function loadEntities(entitySet, entities) {
        var entities2 = entities;
        if (!upshot.isArray(entities)) {
            entities2 = [entities];
        }
        entities2 = entitySet.__loadEntities(entities2);
        if (!upshot.isArray(entities)) {
            return entities2[0];
        }
        return entities2;
    }

    function createEventTracker(entitySet) {
        var e = [],
            tracker = {
                events: e,
                propertyChanged: function (entity, path, value) {
                    e.push({ type: "propertyChanged", entity: entity, path: path, value: value });
                },
                entityUpdated: function (entity, path, eventArgs) {
                    e.push({ type: "entityUpdated", entity: entity, path: path, eventArgs: eventArgs });
                }
            };
        entitySet.bind("propertyChanged", tracker.propertyChanged);
        entitySet.bind("entityUpdated", tracker.entityUpdated);
        return tracker;
    }

    function dataEqual(actual, expected, message) {
        var count = 0;
        function compare(actualValue, expectedValue, property) {
            count++;
            if (actualValue !== expectedValue) {
                return count + ": property '" + property + "' with value '" + actualValue + "' does not equal expected value '" + expectedValue + "'.\n";
            }
            return "";
        }

        equal(dataEqualRecursive(actual, expected, compare), "", message);
    }

    function dataEqualRecursive(actual, expected, compare, property) {
        var difference = "";
        if (upshot.isArray(actual) && upshot.isArray(expected)) {
            $.each(expected, function (index, value) {
                difference += dataEqualRecursive(actual[index], value, compare, index);
            });
        } else if (upshot.isObject(actual) && upshot.isObject(expected)) {
            $.each(expected, function (key, value) {
                difference += dataEqualRecursive(actual[key], value, compare, key);
            });
        } else if (ko.isObservable(actual) && ko.isObservable(expected)) {
            difference = dataEqualRecursive(ko.utils.unwrapObservable(actual), ko.utils.unwrapObservable(expected), compare, property);
        } else if (upshot.isDate(actual) && upshot.isDate(expected)) {
            difference = compare(actual.toString(), expected.toString(), property);
        } else {
            difference = compare(actual, expected, property);
        }
        return difference;
    }

    function testWithRevertVariations(nested, testFn) {
        if ($.isFunction(nested)) {
            testFn = nested;
            nested = false;
        }
        testFn(function (es, entity, propertyName) {
            var observer;
            if (!nested) {
                observer = function () {
                    equal(es.isUpdated(entity, propertyName), false, "The entity should not have property changes during callbacks");
                };
                es.bind("propertyChanged", observer);
            }
            es.revertChanges();
            if (!nested) {
                es.unbind("propertyChanged", observer);
            }
            equal(es.getEntityState(entity), upshot.EntityState.Unmodified, "The entity state should be 'Unmodified' again");
            equal(es.isUpdated(entity), false, "The entity should no longer have changes");
        });
        testFn(function (es, entity, propertyName, isUpdated) {
            var observer;
            if (!nested) {
                observer = function () {
                    equal(es.isUpdated(entity, propertyName), false, "The entity should not have property changes during callbacks");
                };
                es.bind("propertyChanged", observer);
            }
            es.revertUpdates(entity, propertyName);
            if (!nested) {
                es.unbind("propertyChanged", observer);
            }
            var expectedState = !!isUpdated ? upshot.EntityState.ClientUpdated : upshot.EntityState.Unmodified;
            equal(es.getEntityState(entity), expectedState, "The entity state should still be '" + expectedState + "'");
            equal(es.isUpdated(entity), !!isUpdated, "The entity should " + (!!isUpdated ? "" : "no longer") + " have changes");
        });
    }

})(this, jQuery, upshot);
