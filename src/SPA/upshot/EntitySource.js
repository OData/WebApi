/// <reference path="IntelliSense\References.js" />
///#RESTORE (function (global, $, upshot, undefined)
{
    var obs = upshot.observability;

    var ctor = function (options) {
        var result = options && options.result;
        if (result) {
            if (upshot.EntitySource.as(result)) {
                throw "Array is already bound to an EntitySource.";
            }
        }

        this._viewsToRecompute = [];
        this._eventCallbacks = {};
        this._clientEntities = result || obs.createCollection();

        // Events shared by subclasses
        this._bindFromOptions(options, [ "arrayChanged", "propertyChanged", "entityStateChanged" ]);

        var self = this;
        obs.track(this._clientEntities, {
            afterChange: function (array, type, eventArguments) {
                upshot.__beginChange();
                self._handleArrayChange(type, eventArguments);
            },
            afterEvent: function () {
                upshot.__endChange();
            }
        });

        upshot.cache(this._clientEntities, "entitySource", this);
    };

    var instanceMembers = {

        // Public methods

        dispose: function () {
            /// <summary>
            /// Disposes the EntitySource instance.
            /// </summary>

            if (this._eventCallbacks) {  // Use _eventCallbacks as an indicator as to whether we've been disposed.
                obs.track(this._clientEntities, null);
                upshot.deleteCache(this._clientEntities, "entitySource");
                this._dispose();  // Give subclass code an opportunity to clean up.
                this._eventCallbacks = null;
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
            /// <returns type="upshot.EntitySource"/>

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
            /// <returns type="upshot.EntitySource"/>

            if (typeof event === "string") {
                var list = this._eventCallbacks && this._eventCallbacks[event];
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

        getEntities: function () {
            /// <summary>
            /// Returns the stable, observable array of model data.
            /// </summary>
            /// <returns type="Array"/>

            return this._clientEntities;
        },


        // Internal methods

        __registerForRecompute: function (entityView) {
            if ($.inArray(entityView, this._viewsToRecompute) <= 0) {
                this._viewsToRecompute.push(entityView);
            }
        },

        __recomputeDependentViews: function () {
            while (this._viewsToRecompute.length > 0) {  // Downstream entity views might be dirtied due to recompute.
                var viewsToRecompute = this._viewsToRecompute.slice();
                this._viewsToRecompute.splice(0, this._viewsToRecompute.length);
                $.each(viewsToRecompute, function (index, entityView) {
                    entityView.__recompute();
                });
            }
        },

        // Used to translate entity inserts through EntityViews (and onto their input EntitySource).
        __addEntity: function (entity) {
            var index = obs.asArray(this._clientEntities).length;
            obs.insert(this._clientEntities, index, [entity]);

            this._handleArrayChange("insert", { index: index, items: [entity] });
        },

        // Used to translate entity removes through EntityViews (and onto their input EntitySource).
        __deleteEntity: function (entity, index) {
            var index = $.inArray(entity, obs.asArray(this._clientEntities));
            ///#DEBUG
            upshot.assert(index >= 0, "entity must exist!");
            ///#ENDDEBUG
            obs.remove(this._clientEntities, index, 1);

            this._handleArrayChange("remove", { index: index, items: [entity] });
        },

        // Private methods

        _bindFromOptions: function (options, events) {
            if (options) {
                var self = this;
                $.each(events, function (unused, event) {
                    var callback = options && options[event];
                    if (callback) {
                        self.bind(event, callback);
                    }
                });
            }
        },

        _dispose: function () {
            // Will be overridden by derived classes.
        },

        _handleArrayChange: function (type, eventArguments) {
            switch (type) {
                case "insert":
                    var entitiesToAdd = eventArguments.items;
                    if (entitiesToAdd.length > 1) {
                        throw "NYI -- Can only add a single entity to/from an array in one operation.";
                    }

                    var entityToAdd = entitiesToAdd[0];
                    this._handleEntityAdd(entityToAdd);
                    break;

                case "remove":
                    throw "Use 'deleteEntity' to delete entities from your array.  Destructive delete is not yet implemented.";

                case "replaceAll":
                    if (!upshot.sameArrayContents(eventArguments.newItems, obs.asArray(this._clientEntities))) {
                        throw "NYI -- Can only replaceAll with own entities.";
                    }
                    break;

                default:
                    throw "NYI -- Array operation '" + type + "' is not supported.";
            }

            this._trigger("arrayChanged", type, eventArguments);
        },

        _handleEntityAdd: function (entity) {
            // Will be overridden by derived classes to do specific handling for an entity add.
        },

        _purgeEntity: function (entity) {
            // TODO -- Should we try to handle duplicates here?
            var index = $.inArray(entity, obs.asArray(this._clientEntities));
            obs.remove(this._clientEntities, index, 1);
            this._trigger("arrayChanged", "remove", { index: index, items: [entity] });
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
        }
    };

    var classMembers = {
        as: function (array) {
            return upshot.cache(array, "entitySource");
        }
    };

    upshot.EntitySource = upshot.defineClass(ctor, instanceMembers, classMembers);
}
///#RESTORE )(this, jQuery, upshot);
