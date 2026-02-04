# Cmd chạy 
docker compose -f docker-compose.fe.yml up -d

# Server test
http://localhost:8080/swagger

# Account test
admin / Admin@123
tech / Tech@123

# Log history
docker compose -f docker-compose.fe.yml logs -f api
