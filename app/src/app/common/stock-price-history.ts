export interface StockPriceHistory {    
    symbol: string,
    date: number,
    open: number,
    high: number,
    low: number 
    close: number,
    volume: number,
    dividends: number,
    stockSplits: number    
}