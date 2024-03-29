import { Component, OnInit } from '@angular/core';
import { StockPriceService } from '../common/stock-price.service';
import { StockPrice } from '../common/stock-price';
import { interval } from 'rxjs';
import { StockPriceHistory } from '../common/stock-price-history';
import * as Highcharts from 'highcharts/highstock';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css'],
})
export class DashboardComponent implements OnInit {

  constructor(private stockPriceService: StockPriceService) { }

  symbols: string[] = ['IIND.AX','NDIA.AX','^NSEI','^GSPC','^AXJO','^IXIC',
  'WORK','MSFT','GOOG','AMZN','AAPL','NFLX','DIS', 'HDB',
  'RELIANCE.NS','HDFCBANK.NS','MARUTI.NS'];
  currencies = ['AUD', 'USD', 'INR'];
  currencyPairs: string[] = [];
  compPairs = {};

  stockPrices = {};
  stockPriceHistories = {};
  interval: any;
  chart = {};
  selectedSymbol = '';
  volChart = false;

  ngOnInit() {
    this.buildCurrencyPairs();
    this.update();
    // interval(15000).subscribe(() => this.update());
  }

  buildCurrencyPairs() {
    for (var i = 0; i < this.currencies.length; i++) {
      for (var j = 0; j < this.currencies.length; j++) {
        if (i !== j) {
          let cPair = this.currencies[i] + this.currencies[j] + "=X";
          this.compPairs[cPair] = [cPair];
          this.currencyPairs.push(this.currencies[i] + this.currencies[j] + "=X");
        }
      }
    }
    // this.symbols = this.symbols.concat(this.currencyPairs);
  }

  update() {
    this.symbols.forEach(element => {
      this.stockPriceService
        .getStockPrice(element)
        .subscribe(data => {
          var newStockPrice = data;
          this.reshapeServiceData(newStockPrice, element);
        })
    });


    this.symbols.forEach(element => {
      this.stockPriceService
        .getStockPriceHistory(element)
        .subscribe(data => {
          this.SetHisNChart(element, data);
        })
    });

    this.currencyPairs.forEach(element => {
      this.stockPriceService
        .getStockPriceHistory(element)
        .subscribe(data => {
          this.SetHisNChart(element, data);
        })
    });
  }

  private SetHisNChart(element: string, data: StockPriceHistory[]) {
    this.stockPriceHistories[element] = data;
    let ohlcData = this.prepareChartData(data);
    let lineData = this.prepareChartData(data, 'line');
    this.chart[element] = {
      ohlc: ohlcData,
      line: lineData
    };
  }

  onSelectStock(symbol) {
    this.stockPriceService
      .getStockPriceHistory(symbol)
      .subscribe(data => {
        this.stockPriceHistories[symbol] = data;
        this.compPairs[symbol] = [symbol];
        this.setChart(data, symbol);
        this.renderSingleSeriesChart(symbol);
        this.selectedSymbol = symbol;
      })
  }

  onCompare(bSymbol, cSymbol) {
    this.compPairs[bSymbol].push(cSymbol);
    this.renderCompareSeriesChart(this.compPairs[bSymbol]);
  }

  renderSingleSeriesChart(symbol) {
    Highcharts.stockChart('chart-' + symbol, {

      rangeSelector: {
        selected: 1
      },

      title: {
        text: symbol
      },
      yAxis : {
        labels:{
            align:'right',
            x:-10
        },
        lineWidth : 0,
        offset : 50
    },

      series: [{
        name: symbol,
        data: this.chart[symbol]['line'],
        tooltip: {
          valueDecimals: 2
        },
        type: 'line'
      }]
    });
  }

  private setChart(data: StockPriceHistory[], symbol: any) {
    let ohlcData = this.prepareChartData(data);
    let lineData = this.prepareChartData(data, 'line');

    this.chart[symbol] = {
      'ohlc': ohlcData,
      'line': lineData 
    };
  }

  renderCompareSeriesChart(symbols: string[]) {
    let seriesOptions: any[] = symbols.map(x => {
      let lineData;
      if(x === this.selectedSymbol) {
        // get data from chart as it might be currency adjusted.
        lineData = this.chart[x]['line'];
      }
      
      else
      {
        // force get unadjusted data as for all comp pairs
        lineData = this.prepareChartData(this.stockPriceHistories[x], 'line');
      }
      return {        
        name: x, data: lineData
      }
    });

    Highcharts.stockChart('chart-' + symbols[0], {
      rangeSelector: {
        selected: 4
      },

      yAxis: {
        labels: {
          formatter: function () {
            return (this.value > 0 ? ' + ' : '') + this.value + '%';
          }
        },
        plotLines: [{
          value: 0,
          width: 2,
          color: 'silver'
        }]
      },

      plotOptions: {
        series: {
          compare: 'percent',
          showInNavigator: true
        }
      },

      tooltip: {
        pointFormat: '<span style="color:{series.color}">{series.name}</span>: <b>{point.y}</b> ({point.change}%)<br/>',
        valueDecimals: 2,
        split: true
      },

      series: seriesOptions
    });
  }



