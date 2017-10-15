var hub = $.connection.socketsHub;

hub.client.clearTables = reset;
hub.client.updateTable = addToTable;
hub.client.lastUpdate = resetLastUpdate;
hub.client.updateTitle = updateTitle;
hub.client.tableChanged = playAudio;

$.connection.hub.start().done(function () {
    hub.server.startListeners();
});

function reset() {
    $('#Results').empty();
}

function addToTable(pair, price, volume, priceChangePercent, volumeChangePercent) {
    var pairLink = pair.split('-');
    $('#Results').append("<tr><td><a href=\"https://www.coinigy.com/main/markets/BTRX/" + pairLink[1] + "/" + pairLink[0] + "\" target=\"_blank\">" + pair + "</a></td><td>" + price + "</td><td>" + volume + "</td><td>" + priceChangePercent + "</td><td>" + volumeChangePercent + "</td></tr>");
}

$('#pTimeFormatList').on('click', 'li', function () {
    $("#pTimeFormat").html(this.id + " <span class=\"caret\"></span>");
});

$('#vTimeFormatList').on('click', 'li', function () {
    $("#vTimeFormat").html(this.id + " <span class=\"caret\"></span>");
});

/* Price/Volume growth filters */
$('.spinner1').on("change", function () {
    hub.server.setPriceChange($("#priceChange").val(), $("#pTimeLength").val(), $("#pTimeFormat").text());
});
$('#pTimeFormatList').on("click", "li", function () {
    hub.server.setPriceChange($("#priceChange").val(), $("#pTimeLength").val(), $("#pTimeFormat").text());
});

$('.spinner2').on("change", function () {
    hub.server.setVolumeChange($("#volumeChange").val(), $("#vTimeLength").val(), $("#vTimeFormat").text());
});
$('#vTimeFormatList').on("click", "li", function () {
    hub.server.setVolumeChange($("#volumeChange").val(), $("#vTimeLength").val(), $("#vTimeFormat").text());
});

/* Price/Volume exclusion filters */
$('.spinner3').on("change", function () {
    hub.server.setExcludePriceChange($("#expriceChange").val(), $("#expTimeLength").val(), $("#expTimeFormat").text());
});
$('#expTimeFormatList').on("click", "li", function () {
    hub.server.setExcludePriceChange($("#expriceChange").val(), $("#expTimeLength").val(), $("#expTimeFormat").text());
});

$('.spinner4').on("change", function () {
    hub.server.setExcludeVolumeChange($("#exvolumeChange").val(), $("#exvTimeLength").val(), $("#exvTimeFormat").text());
});
$('#exvTimeFormatList').on("click", "li", function () {
    hub.server.setExcludeVolumeChange($("#exvolumeChange").val(), $("#exvTimeLength").val(), $("#exvTimeFormat").text());
});

/* Base currency filters */
$('#BTCPairs, #USDTPairs, #ETHPairs').on('change', function () {
    hub.server.togglePair(this.id.substring(0,3));
});

$("#maxPrice, #minPrice, #minVolume, #maxVolume").on('change', function () {
    if (this.id == "maxPrice") {
        hub.server.changeMaxPrice($("#maxPrice").val());
    }
    else if (this.id == "minPrice") {
        hub.server.changeMinPrice($("#minPrice").val());
    }
    else if (this.id == "maxVolume") {
        hub.server.changeMaxVolume($("#maxVolume").val());
    }
    else if (this.id == "minVolume") {
        hub.server.changeMinVolume($("#minVolume").val());
    }
});

function resetLastUpdate() {
    $("#lastUpdated").text("0");
}

setInterval(function () { //Increment last updated every second
    $("#lastUpdated").text(parseInt($("#lastUpdated").text()) + 1);
}, 1000);

