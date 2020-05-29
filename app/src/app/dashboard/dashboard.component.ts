import { Component, OnInit } from '@angular/core';
import { StockPriceService } from '../common/stock-price.service';
import { StockPrice } from '../common/stock-price';
import { interval } from 'rxjs';
import { distinctUntilChanged } from 'rxjs/operators';
import { GoogleChartsModule } from 'angular-google-charts'
import { StockPriceHistory } from '../common/stock-price-history';



@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css'],
})
export class DashboardComponent implements OnInit {

  constructor(private stockPriceService: StockPriceService) { }

  ausymbols = ['IIND.AX', 'NDIA.AX', 'OOO.AX', 'GEAR.AX', 'NAB.AX','TLS.AX'];
  uksymbols = ['RB.L', 'BATS.L', 'RTO.L','CCL.L', 'TSCO.L', 'RR.L'];
  stockPrices: StockPrice[] = [];
  interval: any;
  chart = {};
  selectedSymbol = '';

  ngOnInit() {
    this.update();
    interval(15000).subscribe(x => this.update());
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
  }

  onSelectStock(symbol)  {   

    this.stockPriceService
      .getStockPriceHistory(symbol)
      .subscribe(data => {

        var TwoDArrayData = this.prepareChartData(data);
        console.log(TwoDArrayData);
        this.chart[symbol] = {
          title: symbol,
          type: 'Line',
          width: '800px',
          height: 800,
          data: TwoDArrayData     
        };
        this.selectedSymbol = symbol;
      })
  }
  prepareChartData(data: StockPriceHistory[]) {
    let finalData = [];
    let cnt = data.length - 1;
    data.forEach(element => {
      let ohlc = [cnt, element.close];
      cnt--;
      finalData.push(ohlc);
    });

    return finalData;
  }

  private reshapeServiceData(newStockPrice: StockPrice, symbol: string) {

    var existingIndex = this.stockPrices.findIndex(x => x.symbol === symbol);

    if (newStockPrice.bid != 0) {
      newStockPrice.previousCloseChange = ((newStockPrice.bid -
        newStockPrice.previousClose) * 100 / newStockPrice.previousClose).toPrecision(3) + "%";
    }
    else {
      newStockPrice.previousCloseChange = ((newStockPrice.regularMarketPreviousClose -
        newStockPrice.previousClose) * 100 / newStockPrice.previousClose).toPrecision(3) + "%";
    }

    if (existingIndex !== -1) {

      if (newStockPrice.bid > this.stockPrices[existingIndex].bid) {
        newStockPrice.tickChangePositive = true;
      }
      else if (newStockPrice.bid < this.stockPrices[existingIndex].bid) {
        newStockPrice.tickChangePositive = false;
      }

      this.stockPrices[existingIndex] = newStockPrice;
    }
    else {
      this.stockPrices.push(newStockPrice);
    }
  }
}
