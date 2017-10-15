var hub = $.connection.socketsHub;

hub.client.clearTables = reset;
hub.client.updateTable = addToTable;
hub.client.lastUpdate = resetLastUpdate;

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
    hub.server.setVolumeChange($("#volumeChange").val(), $("#vTimeLength").val(), $("vTimeFormat").text());
});
$('#vTimeFormatList').on("click", "li", function () {
    hub.server.setVolumeChange($("#volumeChange").val(), $("#vTimeLength").val(), $("#vTimeFormat").text());
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

(function ($) {
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
})(jQuery);