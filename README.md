# Identity Capybara

Этот репозиторий является примером использования технологии OAuth2 на базе IdentityServer4 (Duende) и ASP.NET Core

Тут находятся версии для авторизации через логин/пароль и клиент/сертификат.

#### Для каждой версии нужны следующие проекты:

- CapibaraAPI - Это пример ресурсного сервера, доступ к которым клиент будет стараться получить
- Сервер авторизации
	- IdentityByCertificate - пример с использованием сертификатов
	- IdentityTest4 - пример с использованием логина/пароля
- Клиент
	- Caplient CLI - консольный пример клиента с использованием логина пароля
	- Capicertent CLI - аналогичный пример, но с использованием сертификатов

#### Порядок запуска
1. Сервер авторизации
2. Сервер ресурсов
3. Клиент

### Обратить внимание
* Для работы необходимо настроиться ссылки на сервера, они находятся в файлах appsettings.json каждого из проекта
#### Типичны следующие ссылки:
Сертификаты
- IdentityByCertificate (https://localhost:7155)
	- В проекте CapibaraAPI и Capicertent CLI
Пароль
- IdentityTest4 (https://localhost:5001)
	- В проекте CapibaraAPI и Caplient CLI
Общее
- CapibaraAPI (https://localhost:7162)
	- Либо в Capicertent CLI если целевое использование с сертификатами и Caplient CLI в ином