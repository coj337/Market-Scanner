var hub = $.connection.socketsHub;
hub.client.updateTable = addToTable;
hub.client.clearTables = reset;

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

$("#timeLength, #timeFormat").on('change', function () {
    hub.server.changeDelay($("#timeLength").text, $("#timeFormat").text);   
});