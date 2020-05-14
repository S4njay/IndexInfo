from setuptools import setup

setup(
   name='IndexInfo.OutboundApi',
   version='1.0',
   description='Outbound API to download stock quotes',
   author='Sanjay Sharma',
   author_email='',
   packages=['.'], 
   install_requires=['flask','yfinance','lxml'], #external packages as dependencies
   scripts=[]
)