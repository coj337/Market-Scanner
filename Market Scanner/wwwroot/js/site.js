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

$('#BTCPairs, #USDTPairs, #ETHPairs').on('change', function () {
    hub.server.togglePair(this.id.substring(0,3));
});

function resetLastUpdate() {
    $("#lastUpdated").text("0");
}

setInterval(function () { //Increment last updated every second
    $("#lastUpdated").text(parseInt($("#lastUpdated").text()) + 1);
}, 1000);