from setuptools import setup, find_packages

setup(
   name='IndexInfo.OutboundApi',
   version='1.0',
   description='Outbound API to download stock quotes',
   author='Sanjay Sharma',
   author_email='',
   packages=find_packages(), 
   install_requires=['flask','yfinance','lxml','flask_jsonpify'], #external packages as dependencies
   scripts=[]
)