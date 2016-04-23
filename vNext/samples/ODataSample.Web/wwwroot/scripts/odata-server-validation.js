"use strict";

angular
	.module("odataValidation", []);

angular
	.module("odataValidation")
	.directive("serverFieldValidation",
		["$http", "$q", "odataValidationSvc",
			function ($http, $q, odataValidationSvc) {
				return {
					require: "ngModel",
					link: function (scope, element, attrs, ngModel) {
						var path = odataValidationSvc.parsePath(attrs.serverFieldValidation, scope, element[0]);
						if (path.element) {
							// Add the error message container after
							// our element
							angular.element(element).after(path.element);
						}
						var config = {
							element: element,
							path: attrs.serverFieldValidation,
							propertyName: path.propertyName,
							formName: path.formName
						};
						scope.$broadcast("resolve-field-validation-url", config);
						var propertyValidationUrl = config.fieldValidationUrl;
						ngModel.$asyncValidators.server =
							function (modelValue, viewValue) {
								var deferred = $q.defer();
								if (path.propertyName) {
									$http.post(propertyValidationUrl, {
										Value: viewValue,
										Name: path.propertyName,
										SetName: config.setName
									}).then(
										function (response) {
											response.data.error.details = response.data.error.details || [];
											odataValidationSvc.setErrors(
												response.data.error.details,
												scope[path.formName][path.propertyName]);
											if (response.data.error.details.length) {
												// TODO: Join the error messages up
												// We could also use the key from the details, rather
												// than assume it's an error for this field
												//scope[path.errorKey] = response.data.error.details[0].message;
												deferred.reject();
											} else {
												deferred.resolve();
											}
										}
									);
								} else {
									deferred.resolve();
								}
								return deferred.promise;
							};
					}
				};
			}]);
angular
	.module("odataValidation")
	.factory("odataValidationSvc", ["$compile",
		function ($compile) {
			var svc = {};
			var errorKey = function (name, form) {
				var errorName = form + "_" + name + "_" + "Error";
				return errorName;
			}
			var generateElement = function (name, form, scope) {
				var errorName = errorKey(name, form);
				var errorElm = $compile("<div id=\"" + errorName + "\" class=\"error-messages\" ng-show=\"(" + form + ".$submitted && " + form + "." + name + ".$error.serverErrors) || (" + form + "." + name + ".$dirty && " + form + "." + name + ".$error.serverErrors)\">" +
					"<div class=\"error-message\" ng-repeat=\"error in " + form + "." + name + ".$error.serverErrors\">{{error.message}}</div>" +
					"</div>")(scope);
				return errorElm;
			}
			var parsePath = function (path, scope, element) {
				var arr = path.split(".");
				var result = null;
				if (arr.length === 3) {
					result = {
						formName: arr[0],
						setName: arr[1],
						propertyName: arr[2]
					}
				} else {
					result = {
						formName: element.form.name,
						setName: arr[0],
						propertyName: arr[1]
					}
				}
				result.errorKey = errorKey(result.propertyName, result.formName);
				result.element = generateElement(result.propertyName, result.formName, scope);
				return result;
			}
			var findError = function(propertyErrors, error) {
				for (var i = 0; i < propertyErrors.length; i++) {
					if (propertyErrors[i].message === error.message &&
						propertyErrors[i].target === error.target) {
						return propertyErrors[i];
					}
				}
				return null;
			}
			var errors = [];
			var getPropertyErrors = function(container) {
				return container.$error.serverErrors = container.$error.serverErrors || [];
			}
			var addError = function (error, container) {
				container.$setValidity(error.code, false);
				var propertyErrors = getPropertyErrors(container); 
				var alreadyExists = findError(propertyErrors, error);
				if (!alreadyExists) {
					propertyErrors.push(error);
				}
				error.clear = function() {
					var index = propertyErrors.indexOf(error);
					if (index > -1) {
						propertyErrors.splice(index, 1);
					}
					container.$setValidity(error.code, true);
				};
				errors.push({
					error: error,
					clear: error.clear
				});
			}
			var addErrors = function (errors, container) {
				for (var i = 0; i < errors.length; i++) {
					addError(errors[i], container);
				}
			}
			var setErrors = function (errors, container) {
				var propertyErrors = getPropertyErrors(container);
				for (var i = 0; i < propertyErrors.length; i++) {
					propertyErrors[i].clear();
				}
				container.$error.serverErrors = [];
				addErrors(errors, container);
			}
			var clear = function () {
				for (var i = 0; i < errors.length; i++) {
					errors[i].clear();
				}
				errors = [];
			}
			svc.generateElement = generateElement;
			svc.parsePath = parsePath;
			svc.errorKey = errorKey;
			svc.addError = addError;
			svc.addErrors = addErrors;
			svc.setErrors = setErrors;
			svc.clear = clear;
			return svc;
		}
	]);

angular
	.module("odataValidation")
	.directive("serverFormValidation",
	[
		"$http", "$q", "$compile", "odataValidationSvc",
		function ($http, $q, $compile, odataValidationSvc) {
			return {
				link: function (scope, element, attrs) {
					var formName = attrs.name;
					var form = scope[formName];
					var updateODataErrors = function (odataErrors) {
						odataValidationSvc.clear();
						if (!odataErrors) {
							return;
						}
						for (var i = 0; i < odataErrors.details.length; i++) {
							var error = odataErrors.details[i];

							// Add the error
							odataValidationSvc.addError(error, form[error.target]);

							var errorKey = odataValidationSvc.errorKey(
								error.target,
								formName);
							var errorElement = element[0].querySelector("#" + errorKey);
							if (!errorElement) {
								errorElement =
									odataValidationSvc.generateElement(
										error.target,
										formName,
										scope);
								var input = element[0].querySelector("input[name=\"" + error.target + "\"]");
								angular.element(input).after(errorElement);
							}
						}
					}
					scope.$watch("odataErrors", function (newVal, oldVal) {
						if (newVal !== oldVal) {
							updateODataErrors(!newVal ? null : newVal.data.error);
						}
					});
				}
			};
		}
	]);