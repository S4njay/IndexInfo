import flask
import ssl
from flask import request, jsonify
import json

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

app.run()