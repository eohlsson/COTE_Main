
app.controller('ResultsCtrl', ['$scope', 'CRUDService', 'uiGridConstants', 

    function ($scope, CRUDService, uiGridConstants) {

        $scope.gridOptions = [];

        var urlParams = new URLSearchParams(window.location.search);
        //window.alert(urlParams.get('form_id'));
        var form_id = urlParams.get('form_id');
        var index_id = urlParams.get('index_id')
 
        var initialpagesize = 0;
        var pagesize = 1000;
        var columns = [];

        //initialpagesize = $scope.pagination.ddlpageSize;

        //Pagination
        $scope.pagination = {
            paginationPageSizes: [25, 50, 75, 100, 1000, "All"],
            ddlpageSize: pagesize,
            pageNumber: 1,

            pageSizeChange: function () {

                //window.alert(this.ddlpageSize);
                pagesize = this.ddlpageSize;

                if (this.ddlpageSize == "All")
                    $scope.gridOptions.paginationPageSize = $scope.pagination.totalItems;
                else
                    $scope.gridOptions.paginationPageSize = pagesize;

                //window.alert("paginationPageSize:" + $scope.gridOptions.paginationPageSize + ", ddlpageSize: " + this.ddlpageSize + ", pagesize: " + pagesize);

                this.pageNumber = 1

            }
        };

        //ui-Grid Call
        $scope.GetResults = function () {
            $scope.loaderMore = true;
            $scope.lblMessage = 'loading please wait....!';
            $scope.result = "color-green";

            var NextPageSize = $scope.pagination.ddlpageSize;
            if (NextPageSize == null)
            {
                NextPageSize = 25;
            }
            //window.alert(NextPageSize);

            var apiRoute = '/dotnet/api/results.aspx/getresults?form_id=' + form_id + '&index_id=' + index_id;
            var result = CRUDService.GetResults(apiRoute);
            result.then(
                function (response) {
                    $scope.pagination.totalItems = response.data.recordsTotal;
                    pagesize = response.data.pagesize;
                    columns = response.data.columns;
                    $scope.gridOptions.columnDefs = columns;
                    $scope.gridOptions.data = response.data.resultsList;
                    $scope.loaderMore = false;
                    $scope.lblMessage = "No Data Found";
                },
                function (response) {
                    //window.alert("Here is the Error: *" + response.data.error_message + "*");
                    window.location = "../Error.aspx/HttpError";
                });

            $scope.gridOptions = {
                useExternalPagination: false,
                useExternalSorting: false,
                enablePaginationControls: false,
                enableFiltering: true,
                enableSorting: true,
                enableRowSelection: true,
                enableSelectAll: true,
                enableGridMenu: true,
                enablePaging: true,

                paginationPageSize: pagesize,

                exporterAllDataFn: function () {
                    return getPage(1, $scope.gridOptions.totalItems, paginationOptions.sort)
                    .then(function () {
                        $scope.gridOptions.useExternalPagination = false;
                        $scope.gridOptions.useExternalSorting = false;
                        getPage = null;
                        });
                },

            };

            //window.alert(initialpagesize);

            //            $scope.gridOptions.columnDefs[8].cellTemplate = '<strong>{{COL_FIELD}}</strong>';

            //var column = [];
            //column.push({ field: 'Supervisor', displayName: 'Supervisor' });
            //window.alert(column[0].field);
            //$scope.gridOptions.columnDefs = column;

            //window.alert(columns[0].field);
            //$scope.gridOptions.columnDefs = columns;

            $scope.gridOptions.onRegisterApi = function (gridApi) {
                $scope.gridApi = gridApi;
            }

        }


        //Default Load
        $scope.GetResults();

    }
]);

app.filter('trusted', function ($sce) {
    return function (value) {
        return $sce.trustAsHtml(value);
    }
});