"use strict";

var odataSampleApp = angular.module("odataSampleApp",
	[
		"odataServerValidation",
		"toastr"
	])
	.run([
		"$rootScope", function ($rootScope) {
			$rootScope.$on("odataServerFieldValidation.ResolveUrl", function (e, config) {
				config.fieldValidationUrl = "/odata/ValidateField";
			});
		}
	]);