/// <reference path="../Scripts/References.js" />
(function (global, upshot, undefined) {

    module("Delete.tests.js", {
        teardown: function () {
            testHelper.unmockAjax();
        }
    });

    function createDataSource(useLocal, result, bufferChanges) {
        if (useLocal) {
            var lds = new upshot.LocalDataSource({
                source: createDataSource(false, null, !!bufferChanges),
                result: result,
                filter: [{ property: "Price", operator: ">", value: 300 }, { property: "Price", operator: "<", value: 700 }]
            });
            return lds;
        } else {
            return new upshot.RemoteDataSource({
                providerParameters: { url: "unused", operationName: "" },
                provider: upshot.riaDataProvider,
                result: result,
                bufferChanges: !!bufferChanges
            });
        }
    }


    function createProductsResult() {
        return {
            GetProductsResult: {
                TotalCount: 5,
                RootResults: [
                    { ID: 1, Manufacturer: "Canon", Price: 200 },
                    { ID: 2, Manufacturer: "Nikon", Price: 400 },
                    { ID: 3, Manufacturer: "Pentax", Price: 500 },
                    { ID: 4, Manufacturer: "Sony", Price: 600 },
                    { ID: 5, Manufacturer: "Olympus", Price: 800 }
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

    function createProfileResult() {
        return {
            GetProfileForProfileUpdateResult: {
                TotalCount: -1,
                IncludedResults: [
                    { __type: "Friend:#BigShelf.Models", FriendId: 2, Id: 1, ProfileId: 1 },
                    { __type: "Friend:#BigShelf.Models", FriendId: 3, Id: 2, ProfileId: 1 },
                    { __type: "Friend:#BigShelf.Models", FriendId: 6, Id: 4, ProfileId: 1 },
                    { __type: "Profile:#BigShelf.Models", AspNetUserGuid: "5ad7976c-7a95-47aa-87ba-29c3fc80643e", EmailAddress: "deepm@microsoft.com", Id: 2, Name: "Deepesh Mohnani" },
                    { __type: "Profile:#BigShelf.Models", AspNetUserGuid: "9330619d-9a8c-4269-9bde-ba4cb1b7b354", EmailAddress: "jeffhand@microsoft.com", Id: 3, Name: "Jeff Handley" },
                    { __type: "Profile:#BigShelf.Models", AspNetUserGuid: "990c62c5-30a4-4ca9-92c5-f248241f6d44", EmailAddress: "gblock@microsoft.com", Id: 6, Name: "Glenn Block" }
                ],
                RootResults: [{ AspNetUserGuid: "3730F12D-1A2A-4499-859B-9F586B2858A4", EmailAddress: "demo@microsoft.com", Id: 1, Name: "Demo User"}],
                Metadata: [
                    {
                        type: "Profile:#BigShelf.Models",
                        key: ["Id"],
                        fields: {
                            AspNetUserGuid: { type: "String:#System" },
                            EmailAddress: { type: "String:#System" },
                            Friends: { type: "Friend:#BigShelf.Models", array: true, association: { name: "Profile_Friend", thisKey: ["Id"], otherKey: ["ProfileId"], isForeignKey: false} },
                            Id: { type: "Int32:#System" },
                            Name: { type: "String:#System" }
                        },
                        rules: {
                            EmailAddress: { required: true, email: true },
                            Name: { required: true }
                        }
                    },
                    {
                        type: "Friend:#BigShelf.Models",
                        key: ["Id"],
                        fields: {
                            FriendId: { type: "Int32:#System" },
                            FriendProfile: { type: "Profile:#BigShelf.Models", association: { name: "Profile_Friend1", thisKey: ["FriendId"], otherKey: ["Id"], isForeignKey: true} },
                            Id: { type: "Int32:#System" },
                            Profile: { type: "Profile:#BigShelf.Models", association: { name: "Profile_Friend", thisKey: ["ProfileId"], otherKey: ["Id"], isForeignKey: true} },
                            ProfileId: { type: "Int32:#System" }
                        }
                    }
                ]
            }
        };
    }

    // "GetProfileForProfileUpdateResult":{"TotalCount":-1,"IncludedResults":[{"__type":"Friend:#BigShelf.Models","FriendId":2,"Id":1,"ProfileId":1},{"__type":"Friend:#BigShelf.Models","FriendId":3,"Id":2,"ProfileId":1},{"__type":"Friend:#BigShelf.Models","FriendId":6,"Id":4,"ProfileId":1},{"__type":"Profile:#BigShelf.Models","AspNetUserGuid":"5ad7976c-7a95-47aa-87ba-29c3fc80643e","EmailAddress":"deepm@microsoft.com","Id":2,"Name":"Deepesh Mohnani"},{"__type":"Profile:#BigShelf.Models","AspNetUserGuid":"9330619d-9a8c-4269-9bde-ba4cb1b7b354","EmailAddress":"jeffhand@microsoft.com","Id":3,"Name":"Jeff Handley"},{"__type":"Profile:#BigShelf.Models","AspNetUserGuid":"990c62c5-30a4-4ca9-92c5-f248241f6d44","EmailAddress":"gblock@microsoft.com","Id":6,"Name":"Glenn Block"}],"RootResults":[{"AspNetUserGuid":"3730F12D-1A2A-4499-859B-9F586B2858A4","EmailAddress":"demo@microsoft.com","Id":1,"Name":"Demo User"}],"Metadata":[{"type":"Profile:#BigShelf.Models","key":["Id"],"fields":{"AspNetUserGuid":{"type":"String:#System"},"Categories":{"type":"Category:#BigShelf.Models","array":true,"association":{"name":"Profile_Category","thisKey":["Id"],"otherKey":["ProfileId"],"isForeignKey":false}},"EmailAddress":{"type":"String:#System"},"FlaggedBooks":{"type":"FlaggedBook:#BigShelf.Models","array":true,"association":{"name":"Profile_FlaggedBook","thisKey":["Id"],"otherKey":["ProfileId"],"isForeignKey":false}},"Friends":{"type":"Friend:#BigShelf.Models","array":true,"association":{"name":"Profile_Friend","thisKey":["Id"],"otherKey":["ProfileId"],"isForeignKey":false}},"Id":{"type":"Int32:#System"},"Name":{"type":"String:#System"}},"rules":{"EmailAddress":{"required":true,"email":true},"Name":{"required":true}}},{"type":"Book:#BigShelf.Models","key":["Id"],"fields":{"AddedDate":{"type":"DateTime:#System"},"ASIN":{"type":"String:#System"},"Author":{"type":"String:#System"},"CategoryId":{"type":"Int32:#System"},"CategoryName":{"type":"CategoryName:#BigShelf.Models","association":{"name":"CategoryName_Book","thisKey":["CategoryId"],"otherKey":["Id"],"isForeignKey":true}},"Description":{"type":"String:#System"},"FlaggedBooks":{"type":"FlaggedBook:#BigShelf.Models","array":true,"association":{"name":"Book_FlaggedBook","thisKey":["Id"],"otherKey":["BookId"],"isForeignKey":false}},"Id":{"type":"Int32:#System"},"PublishDate":{"type":"DateTime:#System"},"Title":{"type":"String:#System"}}},{"type":"FlaggedBook:#BigShelf.Models","key":["Id"],"fields":{"Book":{"type":"Book:#BigShelf.Models","association":{"name":"Book_FlaggedBook","thisKey":["BookId"],"otherKey":["Id"],"isForeignKey":true}},"BookId":{"type":"Int32:#System"},"Id":{"type":"Int32:#System"},"IsFlaggedToRead":{"type":"Int32:#System"},"Profile":{"type":"Profile:#BigShelf.Models","association":{"name":"Profile_FlaggedBook","thisKey":["ProfileId"],"otherKey":["Id"],"isForeignKey":true}},"ProfileId":{"type":"Int32:#System"},"Rating":{"type":"Int32:#System"}}},{"type":"Friend:#BigShelf.Models","key":["Id"],"fields":{"FriendId":{"type":"Int32:#System"},"FriendProfile":{"type":"Profile:#BigShelf.Models","association":{"name":"Profile_Friend1","thisKey":["FriendId"],"otherKey":["Id"],"isForeignKey":true}},"Id":{"type":"Int32:#System"},"Profile":{"type":"Profile:#BigShelf.Models","association":{"name":"Profile_Friend","thisKey":["ProfileId"],"otherKey":["Id"],"isForeignKey":true}},"ProfileId":{"type":"Int32:#System"}}},{"type":"Category:#BigShelf.Models","key":["Id"],"fields":{"CategoryId":{"type":"Int32:#System"},"CategoryName":{"type":"CategoryName:#BigShelf.Models","association":{"name":"CategoryName_Category","thisKey":["CategoryId"],"otherKey":["Id"],"isForeignKey":true}},"Id":{"type":"Int32:#System"},"Profile":{"type":"Profile:#BigShelf.Models","association":{"name":"Profile_Category","thisKey":["ProfileId"],"otherKey":["Id"],"isForeignKey":true}},"ProfileId":{"type":"Int32:#System"}}},{"type":"CategoryName:#BigShelf.Models","key":["Id"],"fields":{"Books":{"type":"Book:#BigShelf.Models","array":true,"association":{"name":"CategoryName_Book","thisKey":["Id"],"otherKey":["CategoryId"],"isForeignKey":false}},"Categories":{"type":"Category:#BigShelf.Models","array":true,"association":{"name":"CategoryName_Category","thisKey":["Id"],"otherKey":["CategoryId"],"isForeignKey":false}},"Id":{"type":"Int32:#System"},"Name":{"type":"String:#System"}}}]}}"	String

    function createSubmitResult(results) {
        var changeResult = [];
        for (var i = 0; i < results.length; ++i) {
            var result = {
                Entity: results[i].entity,
                EntityActions: null,
                HasMemberChanges: false,
                Id: i,
                Operation: 4
            };
            if (results[i].errors) {
                result.ValidationErrors = results[i].errors;
            }
            changeResult.push(result);
        }
        return { SubmitChangesResult: changeResult };
    }

    function deleteEntities(products, results, bufferChanges, destructive, revert) {
        dsTestDriver.simulatePostSuccessService(createSubmitResult(results));
        setTimeout(function () {
            for (var i = 0; i < results.length; ++i) {
                if (destructive) {
                    $.observable(products).remove(results[i].entity);
                    equal(upshot.EntitySource.as(products).getEntityState(results[i].entity), upshot.EntityState.ClientDeleted, "expect delete state");
                } else {
                    upshot.EntitySource.as(products).deleteEntity(results[i].entity);
                    equal(upshot.EntitySource.as(products).getEntityState(results[i].entity), upshot.EntityState.ClientDeleted, "expect delete state");
                }
            }
            if (bufferChanges) {
                if (revert) {
                    upshot.EntitySource.as(products).revertChanges();
                    for (var i = 0; i < results.length; ++i) {
                        equal(upshot.EntitySource.as(products).getEntityState(results[i].entity), upshot.EntityState.Unmodified, "expect revert state");
                    }
                    revert();
                } else {
                    upshot.EntitySource.as(products).commitChanges();
                }
            }
        }, 10);
    }

    for (var d = 0; d < 1; ++d) {  // TODO: Destructive delete tests disabled until we can redevelop this feature.
        for (var b = 0; b < 2; ++b) {
            (function (bufferChanges, destructive) {

                test((!destructive ? "Non-destructive " : "Destructive ") + "LDS over RDS" + (bufferChanges ? " batch-" : " ") + "success", 7, function () {
                    stop();
                    dsTestDriver.simulateSuccessService(createProductsResult());
                    var rproducts = [];
                    var lproducts = [];
                    var refreshNeeded = 0;
                    var rds = createDataSource(false, rproducts, bufferChanges);
                    var lds = new upshot.LocalDataSource({
                        source: rds,
                        result: lproducts,
                        filter: [{ property: "Price", operator: ">", value: 300 }, { property: "Price", operator: "<", value: 700}]
                    });

                    $([lproducts]).bind("replaceAll", function () {
                        equal(lproducts.length, 3, "lcount matched");
                        equal(rproducts.length, 5, "rcount matched");
                        deleteEntities(rproducts, [{ entity: rproducts[1] }, { entity: rproducts[3]}], bufferChanges, destructive);
                    });

                    lds.bind({
                        refreshNeeded: function () {
                            ++refreshNeeded;
                        }
                    });

                    rds.bind({
                        commitSuccess: function () {
                            // the lproducts is purged automatically thru purge sequence
                            equal(lproducts.length, 1, "lcount matched");
                            equal(rproducts.length, 3, "rcount matched");
                            equal(refreshNeeded, destructive ? 1 : 0, "refreshNeeded matched");  // Non-destructive will only trigger refreshNeeded when the LDS has paging parameters.
                            start();
                        }
                    });

                    lds.refresh({ all: true });
                });

                if (bufferChanges) {
                    test((!destructive ? "Non-destructive " : "Destructive ") + "LDS over RDS" + (bufferChanges ? " batch-" : " ") + "revert", 9, function () {
                        stop();
                        dsTestDriver.simulateSuccessService(createProductsResult());
                        var rproducts = [];
                        var lproducts = [];
                        var refreshNeeded = 0;
                        var rds = createDataSource(false, rproducts, bufferChanges);
                        var lds = new upshot.LocalDataSource({
                            source: rds,
                            result: lproducts,
                            filter: [{ property: "Price", operator: ">", value: 300 }, { property: "Price", operator: "<", value: 700 }]
                        });

                        $([lproducts]).bind("replaceAll", function () {
                            var revert = function () {
                                equal(lproducts.length, 3, "lcount matched");
                                equal(rproducts.length, destructive ? 3 : 5, "rcount matched");
                                equal(refreshNeeded, destructive ? 1 : 0, "refreshNeeded matched");  // Non-destructive will only see entity states change.  No reason to refresh.
                                start();
                            }
                            equal(lproducts.length, 3, "lcount matched");
                            equal(rproducts.length, 5, "rcount matched");
                            deleteEntities(rproducts, [{ entity: rproducts[1] }, { entity: rproducts[3]}], bufferChanges, destructive, revert);
                        });

                        lds.bind({
                            refreshNeeded: function () {
                                ++refreshNeeded;
                            }
                        });

                        lds.refresh({ all: true });
                    });
                }
            })(!!b, !!d);
        }
    }

    for (var d = 0; d < 1; ++d) {  // TODO: Destructive delete tests disabled until we can redevelop this feature.
        for (var b = 0; b < 2; ++b) {
            (function (bufferChanges, destructive) {
                // AssociatedEntitiesView does not support batch commit
                if (!bufferChanges) {
                    test((!destructive ? "Non-destructive " : "Destructive ") + "AssociatedEntitiesView" + (bufferChanges ? " batch-" : " ") + "success", 4, function () {
                        stop();
                        dsTestDriver.simulateSuccessService(createProfileResult());
                        var profiles = [];
                        var profile;
                        var rds = createDataSource(false, profiles, bufferChanges);

                        $([profiles]).bind("replaceAll", function () {
                            equal(profiles.length, 1, "profiles length");
                            profile = profiles[0];
                            var friends = profile.Friends;
                            equal(friends.length, 3, "friends length");
                            deleteEntities(friends, [{ entity: friends[1]}], bufferChanges, destructive);
                        });

                        rds.bind({
                            commitSuccess: function () {
                                // the lproducts is purged automatically thru purge sequence
                                var friends = profile.Friends;
                                equal(friends.length, 2, "friends length");
                                start();
                            }
                        });

                        rds.refresh();
                    });
                }

                if (bufferChanges) {
                    test((!destructive ? "Non-destructive " : "Destructive ") + "AssociatedEntitiesView" + (bufferChanges ? " batch-" : " ") + "revert", 5, function () {
                        stop();
                        dsTestDriver.simulateSuccessService(createProfileResult());
                        var profiles = [];
                        var profile;
                        var rds = createDataSource(false, profiles, bufferChanges);

                        $([profiles]).bind("replaceAll", function () {
                            equal(profiles.length, 1, "profiles length");
                            profile = profiles[0];
                            var friends = profile.Friends;
                            equal(friends.length, 3, "friends length");

                            var revert = function () {
                                // the lproducts is purged automatically thru purge sequence
                                var friends = profile.Friends;
                                equal(friends.length, destructive ? 2 : 3, "friends length");
                                start();
                            };
                            deleteEntities(friends, [{ entity: friends[1]}], bufferChanges, destructive, revert);
                        });

                        rds.refresh();
                    });
                }

            })(!!b, !!d);
        }
    }

    // return; // TODO, suwatch: below tests multiple variations and scenarios.
    // commented out for now as it might be hard to debug.  Will only be run by me.

    // Testing a combination of ..
    // LDS vs. RDS
    // Batch vs. Auto commit
    // Destructive vs. Non-destructive delete
    // success vs. failure (failure-revert vs. failure-recommit) vs. simply revert
    for (var d = 0; d < 1; ++d) {  // TODO: Destructive delete tests disabled until we can redevelop this feature.
        for (var l = 0; l < 2; ++l) {
            for (var b = 0; b < 2; ++b) {

                (function (useLocal, bufferChanges, destructive) {

                    test((!destructive ? "Non-destructive " : "Destructive ") + (!!useLocal ? "LDS" : "RDS") + (bufferChanges ? " batch-" : " ") + "success", 13, function () {
                        stop();
                        dsTestDriver.simulateSuccessService(createProductsResult());
                        var products = [];
                        var removes = [];
                        var refreshNeeded = 0;

                        $([products]).bind("replaceAll", function () {
                            if (useLocal) {
                                equal(products.length, 3, "count matched");
                                deleteEntities(products, [{ entity: products[0] }, { entity: products[2]}], bufferChanges, destructive);
                            } else {
                                equal(products.length, 5, "count matched");
                                deleteEntities(products, [{ entity: products[1] }, { entity: products[3]}], bufferChanges, destructive);
                            }
                        });

                        $([products]).bind("remove insert", function (event, args) {
                            equal(event.type, "remove", "expect event");
                            removes.push(args.items[0]);
                        });

                        var ds = createDataSource(useLocal, products, bufferChanges);

                        var refreshNeededObserver = {
                            refreshNeeded: function () {
                                ++refreshNeeded;
                            }
                        };

                        var commitObserver = {
                            commitSuccess: function () {
                                equal(removes.length, 2, "removes matched");
                                equal(removes[0].ID, 2, "REMOVE0 matched");
                                equal(removes[1].ID, 4, "REMOVE1 matched");
                                removes = [];
                                if (useLocal) {
                                    equal(products.length, 1, "count matched");
                                    equal(products[0].ID, 3, "ID0 matched");
                                    equal(products[1], undefined, "ID1 matched");
                                    equal(products[2], undefined, "ID2 matched");
                                } else {
                                    equal(products.length, 3, "count matched");
                                    equal(products[0].ID, 1, "ID0 matched");
                                    equal(products[1].ID, 3, "ID1 matched");
                                    equal(products[2].ID, 5, "ID2 matched");
                                }
                                equal(refreshNeeded, 0, "refreshNeeded event");  // Non-destructive will only trigger refreshNeeded when the LDS has paging parameters.
                                start();
                            }
                        };

                        if (useLocal) {
                            // LDS does not have Commit api
                            ds.commitChanges = ds.commitChanges || function () { ds._entitySource.commitChanges(); };
                            ds.bind(refreshNeededObserver);
                            ds._entitySource.bind(commitObserver);
                        } else {
                            ds.bind(commitObserver);
                        }

                        ds.refresh(useLocal && { all: true });
                    });

                    if (bufferChanges) {
                        test((!destructive ? "Non-destructive " : "Destructive ") + (!!useLocal ? "LDS" : "RDS") + " batch-revert", 8, function () {
                            stop();
                            dsTestDriver.simulateSuccessService(createProductsResult());
                            var products = [];
                            var removes = [];
                            var refreshNeeded = 0;
                            $([products]).bind("replaceAll", function () {
                                var revert = function () {
                                    equal(removes.length, destructive ? 2 : 0, "removes length matched");
                                    removes = [];
                                    if (useLocal) {
                                        equal(products.length, destructive ? 1 : 3, "count matched");
                                    } else {
                                        equal(products.length, destructive ? 3 : 5, "count matched");
                                    }
                                    // revert destructive should raise refreshNeeded event
                                    equal(refreshNeeded, (useLocal && destructive) ? 1 : 0, "refreshNeeded event");  // Non-destructive will only see entity states change.  No reason to refresh.
                                    start();
                                };
                                if (useLocal) {
                                    equal(products.length, 3, "count matched");
                                    deleteEntities(products, [{ entity: products[0] }, { entity: products[2]}], bufferChanges, destructive, revert);
                                } else {
                                    equal(products.length, 5, "count matched");
                                    deleteEntities(products, [{ entity: products[1] }, { entity: products[3]}], bufferChanges, destructive, revert);
                                }
                            });
                            $([products]).bind("remove", function (event, args) {
                                //equal(event.type, "remove", "expect event");
                                removes.push(args.items[0]);
                            });
                            var ds = createDataSource(useLocal, products, bufferChanges);
                            if (useLocal) {
                                ds.bind({ refreshNeeded: function () { refreshNeeded++; } });
                            }
                            ds.refresh(useLocal && { all: true });
                        });
                    }

                    if (!destructive) {
                        test((!destructive ? "Non-destructive " : "Destructive ") + (!!useLocal ? "LDS" : "RDS") + (bufferChanges ? " batch-" : " ") + "failure-recommit", 21, function () {
                            stop();
                            dsTestDriver.simulateSuccessService(createProductsResult());
                            var products = [];
                            var removes = [];
                            var refreshNeeded = 0;
                            $([products]).bind("replaceAll", function () {
                                if (useLocal) {
                                    equal(products.length, 3, "count matched");
                                    deleteEntities(products, [{ entity: products[0] }, { entity: products[2], errors: [{ Message: "Simulated failure!"}]}], bufferChanges, destructive);
                                } else {
                                    equal(products.length, 5, "count matched");
                                    deleteEntities(products, [{ entity: products[1] }, { entity: products[3], errors: [{ Message: "Simulated failure!"}]}], bufferChanges, destructive);
                                }
                            });
                            $([products]).bind("remove", function (event, args) {
                                //equal(event.type, "remove", "expect event");
                                removes.push(args.items[0]);
                            });
                            var ds = createDataSource(useLocal, products, bufferChanges);

                            var refreshNeededObserver = {
                                refreshNeeded: function () {
                                    ++refreshNeeded;
                                }
                            };

                            var commitObserver = {
                                commitSuccess: function () {
                                    equal(removes.length, 1, "removes matched");
                                    equal(removes[0].ID, 4, "REMOVE0 matched");
                                    removes = [];
                                    if (useLocal) {
                                        equal(products.length, 1, "count matched");
                                        equal(products[0].ID, 3, "ID1 matched");
                                        equal(products[1], undefined, "ID1 matched");
                                        equal(products[2], undefined, "ID2 matched");
                                    } else {
                                        equal(products.length, 3, "count matched");
                                        equal(products[0].ID, 1, "ID0 matched");
                                        equal(products[1].ID, 3, "ID1 matched");
                                        equal(products[2].ID, 5, "ID2 matched");
                                    }
                                    equal(refreshNeeded, (useLocal && destructive) ? 1 : 0, "refreshNeeded event");  // Non-destructive will only trigger refreshNeeded when the LDS has paging parameters.
                                    start();
                                },
                                commitError: function (httpStatus, errorText) {
                                    equal(removes.length, destructive ? 2 : 1, "removes matched");
                                    equal(removes[0].ID, 2, "REMOVE0 matched");
                                    var removed;
                                    if (destructive) {
                                        equal(removes[1].ID, 4, "REMOVE1 matched");
                                        removed = removes[1];
                                    } else {
                                        equal(removes[1], undefined, "REMOVE1 matched");
                                    }
                                    removes = [];
                                    if (useLocal) {
                                        equal(products.length, destructive ? 1 : 2, "count matched");
                                        equal(errorText, "Simulated failure!", "errorText matched");
                                        equal(products[0].ID, 3, "ID0 matched");
                                        if (destructive) {
                                            equal(products[1], undefined, "ID1 matched");
                                            equal(this.getEntityState(removed), upshot.EntityState.ClientDeleted, "state checked");
                                        } else {
                                            equal(products[1].ID, 4, "ID1 matched");
                                            equal(this.getEntityState(products[1]), upshot.EntityState.ClientDeleted, "state checked");
                                        }
                                        equal(products[2], undefined, "ID2 matched");
                                        equal(products[3], undefined, "ID3 matched");
                                        deleteEntities(products, [{ entity: removed || products[1]}], bufferChanges, destructive);
                                    } else {
                                        equal(products.length, destructive ? 3 : 4, "count matched");
                                        equal(errorText, "Simulated failure!", "errorText matched");
                                        equal(products[0].ID, 1, "ID0 matched");
                                        equal(products[1].ID, 3, "ID1 matched");
                                        if (destructive) {
                                            equal(products[2].ID, 5, "ID2 matched");
                                            equal(products[3], undefined, "ID3 matched");
                                            equal(this.getEntityState(removed), undefined, "state checked");
                                        } else {
                                            equal(products[2].ID, 4, "ID2 matched");
                                            equal(products[3].ID, 5, "ID3 matched");
                                            equal(this.getEntityState(products[2]), upshot.EntityState.ClientDeleted, "state checked");
                                        }
                                        deleteEntities(products, [{ entity: removed || products[2]}], bufferChanges, destructive);
                                    }
                                }
                            };

                            if (useLocal) {
                                // LDS does not have Commit api
                                ds.commitChanges = ds.commitChanges || function () { ds._entitySource.commitChanges(); };
                                ds.bind(refreshNeededObserver);
                                ds._entitySource.bind(commitObserver);
                            } else {
                                ds.bind(commitObserver);
                            }

                            ds.refresh(useLocal && { all: true });
                        });
                    }

                    test((!destructive ? "Non-destructive " : "Destructive ") + (!!useLocal ? "LDS" : "RDS") + (bufferChanges ? " batch-" : " ") + "failure-revert", 15, function () {
                        stop();
                        dsTestDriver.simulateSuccessService(createProductsResult());
                        var products = [];
                        var removes = [];
                        var refreshNeeded = 0;

                        $([products]).bind("replaceAll", function () {
                            if (useLocal) {
                                equal(products.length, 3, "count matched");
                                deleteEntities(products, [{ entity: products[0] }, { entity: products[2], errors: [{ Message: "Simulated failure!"}]}], bufferChanges, destructive);
                            } else {
                                equal(products.length, 5, "count matched");
                                deleteEntities(products, [{ entity: products[1] }, { entity: products[3], errors: [{ Message: "Simulated failure!"}]}], bufferChanges, destructive);
                            }
                        });

                        $([products]).bind("remove", function (event, args) {
                            //equal(event.type, "remove", "expect event");
                            removes.push(args.items[0]);
                        });

                        var ds = createDataSource(useLocal, products, bufferChanges);

                        var refreshNeededObserver = {
                            refreshNeeded: function () {
                                refreshNeeded++;
                            }
                        };

                        var commitObserver = {
                            commitError: function (httpStatus, errorText) {
                                equal(removes.length, destructive ? 2 : 1, "removes matched");
                                equal(removes[0].ID, 2, "REMOVE0 matched");
                                var tmpProducts = [];
                                $.each(products, function (unused, product) {
                                    tmpProducts.push(product);
                                });

                                if (destructive) {
                                    equal(removes[1].ID, 4, "REMOVE1 matched");
                                    tmpProducts.splice(useLocal ? 1 : 2, 0, removes[1]);
                                } else {
                                    equal(removes[1], undefined, "REMOVE1 matched");
                                }
                                removes = [];

                                if (useLocal) {
                                    equal(tmpProducts.length, 2, "count matched");
                                    equal(errorText, "Simulated failure!", "errorText matched");
                                    equal(tmpProducts[0].ID, 3, "ID0 matched");
                                    equal(tmpProducts[1].ID, 4, "ID1 matched");
                                    equal(tmpProducts[2], undefined, "ID2 matched");
                                    equal(tmpProducts[3], undefined, "ID3 matched");
                                    equal(this.getEntityState(tmpProducts[1]), upshot.EntityState.ClientDeleted, "state checked");
                                    this.revertChanges();
                                    equal(this.getEntityState(tmpProducts[1]), upshot.EntityState.Unmodified, "state checked");
                                } else {
                                    equal(tmpProducts.length, 4, "count matched");
                                    equal(errorText, "Simulated failure!", "errorText matched");
                                    equal(tmpProducts[0].ID, 1, "ID0 matched");
                                    equal(tmpProducts[1].ID, 3, "ID1 matched");
                                    equal(tmpProducts[2].ID, 4, "ID2 matched");
                                    equal(tmpProducts[3].ID, 5, "ID3 matched");
                                    equal(this.getEntityState(tmpProducts[2]), upshot.EntityState.ClientDeleted, "state checked");
                                    this.revertChanges();
                                    equal(this.getEntityState(tmpProducts[2]), upshot.EntityState.Unmodified, "state checked");
                                }

                                equal(refreshNeeded, (useLocal && destructive) ? 1 : 0, "refreshNeeded event");  // Non-destructive will only see entity states change.  No reason to refresh.
                                start();
                            }
                        };

                        if (useLocal) {
                            // LDS does not have Commit api
                            ds.commitChanges = ds.commitChanges || function () { ds._entitySource.commitChanges(); };
                            ds.bind(refreshNeededObserver);
                            ds._entitySource.bind(commitObserver);
                        } else {
                            ds.bind(commitObserver);
                        }

                        ds.refresh(useLocal && { all: true });
                    });

                })(!!l, !!b, !!d);
            }
        }
    }

    for (var d = 0; d < 1; ++d) {  // TODO: Destructive delete tests disabled until we can redevelop this feature.
        for (var b = 0; b < 2; ++b) {
            (function (bufferChanges, destructive) {

                test((!destructive ? "Non-destructive " : "Destructive ") + "LDS over RDS" + (bufferChanges ? " batch-" : " ") + "success", 7, function () {
                    stop();
                    dsTestDriver.simulateSuccessService(createProductsResult());
                    var rproducts = [];
                    var lproducts = [];
                    var refreshNeeded = 0;
                    var rds = createDataSource(false, rproducts, bufferChanges);
                    var lds = new upshot.LocalDataSource({
                        source: rds,
                        result: lproducts,
                        filter: [{ property: "Price", operator: ">", value: 300 }, { property: "Price", operator: "<", value: 700 }]
                    });

                    $([lproducts]).bind("replaceAll", function () {
                        equal(lproducts.length, 3, "lcount matched");
                        equal(rproducts.length, 5, "rcount matched");
                        deleteEntities(rproducts, [{ entity: rproducts[1] }, { entity: rproducts[3]}], bufferChanges, destructive);
                    });

                    lds.bind({
                        refreshNeeded: function () {
                            ++refreshNeeded;
                        }
                    });

                    rds.bind({
                        commitSuccess: function () {
                            // the lproducts is purged automatically thru purge sequence
                            equal(lproducts.length, 1, "lcount matched");
                            equal(rproducts.length, 3, "rcount matched");
                            equal(refreshNeeded, destructive ? 1 : 0, "refreshNeeded matched");  // Non-destructive will only trigger refreshNeeded when the LDS has paging parameters.
                            start();
                        }
                    });

                    lds.refresh({ all: true });
                });

                if (bufferChanges) {
                    test((!destructive ? "Non-destructive " : "Destructive ") + "LDS over RDS" + (bufferChanges ? " batch-" : " ") + "revert", 9, function () {
                        stop();
                        dsTestDriver.simulateSuccessService(createProductsResult());
                        var rproducts = [];
                        var lproducts = [];
                        var refreshNeeded = 0;
                        var rds = createDataSource(false, rproducts, bufferChanges);
                        var lds = new upshot.LocalDataSource({
                            source: rds,
                            result: lproducts,
                            filter: [{ property: "Price", operator: ">", value: 300 }, { property: "Price", operator: "<", value: 700 }]
                        });

                        $([lproducts]).bind("replaceAll", function () {
                            var revert = function () {
                                equal(lproducts.length, 3, "lcount matched");
                                equal(rproducts.length, destructive ? 3 : 5, "rcount matched");
                                equal(refreshNeeded, destructive ? 1 : 0, "refreshNeeded matched");  // Non-destructive will only see entity states change.  No reason to refresh.
                                start();
                            }
                            equal(lproducts.length, 3, "lcount matched");
                            equal(rproducts.length, 5, "rcount matched");
                            deleteEntities(rproducts, [{ entity: rproducts[1] }, { entity: rproducts[3]}], bufferChanges, destructive, revert);
                        });

                        lds.bind({
                            refreshNeeded: function () {
                                ++refreshNeeded;
                            }
                        });

                        lds.refresh({ all: true });
                    });
                }
            })(!!b, !!d);
        }
    }

    for (var d = 0; d < 1; ++d) {  // TODO: Destructive delete tests disabled until we can redevelop this feature.
        for (var b = 0; b < 2; ++b) {
            (function (bufferChanges, destructive) {
                // AssociatedEntitiesView does not support batch commit
                if (!bufferChanges) {
                    test((!destructive ? "Non-destructive " : "Destructive ") + "AssociatedEntitiesView" + (bufferChanges ? " batch-" : " ") + "success", 4, function () {
                        stop();
                        dsTestDriver.simulateSuccessService(createProfileResult());
                        var profiles = [];
                        var profile;
                        var rds = createDataSource(false, profiles, bufferChanges);

                        $([profiles]).bind("replaceAll", function () {
                            equal(profiles.length, 1, "profiles length");
                            profile = profiles[0];
                            var friends = profile.Friends;
                            equal(friends.length, 3, "friends length");
                            deleteEntities(friends, [{ entity: friends[1]}], bufferChanges, destructive);
                        });

                        rds.bind({
                            commitSuccess: function () {
                                // the lproducts is purged automatically thru purge sequence
                                var friends = profile.Friends;
                                equal(friends.length, 2, "friends length");
                                start();
                            }
                        });

                        rds.refresh();
                    });
                }

                if (bufferChanges) {
                    test((!destructive ? "Non-destructive " : "Destructive ") + "AssociatedEntitiesView" + (bufferChanges ? " batch-" : " ") + "revert", 5, function () {
                        stop();
                        dsTestDriver.simulateSuccessService(createProfileResult());
                        var profiles = [];
                        var profile;
                        var rds = createDataSource(false, profiles, bufferChanges);

                        $([profiles]).bind("replaceAll", function () {
                            equal(profiles.length, 1, "profiles length");
                            profile = profiles[0];
                            var friends = profile.Friends;
                            equal(friends.length, 3, "friends length");

                            var revert = function () {
                                // the lproducts is purged automatically thru purge sequence
                                var friends = profile.Friends;
                                equal(friends.length, destructive ? 2 : 3, "friends length");
                                start();
                            };
                            deleteEntities(friends, [{ entity: friends[1]}], bufferChanges, destructive, revert);
                        });

                        rds.refresh();
                    });
                }

            })(!!b, !!d);
        }
    }

})(this, upshot);
