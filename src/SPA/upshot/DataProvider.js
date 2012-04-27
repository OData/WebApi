/// <reference path="IntelliSense\References.js" />
///#RESTORE (function (global, $, upshot, undefined)
{
    function getQueryResult(getResult, wrappedResult) {
        var entities, totalCount;

        if (wrappedResult) {
            entities = getResult.Results;
            totalCount = getResult.TotalCount;
        }
        else {
            entities = getResult;
        }

        entities = upshot.isArray(entities) ? entities : [entities];

        $.each(entities, function (unused, entity) {
            // This is not strictly model data.
            delete entity["$type"];
        });

        return {
            entities: entities,
            totalCount: totalCount
        };
    }

    var operations = (function () {
        var operations = {};
        operations[upshot.ChangeKind.Add] = 1;
        operations[upshot.ChangeKind.Update] = 2;
        operations[upshot.ChangeKind.Delete] = 3;
        return operations;
    })();

    function transformChangeSet(changeSet) {
        return $.map(changeSet, function (change, index) {
            var changeSetEntry =  {
                Id: index.toString(),
                Operation: operations[change.changeKind]
            };
            $.each({ entity: "Entity", originalEntity: "OriginalEntity" }, function (key, value) {
                if (change[key]) {
                    changeSetEntry[value] = $.extend(true, {}, { "$type": change.entityType }, change[key]);
                }
            });
            return changeSetEntry;
        });
    }

    function transformSubmitResult(result) {
        return $.map(result, function (changeSetEntry) {
            var result = {};

            if (changeSetEntry.Entity) {
                result.entity = changeSetEntry.Entity;
            }

            // transform to Error property
            // even though upshot currently doesn't support reporting of concurrency conflicts,
            // we must still identify such failures
            $.each(["ConflictMembers", "ValidationErrors", "IsDeleteConflict"], function (index, property) {
                if (changeSetEntry.hasOwnProperty(property)) {
                    result.error = result.error || {};
                    result.error[property] = changeSetEntry[property];
                }
            });

            return result;
        });
    }

    var instanceMembers = {

        // Public methods

        get: function (parameters, queryParameters, success, error) {
            /// <summary>
            /// Asynchronously gets data from the server using the specified parameters
            /// </summary>
            /// <param name="parameters" type="String">The get parameters</param>
            /// <param name="queryParameters" type="Object">An object where each property is a query to pass to the operation. This parameter is optional.</param>
            /// <param name="success" type="Function">Optional success callback</param>
            /// <param name="error" type="Function">Optional error callback</param>
            /// <returns type="Promise">A Promise representing the result of the load operation</returns>

            var operation, operationParameters;
            if (parameters) {
                operation = parameters.operationName;
                operationParameters = parameters.operationParameters;
            }

            if ($.isFunction(operationParameters)) {
                success = operationParameters;
                error = queryParameters;
            }

            var self = this;

            // set up the request parameters
            var url = upshot.DataProvider.normalizeUrl(parameters.url) + operation;
            var oDataQueryParams = upshot.ODataDataProvider.getODataQueryParameters(queryParameters);
            var data = $.extend({}, operationParameters, oDataQueryParams);
            var wrappedResult = oDataQueryParams.$inlinecount == "allpages";

            // invoke the query
            $.ajax({
                url: url,
                data: data,
                success: success && function () {
                    arguments[0] = getQueryResult(arguments[0], wrappedResult);
                    success.apply(self, arguments);
                },
                error: error && function (jqXHR, statusText, errorText) {
                    error.call(self, jqXHR.status, self._parseErrorText(jqXHR.responseText) || errorText, jqXHR);
                },
                dataType: "json"
            });
        },

        submit: function (parameters, changeSet, success, error) {
            /// <summary>
            /// Asynchronously submits the specified changeset
            /// </summary>
            /// <param name="parameters" type="String">The submit parameters</param>
            /// <param name="changeSet" type="Object">The changeset to submit</param>
            /// <param name="success" type="Function">Optional success callback</param>
            /// <param name="error" type="Function">Optional error callback</param>
            /// <returns type="Promise">A Promise representing the result of the post operation</returns>

            var self = this,
                encodedChangeSet = JSON.stringify(transformChangeSet(changeSet));

            $.ajax({
                url: upshot.DataProvider.normalizeUrl(parameters.url) + "Submit",
                contentType: "application/json",
                data: encodedChangeSet,
                dataType: "json",
                type: "POST",
                success: (success || error) && function (submitChangesResult, statusText, jqXHR) {
                    var result = submitChangesResult ? transformSubmitResult(submitChangesResult) : [];
                    var hasErrors = $.grep(result, function (subresult) {
                        return subresult.hasOwnProperty("error");
                    }).length > 0;

                    if (!hasErrors) {
                        if (success) {
                            success.call(self, result);
                        }
                    } else if (error) {
                        var errorText = "Submit failed.";
                        if (submitChangesResult) {
                            for (var i = 0; i < submitChangesResult.length; ++i) {
                                // TODO: Why does this only treat ValidationErrors?  What about ConflictMembers and IsDeleteConflict?
                                var validationError = (submitChangesResult[i].ValidationErrors && submitChangesResult[i].ValidationErrors[0] && submitChangesResult[i].ValidationErrors[0].Message);
                                if (validationError) {
                                    errorText = validationError;
                                    break;
                                }
                            }
                        }
                        error.call(self, jqXHR.status, errorText, jqXHR, result);
                    }
                },
                error: error && function (jqXHR, statusText, errorText) {
                    error.call(self, jqXHR.status, self._parseErrorText(jqXHR.responseText) || errorText, jqXHR);
                }
            });
        },

        _parseErrorText: function (responseText) {
            var match = /Exception]: (.+)\r/g.exec(responseText);
            if (match && match[1]) {
                return match[1];
            }
            if (/^{.*}$/g.test(responseText)) {
                var error = JSON.parse(responseText);
                // TODO: error.Message returned by DataController
                // Does ErrorMessage check still necessary?
                if (error.ErrorMessage) {
                    return error.ErrorMessage;
                } else if (error.Message) {
                    return error.Message;
                }
            }
        }
    }

    var classMembers = {
        normalizeUrl: function (url) {
            if (url && url.substring(url.length - 1) !== "/") {
                return url + "/";
            }
            return url;
        }
    }

    upshot.DataProvider = upshot.defineClass(null, instanceMembers, classMembers);
}
///#RESTORE )(this, jQuery, upshot);
