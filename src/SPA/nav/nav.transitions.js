(function() {

  /*!
  nav.transitions.js v0.1 - (c) Microsoft Corporation
  */

  var $, eventName, features, findFirstChildWithClass, oppositeDirection, oppositeTransition, _i, _len, _ref;
  var __hasProp = Object.prototype.hasOwnProperty;

  $ = x$;

  features = {
    vendor: /webkit/i.test(navigator.appVersion) ? 'webkit' : /firefox/i.test(navigator.userAgent) ? 'Moz' : 'opera' in window ? 'O' : '',
    isAndroid: /android/gi.test(navigator.appVersion)
  };

  features.useCssTransform = (!features.isAndroid) && (features.vendor + 'Transform' in document.documentElement.style);

  features.cssTransformPrefix = "-" + features.vendor.toLowerCase() + "-";

  features.transitionEndEvent = features.vendor === 'webkit' ? 'webkitTransitionEnd' : features.vendor === 'O' ? 'oTransitionEnd' : 'transitionend';

  $.isTouch = 'ontouchstart' in document.documentElement;

  $.clickOrTouch = $.isTouch ? 'touchstart' : 'click';

  features.supportsCssTouchScroll = typeof document.body.style.webkitOverflowScrolling !== "undefined";

  features.supportsIScroll = features.vendor === 'webkit' || features.vendor === "Moz";

  findFirstChildWithClass = function(elem, className) {
    var child;
    child = elem.firstChild;
    while (child) {
      if ($(child).hasClass(className)) return child;
      child = child.nextSibling;
    }
    return null;
  };

  oppositeDirection = {
    left: 'right',
    right: 'left',
    top: 'bottom',
    bottom: 'top'
  };

  oppositeTransition = function(transition) {
    var key;
    if (transition) {
      for (key in transition) {
        if (!__hasProp.call(transition, key)) continue;
        if ($.paneTransitionInverters.hasOwnProperty(key)) {
          return $.paneTransitionInverters[key](transition[key]);
        }
      }
    }
    return null;
  };

  $.getJSON = function(url, options) {
    var callback;
    callback = typeof options === "function" ? options : options.callback;
    return $(null).xhr(url, {
      method: options.method,
      async: true,
      data: JSON.stringify(options.data),
      headers: options.headers,
      callback: function() {
        return callback(JSON.parse(this.responseText));
      }
    });
  };

  $.map = function(items, map) {
    var item, mapped, results, _i, _len;
    results = [];
    for (_i = 0, _len = items.length; _i < _len; _i++) {
      item = items[_i];
      mapped = map(item);
      if (mapped !== void 0) results.push(mapped);
    }
    return results;
  };

  $.fn.togglePane = function(show) {
    return this.each(function() {
      if (show) {
        this.style.display = 'block';
        if (this.isOffScreen) {
          this.isOffScreen = false;
          this.style.top = '';
          this.style.bottom = '';
        }
      } else {
        this.isOffScreen = true;
        this.style.top = '-10000px';
        this.style.bottom = '10000px';
      }
      return this;
    });
  };

  $.fn.afterNextTransition = function(callback) {
    return this.each(function() {
      var elem, handlerWrapper;
      elem = this;
      handlerWrapper = function() {
        callback.apply(this, arguments);
        return elem.removeEventListener(features.transitionEndEvent, handlerWrapper);
      };
      return elem.addEventListener(features.transitionEndEvent, handlerWrapper);
    });
  };

  $.fn.animateTranslation = function(finalPos, transition, callback) {
    callback = callback || function() {
      return {};
    };
    return this.each(function() {
      var $this, transform;
      $this = $(this);
      if (features.useCssTransform) {
        transform = {};
        transform[features.cssTransformPrefix + "transform"] = "translate(" + finalPos.left + ", " + finalPos.top + ")";
        transform[features.cssTransformPrefix + "transition"] = transition ? features.cssTransformPrefix + "transform 250ms ease-out" : null;
        if (transition) $this.afterNextTransition(callback);
        $this.css(transform);
        if (!transition) return callback();
      } else {
        if (transition) {
          return $this.tween(finalPos, callback);
        } else {
          $this.css(finalPos);
          return callback();
        }
      }
    });
  };

  $.fn.setPanePosition = function(position, transition, callback) {
    callback = callback || function() {
      return {};
    };
    return this.each(function() {
      var $this, finalPos, height, width, x, y;
      $this = $(this).togglePane(true);
      x = 0;
      y = 0;
      width = this.parentNode.offsetWidth;
      height = this.parentNode.offsetHeight;
      switch (position) {
        case 'right':
          x = width;
          break;
        case 'left':
          x = -1 * width;
          break;
        case 'top':
          y = -1 * height;
          break;
        case 'bottom':
          y = height;
      }
      finalPos = {
        left: x + 'px',
        right: (-1 * x) + 'px',
        top: y + 'px',
        bottom: (-1 * y) + 'px'
      };
      return $this.animateTranslation(finalPos, transition, callback);
    });
  };

  $.fn.slidePane = function(options) {
    return this.each(function() {
      var $this, afterSlide;
      $this = $(this);
      afterSlide = function() {
        if (options.to) $this.togglePane(false);
        if (options.callback) return options.callback();
      };
      return $this.setPanePosition(options.from, null).setPanePosition(options.to, true, afterSlide);
    });
  };

  $.fn.showPane = function(options) {
    options = options || {};
    return this.each(function() {
      var activePane, transitionKey, transitionToUse, _ref;
      activePane = findFirstChildWithClass(this.parentNode, "active");
      if (activePane !== this) {
        $(this).has(".scroll-y.autoscroll").touchScroll({
          hScroll: false
        });
        $(this).has(".scroll-x.autoscroll").touchScroll({
          yScroll: false
        });
        transitionToUse = 'default';
        _ref = $.paneTransitions;
        for (transitionKey in _ref) {
          if (!__hasProp.call(_ref, transitionKey)) continue;
          if (options.hasOwnProperty(transitionKey)) {
            transitionToUse = transitionKey;
            break;
          }
        }
        $.paneTransitions[transitionKey](this, activePane, options[transitionKey]);
        $(this).addClass("active");
        if (activePane) return $(activePane).removeClass("active");
      }
    });
  };

  $.fn.showBySlidingParent = function(options) {
    return this.each(function() {
      var finalPos, targetPaneOffsetLeft, targetPaneOffsetTop;
      targetPaneOffsetLeft = parseInt(this.style.left) || 0;
      targetPaneOffsetTop = parseInt(this.style.top) || 0;
      finalPos = {
        left: (-1 * targetPaneOffsetLeft) + '%',
        right: targetPaneOffsetLeft + '%',
        top: (-1 * targetPaneOffsetTop) + '%',
        bottom: targetPaneOffsetTop + '%'
      };
      $(this).css({
        display: 'block'
      });
      $(this.parentNode).css({
        'overflow': 'visible'
      }).animateTranslation(finalPos, options.animate !== false);
      return this;
    });
  };

  $.fn.touchScroll = function(options) {
    if ((!features.supportsCssTouchScroll) && features.supportsIScroll) {
      this.each(function() {
        var doRefresh;
        var _this = this;
        if (!this.hasIScroll) this.hasIScroll = new iScroll(this, options);
        doRefresh = function() {
          return _this.hasIScroll.refresh();
        };
        setTimeout(doRefresh, 0);
        return this;
      });
    }
    return this;
  };

  $.fn.clickOrTouch = function(handler) {
    return this.on($.clickOrTouch, handler);
  };

  _ref = ['click'];
  for (_i = 0, _len = _ref.length; _i < _len; _i++) {
    eventName = _ref[_i];
    if (!$.fn[eventName]) {
      $.fn[eventName] = function(handler) {
        return this.on(eventName, handler);
      };
    }
  }

  $.paneTransitions = {
    slideFrom: function(incomingPane, outgoingPane, options) {
      $(incomingPane).slidePane({
        from: options
      });
      if (outgoingPane) {
        return $(outgoingPane).slidePane({
          to: oppositeDirection[options]
        });
      }
    },
    coverFrom: function(incomingPane, outgoingPane, options) {
      var outgoingZIndex;
      outgoingZIndex = (outgoingPane != null ? outgoingPane.style.zIndex : void 0) || 0;
      return $(incomingPane).css({
        zIndex: outgoingZIndex + 1
      }).slidePane({
        from: options,
        callback: function() {
          return $(outgoingPane).togglePane(false);
        }
      });
    },
    uncoverTo: function(incomingPane, outgoingPane, options) {
      var incomingZIndex;
      incomingZIndex = incomingPane.style.zIndex || 0;
      $(incomingPane).togglePane(true).setPanePosition();
      return $(outgoingPane).css({
        zIndex: incomingZIndex + 1
      }).slidePane({
        to: options
      });
    },
    "default": function(incomingPane, outgoingPane, options) {
      $(incomingPane).togglePane(true).setPanePosition();
      return $(outgoingPane).togglePane(false);
    }
  };

  $.paneTransitionInverters = {
    slideFrom: function(direction) {
      return {
        slideFrom: oppositeDirection[direction]
      };
    },
    coverFrom: function(direction) {
      return {
        uncoverTo: direction
      };
    },
    uncoverTo: function(direction) {
      return {
        coverFrom: direction
      };
    }
  };

  window.NavHistory.animatePane = function(elementId, navInfo) {
    var transition;
    transition = navInfo.transition || (!navInfo.isFirst ? {
      slideFrom: 'right'
    } : null);
    if (navInfo.isBack) transition = oppositeTransition(transition);
    return x$('#' + elementId).showPane(transition);
  };

  window.NavHistory.slideParent = function(elementId, navInfo) {
    return x$('#' + elementId).showBySlidingParent({
      animate: !navInfo.isFirst
    });
  };

}).call(this);