  onCurrencyChange(symbol, currency) {
    var stockPrice = this.stockPrices[symbol];
    this.compPairs[symbol] = [symbol];

    if (stockPrice.currency === currency) {
      this.setChart(this.stockPriceHistories[symbol], symbol);
      this.stockPrices[symbol].viewCurrency = currency;
      this.renderSingleSeriesChart(symbol);
      return;
    }

    let currencyPair = stockPrice.viewCurrency + currency + "=X";
    let adjustedSphOhlc = this.adjustForCurrencyOhlc(
      this.chart[symbol]['ohlc'],
      this.chart[currencyPair]['ohlc']);
    
    if (adjustedSphOhlc.length === 0) {
      return;
    }

    this.chart[symbol]['ohlc'] = adjustedSphOhlc;


    let adjustedSphLine = this.adjustForCurrencyOhlc(
      this.chart[symbol]['line'],
      this.chart[currencyPair]['line']);
    
    if (adjustedSphLine.length === 0) {
      return;
    }

    this.chart[symbol]['line'] = adjustedSphLine;

    this.stockPrices[symbol].viewCurrency = currency;

    this.renderSingleSeriesChart(symbol);

  }

  prepareChartData(data: StockPriceHistory[], type: string = 'ohlc') {
    let finalData = [];
    let cnt = data.length - 1;
    switch (type) {
      case 'ohlc':
        data.forEach(e => {
          let ohlc = [e.date * 1000, e.open, e.high, e.low, e.close];
          cnt--;
          finalData.push(ohlc);
        });
        break;
      case 'line':
        data.forEach(e => {
          let line = [e.date * 1000, e.close];
          cnt--;
          finalData.push(line);
        });
    }


    return finalData;
  }

  adjustForCurrencyOhlc(existing: any[], currency: any[]) {
    let min1 = Math.min.apply(Math, existing.map(o => o[0]));
    let min2 = Math.min.apply(Math, currency.map(o => o[0]));
    let max1 = Math.max.apply(Math, existing.map(o => o[0]));
    let max2 = Math.max.apply(Math, currency.map(o => o[0]));

    let min = Math.max(min1, min2);
    let max = Math.min(max1, max2);
    let fHis: any[] = existing
      .filter(x => x[0] >= min && x[0] <= max)
      .sort(x => x[0]);

    let fCur: any[] = currency
      .filter(x => x[0] >= min && x[0] <= max)
      .sort(x => x[0]);

    let finalData = [];

    for (var i = 0; i < fHis.length; i++) {
      let date = fHis[i][0];
      let cFcur = fCur.find(x => x[0] === date);
      if(cFcur == undefined) {
        continue;
      }

      let ohlc = [
        fHis[i][0],
        fHis[i][1] * cFcur[1],
        fHis[i][2] * cFcur[2],
        fHis[i][3] * cFcur[3],
        fHis[i][4] * cFcur[3]
      ];

      finalData.push(ohlc);
    }

    return finalData;
  }

  adjustForCurrencyLine(existing: any[], currency: any[]) {
    let min1 = Math.min.apply(Math, existing.map(o => o[0]));
    let min2 = Math.min.apply(Math, currency.map(o => o[0]));
    let max1 = Math.max.apply(Math, existing.map(o => o[0]));
    let max2 = Math.max.apply(Math, currency.map(o => o[0]));

    let min = Math.max(min1, min2);
    let max = Math.min(max1, max2);
    let fHis: any[] = existing
      .filter(x => x[0] >= min && x[0] <= max)
      .sort(x => x[0]);

    let fCur: any[] = currency
      .filter(x => x[0] >= min && x[0] <= max)
      .sort(x => x[0]);

    let finalData = [];

    for (var i = 0; i < Math.min(fHis.length, fCur.length); i++) {
      let ohlc = [
        fHis[i][0],
        fHis[i][1] * fCur[i][1]
      ];

      finalData.push(ohlc);
    }

    return finalData;
  }

  private reshapeServiceData(newStockPrice: StockPrice, symbol: string) {

    var existing = this.stockPrices[symbol] || undefined;

    newStockPrice.viewCurrency = newStockPrice.currency;

    if (newStockPrice.bid != 0) {
      newStockPrice.previousCloseChange = ((newStockPrice.bid -
        newStockPrice.previousClose) * 100 / newStockPrice.previousClose).toPrecision(3) + "%";
    }
    else {
      newStockPrice.previousCloseChange = ((newStockPrice.regularMarketPreviousClose -
        newStockPrice.previousClose) * 100 / newStockPrice.previousClose).toPrecision(3) + "%";
    }

    if (existing !== undefined) {

      // preserve viewCurrency

      newStockPrice.viewCurrency = existing.viewCurrency;

      if (newStockPrice.bid > this.stockPrices[symbol].bid) {
        newStockPrice.tickChangePositive = true;
      }

      else if (newStockPrice.bid < this.stockPrices[symbol].bid) {
        newStockPrice.tickChangePositive = false;
      }

      this.stockPrices[symbol] = newStockPrice;
    }
    else {
      this.stockPrices[symbol] = newStockPrice;
    }
  }
}
