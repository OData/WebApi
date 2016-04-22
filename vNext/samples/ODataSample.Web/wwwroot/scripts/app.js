"use strict";

var odataSampleApp =
	angular
		.module("odataSampleApp", [])
		.directive("products", function() {
			return {
				restrict: "E",
				scope: {
				
				},
				templateUrl: "/directives/products.html",
				controller: ["$http","$scope",
					function ($http, $scope) {
						$scope.addProduct = function() {
							$http.post("odata/Products", {
								Name: "Josh",
								Price: 200
							});
						};
					}]
			};
		});