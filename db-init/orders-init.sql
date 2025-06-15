-- расширение для генерации UUID
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE IF NOT EXISTS orders (
                                      id         UUID        PRIMARY KEY DEFAULT uuid_generate_v4(),
    userid     UUID        NOT NULL,
    amount     NUMERIC     NOT NULL,
    status     INT         NOT NULL,
    createdat  TIMESTAMP   NOT NULL     DEFAULT now()
    );

CREATE TABLE IF NOT EXISTS outbox (
                                      id          UUID        PRIMARY KEY DEFAULT uuid_generate_v4(),
    occurredon  TIMESTAMP   NOT NULL     DEFAULT now(),
    type        TEXT        NOT NULL,
    payload     TEXT        NOT NULL,
    processedat TIMESTAMP
    );

-- ускоряет работу OutboxPublisher
CREATE INDEX IF NOT EXISTS ix_outbox_processedat
    ON outbox (processedat);
