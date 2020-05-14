import { Injectable } from '@angular/core';
import { StockPrice } from './stock-price'
import { HttpClient } from '@angular/common/http'
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class StockPriceService {

  constructor(private http: HttpClient) { }

  baseUrl = "https://localhost:5001/api/StockPrices/";

  getStockPrice(symbol: string): Observable<any> {
    return this.http.get(this.baseUrl + `${symbol}`);
  }

}


