/// <reference path="IntelliSense\References.js" />
///#RESTORE (function (global, $, WinJS, upshot, undefined)
{
    // Customizations to WinJS observability to interop with Upshot.

    var ObservableProxy = WinJS.Binding.as({}).constructor;
    var base = ObservableProxy.prototype;
    var RiaObservableProxy = WinJS.Class.derive(base, function (data, beforeChange, afterChange, afterEvent) {
        base.constructor.call(this, data);
        this._beforeChange = beforeChange;
        this._afterChange = afterChange;
        this._afterEvent = afterEvent;
    }, {
        updateProperty: function (name, value) {
            var oldValue = this._backingData[name];
            var newValue = WinJS.Binding.unwrap(value);
            if (oldValue !== newValue) {
                var eventArguments = { path: name, value: value };

                if (this._beforeChange) {
                    this._beforeChange(this._backingData, "propertyChange", eventArguments);
                }

                this._backingData[name] = newValue;

                if (this._afterChange) {
                    this._afterChange(this._backingData, "propertyChange", eventArguments);
                }

                var notifyPromise = this._notifyListeners(name, newValue, oldValue),
                     afterEvent = this._afterEvent;
                if (afterEvent) {
                    return notifyPromise.then(function () {
                        afterEvent(this._backingData, "propertyChange", eventArguments);
                    });  // TODO: Implement "cancel" too, as that's what we'll get when events are coalesced.
                } else {
                    return notifyPromise;
                }
            }
            return WinJS.Promise.as();
        }
    });

    function track(data, options) {
        if (!options) {
            delete data._getObservable;
        } else {
            if ($.isArray(data)) {
                // TODO: Integrate WinJS array observability when it exists.  We assume jQuery observability for arrays until then.
                $.observable.track(data, {
                    beforeChange: options.beforeChange,
                    afterChange: options.afterChange,
                    afterEvent: options.afterEvent
                });
            } else {
                if ($.inArray("_getObservable", Object.getOwnPropertyNames(data)) >= 0) {
                    throw "Entities added via Upshot/JS must not previously have been treated with WinJS.Binding.as";
                }

                var observable = new RiaObservableProxy(data, options.beforeChange, options.afterChange, options.afterEvent);
                observable.backingData = data;
                Object.defineProperty(data, "_getObservable", {
                    value: function () { return observable; },
                    enumerable: false,
                    writable: false
                });
            }
        }
    }

    // TODO: Integrate WinJS array observability when it exists.  We assume jQuery observability for arrays until then.
    function createInsertDeferredEvent(array, index, items) {
        return function () {
            var eventArguments = {
                change: "insert",
                index: index,
                items: items
            };
            $([array]).triggerHandler("arrayChange", eventArguments);
        };
    }

    // TODO: Integrate WinJS array observability when it exists.  We assume jQuery observability for arrays until then.
    function createRemoveDeferredEvent(array, index, itemsRemoved) {
        return function () {
            var eventArguments = {
                change: "remove",
                index: index,
                items: itemsRemoved
            };
            $([array]).triggerHandler("arrayChange", eventArguments);
        };
    }

    // TODO: Integrate WinJS array observability when it exists.  We assume jQuery observability for arrays until then.
    function createRefreshDeferredEvent(array, oldItems, newItems) {
        return function () {
            var eventArguments = {
                change: "refresh",
                oldItems: oldItems,
                newItems: newItems
            };
            $([array]).triggerHandler("arrayChange", eventArguments);
        };
    }

    function createSetPropertyDeferredEvent(item, name, oldValue, newValue) {
        var observable = WinJS.Binding.as(item);
        return function () {
            observable._notifyListeners(name, newValue, oldValue);
        };
    }

    var observability = upshot.defineNamespace("upshot.observability");

    observability.winjs = {
        track: track,

        createInsertDeferredEvent: createInsertDeferredEvent,
        createRemoveDeferredEvent: createRemoveDeferredEvent,
        createRefreshDeferredEvent: createRefreshDeferredEvent,
        createSetPropertyDeferredEvent: createSetPropertyDeferredEvent
    };

    observability.configuration = observability.winjs;

    // WinJS DataSource support follows.

    var DataAdaptor = WinJS.Class.define(function (riaDataSource) {
        var dataSource;
        if (riaDataSource.applyLocalQuery) {
            dataSource = $.dataSource.unwrapHack(riaDataSource.getEntities());
        } else {
            dataSource = riaDataSource;
        }

        this._dataSource = dataSource;
        this._dataSourceObserver = null;
        this._arrayChangeHandler = null;

        this.compareByIdentity = true;
    }, {
        dispose: function () {
            if (this._dataSource) {
                this._dataSource.removeObserver(this._dataSourceObserver);
                $([this._dataSource.getEntities()]).unbind("arrayChange", this._arrayChangeHandler);
                this._dataSource = null;
            }
        },

        setNotificationHandler: function (notificationHandler) {
            var that = this;

            function handleArrayChange(event, eventArgs) {
                var index;
                notificationHandler.beginNotifications();
                switch (eventArgs.change) {
                    case "refresh":
                        notificationHandler.invalidateAll();
                        break;

                    case "insert":
                        var entities = that._dataSource.getEntities(),
                            previousItem = index === 0 ? null : entities[index - 1],
                            nextItem = index === entities.length - 1 ? null : entities[index + 1];

                        index = eventArgs.index;
                        $.each(eventArgs.items, function () {
                            var item = { key: that._getEntityKey(this), data: this };
                            notificationHandler.inserted(item, previousItem && that._getEntityKey(previousItem), nextItem && that._getEntityKey(nextItem), index);
                            previousItem = this;
                            index++;
                        });
                        break;

                    case "remove":
                        index = eventArgs.index;
                        $.each(eventArgs.items, function () {
                            notificationHandler.removed(that._getEntityKey(this), index);
                        });
                        break;

                    case "move":  // TODO: Our data sources never issue move events presently, but this could still be implemented.
                    default:
                        throw "Unexpected array changed event.";
                }
                notificationHandler.endNotifications();
            }

            this._dataSourceObserver = {
                propertyChanged: function (entity, property, value) {
                    notificationHandler.changed({ key: that._getEntityKey(entity), data: entity });
                }
            };
            this._dataSource.addObserver(this._dataSourceObserver);

            // TODO: We should have a platform-neutral way of communicating array changes, so we
            // don't have a hard dependency on a single observability pattern.
            this._arrayChangeHandler = handleArrayChange;
            $([this._dataSource.getEntities()]).bind("arrayChange", handleArrayChange);
        },

        // compareByIdentity: set in constructor
        // itemsFromStart: not implemented
        // itemsFromEnd: not implemented
        // itemsFromKey: not implemented

        itemsFromIndex: function (index, countBefore, countAfter) {
            var skip = index - countBefore,
            take = countBefore + 1 + countAfter,
            allEntities = this._dataSource.getEntities(),
            entities = allEntities.slice(skip, skip + take),
            that = this;
            return WinJS.Promise.wrap({
                items: $.map(entities, function (entity) {
                    return { key: that._getEntityKey(entity), data: entity };
                }),
                offset: countBefore,
                totalCount: allEntities.length,
                absoluteIndex: index
                // TODO: atEnd?
            });
        },

        // itemsFromDescription: not implemented

        getCount: function () {
            return WinJS.Promise.wrap(this._dataSource.getEntities().length);
        },

        // NOTE: We don't implement these, as there is an edit model inherent in the
        // Upshot data source passed into this adaptor.  This adaptor component becomes much
        // more complex if we allow for editing over both WinJS's and Upshot's data sources.
        // 
        // insertAtStart: function (key, data) {
        // insertBefore: function (key, data, nextKey, nextIndexHint) {
        // insertAfter: function (key, data, previousKey, previousIndexHint) {
        // insertAtEnd: function (key, data) {
        // change: function (key, newData, indexHint) {
        // moveToStart: not implemented
        // moveBefore: not implemented
        // moveAfter: not implemented
        // moveToEnd: not implemented
        // remove: not implemented

        _getEntityKey: function (entity) {
            return this._dataSource.getEntityId(entity);
        }
    });

    var VirtualizingDataAdaptor = WinJS.Class.define(function (options) {

        this._dataContext = options.dataContext;
        if (!this._dataContext) {
            var implicitCommitHandler;
            if (!options.bufferChanges) {
                // since we're not change tracking, define an implicit commit callback
                // and pass into the DC
                var self = this;
                implicitCommitHandler = function () {
                    self._dataContext._commitChanges({ providerParameters: self._providerParameters });
                }
            }
            this._dataContext = new upshot.DataContext(new upshot.riaDataProvider(), implicitCommitHandler);
        }
        this._providerParameters = options.providerParameters;
        this._entityType = options.entityType;

        this._lastTotalCount = null;
        this._notificationHandler = null;
        this._entitySet = null;
        this._entitySetObserver = null;
        this._arrayChangeHandler = null;

        this._sort = null;
        this._filters = null;

        // TODO: Ick!
        this._setFilter = upshot.RemoteDataSource.prototype.setFilter;
        this._processFilter = upshot.DataSource.prototype._processFilter;

        this.compareByIdentity = true;  // TODO: How do this control list rerender vs. div surgery?
    }, {
        entitySet: {
            get: function () {
                return this._entitySet;
            }
        },

        filter: {
            set: function (filter) {
                this._setFilter(filter);
            }
        },

        sort: {
            set: function (sort) {
                this._sort = sort;
            }
        },

        refresh: function () {
            this._notificationHandler.invalidateAll();
        },

        dispose: function () {
            if (this._entitySet) {
                this._entitySet.removeObserver(this._entitySetObserver);
                $([this._entitySet.getEntities()]).unbind("arrayChange", this._arrayChangeHandler);
                this._entitySet = null;
            }
        },

        setNotificationHandler: function (notificationHandler) {
            this._notificationHandler = notificationHandler;
        },

        // compareByIdentity: set in constructor
        // itemsFromStart: not implemented
        // itemsFromEnd: not implemented
        // itemsFromKey: not implemented

        itemsFromIndex: function (index, countBefore, countAfter) {
            var that = this;
            return new WinJS.Promise(function (complete, error) {
                var skip = index - countBefore,
                take = countBefore + 1 + countAfter,
                success = function (entities) {
                    var result = {
                        items: $.map(entities, function (entity) {
                            return { key: that._getEntityKey(entity), data: entity };
                        }),
                        offset: countBefore,
                        totalCount: that._lastTotalCount,
                        absoluteIndex: index
                        // TODO: atEnd?
                    };
                    complete(result);
                };
                that._loadEntities(skip, take, success);
            });
        },

        // itemsFromDescription: not implemented

        getCount: function () {
            if (this._lastTotalCount !== null) {
                return WinJS.Promise.wrap(this._lastTotalCount);
            } else {
                var that = this;
                return new WinJS.Promise(function (complete, error) {
                    // Ask for zero entities here.  We merely want the total count from the server.
                    that._loadEntities(null, 0, function () {
                        complete(that._lastTotalCount);
                    });
                });
            }
        },

        // insertAtStart: function (key, data) {
        // insertBefore: function (key, data, nextKey, nextIndexHint) {
        // insertAfter: function (key, data, previousKey, previousIndexHint) {
        // insertAtEnd: function (key, data) {
        // change: function (key, newData, indexHint) {
        // moveToStart: not implemented
        // moveBefore: not implemented
        // moveAfter: not implemented
        // moveToEnd: not implemented
        // remove: not implemented

        _loadEntities: function (skip, take, success, fail) {
            var that = this,
                loadSucceeded = function (entitySet, entities, totalCount) {
                    that._bindToEntitySet(entitySet);
                    that._lastTotalCount = totalCount;  // TODO: Is there a way to event to the ListDataSource that the count has changed?  Is it invalidateAll?
                    if (success) {
                        success.call(that, entities);
                    }
                },
                loadFailed = function (statusText, error) {
                    if (fail) {
                        fail.call(that, statusText, error);
                    }
                };

            this._dataContext.__load({
                providerParameters: this._providerParameters,
                entityType: this._entityType,

                queryParameters: {
                    filters: this._filters,
                    sort: this._sort,
                    skip: skip,
                    take: take,
                    includeTotalCount: true
                }
            }, loadSucceeded, loadFailed);
        },

        _bindToEntitySet: function (entitySet) {
            if (this._entitySet) {
                return;
            }

            this._entitySet = entitySet;

            var that = this;

            function handleArrayChange(event, eventArgs) {
                that._notificationHandler.beginNotifications();
                switch (eventArgs.change) {
                    case "insert":
                        // Such inserts are from other queries returning entities of this type
                        // or they are internal inserts that might not even be committed yet.
                        // The app should explicitly refresh their ListView here.
                        break;

                    case "remove":
                        $.each(eventArgs.items, function () {
                            that._notificationHandler.removed(that._getEntityKey(this));
                        });
                        break;

                    case "move":  // TODO: Our entity sets don't issue move events presently.
                    case "refresh":  // TODO: Our entity sets don't issue refresh events presently.
                    default:
                        throw "Unexpected array changed event.";
                        break;
                }
                that._notificationHandler.endNotifications();
            }

            this._entitySetObserver = {
                propertyChanged: function (entity, property, value) {
                    that._notificationHandler.changed({ key: that._getEntityKey(entity), data: entity });
                }
            };
            entitySet.addObserver(this._entitySetObserver);

            // TODO: We should have a platform-neutral way of communicating array changes, so we
            // don't have a hard dependency on a single observability pattern.
            this._arrayChangeHandler = handleArrayChange;
            $([entitySet.getEntities()]).bind("arrayChange", handleArrayChange);
        },

        _getEntityKey: function (entity) {
            return this._entitySet.getEntityId(entity);
        }

        // TODO: Investigate why the ListView stops loading if you drag to the right too aggressive.
    });

    WinJS.Namespace.define("upshot.WinJS", {
        DataSource: function (riaDataSource) {
            var dataAdaptor = new DataAdaptor(riaDataSource),
            dataSource = new WinJS.UI.ListDataSource(dataAdaptor);

            dataSource.dispose = function () {
                dataAdaptor.dispose();
            };

            return dataSource;
        },
        VirtualizingDataSource: function (options) {
            var dataAdaptor = new VirtualizingDataAdaptor(options),
            dataSource = new WinJS.UI.ListDataSource(dataAdaptor);

            Object.defineProperty(dataSource, "filter", {
                set: function (filter) {
                    dataAdaptor.filter = filter;
                }
            });
            Object.defineProperty(dataSource, "sort", {
                set: function (sort) {
                    dataAdaptor.sort = sort;
                }
            });
            Object.defineProperty(dataSource, "entitySet", {
                get: function () {
                    return dataAdaptor.entitySet;
                }
            });
            dataSource.refresh = function () {
                dataAdaptor.refresh();
            };
            dataSource.dispose = function () {
                dataAdaptor.dispose();
            };

            return dataSource;
        }
    });

}
///#RESTORE )(this, jQuery, WinJS, upshot);
