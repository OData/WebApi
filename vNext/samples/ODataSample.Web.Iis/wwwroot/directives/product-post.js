"use strict";
angular
	.module("odataSampleApp")
	.directive("productPost", function () {
		return {
			restrict: "E",
			scope: {

			},
			templateUrl: "/directives/product-post.html",
			controller: [
				"$http", "$scope", "toastr",
				function ($http, $scope, toastr) {
					$scope.$on("resolve-field-validation-url", function (e, config) {
						config.setName = "Products";
						config.fieldValidationUrl = "/odata/ValidateField";
					});
					$scope.Name = "a";
					$scope.Price = 7;
					$scope.OwnerEmailAddress = "j@j.co";
					$scope.addProduct = function () {
						$http.post("odata/Products", {
							Name: $scope.Name,
							Price: $scope.Price,
							OwnerEmailAddress: $scope.OwnerEmailAddress,
							DateInvented: $scope.DateInvented||""
						//UsedByUsers: [{ Id: "1" }, { Id: "3" }, { Id: "4" }]
					})
							.then(function (success) {
									toastr.success("POST success");
									$scope.odataErrors = null;
								},
								function(errors) {
									toastr.error("POST fail");
									$scope.odataErrors = errors;
								});
					};
				}
			]
		};
	});