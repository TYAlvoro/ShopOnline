CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE IF NOT EXISTS wallets (
                                       userid   UUID       PRIMARY KEY,
                                       balance  NUMERIC    NOT NULL DEFAULT 0,
                                       version  INTEGER    NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS transactions (
                                            id         UUID       PRIMARY KEY DEFAULT uuid_generate_v4(),
    userid     UUID       NOT NULL,
    amount     NUMERIC    NOT NULL,
    occurredat TIMESTAMP  NOT NULL DEFAULT now()
    );

CREATE TABLE IF NOT EXISTS inbox (
                                     id         UUID       PRIMARY KEY,
                                     topic      TEXT,
                                     receivedat TIMESTAMP  NOT NULL DEFAULT now()
    );

CREATE INDEX IF NOT EXISTS ix_inbox_topic ON inbox (topic);

CREATE TABLE IF NOT EXISTS outbox (
                                      id          UUID       PRIMARY KEY DEFAULT uuid_generate_v4(),
    occurredon  TIMESTAMP  NOT NULL DEFAULT now(),
    type        TEXT       NOT NULL,
    payload     TEXT       NOT NULL,
    processedat TIMESTAMP
    );

CREATE INDEX IF NOT EXISTS ix_outbox_processedat ON outbox (processedat);
