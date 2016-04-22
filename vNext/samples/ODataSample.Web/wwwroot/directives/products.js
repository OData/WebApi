"use strict";
angular
	.module("odataSampleApp")
	.directive("products", function() {
		return {
			restrict: "E",
			scope: {
				
			},
			templateUrl: "/directives/products.html",
			controller: [
				"$http", "$scope",
				function ($http, $scope) {
					$scope.name = "";
					$scope.price = 200;
					$scope.addProduct = function() {
						$http.post("odata/Products", {
							Name: $scope.name,
							Price: $scope.price
						});
					};
				}
			]
		};
	});