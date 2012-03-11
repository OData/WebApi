(function() {

  /*!
  nav.js v0.1 - (c) Microsoft Corporation
  */

  var readWriteValue;
  var __bind = function(fn, me){ return function(){ return fn.apply(me, arguments); }; }, __hasProp = Object.prototype.hasOwnProperty;

  readWriteValue = function(initialValue) {
    var currentValue;
    currentValue = initialValue;
    return function() {
      if (arguments.length > 0) currentValue = arguments[0];
      return currentValue;
    };
  };

  window.NavHistory = (function() {

    function NavHistory(opts) {
      this.navigateAll = __bind(this.navigateAll, this);
      this.navigate = __bind(this.navigate, this);
      this._entriesArray = __bind(this._entriesArray, this);
      this.forward = __bind(this.forward, this);
      this.back = __bind(this.back, this);
      this.loadedData = __bind(this.loadedData, this);
      this.params = __bind(this.params, this);
      this.current = __bind(this.current, this);
      this.relative = __bind(this.relative, this);
      this.length = __bind(this.length, this);
      var useKo;
      this.options = opts || {};
      this.options.params = this._extend({}, this.options.params, this._asString);
      this.isLinkedToUrl = false;
      useKo = 'ko' in this.options ? this.options.ko : (typeof ko !== "undefined" && ko !== null ? ko.observable : void 0) != null;
      this.position = useKo ? ko.observable(-1) : readWriteValue(-1);
      this.entries = useKo ? ko.observableArray([]) : [];
    }

    NavHistory.prototype.length = function() {
      return this._entriesArray().length;
    };

    NavHistory.prototype.relative = function(offset) {
      return this._entriesArray()[this.position() + offset] || {};
    };

    NavHistory.prototype.current = function() {
      return this.relative(0);
    };

    NavHistory.prototype.params = function() {
      return this.current().params || {};
    };

    NavHistory.prototype.loadedData = function() {
      return this.current().loadedData;
    };

    NavHistory.prototype.back = function() {
      if (this.position() > 0) return this.navigateAll(this.relative(-1).params);
    };

    NavHistory.prototype.forward = function() {
      if (this.position() < this.length() - 1) {
        return this.navigateAll(this.relative(1).params);
      }
    };

    NavHistory.prototype._entriesArray = function() {
      if (typeof this.entries === 'function') {
        return this.entries();
      } else {
        return this.entries;
      }
    };

    NavHistory.prototype.initialize = function(opts) {
      if (opts != null ? opts.linkToUrl : void 0) {
        this._linkToUrl();
      } else {
        this.navigateAll((opts != null ? opts.params : void 0) || {});
      }
      return this;
    };

    NavHistory.prototype.navigate = function(newParams, opts) {
      var newParamsPlusCurrent;
      newParamsPlusCurrent = this._extend(this._extend({}, this.params()), newParams);
      return this.navigateAll(newParamsPlusCurrent, opts);
    };

    NavHistory.prototype.navigateAll = function(newParams, opts) {
      var beforeNavigateCallback, isBack, isForward, isNoChange, navEntry, navInfo, threadLoadToken, transition, _ref, _ref2;
      var _this = this;
      newParams = this._normalizeParams(newParams);
      isBack = false;
      isForward = false;
      isNoChange = false;
      transition = opts != null ? opts.transition : void 0;
      navEntry = null;
      if (this.length() && this._propsAreEqual(newParams, this.params())) {
        if (opts != null ? opts.force : void 0) {
          isNoChange = true;
          navEntry = this.current();
        } else {
          return;
        }
      } else if (this._propsAreEqual(newParams, (_ref = this.relative(-1)) != null ? _ref.params : void 0)) {
        isBack = true;
        transition = transition || this.current().savedTransition;
        navEntry = this.relative(-1);
      } else if (this._propsAreEqual(newParams, (_ref2 = this.relative(1)) != null ? _ref2.params : void 0)) {
        isForward = true;
        navEntry = this.relative(1);
        transition = transition || this.current().savedTransition;
      } else {
        navEntry = {
          params: newParams,
          navEntryId: "navEntry_" + this._getUniqueSequenceValue()
        };
      }
      navInfo = {
        isFirst: this.length() === 0,
        isBack: isBack,
        isForward: isForward,
        transition: transition
      };
      beforeNavigateCallback = function() {
        var deleteCount, updatedQueryString;
        if (isBack) {
          _this.position(_this.position() - 1);
        } else if (isForward) {
          _this.position(_this.position() + 1);
        } else if (!isNoChange) {
          deleteCount = _this.length() - _this.position() - 1;
          _this.entries().splice(_this.position() + 1, deleteCount, navEntry);
          if (_this.options.maxEntries && _this.length() > _this.options.maxEntries) {
            _this.entries.shift();
          } else {
            _this.position(_this.position() + 1);
            if (typeof _this.entries.valueHasMutated === 'function') {
              _this.entries.valueHasMutated();
            }
          }
        }
        if (!isBack && navInfo.transition) {
          _this.current().savedTransition = navInfo.transition;
        }
        if (_this.isLinkedToUrl && ((opts != null ? opts.updateUrl : void 0) !== false) && !isNoChange) {
          updatedQueryString = _this._getUpdatedQueryString(_this.params());
          window.NavHistory.historyProvider.pushState({
            url: updatedQueryString
          });
        }
        if (_this.options.onNavigate) {
          return _this.options.onNavigate.call(_this, _this.current(), navInfo);
        }
      };
      if (!this.options.beforeNavigate) {
        beforeNavigateCallback();
      } else {
        threadLoadToken = this.objectLoadToken = {};
        this.options.beforeNavigate.call(this, navEntry, navInfo, (function(loadedData) {
          if (threadLoadToken === _this.objectLoadToken) {
            if (loadedData !== void 0) navEntry.loadedData = loadedData;
            return beforeNavigateCallback();
          }
        }));
      }
      return this;
    };

    NavHistory.prototype._asString = function(val) {
      if (val === null || val === void 0) {
        return "";
      } else {
        return val.toString();
      }
    };

    NavHistory.prototype._extend = function(target, source, mapFunction) {
      var key, value;
      for (key in source) {
        if (!__hasProp.call(source, key)) continue;
        value = source[key];
        target[key] = mapFunction ? mapFunction(value) : value;
      }
      return target;
    };

    NavHistory.prototype._normalizeParams = function(params) {
      var defaults;
      defaults = this.options.params || {};
      return this._extend(this._extend({}, defaults), params || {}, this._asString);
    };

    NavHistory.prototype._propsAreEqual = function(obj1, obj2) {
      var obj1key, obj1value, obj2key, obj2value;
      if (!(obj1 && obj2)) return obj1 === obj2;
      for (obj1key in obj1) {
        if (!__hasProp.call(obj1, obj1key)) continue;
        obj1value = obj1[obj1key];
        if (obj2[obj1key] !== obj1value) return false;
      }
      for (obj2key in obj2) {
        if (!__hasProp.call(obj2, obj2key)) continue;
        obj2value = obj2[obj2key];
        if (obj1[obj2key] !== obj2value) return false;
      }
      return true;
    };

    NavHistory.prototype._parseQueryString = function(url) {
      var pair, query, result, tokens, _i, _len, _ref;
      if (url.indexOf('?') < 0) return {};
      query = url.substring(url.lastIndexOf('?') + 1);
      result = {};
      _ref = query.split("&");
      for (_i = 0, _len = _ref.length; _i < _len; _i++) {
        pair = _ref[_i];
        tokens = pair.split("=");
        if (tokens.length === 2) result[tokens[0]] = decodeURIComponent(tokens[1]);
      }
      return result;
    };

    NavHistory.prototype._formatQueryString = function(params) {
      var formattedUrl, key, value;
      formattedUrl = '?';
      for (key in params) {
        if (!__hasProp.call(params, key)) continue;
        value = params[key];
        if (formattedUrl !== '?') formattedUrl += '&';
        formattedUrl += key + '=' + encodeURIComponent(value);
      }
      return formattedUrl;
    };

    NavHistory.prototype._getUpdatedQueryString = function(params) {
      var allUrlParams, defaultValue, key, suppliedValue, _ref;
      allUrlParams = this._parseQueryString(window.NavHistory.historyProvider.getState().url);
      _ref = this.options.params;
      for (key in _ref) {
        if (!__hasProp.call(_ref, key)) continue;
        defaultValue = _ref[key];
        suppliedValue = params[key];
        if (suppliedValue === defaultValue) {
          delete allUrlParams[key];
        } else {
          allUrlParams[key] = suppliedValue;
        }
      }
      return this._formatQueryString(allUrlParams);
    };

    NavHistory.prototype._getUniqueSequenceValue = function() {
      NavHistory._sequence = NavHistory._sequence || 0;
      return (NavHistory._sequence++).toString();
    };

    NavHistory.prototype._linkToUrl = function() {
      var onStateChange;
      var _this = this;
      this.isLinkedToUrl = true;
      onStateChange = function() {
        var allUrlParams, applicableParams, defaults, key, value;
        applicableParams = {};
        allUrlParams = _this._parseQueryString(window.NavHistory.historyProvider.getState().url);
        defaults = _this.options.params || {};
        for (key in allUrlParams) {
          if (!__hasProp.call(allUrlParams, key)) continue;
          value = allUrlParams[key];
          if (defaults.hasOwnProperty(key)) applicableParams[key] = value;
        }
        return _this.navigateAll(applicableParams, {
          updateUrl: false
        });
      };
      onStateChange();
      return window.NavHistory.historyProvider.onStateChange(onStateChange);
    };

    return NavHistory;

  })();

  window.NavHistory.historyProvider = {
    onStateChange: function(handler) {
      return History.Adapter.bind(window, 'statechange', handler);
    },
    pushState: function(data) {
      return History.pushState(null, null, data.url);
    },
    getState: function() {
      return History.getState();
    },
    back: function() {
      return History.back();
    }
  };

  window.NavHistory.showPane = function(elementId, navInfo) {
    var elemToShow, sibling, _i, _len, _ref;
    elemToShow = document.getElementById(elementId);
    if (elemToShow) {
      _ref = elemToShow.parentNode.childNodes;
      for (_i = 0, _len = _ref.length; _i < _len; _i++) {
        sibling = _ref[_i];
        if (sibling.nodeType === 1) sibling.style.display = 'none';
      }
      return elemToShow.style.display = 'block';
    }
  };

}).call(this);
