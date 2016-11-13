//Encapsulates data calls to server (AJAX calls)
var dataService = new function () {
    var serviceBase = '/api/dataService/',
        getAccount = function(acctNumber) {
            return $.getJSON(serviceBase + 'account/' + acctNumber);
        },

        getMarketIndexes = function() {
            return $.getJSON(serviceBase + 'marketIndices');
        },

        getQuote = function(symbol) {
            return $.getJSON(serviceBase + 'quote/' + symbol);
        },

        getTickerQuotes = function() {
            return $.getJSON(serviceBase + 'tickerQuotes');
        };

    return {
        getAccount: getAccount,
        getMarketIndexes: getMarketIndexes,
        getQuote: getQuote,
        getTickerQuotes: getTickerQuotes
    };

} ();