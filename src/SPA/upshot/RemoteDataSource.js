/// <reference path="IntelliSense\References.js" />
///#RESTORE (function (global, $, upshot, undefined)
{
    var base = upshot.DataSource.prototype;

    var commitEvents = ["commitStart", "commitSuccess", "commitError"];

    var ctor = function (options) {
        /// <summary>
        /// RemoteDataSource is used to load model data matching a query that is evaluated on the server.
        /// </summary>
        /// <param name="options" optional="true">
        /// Options used in the construction of the RemoteDataSource:
        ///     &#10;bufferChanges: (Optional) If 'true', edits to model data are buffered until RemoteDataSource.commitChanges.  Otherwise, edits are committed to the server immediately.
        ///     &#10;result: (Optional) The observable array into which the RemoteDataSource will load model data.
        ///     &#10;dataContext: (Optional) A DataContext instance that acts as a shared cache for multiple DataSource instances.  When not supplied, a DataContext instance is instantiated for this RemoteDataSource.
        ///     &#10;entityType: The type of model data that will be loaded by this RemoteDataSource instance.
        ///     &#10;provider: (Optional) Specifies the DataProvider that will be used to get model data and commit edits to the model data.  Defaults to upshot.DataProvider which works with Microsoft.Web.Http.Data.DataController.
        ///     &#10;providerParameters: (Optional) Parameters that are supplied to the DataProvider for this RemoteDataSource and used by the DataProvider when it gets model data from the server.
        ///     &#10;mapping: (Optional) A function (typically a constructor) used to translate raw model data loaded via the DataProvider into model data that will be surfaced by this RemoteDataSource.
        /// </param>

        // support no new ctor
        if (this._trigger === undefined) {
            return new upshot.RemoteDataSource(options);
        }

        // Optional query options
        this._sort = null;
        this._filters = null;

        var dataProvider, dataContext, mapping;
        if (options) {
            this._providerParameters = options.providerParameters;
            this._entityType = options.entityType;
            dataContext = options.dataContext;

            // support both specification of a provider instance as well as
            // a provider function. In the latter case, we create the provider
            // for the user
            if (!options.provider && !options.dataContext) {
                // we're in a remote scenario but no context or provider has been specified.
                // use our default provider in this case.
                dataProvider = upshot.DataProvider;
            } else {
                dataProvider = options.provider;
            }

            if ($.isFunction(dataProvider)) {
                dataProvider = new dataProvider();
            }

            // Acceptable formats for "mapping":
            // { entityType: "Customer", mapping: Customer }
            // { entityType: "Customer", mapping: { map: Customer, unmap: unmapCustomer } }
            // { mapping: { "Customer": Customer } }
            // { mapping: { "Customer": { map: Customer, unmap: unmapCustomer } } }
            mapping = options.mapping;
            if (mapping &&
                ($.isFunction(mapping) ||  // Mapping supplied as a "map" function.
                 ($.isFunction(mapping.map) || $.isFunction(mapping.unmap)))) {  // Mapping supplied as map/unmap functions.

                if (!this._entityType) {
                    // TODO: Build out the no-type scenario where the DataSource supplies a mapping
                    // function and our merge algorithm ignores any typing from the DataProvider.
                    throw "Need 'entityType' option in order to supply " +
                        ($.isFunction(mapping) ? "a function" : "map/unmap functions") +
                        " for 'mapping' option.";
                }

                var mappingForType = mapping;
                mapping = {};
                mapping[this._entityType] = mappingForType;
            }
        }

        var self = this;
        if (!dataContext) {
            var implicitCommitHandler;
            if (!options.bufferChanges) {
                // since we're not change tracking, define an implicit commit callback
                // and pass into the DC
                implicitCommitHandler = function () {
                    self._dataContext._commitChanges({ providerParameters: self._providerParameters });
                }
            }

            dataContext = new upshot.DataContext(dataProvider, implicitCommitHandler, mapping);
            // TODO -- If DS exclusively owns the DC, can we make it non-accumulating?
        } else if (mapping) {
            // This will throw if the app is supplying a different mapping for a given entityType.
            dataContext.addMapping(mapping);
        }

        this._dataContext = dataContext;

        // define commit[Start,Success,Error] observers
        var observer = {};
        $.each(commitEvents, function (unused, name) {
            observer[name] = function () {
                self._trigger.apply(self, [name].concat(Array.prototype.slice.call(arguments)));
            };
        });

        this._dataContextObserver = observer;
        this._dataContext.bind(this._dataContextObserver);

        var entitySource = options && options.entityType && this._dataContext.getEntitySet(options.entityType);
        if (entitySource) {
            options = $.extend({}, options, { source: entitySource });
        } else {
            // Until we can bindToEntitySource, fill in the DataContext-specific methods with some usable defaults.
            $.each(upshot.EntityView.__dataContextMethodNames, function (index, name) {
                if (name !== "getDataContext") {
                    self[name] = function () {
                        throw "DataContext-specific methods are not available on RemoteDataSource a result type can be determined.  Consider supplying the \"entityType\" option when creating a RemoteDataSource or execute an initial query against your RemoteDatasource to determine the result type.";
                    };
                }
            });
            this.getDataContext = function () {
                return this._dataContext;
            };
        }

        base.constructor.call(this, options);

        // Events specific to RemoteDataSource
        this._bindFromOptions(options, commitEvents);
    };

    var instanceMembers = {

        setSort: function (sort) {
            /// <summary>
            /// Establishes the sort specification that is to be applied as part of a server query when loading model data.
            /// </summary>
            /// <param name="sort">
            /// &#10;The sort specification to applied when loading model data.
            /// &#10;Should be supplied as an object of the form &#123; property: &#60;propertyName&#62; [, descending: &#60;bool&#62; ] &#125; or an array of ordered objects of this form.
            /// &#10;When supplied as null or undefined, the sort specification for this RemoteDataSource is cleared.
            /// </param>
            /// <returns type="upshot.RemoteDataSource"/>

            // TODO -- Validate sort specification?
            this._sort = (sort && !upshot.isArray(sort)) ? [sort] : sort;
            return this;
        },

        setFilter: function (filter) {
            /// <summary>
            /// Establishes the filter specification that is to be applied as part of a server query when loading model data.
            /// </summary>
            /// <param name="filter">
            /// &#10;The filter specification to applied when loading model data.
            /// &#10;Should be supplied as an object of the form &#123; property: &#60;propertyName&#62;, value: &#60;propertyValue&#62; [, operator: &#60;operator&#62; ] &#125; or an array of ordered objects of this form.
            /// &#10;When supplied as null or undefined, the filter specification for this RemoteDataSource is cleared.
            /// </param>
            /// <returns type="upshot.RemoteDataSource"/>

            this._filters = filter && this._normalizeFilters(filter);
            return this;
        },

        // TODO -- We should do a single setTimeout here instead, just in case N clients request a refresh
        // in response to callbacks.
        refresh: function (options, success, error) {
            /// <summary>
            /// Initiates an asynchronous get to load model data matching the query established with setSort, setFilter and setPaging.
            /// </summary>
            /// <param name="options" optional="true">
            /// &#10;There are no valid options recognized by RemoteDataSource.
            /// </param>
            /// <param name="success" type="Function" optional="true">
            /// &#10;A success callback with signature function(entities, totalCount).
            /// </param>
            /// <param name="error" type="Function" optional="true">
            /// &#10;An error callback with signature function(httpStatus, errorText, context).
            /// </param>
            /// <returns type="upshot.RemoteDataSource"/>

            this._verifyOkToRefresh();

            if ($.isFunction(options)) {
                error = success;
                success = options;
                options = undefined;
            }

            this._trigger("refreshStart");

            var self = this,
                onSuccess = function (entitySet, entities, totalCount) {
                    self._bindToEntitySource(entitySet);
                    self._completeRefresh(entities, totalCount, success);
                },
                onError = function (httpStatus, errorText, context) {
                    self._failRefresh(httpStatus, errorText, context, error);
                };

            this._dataContext.__load({
                entityType: this._entityType,
                providerParameters: this._providerParameters,

                queryParameters: {
                    filters: this._filters,
                    sort: this._sort,
                    skip: this._skip,
                    take: this._take,
                    includeTotalCount: this._includeTotalCount
                }
            }, onSuccess, onError);
            return this;
        },

        commitChanges: function (success, error) {
            /// <summary>
            /// Initiates an asynchronous commit of any model data edits collected by the DataContext for this RemoteDataSource.
            /// </summary>
            /// <param name="success" type="Function" optional="true">
            /// &#10;A success callback.
            /// </param>
            /// <param name="error" type="Function" optional="true">
            /// &#10;An error callback with signature function(httpStatus, errorText, context).
            /// </param>
            /// <returns type="upshot.RemoteDataSource"/>

            this._dataContext.commitChanges({
                providerParameters: this._providerParameters
            }, $.proxy(success, this), $.proxy(error, this));
            return this;
        },


        // Private methods

        _dispose: function () {
            this._dataContext.unbind(this._dataContextObserver);
            base._dispose.apply(this, arguments);
        },

        _bindToEntitySource: function (entitySource) {

            base._bindToEntitySource.call(this, entitySource);

            // Reverting changes at this level with no "entities" arguments will revert all changes in the data context.
            // TODO -- should AssociatedEntitiesView do the same thing with respect to revertChanges
            this.revertChanges = function () {
                return arguments.length > 0
                    ? this._entitySource.revertChanges.apply(this._entitySource, arguments)
                    : this._dataContext.revertChanges();
            };
        }

    };

    upshot.RemoteDataSource = upshot.deriveClass(base, ctor, instanceMembers);

}
///#RESTORE )(this, jQuery, upshot);
