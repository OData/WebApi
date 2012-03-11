/// <reference path="IntelliSense\References.js" />
///#RESTORE (function (global, undefined)
{

    function extend(target, members) {
        for (var member in members) {
            target[member] = members[member];
        }
        return target;
    }

    function defineNamespace(name) {
        var names = name.split(".");
        var current = global;
        for (var i = 0; i < names.length; i++) {
            var ns = current[names[i]];
            if (!ns || typeof ns !== "object") {
                current[names[i]] = ns = {};
            }
            current = ns;
        }
        return current;
    }

    function defineClass(ctor, instanceMembers, classMembers) {
        ctor = ctor || function () { };
        if (instanceMembers) {
            extend(ctor.prototype, instanceMembers);
        }
        if (classMembers) {
            extend(ctor, classMembers);
        }
        return ctor;
    }

    function deriveClass(basePrototype, ctor, instanceMembers) {
        var prototype = {};
        extend(prototype, basePrototype);
        extend(prototype, instanceMembers);  // Will override like-named members on basePrototype.

        ctor = ctor || function () { };
        ctor.prototype = prototype;
        ctor.prototype.constructor = ctor;
        return ctor;
    }

    function classof(o) {
        if (o === null) {
            return "null";
        }
        if (o === undefined) {
            return "undefined";
        }
        return Object.prototype.toString.call(o).slice(8, -1).toLowerCase();
    }

    function isArray(o) {
        return classof(o) === "array";
    }

    function isObject(o) {
        return classof(o) === "object";
    }

    function isValueArray(o) {
        return isArray(o) && (o.length === 0 || !(isArray(o[0]) || isObject(o[0])));
    }

    function isDate(o) {
        return classof(o) === "date";
    }

    function isFunction(o) {
        return classof(o) === "function";
    }

    function isGuid(value) {
        return (typeof value === "string") && /[a-fA-F\d]{8}-(?:[a-fA-F\d]{4}-){3}[a-fA-F\d]{12}/.test(value);
    }

    var hasOwnProperty = Object.prototype.hasOwnProperty;
    function isEmpty(obj) {
        if (obj === null || obj === undefined) {
            return true;
        }
        for (var key in obj) {
            if (hasOwnProperty.call(obj, key)) {
                return false;
            }
        }
        return true;
    }

    var idCounter = 0;
    function uniqueId(prefix) {
        /// <summary>Generates a unique id (unique within the entire client session)</summary>
        /// <param name="prefix" type="String">Optional prefix to the id</param>
        /// <returns type="String" />
        prefix || (prefix = "");
        return prefix + idCounter++;
    }

    function cache(object, key, value) {
        if (!object) {
            return;
        }
        if (arguments.length === 2) {
            // read
            var cacheName = upshot.cacheName;
            if (cacheName && object[cacheName]) {
                return object[cacheName][key];
            }
            return null;
        } else {
            // write
            if (object.nodeType !== undefined) {
                throw "upshot.cache cannot be used with DOM elements";
            }
            var cacheName = upshot.cacheName || (upshot.cacheName = uniqueId("__upshot__"));
            object[cacheName] || (object[cacheName] = function () { });
            return object[cacheName][key] = value;
        }
    }

    function deleteCache(object, key) {
        var cacheName = upshot.cacheName;
        if (cacheName && object && object[cacheName]) {
            if (key) {
                delete object[cacheName][key];
            }
            if (!key || isEmpty(object[cacheName])) {
                delete object[cacheName];
            }
        }
    }

    function sameArrayContents(array1, array2) {
        if (array1.length !== array2.length) {
            return false;
        } else {
            for (var i = 0; i < array1.length; i++) {
                if (array1[i] !== array2[i]) {
                    return false;
                }
            }
        }
        return true;
    }

    // This routine provides an equivalent of array.push(item) missing from JavaScript array.
    function arrayRemove(array, item) {
        var callback = upshot.isFunction(item) ? item : undefined;
        for (var index = 0; index < array.length; index++) {
            if (callback ? callback(array[index]) : (array[index] === item)) {
                array.splice(index, 1);
                return index;
            }
        }
        return -1;
    }

    // pre-defined ns
    ///#RESTORE var upshot = defineNamespace("upshot");

    // pre-defined routines
    upshot.extend = extend;
    upshot.defineNamespace = defineNamespace;
    upshot.defineClass = defineClass;
    upshot.deriveClass = deriveClass;
    upshot.classof = classof;
    upshot.isArray = isArray;
    upshot.isObject = isObject;
    upshot.isValueArray = isValueArray;
    upshot.isDate = isDate;
    upshot.isFunction = isFunction;
    upshot.isGuid = isGuid;
    upshot.isEmpty = isEmpty;
    upshot.uniqueId = uniqueId;
    upshot.cacheName = null;
    upshot.cache = cache;
    upshot.deleteCache = deleteCache;
    upshot.sameArrayContents = sameArrayContents;
    upshot.arrayRemove = arrayRemove;

    upshot.EntityState = {
        Unmodified: "Unmodified",
        ClientUpdated: "ClientUpdated",
        ClientAdded: "ClientAdded",
        ClientDeleted: "ClientDeleted",
        ServerUpdating: "ServerUpdating",
        ServerAdding: "ServerAdding",
        ServerDeleting: "ServerDeleting",
        Deleted: "Deleted",

        isClientModified: function (entityState) {
            return entityState && entityState.indexOf("Client") === 0;
        },
        isServerSyncing: function (entityState) {
            return entityState && entityState.indexOf("Server") === 0;
        },
        isUpdated: function (entityState) {
            return entityState && entityState.indexOf("Updat") > 0;
        },
        isDeleted: function (entityState) {
            return entityState && entityState.indexOf("Delet") > 0;
        },
        isAdded: function (entityState) {
            return entityState && entityState.indexOf("Add") > 0;
        }
    };

    ///#DEBUG
    upshot.assert = function (cond, msg) {
        if (!cond) {
            alert(msg || "assert is encountered!");
        }
    }
    ///#ENDDEBUG

    var entitySources = [];
    function registerRootEntitySource (entitySource) {
        entitySources.push(entitySource);
    }

    function deregisterRootEntitySource (entitySource) {
        entitySources.splice($.inArray(entitySource, entitySources), 1);
    }

    var recomputeInProgress;
    function triggerRecompute () {
        if (recomputeInProgress) {
            throw "Cannot make observable edits from within an event callback.";
        }

        try {
            recomputeInProgress = true;

            var sources = entitySources.slice();
            $.each(sources, function (index, source) {
                source.__recomputeDependentViews();
            });

            $.each(sources, function (index, source) {
                if (source.__flushEntityStateChangedEvents) {
                    source.__flushEntityStateChangedEvents();
                }
            });
        }
        finally {
            recomputeInProgress = false;
        }
    }

    function beginChange () {
        if (recomputeInProgress) {
            throw "Cannot make observable edits from within an event callback.";
        }
    }

    function endChange () {
        triggerRecompute();
    }

    upshot.__registerRootEntitySource = registerRootEntitySource;
    upshot.__deregisterRootEntitySource = deregisterRootEntitySource;
    upshot.__triggerRecompute = triggerRecompute;
    upshot.__beginChange = beginChange;
    upshot.__endChange = endChange;
}
///#RESTORE )(this);
