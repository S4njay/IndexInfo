import { Injectable } from '@angular/core';
import { StockPrice } from './stock-price'
import { HttpClient } from '@angular/common/http'
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { StockPriceHistory } from './stock-price-history';

@Injectable({
  providedIn: 'root'
})
export class StockPriceService {

  constructor(private http: HttpClient) { }

  baseUrl = "https://localhost:5001/api/";

  getStockPrice(symbol: string): Observable<any> {
    return this.http.get(this.baseUrl + 'StockPrices/' + `${symbol}`);
  }

  getStockPriceHistory(symbol: string): Observable<StockPriceHistory[]> {
    return this.http.get<StockPriceHistory[]>(this.baseUrl + 'StockPricesHistory/' + `${symbol}`)  
  }
}


