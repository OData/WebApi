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
						function(modelValue, viewValue) {
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