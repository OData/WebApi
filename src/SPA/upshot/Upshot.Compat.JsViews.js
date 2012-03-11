/// <reference path="IntelliSense\References.js" />
///#RESTORE (function (global, $, upshot, undefined)
{
    function track(data, options) {
        if (!options) {
            $.observable.track(data, null);
        } else {
            $.observable.track(data, {
                beforeChange: options.beforeChange && wrapCallback(options.beforeChange),
                afterChange: options.afterChange && wrapCallback(options.afterChange),
                afterEvent: options.afterEvent && wrapCallback(options.afterEvent)
            });
        }
    }

    // transform JsViews to upshot eventArg style
    function wrapCallback(callback) {
        return function ($target, type, data) {
            if (type == "propertyChange") {
                var eventArg = { newValues: {}, oldValues: {} };
                eventArg.newValues[data.path] = data.value;
                eventArg.oldValues[data.path] = $target[data.path];
                return callback.call(this, $target, "change", eventArg);
            } else if (type == "arrayChange") {
                switch (data.change) {
                    case "insert":
                        return callback.call(this, $target, "insert", { index: data.index, items: data.items });
                    case "remove":
                        return callback.call(this, $target, "remove", { index: data.index, items: data.items });
                    case "refresh":
                        return callback.call(this, $target, "replaceAll", { newItems: data.newItems, oldItems: data.oldItems });
                }
            }
            throw "NYI - event '" + type + "' and '" + data.change + "' is not supported!";
        };
    }

    function insert(array, index, items) {
        array.splice.apply(array, [index, 0].concat(items));
        var eventArguments = {
            change: "insert",
            index: index,
            items: items
        };
        $([array]).triggerHandler("arrayChange", eventArguments);
    }

    function remove(array, index, numToRemove) {
        var itemsRemoved = array.slice(index, index + numToRemove);
        array.splice(index, numToRemove);
        var eventArguments = {
            change: "remove",
            index: index,
            items: itemsRemoved
        };
        $([array]).triggerHandler("arrayChange", eventArguments);
    }

    function refresh(array, newItems) {
        var oldItems = array.slice(0);
        array.splice.apply(array, [0, array.length].concat(newItems));
        var eventArguments = {
            change: "refresh",
            oldItems: oldItems,
            newItems: newItems
        };
        $([array]).triggerHandler("arrayChange", eventArguments);
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
        var eventArguments = {
            path: name,
            value: value
        };
        $(item).triggerHandler("propertyChange", eventArguments);
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

    observability.jsviews = {
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

    observability.configuration = observability.jsviews;

}
///#RESTORE )(this, jQuery, upshot);

