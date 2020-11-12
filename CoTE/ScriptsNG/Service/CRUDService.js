
app.service('CRUDService', function ($http) {
    //**********----Get Record----***************
    this.GetResults = function (apiRoute) {
        return $http.get(apiRoute);
    }
});