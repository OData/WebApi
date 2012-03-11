/// <reference path="IntelliSense\References.js" />
///#RESTORE (function (global, $, upshot, undefined)
{
    var base = upshot.EntityView.prototype;

    var obs = upshot.observability;

    var ctor = function (entity, parentEntitySet, childEntitySet, associationMetadata, parentPropertySetter, result) {
        this._entity = entity;
        this._parentEntitySet = parentEntitySet;
        this._childEntitySet = childEntitySet;
        this._associationMetadata = associationMetadata;
        this._parentPropertySetter = parentPropertySetter;

        // The EntityView base class observes its "source" option (which is the target entity set) for 
        // array- and property-changes.
        // Additionally, we need to observe property changes on the source entity set to catch:
        // - FK property changes that would affect a parent association property
        // - PK (non-FK) property changes that would affect a child association property
        var self = this;
        this._sourceEntitySetObserver = function (entity, property, newValue) {
            if (!self._needRecompute &&
                $.inArray(property, associationMetadata.thisKey) >= 0) {
                self._setNeedRecompute();
            }
        };
        var sourceEntitySet = associationMetadata.isForeignKey ? childEntitySet : parentEntitySet;
        sourceEntitySet.bind("propertyChanged", this._sourceEntitySetObserver);

        var entitySource = associationMetadata.isForeignKey ? parentEntitySet : childEntitySet;
        base.constructor.call(this, { source: entitySource, result: result });

        // We only ever instantiate AssociatedEntitiesViews when adding entities to 
        // an EntitySet, which always ends with a recompute.
        this._initialized = false;
        this._setNeedRecompute();
    };

    var instanceMembers = {

        // Internal methods

        // This is called from EntitySet.js as it treats tracked changes to parent association
        // properties on child entities.
        __handleParentPropertySet: function (parentEntity) {
            this._handleRelationshipEdit(this._entity, parentEntity);
        },

        // Private methods

        _dispose: function () {
            var sourceEntitySet = this._associationMetadata.isForeignKey ? this._childEntitySet : this._parentEntitySet;
            sourceEntitySet.unbind("propertyChanged", this._sourceEntitySetObserver);
            base._dispose.apply(this, arguments);
        },

        _handleEntityAdd: function (entity) {
            this._handleRelationshipEdit(entity, this._entity);
        },

        // Do the appropriate EntitySet adds and FK property changes to reflect an editied relationship
        // between childEntity and parentEntity.
        _handleRelationshipEdit: function (childEntity, parentEntity) {
            var associationMetadata = this._associationMetadata,
                isForeignKey = associationMetadata.isForeignKey,
                parentKeyValue;
            if (!parentEntity) {
                parentKeyValue = null;
            } else {
                if ($.inArray(parentEntity, obs.asArray(this._parentEntitySet.getEntities())) < 0) {
                    // TODO -- Should this implicitly add the parent entity?  I doubt it.
                    throw "Parent entity is not in the parent entity set for this association.";
                } else if ((this._parentEntitySet.getEntityState(parentEntity) || "").indexOf("Add") > 0) {
                    // TODO -- Add support for added parent entities without an established key value, fix-up after commit.
                    throw "NYI -- Cannot set foreign keys to key values computed from added entities.  Commit the parent entity first.";
                }

                var parentKey = isForeignKey ? associationMetadata.otherKey : associationMetadata.thisKey;
                parentKeyValue = obs.getProperty(parentEntity, parentKey[0]);  // TODO -- Generalize to N fields.
                if (parentKeyValue === undefined) {
                    throw "Parent entity has no value for its '" + parentKey[0] + "' key property.";
                }
            }

            var childKey = isForeignKey ? associationMetadata.thisKey : associationMetadata.otherKey,
                childKeyValue = obs.getProperty(childEntity, childKey[0]),  // TODO -- Generalize to N fields.
                setForeignKeyValue;
            if (!parentEntity) {
                if (childKeyValue !== null) {
                    setForeignKeyValue = true;
                }
            } else if (childKeyValue === undefined || childKeyValue !== parentKeyValue) {
                setForeignKeyValue = true;
            }

            var isAddToChildEntities = !isForeignKey;
            if (isAddToChildEntities && $.inArray(childEntity, obs.asArray(this._entitySource.getEntities())) < 0) {
                // Base class will translate add to child entities into an add on our input EntitySet.
                base._handleEntityAdd.call(this, childEntity);
            }

            if (setForeignKeyValue) {
                // Do this after the entitySet add above.  That way, the property change will be observable by clients
                // interested in childEntitiesCollection or the EntitySet.
                // Likewise, above, we will have done obs.track (as part of adding to the EntitySet) before 
                // obs.setProperty, in case establishing observable proxies is done implicitly w/in setProperty
                // (as WinJS support does).
                this._childEntitySet.__setProperty(childEntity, childKey[0], parentKeyValue);  // TODO -- Generalize to N fields.
            }
        },

        _onPropertyChanged: function (entity, property, newValue) {
            if (!this._needRecompute &&
                $.inArray(property, this._associationMetadata.otherKey) >= 0) {
                this._setNeedRecompute();
            }
            base._onPropertyChanged.apply(this, arguments);
        },

        _onArrayChanged: function (type, eventArgs) {
            if (this._needRecompute) {
                return;
            }

            var needRecompute;
            switch (type) {
                case "insert":
                case "remove":
                    var self = this;
                    $.each(eventArgs.items, function (index, entity) {
                        if (self._haveEntity(entity) ^ type === "insert") {
                            needRecompute = true;
                            return false;
                        }
                    });
                    break;

                case "replaceAll":
                    needRecompute = true;
                    break;

                default:
                    throw "NYI -- Array operation '" + type + "' is not supported.";
            }

            if (needRecompute) {
                this._setNeedRecompute();
            }
        },

        _recompute: function () {
            var clientEntities = this._clientEntities,
                newEntities = this._computeAssociatedEntities();

            if (!this._initialized) {
                this._initialized = true;

                if (newEntities.length > 0) {  // Don't event a replaceAll if we're not actually modifying the entities array.
                    var oldEntities = obs.asArray(clientEntities).slice();  // Here, assume a live array.  It will be for jQuery compat.
                    obs.refresh(clientEntities, newEntities);
                    this._trigger("arrayChanged", "replaceAll", { oldItems: oldEntities, newItems: obs.asArray(clientEntities) });
                }
            } else {
                // Perform adds/removes on clientEntities to have it reflect the same membership
                // as newEntities.  Issue change events for the adds/removes.
                // Don't try to preserve ordering between clientEntities and newEntities.
                // Assume that obs.asArray returns a non-live array instance.  It will be for Knockout compat.
                // Don't cache obs.asArray(clientEntities) below.
                var self = this;

                var addedEntities = $.grep(newEntities, function (entity) {
                    return $.inArray(entity, obs.asArray(clientEntities)) < 0;
                });
                $.each(addedEntities, function (unused, entity) {
                    var index = obs.asArray(clientEntities).length,
                        items = [entity];
                    obs.insert(clientEntities, index, items);
                    self._trigger("arrayChanged", "insert", { index: index, items: items });
                });

                var removedEntities = $.grep(obs.asArray(clientEntities), function (entity) {
                    return $.inArray(entity, newEntities) < 0;
                });
                $.each(removedEntities, function (unused, entity) {
                    var indexRemove = $.inArray(entity, obs.asArray(clientEntities));
                    obs.remove(clientEntities, indexRemove, 1);
                    self._trigger("arrayChanged", "remove", { index: indexRemove, items: [entity] });
                });
            }

            if (this._parentPropertySetter) {
                // EntitySet.js has supplied a handler with which to make observable changes
                // to a parent association property on a child entity.
                this._parentPropertySetter.apply(this);
            }
        },

        _computeAssociatedEntities: function () {
            var entity = this._entity,
                associationMetadata = this._associationMetadata,
                sourceKeyValue = obs.getProperty(entity, associationMetadata.thisKey[0]),  // TODO -- Generalize to N fields.
                targetEntitySet = associationMetadata.isForeignKey ? this._parentEntitySet : this._childEntitySet,
                targetEntities = obs.asArray(targetEntitySet.getEntities()),
                targetKey = associationMetadata.otherKey,
                associatedEntities = [];
            for (var i = 0; i < targetEntities.length; i++) {
                var targetEntity = targetEntities[i],
                    targetKeyValue = obs.getProperty(targetEntity, targetKey[0]);  // TODO -- Generalize to N fields.
                if (targetKeyValue !== undefined && targetKeyValue === sourceKeyValue) {
                    associatedEntities.push(targetEntity);
                }
            }
            return associatedEntities;
        }

        // TODO -- Make array removals from "_clientEntities" null out foreign key values.
    };

    upshot.AssociatedEntitiesView = upshot.deriveClass(base, ctor, instanceMembers);
}
///#RESTORE )(this, jQuery, upshot);
