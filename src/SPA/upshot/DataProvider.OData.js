/// <reference path="IntelliSense\References.js" />
///#RESTORE (function (global, $, upshot, undefined)
{
    function pad(count, value) {
        var str = "0000" + value;
        return str.slice(str.length - count);
    }

    function formatDateTime(date) {
        return "datetime" +
            "'" + pad(4, date.getUTCFullYear()) +
            "-" + pad(2, date.getUTCMonth() + 1) +
            "-" + pad(2, date.getUTCDate()) +
            "T" + pad(2, date.getUTCHours()) +
            ":" + pad(2, date.getUTCMinutes()) +
            ":" + pad(2, date.getUTCSeconds()) + "'";
    }

    function getQueryResult(getResult) {
        var entities = getResult.results,
            resultType = entities.length && entities[0].__metadata.type;

        var metadata;
        if (resultType) {
            metadata = {};
            metadata[resultType] = {
                key: ["__metadata.uri"]
            };
        }

        var count = getResult.__count,
            totalCount = count === undefined ? null : +count;

        return {
            type: resultType,
            metadata: metadata,
            entities: entities,
            totalCount: totalCount
        };
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

            // $.map applied to objects is supported in jQuery >= 1.6. Our current baseline is jQuery 1.5
            var parameterStrings = [];
            $.each($.extend({}, operationParameters, upshot.ODataDataProvider.getODataQueryParameters(queryParameters)), function (key, value) {
                parameterStrings.push(key.toString() + "=" + value.toString());
            });
            var queryString = parameterStrings.length ? ("?" + parameterStrings.join("&")) : "";

            // Invoke the query
            OData.read(upshot.DataProvider.normalizeUrl(parameters.url) + operation + queryString,
                function (result) {
                    if (success) {
                        arguments[0] = getQueryResult(arguments[0]);
                        success.apply(self, arguments);
                    }
                },
                function (reason) {
                    if (error) {
                        error.call(self, -1, reason.message, reason);
                    }
                }
            );
        },

        submit: function () {
            throw "Saving edits through the OData data provider is not supported.";
        }
    };

    var classMembers = {
        getODataQueryParameters: function (query) {
            query = query || {};
            var queryParameters = {};

            // filters -> $filter
            if (query.filters && query.filters.length) {
                var filterParameter = "",
                applyOperator = function (property, operator, value) {
                    if (typeof value === "string") {
                        if (upshot.isGuid(value)) {
                            value = "guid'" + value + "'";
                        } else {
                            value = "'" + value + "'";
                        }
                    } else if (upshot.isDate(value)) {
                        value = formatDateTime(value);
                    }

                    switch (operator) {
                        case "<": return property + " lt " + value;
                        case "<=": return property + " le " + value;
                        case "==": return property + " eq " + value;
                        case "!=": return property + " ne " + value;
                        case ">=": return property + " ge " + value;
                        case ">": return property + " gt " + value;
                        case "StartsWith": return "startswith(" + property + "," + value + ") eq true";
                        case "EndsWith": return "endswith(" + property + "," + value + ") eq true";
                        case "Contains": return "substringof(" + value + "," + property + ") eq true";
                        default: throw "The operator '" + operator + "' is not supported.";
                    }
                };

                $.each(query.filters, function (index, filter) {
                    if (filterParameter) {
                        filterParameter += " and ";
                    }
                    filterParameter += applyOperator(filter.property, filter.operator, filter.value);
                });

                queryParameters.$filter = filterParameter;
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

            // take -> $top
            if (query.take) {
                queryParameters.$top = query.take;
            }

            // includeTotalCount -> $inlinecount
            if (query.includeTotalCount) {
                queryParameters.$inlinecount = "allpages";
            }

            return queryParameters;
        }
    }

    upshot.ODataDataProvider = upshot.defineClass(null, instanceMembers, classMembers);

}
///#RESTORE )(this, jQuery, upshot);
