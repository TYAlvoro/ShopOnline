# ShopOnline — демо‑магазин с Saga

> Короткое README: что есть, куда жать и как быстро поднять систему.

---

## 1. Состав репозитория

| Путь                   | Назначение                                                         |
| ---------------------- | ------------------------------------------------------------------ |
| **api-gateway/**       | YARP‑reverse‑proxy, склеивает все сервисы и проксирует SignalR‑хаб |
| **order-service/**     | Сервис заказов + хаб `OrderHub`                                    |
| **payment-service/**   | Сервис кошельков / оплат                                           |
| **frontend/**          | Vite + React мини‑UI для приёма уведомлений                        |
| **ShopOnline.Shared/** | Общие контракты, outbox, Kafka‑обёртка                             |
| **db-init/**           | SQL‑скрипты для инициализации Postgres                             |
| **docker-compose.yml** | Поднимает всё сразу (Postgres, Kafka, сервисы, фронт)              |

---

## 2. Что реализовано

* **Saga (Outbox + Kafka)**: заказ → публикация `OrderCreated` → списание денег → `PaymentCompleted`/`PaymentFailed` → изменение статуса заказа.
* **Outbox / Inbox** паттерны для идемпотентности.
* **SignalR** — пуш событий в браузер через gateway.
* **Unit‑тесты** для бизнес‑логики Order и Payment.

---

## 3. API‑ручки

### Order Service (`http://localhost:8080/swagger`)

| Метод           | URL            | Описание                                                                   |
| --------------- | -------------- | -------------------------------------------------------------------------- |
| **POST**        | `/orders`      | Создать заказ.<br>Header `user_id` обязателен, тело: `{ amount, userId? }` |
| **GET**         | `/orders`      | Список заказов пользователя (header `user_id`).                            |
| **GET**         | `/orders/{id}` | Получить конкретный заказ.                                                 |
| **SignalR Hub** | `/hub/orders`  | События `PaymentCompleted` / `PaymentFailed`.                              |

### Payment Service (`http://localhost:8081/swagger`)

| Метод    | URL                 | Описание          |
| -------- | ------------------- | ----------------- |
| **POST** | `/payments/account` | Создать кошелёк.  |
| **POST** | `/payments/deposit` | Пополнить баланс. |
| **GET**  | `/payments/balance` | Текущий баланс.   |

> Все вызовы требуют header `user_id: <guid>`.

---

## 4. Быстрый старт

```bash
# 1.  Клонировать репо
# 2.  Запуск (нужно Docker + Docker Compose ≥ v2)
$ docker compose build        # ~4‑5 мин первый раз
$ docker compose up -d        # поднимаем кластеры

# 3.  Проверить
$ open http://localhost:5173          # фронт
$ open http://localhost:8080/swagger  # заказы
$ open http://localhost:8081/swagger  # платежи
```

Контейнеры:

| Имя                   | Порт        | Что это                      |
| --------------------- | ----------- | ---------------------------- |
| `orders-database`     | 5433        | Postgres «orders»            |
| `payments-database`   | 5432        | Postgres «payments»          |
| `kafka` / `zookeeper` | 9092 / 2181 | Kafka‑кластер                |
| `order-service`       | 8080        | ASP.NET сервис заказов       |
| `payment-service`     | 8081        | ASP.NET сервис оплат         |
| `api-gateway`         | 80          | YARP proxy (WS‑upgrade вкл.) |
| `frontend`            | 5173        | Nginx + React bundle         |

> Логи смотрим так: `docker compose logs -f order-service payment-service`.
