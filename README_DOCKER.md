docker rm mqtt_relay --force

docker build -t mqtt_relay:latest .

docker run --name mqtt_relay --rm -it -p 8080:8080 -p 8081:8081 mqtt_relay
