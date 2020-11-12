
var menuItem = new Array()
//menuItem[2] = new Array("1", "1", "", "Requirements and Procedures", "/dotnet/webpages/webpage.aspx/webpage?page_level=4.1.3")
menuItem[0] = new Array("1", "1", "", "Districts with Placement Contracts", "/dotnet/webpages/webpage.aspx/facilitylist")
menuItem[1] = new Array("2", "1", "", "Links", "/dotnet/webpages/webpage.aspx/webpage?page_level=4.1.2")
menuItem[2] = new Array("3", "1", "", "Endorsements", "/dotnet/webpages/webpage.aspx/webpage?page_level=4.1.4")
menuItem[3] = new Array("4", "1", "", "EDPR 203", "/dotnet/webpages/webpage.aspx/webpage?page_level=2.5")
menuItem[4] = new Array("5", "1", "", "Presentations", "/dotnet/webpages/webpage.aspx/webpage?page_level=4.1.6")
menuItem[5] = new Array("6", "1", "", "Candidate Petition", "https://cte-s.education.illinois.edu/documents/Candidate_Petition.pdf")
menuItem[6] = new Array("7", "1", "", "Student Handbook", "/dotnet/webpages/webpage.aspx/webpage?page_level=4.1.8")


//window.alert("We are here" + strWhereAmI);

var strSpacer = ""
var strFontStyle = ""
var strLinkStart = ""
var strLinkEnd = ""
var strOldLevel = ""
var strMenu = "<nav><h1>Current Candidates</h1>"

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

