var xhttp;
xhttp = new XMLHttpRequest();
xhttp.onreadystatechange = function () {
    if (this.readyState == 4 && this.status == 200) {

    }
};

function myFunction(xml) {
    var x, i, txt, xmlDoc, y, j, k, l;
    xmlDoc = xml.responseXML;
    txt = "";
    stateid = "";
    var stateElements = document.getElementsByTagName("div");
    var states = xmlDoc.getElementsByTagName("area");
}
