/// <reference path="IntelliSense\References.js" />
///#RESTORE (function (global, $, upshot, undefined)
{
    function transformQuery(query) {
        var queryParameters = {};

        // filters -> $where
        if (query.filters && query.filters.length) {
            var whereParameter = "",
                applyOperator = function (property, operator, value) {
                    if (typeof value === "string") {
                        if (upshot.isGuid(value)) {
                            value = "Guid(" + value + ")";
                        } else {
                            value = '"' + value + '"';
                        }
                    } else if (upshot.isDate(value)) {
                        // DomainService expects ticks; js Date.getTime() gives ms since epoch
                        value = "DateTime(" + (value.getTime() * 10000 + 621355968000000000) + ")";
                    }

                    switch (operator) {
                        case "<":
                        case "<=":
                        case "==":
                        case "!=":
                        case ">=":
                        case ">": return property + operator + value;
                        case "StartsWith":
                        case "EndsWith":
                        case "Contains": return property + "." + operator + "(" + value + ")";
                        default: throw "The operator '" + operator + "' is not supported.";
                    }
                };

            $.each(query.filters, function (index, filter) {
                if (whereParameter) {
                    whereParameter += " AND ";
                }
                whereParameter += applyOperator(filter.property, filter.operator, filter.value);
            });

            queryParameters.$where = whereParameter;
        }

        // sort -> $orderby
        if (query.sort && query.sort.length) {
            var formatSort = function (sort) {
                return !!sort.descending ? (sort.property + " desc") : sort.property;
            };
            queryParameters.$orderby = $.map(query.sort, function (sort, index) {
                return formatSort(sort);
            }).join();
        }

        // skip -> $skip
        if (query.skip) {
            queryParameters.$skip = query.skip;
        }

        // take -> $take
        if (query.take) {
            queryParameters.$take = query.take;
        }

        // includeTotalCount -> $includeTotalCount
        if (query.includeTotalCount) {
            queryParameters.$includeTotalCount = query.includeTotalCount;
        }

        return queryParameters;
    }

    function transformParameters(parameters) {
        // perform any required transformations on the specified parameters
        // before invoking the service, for example json serializing arrays
        // and other complex parameters.
        if (parameters) {
            $.each(parameters || {}, function (key, value) {
                if ($.isArray(value)) {
                    // json serialize arrays since this is the format the json
                    // endpoint expects.
                    parameters[key] = JSON.stringify(value);
                }
            });
        }

        return parameters;
    }

    function getQueryResult(getResult) {
        var resultKey;
        $.each(getResult, function (key) {
            if (/Result$/.test(key)) {
                resultKey = key;
                return false;
            }
        });
        var result = getResult[resultKey];
        
        // process the metadata
        var metadata = {};
        $.each(result.Metadata, function (unused, metadataForType) {
            metadata[metadataForType.type] = {
                key: metadataForType.key,
                fields: metadataForType.fields,
                rules: metadataForType.rules,
                messages: metadataForType.messages
            };
        });

        $.each(result.RootResults, function (unused, entity) {
            // This is not strictly model data.
            delete entity.__type;
        });

        var includedEntities;
        if (result.IncludedResults) {
            // group included entities by type
            includedEntities = {};
            $.each(result.IncludedResults, function (unused, entity) {
                var entityType = entity.__type;
                var entities = includedEntities[entityType] || (includedEntities[entityType] = []);
                entities.push(entity);
            });
        }

        return {
            type: result.Metadata[0].type,
            metadata: metadata,
            entities: result.RootResults,
            includedEntities: includedEntities,
            totalCount: result.TotalCount || 0
        };
    }

    var operations = (function () {
        var operations = {};
        operations[upshot.ChangeKind.Add] = 2;
        operations[upshot.ChangeKind.Update] = 3;
        operations[upshot.ChangeKind.Delete] = 4;
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
                    changeSetEntry[value] = $.extend(true, {}, { __type: change.entityType }, change[key]);
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

            // Invoke the query
            $.ajax({
                url: upshot.DataProvider.normalizeUrl(parameters.url) + "json/" + operation,
                data: $.extend({}, transformParameters(operationParameters), transformQuery(queryParameters || {})),
                success: success && function () {
                    arguments[0] = getQueryResult(arguments[0]);
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
                encodedChangeSet = JSON.stringify({ changeSet: transformChangeSet(changeSet) });

            $.ajax({
                url: upshot.DataProvider.normalizeUrl(parameters.url) + "json/SubmitChanges",
                contentType: "application/json",
                data: encodedChangeSet,
                dataType: "json",
                type: "POST",
                success: (success || error) && function (data, statusText, jqXHR) {
                    var submitChangesResult = data.SubmitChangesResult,
                        result = submitChangesResult ? transformSubmitResult(submitChangesResult) : [],
                        hasErrors = $.grep(result, function (subresult) {
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
                if (error.ErrorMessage) {
                    return error.ErrorMessage;
                }
            }
        }
    }

    upshot.riaDataProvider = upshot.defineClass(null, instanceMembers);
}
///#RESTORE )(this, jQuery, upshot);
