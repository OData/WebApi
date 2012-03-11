/// <reference path="IntelliSense\References.js" />
///#RESTORE (function (global, $, upshot, undefined)
{
    var base = upshot.EntityView.prototype;

    var obs = upshot.observability;
    var queryOptions = { paging: "setPaging", sort: "setSort", filter: "setFilter" };

    var ctor = function (options) {

        if (options && options.result && options.result.length !== 0) {
            throw "NYI -- Currently, \"result\" array must be empty to bind to a data source.";
        }

        this._skip = null;
        this._take = null;
        this._includeTotalCount = false;
        this._lastRefreshTotalEntityCount = 0;
        this._allowRefreshWithEdits = options && !!options.allowRefreshWithEdits;

        if (options) {
            var self = this;
            $.each(options, function (key, value) {
                if (queryOptions[key]) {
                    self[queryOptions[key]](value);
                }
            });
        }

        base.constructor.apply(this, arguments);

        // Events specific to DataSource
        this._bindFromOptions(options, [ "refreshStart", "refreshSuccess", "refreshError" ]);
    };

    var instanceMembers = {

        // Public methods

        // TODO -- These query set-* methods should be consolidated, passing a "settings" parameter.
        // That way, we can issue a single "needs refresh" event when the client updates the query settings.
        // TODO -- Changing query options should trigger "results stale".

        setSort: function (sort) {
            throw "Unreachable";  // Abstract/pure virtual method.
        },

        setFilter: function (filter) {
            throw "Unreachable";  // Abstract/pure virtual method.
        },

        setPaging: function (paging) {
            /// <summary>
            /// Establishes the paging specification that is to be applied when loading model data.
            /// </summary>
            /// <param name="paging">
            /// &#10;The paging specification to be applied when loading model data.
            /// &#10;Should be supplied as an object of the form &#123; skip: &#60;number&#62;, take: &#60;number&#62;, includeTotalCount: &#60;bool&#62; &#125;.  All properties on this object are optional.
            /// &#10;When supplied as null or undefined, the paging specification for this DataSource is cleared.
            /// </param>
            /// <returns type="upshot.DataSource"/>

            paging = paging || {};
            this._skip = paging.skip;
            this._take = paging.take;
            this._includeTotalCount = !!paging.includeTotalCount;
            return this;
        },

        getTotalEntityCount: function () {
            /// <summary>
            /// Returns the total entity count from the last refresh operation on this DataSource.  This count will differ from DataSource.getEntities().length when a filter or paging specification is applied to this DataSource.
            /// </summary>
            /// <returns type="Number"/>

            // NOTE: We had been updating this to reflect internal, client-only adds, but this doesn't
            // generalize nicely.  For instance, a RemoteDataSource might have server-only logic that 
            // determines whether an added entity should be included in a filtered query result.
            // TODO: Revisit this conclusion.
            return this._lastRefreshTotalEntityCount;
        },

        refresh: function (options) {
            throw "Unreachable";  // Abstract/pure virtual method.
        },

        reset: function () {
            /// <summary>
            /// Empties the result array for this DataSource (that is, the array returned by DataSource.getEntities()).  The result array can be repopulated using DataSource.refresh().
            /// </summary>
            /// <returns type="Number"/>

            this._applyNewQueryResult([]);
            return this;
        },


        // Private methods

        // acceptable filter parameter
        // { property: "Id", operator: "==", value: 1 }  // default operator is "=="
        // and an array of such
        _normalizeFilters: function (filter) {
            filter = upshot.isArray(filter) ? filter : [filter];
            var filters = [];
            for (var i = 0; i < filter.length; i++) {
                var filterPart = filter[i];
                if (filterPart) {
                    if (!$.isFunction(filterPart)) {
                        filterPart.operator = filterPart.operator || "==";
                    }
                    filters.push(filterPart);
                }
            }
            return filters;
        },

        _verifyOkToRefresh: function () {
            if (!this._allowRefreshWithEdits) {
                var self = this;
                $.each(obs.asArray(this._clientEntities), function (unused, entity) {
                    if (self.getEntityState(entity) !== upshot.EntityState.Unmodified) {
                        throw "Refreshing this DataSource will potentially remove unsaved entities.  Such entities might encounter errors during save, and your app should have UI to view such errors.  Either disallow DataSource.refresh() with edits or build error UI and suppress this exception with the 'allowRefreshWithEdits' DataSource option.";
                    }
                });
            }
        },

        _completeRefresh: function (entities, totalCount, success) {
            if (this._applyNewQueryResult(entities, totalCount)) {
                upshot.__triggerRecompute();
            }

            var newClientEntities = obs.asArray(this._clientEntities),
            newTotalCount = this._lastRefreshTotalEntityCount;
            this._trigger("refreshSuccess", newClientEntities, newTotalCount);
            if ($.isFunction(success)) {
                success.call(this, newClientEntities, newTotalCount);
            }
        },

        _failRefresh: function (httpStatus, errorText, context, fail) {
            this._trigger("refreshError", httpStatus, errorText, context);
            if ($.isFunction(fail)) {
                fail.call(this, httpStatus, errorText, context);
            }
        },

        _applyNewQueryResult: function (entities, totalCount) {
            this._lastRefreshTotalEntityCount = totalCount;

            var sameEntities = upshot.sameArrayContents(obs.asArray(this._clientEntities), entities);
            if (!sameEntities) {
                // Update our client entities.
                var oldEntities = obs.asArray(this._clientEntities).slice();
                obs.refresh(this._clientEntities, entities);
                this._trigger("arrayChanged", "replaceAll", { oldItems: oldEntities, newItems: obs.asArray(this._clientEntities) });
                return true;
            } else {
                return false;
            }
        }
    };

    upshot.DataSource = upshot.deriveClass(base, ctor, instanceMembers);

}
///#RESTORE )(this, jQuery, upshot);
