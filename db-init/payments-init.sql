CREATE TABLE IF NOT EXISTS outbox
(
    id          UUID PRIMARY KEY,
    occurredon  TIMESTAMP,
    type        TEXT,
    payload     TEXT,
    processedat TIMESTAMP
);

CREATE TABLE IF NOT EXISTS inbox
(
    id         UUID PRIMARY KEY,
    topic      TEXT,
    receivedat TIMESTAMP
);
