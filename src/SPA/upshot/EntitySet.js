/// <reference path="IntelliSense\References.js" />
///#RESTORE (function (global, $, upshot, undefined)
{
    var base = upshot.EntitySource.prototype;

    var obs = upshot.observability;

    var tokenizePath = function (obj, path) {
        var evalTokens = function (obj, tokens) {
            if (tokens.length === 0) return [obj];
            var objs = evalTokens(upshot.isArray(obj) ? obj[tokens.shift()] : obs.getProperty(obj, tokens.shift()), tokens);
            objs.unshift(obj);
            return objs;
        };

        var tokens = path.replace(/\]|\(|\)/g, "").replace(/\[/g, ".").split(".");
        return { tokens: tokens, objs: evalTokens(obj, tokens.slice()) };
    }

    var ctor = function (dataContext, entityType) {

        this._dataContext = dataContext;
        this._entityType = entityType;

        this._callbacks = {};
        this._serverEntities = [];
        this._entityStates = {};
        this._addedEntities = [];
        this._errors = [];
        this._associatedEntitiesViews = {};
        this._tracked = {};
        this._tracker = null;
        this._deferredEntityStateChangeEvents = [];

        base.constructor.call(this);

        upshot.__registerRootEntitySource(this);
    };

    var instanceMembers = {

        // Public methods

        dispose: function () {
            throw "EntitySets should only be disposed by their DataContext.";
        },

        getEntityState: function (entity) {
            /// <summary>
            /// Returns the EntityState for the supplied entity.
            /// </summary>
            /// <param name="entity" type="Object">
            /// &#10;The entity for which EntityState will be returned.
            /// </param>
            /// <returns type="upshot.EntityState"/>

            var id = this.getEntityId(entity);
            return id === null ? null : this._entityStates[id];
        },

        getEntityId: function (entity) {
            /// <summary>
            /// Returns an identifier for the supplied entity.
            /// </summary>
            /// <param name="entity" type="Object"/>
            /// <returns type="String"/>

            var addedEntity = this._getAddedEntityFromEntity(entity);
            if (addedEntity) {
                return addedEntity.clientId;
            }

            try {  // This entity might not have valid PK property values (a reverted add, for instance).
                // Trust only the property values on the original entity, allowing the client to update id properties.
                // The only other way to compute this for some unvetted entity would be to do an O(n) search
                // over this._serverEntities (too slow).
                return this._getEntityIdentity(this._getChanges(entity) ? this._getOriginalValue(entity, this._entityType) : entity);
            } catch (e) {
                return null;
            }
        },

        getDataContext: function () {
            /// <summary>
            /// Returns the DataContext used as a cache for model data.
            /// </summary>
            /// <returns type="upshot.DataContext"/>

            return this._dataContext;
        },

        getEntityValidationRules: function () {
            /// <summary>
            /// Returns entity validation rules for the type of entity cached by this EntitySet.
            /// </summary>
            /// <returns type="Object"/>

            var metadata = upshot.metadata(this._entityType);
            return metadata && metadata.rules && {
                rules: metadata.rules,
                messages: metadata.messages
            };
        },

        getEntityErrors: function () {
            /// <summary>
            /// Returns an array of server errors by entity, of the form [ &#123; entity: &#60;entity&#62;, error: &#60;object&#62; &#125;, ... ].
            /// </summary>
            /// <returns type="Array"/>

            var self = this;
            return $.map(this._errors, function (trackingId) {
                var tracking = self._tracked[trackingId];
                return { entity: tracking.obj, error: tracking.error };
            });
        },

        getEntityError: function (entity) {
            /// <summary>
            /// Returns server errors for the supplied entity.
            /// </summary>
            /// <param name="entity" type="Object">
            /// &#10;The entity for which server errors are to be returned.
            /// </param>
            /// <returns type="Object"/>

            var trackingId = this._getTrackingId(entity);
            if (trackingId) {
                return this._tracked[trackingId].error;
            }
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
            /// <returns type="upshot.EntitySet"/>

            var id = this.getEntityId(entity);

            var state = id !== null && this._entityStates[id];
            if (!state || state === upshot.EntityState.Deleted) {
                throw "Entity not cached in data context.";
            } else if (upshot.EntityState.isServerSyncing(state)) {
                throw "Can't revert an entity while changes are being committed.";
            } else if (state !== upshot.EntityState.ClientUpdated) {
                return this;
            } else if (!path) {
                return this.revertChanges(entity);
            } else {
                var tokens = tokenizePath(entity, path);

                var snapshot = this._clearChangesOnPath(tokens.objs.slice(), tokens.tokens.slice(), skipChildren);
                !this.isUpdated(entity) && this._updateEntityState(id, upshot.EntityState.Unmodified);
                this._restoreOriginalValues(entity, this._entityType, snapshot);
                this._triggerEntityUpdated(entity);

                ///#DEBUG
                this._verifyConsistency(entity, id);
                ///#ENDDEBUG

                upshot.__triggerRecompute();

                return this;
            }
        },

        revertChanges: function (entities) {
            /// <summary>
            /// Reverts the specified entities back to their original state.
            /// </summary>
            /// <param name="entities" type="Array" optional="true">
            /// &#10;One or more entities to revert. This parameter is optional. When omitted, all entities will be reverted.
            /// </param>
            /// <returns type="upshot.EntitySet"/>

            if (!entities) {
                this.__revertChanges();
            } else {
                if (!upshot.isArray(entities)) {
                    entities = [entities];
                }
                var self = this;
                $.each(entities, function (index, entity) {
                    var id = self.getEntityId(entity);

                    var state = id !== null && self._entityStates[id];
                    if (!state || state === upshot.EntityState.Deleted) {
                        throw "Entity no longer cached in data context.";
                    } else if (upshot.EntityState.isServerSyncing(state)) {
                        throw "Can't revert an entity while changes are being committed.";
                    } else if (state === upshot.EntityState.Unmodified) {
                        return;
                    } else if (state === upshot.EntityState.ClientDeleted || state === upshot.EntityState.ClientUpdated) {
                        // Do this before the model change, so listeners on data change events see consistent entity state.
                        self._updateEntityState(id, upshot.EntityState.Unmodified);

                        var snapshot = self._clearChanges(entity, true);
                        self._restoreOriginalValues(entity, self._entityType, snapshot);
                        self._triggerEntityUpdated(entity);
                    } else if (state === upshot.EntityState.ClientAdded) {
                        self._purgeUncommittedAddedEntity(self._getAddedEntityFromId(id), true);
                    } else {
                        throw "Entity changes cannot be reverted for entity in state '" + state + "'.";
                    }
                });
            }

            upshot.__triggerRecompute();

            return this;
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
            var id = this.getEntityId(entity);

            var state = id !== null && this._entityStates[id];
            if (!state || !upshot.EntityState.isUpdated(state)) {
                return false;
            }

            var obj = entity,
                property,
                child;
            if (path) {
                var tokens = tokenizePath(entity, path);
                // objs) [obj, A, B], [obj, A, B, B[0]], [obj, "str"]
                // tokens) ["A", "B"], ["A", "B", "0"], ["C"]
                obj = tokens.objs[tokens.objs.length - 2];
                child = tokens.objs[tokens.objs.length - 1];
                if (!upshot.isArray(obj)) {
                    property = tokens.tokens[tokens.tokens.length - 1];
                }
            }

            var changes = this._getChanges(obj);
            if (!changes) { return false; }
            if (!path) { return true; }

            return (property ? changes.original.hasOwnProperty(property) : false) || (ignoreChildren ? false : this._getChanges(child) !== null);
        },

        deleteEntity: function (entity) {
            /// <summary>
            /// Marks the supplied entity for deletion.  This is a non-destructive operation, meaning that the entity will remain in the EntitySet.getEntities() array until the server commits the delete.
            /// </summary>
            /// <param name="entity" type="Object">
            /// &#10;The entity to be marked for deletion.
            /// </param>
            /// <returns type="upshot.EntitySet"/>

            var id = this.getEntityId(entity),
                entityState = id !== null && this._entityStates[id];
            if (!entityState) {
                throw "Entity not cached in data context.";
            } else if (entityState === upshot.EntityState.ClientAdded) {
                // Deleting a entity that is uncommitted and only on the client.
                this._purgeUncommittedAddedEntity(this._getAddedEntityFromId(id));
            } else if (upshot.EntityState.isServerSyncing(entityState)) {
                // Force the application to block deletes while saving any edit to this same entity.
                // We don't have a mechanism to enqueue this edit, then apply when the commit succeeds,
                // possibly discard it when the commit fails.
                throw "Can't delete an entity while previous changes are being committed.";
            } else {
                // If the entity is ClientUpdated, we'll implicitly switch to ClientDeleted.
                // This saves extra app code that would revert the update before allowing the delete.
                this._updateEntityState(id, upshot.EntityState.ClientDeleted);

                ///#DEBUG
                this._verifyConsistency(entity, id);
                ///#ENDDEBUG

                // This treats the case where the entity is already ClientDeleted but an implicit
                // commit failed.  As a convenience, a subsequent deleteEntity call will retry
                // the failed commit.
                this._dataContext.__queueImplicitCommit();
            }

            upshot.__triggerRecompute();

            return this;
        },

        // Internal methods

        __dispose: function () {
            var self = this;
            $.each(this._tracked, function (key, value) {
                self._deleteTracking(value.obj);
            });
            upshot.__deregisterRootEntitySource(this);
            base.dispose.call(this);
        },

        __loadEntities: function (entities) {
            // For each entity, either merge it with a cached entity or add it to the cache.
            var self = this,
            entitiesNewToEntitySet = [],
            indexToInsertClientEntities = this._serverEntities.length;
            // Re: indexToInsertClientEntities, by convention, _clientEntities are layed out as updated followed by
            // added entities.
            var mergedLoadedEntities = $.map(entities, function (entity) {
                return self._loadEntity(entity, entitiesNewToEntitySet);
            });

            if (entitiesNewToEntitySet.length > 0) {
                // Don't trigger a 'reset' here.  What would RemoteDataSources do with such an event?
                // They only have a subset of our entities as the entities they show their clients.  They could
                // only reapply their remote query in response to "refresh".
                obs.insert(this._clientEntities, indexToInsertClientEntities, entitiesNewToEntitySet);
                this._trigger("arrayChanged", "insert", { index: indexToInsertClientEntities, items: entitiesNewToEntitySet });
            }

            return mergedLoadedEntities;
        },

        __getEditedEntities: function () {
            var self = this,
            entities = [];
            $.each(this._entityStates, function (id, state) {
                if (upshot.EntityState.isClientModified(state)) {
                    entities.push(self._getEntityFromId(id));
                }
            });

            return entities;
        },

        __getEntityEdit: function (entity) {
            var id = this.getEntityId(entity),
            self = this,
            submittingState,
            operation,
            addEntityType = function (entityToExtend) {
                return $.extend({ "__type": self._entityType }, entityToExtend);
            };
            switch (this._entityStates[id]) {
                case upshot.EntityState.ClientUpdated:
                    submittingState = upshot.EntityState.ServerUpdating;
                    operation = {
                        Operation: 3,
                        Entity: addEntityType(this._getSerializableEntity(entity)),
                        OriginalEntity: addEntityType(this._getOriginalValue(entity, this._entityType))
                    };
                    break;

                case upshot.EntityState.ClientAdded:
                    submittingState = upshot.EntityState.ServerAdding;
                    entity = this._getAddedEntityFromId(id).entity;
                    operation = {
                        Operation: 2,
                        Entity: addEntityType(this._getSerializableEntity(entity))
                    };
                    break;

                case upshot.EntityState.ClientDeleted:
                    submittingState = upshot.EntityState.ServerDeleting;
                    operation = {
                        Operation: 4,
                        Entity: addEntityType(this._getOriginalValue(entity, this._entityType))
                    };
                    // TODO -- Do we allow for concurrency guards here?
                    break;

                default:
                    throw "Unrecognized entity state.";
            }

            var lastError = self.getEntityError(entity);
            var edit = {
                entityType: this._entityType,
                storeEntity: entity,
                operation: operation,
                updateEntityState: function () {
                    self._updateEntityState(id, submittingState, lastError);

                    ///#DEBUG
                    self._verifyConsistency(entity, id);
                    ///#ENDDEBUG
                },
                succeeded: function (result) {
                    self._handleSubmitSucceeded(id, operation, result);
                },
                failed: function (error) {
                    self._handleSubmitFailed(id, operation, error || lastError);
                }
            };
            return edit;
        },

        __revertChanges: function () {
            var synchronizing;
            $.each(this._entityStates, function (unused, state) {
                if (upshot.EntityState.isServerSyncing(state)) {
                    synchronizing = true;
                    return false;
                }
            });
            if (synchronizing) {
                throw "Can't revert changes to all entities while previous changes are being committed.  Try 'revertChanges(entity)' for entities not presently being committed.";
            }

            var entities = [];
            for (var id in this._entityStates) {
                if (upshot.EntityState.isClientModified(this._entityStates[id])) {
                    entities.push(this._getEntityFromId(id));
                }
            }
            if (entities.length > 0) {
                this.revertChanges(entities);
            }
        },

        __flushEntityStateChangedEvents: function () {
            var self = this;
            $.each(this._deferredEntityStateChangeEvents, function (index, eventArguments) {
                self._trigger.apply(self, ["entityStateChanged"].concat(eventArguments));
            });
            this._deferredEntityStateChangeEvents.splice(0, this._deferredEntityStateChangeEvents.length);
        },

        // Used when AssociatedEntitiesView translates an entity add into a FK property change.
        __setProperty: function (entity, propertyName, value) {
            var eventArguments = { oldValues: {}, newValues: {} };
            eventArguments.oldValues[propertyName] = obs.getProperty(entity, propertyName);
            eventArguments.newValues[propertyName] = value;

            // NOTE: Cribbed from _getTracker/beforeChange.  Keep in sync.
            this._copyOnWrite(entity, eventArguments);
            this._removeTracking(this._getOldFromEvent("change", eventArguments));

            obs.setProperty(entity, propertyName, value);

            // NOTE: Cribbed from _getTracker/afterChange.  Keep in sync.
            this._addTracking([value], upshot.metadata.getPropertyType(this._entityType, propertyName), entity, propertyName);
            this._bubbleChange(entity, null, "", eventArguments);
        },


        // Private methods

        _loadEntity: function (entity, entitiesNewToEntitySet) {
            var identity = this._getEntityIdentity(entity),
                index = this._getEntityIndexFromIdentity(identity);
            if (index >= 0) {
                entity = this._merge(this._serverEntities[index].entity, entity);
            } else {
                var id = identity;  // Ok to use this as an id, as this is a new, unmodified server entity.
                this._addTracking([entity], this._entityType);
                this._entityStates[id] = upshot.EntityState.Unmodified;
                this._serverEntities.push({ entity: entity, identity: id });
                this._updateEntityState(id, upshot.EntityState.Unmodified, null, entity);
                this._addAssociationProperties(entity);

                ///#DEBUG
                this._verifyConsistency(entity, id);
                ///#ENDDEBUG

                entitiesNewToEntitySet.push(entity);
            }
            return entity;
        },

        _handleEntityAdd: function (entity) {
            if (this._getEntityIndex(entity) >= 0 ||  // ...in server entities
                this._getAddedEntityFromEntity(entity)) {  // ...in added entities
                throw "Entity already in data source.";
            }

            var id = upshot.uniqueId("added");
            addedEntity = { entity: entity, clientId: id };
            this._addedEntities.push(addedEntity);
            // N.B.  Entity will already have been added to this._clientEntities, as clients issue CUD operations
            // against this._clientEntities.
            this._addTracking([entity], this._entityType);
            this._entityStates[id] = upshot.EntityState.Unmodified;
            this._updateEntityState(id, upshot.EntityState.ClientAdded);
            this._addAssociationProperties(entity);

            ///#DEBUG
            this._verifyConsistency(entity, id);
            ///#ENDDEBUG

            this._dataContext.__queueImplicitCommit();

            base._handleEntityAdd.apply(this, arguments);
        },

        _changeEntityStateForUpdate: function (entity) {

            var id = this.getEntityId(entity),
                entityState = id !== null && this._entityStates[id];
            if (!entityState) {
                throw "Entity not cached in data context.";
            } else if (entityState === upshot.EntityState.ClientAdded) {
                // Updating a entity that is uncommitted and only on the client.
                // Edit state remains "ClientAdded".  We won't event an edit state change (so clients had
                // better be listening on "change").
                // Fall through and do implicit commit.
            } else if (upshot.EntityState.isServerSyncing(entityState)) {
                // Force the application to block updates while saving any edit to this same entity.
                // We don't have a mechanism to enqueue this edit, then apply when the commit succeeds,
                // possibly discard it when the commit fails.
                throw "Can't update an entity while previous changes are being committed.";
            } else {
                // If this entity is ClientDeleted, we'll implicitly switch to ClientUpdated.
                // This saves extra app code that would revert the delete before allowing updates.
                this._updateEntityState(id, upshot.EntityState.ClientUpdated, this.getEntityError(entity));

                ///#DEBUG
                this._verifyConsistency(entity, id);
                ///#ENDDEBUG
            }

            // This treats the case where the entity is already ClientAdded/ClientUpdated but an implicit
            // commit failed.  As a convenience, a subsequent update will retry the failed commit.
            this._dataContext.__queueImplicitCommit();

            // The caller is responsible for calling upshot.__triggerRecompute().
        },

        _updateEntityState: function (id, state, error, entity) {
            /// <param name="errors" optional="true"></param>
            /// <param name="entity" optional="true"></param>

            entity = entity || this._getEntityFromId(id);  // Notifying after a purge requires that we pass the entity for id.

            var oldState = this._entityStates[id];
            if (this._entityStates[id]) {  // We'll purge the entity before raising "Deleted".
                this._entityStates[id] = state;
                if (oldState !== state) {
                    // TODO: The change event for EntityState won't be deferred here, like it is for _raiseEntityStateChangedEvent.
                    obs.setContextProperty(entity, "entity", "state", state);
                }
            }

            var errorChanged = this._updateEntityError(entity, error);

            if (oldState !== state || errorChanged) {
                // We defer entityStateChange events here so that -- for adds -- they follow 
                // "insert" events for LocalDataSource, AssociationEntitiesView.
                this._deferredEntityStateChangeEvents.push([entity, state, error]);
            }
        },

        // ----------------------------------------
        // | old | new | description              |
        // |-----|-----|--------------------------|
        // |  X  |  X  | no-op                    |
        // |  /  |  X  | remove old from _errors  |
        // |  X  |  /  | add new to _errors       |
        // |  /  |  /  | replace old with new     | 
        // ----------------------------------------
        _updateEntityError: function (entity, newError) {
            var trackingId = this._getTrackingId(entity),
                tracking = this._tracked[trackingId],
                changed;

            var oldIndex = $.inArray(trackingId, this._errors);
            if (oldIndex <= -1) {
                if (newError) {
                    this._errors.push(trackingId);
                    tracking.error = newError;
                    changed = true;
                }
            } else if (newError) {
                if (newError !== tracking.error) {
                    tracking.error = newError;
                    changed = true;
                }
            } else {
                this._errors.splice(oldIndex, 1);
                delete tracking.error;
                changed = true;
            }

            if (changed) {
                obs.setContextProperty(entity, "entity", "error", newError);
            }
            return changed;
        },

        _purgeUncommittedAddedEntity: function (addedEntity) {
            this._purgeEntityInternal(addedEntity.entity, addedEntity.clientId);
        },

        _purgeServerEntity: function (entity, id) {
            this._serverEntities.splice(this._getEntityIndex(entity), 1);
            this._purgeEntityInternal(entity, id);
        },

        _purgeEntityInternal: function (entity, id) {
            var entityState = this._entityStates[id];

            // Do this before the model change, so listeners on data change events see consistent entity state.
            this._updateEntityState(id, upshot.EntityState.Deleted, null, entity);

            // Remove our observable extensions from the entity being purged.
            this._clearChanges(entity);
            this._removeTracking([entity]);
            // Remove this entity from _addedEntities, if it's there.
            for (var i = 0; i < this._addedEntities.length; i++) {
                if (this._addedEntities[i].clientId === id) {
                    this._addedEntities.splice(i, 1);
                    break;
                }
            }
            delete this._entityStates[id];
            this._disposeAssociationEntitiesViews(id);
            this._purgeEntity(entity);  // Superclass method that removes entity from EntitySource.getEntities().

            ///#DEBUG
            this._verifyConsistency(entity, id, true);
            ///#ENDDEBUG
        },

        _getEntityIdentity: function (entity) {
            return upshot.EntitySet.__getIdentity(entity, this._entityType);
        },

        _getEntityIndexFromIdentity: function (identity) {
            var index = -1;
            for (var i = 0; i < this._serverEntities.length; i++) {
                if (this._serverEntities[i].identity === identity) {
                    index = i;
                    break;
                }
            }

            return index;
        },

        _getEntityIndex: function (entity) {
            var index = -1;
            for (var i = 0; i < this._serverEntities.length; i++) {
                if (this._serverEntities[i].entity === entity) {
                    index = i;
                    break;
                }
            }

            return index;
        },

        _addTracking: function (objects, type, parent, property) {
            var self = this;
            $.each(objects, function (index, value) {
                self._addTrackingRecursive(parent, property, value, type);
            });
        },

        _addTrackingRecursive: function (parent, property, obj, type) {
            if (upshot.isArray(obj) || upshot.isObject(obj)) {
                var tracking = this._getTracking(obj);
                if (tracking) {
                    if (tracking.active) {
                        throw "Value is already tracked";
                    }
                } else {
                    var trackingId = upshot.cache(obj, "trackingId", upshot.uniqueId("tracking"));
                    tracking = this._tracked[trackingId] = {};
                }

                tracking.obj = obj;
                tracking.type = type;
                tracking.parentId = this._getTrackingId(parent) || null;
                tracking.property = upshot.isArray(parent) ? null : property;
                tracking.active = true;
                tracking.changes = tracking.changes || null;

                obs.track(obj, this._getTracker(), this._entityType);

                if (upshot.isArray(obj)) {
                    // Primitive values don't get mapped.  Avoid iteration over the potentially large array.
                    // TODO: This precludes heterogeneous arrays.  Should we test for primitive element type here instead?
                    if (!upshot.isValueArray(obj)) {
                        // Since we're recursing through the entity, we won't need to use asArray on collection-typed properties
                        var self = this;
                        $.each(obj, function (index, value) {
                            self._addTrackingRecursive(obj, index, value, type);
                        });
                    }
                } else {
                    var self = this;
                    $.each(upshot.metadata.getProperties(obj, type), function (index, prop) {
                        self._addTrackingRecursive(obj, prop.name, obs.getProperty(obj, prop.name), prop.type);
                    });
                }
            }
        },

        _getTracker: function () {
            if (this._tracker === null) {
                var self = this;
                this._tracker = {
                    beforeChange: function (target, type, eventArguments) {
                        if (!self._isAssociationPropertySet(target, type, eventArguments)) {
                            self._copyOnWrite(target, eventArguments);
                            self._removeTracking(self._getOldFromEvent(type, eventArguments));
                        }
                    },
                    afterChange: function (target, type, eventArguments) {
                        upshot.__beginChange();
                        if (!self._handleAssociationPropertySet(target, type, eventArguments)) {
                            var tracking = self._getTracking(target);
                            if (type === "change") {
                                $.each(eventArguments.newValues, function (key, value) {
                                    self._addTracking([value], upshot.metadata.getPropertyType(tracking.type, key), target, key);
                                });
                            } else {
                                self._addTracking(self._getNewFromEvent(type, eventArguments), tracking.type, target);
                            }
                            self._bubbleChange(target, null, [], eventArguments);
                        }
                    },
                    afterEvent: function (target, type, eventArguments) {
                        upshot.__endChange();
                    }
                };
                if (this._dataContext.__manageAssociations) {
                    this._tracker.includeAssociations = true;
                }
            }
            return this._tracker;
        },

        _isAssociationPropertySet: function (target, type, eventArguments) {
            if (type === "change" && this._dataContext.__manageAssociations && this._getTracking(target).parentId === null) {
                var fieldsMetadata = (upshot.metadata(this._entityType) || {}).fields;
                if (fieldsMetadata) {
                    for (var fieldName in fieldsMetadata) {
                        var fieldMetadata = fieldsMetadata[fieldName];
                        if (fieldMetadata.association &&
                            (fieldName in eventArguments.oldValues || fieldName in eventArguments.newValues)) {

                            // TODO: Treat case when oldValues/newValues contains multiple property names.
                            if (this._getOldFromEvent(type, eventArguments).length > 1 ||
                                this._getNewFromEvent(type, eventArguments).length > 1) {
                                throw "NYI -- Can't include association properties in N>1 property sets.";
                            }

                            return true;
                        }
                    }
                }
            }

            return false;
        },

        _handleAssociationPropertySet: function (target, type, eventArguments) {
            if (!this._isAssociationPropertySet(target, type, eventArguments)) {
                return false;
            }
            // TODO: Throw an exception when someone tries to replace the array under a child entities property.

            var id = this.getEntityId(target);
            if (id === null || !(id in this._entityStates)) {
                throw "Entity not cached in data context.";
            }

            // Determine the single property whose value is being set.
            var fieldName;
            for (var fieldNameT in eventArguments.oldValues) {
                fieldName = fieldNameT;
                break;
            }
            if (!fieldName) {
                for (var fieldNameT in eventArguments.newValues) {
                    fieldName = fieldNameT;
                    break;
                }
            }

            var associatedEntitiesViews = this._associatedEntitiesViews[id],
                associatedEntitiesView = associatedEntitiesViews[fieldName];
            associatedEntitiesView.__handleParentPropertySet(eventArguments.newValues[fieldName]);

            return true;
        },

        _removeTracking: function (objects) {
            var self = this;
            $.each(objects, function (index, value) {
                self._removeTrackingRecursive(value);
            });
        },

        _removeTrackingRecursive: function (obj) {
            if (upshot.isArray(obj) || upshot.isObject(obj)) {
                var tracking = this._getTracking(obj);
                if (tracking) {
                    if (tracking.changes === null) {
                        this._deleteTracking(obj);
                    } else {
                        tracking.active = false;
                    }

                    obs.track(obj, null);

                    if (upshot.isArray(obj)) {
                        // Primitive values don't get mapped.  Avoid iteration over the potentially large array.
                        // TODO: This precludes heterogeneous arrays.  Should we test for primitive element type here instead? 
                        if (!upshot.isValueArray(obj)) {
                            var self = this;
                            $.each(obj, function (index, value) {
                                self._removeTrackingRecursive(value);
                            });
                        }
                    } else {
                        var self = this;
                        $.each(obj, function (key) {
                            self._removeTrackingRecursive(obs.getProperty(obj, key));
                        });
                    }
                }
            }
        },

        _deleteTracking: function (obj) {
            var trackingId = this._getTrackingId(obj);

            upshot.deleteCache(obj, "trackingId");

            delete this._tracked[trackingId];
        },

        _getOldFromEvent: function (type, eventArguments) {
            if (type === "change") {
                // TODO -- add test coverage for N-property updates
                var old = [];
                $.each(eventArguments.oldValues, function (key, value) {
                    old.push(value);
                });
                return old;
            } else {
                if (type === "remove") {
                    return eventArguments.items;
                } else if (type === "replaceAll") {
                    return eventArguments.oldItems;
                }
            }
            return [];
        },

        _getNewFromEvent: function (type, eventArguments) {
            if (type === "change") {
                // TODO -- add test coverage for N-property updates
                var _new = [];
                $.each(eventArguments.newValues, function (key, value) {
                    _new.push(value);
                });
                return _new;
            } else {
                if (type === "insert") {
                    return eventArguments.items;
                } else if (type === "replaceAll") {
                    return eventArguments.newItems;
                }
            }
            return [];
        },

        _getTrackingId: function (obj) {
            return upshot.cache(obj, "trackingId");
        },

        _getTracking: function (obj) {
            var trackingId = this._getTrackingId(obj);
            return trackingId ? this._tracked[trackingId] : null;
        },

        _getChanges: function (obj) {
            var tracking = this._getTracking(obj);
            return tracking ? this._getTracking(obj).changes : null;
        },

        _bubbleChange: function (obj, child, path, eventArguments) {
            var tracking = this._getTracking(obj);

            if (child) {
                this._recordChangedChild(obj, child);
            }

            if (tracking.parentId === null) {
                if (path.length > 1) {
                    // strip the '.' off the path
                    path = path.slice(1);
                }
                this._handlePropertyChange(obj, eventArguments, child === null);
                this._triggerEntityUpdated(obj, path, eventArguments);
            } else {
                var parent = this._tracked[tracking.parentId].obj;
                path = (tracking.property ? "." + tracking.property : "[" + $.inArray(obj, parent) + "]") + path;
                this._bubbleChange(parent, obj, path, eventArguments);
            }
        },

        _copyOnWrite: function (obj, eventArguments) {
            if (upshot.isArray(obj)) {
                this._recordWriteToArray(obj);
            } else {
                this._recordWriteToObject(obj, eventArguments.oldValues);
            }
        },

        _recordWriteToArray: function (array) {
            var tracking = this._getTracking(array),
                changes = tracking.changes || (tracking.changes = {
                    original: null,
                    children: {}
                });

            changes.original || (changes.original = array.slice(0));
        },

        _recordWriteToObject: function (obj, oldValues) {
            var tracking = this._getTracking(obj),
                changes = tracking.changes || (tracking.changes = {
                    original: {},
                    children: {}
                });

            $.each(oldValues, function (key, value) {
                changes.original.hasOwnProperty(key) || (changes.original[key] = value);
                obs.setContextProperty(obj, "property", key, true);
            });
        },

        _recordChangedChild: function (obj, child) {
            upshot.isArray(obj) ?
                this._recordChangedChildOfArray(obj, child) :
                this._recordChangedChildOfObject(obj, child);
        },

        _recordChangedChildOfArray: function (array, child) {
            var tracking = this._getTracking(array),
                changes = tracking.changes || (tracking.changes = {
                    original: null,
                    children: {}
                }),
                childId = this._getTrackingId(child);

            changes.children[childId] = childId;
        },

        _recordChangedChildOfObject: function (obj, child) {
            var tracking = this._getTracking(obj),
                changes = tracking.changes || (tracking.changes = {
                    original: {},
                    children: {}
                }),
                childId = this._getTrackingId(child);

            changes.children[childId] = childId;
        },

        _clearChanges: function (obj, revert) {
            var tracking = this._getTracking(obj),
                changes = tracking.changes,
                snapshot = { original: {}, children: {} };

            if (changes) {
                if (upshot.isArray(obj)) {
                    snapshot.original = changes.original;
                } else {
                    $.each(changes.original, function (key, value) {
                        snapshot.original[key] = value;
                        delete changes.original[key];
                        obs.setContextProperty(obj, "property", key, false);
                    });
                }

                var self = this;
                $.each(changes.children, function (key, value) {
                    var childTracking = self._tracked[value];
                    snapshot.children[value] = self._clearChanges(childTracking.obj, revert);
                });

                tracking.changes = null;
                if (!revert && !tracking.active) {
                    this._deleteTracking(obj);
                }
            }
            return snapshot;
        },

        _clearChangesOnPath: function (objs, tokens, skipChildren) { // This is only used for revert scenarios
            // objs) [obj, A, B], [obj, A, B, B[0]], [obj, "str"]
            // tokens) ["A", "B"], ["A", "B", "0"], ["C"]
            var obj = objs.shift(),
                property = tokens.shift(),
                tracking = this._getTracking(obj),
                changes = tracking.changes,
                snapshot = { original: {}, children: {} };

            if (changes) {
                if (tokens.length === 0) {
                    var children = [objs[0]];
                    if (changes.original.hasOwnProperty(property)) {
                        children.push(changes.original[property]);
                        snapshot.original[property] = children[1];
                        delete changes.original[property];
                        obs.setContextProperty(obj, "property", property, false);
                    }

                    if (!skipChildren) {
                        var self = this;
                        $.each(children, function (index, child) {
                            var childId = self._getTrackingId(child);
                            if (childId && changes.children.hasOwnProperty(childId)) {
                                snapshot.children[childId] = self._clearChanges(child, true);
                                delete changes.children[childId];
                            }
                        });
                    }
                } else {
                    var childId = this._getTrackingId(objs[0]);
                    if (childId && changes.children.hasOwnProperty(childId)) {
                        snapshot.children[childId] = this._clearChangesOnPath(objs, tokens, skipChildren);
                        if (this._tracked[childId].changes === null) {
                            delete changes.children[childId];
                        }
                    }
                }

                if (upshot.isEmpty(changes.original) && upshot.isEmpty(changes.children)) {
                    tracking.changes = null;
                }
            }
            return snapshot;
        },

        _getOriginalValue: function (obj, type, property) {
            if (upshot.isArray(obj) && property) {
                throw "property is not a supported parameter when getting original array values";
            }

            var self = this,
                changes = this._getChanges(obj),
                original;

            if (property) {
                return changes && changes.original.hasOwnProperty(property) ?
                    changes.original[property] :
                    obs.getProperty(obj, property);
            } else if (upshot.isArray(obj)) {
                original = (changes ? changes.original || obj : obj).slice(0);
                $.each(original, function (index, value) {
                    original[index] = self._getOriginalValue(value, type);
                });
                return original;
            } else if (upshot.isObject(obj)) {
                original = {};
                $.each(upshot.metadata.getProperties(obj, type), function (index, prop) {
                    var value = self._getOriginalValue(obj, type, prop.name);
                    original[prop.name] = self._getOriginalValue(value, prop.type);
                });
                return original;
            }
            return obj;
        },

        _restoreOriginalValues: function (obj, type, changes) {
            if (changes) {
                var self = this;
                if (upshot.isArray(obj)) {
                    if (changes.original !== null) {
                        self._arrayRefresh(obj, type, changes.original);
                    }
                } else {
                    $.each(changes.original, function (key, value) {
                        self._setProperty(obj, upshot.metadata.getPropertyType(type, key), key, value);
                    });
                }

                $.each(changes.children, function (key, value) {
                    var tracking = self._tracked[key];
                    self._restoreOriginalValues(tracking.obj, tracking.type, value);
                });
            }
        },

        _merge: function (entity, _new) {
            if (!this.isUpdated(entity)) {
                // Only merge entities without changes
                this._mergeObject(entity, _new, this._entityType);
            }
            return entity;
        },

        _mergeObject: function (obj, _new, type) {
            ///#DEBUG
            // TODO: For unmanaged associations, we'll need to descend into associations below to
            // pick up child association membership changes and parent association value changes.
            upshot.assert(this._dataContext.__manageAssociations);
            ///#ENDDEBUG

            var self = this;
            $.each(upshot.metadata.getProperties(_new, type, false /* see TODO above */), function (index, prop) {
                var oldValue = obs.getProperty(obj, prop.name),
                    value = obs.getProperty(_new, prop.name);
                if (oldValue !== value) {
                    if (upshot.classof(oldValue) === upshot.classof(value)) {
                        // We only try to deep-merge when classes match
                        if (upshot.isArray(oldValue)) {
                            if (!upshot.isValueArray(oldValue)) {
                                self._mergeArray(oldValue, value, prop.type);
                            }
                            return;
                        } else if (upshot.isObject(oldValue)) {
                            self._mergeObject(oldValue, value, prop.type);
                            return;
                        }
                    }
                    self._setProperty(obj, type, prop.name, value);
                }
            });
        },

        _mergeArray: function (array, _new, type) {
            var self = this;
            $.each(_new, function (index, value) {
                var oldValue = array[index];
                if (oldValue) {
                    self._mergeObject(oldValue, value, type);
                }
            });
            if (array.length > _new.length) {
                this._arrayRemove(array, type, _new.length, array.length - _new.length);
            } else if (array.length < _new.length) {
                this._arrayInsert(array, type, array.length, _new.slice(array.length));
            }
        },

        _handlePropertyChange: function (entity, eventArguments, raisePropertyChanged) {
            // TODO, suwatch: to support N properties
            var path, value;
            for (var propertyName in eventArguments.newValues) {
                if (eventArguments.newValues.hasOwnProperty(propertyName)) {
                    if (path) {
                        throw "NYI -- Can only update one property at a time.";
                    }
                    path = propertyName;
                    value = eventArguments.newValues[propertyName];
                }
            }

            if (path === "") {
                // Data-linking sends all <input> changes to the linked object.
                return;
            }

            if (raisePropertyChanged) {
                this._trigger("propertyChanged", entity, path, value);
                // Issue the "entity change" event prior to any related "entity state changed" event below...
            }

            this._changeEntityStateForUpdate(entity);
        },

        _setProperty: function (obj, type, key, value) {
            var parentId = this._getTracking(obj).parentId;

            this._removeTracking([obs.getProperty(obj, key)]);
            obs.setProperty(obj, key, value);  // TODO: Shouldn't we be _addTracking before obs.setProperty, so we won't be reentered in a inconsistent state?
            this._addTracking([value], upshot.metadata.getPropertyType(type, key), obj, key);
            if (parentId === null) {
                // Only raise "propertyChanged" for entity-level property changes.
                this._trigger("propertyChanged", obj, key, value);
            }
        },

        _arrayRefresh: function (array, type, values) {
            this._removeTracking(array);
            obs.refresh(array, values);
            this._addTracking(array, type, array);
        },

        _arrayRemove: function (array, type, index, numToRemove) {
            this._removeTracking(array.slice(index, numToRemove));
            obs.remove(array, index, numToRemove);
        },

        _arrayInsert: function (array, type, index, items) {
            obs.insert(array, index, items);
            this._addTracking(items, type, array);
        },

        _triggerEntityUpdated: function (entity, path, eventArguments) {
            // Only raise events when the entity is updated or being reverted or accepted
            if (this.isUpdated(entity) || !eventArguments) {
                this._trigger("entityUpdated", entity, path, eventArguments);
            }
        },

        _getAddedEntityFromId: function (id) {
            var addedEntities = $.grep(this._addedEntities, function (addedEntity) { return addedEntity.clientId === id; });
            ///#DEBUG
            upshot.assert(addedEntities.length <= 1);
            ///#ENDDEBUG
            return addedEntities[0];
        },

        _getAddedEntityFromEntity: function (entity) {
            var addedEntities = $.grep(this._addedEntities, function (addedEntity) { return addedEntity.entity === entity; });
            ///#DEBUG
            upshot.assert(addedEntities.length <= 1);
            ///#ENDDEBUG
            return addedEntities[0];
        },

        _handleSubmitSucceeded: function (id, operation, result) {
            var entity = this._getEntityFromId(id);  // ...before we purge.

            switch (operation.Operation) {
                case 2:
                    // Do this before the model change, so listeners on data change events see consistent entity state.
                    this._updateEntityState(id, upshot.EntityState.Unmodified);

                    // Keep entity in addedEntities to maintain its synthetic id as the client-known id.
                    this._getAddedEntityFromId(id).committed = true;

                    this._serverEntities.push({ entity: entity, identity: this._getEntityIdentity(result.Entity) });
                    this._clearChanges(entity, false);
                    this._merge(entity, result.Entity);

                    ///#DEBUG
                    this._verifyConsistency(entity, id, false, true);
                    ///#ENDDEBUG

                    break;

                case 3:
                    // Do this before the model change, so listeners on data change events see consistent entity state.
                    this._updateEntityState(id, upshot.EntityState.Unmodified);

                    this._clearChanges(entity, false);
                    this._merge(entity, result.Entity);
                    this._triggerEntityUpdated(entity);

                    ///#DEBUG
                    this._verifyConsistency(entity, id);
                    ///#ENDDEBUG

                    break;

                case 4:
                    this._purgeServerEntity(entity, id);  // This updates entity state to Deleted, verifies consistency.
                    break;
            }

            ///#DEBUG
            upshot.assert(!this.getEntityError(entity), "must not have error!");
            ///#ENDDEBUG
        },

        _handleSubmitFailed: function (id, operation, error) {
            var entity = this._getEntityFromId(id);

            var state;
            switch (operation.Operation) {
                case 2: state = upshot.EntityState.ClientAdded; break;
                case 3: state = upshot.EntityState.ClientUpdated; break;
                case 4: state = upshot.EntityState.ClientDeleted; break;
            }

            this._updateEntityState(id, state, error);

            ///#DEBUG
            this._verifyConsistency(entity, id);
            ///#ENDDEBUG
        },

        _getEntityFromId: function (id) {
            var addedEntity = this._getAddedEntityFromId(id);
            if (addedEntity) {
                // 'id' is one of our synthesized ones for client-added entities.
                return addedEntity.entity;
            } else {
                // 'id' is one computed based on server identity, which is assumed to be immutable.
                for (var i = 0; i < this._serverEntities.length; i++) {
                    if (this._serverEntities[i].identity === id) {
                        return this._serverEntities[i].entity;
                    }
                }
            }
        },

        _addAssociationProperties: function (entity) {
            if (!this._dataContext.__manageAssociations) {
                return;
            }

            var fieldsMetadata = (upshot.metadata(this._entityType) || {}).fields;
            if (fieldsMetadata) {
                var id = this.getEntityId(entity);
                ///#DEBUG
                upshot.assert(id !== null && id in this._entityStates, "Entity should be cached in data context.");
                ///#ENDDEBUG

                var associatedEntitiesViews = this._associatedEntitiesViews[id] = {},
                    self = this;
                $.each(fieldsMetadata, function (fieldName, fieldMetadata) {
                    if (fieldMetadata.association) {
                        if (associatedEntitiesViews[fieldName]) {
                            throw "Duplicate property metadata for property '" + fieldName + "'.";
                        }
                        associatedEntitiesViews[fieldName] = self._createAssociatedEntitiesView(entity, fieldName, fieldMetadata);
                    }
                });
            }
        },

        _createAssociatedEntitiesView: function (entity, fieldName, fieldMetadata) {
            if (!fieldMetadata.association.isForeignKey && !fieldMetadata.array) {
                // TODO -- Singleton child entities?
                throw "NYI: Singleton child entities are not currently supported";
            }

            var targetEntitySet = this._dataContext.getEntitySet(fieldMetadata.type),
                isForeignKey = fieldMetadata.association.isForeignKey,
                parentPropertySetter = isForeignKey ? function () {  // AssociatedEntitiesView for a parent property needs a function to do the property set.
                    var oldParent = obs.getProperty(entity, fieldName),
                    newParent = obs.asArray(this.getEntities())[0] || null;  // TODO: What if there are N>1 parent entities?
                    if (oldParent !== newParent) {
                        // TODO: For KO, this won't be an observable set if KO's map didn't already establish a observable property here.  Is this ok?
                        obs.setProperty(entity, fieldName, newParent);
                        this._trigger("propertyChanged", entity, fieldName, newParent);
                    }
                } : null,
                parentEntitySet = isForeignKey ? targetEntitySet : this,
                childEntitySet = isForeignKey ? this : targetEntitySet;

            var result;
            if (fieldMetadata.array) {
                // TODO: We can't reuse an existing KO observable array here.  Without more work, that will double-track
                // the reused KO observable array (once, during obs.track of entity...a second time in the EntitySource ctor).
                result = obs.createCollection();

                // TODO: KO's obs.setProperty doesn't do what we want here.  It will set an already existing KO observable array to
                // have a value which is _another_ observable array.
                entity[fieldName] = result;
            }

            return new upshot.AssociatedEntitiesView(entity, parentEntitySet, childEntitySet, fieldMetadata.association, parentPropertySetter, result);
        },

        _disposeAssociationEntitiesViews: function (id) {
            var associatedEntitiesViews = this._associatedEntitiesViews[id];
            if (associatedEntitiesViews) {
                $.each(associatedEntitiesViews, function (unused, associatedEntitiesView) {
                    associatedEntitiesView.dispose();
                });
            }
            delete this._associatedEntitiesViews[id];
        },

        ///#DEBUG
        _verifyConsistency: function (entity, id, isPurged, isNewlyCommittedAdd) {
            var entityState = this._entityStates[id];
            upshot.assert(isPurged || entityState, "Entities in EntitySet always have an entity state.");
            upshot.assert(entityState !== upshot.EntityState.Deleted,
                   "The Deleted entity state is only for the 'entityStateChanged' event.  It's never in _entityStates.");
            entityState = entityState || upshot.EntityState.Deleted;

            // isNewlyCommittedAdded is only supplied as true for a client-added entity that has
            // just been committed (and is now in the Unmodified state).
            upshot.assert(!isNewlyCommittedAdd || entityState === upshot.EntityState.Unmodified);

            var addedEntity = this._getAddedEntityFromId(id);
            upshot.assert(!(isNewlyCommittedAdd || entityState === upshot.EntityState.ClientAdded || entityState === upshot.EntityState.ServerAdding) ||
                   addedEntity,
                   "Client-added entities are always tracked in _addedEntities");

            var committedAdd = addedEntity && addedEntity.committed;
            upshot.assert(!committedAdd || !upshot.EntityState.isAdded(entityState),
                   "Committed, client-added entities should never be in the ClientAdded/ServerAdding states");

            var isInServerEntities = this._getEntityIndex(entity) >= 0;
            upshot.assert(!committedAdd || isInServerEntities, "Committed, client-added entities should always be in _serverEntities");

            var uncommittedAdd = addedEntity && !addedEntity.committed;
            upshot.assert(!uncommittedAdd || !isInServerEntities, "Uncommitted, client-added entities should never be in _serverEntities");

            upshot.assert(entityState !== upshot.EntityState.Deleted ||
                   !(isInServerEntities || addedEntity || (id in this._entityStates) || this.getEntityError(entity)),
                  "Deleted/purged entities should never be found in EntitySet");

            upshot.assert(entityState === upshot.EntityState.Deleted || this.getEntityId(entity) === id,
                   "Entities in an entity set always have a non-null, stable id");

            upshot.assert(!(entityState === upshot.EntityState.Unmodified || entityState === upshot.EntityState.Deleted) ||
                   !this.getEntityError(entity),
                   "Unmodified and Deleted/purged entities should never have errors");

            upshot.assert(upshot.EntityState.isUpdated(entityState) === this.isUpdated(entity),
                          "An entity 'isUpdated' iff ClientUpdated/ServerUpdating");
        },
        ///#ENDDEBUG

        _getSerializableEntity: function (entity) {
            if (!this._dataContext.__manageAssociations) {
                return entity;
            }

            var sanitizedEntity = {};
            $.each(upshot.metadata.getProperties(entity, this._entityType), function (index, property) {
                sanitizedEntity[property.name] = entity[property.name];
            });
            return sanitizedEntity;
        }
    };

    var classMembers = {
        __getIdentity: function (entity, entityType) {
            // Produce a unique identity string for the given entity, based on simple
            // concatenation of key values.
            var metadata = upshot.metadata(entityType);
            if (!metadata) {
                throw "No metadata available for type '" + entityType + "'.  Register metadata using 'upshot.metadata(...)'.";
            }
            var keys = metadata.key;
            if (!keys) {
                throw "No key metadata specified for entity type '" + entityType + "'";
            }

            // optimize for the common single part key case
            if (keys.length == 1 && (keys[0].indexOf('.') == -1)) {
                var keyMember = keys[0];
                upshot.EntitySet.__validateKeyMember(keyMember, keyMember, entity, entityType);
                return obs.getProperty(entity, keyMember).toString();
            }

            var identity = "";
            $.each(keys, function (index, key) {
                if (identity.length > 0) {
                    identity += ",";
                }

                // support dotted paths
                var parts = key.split(".")
                var value = entity;
                $.each(parts, function (index, part) {
                    upshot.EntitySet.__validateKeyMember(part, key, value, entityType);
                    value = obs.getProperty(value, part);
                });

                identity += value;
            });
            return identity;
        },

        __validateKeyMember: function (keyMember, fullKey, entity, entityType) {
            if (!entity || !(keyMember in entity)) {
                throw "Key member '" + fullKey + "' doesn't exist on entity type '" + entityType + "'";
            }
        }
    };

    upshot.EntitySet = upshot.deriveClass(base, ctor, instanceMembers);

    $.extend(upshot.EntitySet, classMembers);
}
///#RESTORE )(this, jQuery, upshot);
