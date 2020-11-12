var menuItem = new Array()
menuItem[0] = new Array("1", "1", "", "Interested in Offering Professional Development Activities, click here.", "/dotnet/webpages/webpage.aspx/webpage?page_level=7.1")
menuItem[1] = new Array("2", "1", "", "Cooperating Personnel working with Candidates in Field Experiences, click here.", "/dotnet/webpages/webpage.aspx/webpage?page_level=4.3.4")
menuItem[2] = new Array("3", "1", "", "University of Illinois Extension activities will continue to be reported through the current program management system. Contact Suzanne Lee (suzannel@illinois.edu) with questions.", "")

//window.alert("We are here" + strWhereAmI);

var strSpacer = ""
var strFontStyle = ""
var strLinkStart = ""
var strLinkEnd = ""
var strOldLevel = ""
var strMenu = "<nav><h1>ISBE Professional Development Hours</h1>"

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


