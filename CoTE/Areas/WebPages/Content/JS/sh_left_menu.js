var menuItem = new Array()
menuItem[0] = new Array("1", "1", "", "Welcome", "/dotnet/webpages/webpage.aspx/webpage?page_level=4.1.8")
menuItem[1] = new Array("1", "1", "", "Program Transition Points", "/dotnet/webpages/webpage.aspx/webpage?page_level=4.1.8.1")
menuItem[2] = new Array("2", "1", "", "Completion of a State Approved Licensure Program", "/dotnet/webpages/webpage.aspx/webpage?page_level=4.1.8.2")
menuItem[3] = new Array("3", "1", "", "Completion of All Required Coursework", "/dotnet/webpages/webpage.aspx/webpage?page_level=4.1.8.3")
menuItem[4] = new Array("4", "1", "", "Completion of Additional Required Training & Criminal Background Check", "/dotnet/webpages/webpage.aspx/webpage?page_level=4.1.8.4")
menuItem[5] = new Array("5", "1", "", "Successful Completion of Student Teaching", "/dotnet/webpages/webpage.aspx/webpage?page_level=4.1.8.5")
menuItem[6] = new Array("6", "1", "", "Required Tests for Illinois Licensure", "/dotnet/webpages/webpage.aspx/webpage?page_level=4.1.8.6")
menuItem[7] = new Array("7", "1", "", "Clinical/Student Teaching Site Policies and Professional Expectations", "/dotnet/webpages/webpage.aspx/webpage?page_level=4.1.8.7")

//window.alert("We are here" + strWhereAmI);

var strSpacer = ""
var strFontStyle = ""
var strLinkStart = ""
var strLinkEnd = ""
var strOldLevel = ""
var strMenu = "<nav><h1>Student Handbook</h1>"

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

