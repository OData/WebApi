"use strict";

angular
	.module("odataServerValidation", []);

angular
	.module("odataServerValidation")
	.factory("odataServerValidationSvc", ["$compile",
		function ($compile) {
			var svc = {};
			var errorKey = function (name, form) {
				var errorName = form + "_" + name + "_" + "Error";
				return errorName;
			}
			var generateElement = function (name, form, scope) {
				var errorName = errorKey(name, form);
				var errorElm = $compile("<div id=\"" + errorName + "\" class=\"alert alert-danger validation-group\" ng-show=\"(" + form + ".$submitted && " + form + "." + name + ".$odataServerErrors.length) || (" + form + "." + name + ".$dirty && " + form + "." + name + ".$odataServerErrors.length)\">" +
					"<div class=\"validation-item\" ng-repeat=\"error in " + form + "." + name + ".$odataServerErrors\">{{error.message}}</div>" +
					"</div>")(scope);
				return errorElm;
			}
			var getEntitySet = function (element) {
				return element.form.attributes["odata-server-field-validation-set"].nodeValue;
			}
			var parsePath = function (attrs, scope, element) {
				var path = attrs.serverFieldValidation;
				var result = null;
				if (!path) {
					result = {
						setName: getEntitySet(element),
						formName: element.form.name,
						propertyName: element.name
					};
				} else {
					var arr = path.split(".");
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
				}
				result.errorKey = errorKey(result.propertyName, result.formName);
				result.element = generateElement(result.propertyName, result.formName, scope);
				return result;
			}
			var findError = function (propertyErrors, error) {
				for (var i = 0; i < propertyErrors.length; i++) {
					if (propertyErrors[i].message === error.message &&
						propertyErrors[i].target === error.target) {
						return propertyErrors[i];
					}
				}
				return null;
			}
			var errors = [];
			var getPropertyErrors = function (container) {
				return container.$odataServerErrors = container.$odataServerErrors || [];
			}
			var addError = function (error, container) {
				container.$setValidity(error.target, false);
				var propertyErrors = getPropertyErrors(container);
				var alreadyExists = findError(propertyErrors, error);
				if (!alreadyExists) {
					propertyErrors.push(error);
				}
				error.clear = function () {
					var index = propertyErrors.indexOf(error);
					if (index > -1) {
						propertyErrors.splice(index, 1);
					}
					container.$setValidity(error.target, true);
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
				container.$odataServerErrors = [];
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
			svc.getEntitySet = getEntitySet;
			return svc;
		}
	]);

angular
	.module("odataServerValidation")
	.directive("odataServerFieldValidation",
		["$http", "$q", "odataServerValidationSvc",
			function ($http, $q, odataServerValidationSvc) {
				return {
					require: "ngModel",
					link: function (scope, element, attrs, ngModel) {
						var path = odataServerValidationSvc.parsePath(attrs, scope, element[0]);
						if (path.element) {
							// Add the error message container after
							// our element
							angular.element(element).after(path.element);
						}
						var config = {
							element: element,
							path: attrs.serverFieldValidation,
							propertyName: path.propertyName,
							formName: path.formName,
							setName: odataServerValidationSvc.getEntitySet(element[0])
						};
						scope.$emit("odataServerFieldValidation.ResolveUrl", config);
						var propertyValidationUrl = config.fieldValidationUrl;
						if (!propertyValidationUrl) {
							throw "No property validation URL endpoint provided for odata-server-field-validation";
						}
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
											odataServerValidationSvc.setErrors(
												response.data.error.details,
												scope[path.formName][path.propertyName]);
											if (response.data.error.details.length) {
												// TODO: Join the error messages up
												// We could also use the key from the details, rather
												// than assume it's an error for this field
												//scope[path.errorKey] = response.data.error.details[0].message;
												//deferred.resolve();
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
	.module("odataServerValidation")
	.directive("odataServerFormValidation",
	["odataServerValidationSvc",
		function (odataServerValidationSvc) {
			return {
				link: function (scope, element, attrs) {
					var formName = attrs.name;
					var form = scope[formName];
					form.unknownInvalid = false;
					//scope.$watch(formName + ".$invalid", function (newVal, oldVal) {
					//	if (!newVal && form.unknownInvalid) {
					//		form.$invalid = true;
					//	}
					//});
					var updateODataErrors = function (odataErrors) {
						odataServerValidationSvc.clear();
						if (!odataErrors) {
							return;
						}
						for (var i = 0; i < odataErrors.details.length; i++) {
							var error = odataErrors.details[i];
							// Add the error
							var newContainer = function (formLocal, errorLocal, elementLocal) {
								return {
									$setValidity: function (name, validity) {
										var v = formLocal.odataMappedValidities = formLocal.odataMappedValidities || {};
										if (!v[name]) {
											v[name] = 0;
										}
										v[name] = !validity;
										var isInvalid = 0;
										for (var property in v) {
											if (v.hasOwnProperty(property)) {
												// do stuff
												isInvalid += v[property];
											}
										}
										formLocal.unknownInvalid = !!isInvalid;
										if (formLocal.unknownInvalid && !formLocal.$invalid) {
											formLocal.$invalid = true;
										}
									}
								};
							}
							var container = form[error.target] = form[error.target] || newContainer(form, error, element);
							odataServerValidationSvc.addError(error,
								// If we don't have a form element for this, use the form itself
								// TODO: This will fail when having multiple non form-element errors
								// and resetting validity for one will reset for all
								container);

							var errorKey = odataServerValidationSvc.errorKey(
								error.target,
								formName);
							var errorElement = element[0].querySelector("#" + errorKey);
							if (!errorElement) {
								errorElement =
									odataServerValidationSvc.generateElement(
										error.target,
										formName,
										scope);
								var selectors = ["input", "select", "textarea", "button"];
								var foundInput = false;
								for (var j = 0; j < selectors.length; j++) {
									var input = element[0].querySelector(selectors[j] + "[name=\"" + error.target + "\"]");
									if (input) {
										angular.element(input).after(errorElement);
										foundInput = true;
										break;
									}
								}
								if (!foundInput) {
									// Now we have an error for an entry that isn't in our form,
									// so we need to print this by either our .odata-server-form-validation-submit
									// submit button or just at the top of the form as a last resort
									element.append(errorElement);
								}
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

angular
	.module("odataServerValidation")
	.directive("odataServerFormValidationSubmit",
	[function () {
		return {
			link: function (scope, element, attrs) {
				var form = element[0].form;
				var formName = form.name;
				element.addClass("odata-server-form-validation-submit");
				scope.$watchGroup([formName + ".$submitted", formName + ".$invalid", formName + ".$pending"],
					function (newValues, oldValues) {
						for (var i = 0; i < newValues.length; i++) {
							if (newValues[i] !== oldValues[i]) {
								var ngForm = scope[formName];
								scope.odataServerValidationAllowSubmit =
									!((ngForm.$submitted === true && ngForm.$invalid === true) ||
									ngForm.$pending === true);
								if (scope.odataServerValidationAllowSubmit) {
									element.removeAttr("disabled");
								} else {
									element.attr("disabled", "disabled");
								}
							}
						}
					});
				//(productsForm.$submitted && productsForm.$invalid) || productsForm.$pending
			}
		}
	}
	]);
