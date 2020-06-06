import flask
import ssl
from flask import request, jsonify
import yfinance as yf
from flask_jsonpify import jsonpify
from datetime import timezone
import pandas as pd



app = flask.Flask(__name__)
app.config["DEBUG"] = True

@app.route('/', methods=['GET'])
def home():
    return yf.Ticker("MSFT")

@app.route('/api/v1/quotes', methods=['GET'])
def api_id():
    if 'id' in request.args:
        id = request.args['id']
    else:
        return "Error: no stock symbol provided."

    ticker = yf.Ticker(id)

    return jsonify(ticker.info)

@app.route('/api/v1/history', methods=['GET'])
def api_history_id():
    if 'id' in request.args:
        id = request.args['id']
    else:
        return "Error: no stock symbol provided."

    ticker = yf.Ticker(id)
    df_list = ticker.history(period="max")
    df_list['date'] = df_list.index.astype(int) / 10**9
    df_list['symbol'] = id
    return jsonpify(df_list.to_dict('records'))

app.run(port=5002)