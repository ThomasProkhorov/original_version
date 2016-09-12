function endsWith(str, suffix) {
    return str.indexOf(suffix, str.length - suffix.length) !== -1;
}

function startWith(str, suffix) {
    return str.indexOf(suffix, 0) !== -1;
}

$(document).ready(function () {
    $("form").kendoValidator();
    $("#content").css("height", ($(window).height() - 28) + "px");
    $("#indexViewIframe").attr("height", $("#indexViewDiv").height() + "px");
});

window.onresize = function (event) {
    $("#content").css("height", ($(window).height() - 28) + "px");
    $("#indexViewIframe").attr("height", $("#indexViewDiv").height() + "px");
}

function changeViewFrame(newSrc) {
    $("#indexViewIframe").attr("src", newSrc);
}