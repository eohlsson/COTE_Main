var menuItem = new Array()
menuItem[0] = new Array("1", "1", "", "Conceptual Framework", "/dotnet/webpages/webpage.aspx/webpage?page_level=2.1")
menuItem[1] = new Array("2", "1", "", "Program List", "/dotnet/webpages/webpage.aspx/webpage?page_level=2.2")
menuItem[2] = new Array("3", "1", "", "Transition Points", "/dotnet/webpages/webpage.aspx/webpage?page_level=2.3")
menuItem[3] = new Array("4", "1", "", "Title II", "https://title2.ed.gov/Public/Home.aspx")
menuItem[4] = new Array("5", "1", "", "EDPR 203", "/dotnet/webpages/webpage.aspx/webpage?page_level=2.5")

//window.alert("We are here" + strWhereAmI);

var strSpacer = ""
var strFontStyle = ""
var strLinkStart = ""
var strLinkEnd = ""
var strOldLevel = ""
var strMenu = "<nav><h1>Professional Education Preparation</h1>"

for (var i = 0; i < menuItem.length; i++) {

    if (menuItem[i][1] != strOldLevel) {
        if (menuItem[i][1] < strOldLevel) {
            strMenu = strMenu + "</ul>"
        }

        switch (menuItem[i][1]) {
            case "1":
                strMenu = strMenu + "<ul>"
                break
            case "2":
                strMenu = strMenu + "<ul>"
                break
        }
    }

    strFontStyle = menuItem[i][2]

    //window.alert("menuItem[i][4]=" + menuItem[i][4] + ", URL=" + window.location.toString());

    var strURL = window.location.toString()

    if (strURL.indexOf(menuItem[i][4]) > 0) {
        switch (menuItem[i][1]) {
            case "1":
                strFontStyle = "is-active"
                break
            case "2":
                strFontStyle = "is-active"
                break
        }
    }

    if (menuItem[i][4] != "") {
        strLinkStart = "<li class='" + strFontStyle + "'><a href='" + menuItem[i][4] + "'>"
        strLinkEnd = "</a></li>"
    }
    else {
        strLinkStart = ""
        strLinkEnd = ""
    }

    strMenu = strMenu + strLinkStart + menuItem[i][3] + strLinkEnd

    strOldLevel = menuItem[i][1]

}

strMenu = strMenu + "</nav>"

//window.alert (strMenu)
document.write(strMenu)

