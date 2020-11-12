<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>
<%@ Import Namespace="WebMatrix.WebData" %>

<!DOCTYPE html>

<html>
<head runat="server">
    <meta name="viewport" content="width=device-width" />
    <title>Index</title>
</head>
<body>
    <form method="post" action="/dotnet/account.aspx/logout">
        <h1>Welcome <%= WebSecurity.CurrentUserName %>!</h1>
        <input type="submit" value="Logout" />
    </form>
</body>
</html>
