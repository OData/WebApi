"use strict";
angular
	.module("odataSampleApp")
	.directive("productPatch", function () {
		return {
			restrict: "E",
			scope: {

			},
			templateUrl: "/directives/product-patch.html",
			controller: [
				"$http", "$scope",
				function ($http, $scope) {
					$scope.$on("resolve-field-validation-url", function (e, config) {
						config.setName = "Products";
						config.fieldValidationUrl = "/odata/ValidateField";
					});
					function makeid() {
						var text = "";
						var possible = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

						for (var i = 0; i < 5; i++)
							text += possible.charAt(Math.floor(Math.random() * possible.length));

						return text;
					}
					$scope.ProductId = 1;
					$scope.Name = makeid();
					$scope.Price = Math.random();
					$scope.addProduct = function() {
						$http.patch("odata/Products(" + $scope.ProductId + ")", {
								Id: $scope.Id,
								Name: $scope.Name,
								Price: $scope.Price
							})
							.then(function(success) {
									$scope.odataErrors = null;
								},
								function(errors) {
									$scope.odataErrors = errors;
								});
					};
				}
			]
		};
	});