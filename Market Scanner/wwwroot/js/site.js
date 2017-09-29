var hub = $.connection.socketsHub;
hub.client.updateTable = addToTable;
hub.client.clearTables = reset;
hub.client.lastUpdate = resetLastUpdate;

$.connection.hub.start().done(function () {
    hub.server.startListeners();    
});

function reset() {
    $('#BTCResults').empty();
    $('#USDTResults').empty();
    $('#ETHResults').empty();
}

function addToTable(pair, price, volume) {
    if(pair[0] == "B")
        $('#BTCResults').append("<tr><td>" + pair.substring(4) + "</td><td>" + price + "</td><td>" + volume + "</td></tr>");
    else if (pair[0] == "U")
        $('#USDTResults').append("<tr><td>" + pair.substring(5) + "</td><td>" + price + "</td><td>" + volume + "</td></tr>");
    else if (pair[0] == "E")
        $('#ETHResults').append("<tr><td>" + pair.substring(4) + "</td><td>" + price + "</td><td>" + volume + "</td></tr>");
}

$("#timeLength").on('change', function () {
    hub.server.changeDelay($("#timeLength").val(), $("#timeFormat").text());   
});

$('#timeFormatList').on('click', 'li', function () {
    $("#timeFormat").html(this.id + " <span class=\"caret\"></span>");
    hub.server.changeDelay($("#timeLength").val(), $("#timeFormat").text());
});

function resetLastUpdate() {
    $("#lastUpdated").text("0");
}

setInterval(function () { //Increment last updated every second
    $("#lastUpdated").text(parseInt($("#lastUpdated").text()) + 1);
}, 1000);