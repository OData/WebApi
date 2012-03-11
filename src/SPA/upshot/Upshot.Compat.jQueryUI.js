/// <reference path="IntelliSense\References.js" />
///#RESTORE (function (global, $, upshot, undefined)
{

    var cachedObservableKey = "__cachedObservable__",
        observable = $.observable.Observable;

    $.observable = function (data) {
        return upshot.cache(data, cachedObservableKey) || new observable(data);
    };

    var base = observable.prototype;

    var UpshotObservable = upshot.deriveClass(base, function (data, beforeChange, afterChange, afterEvent) {
        observable.call(this, data);
        this.beforeChange = beforeChange;
        this.afterChange = afterChange;
        this.afterEvent = afterEvent;
    }, {
        _property: function (oldValues, newValues) {
            if (this.beforeChange) {
                this.beforeChange(this.data, "change", { oldValues: oldValues, newValues: newValues });
            }

            return base._property.apply(this, arguments);
        },

        _insert: function (index, items) {
            if (this.beforeChange) {
                this.beforeChange(this.data, "insert", { index: index, items: items });
            }

            base._insert.apply(this, arguments);
        },

        _remove: function (index, numToRemove) {
            if (this.beforeChange) {
                var items = this.data.slice(index, index + numToRemove);
                this.beforeChange(this.data, "remove", { index: index, items: items });
            }

            base._remove.apply(this, arguments);
        },

        replaceAll: function (newItems) {
            if (this.beforeChange) {
                this.beforeChange(this.data, "replaceAll", { oldItems: this.data.slice(0), newItems: newItems });
            }

            base.replaceAll.apply(this, arguments);
        },

        _trigger: function (type, eventData) {
            if (this.afterChange) {
                this.afterChange(this.data, type, eventData);
            }

            base._trigger.apply(this, arguments);

            if (this.afterEvent) {
                this.afterEvent(this.data, type, eventData);
            }
        }

    });

    // TODO, kylemc, Knockout compatibility uses an "entityType" parameter here and tracks only metadata-specified properties.
    function track(data, options) {
        if (!options) {
            upshot.deleteCache(data, cachedObservableKey);
        } else {
            upshot.cache(data, cachedObservableKey, new UpshotObservable(data, options.beforeChange, options.afterChange, options.afterEvent));
        }
    }

    function insert(array, index, items) {
        array.splice.apply(array, [index, 0].concat(items));
        var eventArguments = {
            index: index,
            items: items
        };
        $([array]).triggerHandler("insert", eventArguments);
    }

    function remove(array, index, numToRemove) {
        var itemsRemoved = array.slice(index, index + numToRemove);
        array.splice(index, numToRemove);
        var eventArguments = {
            index: index,
            items: itemsRemoved
        };
        $([array]).triggerHandler("remove", eventArguments);
    }

    function refresh(array, newItems) {
        var oldItems = array.slice(0);
        array.splice.apply(array, [0, array.length].concat(newItems));
        var eventArguments = {
            oldItems: oldItems,
            newItems: newItems
        };
        $([array]).triggerHandler("replaceAll", eventArguments);
    }

    function isProperty(item, name) {
        return !$.isFunction(item[name]);
    }

    function getProperty(item, name) {
        return item[name];
    }

    function setProperty(item, name, value) {
        var oldValue = item[name];
        item[name] = value;

        var oldValues = {},
        newValues = {};
        oldValues[name] = oldValue;
        newValues[name] = value;
        var eventArguments = {
            oldValues: oldValues,
            newValues: newValues
        };
        $(item).triggerHandler("change", eventArguments);
    }

    function isArray(item) {
        return upshot.isArray(item);
    }

    function createCollection(initialValues) {
        return initialValues || [];
    }

    function asArray(collection) {
        return collection;
    }

    function map(item) {
        return item;
    }

    function unmap(item) {
        return item;
    }

    var observability = upshot.defineNamespace("upshot.observability");

    observability.jquery = {
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

        setContextProperty: $.noop
    };

    observability.configuration = observability.jquery;

}
///#RESTORE )(this, jQuery, upshot);

