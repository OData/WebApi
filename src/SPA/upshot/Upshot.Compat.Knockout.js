/// <reference path="IntelliSense\References.js" />
///#RESTORE (function (global, ko, upshot, undefined)
{
    var cacheKey = "koConfig",
        stateProperty = "EntityState",
        errorProperty = "EntityError",
        updatedProperty = "IsUpdated",
        addedProperty = "IsAdded",
        deletedProperty = "IsDeleted",
        changedProperty = "IsChanged",
        canDeleteProperty = "CanDelete",
        validationErrorProperty = "ValidationError";
    
    var splice = function (array, start, howmany, items) {
        array.splice.apply(array, items ? [start, howmany].concat(items) : [start, howmany]);
    };
    var copy = function (value) {
        var copy = upshot.isArray(value) ? value.slice(0) : value;
        if (value && (value[upshot.cacheName] !== undefined)) {
            // Handles in-place array edits
            copy[upshot.cacheName] = value[upshot.cacheName];
        }
        return copy;
    };
    var executeAndIgnoreChanges = function (observable, fn) {
        var tracker = upshot.cache(observable, cacheKey);
        try {
            tracker && (tracker.skip = true);
            fn();
        } finally {
            tracker && (tracker.skip = false);
        }
    }
    var punchKoMinified = function (obj, fn, _new) {
        // replaces all instance of a function with the new version
        for (var prop in obj) {
            if (obj[prop] === fn) {
                obj[prop] = _new;
            }
        }
    }

    // Observability configuration
    function track(data, options, entityType) {
        if (!options) {
            if (upshot.isObject(data)) {
                for (var prop in data) {
                    var tracker = upshot.cache(data[prop], cacheKey);
                    tracker && tracker.dispose();
                    upshot.deleteCache(data[prop], cacheKey);
                }
            } else if (ko.isObservable(data) && data.hasOwnProperty("push")) { // observableArray
                var tracker = upshot.cache(data[prop], cacheKey);
                tracker && tracker.dispose();
                upshot.deleteCache(data, cacheKey);
            }
        } else {
            if (upshot.isObject(data)) {
                trackObject(data, options, entityType);
            } else if (ko.isObservable(data) && data.hasOwnProperty("push")) { // observableArray
                trackArray(data, options);
            }
        }
    }

    function trackObject(data, options, entityType) {
        ko.utils.arrayForEach(upshot.metadata.getProperties(data, entityType, !!options.includeAssociations), function (property) {
            var observable = data[property.name];
            if (ko.isObservable(observable)) {
                var tracker = function () {
                    var self = this,
                        oldValue = null,
                        createArgs = function (old, _new) {
                            var oldValues = {},
                                newValues = {};
                            oldValues[property.name] = old;
                            newValues[property.name] = _new;
                            return { oldValues: oldValues, newValues: newValues };
                        },
                        realNotifySubscribers = observable.notifySubscribers,
                        notifySubscribers = function (valueToNotify, event) {
                            var before = (event === "beforeChange"),
                                after = (event === undefined || event === "change");

                            if (before && options.beforeChange) {
                                oldValue = copy(valueToNotify);
                            }
                            if (self.skip) {
                                realNotifySubscribers.apply(this, arguments);
                            } else {
                                if (before) {
                                    if (options.beforeChange) {
                                        options.beforeChange(data, "change", createArgs(oldValue));
                                    }
                                } else if (after && options.afterChange) {
                                    options.afterChange(data, "change", createArgs(oldValue, valueToNotify));
                                }
                                realNotifySubscribers.apply(this, arguments);
                                if (after && options.afterEvent) {
                                    options.afterEvent(data, "change", createArgs(oldValue, valueToNotify));
                                }
                            }
                        };

                    this.skip = false;
                    this.dispose = function () {
                        punchKoMinified(observable, notifySubscribers, realNotifySubscribers);
                    };

                    punchKoMinified(observable, realNotifySubscribers, notifySubscribers);
                };
                upshot.cache(observable, cacheKey, new tracker());
            }
        });
    }

    function trackArray(data, options) {
        // Creates a tracker to raise before/after callbacks for the array
        var tracker = function (observable) {
            var self = this,
                oldValue = null,
                eventArgs = null,
                createArgs = function (old, _new) {
                    var args = null,
                        edits = ko.utils.compareArrays(old, _new);
                    for (var i = 0, length = edits.length; i < length; i++) {
                        var type = null;
                        switch (edits[i].status) {
                            case "added":
                                type = "insert";
                                break;
                            case "deleted":
                                type = "remove";
                                break;
                            default:
                                continue;
                        }
                        if (type !== null) {
                            if (args === null) {
                                args = { type: type, args: { index: i, items: [edits[i].value]} };
                            } else {
                                // We'll aggregate two separate edits into a single event
                                args = { type: "replaceAll", args: { newItems: _new, oldItems: old} };
                                break;
                            }
                        }
                    }
                    return eventArgs = args;
                },
                realNotifySubscribers = observable.notifySubscribers,
                notifySubscribers = function (valueToNotify, event) {
                    var before = (event === "beforeChange"),
                        after = (event === undefined || event === "change");

                    if (before) {
                        oldValue = copy(valueToNotify);
                    }
                    if (self.skip) {
                        realNotifySubscribers.apply(this, arguments);
                    } else {
                        if (before) {
                            // TODO, kylemc, What does this comment mean?
                            // do nothing; there isn't a meaningful callback we can make with our data
                        } else if (after && options.afterChange) {
                            createArgs(oldValue, valueToNotify);
                            options.afterChange(data, eventArgs.type, eventArgs.args);
                        }
                        realNotifySubscribers.apply(this, arguments);
                        if (after && options.afterEvent) {
                            options.afterEvent(data, eventArgs.type, eventArgs.args);
                        }
                    }
                };

            this.skip = false;
            this.dispose = function () {
                punchKoMinified(observable, notifySubscribers, realNotifySubscribers);
            };

            punchKoMinified(observable, realNotifySubscribers, notifySubscribers);
        };
        upshot.cache(data, cacheKey, new tracker(data));
    }

    function insert(array, index, items) {
        executeAndIgnoreChanges(array, function () {
            splice(array, index, 0, items);
        });
    }

    function remove(array, index, numToRemove) {
        executeAndIgnoreChanges(array, function () {
            splice(array, index, numToRemove);
        });
    }

    function refresh(array, newItems) {
        executeAndIgnoreChanges(array, function () {
            splice(array, 0, array().length, newItems);
        });
    }

    function isProperty(item, name) {
        if (name === stateProperty || name === errorProperty) {
            return false;
        }
        var value = item[name];
        // filter out dependent observables
        if (ko.isObservable(value) && value.hasOwnProperty("getDependenciesCount")) {
            return false;
        }
        return true;
    }

    function getProperty(item, name) {
        return ko.utils.unwrapObservable(item[name]);
    }

    function setProperty(item, name, value) {
        executeAndIgnoreChanges(item[name], function () {
            ko.isObservable(item[name]) ? item[name](value) : item[name] = value;
        });
    }

    function isArray(item) {
        return upshot.isArray(ko.utils.unwrapObservable(item));
    }

    function createCollection(initialValues) {
        return ko.observableArray(initialValues || []);
    }

    function asArray(collection) {
        return collection();
    }

    function map(item, type, mapNested, context) {
        if (upshot.isArray(item)) {
            var array;
            if (upshot.isValueArray(item)) {
                // Primitive values don't get mapped.  Avoid iteration over the potentially large array.
                // TODO: This precludes heterogeneous arrays.  Should we test for primitive element type here instead?
                array = item;
            } else {
                array = ko.utils.arrayMap(item, function (value) {
                    return (mapNested || map)(value, type);
                });
            }
            return ko.observableArray(array);
        } else if (upshot.isObject(item)) {
            var obj = context || {};
            // Often, server entities will not carry null-valued nullable-type properties.
            // Use getProperties here so that we'll map missing property value like these to observables.
            ko.utils.arrayForEach(upshot.metadata.getProperties(item, type, true), function (prop) {
                var value = (mapNested || map)(item[prop.name], prop.type);
                obj[prop.name] = ko.isObservable(value) ? value : ko.observable(value);
            });
            if (upshot.metadata.isEntityType(type)) {
                upshot.addEntityProperties(obj, type);
            }
            // addUpdatedProperties is applied shallowly. This allows map (which is applied deeply) to add the
            // properties at each level or for custom type mapping to decide whether the properties should be added
            upshot.addUpdatedProperties(obj, type);
            return obj;
        }
        return item;
    }

    function unmap(item, entityType) {
        item = ko.utils.unwrapObservable(item);
        if (upshot.isArray(item)) {
            var array = ko.utils.arrayMap(item, function (value) {
                return unmap(value);
            });
            return array;
        } else if (upshot.isObject(item)) {
            var obj = {};
            if (item.hasOwnProperty("__type")) { // make sure __type flows through first
                obj["__type"] = ko.utils.unwrapObservable(item["__type"]);
            }
            ko.utils.arrayForEach(upshot.metadata.getProperties(item, entityType), function (prop) {
                // TODO: determine if there are scenarios where we want to support hasOwnProperty returning false
                if (item.hasOwnProperty(prop.name)) {
                    obj[prop.name] = unmap(item[prop.name], prop.type);
                }
            });
            return obj;
        }
        return item;
    }

    function setContextProperty(item, kind, name, value) {
        if (kind === "entity") {
            // TODO -- do we want these 'reserved' properties to be configurable?
            var prop;
            if (name === "state") {
                // set item.EntityState
                prop = stateProperty;
            } else if (name === "error") {
                // set item.EntityError
                prop = errorProperty;
            }
            if (ko.isObservable(item[prop])) {
                setProperty(item, prop, value);
            }
        } else if (kind === "property") {
            // set item.name.IsUpdated
            if (ko.isObservable(item[name])) {
                var observable = item[name][updatedProperty];
                observable && observable(value);
            }
        }
    }

    var observability = upshot.defineNamespace("upshot.observability");

    observability.knockout = {
        track: track,

        insert: insert,
        remove: remove,
        refresh: refresh,

        isProperty: isProperty,
        getProperty: getProperty,
        setProperty: setProperty,

        isArray: isArray,
        createCollection: createCollection,
        asArray: asArray,

        map: map,
        unmap: unmap,

        setContextProperty: setContextProperty
    };

    observability.configuration = observability.knockout;

    // KO entity extensions
    function addValidationErrorProperty(entity, prop) {
        var observable = entity[prop];
        if (observable) {  // Custom mappings might not include this property.
            observable[validationErrorProperty] = ko.computed(function () {
                var allErrors = entity[errorProperty]();
                if (allErrors && allErrors.ValidationErrors) {
                    var matchingError = ko.utils.arrayFirst(allErrors.ValidationErrors, function (error) {
                        return ko.utils.arrayIndexOf(error.SourceMemberNames, prop) >= 0;
                    });
                    if (matchingError) {
                        return matchingError.Message;
                    }
                }
                return null;
            });
        }
    }

    upshot.addEntityProperties = function (entity, entityType) {
        // Adds properties to an entity that will be managed by upshot
        entity[stateProperty] = ko.observable(upshot.EntityState.Unmodified);
        entity[errorProperty] = ko.observable();

        // Minimize view boilerplate by exposing state flags
        var containsState = function (states) {
            return ko.utils.arrayIndexOf(states, entity[stateProperty]()) !== -1;
        };
        var es = upshot.EntityState;
        entity[updatedProperty] = ko.computed(function () {
            return containsState([es.ClientUpdated, es.ServerUpdating]);
        });
        entity[addedProperty] = ko.computed(function () {
            return containsState([es.ClientAdded, es.ServerAdding]);
        });
        entity[deletedProperty] = ko.computed(function () {
            return containsState([es.ClientDeleted, es.ServerDeleting, es.Deleted]);
        });
        entity[changedProperty] = ko.computed(function () {
            return !containsState([es.Unmodified, es.Deleted]);
        });
        entity[canDeleteProperty] = ko.computed(function () {
            return !(entity[addedProperty]() || entity[deletedProperty]());
        });

        // TODO -- these are only applied a single level deep; see if there's a consistent way to apply CTs
        ko.utils.arrayForEach(upshot.metadata.getProperties(entity, entityType, true), function (prop) {
            addValidationErrorProperty(entity, prop.name);
        });
    }

    upshot.addUpdatedProperties = function (obj, type) {
        ko.utils.arrayForEach(upshot.metadata.getProperties(obj, type, false), function (prop) {
            var observable = obj[prop.name];
            if (ko.isObservable(observable)) {
                observable[updatedProperty] = ko.observable(false);
            }
        });
    }
}
///#RESTORE )(this, ko, upshot);