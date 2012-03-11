/// <reference path="IntelliSense\References.js" />
///#RESTORE (function (global, $, upshot, undefined)
{
    var base = upshot.EntitySource.prototype;

    var obs = upshot.observability;

    var ctor = function (options) {

        this._needRecompute = false;

        var self = this;
        this._observer = {
            propertyChanged: function (entity, property, newValue) { self._onPropertyChanged(entity, property, newValue); },
            arrayChanged: function (type, eventArgs) { self._onArrayChanged(type, eventArgs); },
            entityStateChanged: function (entity, state, error) { self._onEntityStateChanged(entity, state, error); },
            entityUpdated: function (entity, path, eventArgs) { self._onEntityUpdated(entity, path, eventArgs); }
        };

        // RemoteDataSource may dynamically bind to its EntitySet as it refreshes.
        this._entitySource = null;  // Make JS runtime type inference happy?

        var entitySource = options && options.source;
        if (entitySource) {
            this._bindToEntitySource(entitySource);
        }

        base.constructor.call(this, options);
    };

    var dataContextMethodNames = [ 
        "getDataContext", 
        "getEntityState", 
        "getEntityValidationRules", 
        "getEntityId", 
        "revertChanges", 
        "deleteEntity",
        "getEntityErrors",
        "getEntityError",
        "isUpdated",
        "revertUpdates"
    ];

    var instanceMembers = {

        ///#DEBUG
        getDataContext: function () {
            /// <summary>
            /// Returns the DataContext used as a cache for model data.
            /// </summary>
            /// <returns type="upshot.DataContext"/>

            throw "Not reached";  // For Intellisense only.
        },

        getEntityState: function (entity) {
            /// <summary>
            /// Returns the EntityState for the supplied entity.
            /// </summary>
            /// <param name="entity" type="Object">
            /// &#10;The entity for which EntityState will be returned.
            /// </param>
            /// <returns type="upshot.EntityState"/>

            throw "Not reached";  // For Intellisense only.
        },

        getEntityValidationRules: function () {
            /// <summary>
            /// Returns entity validation rules for the type of entity returned by this EntityView.
            /// </summary>
            /// <returns type="Object"/>

            throw "Not reached";  // For Intellisense only.
        },

        getEntityId: function (entity) {
            /// <summary>
            /// Returns an identifier for the supplied entity.
            /// </summary>
            /// <param name="entity" type="Object"/>
            /// <returns type="String"/>

            throw "Not reached";  // For Intellisense only.
        },

        revertChanges: function (all) {
            /// <summary>
            /// Reverts any edits to model data (to entities) back to original entity values.
            /// </summary>
            /// <param name="all" type="Boolean" optional="true">
            /// &#10;Revert all model edits in the underlying DataContext (to all types of entities).  Otherwise, revert changes only to those entities of the type loaded by this EntityView.
            /// </param>
            /// <returns type="upshot.EntityView"/>

            throw "Not reached";  // For Intellisense only.
        },

        deleteEntity: function (entity) {
            /// <summary>
            /// Marks the supplied entity for deletion.  This is a non-destructive operation, meaning that the entity will remain in the EntityView.getEntities() array until the server commits the delete.
            /// </summary>
            /// <param name="entity" type="Object">
            /// &#10;The entity to be marked for deletion.
            /// </param>
            /// <returns type="upshot.EntityView"/>

            throw "Not reached";  // For Intellisense only.
        },

        getEntityErrors: function () {
            /// <summary>
            /// Returns an array of server errors by entity, of the form [ &#123; entity: &#60;entity&#62;, error: &#60;object&#62; &#125;, ... ].
            /// </summary>
            /// <returns type="Array"/>

            throw "Not reached";  // For Intellisense only.
        },

        getEntityError: function (entity) {
            /// <summary>
            /// Returns server errors for the supplied entity.
            /// </summary>
            /// <param name="entity" type="Object">
            /// &#10;The entity for which server errors are to be returned.
            /// </param>
            /// <returns type="Object"/>

            throw "Not reached";  // For Intellisense only.
        },

        isUpdated: function (entity, path, ignoreChildren) {
            /// <summary>
            /// Returns whether the entity of any of the objects or arrays it contains are updated. When a path is specified,
            /// it returns whether the specified property of any of its children are updated. This function will never return
            /// 'true' for entities not in the 'ClientUpdated' state.
            /// </summary>
            /// <param name="entity" type="Object">
            /// &#10;The entity to check for updates
            /// </param>
            /// <param name="path" type="String" optional="true">
            /// &#10;The path to the property to check for updates. The path should be valid javascript; for example "Addresses[3].Street".
            /// </param>
            /// <param name="ignoreChildren" type="Boolean" optional="true">
            /// &#10;Whether or not updates to the children of the specified property should be considered in the result
            /// </param>
            /// <returns type="Boolean"/>

            throw "Not reached";  // For Intellisense only.
        },

        revertUpdates: function (entity, path, skipChildren) {
            /// <summary>
            /// Reverts updates to the entity and all the objects or arrays it contains. When a path is specified, it will
            /// revert only updates to the specified property and all of its children. This function is a no-op for entities
            /// not in the 'ClientUpdated' state.
            /// </summary>
            /// <param name="entity" type="Object">
            /// &#10;The entity to revert updates for
            /// </param>
            /// <param name="path" type="String" optional="true">
            /// &#10;The path to the property to revert updates for. The path should be valid javascript; for example "Addresses[3].Street".
            /// </param>
            /// <param name="skipChildren" type="Boolean" optional="true">
            /// &#10;Whether or not to revert updates to the children of the specified property
            /// </param>
            /// <returns type="upshot.EntityView"/>

            throw "Not reached";  // For Intellisense only.
        },

        ///#ENDDEBUG


        // Internal methods

        __registerForRecompute: function (entityView) {
            // Some EntityView that depends on us wants to recompute.
            base.__registerForRecompute.apply(this, arguments);

            // Register on our input EntitySource transitively back to the root EntitySources, from which
            // our recompute wave originates.
            this._entitySource.__registerForRecompute(this);
        },

        __recompute: function () {
            // Our input EntitySource is giving us an opportunity to recompute.  Do so, if we've so-marked.
            if (this._needRecompute) {
                this._needRecompute = false;
                this._recompute();
            }

            // Tell EntityViews that depend on us to recompute.
            this.__recomputeDependentViews();
        },


        // Private methods

        _dispose: function () {
            if (this._entitySource) {  // RemoteDataSource dynamically binds to its input EntitySource.
                this._entitySource.unbind(this._observer);
            }
            base._dispose.apply(this, arguments);
        },

        _bindToEntitySource: function (entitySource) {

            if (this._entitySource === entitySource) {
                return;
            }

            var self = this;

            // Remove proxied DataContext-derived methods.
            if (this._entitySource) {
                $.each(dataContextMethodNames, function (index, name) {
                    if (self[name]) {
                        delete self[name];
                    }
                });

                this._entitySource.unbind(this._observer);
            }

            this._entitySource = entitySource;

            // Proxy these DataContext-derived methods, if they're available.
            if (entitySource.getDataContext) {
                $.each(dataContextMethodNames, function (index, name) {
                    if (name !== "getEntityErrors") {
                        // Don't use $.proxy here, as that will statically bind to entitySource[name] and
                        // RemoteDataSource will dynamically change entitySource[name].
                        self[name] = function () {
                            var ret = entitySource[name].apply(entitySource, arguments);
                            return (name === "deleteEntity") ? self : ret;
                        };
                    }
                });
            }

            this.getEntityErrors = function () {
                return $.grep(entitySource.getEntityErrors(), function (error) {
                    return self._haveEntity(error.entity);
                });
            };

            entitySource.bind(this._observer);
        },

        _setNeedRecompute: function () {
            // Sub-classes will call this method to mark themselves as being dirty, requiring recompute.
            this._needRecompute = true;
            this._entitySource.__registerForRecompute(this);
        },

        _recompute: function () {
            // In response to a call to _setNeedRecompute, we're getting called to recompute as
            // part of the next recompute wave.
            throw "Unreachable";  // Abstract/pure virtual method.
        },

        _handleEntityAdd: function (entity) {
            // Translate adds onto our input EntitySource.
            this._entitySource.__addEntity(entity);

            base._handleEntityAdd.apply(this, arguments);
        },

        _haveEntity: function (entity) {
            return $.inArray(entity, obs.asArray(this._clientEntities)) >= 0;
        },

        _purgeEntity: function (entity) {
            base._purgeEntity.apply(this, arguments);
        },

        _onPropertyChanged: function (entity, property, newValue) {
            // Translate property changes from our input EntitySource onto our result, if appropriate.
            // NOTE: _haveEntity will be with respect to our current, stable set of result entities.
            // Ignoring direct, observable inserts and removes, this result set will only change
            // as part of our separate recompute wave, which happens _after_ such data change events.
            if (this._haveEntity(entity)) {
                this._trigger("propertyChanged", entity, property, newValue);
            }
        },

        _onArrayChanged: function (type, eventArgs) {
            // NOTE: These are not translated directly in the same way that property and entity state
            // change events are.  Rather, subclasses have specific logic as to how changes to the 
            // membership of their input EntitySource impacts their result entity membership.

            // Will be overridden by derived classes.
        },

        _onEntityStateChanged: function (entity, state, error) {
            if (this._haveEntity(entity)) {
                if (state === upshot.EntityState.Deleted) {
                    // Entities deleted from our cache (due to an accepted server delete or due to a
                    // reverted internal add) should disappear from all dependent EntityViews.
                    this._purgeEntity(entity);
                }

                // Translate entity state changes from our input EntitySource onto our result, if appropriate.
                // NOTE: _haveEntity will be with respect to our current, stable set of result entities.
                // Ignoring direct, observable inserts and removes, this result set will only change
                // as part of our separate recompute wave, which happens _after_ such change events.
                this._trigger("entityStateChanged", entity, state, error);
            }
        },

        _onEntityUpdated: function (entity, path, eventArgs) {
            // Translate property changes from our input EntitySource onto our result, if appropriate.
            // NOTE: _haveEntity will be with respect to our current, stable set of result entities.
            // Ignoring direct, observable inserts and removes, this result set will only change
            // as part of our separate recompute wave, which happens _after_ such data change events.
            if (this._haveEntity(entity)) {
                this._trigger("entityUpdated", entity, path, eventArgs);
            }
        }
    };

    upshot.EntityView = upshot.deriveClass(base, ctor, instanceMembers);

    upshot.EntityView.__dataContextMethodNames = dataContextMethodNames;
}
///#RESTORE )(this, jQuery, upshot);
