import { Component, OnInit } from '@angular/core';
import { StockPriceService } from '../common/stock-price.service';
import { StockPrice } from '../common/stock-price';
import { interval } from 'rxjs';
import { StockPriceHistory } from '../common/stock-price-history';



@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css'],
})
export class DashboardComponent implements OnInit {

  constructor(private stockPriceService: StockPriceService) { }

  ausymbols = ['IIND.AX', 'NDIA.AX', 'OOO.AX', 'GEAR.AX', 'NAB.AX','TLS.AX','AUDINR=X'];
  uksymbols = ['RB.L', 'BATS.L', 'RTO.L','CCL.L', 'TSCO.L', 'RR.L'];
  currencies = ['AUD','USD','INR'];
  currencyPairs = [];

  stockPrices = {};
  stockPriceHistories = {};
  interval: any;
  chart = {};
  selectedSymbol = '';

  ngOnInit() {
    this.buildCurrencyPairs();
    this.update();
    interval(15000).subscribe(() => this.update());
  }

  buildCurrencyPairs() {
    for(var i = 0; i < this.currencies.length; i++)
    {
      for(var j = 0; j < this.currencies.length; j++)
      {
        if(i !== j)
        {
          this.currencyPairs.push(this.currencies[i] + this.currencies[j] + "=X");
        }
      }
    }
  }

  update() {
    this.ausymbols.forEach(element => {
      this.stockPriceService
        .getStockPrice(element)
        .subscribe(data => {
          var newStockPrice = data;
          this.reshapeServiceData(newStockPrice, element);
        })
    });

    
    this.currencyPairs.forEach(element => {     
      this.stockPriceService
        .getStockPriceHistory(element)
        .subscribe(data => {
          this.stockPriceHistories[element] = data;
          var TwoDArrayData = this.prepareChartData(data);
          this.chart[element] = {
            title: element,
            type: 'Line',
            width: '800px',
            height: 800,
            data: TwoDArrayData     
          };
        })
      });    
  }

  onSelectStock(symbol)  {   
    this.stockPriceService
      .getStockPriceHistory(symbol)
      .subscribe(data => {
        this.stockPriceHistories[symbol] = data;
        this.setChart(data, symbol);
        this.selectedSymbol = symbol;
      })
  }

  private setChart(data: StockPriceHistory[], symbol: any) {
    var TwoDArrayData = this.prepareChartData(data);
    this.chart[symbol] = {
      title: symbol,
      type: 'Line',
      width: '800px',
      height: 800,
      data: TwoDArrayData
    };
  }

  onCurrencyChange(symbol, currency){
    var stockPrice = this.stockPrices[symbol];

    if(stockPrice.currency === currency)
    {
      this.setChart(this.stockPriceHistories[symbol], symbol);
      this.stockPrices[symbol].viewCurrency = currency;
      return;     
    }

    let currencyPair = stockPrice.viewCurrency + currency + "=X";
    let adjustedSph = this.adjustForCurrency(
      this.stockPriceHistories[symbol], 
      this.stockPriceHistories[currencyPair]);
    
    if(adjustedSph.length === 0)
    {
      return;
    }

    this.chart[symbol].data = adjustedSph;
    this.stockPrices[symbol].viewCurrency = currency;
  }

  prepareChartData(data: StockPriceHistory[]) {
    let finalData = [];
    let cnt = data.length - 1;
    data.forEach(element => {
      let ohlc = [new Date(element.date * 1000), element.close];
      cnt--;
      finalData.push(ohlc);
    });

    return finalData;
  }

  adjustForCurrency(existing: StockPriceHistory[], currency: StockPriceHistory[]) {
    let min1 = Math.min.apply(Math, existing.map(o => o.date));
    let min2 = Math.min.apply(Math, currency.map(o => o.date));
    let max1 = Math.max.apply(Math, existing.map(o => o.date));
    let max2 = Math.max.apply(Math, currency.map(o => o.date));

    let min = Math.max(min1, min2);
    let max = Math.min(max1, max2);

    let newStockPriceHistory: StockPriceHistory[] = existing
      .filter(x => x.date >= min && x.date <= max)
      .sort(x => x.date);

    let newCurrency: StockPriceHistory[] = currency
      .filter(x => x.date >= min && x.date <= max)
      .sort(x => x.date);

    let finalData = [];

    for(var i=0; i < Math.min(newStockPriceHistory.length, newCurrency.length); i++) {
      let ohlc = [new Date(newStockPriceHistory[i].date * 1000), 
        newStockPriceHistory[i].close * newCurrency[i].close];
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
