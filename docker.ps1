#docker rm $(docker ps -q) --force
cd app
docker build -t app .
# docker run -d -p 4200:4200 app
cd ..
cd IndexInfo.WebApi
docker build -t webapi .
# docker run -d -p 5001:80 webapi
cd ..
cd IndexInfo.OutboundApi/api
docker build -t oapi .
# docker run -d -p 5000:5000 oapi
cd ..