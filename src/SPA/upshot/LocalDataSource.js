/// <reference path="IntelliSense\References.js" />
///#RESTORE (function (global, $, upshot, undefined)
{
    var base = upshot.DataSource.prototype;

    var obs = upshot.observability;

    var ctor = function (options) {
        /// <summary>
        /// LocalDataSource is used to load model data matching a query that is evaluated in-memory.
        /// </summary>
        /// <param name="options" optional="true">
        /// Options used in the construction of the LocalDataSource:
        ///     &#10;source: The input data which is to be sorted, filtered and paged to produce the output for this LocalDataSource.  Can be supplied as some instance deriving from EntitySource or as an array from some EntitySource.getEntities().
        ///     &#10;autoRefresh: (Optional) Instructs the LocalDataSource to implicitly reevaluate its query in response to edits to input data.  Otherwise, LocalDataSource.refresh() must be used to reevaluate the query against modified input data.
        /// </param>

        // support no new ctor
        if (this._trigger === undefined) {
            return new upshot.LocalDataSource(options);
        }

        var input = options && options.source;
        if (input && !input.__recomputeDependentViews) {  // Test if "input" is an EntitySource.
            var entitySource = obs.isArray(input) && upshot.EntitySource.as(input);
            if (!entitySource) {
                throw "Input data for a LocalDataSource must be an EntitySource or the array returned by EntitySource.getEntities().";
            }

            options.source = entitySource;

            // TODO -- If this is an array that isn't already the output of an EntitySource, 
            // engage the compatibility layer to wrap the raw array in an EntitySource.
            // Such an EntitySource would upshot.__registerRootEntitySource and would turn
            // observable CUD into Upshot "change" and "arrayChange" events.
        }

        this._autoRefresh = options && options.autoRefresh;  // TODO -- Should we make "auto refresh" a feature of RemoteDataSource too?

        // Optional query options
        this._sort = null;
        this._filter = null;

        // State
        this._refreshAllInProgress = false;

        base.constructor.call(this, options);

        // Events specific to LocalDataSource
        this._bindFromOptions(options, [ "refreshNeeded" ]);

        if (this._autoRefresh) {
            this.refresh();
        }
    };

    var instanceMembers = {

        // override "sort" option setter
        setSort: function (sort) {
            /// <summary>
            /// Establishes the sort specification that is to be applied to the input data.
            /// </summary>
            /// <param name="sort">
            /// &#10;The sort specification to applied when loading model data.
            /// &#10;Should be supplied as an object of the form &#123; property: &#60;propertyName&#62; [, descending: &#60;bool&#62; ] &#125; or an array of ordered objects of this form.
            /// &#10;When supplied as null or undefined, the sort specification for this LocalDataSource is cleared.
            /// </param>
            /// <returns type="upshot.LocalDataSource"/>

            // TODO -- Should really raise "need refresh" event when changed (throughout).
            // TODO -- Validate sort specification?
            this._sort = sort;
            return this;
        },

        // override "filter" option setter
        setFilter: function (filter) {
            /// <summary>
            /// Establishes the filter specification that is to be applied to the input data.
            /// </summary>
            /// <param name="filter">
            /// &#10;The filter specification to applied when loading model data.
            /// &#10;Should be supplied as an object of the form &#123; property: &#60;propertyName&#62;, value: &#60;propertyValue&#62; [, operator: &#60;operator&#62; ] &#125; or a function(entity) returning Boolean or an ordered array of these forms.
            /// &#10;When supplied as null or undefined, the filter specification for this LocalDataSource is cleared.
            /// </param>
            /// <returns type="upshot.LocalDataSource"/>

            // TODO -- Should really raise "need refresh" event when changed (throughout).
            this._filter = filter && this._createFilterFunction(filter);
        },

        refresh: function (options, success, fail) {
            /// <summary>
            /// Initiates an asynchronous reevaluation of the query established with setSort, setFilter and setPaging.
            /// </summary>
            /// <param name="options" type="Object" optional="true">
            /// &#10;If supplied, an object with an "all" property, indicating that the input DataSource is to be refreshed prior to reevaluating this LocalDataSource query.
            /// </param>
            /// <param name="success" type="Function" optional="true">
            /// &#10;A success callback with signature function(entities, totalCount).
            /// </param>
            /// <param name="error" type="Function" optional="true">
            /// &#10;An error callback with signature function(httpStatus, errorText, context).
            /// </param>
            /// <returns type="upshot.LocalDataSource"/>

            this._verifyOkToRefresh();

            if ($.isFunction(options)) {
                fail = success;
                success = options;
                options = undefined;
            }

            this._trigger("refreshStart");

            var self = this,
                sourceIsDataSource = this._entitySource.refresh;  // Only DataSources will have "refresh" (and not EntitySet and AssociatedEntitiesView).
            if (options && !!options.all && sourceIsDataSource) {
                // N.B.  "all" is a helper, in the sense that it saves a client from doing a serverDataSource.refresh and then,
                // in response to serverDataSource.onRefresh, calling localDataSource.refresh.  Also, it allows the app to listen
                // on refreshStart/refresh events from this LDS alone (and not the inner SDS as well).
                this._refreshAllInProgress = true;
                this._entitySource.refresh({ all: true }, function (entities) {
                    completeRefresh(entities);
                    self._refreshAllInProgress = false;
                }, function (httpStatus, errorText, context) {
                    self._failRefresh(httpStatus, errorText, context, fail);
                });
            } else {
                // We do this refresh asynchronously so that, if this refresh was called during a callback,
                // the app receives remaining callbacks first, before the new batch of callbacks with respect to this refresh.
                // TODO -- We should only refresh once in response to N>1 "refresh" calls.
                setTimeout(function () { completeRefresh(obs.asArray(self._entitySource.getEntities())); });
            }

            return this;

            function completeRefresh(entities) {
                self._needRecompute = false;

                var results = self._applyQuery(entities);
                self._completeRefresh(results.entities, results.totalCount, success);
            };
        },

        refreshNeeded: function () {
            /// <summary>
            /// Indicates whether the input data has been modified in such a way that LocalDataSource.getEntities() would change on the next LocalDataSource.refresh() call.
            /// </summary>
            /// <returns type="Boolean"/>

            return this._needRecompute;
        },

        // Private methods

        _setNeedRecompute: function () {
            ///#DEBUG
            upshot.assert(!this._needRecompute);  // Callers should only determine dirtiness if we're not already dirty.
            ///#ENDDEBUG

            if (this._autoRefresh) {
                // Recompute our result entities according to our regular recompute cycle,
                // just as AssociatedEntitiesView does in response to changes in its
                // target EntitySet.
                base._setNeedRecompute.call(this);
            } else {
                // Explicit use of "refresh" was requested here.  Reuse (or abuse) the _needRecompute
                // flag to track our dirtiness.
                this._needRecompute = true;
                this._trigger("refreshNeeded");
            }
        },

        _recompute: function () {
            ///#DEBUG
            upshot.assert(this._autoRefresh);  // We should only get here if we scheduled a recompute, for auto-refresh.
            ///#ENDDEBUG

            this._trigger("refreshStart");

            var results = this._applyQuery(obs.asArray(this._entitySource.getEntities()));
            // Don't __triggerRecompute here.  Downstream listeners on this data source will recompute
            // as part of this wave.
            this._applyNewQueryResult(results.entities, results.totalCount);

            this._trigger("refreshSuccess", obs.asArray(this._clientEntities), this._lastRefreshTotalEntityCount);
        },

        _normalizePropertyValue: function (entity, property) {
            // TODO -- Should do this based on metadata and return default value of the correct scalar type.
            return obs.getProperty(entity, property) || "";
        },

        _onPropertyChanged: function (entity, property, newValue) {
            base._onPropertyChanged.apply(this, arguments);

            if (this._refreshAllInProgress) {
                // We don't want to event "need refresh" due to a "refresh all".
                // Rather, we want to issue "refresh completed".
                return;
            }

            if (!this._needRecompute) {
                var needRecompute = false;
                if (this._haveEntity(entity)) {
                    if (this._filter && !this._filter(entity)) {
                        needRecompute = true;
                    }
                } else if (this._filter && this._filter(entity)) {
                    // This is overly pessimistic if we have paging options in place.
                    // It could be that this entity is already on a preceding page (and will
                    // stay there on recompute) or would be added to a following page (and
                    // excluded from the current page on recompute).
                    needRecompute = true;
                }
                if (this._haveEntity(entity) && this._sort) {
                    if ($.isFunction(this._sort)) {
                        needRecompute = true;
                    } else if (upshot.isArray(this._sort)) {
                        needRecompute = $.grep(this._sort, function (sortPart) {
                            return sortPart.property === property;
                        }).length > 0;
                    } else {
                        needRecompute = this._sort.property === property;
                    }
                }

                if (needRecompute) {
                    this._setNeedRecompute();
                }
            }
        },

        // support the following filter formats
        // function, [functions], filterPart, [filterParts]
        // return: function
        _createFilterFunction: function (filter) {
            var self = this;
            if ($.isFunction(filter)) {
                return filter;
            }

            var filters = this._normalizeFilters(filter);
            var comparisonFunctions = []
            for (var i = 0; i < filters.length; i++) {
                var filterPart = filters[i];
                if ($.isFunction(filterPart)) {
                    comparisonFunctions.push(filterPart);
                } else {
                    var func = createFunction(filterPart.property, filterPart.operator, filterPart.value);
                    comparisonFunctions.push(func);
                }
            }
            return function (entity) {
                for (var i = 0; i < comparisonFunctions.length; i++) {
                    if (!comparisonFunctions[i](entity)) {
                        return false;
                    }
                }
                return true;
            };

            function createFunction(filterProperty, filterOperator, filterValue) {
                var comparer;
                switch (filterOperator) {
                    case "<": comparer = function (propertyValue) { return propertyValue < filterValue; }; break;
                    case "<=": comparer = function (propertyValue) { return propertyValue <= filterValue; }; break;
                    case "==": comparer = function (propertyValue) { return propertyValue == filterValue; }; break;
                    case "!=": comparer = function (propertyValue) { return propertyValue != filterValue; }; break;
                    case ">=": comparer = function (propertyValue) { return propertyValue >= filterValue; }; break;
                    case ">": comparer = function (propertyValue) { return propertyValue > filterValue; }; break;
                    case "Contains":
                        comparer = function (propertyValue) {
                            if (typeof propertyValue === "string" && typeof filterValue === "string") {
                                propertyValue = propertyValue.toLowerCase();
                                filterValue = filterValue.toLowerCase();
                            }
                            return propertyValue.indexOf(filterValue) >= 0;
                        };
                        break;
                    default: throw "Unrecognized filter operator.";
                };

                return function (entity) {
                    // Can't trust added entities, for instance, to have all required property values.
                    var propertyValue = self._normalizePropertyValue(entity, filterProperty);
                    return comparer(propertyValue);
                };
            };
        },

        _getSortFunction: function () {
            var self = this;
            if (!this._sort) {
                return null;
            } else if ($.isFunction(this._sort)) {
                return this._sort;
            } else if (upshot.isArray(this._sort)) {
                var sortFunction;
                $.each(this._sort, function (unused, sortPart) {
                    var sortPartFunction = getSortPartFunction(sortPart);
                    if (!sortFunction) {
                        sortFunction = sortPartFunction;
                    } else {
                        sortFunction = function (sortPartFunction1, sortPartFunction2) {
                            return function (entity1, entity2) {
                                var result = sortPartFunction1(entity1, entity2);
                                return result === 0 ? sortPartFunction2(entity1, entity2) : result;
                            };
                        } (sortFunction, sortPartFunction);
                    }
                });
                return sortFunction;
            } else {
                return getSortPartFunction(this._sort);
            }

            function getSortPartFunction(sortPart) {
                return function (entity1, entity2) {
                    var isAscending = !sortPart.descending,
                        propertyName = sortPart.property,
                        propertyValue1 = self._normalizePropertyValue(entity1, propertyName),
                        propertyValue2 = self._normalizePropertyValue(entity2, propertyName);
                    if (propertyValue1 == propertyValue2) {
                        return 0;
                    } else if (propertyValue1 > propertyValue2) {
                        return isAscending ? 1 : -1;
                    } else {
                        return isAscending ? -1 : 1;
                    }
                };
            }
        },

        _applyQuery: function (entities) {
            var self = this;

            var filteredEntities;
            if (this._filter) {
                filteredEntities = $.grep(entities, function (entity, index) {
                    return self._filter(entity);
                });
            } else {
                filteredEntities = entities;
            }

            var sortFunction = this._getSortFunction(),
            sortedEntities;
            if (sortFunction) {
                // "sort" modifies filtered entities, so we must be operating against a copy
                // by this point.  Otherwise, we'll potentially update the LDS input array.
                if (filteredEntities === entities) {
                    filteredEntities = filteredEntities.slice(0);
                }
                sortedEntities = filteredEntities.sort(sortFunction);
            } else {
                sortedEntities = filteredEntities;
            }

            var skip = this._skip || 0,
                pagedEntities = skip > 0 ? sortedEntities.slice(skip) : sortedEntities;
            if (this._take) {
                pagedEntities = pagedEntities.slice(0, this._take);
            }

            return { entities: pagedEntities, totalCount: sortedEntities.length };
        },

        _onArrayChanged: function (type, eventArguments) {
            base._onArrayChanged.apply(this, arguments);

            if (this._refreshAllInProgress) {
                // We don't want to event "need refresh" due to a "refresh all".
                // Rather, we want to issue "refresh completed".
                return;
            }

            if (!this._needRecompute) {
                // See if the inner array change should cause us to raise the "need refresh" event.
                var self = this,
                    needRecompute = false;

                switch (type) {
                    case "insert":
                        var insertedEntities = eventArguments.items;
                        if (insertedEntities.length > 0) {
                            var anyExternallyInsertedEntitiesMatchFilter = $.grep(insertedEntities, function (entity) {
                                return (!self._filter || self._filter(entity)) && $.inArray(entity, obs.asArray(self._clientEntities)) < 0;
                            }).length > 0;
                            if (anyExternallyInsertedEntitiesMatchFilter) {
                                needRecompute = true;
                            }
                        }
                        break;

                    case "remove":
                        if (this._take > 0 || this._skip > 0) {
                            // If we have paging options, we have to conservatively assume that the result will be shy
                            // of the _limit due to this delete or the result should be shifted due to a delete from
                            // those entities preceding the _skip.
                            needRecompute = true;

                            // NOTE: This covers the case where an entity in our input reaches the upshot.EntityState.Deleted
                            // state.  We assume that this will cause the entity to be removed from the input EntitySource.
                        } else {
                            var nonDeletedResultEntitiesRemoved = $.grep(eventArguments.items, function (entity) {
                                return self._haveEntity(entity) &&
                                    (self.getEntityState(entity) || upshot.EntityState.Deleted) !== upshot.EntityState.Deleted;
                            });
                            if (nonDeletedResultEntitiesRemoved.length > 0) {
                                // If the input EntitySource happens to be an EntityView and entities leave that view
                                // for some other reason than reaching the upshot.EntityState.Deleted state, we should
                                // signal the need for a recompute to remove these entities from our results (but let
                                // the client control this for the non-auto-refresh case).
                                needRecompute = true;
                            }
                        }
                        break;

                    case "replaceAll":
                        if (!this._refreshAllInProgress) {
                            // We don't want to event "need refresh" due to a "refresh all".
                            // Rather, we want to issue "refresh completed".

                            var results = this._applyQuery(eventArguments.newItems);
                            if (this.totalCount !== results.totalCount) {
                                needRecompute = true;
                            } else {
                                // Reference comparison is enough here.  "property changed" catches deeper causes of "need refresh".
                                needRecompute = !upshot.sameArrayContents(obs.asArray(this._clientEntities), results.entities);
                            }
                        }
                        break;

                    default:
                        throw "Unknown array operation '" + type + "'.";
                }

                if (needRecompute) {
                    this._setNeedRecompute();
                }
            }
        },

        _handleEntityAdd: function (entity) {
            if (!this._needRecompute) {
                if (this._filter && !this._filter(entity)) {
                    this._setNeedRecompute();
                }
            }
            base._handleEntityAdd.apply(this, arguments);
        },

        _handleEntityDelete: function (entity) {
            if (!this._needRecompute && this._take > 0) {
                // If we have a _take, we have to conservatively assume that the result will be one entity shy of
                // the take due to this delete.
                this._setNeedRecompute();
            }
            base._handleEntityDelete.apply(this, arguments);
        }
    };

    upshot.LocalDataSource = upshot.deriveClass(base, ctor, instanceMembers);

}
///#RESTORE )(this, jQuery, upshot);
