После первого запуска необходимо настроить AuthService:
- Залогиниться в Keycloak под ролью адмнистратора
- перейти в Clients => admin-cli
- в Settings в секции Capability config включить Client authentication, установить галочку на Service account roles
- перейти в Credentials, сгенерировать Client Secret и вставить его в docker-compose в authservice.webapi => environment => Keycloak__AdminClientSecret
