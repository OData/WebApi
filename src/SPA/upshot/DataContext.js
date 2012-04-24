/// <reference path="IntelliSense\References.js" />
///#RESTORE (function (global, $, upshot, undefined)
{
    var obs = upshot.observability;

    var ctor = function (dataProvider, implicitCommitHandler, mappings) {

        // support no new ctor
        if (this._trigger === undefined) {
            return new upshot.DataContext(dataProvider, implicitCommitHandler);
        }

        this._dataProvider = dataProvider;
        this.__manageAssociations = true;  // TODO: Make this configurable by the app.  Fix unmanaged associations.
        this._implicitCommitHandler = implicitCommitHandler;

        this._eventCallbacks = {};
        this._entitySets = {};

        this._mappings = {};
        if (mappings) {
            this.addMapping(mappings);
        }
    };

    function getProviderParameters(type, parameters) {
        var result;
        if (parameters) {
            // first include any explicit get/submit
            // param properties
            result = $.extend(result, parameters[type] || {});

            // next, add any additional "outer" properties
            for (var prop in parameters) {
                if (prop !== "get" && prop !== "submit") {
                    result[prop] = parameters[prop];
                }
            }
        }
        return result;
    }

    var instanceMembers = {

        // Public methods

        dispose: function () {
            /// <summary>
            /// Disposes the DataContext instance.
            /// </summary>

            if (this._entitySets) {  // Use _entitySets as an indicator as to whether we've been disposed.
                $.each(this._entitySets, function (index, entitySet) {
                    entitySet.__dispose();
                });
                this._entitySets = null;
            }
        },

        // TODO: bind/unbind/_trigger are duplicated in EntitySource and DataContext, consider common routine.
        bind: function (event, callback) {
            /// <summary>
            /// Registers the supplied callback to be called when an event is raised.
            /// </summary>
            /// <param name="event" type="String">
            /// &#10;The event name.
            /// </param>
            /// <param name="callback" type="Function">
            /// &#10;The callback function.
            /// </param>
            /// <returns type="upshot.DataContext"/>

            if (typeof event === "string") {
                var list = this._eventCallbacks[event] || (this._eventCallbacks[event] = []);
                list.push(callback);
            } else {
                for (var key in event) {
                    this.bind(key, event[key]);
                }
            }
            return this;
        },

        unbind: function (event, callback) {
            /// <summary>
            /// Deregisters the supplied callback for the supplied event.
            /// </summary>
            /// <param name="event" type="String">
            /// &#10;The event name.
            /// </param>
            /// <param name="callback" type="Function">
            /// &#10;The callback function to be deregistered.
            /// </param>
            /// <returns type="upshot.DataContext"/>

            if (typeof event === "string") {
                var list = this._eventCallbacks[event];
                if (list) {
                    for (var i = 0, l = list.length; i < l; i++) {
                        if (list[i] === callback) {
                            list.splice(i, 1);
                            break;
                        }
                    }
                }
            } else {
                for (var key in event) {
                    this.unbind(key, event[key]);
                }
            }
            return this;
        },

        addMapping: function (entityType, mapping) {  // TODO: Should we support CTs here too?  Or take steps to disallow?
            // TODO: Need doc comments.

            if (typeof entityType === "string") {
                var mappingT = mapping;
                mapping = {};
                mapping[entityType] = mappingT;
            } else {
                mapping = entityType;
            }

            var self = this;
            $.each(mapping, function (entityType, mapping) {
                if ($.isFunction(mapping)) {
                    mapping = { map: mapping };
                }

                var existingMapping = self._mappings[entityType];
                if (!existingMapping) {
                    var entitySet = self._entitySets[entityType];
                    if (entitySet && entitySet.getEntities().length > 0) {
                        throw "Supply a mapping for a type before loading data of that type";
                    }
                    self._mappings[entityType] = { map: mapping.map, unmap: mapping.unmap };
                } else if (existingMapping.map !== mapping.map || existingMapping.unmap !== mapping.unmap) {
                    throw "For a given type, DataContext.addMapping must be supplied the same map/unmap functions.";
                }
            });

            return this;
        },

        getEntitySet: function (entityType) {
            /// <summary>
            /// Returns the EntitySet for the supplied type.
            /// </summary>
            /// <param name="entityType" type="String"/>
            /// <returns type="upshot.EntitySet"/>

            var entitySet = this._entitySets[entityType];
            if (!entitySet) {
                entitySet = this._entitySets[entityType] = new upshot.EntitySet(this, entityType);
            }
            return entitySet;
        },

        getEntityErrors: function () {
            /// <summary>
            /// Returns an array of server errors by entity, of the form [ &#123; entity: &#60;entity&#62;, error: &#60;object&#62; &#125;, ... ].
            /// </summary>
            /// <returns type="Array"/>

            var errors = [];
            $.each(this._entitySets, function (type, entitySet) {
                var spliceArguments = [errors.length, 0].concat(entitySet.getEntityErrors());
                [ ].splice.apply(errors, spliceArguments);
            });
            return errors;
        },

        getEntityError: function (entity) {
            /// <summary>
            /// Returns server errors for the supplied entity.
            /// </summary>
            /// <param name="entity" type="Object">
            /// &#10;The entity for which server errors are to be returned.
            /// </param>
            /// <returns type="Object"/>

            var error;
            // TODO: we should get related-entitySet for an entity, then getEntityError.
            // this will need type rationalization across all providers.
            $.each(this._entitySets, function (unused, entitySet) {
                error = entitySet.getEntityError(entity);
                return !error;
            });
            return error;
        },

        commitChanges: function (options, success, error) {
            /// <summary>
            /// Initiates an asynchronous commit of any model data changes collected by this DataContext.
            /// </summary>
            /// <param name="success" type="Function" optional="true">
            /// &#10;A success callback.
            /// </param>
            /// <param name="error" type="Function" optional="true">
            /// &#10;An error callback with signature function(httpStatus, errorText, context).
            /// </param>
            /// <returns type="upshot.DataContext"/>

            if (this._implicitCommitHandler) {
                throw "Data context must be in change-tracking mode to explicitly commit changes.";
            }
            this._commitChanges(options, success, error);
            return this;
        },

        revertChanges: function () {
            /// <summary>
            /// Reverts any changes to model data (to entities) back to original entity values.
            /// </summary>
            /// <returns type="upshot.DataContext"/>

            $.each(this._entitySets, function (type, entitySet) {
                entitySet.__revertChanges();
            });
            upshot.__triggerRecompute();
            return this;
        },

        merge: function (entities, type, includedEntities) {
            /// <summary>Merges data into the cache</summary>
            /// <param name="entities" type="Array">The array of entities to add or merge into the cache</param>
            /// <param name="type" type="String">The type of the entities to be merge into the cache. This parameter can be null/undefined when no entities are supplied</param>
            /// <param name="includedEntities" type="Array">An additional array of entities (possibly related) to add or merge into the cache.  These entities will not be returned from this function. This parameter is optional</param>
            /// <returns type="Array">The array of entities with newly merged values</returns>

            var self = this;
            includedEntities = includedEntities || {};

            $.each(entities, function (unused, entity) {
                self.__flatten(entity, type, includedEntities);
            });

            $.each(includedEntities, function (type, entities) {
                var entitySet = self.getEntitySet(type);
                entitySet.__loadEntities(entities);
            });

            var entitySet = type && this.getEntitySet(type),
                mergedEntities = entitySet ? entitySet.__loadEntities(entities) : [];

            upshot.__triggerRecompute();

            return mergedEntities;
        },

        // TODO -- We have no mechanism to similarly clear data sources.
        //// clear: function () {
        ////     $.each(this._entitySets, function (type, entitySet) {
        ////         entitySet.__clear();
        ////     });
        //// },

        // Internal methods

        // recursively visit the specified entity and its associations, accumulating all
        // associated entities to the included entities collection
        __flatten: function (entity, entityType, includedEntities) {
            var self = this;

            $.each(upshot.metadata.getProperties(entity, entityType, true), function (index, prop) {
                var value = obs.getProperty(entity, prop.name);
                if (value) {
                    if (prop.association) {
                        var associatedEntities = upshot.isArray(value) ? value : [value],
                            associatedEntityType = prop.type,
                            entities = includedEntities[associatedEntityType] || (includedEntities[associatedEntityType] = []);

                        $.each(associatedEntities, function (inner_index, associatedEntity) {
                            // add the associated entity
                            var identity = upshot.EntitySet.__getIdentity(associatedEntity, associatedEntityType);

                            if (!entities.identityMap) {
                                entities.identityMap = {};
                            }
                            if (!entities.identityMap[identity]) {
                                // add the entity and recursively flatten it
                                entities.identityMap[identity] = true;
                                entities.push(associatedEntity);
                                self.__flatten(associatedEntity, associatedEntityType, includedEntities);
                            }
                            ///#DEBUG
                            // TODO: For unmanaged associations, where is it that we should fix up internal reference
                            // refer only to the atomized entity for a given identity?
                            upshot.assert(self.__manageAssociations);
                            ///#ENDDEBUG
                        });
                    }
                }
            });
        },

        __load: function (options, success, error) {

            var dataProvider = this._dataProvider,
                self = this,
                onSuccess = function (result) {
                    if (self._isDisposed()) {
                        return;
                    }

                    // add metadata if specified
                    if (result.metadata) {
                        upshot.metadata(result.metadata);
                    }

                    // determine the result type
                    var entityType = result.type || options.entityType;
                    if (!entityType) {
                        throw "Unable to determine entity type.";
                    }

                    var entities = $.map(result.entities, function (entity) {
                        return self._mapEntity(entity, entityType);
                    });
                    var includedEntities;
                    if (result.includedEntities) {
                        includedEntities = {};
                        $.each(result.includedEntities, function (type, entities) {
                            includedEntities[type] = $.map(entities, function (entity) {
                                return self._mapEntity(entity, type);
                            });
                        });
                    }

                    var mergedEntities = self.merge(entities, entityType, includedEntities);

                    success.call(self, self.getEntitySet(entityType), mergedEntities, result.totalCount);
                },
                onError = function (httpStatus, errorText, context) {
                    if (!self._isDisposed()) {
                        error.call(self, httpStatus, errorText, context);
                    }
                };

            var getParameters = getProviderParameters("get", options.providerParameters);

            dataProvider.get(getParameters, options.queryParameters, onSuccess, onError);
        },

        __queueImplicitCommit: function () {
            if (this._implicitCommitHandler) {
                // when in implicit commit mode, we group all implicit commits within
                // a single thread of execution by queueing a timer callback that expires
                // immediately.
                if (!this._implicitCommitQueued) {
                    this._implicitCommitQueued = true;

                    var self = this;
                    setTimeout(function () {
                        if (!self._isDisposed()) {
                            self._implicitCommitQueued = false;
                            self._implicitCommitHandler();
                        }
                    }, 0);
                }
            }
        },

        // Private methods

        _isDisposed: function () {
            return this._entitySets === null;
        },

        _trigger: function (eventType) {
            var list = this._eventCallbacks[eventType];
            if (list) {
                var args = Array.prototype.slice.call(arguments, 1);
                // clone the list to be robust against bind/unbind during callback
                list = list.slice(0);
                for (var i = 0, l = list.length; i < l; i++) {
                    list[i].apply(this, args);
                }
            }
            return this;
        },

        _submitChanges: function (options, changedEntities, success, error) {

            this._trigger("commitStart");

            var changes = $.map(changedEntities, function (changedEntity) {
                return changedEntity.entitySet.__getEntityChange(changedEntity.entity);
            });

            $.each(changes, function (index, change) { change.updateEntityState(); });

            var self = this;
            var mapChangeResult = function (result, changeKind, entityType) {
                if (changeKind !== upshot.ChangeKind.Delete) { // Only add/update operations require mapped entity.
                    return $.extend({}, result, {
                        entity: self._mapEntity(result.entity, entityType)
                    });
                }
                return result;
            };

            var unmapChange = function (change) {
                if (change.changeKind !== upshot.ChangeKind.Delete) { // Delete operations don't require unmapping
                    var unmap = (self._mappings[change.entityType] || {}).unmap || obs.unmap;
                    change.entity = unmap(change.entity, change.entityType);
                }
            };
            $.each(changes, function (unused, change) {
                return unmapChange(change.changeSetEntry);
            });

            var onSuccess = function (submitResult) {
                    if (self._isDisposed()) {
                        return;
                    }

                    // all updates in the changeset where successful
                    $.each(changes, function (index, change) {
                        change.succeeded(mapChangeResult(submitResult[index], change.changeSetEntry.changeKind, change.changeSetEntry.entityType));
                    });
                    upshot.__triggerRecompute();
                    self._trigger("commitSuccess", submitResult);
                    if (success) {
                        success.call(self, submitResult);
                    }
                },
                onError = function (httpStatus, errorText, context, submitResult) {
                    if (self._isDisposed()) {
                        return;
                    }

                    // one or more updates in the changeset failed
                    $.each(changes, function (index, change) {
                        if (submitResult) {
                            // if a submitResult was provided, we use that data in the
                            // completion of the change
                            var changeResult = submitResult[index];
                            if (changeResult.error) {
                                change.failed(changeResult.error);
                            } else {
                                // even though there were failures in the changeset,
                                // this particular change is marked as completed, so
                                // we need to accept changes for it
                                change.succeeded(mapChangeResult(change.changeSetEntry.entityType, changeResult));
                            }
                        } else {
                            // if we don't have a submitResult, we still need to state
                            // transition the change properly
                            change.failed(null);
                        }
                    });

                    upshot.__triggerRecompute();
                    self._trigger("commitError", httpStatus, errorText, context, submitResult);
                    if (error) {
                        error.call(self, httpStatus, errorText, context, submitResult);
                    }
                };

            var submitParameters = getProviderParameters("submit", options.providerParameters),
                changeSet = $.map(changes, function (change) {
                    return change.changeSetEntry;
                });

            this._dataProvider.submit(submitParameters, changeSet, onSuccess, onError);
        },

        _commitChanges: function (options, success, error) {
            var changedEntities = [];
            $.each(this._entitySets, function (type, entitySet) {
                var entities = $.map(entitySet.__getChangedEntities(), function (entity) {
                    return { entitySet: entitySet, entity: entity };
                });
                [ ].push.apply(changedEntities, entities);
            });

            this._submitChanges(options, changedEntities, success, error);
            upshot.__triggerRecompute();
        },

        _mapEntity: function (data, entityType) {
            return this._map(data, entityType, true);
        },

        _map: function (data, entityType, isObject) {
            if (isObject || upshot.isObject(data)) {
                var map = (this._mappings[entityType] || {}).map;
                if (map) {
                    // Don't pass "entityType"/"mapNested" as we do below for obs.map.
                    // This would pollute the signature for app-supplied map functions (especially 
                    // when ctors are supplied).
                    return new map(data);  // Use "new" here to allow ctors to be passed as map functions.
                }
            }

            // The "map" function provided by the observability layer takes a function
            // to map nested objects, so we take advantage of app-supplied mapping functions.
            var self = this,
                mapNested = function (data, entityType) {
                    return self._map(data, entityType);
                };
            return obs.map(data, entityType, mapNested);
        }
    };

    upshot.DataContext = upshot.defineClass(ctor, instanceMembers);

}
///#RESTORE )(this, jQuery, upshot);