//Include events
$('.spinner1 .btn:first-of-type').on('click', function () {
    if ($('#priceChange').val() == "") {
        $('#priceChange').val(0);
    }
    $('#priceChange').val(parseInt($('#priceChange').val(), 10) + 1);
    hub.server.setPriceChange($("#maxTime").val(), $("#pTimeFormat").text());
});
$('.spinner1 .btn:last-of-type').on('click', function () {
    if ($('#priceChange').val() == "") {
        $('#priceChange').val(0);
    }
    $('#priceChange').val(parseInt($('#priceChange').val(), 10) - 1);
    hub.server.setPriceChange($("#maxTime").val(), $("#pTimeFormat").text());
});

$('.spinner2 .btn:first-of-type').on('click', function () {
    if ($('#volumeChange').val() == "") {
        $('#volumeChange').val(0);
    }
    $('#volumeChange').val(parseInt($('#volumeChange').val(), 10) + 1);
    hub.server.setVolumeChange($("#maxVolume").val(), $("#vTimeFormat").text());
});
$('.spinner2 .btn:last-of-type').on('click', function () {
    if ($('#volumeChange').val() == "") {
        $('#volumeChange').val(0);
    }
    $('#volumeChange').val(parseInt($('#volumeChange').val(), 10) - 1);
    hub.server.setVolumeChange($("#maxVolume").val(), $("#vTimeFormat").text());
});

//Exclude events
$('.spinner3 .btn:first-of-type').on('click', function () {
    if ($('#expriceChange').val() == "") {
        $('#expriceChange').val(0);
    }
    $('#expriceChange').val(parseInt($('#expriceChange').val(), 10) + 1);
    hub.server.setExcludePriceChange($("#exmaxTime").val(), $("#expTimeFormat").text());
});
$('.spinner3 .btn:last-of-type').on('click', function () {
    if ($('#expriceChange').val() == "") {
        $('#expriceChange').val(0);
    }
    $('#expriceChange').val(parseInt($('#expriceChange').val(), 10) - 1);
    hub.server.setExcludePriceChange($("#exmaxTime").val(), $("#expTimeFormat").text());
});

$('.spinner4 .btn:first-of-type').on('click', function () {
    if ($('#exvolumeChange').val() == "") {
        $('#exvolumeChange').val(0);
    }
    $('#exvolumeChange').val(parseInt($('#exvolumeChange').val(), 10) + 1);
    hub.server.setExcludeVolumeChange($("#exmaxVolume").val(), $("#exvTimeFormat").text());
});
$('.spinner4 .btn:last-of-type').on('click', function () {
    if ($('#exvolumeChange').val() == "") {
        $('#exvolumeChange').val(0);
    }
    $('#exvolumeChange').val(parseInt($('#exvolumeChange').val(), 10) - 1);
    hub.server.setExcludeVolumeChange($("#exmaxVolume").val(), $("#exvTimeFormat").text());
});

//Profile events
$('#fatFingerButton').on('click', function () {
    $('#priceChange').val(-5);
    $("#pTimeLength").val(10);
    $("#pTimeFormat").text("Minutes");
    hub.server.setPriceChange($('#priceChange').val(), $("#pTimeLength").val(), $("#pTimeFormat").text());

    $('#volumeChange').val(-10);
    $("#vTimeLength").val(10);
    $("#vTimeFormat").text("Minutes");
    hub.server.setVolumeChange($('#volumeChange').val(), $("#vTimeLength").val(), $("#vTimeFormat").text());

    $("#profileButton").html("Fat Finger <span class=\"caret\"></span>");
});

//UX Functions
function updateTitle(newTitle) {
    document.title = newTitle;
}

function toggleAudio() {
    $('#volumeIcon').toggleClass("glyphicon-volume-up");
    $('#volumeIcon').toggleClass("glyphicon-volume-off");
}

function playAudio() {
    if ($('#volumeIcon').hasClass("glyphicon-volume-up")) {
        var audio = new Audio('sound/arpeggio.mp3');
        audio.play();
    }
}