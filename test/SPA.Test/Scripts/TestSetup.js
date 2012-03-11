/// <reference path="../../Scripts/References.js" />

if (window.sessionStorage) {
    window.sessionStorage.clear();
}
module("DataSource Setup");

var defaultTestTimeout = 10000,
    retryLimit = 10;

QUnit.config.testTimeout = defaultTestTimeout;

var testHelper = {
    isChrome: /chrome/.test(navigator.userAgent.toLowerCase()),
    isFirefox: /mozilla/.test(navigator.userAgent.toLowerCase()) && (!/(compatible|webkit)/.test(navigator.userAgent.toLowerCase())),
    initService: function (url, typeOrOptions) {
        QUnit.ok(true, "start Init service:" + url + ", time: " + new Date().toLocaleString());
        QUnit.config.testTimeout = 120 * 1000;
        stop();
        var beat = setInterval(function () {
            if (window.TestSwarm && window.TestSwarm.heartbeat) {
                window.TestSwarm.heartbeat();
            }
        }, 1000);
        testHelper.setCookie("dbi", null, -1);

        var options;
        if (typeof(typeOrOptions) === "string") {
            options = { type: typeOrOptions };
        } else {
            options = typeOrOptions;
        }

        testHelper.serviceCall(url, options, 0);
    },
    serviceCall: function (url, options, retried, beat) {
        jQuery.ajax(url, options).then(function () {
            QUnit.config.testTimeout = defaultTestTimeout;
            clearInterval(beat);
            QUnit.ok(true, "Service request succeeded, tried: " + (retried + 1) + " times, timestamp:" + new Date().toLocaleString());
            start();
        }, function () {
            QUnit.ok(true, "Service request failed, retrying " + (retryLimit - retried - 1) + " more times, timestamp:" + new Date().toLocaleString());
            setTimeout(function () {
                retried++;
                if (retried < retryLimit) {
                    testHelper.serviceCall(url, options, retried);
                } else {
                    // Fail the rest quickly
                    QUnit.ok(false, "Service call to " + url + " failed");
                    QUnit.config.testTimeout = 100;
                    clearInterval(beat);
                    start();
                }
            }, 1000);
        })
    },

    setCookie: function (name, value, days) {
        if (days) { var date = new Date(); date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000)); var expires = "; expires=" + date.toGMTString(); } else var expires = ""; document.cookie = name + "=" + value + expires + "; path=/";
    },

    startOnPageSetupComplete: function (url) {
        stop();
        jQuery("#testFrame").one("load", function () {
            // make $ reference the jQuery in the iframe, intentionally global for tests to use
            var testFrame = window.frames[0];
            $ = testFrame.jQuery;

            //Wait for the page to trigger complete
            $(testFrame).one("PageSetupComplete", function () {
                start()
            });
        });
        jQuery("#testFrame").attr("src", url);
    },
    startOnPageLoad: function (url, cond) {
        stop();
        jQuery("#testFrame").one("load", function () {
            // make $ reference the jQuery in the iframe, intentionally global for tests to use
            var testFrame = window.frames[0];
            $ = testFrame.jQuery;
            testFrame.alert = function (error) { ok(false, "'alert(" + error + ")' called, failing test"); };
            if (cond) {
                var checkCond = function () {
                    if (cond()) {
                        start();
                    } else {
                        setTimeout(function () { checkCond(); }, 200);
                    }
                }
                checkCond();
            } else {
                start();
            }
        });
        jQuery("#testFrame").attr("src", url);
    },
    wrapFunctions: function (functionNames, triggerName) {
        // Allow string or array of strings to be passed
        functionNames = $.isArray(functionNames) ? functionNames : [functionNames];

        $.each(functionNames, function (index, functionName) {
            // Only allow one wrapping per function
            if ($.fn[functionName].__oldFunction__) {
                testHelper.unwrapFunctions([functionName]);
            }
            var newFunction = function () {
                // Create trigger arguments with function's name that was wrapped
                var returnedArgs = [functionName].concat(Array.prototype.slice.call(arguments, 0)),
                returnValue = newFunction.__oldFunction__.apply(this, arguments);
                // trigger after applying to allow state to be checked in callback
                $(this).trigger(triggerName, returnedArgs);
                // return the results so wrapped function behaves the same
                return returnValue;
            };
            // Store the new function on the old one
            newFunction.__oldFunction__ = $.fn[functionName];
            $.fn[functionName] = newFunction;
        });
        return function () {
            //do cleanup
            testHelper.unwrapFunctions(functionNames);
        };
    },
    unwrapFunctions: function (functionNames) {
        $.each(functionNames, function (index, name) {
            $.fn[name] = $.fn[name].__oldFunction__;
        });
    },
    curryStartOnPageLoad: function (url) {
        // Return a function with url already set, helpful for passing a callback to a module
        return function () {
            stop();
            jQuery("#testFrame").one("load", function () {
                // make $ reference the jQuery in the iframe, intentionally global for tests to use
                window.$ = window.frames[0].jQuery;
                // TODO: Fix this workaround for page load being too slow to load a new one every test function
                // especially slow when debugging
                setTimeout(QUnit.start, 100);
            });
            jQuery("#testFrame").attr("src", url);
        }
    },
    curryPost: function (url) {
        return function () {
            QUnit.stop();
            //REVIEW: This should be an implementation detail of DataSource / DomainSerivceProxy
            jQuery.post(url, function () {
                QUnit.start();
            }, "json");
        }
    },
    setLatency: function (url, queryDelay, cudDelay, success) {
        jQuery.ajax({
            type: "POST",
            url: url + "/SetLatency",
            data: '{"queryDelay":' + queryDelay + ',"cudDelay":' + cudDelay + '}',
            dataType: "json",
            contentType: "application/json",
            success: function () {
                success();
            },
            error: function () {
                ok(false, "setLatency request failed");
                start();
            }
        });
    },
    wrapOrAddFunction: function (baseObject, functionName, options) {
        // This function either:
        // 1) replaces the existing function making callbacks before and after the function would have been invoked
        // 2) creates the function making callbacks before and after the function would have been invoked
        // If the function existed it will be called as if it were not replaced
        // After the function is invoked once it will be reverted back to its original state unless
        // revertToOriginal: false is given via the options parameter
        // The original function is the return value which allows for the function to be reverted at any time (not after a single callback as usual)

        var before = options.before,
        after = options.after,
        revertToOriginal = options.revertToOriginal !== false, // Default to true when unsupplied
        backupOfOriginalFunction;

        // If function exists back it up
        if (baseObject[functionName]) {
            backupOfOriginalFunction = baseObject[functionName];
        }

        // Overwrite or create function
        baseObject[functionName] = function () {
            // Revert to original method, revertToOriginal is true by default
            if (revertToOriginal) {
                if (backupOfOriginalFunction) {
                    baseObject[functionName] = backupOfOriginalFunction;
                } else {
                    // If there was no original function remove the one added by wrapOrAddFunction
                    delete baseObject[functionName];
                }
            }

            if (before) {
                before.apply(this, Array.prototype.slice.call(arguments));
            }

            if (backupOfOriginalFunction) {
                backupOfOriginalFunction.apply(this, Array.prototype.slice.call(arguments));
            }

            if (after) {
                after.apply(this, Array.prototype.slice.call(arguments));
            }
        }

        // Allows one to revert to original state in their own code when revertToOriginal is false
        return backupOfOriginalFunction;
    },
    whenCondition: function (callback, interval, timeout) {
        var retry,
            deferred = $.Deferred();
        interval = interval || 500;
        timeout = timeout || QUnit.config.testTimeout;
        retry = timeout / interval;

        if (!callback()) {
            var pollCallback = function () {
                if (!callback()) {
                    --retry;
                    if (retry > 0) {
                        setTimeout(pollCallback, interval);
                    } else {
                        deferred.reject();
                    }
                } else {
                    deferred.resolve();
                }
            };
            setTimeout(pollCallback, interval);
        } else {
            deferred.resolve();
        }

        return deferred.promise();
    },
    startOnCondition: function (callback, interval, timeout) {
        var retry;
        interval = interval || 500;
        timeout = timeout || QUnit.config.testTimeout;
        retry = timeout / interval;
        stop();
        testHelper.whenCondition(callback, interval)
            .then(start);
    },
    changeValueAndWait: function (selector, value, delay) {
        stop();
        var input = $(selector);
        if (input.length !== 1) {
            throw "Only support one input";
        }
        var test = function () {
            input.unbind("change", test);
            setTimeout(function () { start(); }, delay ? delay : 100);
        }
        input.one("change", test).focusin().val(value).focusout().trigger("change");
    },
    // this will allow one-time ajax mock.
    mockAjaxOnce: function (url, result, statusText, error) {
        if ($.ajax._simulatedurl) {
            throw "cannot simulate ajax concurrently (prev=" + $.ajax._simulatedurl + ")";
        }
        var $ajax = $.ajax;
        $.ajax = function (settings) {
            if (settings.url.indexOf(url) == 0) {
                // revert $.ajax to orginal
                $.ajax = $ajax;
                var deferred = $.Deferred();
                setTimeout(function () {
                    // this follows $.ajax().fail() and $.ajax().done() signature
                    if (statusText) {
                        if (settings.error) {
                            settings.error.apply(null, [{ responseText: statusText, status: 200 }, statusText, error]);
                        }
                        deferred.reject(undefined, statusText, error);
                    } else {
                        if (settings.type === "POST") {
                            var data = JSON.parse(settings.data);
                            var ret = result || {
                                SubmitChangesResult: [{ Entity: data.changeSet[0].Entity}]
                            };
                            if (settings.success) {
                                settings.success.apply(null, [ret, "statusText", { status: 200}]);
                            }
                            deferred.resolve(ret);
                        } else {
                            var ret = result || {
                                EmptyResult: {
                                    RootResults: [],
                                    Metadata: [{ type: "dummy"}]
                                }
                            };
                            if (settings.success) {
                                settings.success.apply(null, [ret]);
                            }
                            deferred.resolve(ret);
                        }
                    }
                }, 10);
                return deferred.promise();
            } else {
                return $ajax(settings);
            }
        }
        $.ajax._simulatedurl = url;
        $.ajax._$ajax = $ajax;
    },
    unmockAjax: function () {
        if ($.ajax._$ajax) {
            $.ajax = $.ajax._$ajax;
        }
    }
};