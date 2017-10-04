var hub = $.connection.socketsHub;
hub.client.updateTable = addToTable;
hub.client.clearTables = reset;
hub.client.lastUpdate = resetLastUpdate;

$.connection.hub.start().done(function () {
    hub.server.startListeners();    
});

function reset() {
    $('#Results').empty();
}

function addToTable(pair, price, volume) {
    $('#Results').append("<tr><td>" + pair + "</td><td>" + price + "</td><td>" + volume + "</td></tr>");
}

$("#timeLength").on('change', function () {
    hub.server.changeDelay($("#timeLength").val(), $("#timeFormat").text());   
});

$('#timeFormatList').on('click', 'li', function () {
    $("#timeFormat").html(this.id + " <span class=\"caret\"></span>");
    hub.server.changeDelay($("#timeLength").val(), $("#timeFormat").text());
});

$('#pTimeFormatList').on('click', 'li', function () {
    $("#pTimeFormat").html(this.id + " <span class=\"caret\"></span>");
});

/* Price/Volume growth filters */
$('.spinner1').on("change", function () {
    hub.server.setPriceChange($("#maxTime").val(), $("#pTimeFormat").text());
});
$('#pTimeFormatList').on("click", "li", function () {
    hub.server.setPriceChange($("#maxTime").val(), $("#pTimeFormat").text());
});

$('.spinner1').on("change", function () {
    hub.server.setVolumeChange($("#maxVolume").val(), $("vTimeFormat").text());
});
$('#vTimeFormatList').on("click", "li", function () {
    hub.server.setVolumeChange($("#maxVolume").val(), $("#vTimeFormat").text());
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
    });
    $('.spinner1 .btn:last-of-type').on('click', function () {
        if ($('#priceChange').val() == "") {
            $('#priceChange').val(0);
        }
        $('#priceChange').val(parseInt($('#priceChange').val(), 10) - 1);
    });

    $('.spinner2 .btn:first-of-type').on('click', function () {
        if ($('#volumeChange').val() == "") {
            $('#volumeChange').val(0);
        }
        $('#volumeChange').val(parseInt($('#volumeChange').val(), 10) + 1);
    });
    $('.spinner2 .btn:last-of-type').on('click', function () {
        if ($('#volumeChange').val() == "") {
            $('#volumeChange').val(0);
        }
        $('#volumeChange').val(parseInt($('#volumeChange').val(), 10) - 1);
    });
})(jQuery);